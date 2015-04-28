using System;
using System.Collections.Generic;
using System.Linq;
using CapCom.Framework;
using Contracts;
using Contracts.Agents;
using UnityEngine;

namespace CapCom
{
	public class CapComWindow : CC_MBW
	{
		private List<CapComContract> activeContracts = new List<CapComContract>();
		private List<CapComContract> offeredContracts = new List<CapComContract>();
		private List<CapComContract> completedContracts = new List<CapComContract>();
		private List<CapComContract> currentContracts = new List<CapComContract>();
		private List<CapComContract> agentOfferedContracts = new List<CapComContract>();
		private List<CapComContract> agentActiveContracts = new List<CapComContract>();
		private CapComContract currentContract;
		private CapComSettingsWindow settings;

		private Vector2 cScroll, infoScroll;
		private int contractIndex;
		private int currentList;
		private bool controlLock;
		private bool showAgency;
		private bool resizing;
		private bool mouseDown;
		private bool dropdown, warnDecline, warnCancel, sortMenu;
		private Rect ddRect;
		private float dH, dragStart;

		private const string lockID = "CapCom_LockID";

		protected override void Awake()
		{
			WindowRect = new Rect(70, 100, 840, 600);
			WindowCaption = "CapCom";
			WindowOptions = new GUILayoutOption[3] { GUILayout.Width(840), GUILayout.Height(300), GUILayout.MaxHeight(Screen.height) };
			WindowStyle = CapComSkins.newWindowStyle;
			Visible = false;
			DragEnabled = true;
			ClampToScreen = true;
			ClampToScreenOffset = new RectOffset(-650, -650, -300, -300);
			TooltipMouseOffset = new Vector2d(-10, -25);

			currentList = 0;

			CC_SkinsLibrary.SetCurrent("CCUnitySkin");

			InputLockManager.RemoveControlLock(lockID);
		}

		protected override void Start()
		{
			WindowRect.x = CapCom.Settings.windowPosX;
			WindowRect.y = CapCom.Settings.windowPosY;
			WindowRect.yMax = WindowRect.y + CapCom.Settings.windowHeight;
		}

		protected override void OnDestroy()
		{
			CapCom.Settings.windowPosX = WindowRect.x;
			CapCom.Settings.windowPosY = WindowRect.y;
			CapCom.Settings.Save();
		}

