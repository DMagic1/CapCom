using System;
using System.Collections.Generic;
using System.Linq;
using CapCom.Framework;
using Contracts;
using UnityEngine;

namespace CapCom
{
	public class CapComWindow : CC_MBW
	{
		private List<CapComContract> activeContracts = new List<CapComContract>();
		private List<CapComContract> offeredContracts = new List<CapComContract>();
		private List<CapComContract> completedContracts = new List<CapComContract>();
		private List<CapComContract> failedContracts = new List<CapComContract>();
		private List<CapComContract> currentContracts = new List<CapComContract>();
		private CapComContract currentContract;

		private int currentList;
		private bool controlLock;
		private bool showBriefing = true;
		private const string lockID = "CapCom_LockID";

		protected override void Awake()
		{
			LogFormatted_DebugOnly("Starting CapCom Window");
			WindowCaption = "";
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(800), GUILayout.Height(600) };
			WindowStyle = CapComSkins.newWindowStyle;
			Visible = false;
			DragEnabled = true;
			ClampToScreen = false;
			TooltipMouseOffset = new Vector2d(-10, -25);

			currentList = 0;

			CC_SkinsLibrary.SetCurrent("CCUnitySkin");

			InputLockManager.RemoveControlLock(lockID);

		}

		protected override void Start()
		{
			
		}

		protected override void OnDestroy()
		{
			
		}

		public void refreshContracts()
		{
			if (CapCom.Instance != null)
			{
				if (CapCom.Instance.Loaded)
				{
					activeContracts = CapCom.Instance.getActiveContracts;
					offeredContracts = CapCom.Instance.getOfferedContracts;
					completedContracts = CapCom.Instance.getCompletedContracts;
					failedContracts = CapCom.Instance.getFailedContracts;

					sortContracts();
				}
			}
		}

		private void sortContracts()
		{
			switch (currentList)
			{
				case 0:
					sortContracts(offeredContracts);
					break;
				case 1:
					sortContracts(activeContracts);
					break;
				case 2:
					sortContracts(completedContracts);
					break;
				case 3:
					sortContracts(failedContracts);
					break;
			}
		}

		private void sortContracts(List<CapComContract> ccList)
		{


			currentContracts = ccList;
		}

		private void unlockControls()
		{
			switch (HighLogic.LoadedScene)
			{
				case GameScenes.TRACKSTATION:
				case GameScenes.SPACECENTER:
					InputLockManager.RemoveControlLock(lockID);
					controlLock = false;
					break;
				case GameScenes.EDITOR:
					EditorLogic.fetch.Unlock(lockID);
					controlLock = false;
					break;
				default:
					break;
			}
		}

		protected override void DrawWindowPre(int id)
		{
			//Prevent click through from activating part options
			if (HighLogic.LoadedSceneIsFlight)
			{
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && GUIUtility.hotControl == 0 && Input.GetMouseButton(0))
				{
					foreach (var window in GameObject.FindObjectsOfType(typeof(UIPartActionWindow)).OfType<UIPartActionWindow>().Where(p => p.Display == UIPartActionWindow.DisplayType.Selected))
					{
						window.enabled = false;
						window.displayDirty = true;
					}
				}
			}

