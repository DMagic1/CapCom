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
		private const string lockID = "CapCom_LockID";

		protected override void Awake()
		{
			LogFormatted_DebugOnly("Starting CapCom Window");
			WindowCaption = "";
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
			GUI.Label(r, CapCom.version, CapComSkins.smallText);
		}

		private void closeButton(int id)
		{
			Rect r = new Rect(WindowRect.width - 50, 1, 18, 18);
			if (GUI.Button(r, CapComSkins.settingsIcon, CapComSkins.textureButton))
			{
				//Settings Window
			}

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

			if (GUILayout.Button("Offered", CapComSkins.tabButton, GUILayout.Width(62)))
			{
				currentList = 0;
				sortContracts();
			}

			if (GUILayout.Button("Active", CapComSkins.tabButton, GUILayout.Width(57)))
			{
				currentList = 1;
				sortContracts();
			}

			if (GUILayout.Button("Completed", CapComSkins.tabButton, GUILayout.Width(82)))
			{
				currentList = 2;
				sortContracts();
			}

			if (GUILayout.Button("Failed", CapComSkins.tabButton, GUILayout.Width(57)))
			{
				currentList = 3;
				sortContracts();
			}

			GUILayout.EndHorizontal();
		}

		private void contractList(int id)
		{
			//Display sorted contract list

			cScroll = GUILayout.BeginScrollView(cScroll);

			foreach (CapComContract cc in currentContracts)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(63);
				if (GUILayout.Button(cc.Name, GUILayout.Width(200), GUILayout.Height(46)))
				{
					currentContract = cc;
				}
				Rect r = GUILayoutUtility.GetLastRect();
				r.width = 60;
				r.x -= 60;
				r.y += 3;
				r.height = 40;
				if (GUI.Button(r, "", CapComSkins.flagButton))
					currentContract = cc;
				GUI.DrawTexture(r, cc.RootAgent.LogoScaled);
				GUILayout.EndHorizontal();
				GUILayout.Space(-4);
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

			GUI.DrawTexture(r, CapComSkins.flagBackDrop);
			GUI.DrawTexture(r, currentContract.RootAgent.Logo);

			GUILayout.BeginHorizontal();
				GUILayout.Space(200);
				GUILayout.BeginVertical();
					GUILayout.Label("Contract:", CapComSkins.headerText);
					GUILayout.Label(currentContract.Name, CapComSkins.titleText);
					GUILayout.Label("Agent:", CapComSkins.headerText);
					GUILayout.Label(currentContract.RootAgent.Name, CapComSkins.titleText);
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		private void currentContractInfo(int id)
		{
			if (currentContract == null)
				return;

			infoScroll = GUILayout.BeginScrollView(infoScroll);

			//Display current contract info
			if (showBriefing)
			{
				GUILayout.Label("Briefing:", CapComSkins.headerText);
				GUILayout.Label(currentContract.Root.Description, CapComSkins.briefingText);
			}
			GUILayout.Label(currentContract.Root.Synopsys, CapComSkins.synopsisText);

			if (currentContract.Root.ContractState == Contract.State.Offered)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Offer Expires: " + KSPUtil.PrintDateDeltaCompact((int)(currentContract.Root.DateExpire - Planetarium.fetch.time), true, true), CapComSkins.headerText);
					if (currentContract.Root.DateDeadline != 0)
						GUILayout.Label("Mission Deadline: " + KSPUtil.PrintDateDeltaCompact((int)(currentContract.Root.DateDeadline - Planetarium.fetch.time), true, true), CapComSkins.headerText);
					else if (currentContract.Root.TimeDeadline != 0)
						GUILayout.Label("Mission Duration: " + KSPUtil.PrintDateDeltaCompact((int)currentContract.Root.TimeDeadline, true, true), CapComSkins.headerText);
				GUILayout.EndHorizontal();
			}
			else if (currentContract.Root.ContractState == Contract.State.Active)
			{
				if (currentContract.Root.DateDeadline != 0)
					GUILayout.Label("Mission Deadline: " + KSPUtil.PrintDateDeltaCompact((int)(currentContract.Root.DateDeadline - Planetarium.fetch.time), true, true), CapComSkins.headerText);
			}

			if (!string.IsNullOrEmpty(currentContract.Notes))
			{
				GUILayout.Label("Mission Notes: ", CapComSkins.headerText);
				GUILayout.Label(currentContract.Notes, CapComSkins.noteText);
			}

			if (currentContract.ParameterCount > 0)
			{
				GUILayout.Label("Objectives: ", CapComSkins.headerText);

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

			GUILayout.Label("Rewards: ", CapComSkins.headerText);
			if (currentContract.Root.FundsAdvance > 0)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Advance: ", CapComSkins.advance);
					Rect r = GUILayoutUtility.GetLastRect();
					r.x += 62;
					sizedContent(ref r, currentContract.FundsAdv, Currency.Funds);
				GUILayout.EndHorizontal();
			}
			if (currentContract.Root.FundsCompletion > 0 || currentContract.Root.ReputationCompletion > 0 || currentContract.Root.ScienceCompletion > 0)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Completion: ", CapComSkins.completion);
					Rect r = GUILayoutUtility.GetLastRect();
					r.x += 76;
					sizedContent(ref r, currentContract.FundsRew, Currency.Funds);
					sizedContent(ref r, currentContract.RepRew, Currency.Reputation);
					sizedContent(ref r, currentContract.SciRew, Currency.Science);
				GUILayout.EndHorizontal();
			}
			if (currentContract.Root.FundsFailure > 0 || currentContract.Root.ReputationFailure > 0)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Failure: ", CapComSkins.failure);
					Rect r = GUILayoutUtility.GetLastRect();
					r.x += 50;
					sizedContent(ref r, currentContract.FundsPen, Currency.Funds);
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
							GUILayout.Label("Completion: ", CapComSkins.completion);
							Rect r = GUILayoutUtility.GetLastRect();
							r.x += 76;
							sizedContent(ref r, cp.FundsRew, Currency.Funds);
							sizedContent(ref r, cp.RepRew, Currency.Reputation);
							sizedContent(ref r, cp.SciRew, Currency.Science);
						GUILayout.EndHorizontal();
					}
					if (cp.Param.FundsFailure > 0 || cp.Param.ReputationFailure > 0)
					{
						GUILayout.BeginHorizontal();
							GUILayout.Label("Failure: ", CapComSkins.failure);
							Rect r = GUILayoutUtility.GetLastRect();
							r.x += 50;
							sizedContent(ref r, cp.FundsPen, Currency.Funds);
							sizedContent(ref r, cp.RepPen, Currency.Reputation);
						GUILayout.EndHorizontal();
					}
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
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
	}
}
