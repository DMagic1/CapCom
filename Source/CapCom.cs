﻿#region license
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
		private Dictionary<Guid, CapComContract> activeContracts = new Dictionary<Guid, CapComContract>();
		private Dictionary<Guid, CapComContract> offeredContracts = new Dictionary<Guid, CapComContract>();
		private Dictionary<Guid, CapComContract> completedContracts = new Dictionary<Guid, CapComContract>();
		private Dictionary<Guid, CapComContract> failedContracts = new Dictionary<Guid, CapComContract>();

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
			GameEvents.Contract.onContractsLoaded.Add(onContractsLoaded);
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
			GameEvents.Contract.onContractsLoaded.Remove(onContractsLoaded);
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

		public CapComContract getActiveContract(Guid id, bool warn = false)
		{
			if (activeContracts.ContainsKey(id))
				return activeContracts[id];
			else if (warn)
				LogFormatted("No Active Contract Of ID: [{0}] Found", id);

			return null;
		}

		public bool addActiveContract(CapComContract c, bool warn = false)
		{
			if (!activeContracts.ContainsKey(c.ID))
			{
				activeContracts.Add(c.ID, c);
				return true;
			}
			else if (warn)
				LogFormatted("Active Contract List Already Has Contract [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public bool removeActiveContract(CapComContract c, bool warn = false)
		{
			if (activeContracts.ContainsKey(c.ID))
			{
				activeContracts.Remove(c.ID);
				return true;
			}
			else if (warn)
				LogFormatted("Contract Not Found In Active Contract List [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public CapComContract getOfferedContract(Guid id, bool warn = false)
		{
			if (offeredContracts.ContainsKey(id))
				return offeredContracts[id];
			else if (warn)
				LogFormatted("No Offered Contract Of ID: [{0}] Found", id);

			return null;
		}

		public bool addOfferedContract(CapComContract c, bool warn = false)
		{
			if (!offeredContracts.ContainsKey(c.ID))
			{
				offeredContracts.Add(c.ID, c);
				return true;
			}
			else if (warn)
				LogFormatted("Offered Contract List Already Has Contract [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public bool removeOfferedContract(CapComContract c, bool warn = false)
		{
			if (offeredContracts.ContainsKey(c.ID))
			{
				offeredContracts.Remove(c.ID);
				return true;
			}
			else if (warn)
				LogFormatted("Contract Not Found In Offered Contract List [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public CapComContract getCompletedContract(Guid id, bool warn = false)
		{
			if (completedContracts.ContainsKey(id))
				return completedContracts[id];
			else if (warn)
				LogFormatted("No Completed Contract Of ID: [{0}] Found", id);

			return null;
		}

		public bool addCompletedContract(CapComContract c, bool warn = false)
		{
			if (!completedContracts.ContainsKey(c.ID))
			{
				completedContracts.Add(c.ID, c);
				return true;
			}
			else if (warn)
				LogFormatted("Completed Contract List Already Has Contract [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public bool removeCompletedContract(CapComContract c, bool warn = false)
		{
			if (completedContracts.ContainsKey(c.ID))
			{
				completedContracts.Remove(c.ID);
				return true;
			}
			else if (warn)
				LogFormatted("Contract Not Found In Completed Contract List [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public CapComContract getFailedContract(Guid id, bool warn = false)
		{
			if (failedContracts.ContainsKey(id))
				return failedContracts[id];
			else if (warn)
				LogFormatted("No Failed Contract Of ID: [{0}] Found", id);

			return null;
		}

		public bool addFailedContract(CapComContract c, bool warn = false)
		{
			if (!failedContracts.ContainsKey(c.ID))
			{
				failedContracts.Add(c.ID, c);
				return true;
			}
			else if (warn)
				LogFormatted("Failed Contract List Already Has Contract [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public bool removeFailedContract(CapComContract c, bool warn = false)
		{
			if (failedContracts.ContainsKey(c.ID))
			{
				failedContracts.Remove(c.ID);
				return true;
			}
			else if (warn)
				LogFormatted("Contract Not Found In Failed Contract List [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public List<CapComContract> getActiveContracts
		{
			get { return activeContracts.Values.ToList(); }
		}

		public List<CapComContract> getOfferedContracts
		{
			get { return offeredContracts.Values.ToList(); }
		}

		public List<CapComContract> getCompletedContracts
		{
			get { return completedContracts.Values.ToList(); }
		}

		public List<CapComContract> getFailedContracts
		{
			get { return failedContracts.Values.ToList(); }
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

			CapComContract cc = getOfferedContract(c.ContractGuid, true);

			if (cc == null)
				return;

			cc.updateTimeValues();

			removeOfferedContract(cc, true);

			addActiveContract(cc, true);
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

			CapComContract cc = getOfferedContract(c.ContractGuid, true);

			if (cc == null)
				return;

			removeOfferedContract(cc, true);
			refreshList();
		}

		private void onFinished(Contract c)
		{
			if (c == null)
			{
				LogFormatted("Error in loading null finished contract");
				return;
			}

			CapComContract cc = getActiveContract(c.ContractGuid);

			if (cc == null)
				cc = getOfferedContract(c.ContractGuid, true);

			if (cc == null)
				return;

			cc.updateTimeValues();

			removeOfferedContract(cc);
			removeActiveContract(cc);
			if (c.ContractState == Contract.State.Completed)
				addCompletedContract(cc, true);
			refreshList();
		}

		private void onOffered(Contract c)
		{
			if (c == null)
			{
				LogFormatted("Error in loading null offered contract");
				return;
			}

			CapComContract cc = new CapComContract(c);

			if (cc == null)
				return;

			addOfferedContract(cc, true);
			refreshList();
		}

		private void onContractsLoaded()
		{
			StartCoroutine(loadContracts());
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

			//Agency modifiers don't seem to work unless I wait a few frames before loading contracts
			while (i < 5)
			{
				i++;
				yield return null;
			}

			foreach(Contract c in ContractSystem.Instance.Contracts)
			{
				if (c == null)
				{
					LogFormatted("Error in loading null contract from master list");
					continue;
				}

				CapComContract cc = new CapComContract(c);

				if (cc.Root == null)
				{
					LogFormatted("Error while loading contract of type {0}; skipping", c.GetType().Name);
					continue;
				}

				switch (cc.Root.ContractState)
				{
					case Contract.State.Active:
						addActiveContract(cc);
						continue;
					case Contract.State.Offered:
						addOfferedContract(cc);
						continue;
					case Contract.State.Completed:
						addCompletedContract(cc);
						continue;
					case Contract.State.Cancelled:
					case Contract.State.DeadlineExpired:
					case Contract.State.Failed:
						addFailedContract(cc);
						continue;
					default:
						continue;
				}
			}

			foreach(Contract c in ContractSystem.Instance.ContractsFinished)
			{
				if (c == null)
				{
					LogFormatted("Error in loading contract from finished list");
					continue;
				}

				CapComContract cc = new CapComContract(c);

				if (cc.Root == null)
				{
					LogFormatted("Error while loading finished contract of type {0}; skipping", c.GetType().Name);
					continue;
				}

				switch (cc.Root.ContractState)
				{
					case Contract.State.Active:
						addActiveContract(cc);
						continue;
					case Contract.State.Offered:
						addOfferedContract(cc);
						continue;
					case Contract.State.Completed:
						addCompletedContract(cc);
						continue;
					case Contract.State.Cancelled:
					case Contract.State.DeadlineExpired:
					case Contract.State.Failed:
						addFailedContract(cc);
						continue;
					default:
						continue;
				}
			}

			LogFormatted("CapCom Contracts Loaded...");

			window.refreshContracts(true);
		}

		private void refreshList()
		{
			window.refreshContracts(false);
		}

		private void updateOrbits(CapComContract c)
		{
			if (!HighLogic.LoadedSceneIsFlight)
				return;

			for (int i = 0; i < c.ParameterCount; i++)
			{
				CapComParameter p = c.getParameter(i);

				if (p == null)
					continue;

				if (p.Param.GetType() != typeof(SpecificOrbitParameter))
					continue;

				SpecificOrbitParameter s = (SpecificOrbitParameter)p.Param;

				MethodInfo orbitSetup = (typeof(SpecificOrbitParameter)).GetMethod("setup", BindingFlags.NonPublic | BindingFlags.Instance);

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

		private void updateWaypoints(CapComContract c)
		{
			if (!HighLogic.LoadedSceneIsFlight)
				return;

			if (WaypointManager.Instance() == null)
				return;

			for (int i = 0; i < c.ParameterCount; i++)
			{
				CapComParameter p = c.getParameter(i);

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