			//Lock space center click through
			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !controlLock)
				{
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.KSC_FACILITIES | ControlTypes.KSC_UI, lockID);
					controlLock = true;
				}
				else if (!WindowRect.Contains(mousePos) && controlLock)
					unlockControls();
			}

			//Lock tracking scene click through
			if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !controlLock)
				{
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.TRACKINGSTATION_ALL, lockID);
					controlLock = true;
				}
				else if (!WindowRect.Contains(mousePos) && controlLock)
					unlockControls();
			}

			//Lock editor click through
			if (HighLogic.LoadedSceneIsEditor)
			{
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !controlLock)
				{
					EditorLogic.fetch.Lock(true, true, true, lockID);
					controlLock = true;
				}
				else if (!WindowRect.Contains(mousePos) && controlLock)
					unlockControls();
			}
		}

		protected override void DrawWindow(int id)
		{
			drawVersion(id);
			closeButton(id);

			GUILayout.BeginHorizontal();
				GUILayout.BeginVertical();
					menuBar(id);
					contractTabs(id);
					contractList(id);
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
					currentContractControls(id);
					currentContractHeader(id);
					currentContractInfo(id);
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			rescaleHandle(id);
		}

		protected override void DrawWindowPost(int id)
		{
			
		}

		private void drawVersion(int id)
		{
			Rect r = new Rect(6, 0, 50, 18);
			GUI.Label(r, CapCom.version);
		}

		private void closeButton(int id)
		{
			Rect r = new Rect(WindowRect.width - 50, 1, 18, 18);
			if (GUI.Button(r, CapComSkins.settingsIcon))
			{
				//Settings Window
			}

			r.x += 28;

			if (GUI.Button(r, "✖"))
			{
				unlockControls();
				Visible = false;
			}
		}

		private void menuBar(int id)
		{
			//Sorting options, etc...
			GUILayout.BeginHorizontal();


			GUILayout.EndHorizontal();
		}

		private void contractTabs(int id)
		{
			//Tabs for different contract lists
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Offered", CapComSkins.tabButton, GUILayout.Width(60)))
			{
				currentList = 0;
				sortContracts();
			}

			if (GUILayout.Button("Active", CapComSkins.tabButton, GUILayout.Width(55)))
			{
				currentList = 1;
				sortContracts();
			}

			if (GUILayout.Button("Completed", CapComSkins.tabButton, GUILayout.Width(80)))
			{
				currentList = 2;
				sortContracts();
			}

			if (GUILayout.Button("Failed", CapComSkins.tabButton, GUILayout.Width(55)))
			{
				currentList = 3;
				sortContracts();
			}

			GUILayout.EndHorizontal();
		}

		private void contractList(int id)
		{
			//Display sorted contract list
			foreach (CapComContract cc in currentContracts)
			{
				if (GUILayout.Button(cc.Name, GUILayout.Width(250)))
				{
					currentContract = cc;
				}
			}
		}

		private void currentContractControls(int id)
		{
			if (currentContract == null)
				return;

			GUILayout.BeginHorizontal();
			if (currentContract.Root.ContractState == Contract.State.Offered)
			{
				if (GUILayout.Button("Accept"))
				{
					currentContract.Root.Accept();
				}

				if (GUILayout.Button("Decline"))
				{
					currentContract.Root.Decline();
				}
			}
			else if (currentContract.Root.ContractState == Contract.State.Active)
			{
				if (GUILayout.Button("Cancel"))
				{
					currentContract.Root.Cancel();
				}
			}

			GUILayout.EndHorizontal();
		}

		private void currentContractHeader(int id)
		{
			if (currentContract == null)
				return;

			Rect r = new Rect(280, 50, 160, 100);

			GUI.DrawTexture(r, currentContract.Root.Agent.Logo);

			GUILayout.BeginHorizontal();
				GUILayout.Space(200);
				GUILayout.BeginVertical();
					GUILayout.Label("Contract:");
					GUILayout.Label(currentContract.Name);
					GUILayout.Label("Agent:");
					GUILayout.Label(currentContract.Root.Agent.Name);
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		private void currentContractInfo(int id)
		{
			if (currentContract == null)
				return;

			//Display current contract info
			if (showBriefing)
			{
				GUILayout.Label("Briefing:");
				GUILayout.Label(currentContract.Root.Description);
			}
			GUILayout.Label(currentContract.Root.Synopsys);

			if (currentContract.Root.ContractState == Contract.State.Offered)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Offer Expires: " + KSPUtil.PrintDateDeltaCompact((int)(currentContract.Root.DateExpire - Planetarium.fetch.time), true, true));
					if (currentContract.Root.DateDeadline != 0)
						GUILayout.Label("Mission Deadline: " + KSPUtil.PrintDateDeltaCompact((int)(currentContract.Root.DateDeadline - Planetarium.fetch.time), true, true));
					else if (currentContract.Root.TimeDeadline != 0)
						GUILayout.Label("Mission Duration: " + KSPUtil.PrintDateDeltaCompact((int)currentContract.Root.TimeDeadline, true, true));
				GUILayout.EndHorizontal();
			}
			else if (currentContract.Root.ContractState == Contract.State.Active)
			{
				if (currentContract.Root.DateDeadline != 0)
					GUILayout.Label("Mission Deadline: " + KSPUtil.PrintDateDeltaCompact((int)(currentContract.Root.DateDeadline - Planetarium.fetch.time), true, true));
			}

			if (!string.IsNullOrEmpty(currentContract.Notes))
				GUILayout.Label("Mission Notes: " + currentContract.Notes);

			if (currentContract.ParameterCount > 0)
			{
				GUILayout.Label("Objectives: ");

				for (int i = 0; i < currentContract.ParameterCount; i++)
				{
					CapComParameter cp = currentContract.getParameter(i);
					if (cp == null)
						continue;
					if (cp.Level > 4)
						continue;

					drawParameter(cp);

					for (int j = 0; j < cp.ParameterCount; j++)
					{
						CapComParameter subP = cp.getParameter(j);
						if (subP == null)
							continue;
						if (subP.Level > 4)
							continue;

						drawParameter(subP);
					}
				}
			}

			GUILayout.Label("Rewards: ");
			if (currentContract.Root.FundsAdvance > 0)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Advance: ");
					GUILayout.Label(currentContract.FundsAdv);
				GUILayout.EndHorizontal();
			}
			if (currentContract.Root.FundsCompletion > 0 || currentContract.Root.ReputationCompletion > 0 || currentContract.Root.ScienceCompletion > 0)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Completion: ");
					GUILayout.Label(currentContract.FundsRew);
					GUILayout.Label(currentContract.RepRew);
					GUILayout.Label(currentContract.SciRew);
				GUILayout.EndHorizontal();
			}
			if (currentContract.Root.FundsFailure > 0 || currentContract.Root.ReputationFailure > 0)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Failure: ");
					GUILayout.Label(currentContract.FundsPen);
					GUILayout.Label(currentContract.RepPen);
				GUILayout.EndHorizontal();
			}
		}

		private void drawParameter(CapComParameter cp)
		{
			GUILayout.BeginHorizontal();
				GUILayout.Space(1 + cp.Level * 3);
				GUILayout.BeginVertical();
					GUILayout.Label(cp.Name);
					if (!string.IsNullOrEmpty(cp.Notes))
						GUILayout.Label(cp.Notes);
					if (cp.Param.FundsCompletion > 0 || cp.Param.ReputationCompletion > 0 || cp.Param.ScienceCompletion > 0)
					{
						GUILayout.BeginHorizontal();
							GUILayout.Label("Completion: ");
							GUILayout.Label(cp.FundsRew);
							GUILayout.Label(cp.RepRew);
							GUILayout.Label(cp.SciRew);
						GUILayout.EndHorizontal();
					}
					if (cp.Param.FundsFailure > 0 || cp.Param.ReputationFailure > 0)
					{
						GUILayout.BeginHorizontal();
							GUILayout.Label("Failure: ");
							GUILayout.Label(cp.FundsPen);
							GUILayout.Label(cp.RepPen);
						GUILayout.EndHorizontal();
					}
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		private void rescaleHandle(int id)
		{

		}
	}
}
