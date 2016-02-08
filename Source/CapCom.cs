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

		private static bool loaded = false;

		protected override void Awake()
		{
			if (!loaded)
			{
				loaded = true;

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

			GameEvents.Contract.onAccepted.Add(onAccepted);
			GameEvents.Contract.onDeclined.Add(onDeclined);
			GameEvents.Contract.onFinished.Add(onFinished);
			GameEvents.Contract.onOffered.Add(onOffered);
			contractParser.onContractsParsed.Add(onContractsLoaded);
			progressParser.onProgressParsed.Add(onProgressLoaded);
			GameEvents.Contract.onContractsListChanged.Add(onListChanged);
		}

		protected override void OnDestroy()
		{
			if (appButton != null)
				Destroy(appButton);

			if (toolbar != null)
				Destroy(toolbar);

			if (window != null)
				Destroy(window);

			GameEvents.Contract.onAccepted.Remove(onAccepted);
			GameEvents.Contract.onDeclined.Remove(onDeclined);
			GameEvents.Contract.onFinished.Remove(onFinished);
			GameEvents.Contract.onOffered.Remove(onOffered);
			contractParser.onContractsParsed.Remove(onContractsLoaded);
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

		private void onAccepted(Contract c)
		{
			if (c == null)
			{
				LogFormatted("Error in loading null accepted contract");
				return;
			}

			contractContainer cc = contractParser.getOfferedContract(c.ContractGuid, true);

			if (cc == null)
				return;

			cc.updateTimeValues();

			refreshList();

			updateWaypoints(cc);
			updateOrbits(cc);
		}

		private void onDeclined(Contract c)
		{
			if (c == null)
			{
				LogFormatted("Error in loading null declined contract");
				return;
			}

			contractContainer cc = contractParser.getOfferedContract(c.ContractGuid, true);

			if (cc == null)
				return;

			refreshList();
		}

		private void onFinished(Contract c)
		{
			if (c == null)
			{
				LogFormatted("Error in loading null finished contract");
				return;
			}

			contractContainer cc = contractParser.getActiveContract(c.ContractGuid);

			if (cc == null)
				cc = contractParser.getOfferedContract(c.ContractGuid, true);

			if (cc == null)
				return;

			cc.updateTimeValues();

			refreshList();
		}

		private void onOffered(Contract c)
		{
			if (c == null)
			{
				LogFormatted("Error in loading null offered contract");
				return;
			}

			contractContainer cc = new contractContainer(c);

			if (cc == null)
				return;

			refreshList();
		}

		private void onContractsLoaded()
		{
			StartCoroutine(loadContracts());
		}

		private void onProgressLoaded()
		{

		}

		private void onListChanged()
		{
			refreshList();
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
			
			//load progress.....
		}

		private void refreshList()
		{
			window.refreshContracts(false);
		}

		private void updateOrbits(contractContainer c)
		{
			if (!HighLogic.LoadedSceneIsFlight)
				return;

			for (int i = 0; i < c.ParameterCount; i++)
			{
				parameterContainer p = c.getParameter(i);

				if (p == null)
					continue;

				if (p.CParam.GetType() != typeof(SpecificOrbitParameter))
					continue;

				SpecificOrbitParameter s = (SpecificOrbitParameter)p.CParam;

				MethodInfo orbitSetup = (typeof(SpecificOrbitParameter)).GetMethod("SetupWaypoints", BindingFlags.NonPublic | BindingFlags.Instance);

				if (orbitSetup == null)
					return;

				try
				{
					orbitSetup.Invoke(s, null);
				}
				catch (Exception e)
				{
					LogFormatted("Error while activating FinePrint Specific Orbit Parameter: {0}", e);
				}
			}
		}

		private void updateWaypoints(contractContainer c)
		{
			if (!HighLogic.LoadedSceneIsFlight)
				return;

			if (WaypointManager.Instance() == null)
				return;

			for (int i = 0; i < c.ParameterCount; i++)
			{
				parameterContainer p = c.getParameter(i);

				if (p == null)
					continue;

				if (p.Way == null)
					continue;

				var waypoints = WaypointManager.Instance().AllWaypoints();

				if (waypoints.Contains(p.Way))
					continue;

				WaypointManager.AddWaypoint(p.Way);
			}
		}

		#endregion

	}
}
