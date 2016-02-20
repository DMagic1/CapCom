#region license
/*The MIT License (MIT)
CapCom - Main control MonoBehaviour for contract information

Copyright (c) 2015 DMagic

KSP Plugin Framework by TriggerAu, 2014: http://forum.kerbalspaceprogram.com/threads/66503-KSP-Plugin-Framework

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CapCom.Framework;
using CapCom.Toolbar;
using Contracts;
using Contracts.Parameters;
using Contracts.Agents;
using FinePrint;
using FinePrint.Contracts.Parameters;
using FinePrint.Utilities;
using UnityEngine;
using ContractParser;
using ProgressParser;

namespace CapCom
{
	[CC_KSPAddonImproved(CC_KSPAddonImproved.Startup.TimeElapses | CC_KSPAddonImproved.Startup.Editor, false)]
	public class CapCom : CC_MBE
	{
		private static CapCom instance;
		private static string version;
		private static CapComSettings settings = null;
		private CapComWindow window;
		private CC_StockToolbar appButton;
		private CC_Toolbar toolbar;

		private const string filePath = "Settings";

		private static bool textureLoaded = false;

		private static bool loaded;

		protected override void Awake()
		{
			if (loaded)
			{
				Destroy(gameObject);
				return;
			}

			loaded = true;

			if (!textureLoaded)
			{
				textureLoaded = true;

				Texture original = null;

				foreach (Texture2D t in Resources.FindObjectsOfTypeAll<Texture2D>())
				{
					if (t.name == "MissionControl")
					{
						original = t;
						break;
					}
				}

				if (original == null)
				{
					LogFormatted("Error loading Mission Control Center Texture atlas; some CapCom UI elements will not appear correctly");
					return;
				}

				Texture2D missionControlTexture = new Texture2D(original.width, original.height);

				var rt = RenderTexture.GetTemporary(original.width, original.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, 1);

				Graphics.Blit(original, rt);

				RenderTexture.active = rt;

				missionControlTexture.ReadPixels(new Rect(0, 0, original.width, original.height), 0, 0);

				RenderTexture.active = null;
				RenderTexture.ReleaseTemporary(rt);

				rt = null;

				original = null;

				missionControlTexture.Apply();

				CapComSkins.texturesFromAtlas(missionControlTexture);

				Destroy(missionControlTexture);
			}

			CapComSkins.currentFlag = GameDatabase.Instance.GetTexture(HighLogic.CurrentGame.flagURL, false);

			int i = 0;
			while (CapComSkins.currentFlag == null && i < AgentList.Instance.Agencies.Count)
			{
				CapComSkins.currentFlag = AgentList.Instance.Agencies[i].LogoScaled;
				i++;
			}

			Assembly assembly = AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly()).assembly;
			var ainfoV = Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
			switch (ainfoV == null)
			{
				case true: version = ""; break;
				default: version = ainfoV.InformationalVersion; break;
			}
		}

		protected override void Start()
		{
			instance = this;

			if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
				Destroy(this);

			if (settings == null)
			{
				settings = new CapComSettings(filePath);

				//if (settings.useKSPStyle)
				//{
				//	CapComSkins.initializeKSPSkins();
				//	CC_SkinsLibrary.SetCurrent("CCKSPSkin");
				//}
				//else
				//{
				//	CapComSkins.initializeUnitySkins();
				//	CC_SkinsLibrary.SetCurrent("CCUnitySkin");
				//}
			}

			window = gameObject.AddComponent<CapComWindow>();

			if (ToolbarManager.ToolbarAvailable && !settings.stockToolbar)
			{
				if (toolbar == null)
					toolbar = gameObject.AddComponent<CC_Toolbar>();
				if (appButton != null)
					Destroy(appButton);
			}
			else
			{
				if (appButton == null)
					appButton = gameObject.AddComponent<CC_StockToolbar>();
				if (toolbar != null)
					Destroy(toolbar);
			}

			contractParser.onContractsParsed.Add(onContractsLoaded);
			contractParser.onContractStateChange.Add(refreshList);
			progressParser.onProgressParsed.Add(onProgressLoaded);
			GameEvents.Contract.onContractsListChanged.Add(onListChanged);
		}

		protected override void OnDestroy()
		{
			loaded = false;

			if (appButton != null)
				Destroy(appButton);

			if (toolbar != null)
				Destroy(toolbar);

			if (window != null)
				Destroy(window);

			contractParser.onContractsParsed.Remove(onContractsLoaded);
			contractParser.onContractStateChange.Remove(refreshList);
			progressParser.onProgressParsed.Remove(onProgressLoaded);
			GameEvents.Contract.onContractsListChanged.Remove(onListChanged);

			instance = null;
		}

		#region Public Accessors

		public static CapCom Instance
		{
			get { return instance; }
		}

		public CapComWindow Window
		{
			get { return window; }
		}

		public static CapComSettings Settings
		{
			get { return settings; }
		}

		public CC_Toolbar Toolbar
		{
			get { return toolbar; }
			set { toolbar = value; }
		}

		public CC_StockToolbar StockToolbar
		{
			get { return appButton; }
			set { appButton = value; }
		}

		public static string Version
		{
			get { return version; }
		}

		#endregion

		#region Events

		private void onContractsLoaded()
		{
			StartCoroutine(loadContracts());
		}

		private void onProgressLoaded()
		{
			StartCoroutine(loadProgress());
		}

		private void onListChanged()
		{
			window.refreshContracts(false);
		}

		#endregion

		#region Methods

		private IEnumerator loadContracts()
		{
			int i = 0;

			while (!contractParser.Loaded && i < 200)
			{
				i++;
				yield return null;
			}

			window.refreshContracts(true);
		}

		private IEnumerator loadProgress()
		{
			int i = 0;

			while (!progressParser.Loaded && i < 200)
			{
				i++;
				yield return null;
			}

			window.updateProgress();
		}

		private void refreshList(Contract c)
		{
			window.refreshContracts(false);

			if (c == null)
				return;

			if (c.ContractState != Contract.State.Active)
				return;

			contractContainer cc = contractParser.getActiveContract(c.ContractGuid);

			if (cc == null)
				return;

			if (cc.Initialized)
				return;

			for (int i = 0; i < cc.ParameterCount; i++)
			{
				parameterContainer p = cc.getParameterFull(i);

				if (p == null)
					continue;

				customStartup(p);
			}

			cc.Initialized = true;
		}

		private void customStartup(parameterContainer p)
		{
			Type t = p.CParam.GetType();
			GameScenes s = HighLogic.LoadedScene;

			try
			{
				if (t == typeof(ReachDestination) && s == GameScenes.FLIGHT && FlightGlobals.ActiveVessel != null)
				{
					if (p.CParam.State == ParameterState.Incomplete && ((ReachDestination)p.CParam).checkVesselDestination(FlightGlobals.ActiveVessel))
					{
						MethodInfo m = (typeof(ContractParameter)).GetMethod("SetComplete", BindingFlags.NonPublic | BindingFlags.Instance);

						if (m == null)
							return;

						m.Invoke(p.CParam, null);
					}
					else if (p.CParam.State == ParameterState.Complete && !((ReachDestination)p.CParam).checkVesselDestination(FlightGlobals.ActiveVessel))
					{
						MethodInfo m = (typeof(ContractParameter)).GetMethod("SetIncomplete", BindingFlags.NonPublic | BindingFlags.Instance);

						if (m == null)
							return;

						m.Invoke(p.CParam, null);
					}
				}
				else if (t == typeof(ReachSituation) && s == GameScenes.FLIGHT && FlightGlobals.ActiveVessel != null)
				{
					if (p.CParam.State == ParameterState.Incomplete && ((ReachSituation)p.CParam).checkVesselSituation(FlightGlobals.ActiveVessel))
					{
						MethodInfo m = (typeof(ContractParameter)).GetMethod("SetComplete", BindingFlags.NonPublic | BindingFlags.Instance);

						if (m == null)
							return;

						m.Invoke(p.CParam, null);
					}
					else if (p.CParam.State == ParameterState.Complete && !((ReachSituation)p.CParam).checkVesselSituation(FlightGlobals.ActiveVessel))
					{
						MethodInfo m = (typeof(ContractParameter)).GetMethod("SetIncomplete", BindingFlags.NonPublic | BindingFlags.Instance);

						if (m == null)
							return;

						m.Invoke(p.CParam, null);
					}
				}
				else if (t == typeof(SpecificOrbitParameter) && s == GameScenes.FLIGHT)
				{
					((SpecificOrbitParameter)p.CParam).SetupWaypoints();
				}
				else if (t == typeof(VesselSystemsParameter) && s == GameScenes.FLIGHT)
				{
					VesselSystemsParameter sys = (VesselSystemsParameter)p.CParam;

					MethodInfo m = (typeof(VesselSystemsParameter)).GetMethod("OnRegister", BindingFlags.NonPublic | BindingFlags.Instance);

					if (m == null)
						return;

					m.Invoke(sys, null);

					if (!sys.requireNew)
						return;

					Vessel v = FlightGlobals.ActiveVessel;

					if (v == null)
						return;

					if (v.situation != Vessel.Situations.PRELAUNCH)
						return;

					uint launchID = v.Parts.Min(r => r.launchID);

					sys.launchID = launchID;
				}
				else if (t == typeof(SurveyWaypointParameter) && s == GameScenes.FLIGHT)
				{
					MethodInfo m = (typeof(SurveyWaypointParameter)).GetMethod("OnRegister", BindingFlags.NonPublic | BindingFlags.Instance);

					if (m == null)
						return;

					m.Invoke((SurveyWaypointParameter)p.CParam, null);

					if (p.Way == null)
						return;

					var waypoints = WaypointManager.Instance().AllWaypoints();

					if (waypoints.Contains(p.Way))
						return;

					WaypointManager.AddWaypoint(p.Way);
				}
				else if (t == typeof(StationaryPointParameter) && s == GameScenes.FLIGHT)
				{
					if (p.Way == null)
						return;

					var waypoints = WaypointManager.Instance().AllWaypoints();

					if (waypoints.Contains(p.Way))
						return;

					WaypointManager.AddWaypoint(p.Way);
				}
				else if (t == typeof(AsteroidParameter) && s == GameScenes.FLIGHT)
				{
					MethodInfo m = (typeof(AsteroidParameter)).GetMethod("OnRegister", BindingFlags.NonPublic | BindingFlags.Instance);

					if (m == null)
						return;

					m.Invoke((AsteroidParameter)p.CParam, null);
				}
				else if (t == typeof(CrewCapacityParameter) && s == GameScenes.FLIGHT)
				{
					MethodInfo m = (typeof(CrewCapacityParameter)).GetMethod("OnRegister", BindingFlags.NonPublic | BindingFlags.Instance);

					if (m == null)
						return;

					m.Invoke((CrewCapacityParameter)p.CParam, null);
				}
				else if (t == typeof(CrewTraitParameter) && s == GameScenes.FLIGHT)
				{
					MethodInfo m = (typeof(CrewTraitParameter)).GetMethod("OnRegister", BindingFlags.NonPublic | BindingFlags.Instance);

					if (m == null)
						return;

					m.Invoke((CrewTraitParameter)p.CParam, null);
				}
				else if (t == typeof(KerbalDestinationParameter) && s == GameScenes.FLIGHT)
				{
					MethodInfo m = (typeof(KerbalDestinationParameter)).GetMethod("OnRegister", BindingFlags.NonPublic | BindingFlags.Instance);

					if (m == null)
						return;

					m.Invoke((KerbalDestinationParameter)p.CParam, null);
				}
				else if (t == typeof(KerbalTourParameter) && s == GameScenes.FLIGHT)
				{
					MethodInfo m = (typeof(KerbalTourParameter)).GetMethod("OnRegister", BindingFlags.NonPublic | BindingFlags.Instance);

					if (m == null)
						return;

					m.Invoke((KerbalTourParameter)p.CParam, null);
				}
				else if (t == typeof(LocationAndSituationParameter) && s == GameScenes.FLIGHT)
				{
					MethodInfo m = (typeof(LocationAndSituationParameter)).GetMethod("OnRegister", BindingFlags.NonPublic | BindingFlags.Instance);

					if (m == null)
						return;

					m.Invoke((LocationAndSituationParameter)p.CParam, null);
				}
				else if (t == typeof(MobileBaseParameter) && s == GameScenes.FLIGHT)
				{
					MethodInfo m = (typeof(MobileBaseParameter)).GetMethod("OnRegister", BindingFlags.NonPublic | BindingFlags.Instance);

					if (m == null)
						return;

					m.Invoke((MobileBaseParameter)p.CParam, null);
				}
				else if (t == typeof(ProgressTrackingParameter) && s == GameScenes.FLIGHT)
				{
					MethodInfo m = (typeof(ProgressTrackingParameter)).GetMethod("OnRegister", BindingFlags.NonPublic | BindingFlags.Instance);

					if (m == null)
						return;

					m.Invoke((ProgressTrackingParameter)p.CParam, null);
				}
				else if (t == typeof(ResourceExtractionParameter) && s == GameScenes.FLIGHT)
				{
					MethodInfo m = (typeof(ResourceExtractionParameter)).GetMethod("OnRegister", BindingFlags.NonPublic | BindingFlags.Instance);

					if (m == null)
						return;

					m.Invoke((ResourceExtractionParameter)p.CParam, null);
				}
				else if (t == typeof(VesselDestinationParameter) && s == GameScenes.FLIGHT)
				{
					MethodInfo m = (typeof(VesselDestinationParameter)).GetMethod("OnRegister", BindingFlags.NonPublic | BindingFlags.Instance);

					if (m == null)
						return;

					m.Invoke((VesselDestinationParameter)p.CParam, null);
				}
				else if (t == typeof(PartTest) && s == GameScenes.FLIGHT)
				{
					MethodInfo m = (typeof(PartTest)).GetMethod("OnRegister", BindingFlags.NonPublic | BindingFlags.Instance);

					if (m == null)
						return;

					m.Invoke((PartTest)p.CParam, null);

					if (((PartTest)p.CParam).hauled)
						return;

					AvailablePart targetPart = ((PartTest)p.CParam).tgtPartInfo;

					if (targetPart == null)
						return;

					for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
					{
						Vessel v = FlightGlobals.Vessels[i];

						if (v == null)
							continue;

						if (!v.loaded)
							continue;

						for (int j = 0; j < v.Parts.Count; j++)
						{
							Part part = v.Parts[j];

							if (part == null)
								continue;

							if (part.partInfo != targetPart)
								continue;

							var mods = part.FindModulesImplementing<ModuleTestSubject>();

							for (int k = 0; k < mods.Count; k++)
							{
								ModuleTestSubject test = mods[k];

								if (test == null)
									continue;

								test.Events["RunTestEvent"].active = true;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				LogFormatted("Error while forcing Contract Parameter activation:\n{0}", e);
			}
		}

		#endregion

	}
}