		protected override void Update()
		{
			Vector2 mousePos = Input.mousePosition;
			mousePos.y = Screen.height - mousePos.y;
			if (WindowRect.Contains(mousePos))
			{
				if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
					mouseDown = true;
				else
					mouseDown = false;

				if (Input.GetKeyDown(CapCom.Settings.scrollUp))
				{
					if (currentContracts.Count > 0)
					{
						if (currentContract == null)
						{
							contractIndex = currentContracts.Count - 1;
							currentContract = currentContracts[contractIndex];
						}
						else if (contractIndex > 0)
						{
							contractIndex -= 1;
							currentContract = currentContracts[contractIndex];
						}
						else
						{
							contractIndex = currentContracts.Count - 1;
							currentContract = currentContracts[contractIndex];
						}
						showAgency = false;
					}
				}
				if (Input.GetKeyDown(CapCom.Settings.scrollDown))
				{
					if (currentContracts.Count > 0)
					{
						if (currentContract == null)
						{
							contractIndex = 0;
							currentContract = currentContracts[contractIndex];
						}
						else if (contractIndex >= currentContracts.Count - 1)
						{
							contractIndex = 0;
							currentContract = currentContracts[contractIndex];
						}
						else
						{
							contractIndex += 1;
							currentContract = currentContracts[contractIndex];
						}
						showAgency = false;
					}
				}
				if (Input.GetKeyDown(CapCom.Settings.listRight))
				{
					if (currentList < 2)
						currentList += 1;
					else
						currentList = 0;

					sortContracts();

					if (currentContracts.Count > 0)
					{
						contractIndex = 0;
						currentContract = currentContracts[contractIndex];
						showAgency = false;
					}
					else
					{
						currentContract = null;
						contractIndex = 0;
						showAgency = false;
					}
				}
				if (Input.GetKeyDown(CapCom.Settings.listLeft))
				{
					if (currentList > 0)
						currentList -= 1;
					else
						currentList = 2;

					sortContracts();

					if (currentContracts.Count > 0)
					{
						contractIndex = 0;
						currentContract = currentContracts[contractIndex];
						showAgency = false;
					}
					else
					{
						currentContract = null;
						contractIndex = 0;
						showAgency = false;
					}
				}
				if (Input.GetKeyDown(CapCom.Settings.accept))
				{
					if (currentContract == null)
						return;

					if (currentContract.Root.ContractState != Contract.State.Offered)
						return;

					if (!CapCom.Settings.activeLimit || ContractSystem.Instance.GetActiveContractCount() < GameVariables.Instance.GetActiveContractsLimit(ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.MissionControl)))
						currentContract.Root.Accept();
				}
				if (Input.GetKeyDown(CapCom.Settings.cancel))
				{
					if (currentContract == null)
						return;

					if (currentContract.Root.ContractState == Contract.State.Offered)
					{
						if (CapCom.Settings.showDeclineWarning)
						{
							dropdown = true;
							warnDecline = true;
						}
						else
							currentContract.Root.Decline();
					}
					else if (currentContract.Root.ContractState == Contract.State.Active)
					{
						if (CapCom.Settings.showCancelWarning)
						{
							dropdown = true;
							warnCancel = true;
						}
						else
							currentContract.Root.Cancel();
					}
				}
			}
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
			}
		}

		private void sortContracts(List<CapComContract> ccList)
		{
			switch (CapCom.Settings.sortMode)
			{
				case 0:
					ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.Root.Prestige.CompareTo(b.Root.Prestige), a.Name.CompareTo(b.Name)));
					break;
				case 1:
					ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.TotalReward.CompareTo(b.TotalReward), a.Name.CompareTo(b.Name)));
					break;
				case 2:
					ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.TotalSciReward.CompareTo(b.TotalSciReward), a.Name.CompareTo(b.Name)));
					break;
				case 3:
					ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.TotalRepReward.CompareTo(b.TotalRepReward), a.Name.CompareTo(b.Name)));
					break;
				case 4:
					ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.RootAgent.Name.CompareTo(b.RootAgent.Name), a.Name.CompareTo(b.Name)));
					break;
				default:
					ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.Root.Prestige.CompareTo(b.Root.Prestige), a.Name.CompareTo(b.Name)));
					break;
			}

			currentContracts = ccList;
		}

		private void unlockControls()
		{
			switch (HighLogic.LoadedScene)
			{
				case GameScenes.TRACKSTATION:
				case GameScenes.SPACECENTER:
				case GameScenes.FLIGHT:
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
				else if (WindowRect.Contains(mousePos) && !controlLock)
				{
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, lockID);
					controlLock = true;
				}
				else if (!WindowRect.Contains(mousePos) && controlLock)
					unlockControls();
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

			if (!dropdown)
			{
				warnCancel = false;
				warnDecline = false;
				sortMenu = false;
			}
		}

		protected override void DrawWindow(int id)
		{
			drawVersion(id);
			closeButton(id);

			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
				GUILayout.BeginVertical();
					menuBar(id);
					contractTabs(id);
					contractList(id);
				GUILayout.EndVertical();
				GUILayout.Space(10);
				GUILayout.BeginVertical();
					currentContractControls(id);
					GUILayout.Space(10);
					currentContractHeader(id);
					if (showAgency)
						currentAgentInfo(id);
					else
						currentContractInfo(id);
				GUILayout.EndVertical();
				GUILayout.Space(10);
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			dropDown(id);
			rescaleHandle(id);
		}

		protected override void DrawWindowPost(int id)
		{
			if (dropdown && Event.current.type == EventType.mouseDown && !ddRect.Contains(Event.current.mousePosition))
			{
				dropdown = false;
			}
		}

		private void drawVersion(int id)
		{
			Rect r = new Rect(6, 0, 50, 18);
			GUI.Label(r, CapCom.Version, CapComSkins.smallText);
		}

		private void closeButton(int id)
		{
			Rect r = new Rect(WindowRect.width - 60, 1, 24, 24);
			if (GUI.Button(r, CapComSkins.settingsIcon, CapComSkins.textureButton))
			{
				if (settings == null)
					settings = gameObject.AddComponent<CapComSettingsWindow>();
				settings.Visible = !settings.Visible;
			}

			r.x += 32;
			r.width = r.height = 22;

			if (GUI.Button(r, "✖", CapComSkins.textureButton))
			{
				unlockControls();
				Visible = false;
			}
		}

		private void menuBar(int id)
		{
			GUILayout.BeginHorizontal();
				if (GUILayout.Button("Sort", CapComSkins.tabButton, GUILayout.Width(60)))
				{
					dropdown = !dropdown;
					sortMenu = !sortMenu;
				}
				GUILayout.Space(10);
				if (GUILayout.Button("Asc/Desc", CapComSkins.tabButton, GUILayout.Width(70)))
				{
					CapCom.Settings.ascending = !CapCom.Settings.ascending;
					CapCom.Settings.Save();
					sortContracts();
				}
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		private void contractTabs(int id)
		{
			GUILayout.BeginHorizontal();
				if (dropdown)
				{
					GUILayout.Label("Offered", CapComSkins.tabButton, GUILayout.Width(100));
					GUILayout.Label("Active", CapComSkins.tabButton, GUILayout.Width(100));
					GUILayout.Label("Completed", CapComSkins.tabButton, GUILayout.Width(100));
				}
				else
				{
					if (GUILayout.Button("Offered", CapComSkins.tabButton, GUILayout.Width(100)))
					{
						currentList = 0;
						sortContracts();
					}

					if (GUILayout.Button("Active", CapComSkins.tabButton, GUILayout.Width(100)))
					{
						currentList = 1;
						sortContracts();
					}

					if (GUILayout.Button("Completed", CapComSkins.tabButton, GUILayout.Width(100)))
					{
						currentList = 2;
						sortContracts();
					}
				}
			GUILayout.EndHorizontal();
		}

		private void contractList(int id)
		{
			cScroll = GUILayout.BeginScrollView(cScroll, false, true, GUILayout.Width(324));

			for (int i = 0; i < currentContracts.Count; i++)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Space(63);
					if (dropdown)
					{
						GUILayout.Label(currentContracts[i].Name, CapComSkins.titleButtonBehind, GUILayout.Width(240), GUILayout.Height(46));
					}
					else
					{
						if (GUILayout.Button(currentContracts[i].Name, GUILayout.Width(240), GUILayout.Height(46)))
						{
							currentContract = currentContracts[i];
							contractIndex = i;
							showAgency = false;
						}
					}
					Rect r = GUILayoutUtility.GetLastRect();
				GUILayout.EndHorizontal();
				GUILayout.Space(-4);

				r.width = 60;
				r.x -= 60;
				r.height = 46;
				if (!dropdown)
				{
					if (GUI.Button(r, "", CapComSkins.iconButton))
					{
						currentContract = currentContracts[i];
						contractIndex = i;
						showAgency = false;
					}
				}
				GUI.DrawTextureWithTexCoords(r, CapComSkins.missionControlTexture, CapComSkins.flagRect);
				r.x += 1;
				r.y += 3;
				r.width = 58;
				r.height = 40;
				GUI.DrawTexture(r, currentContracts[i].RootAgent.LogoScaled);
				r.width = 11;
				r.height = 40;
				r.x += 287;
				GUI.DrawTexture(r, contractPrestigeIcon(currentContracts[i].Root.Prestige));
			}
			GUILayout.EndScrollView();
		}

		private void currentContractControls(int id)
		{
			if (currentContract == null)
				return;

			if (currentContract.Root.ContractState == Contract.State.Offered)
			{
				GUI.Label(new Rect(550, 15, 100, 20), "Offered", CapComSkins.headerText);

				Rect r = new Rect(WindowRect.width - 60, 40, 44, 44);

				if (GUI.Button(r, "", CapComSkins.iconButton))
				{
					if (!CapCom.Settings.activeLimit || ContractSystem.Instance.GetActiveContractCount() < GameVariables.Instance.GetActiveContractsLimit(ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.MissionControl)))
						currentContract.Root.Accept();
				}
				if (r.Contains(Event.current.mousePosition) && mouseDown)
					GUI.DrawTextureWithTexCoords(r, CapComSkins.missionControlTexture, CapComSkins.acceptButtonActive);
				else if (r.Contains(Event.current.mousePosition))
					GUI.DrawTextureWithTexCoords(r, CapComSkins.missionControlTexture, CapComSkins.acceptButtonHover);
				else
					GUI.DrawTextureWithTexCoords(r, CapComSkins.missionControlTexture, CapComSkins.acceptButtonNormal);

				r.y += 70;

				if (GUI.Button(r, "", CapComSkins.iconButton))
				{
					if (CapCom.Settings.showDeclineWarning)
					{
						dropdown = true;
						warnDecline = true;
					}
					else
						currentContract.Root.Decline();
				}
				if (r.Contains(Event.current.mousePosition) && mouseDown)
					GUI.DrawTextureWithTexCoords(r, CapComSkins.missionControlTexture, CapComSkins.declineButtonActive);
				else if (r.Contains(Event.current.mousePosition))
					GUI.DrawTextureWithTexCoords(r, CapComSkins.missionControlTexture, CapComSkins.declineButtonHover);
				else
					GUI.DrawTextureWithTexCoords(r, CapComSkins.missionControlTexture, CapComSkins.declineButtonNormal);

			}
			else if (currentContract.Root.ContractState == Contract.State.Active)
			{
				GUI.Label(new Rect(550, 15, 100, 20), "Active", CapComSkins.headerText);

				Rect r = new Rect(WindowRect.width - 60, 110, 44, 44);

				if (GUI.Button(r, "", CapComSkins.iconButton))
				{
					if (CapCom.Settings.showCancelWarning)
					{
						dropdown = true;
						warnCancel = true;
					}
					else
						currentContract.Root.Cancel();
				}
				if (r.Contains(Event.current.mousePosition) && mouseDown)
					GUI.DrawTextureWithTexCoords(r, CapComSkins.missionControlTexture, CapComSkins.cancelButtonActive);
				else if (r.Contains(Event.current.mousePosition))
					GUI.DrawTextureWithTexCoords(r, CapComSkins.missionControlTexture, CapComSkins.cancelButtonHover);
				else
					GUI.DrawTextureWithTexCoords(r, CapComSkins.missionControlTexture, CapComSkins.cancelButtonNormal);
			}
			else
			{
				GUI.Label(new Rect(550, 15, 100, 20), "Completed", CapComSkins.headerText);
			}
		}

		private void currentContractHeader(int id)
		{
			if (currentContract == null)
				return;

			Rect r = new Rect(340, 33, 160, 106);

			if (GUI.Button(r, "", CapComSkins.iconButton))
			{
				showAgency = !showAgency;
				agentOfferedContracts.Clear();
				agentActiveContracts.Clear();
				foreach (CapComContract c in offeredContracts)
				{
					if (c.RootAgent == currentContract.RootAgent)
					{
						if (c == currentContract)
							continue;

						agentOfferedContracts.Add(c);
					}
				}
				foreach (CapComContract c in activeContracts)
				{
					if (c.RootAgent == currentContract.RootAgent)
					{
						if (c == currentContract)
							continue;

						agentActiveContracts.Add(c);
					}
				}
			}
			GUI.DrawTextureWithTexCoords(r, CapComSkins.missionControlTexture, CapComSkins.flagRect);
			r.x += 4;
			r.y += 5;
			r.width = 152;
			r.height = 95;
			GUI.DrawTexture(r, currentContract.RootAgent.Logo);

			GUILayout.BeginHorizontal();
			GUILayout.Space(170);
			GUILayout.BeginVertical();
			GUILayout.Label("Contract:", CapComSkins.headerText, GUILayout.Width(80));
			r = GUILayoutUtility.GetLastRect();
			r.x += 100;
			r.width = 60;
			r.height = 16;
			GUI.DrawTexture(r, contractPrestigeIcon(currentContract.Root.Prestige, false));
			GUILayout.Label(currentContract.Name, CapComSkins.titleText, GUILayout.Width(270));
			GUILayout.Label("Agent:", CapComSkins.headerText, GUILayout.Width(80));
			GUILayout.Label(currentContract.RootAgent.Name, CapComSkins.titleText, GUILayout.Width(270));
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		private void currentContractInfo(int id)
		{
			if (currentContract == null)
				return;

			GUILayout.Space(10);

			infoScroll = GUILayout.BeginScrollView(infoScroll, GUILayout.Width(500));

			GUILayout.Label("Briefing:", CapComSkins.headerText, GUILayout.Width(80));

			if (!CapCom.Settings.hideBriefing)
			{
				GUILayout.Label(currentContract.Briefing, CapComSkins.briefingText);
			}

			GUILayout.Label(currentContract.Root.Synopsys, CapComSkins.synopsisText);

			if (currentContract.Root.ContractState == Contract.State.Offered)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Offer Expires: ", CapComSkins.headerText, GUILayout.Width(85));
					GUILayout.Label(KSPUtil.PrintDateDeltaCompact((int)(currentContract.Root.DateExpire - Planetarium.fetch.time), true,	true), CapComSkins.timerText, GUILayout.Width(100));
					if (currentContract.Root.DateDeadline != 0)
					{
						GUILayout.Label("Mission Deadline: ", CapComSkins.headerText, GUILayout.Width(115));
						GUILayout.Label(KSPUtil.PrintDateDeltaCompact((int)(currentContract.Root.DateDeadline - Planetarium.fetch.time),	true, true), CapComSkins.timerText, GUILayout.Width(140));
					}
					else if (currentContract.Root.TimeDeadline != 0)
					{
						GUILayout.Label("Mission Duration: ", CapComSkins.headerText, GUILayout.Width(110));
						GUILayout.Label(KSPUtil.PrintDateDeltaCompact((int)currentContract.Root.TimeDeadline, true, true),	CapComSkins.timerText, GUILayout.Width(140));
					}
				GUILayout.EndHorizontal();
			}
			else if (currentContract.Root.ContractState == Contract.State.Active)
			{
				GUILayout.BeginHorizontal();
					if (currentContract.Root.DateDeadline != 0)
					{
						GUILayout.Label("Mission Deadline: ", CapComSkins.headerText, GUILayout.Width(115));
						GUILayout.Label(KSPUtil.PrintDateDeltaCompact((int)(currentContract.Root.DateDeadline - Planetarium.fetch.time),	true, true), CapComSkins.timerText, GUILayout.Width(150));
					}
				GUILayout.EndHorizontal();
			}

			if (!string.IsNullOrEmpty(currentContract.Notes))
			{
				GUILayout.Label("Mission Notes: ", CapComSkins.headerText, GUILayout.Width(100));

				if (CapCom.Settings.hideNotes)
				{
					Rect r = GUILayoutUtility.GetLastRect();
					r.x += 120;
					r.width = 13;
					r.height = 16;
					if (GUI.Button(r, currentContract.ShowNotes ? CapComSkins.notesMinusIcon : CapComSkins.notesPlusIcon, CapComSkins.textureButton))
						currentContract.ShowNotes = !currentContract.ShowNotes;
				}

				if (!CapCom.Settings.hideNotes || currentContract.ShowNotes)
					GUILayout.Label(currentContract.Notes, CapComSkins.noteText);
			}

			if (currentContract.ParameterCount > 0)
			{
				GUILayout.Label("Objectives: ", CapComSkins.headerText, GUILayout.Width(100));

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

			GUILayout.Label("Rewards: ", CapComSkins.headerText, GUILayout.Width(80));
			if (currentContract.Root.FundsAdvance > 0)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Advance: ", CapComSkins.advance, GUILayout.Width(80));
					Rect r = GUILayoutUtility.GetLastRect();
					r.x += 62;
					sizedContent(ref r, currentContract.FundsAdv, Currency.Funds);
				GUILayout.EndHorizontal();
			}
			if (currentContract.Root.FundsCompletion > 0 || currentContract.Root.ReputationCompletion > 0 || currentContract.Root.ScienceCompletion > 0)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Completion: ", CapComSkins.completion, GUILayout.Width(80));
					Rect r = GUILayoutUtility.GetLastRect();
					r.x += 76;
					if (currentContract.Root.FundsCompletion > 0)
						sizedContent(ref r, currentContract.FundsRew, Currency.Funds);
					if (currentContract.Root.ScienceCompletion > 0)
						sizedContent(ref r, currentContract.SciRew, Currency.Science);
					if (currentContract.Root.ReputationCompletion > 0)
						sizedContent(ref r, currentContract.RepRew, Currency.Reputation);
				GUILayout.EndHorizontal();
			}
			if (currentContract.Root.FundsFailure > 0 || currentContract.Root.ReputationFailure > 0)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Failure: ", CapComSkins.failure, GUILayout.Width(80));
					Rect r = GUILayoutUtility.GetLastRect();
					r.x += 50;
					if (currentContract.Root.FundsFailure > 0)
						sizedContent(ref r, currentContract.FundsPen, Currency.Funds);
					if (currentContract.Root.ReputationFailure > 0)
						sizedContent(ref r, currentContract.RepPen, Currency.Reputation);
				GUILayout.EndHorizontal();
			}

			GUILayout.EndScrollView();
		}

		private void drawParameter(CapComParameter cp)
		{
			bool notes = !string.IsNullOrEmpty(cp.Notes);

			GUILayout.BeginHorizontal();
				if (notes && CapCom.Settings.hideNotes)
					GUILayout.Space(30 + cp.Level * 8);
				else
					GUILayout.Space(16 + cp.Level * 8);

				GUILayout.BeginVertical();
					GUILayout.Label(cp.Name, cp.Level == 0 ? CapComSkins.parameterText : CapComSkins.subParameterText);
					Rect b = GUILayoutUtility.GetLastRect();

					if (notes && CapCom.Settings.hideNotes)
					{
						b.x -= 28;
						b.height = 16;
						b.width = 13;
						if (GUI.Button(b, cp.ShowNotes ? CapComSkins.notesMinusIcon :CapComSkins.notesPlusIcon, CapComSkins.textureButton))
							cp.ShowNotes = !cp.ShowNotes;

						b.y += 4;
						b.x += 16;
						b.height = 10;
						b.width = 10;
						GUI.DrawTexture(b, parameterStateIcon(cp.Param));
					}
					else
					{
						b.x -= 14;
						b.y += 4;
						b.height = 10;
						b.width = 10;
						GUI.DrawTexture(b, parameterStateIcon(cp.Param));
					}

					if (notes)
					{
						if (!CapCom.Settings.hideNotes || cp.ShowNotes)
							GUILayout.Label(cp.Notes, CapComSkins.noteText);
					}

					if (cp.Param.FundsCompletion > 0 || cp.Param.ReputationCompletion > 0 || cp.Param.ScienceCompletion > 0)
					{
						GUILayout.BeginHorizontal();
							GUILayout.Label("Completion: ", CapComSkins.completion, GUILayout.Width(80));
							Rect r = GUILayoutUtility.GetLastRect();
							r.x += 76;
							if (cp.Param.FundsCompletion > 0)
								sizedContent(ref r, cp.FundsRew, Currency.Funds);
							if (cp.Param.ScienceCompletion > 0)
								sizedContent(ref r, cp.SciRew, Currency.Science);
							if (cp.Param.ReputationCompletion > 0)
								sizedContent(ref r, cp.RepRew, Currency.Reputation);
						GUILayout.EndHorizontal();
					}
					if (cp.Param.FundsFailure > 0 || cp.Param.ReputationFailure > 0)
					{
						GUILayout.BeginHorizontal();
							GUILayout.Label("Failure: ", CapComSkins.failure, GUILayout.Width(80));
							Rect r = GUILayoutUtility.GetLastRect();
							r.x += 50;
							if (cp.Param.FundsFailure > 0)
								sizedContent(ref r, cp.FundsPen, Currency.Funds);
							if (cp.Param.ReputationFailure > 0)
								sizedContent(ref r, cp.RepPen, Currency.Reputation);
						GUILayout.EndHorizontal();
					}
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		private void currentAgentInfo(int id)
		{
			if (currentContract == null)
				return;

			GUILayout.Space(10);

			infoScroll = GUILayout.BeginScrollView(infoScroll, GUILayout.Width(500));

			GUILayout.Label("Agency Description:", CapComSkins.headerText);
			GUILayout.Label(currentContract.RootAgent.Description, CapComSkins.briefingText);

			if (currentContract.RootAgent.Mentality.Count > 0)
			{
				GUILayout.Label("Agency Mentalities:", CapComSkins.headerText);
				foreach (AgentMentality m in currentContract.RootAgent.Mentality)
				{
					GUILayout.Label(m.GetType().Name + ":", CapComSkins.synopsisText);
					if (string.IsNullOrEmpty(m.Description))
						continue;
					GUILayout.BeginHorizontal();
						GUILayout.Space(20);
						GUILayout.Label(m.Description, CapComSkins.briefingText);
					GUILayout.EndHorizontal();
				}
			}

			if (agentOfferedContracts.Count > 0)
			{
				GUILayout.Label("Currently Offered Contracts:", CapComSkins.headerText);
				foreach (CapComContract c in agentOfferedContracts)
				{
					GUILayout.BeginHorizontal();
						GUILayout.Space(20);
						GUILayout.Label(c.Name, CapComSkins.agencyContractText);
					GUILayout.EndHorizontal();
				}
			}

			if (agentActiveContracts.Count > 0)
			{
				GUILayout.Label("Currently Active Contracts:", CapComSkins.headerText);
				foreach (CapComContract c in agentActiveContracts)
				{
					GUILayout.BeginHorizontal();
						GUILayout.Space(20);
						GUILayout.Label(c.Name, CapComSkins.agencyContractText);
					GUILayout.EndHorizontal();
				}
			}

			GUILayout.EndScrollView();
		}

		private void rescaleHandle(int id)
		{
			Rect r = new Rect(WindowRect.width - 24, WindowRect.height - 24, 22, 22);
			GUI.DrawTexture(r, CapComSkins.resizeHandle);

			if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
			{
				if (r.Contains(Event.current.mousePosition))
				{
					resizing = true;
					dragStart = Input.mousePosition.y;
					dH = WindowRect.height;
					Event.current.Use();
				}
			}

			if (resizing)
			{
				if (Input.GetMouseButtonUp(0))
				{
					resizing = false;
					WindowRect.yMax = WindowRect.y + dH;
					CapCom.Settings.windowHeight = WindowRect.height;
					CapCom.Settings.Save();
				}
				else
				{
					float height = Input.mousePosition.y;
					if (Input.mousePosition.y < 0)
						height = 0;
					dH += dragStart - height;
					dragStart = height;
					WindowRect.yMax = WindowRect.y + dH;
					if (WindowRect.yMax > Screen.height)
					{
						WindowRect.yMax = Screen.height;
						dH = WindowRect.yMax - WindowRect.y;
					}
					if (dH < 300)
						dH = 300;
				}
			}
		}

		private void dropDown(int id)
		{
			if (dropdown)
			{
				if (sortMenu)
				{
					ddRect = new Rect(15, 40, 100, 144);
					GUI.Box(ddRect, "");
					Rect r = new Rect(ddRect.x + 5, ddRect.y + 5, 90, 20);
					GUI.Label(r, "Sort Options:");
					for (int i = 0; i < 5; i++)
					{
						r.y += 22;
						if (GUI.Button(r, sortName(i), CapComSkins.menuButton))
						{
							CapCom.Settings.sortMode = i;
							CapCom.Settings.Save();
							sortContracts();
							sortMenu = false;
							dropdown = false;
						}
					}
				}
				else if (warnCancel)
				{
					ddRect = new Rect(WindowRect.width - 230, 95, 160, 70);
					GUI.Box(ddRect, "");
					Rect r = new Rect(ddRect.x + 10, ddRect.y + 5, 140, 30);
					GUI.Label(r, "Cancel this contract?", CapComSkins.warningText);
					r = new Rect(ddRect.x + 40, ddRect.y + 30, 80, 30);
					if (GUI.Button(r, "Confirm", CapComSkins.warningButton))
					{
						currentContract.Root.Cancel();
						warnCancel = false;
						dropdown = false;
					}
				}
				else if (warnDecline)
				{
					ddRect = new Rect(WindowRect.width - 230, 95, 160, 70);
					GUI.Box(ddRect, "");
					Rect r = new Rect(ddRect.x + 10, ddRect.y + 5, 140, 30);
					GUI.Label(r, "Decline this contract?", CapComSkins.warningText);
					r = new Rect(ddRect.x + 40, ddRect.y + 30, 80, 30);
					if (GUI.Button(r, "Confirm", CapComSkins.warningButton))
					{
						currentContract.Root.Decline();
						warnDecline = false;
						dropdown = false;
					}
				}
				else
					dropdown = false;
			}
		}

		private void sizedContent(ref Rect r, string t, Currency type)
		{
			if (string.IsNullOrEmpty(t))
				return;
			GUIStyle s = currencyStyle(type);
			Vector2 sz = s.CalcSize(new GUIContent(t));

			if (type == Currency.Funds)
				r.width = 10;
			else
				r.width = 16;
			r.height = 16;

			GUI.DrawTexture(r, currencyIcon(type));

			r.x += r.width + 4;

			r.width = sz.x;
			r.height = sz.y;

			GUI.Label(r, t, s);

			r.x += sz.x + 14;
		}

		private GUIStyle currencyStyle(Currency t)
		{
			switch (t)
			{
				case Currency.Funds:
					return CapComSkins.funds;
				case Currency.Reputation:
					return CapComSkins.rep;
				default:
					return CapComSkins.sci;
			}
		}

		private Texture2D currencyIcon(Currency t)
		{
			switch (t)
			{
				case Currency.Funds:
					return CapComSkins.fundsGreen;
				case Currency.Reputation:
					return CapComSkins.repRed;
				default:
					return CapComSkins.science;
			}
		}

		private Texture2D contractPrestigeIcon(Contract.ContractPrestige p, bool vertical = true)
		{
			switch (p)
			{
				case Contract.ContractPrestige.Trivial:
					return vertical ? CapComSkins.goldStarVertical : CapComSkins.goldStar;
				case Contract.ContractPrestige.Significant:
					return vertical ? CapComSkins.goldStarTwoVertical : CapComSkins.goldStarTwo;
				default:
					return vertical ? CapComSkins.goldStarThreeVertical : CapComSkins.goldStarThree;
			}
		}

		private Texture2D parameterStateIcon(ContractParameter p)
		{
			switch (p.State)
			{
				case ParameterState.Complete:
					return CapComSkins.checkBox;
				case ParameterState.Incomplete:
					return CapComSkins.emptyBox;
				default:
					return CapComSkins.failBox;
			}
		}

		private string sortName(int i)
		{
			switch (i)
			{
				case 0:
					return "Difficulty";
				case 1:
					return "Funds";
				case 2:
					return "Science";
				case 3:
					return "Reputation";
				case 4:
					return "Agency";
				default:
					return "";
			}
		}

	}
}
