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

		private Vector2 cScroll, infoScroll;
		private int currentList;
		private bool controlLock;
		private bool showBriefing = true;
		private bool showAgency;
		private const string lockID = "CapCom_LockID";

		protected override void Awake()
		{
			WindowCaption = "CapCom";
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(800), GUILayout.Height(500) };
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
					//failedContracts = CapCom.Instance.getFailedContracts;

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
				//case 3:
				//	sortContracts(failedContracts);
				//	break;
			}
		}

		private void sortContracts(List<CapComContract> ccList)
		{
			ccList.Sort((a,b) => RUIutils.SortAscDescPrimarySecondary(true, a.Root.Prestige.CompareTo(b.Root.Prestige), a.Name.CompareTo(b.Name)));

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
				GUILayout.Space(10);
				GUILayout.BeginVertical();
					currentContractControls(id);
					currentContractHeader(id);
					if (showAgency)
						currentAgentInfo(id);
					else
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
			GUI.Label(r, CapCom.Version, CapComSkins.smallText);
		}

		private void closeButton(int id)
		{
			Rect r = new Rect(WindowRect.width - 50, 1, 18, 18);
			//if (GUI.Button(r, CapComSkins.settingsIcon, CapComSkins.textureButton))
			//{
			//	//Settings Window
			//}

			r.x += 28;

			if (GUI.Button(r, "✖", CapComSkins.textureButton))
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

			if (GUILayout.Button("Offered", CapComSkins.tabButton, GUILayout.Width(80)))
			{
				currentList = 0;
				sortContracts();
			}

			if (GUILayout.Button("Active", CapComSkins.tabButton, GUILayout.Width(70)))
			{
				currentList = 1;
				sortContracts();
			}

			if (GUILayout.Button("Completed", CapComSkins.tabButton, GUILayout.Width(100)))
			{
				currentList = 2;
				sortContracts();
			}

			//if (GUILayout.Button("Failed", CapComSkins.tabButton, GUILayout.Width(57)))
			//{
			//	currentList = 3;
			//	sortContracts();
			//}

			GUILayout.EndHorizontal();
		}

		private void contractList(int id)
		{
			//Display sorted contract list

			cScroll = GUILayout.BeginScrollView(cScroll, false, true, GUILayout.Width(284));

			foreach (CapComContract cc in currentContracts)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(63);
				if (GUILayout.Button(cc.Name, GUILayout.Width(200), GUILayout.Height(46)))
				{
					currentContract = cc;
				}
				Rect r = GUILayoutUtility.GetLastRect();
				GUILayout.EndHorizontal();
				GUILayout.Space(-4);

				r.width = 60;
				r.x -= 60;
				r.y += 3;
				r.height = 40;
				if (GUI.Button(r, "", CapComSkins.flagButton))
					currentContract = cc;
				GUI.DrawTexture(r, cc.RootAgent.LogoScaled);
				r.width = 11;
				r.height = 40;
				r.x += 248;
				GUI.DrawTexture(r, contractPrestigeIcon(cc.Root.Prestige));
			}

			GUILayout.EndScrollView();
		}

		private void currentContractControls(int id)
		{
			if (currentContract == null)
				return;

			GUILayout.BeginHorizontal();
			if (currentContract.Root.ContractState == Contract.State.Offered)
			{
				if (GUILayout.Button("Accept", CapComSkins.tabButton, GUILayout.Width(90)))
				{
					currentContract.Root.Accept();
				}

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Decline", CapComSkins.tabButton, GUILayout.Width(90)))
				{
					currentContract.Root.Decline();
				}
			}
			else if (currentContract.Root.ContractState == Contract.State.Active)
			{
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Cancel", CapComSkins.tabButton, GUILayout.Width(90)))
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

			Rect r = new Rect(300, 50, 160, 100);

			GUI.DrawTexture(r, CapComSkins.flagBackDrop);
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
					GUILayout.Label(currentContract.Name, CapComSkins.titleText);
					GUILayout.Label("Agent:", CapComSkins.headerText, GUILayout.Width(80));
					GUILayout.Label(currentContract.RootAgent.Name, CapComSkins.titleText);
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		private void currentContractInfo(int id)
		{
			if (currentContract == null)
				return;

			GUILayout.Space(10);

			infoScroll = GUILayout.BeginScrollView(infoScroll, GUILayout.Width(500));

			//Display current contract info
			if (showBriefing)
			{
				GUILayout.Label("Briefing:", CapComSkins.headerText, GUILayout.Width(80));
				GUILayout.Label(currentContract.Root.Description, CapComSkins.briefingText);
			}
			GUILayout.Label(currentContract.Root.Synopsys, CapComSkins.synopsisText);

			if (currentContract.Root.ContractState == Contract.State.Offered)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Offer Expires: ", CapComSkins.headerText, GUILayout.Width(85));
					GUILayout.Label(KSPUtil.PrintDateDeltaCompact((int)(currentContract.Root.DateExpire - Planetarium.fetch.time), true, true), CapComSkins.parameterText, GUILayout.Width(100));
					if (currentContract.Root.DateDeadline != 0)
					{
						GUILayout.Label("Mission Deadline: ", CapComSkins.headerText, GUILayout.Width(115));
						GUILayout.Label(KSPUtil.PrintDateDeltaCompact((int)(currentContract.Root.DateDeadline - Planetarium.fetch.time), true, true), CapComSkins.parameterText, GUILayout.Width(140));
					}
					else if (currentContract.Root.TimeDeadline != 0)
					{
						GUILayout.Label("Mission Duration: ", CapComSkins.headerText, GUILayout.Width(110));
						GUILayout.Label(KSPUtil.PrintDateDeltaCompact((int)currentContract.Root.TimeDeadline, true, true), CapComSkins.parameterText, GUILayout.Width(140));
					}
				GUILayout.EndHorizontal();
			}
			else if (currentContract.Root.ContractState == Contract.State.Active)
			{
				GUILayout.BeginHorizontal();
					if (currentContract.Root.DateDeadline != 0)
					{
						GUILayout.Label("Mission Deadline: ", CapComSkins.headerText, GUILayout.Width(115));
						GUILayout.Label(KSPUtil.PrintDateDeltaCompact((int)(currentContract.Root.DateDeadline - Planetarium.fetch.time), true, true), CapComSkins.parameterText, GUILayout.Width(150));
					}
				GUILayout.EndHorizontal();
			}

			if (!string.IsNullOrEmpty(currentContract.Notes))
			{
				GUILayout.Label("Mission Notes: ", CapComSkins.headerText, GUILayout.Width(100));
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
			GUILayout.BeginHorizontal();
				GUILayout.Space(1 + cp.Level * 3);
				GUILayout.BeginVertical();
					GUILayout.Label(cp.Name, CapComSkins.parameterText);
					if (!string.IsNullOrEmpty(cp.Notes))
						GUILayout.Label(cp.Notes, CapComSkins.noteText);
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

		}

		private void rescaleHandle(int id)
		{

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

			r.x += sz.x + 16;
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
	}
}
