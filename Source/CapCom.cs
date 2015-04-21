using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CapCom.Framework;
using CapCom.Toolbar;
using Contracts;
using Contracts.Parameters;
using FinePrint;
using FinePrint.Contracts.Parameters;
using FinePrint.Utilities;

namespace CapCom
{
	[CC_KSPAddonImproved(CC_KSPAddonImproved.Startup.TimeElapses | CC_KSPAddonImproved.Startup.Editor, false)]
	public class CapCom : CC_MBE
	{
		private static CapCom instance;
		private static string version;
		private CapComWindow window;
		private CC_StockToolbar appButton;
		private CC_Toolbar toolbar;
		private bool loaded;
		private Dictionary<Guid, CapComContract> activeContracts = new Dictionary<Guid, CapComContract>();
		private Dictionary<Guid, CapComContract> offeredContracts = new Dictionary<Guid, CapComContract>();
		private Dictionary<Guid, CapComContract> completedContracts = new Dictionary<Guid, CapComContract>();
		private Dictionary<Guid, CapComContract> failedContracts = new Dictionary<Guid, CapComContract>();

		protected override void Awake()
		{
			instance = this;

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
			if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
				Destroy(this);

			window = gameObject.AddComponent<CapComWindow>();

			appButton = gameObject.AddComponent<CC_StockToolbar>();

			GameEvents.Contract.onAccepted.Add(onAccepted);
			GameEvents.Contract.onCompleted.Add(onCompleted);
			GameEvents.Contract.onDeclined.Add(onDeclined);
			GameEvents.Contract.onFailed.Add(onFailed);
			GameEvents.Contract.onFinished.Add(onFinished);
			GameEvents.Contract.onOffered.Add(onOffered);
			GameEvents.Contract.onContractsLoaded.Add(onContractsLoaded);
			GameEvents.Contract.onContractsListChanged.Add(onListChanged);
		}

		protected override void OnDestroy()
		{
			if (window != null)
				Destroy(window);

			if (appButton != null)
				Destroy(appButton);

			GameEvents.Contract.onAccepted.Remove(onAccepted);
			GameEvents.Contract.onCompleted.Remove(onCompleted);
			GameEvents.Contract.onDeclined.Remove(onDeclined);
			GameEvents.Contract.onFailed.Remove(onFailed);
			GameEvents.Contract.onFinished.Remove(onFinished);
			GameEvents.Contract.onOffered.Remove(onOffered);
			GameEvents.Contract.onContractsLoaded.Remove(onContractsLoaded);
			GameEvents.Contract.onContractsListChanged.Remove(onListChanged);
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

		public bool Loaded
		{
			get { return loaded; }
		}

		public static string Version
		{
			get { return version; }
		}

		public CapComContract getActiveContract(Guid id)
		{
			if (activeContracts.ContainsKey(id))
				return activeContracts[id];
			else
				LogFormatted("No Active Contract Of ID: [{0}] Found", id);

			return null;
		}

		public bool addActiveContract(CapComContract c)
		{
			if (!activeContracts.ContainsKey(c.ID))
			{
				activeContracts.Add(c.ID, c);
				return true;
			}
			else
				LogFormatted("Active Contract List Already Has Contract [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public bool removeActiveContract(CapComContract c)
		{
			if (activeContracts.ContainsKey(c.ID))
			{
				activeContracts.Remove(c.ID);
				return true;
			}
			else
				LogFormatted("Contract Not Found In Active Contract List [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public CapComContract getOfferedContract(Guid id)
		{
			if (offeredContracts.ContainsKey(id))
				return offeredContracts[id];
			else
				LogFormatted("No Offerd Contract Of ID: [{0}] Found", id);

			return null;
		}

		public bool addOfferedContract(CapComContract c)
		{
			if (!offeredContracts.ContainsKey(c.ID))
			{
				offeredContracts.Add(c.ID, c);
				return true;
			}
			else
				LogFormatted("Offered Contract List Already Has Contract [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public bool removeOfferedContract(CapComContract c)
		{
			if (offeredContracts.ContainsKey(c.ID))
			{
				offeredContracts.Remove(c.ID);
				return true;
			}
			else
				LogFormatted("Contract Not Found In Offered Contract List [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public CapComContract getCompletedContract(Guid id)
		{
			if (completedContracts.ContainsKey(id))
				return completedContracts[id];
			else
				LogFormatted("No Completed Contract Of ID: [{0}] Found", id);

			return null;
		}

		public bool addCompletedContract(CapComContract c)
		{
			if (!completedContracts.ContainsKey(c.ID))
			{
				completedContracts.Add(c.ID, c);
				return true;
			}
			else
				LogFormatted("Completed Contract List Already Has Contract [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public bool removeCompletedContract(CapComContract c)
		{
			if (completedContracts.ContainsKey(c.ID))
			{
				completedContracts.Remove(c.ID);
				return true;
			}
			else
				LogFormatted("Contract Not Found In Completed Contract List [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public CapComContract getFailedContract(Guid id)
		{
			if (failedContracts.ContainsKey(id))
				return failedContracts[id];
			else
				LogFormatted("No Failed Contract Of ID: [{0}] Found", id);

			return null;
		}

		public bool addFailedContract(CapComContract c)
		{
			if (!failedContracts.ContainsKey(c.ID))
			{
				failedContracts.Add(c.ID, c);
				return true;
			}
			else
				LogFormatted("Failed Contract List Already Has Contract [{0} ; ID: {1}]", c.Name, c.ID);

			return false;
		}

		public bool removeFailedContract(CapComContract c)
		{
			if (failedContracts.ContainsKey(c.ID))
			{
				failedContracts.Remove(c.ID);
				return true;
			}
			else
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
			CapComContract cc = getOfferedContract(c.ContractGuid);

			if (cc == null)
			{
				LogFormatted("");
				return;
			}

			removeOfferedContract(cc);

			addActiveContract(cc);
			refreshList();

			updateWaypoints(cc);
			updateOrbits(cc);
		}

		private void onCompleted(Contract c)
		{
			CapComContract cc = getActiveContract(c.ContractGuid);

			if (cc == null)
			{
				LogFormatted("");
				return;
			}

			removeActiveContract(cc);

			addCompletedContract(cc);
			refreshList();
		}

		private void onDeclined(Contract c)
		{
			CapComContract cc = getOfferedContract(c.ContractGuid);

			if (cc == null)
			{
				LogFormatted("");
				return;
			}

			removeOfferedContract(cc);
			refreshList();
		}

		private void onFailed(Contract c)
		{
			CapComContract cc = getActiveContract(c.ContractGuid);

			if (cc == null)
			{
				LogFormatted("");
				return;
			}

			removeActiveContract(cc);

			addFailedContract(cc);
			refreshList();
		}

		private void onFinished(Contract c)
		{
			CapComContract cc = getOfferedContract(c.ContractGuid);

			if (cc == null)
			{
				LogFormatted("");
				return;
			}

			removeOfferedContract(cc);
			refreshList();
		}

		private void onOffered(Contract c)
		{
			CapComContract cc = new CapComContract(c);

			addOfferedContract(cc);
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

			while (ContractSystem.Instance == null)
				yield return null;

			LogFormatted_DebugOnly("Loading Contracts");

			foreach(Contract c in ContractSystem.Instance.Contracts)
			{
				CapComContract cc = new CapComContract(c);

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
				CapComContract cc = new CapComContract(c);

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

			loaded = true;
			window.refreshContracts();
		}

		private void refreshList()
		{
			window.refreshContracts();
		}

		private void updateOrbits(CapComContract c)
		{
			LogFormatted_DebugOnly("Trying to activate orbit parameter");
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

				LogFormatted_DebugOnly("Orbit Parameter identified; activating...");

				MethodInfo orbitSetup = (typeof(SpecificOrbitParameter)).GetMethod("setup", BindingFlags.NonPublic | BindingFlags.Instance);

				if (orbitSetup == null)
					return;

				try
				{
					orbitSetup.Invoke(s, null);
					LogFormatted_DebugOnly("Activating Orbit...");
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

				LogFormatted_DebugOnly("Trying To Add FinePrint Waypoint...");

				var waypoints = WaypointManager.Instance().AllWaypoints();

				if (waypoints.Contains(p.Way))
					continue;

				WaypointManager.AddWaypoint(p.Way);

				LogFormatted_DebugOnly("Waypoint Added To System");
			}
		}


		#endregion

	}
}
