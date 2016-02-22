#region license
/*The MIT License (MIT)
CapComWindow - Primary interface for the addon

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
using System.Collections.Generic;
using System.Linq;
using CapCom.Framework;
using Contracts;
using Contracts.Agents;
using UnityEngine;
using ContractParser;
using ProgressParser;

namespace CapCom
{
	public class CapComWindow : CC_MBW
	{
		private List<progressInterval> intervalNodes = new List<progressInterval>();
		private List<progressStandard> standardNodes = new List<progressStandard>();
		private List<progressStandard> POInodes = new List<progressStandard>();
		private List<progressBodyCollection> bodyNodes = new List<progressBodyCollection>();
		private List<List<progressStandard>> bodySubNodes = new List<List<progressStandard>>();
		private int selectedProgress;
		private Agent progressAgency;

		private List<contractContainer> activeContracts = new List<contractContainer>();
		private List<contractContainer> offeredContracts = new List<contractContainer>();
		private List<contractContainer> completedContracts = new List<contractContainer>();
		private List<contractContainer> currentContracts = new List<contractContainer>();
		private List<contractContainer> agentOfferedContracts = new List<contractContainer>();
		private List<contractContainer> agentActiveContracts = new List<contractContainer>();
		private List<contractContainer> selectedContracts = new List<contractContainer>();
		private contractContainer currentContract;
		private CapComSettingsWindow settings;

		private Vector2 cScroll, infoScroll;
		private int contractIndex;
		private int currentList;
		private bool controlLock;
		private bool showAgency;
		private bool multiSelectKeyDown;
		private bool resizing;
		private bool dropdown, warnDecline, warnCancel, sortMenu;
		private Rect ddRect;
		private float dH, dragStart;
		private int maxContracts;

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

			progressAgency = getWorldsFirst();

			InputLockManager.RemoveControlLock(lockID);
		}

		private Agent getWorldsFirst()
		{
			Agent a = AgentList.Instance.GetAgent("Kerbin World-Firsts Record-Keeping Society");

			if (a == null)
				a = AgentList.Instance.GetAgentRandom();

			return a;
		}

		protected override void Start()
		{
			WindowRect.x = CapCom.Settings.windowPosX;
			WindowRect.y = CapCom.Settings.windowPosY;
			WindowRect.yMax = WindowRect.y + CapCom.Settings.windowHeight;
			TooltipsEnabled = CapCom.Settings.tooltipsEnabled;

			maxContracts = getMaxContracts();
		}

		protected override void OnDestroy()
		{
			CapCom.Settings.windowPosX = WindowRect.x;
			CapCom.Settings.windowPosY = WindowRect.y;
			CapCom.Settings.Save();
		}

		private int getMaxContracts()
		{
			return GameVariables.Instance.GetActiveContractsLimit(ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.MissionControl));
		}

		protected override void Update()
		{
			Vector2 mousePos = Input.mousePosition;
			mousePos.y = Screen.height - mousePos.y;
			if (WindowRect.Contains(mousePos))
			{
				multiSelectKeyDown = Input.GetKey(CapCom.Settings.multiSelect);

				if (Input.GetKeyDown(CapCom.Settings.scrollUp))
				{
					if (currentContract == null)
						selectContract(currentContracts.Count - 1);
					else if (contractIndex > 0)
						selectContract(contractIndex - 1);
					else
						selectContract(currentContracts.Count - 1);
				}
				if (Input.GetKeyDown(CapCom.Settings.scrollDown))
				{
					if (currentContract == null)
						selectContract(0);
					else if (contractIndex >= currentContracts.Count - 1)
						selectContract(0);
					else
						selectContract(contractIndex + 1);
				}
				if (Input.GetKeyDown(CapCom.Settings.listRight))
				{
					if (currentList < 2)
						currentList += 1;
					else
						currentList = 0;

					sortContracts();

					selectContract(0);
				}
				if (Input.GetKeyDown(CapCom.Settings.listLeft))
				{
					if (currentList > 0)
						currentList -= 1;
					else
						currentList = 2;

					sortContracts();

					selectContract(0);
				}
				if (Input.GetKeyDown(CapCom.Settings.accept))
				{
					if (currentContract == null)
						return;

					if (HighLogic.LoadedSceneIsEditor)
						return;

					if (currentList != 0)
						return;

					acceptContract();
				}
				if (Input.GetKeyDown(CapCom.Settings.cancel))
				{
					if (currentContract == null)
						return;

					if (HighLogic.LoadedSceneIsEditor)
						return;

					if (currentList == 0)
					{
						if (!CapCom.Settings.forceDecline && !selectedContracts.Any(c => c.CanBeDeclined))
							return;

						if (CapCom.Settings.showDeclineWarning)
						{
							dropdown = true;
							warnDecline = true;
						}
						else
							declineContract();
					}
					else if (currentList == 1)
					{
						if (!CapCom.Settings.forceCancel && !selectedContracts.Any(c => c.CanBeCancelled))
							return;

						if (CapCom.Settings.showCancelWarning)
						{
							dropdown = true;
							warnCancel = true;
						}
						else
							cancelContract();
					}
				}
			}
		}

		public void refreshContracts(bool setContract)
		{
			if (CapCom.Instance == null)
				LogFormatted("CapCom Instance Not Loaded; Something Went Wrong Here...");

			activeContracts = contractParser.getActiveContracts;
			offeredContracts = contractParser.getOfferedContracts;
			completedContracts = contractParser.getCompletedContracts;

			sortContracts();

			if (setContract)
				selectContract(0);
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
					updateProgress();
					break;
			}
		}

		private void sortContracts(List<contractContainer> ccList)
		{
			switch (CapCom.Settings.sortMode)
			{
				case 0:
					ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.Root.Prestige.CompareTo(b.Root.Prestige), a.Title.CompareTo(b.Title)));
					break;
				case 1:
					ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.TotalReward.CompareTo(b.TotalReward), a.Title.CompareTo(b.Title)));
					break;
				case 2:
					ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.TotalSciReward.CompareTo(b.TotalSciReward), a.Title.CompareTo(b.Title)));
					break;
				case 3:
					ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.TotalRepReward.CompareTo(b.TotalRepReward), a.Title.CompareTo(b.Title)));
					break;
				case 4:
					ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.RootAgent.Name.CompareTo(b.RootAgent.Name), a.Title.CompareTo(b.Title)));
					break;
				case 5:
					ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.TargetPlanet.CompareTo(b.TargetPlanet), a.Title.CompareTo(b.Title)));
					break;
				case 6:
					switch (currentList)
					{
						case 0:
							ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.Expire.CompareTo(b.Expire), a.Title.CompareTo(b.Title)));
							break;
						case 1:
							ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.Deadline.CompareTo(b.Deadline), a.Title.CompareTo(b.Title)));
							break;
						case 2:
							ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.Finished.CompareTo(b.Finished), a.Title.CompareTo(b.Title)));
							break;
						default:
							break;
					}
					break;
				default:
					ccList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(CapCom.Settings.ascending, a.Root.Prestige.CompareTo(b.Root.Prestige), a.Title.CompareTo(b.Title)));
					break;
			}

			currentContracts = ccList;
		}

		public void updateProgress()
		{
			intervalNodes = progressParser.getAllIntervalNodes;
			standardNodes = progressParser.getAllStandardNodes;
			POInodes = progressParser.getAllPOINodes;
			bodyNodes = progressParser.getAllBodyNodes;

			bodySubNodes = new List<List<progressStandard>>(bodyNodes.Count);

			for (int i = 0; i < bodyNodes.Count; i++)
			{
				bodySubNodes.Add(bodyNodes[i].getAllNodes);
			}
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
					GUILayout.Space(40);
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
					else if (currentList == 3)
						currentProgressRecord(id);
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
			if (GUI.Button(r, new GUIContent(CapComSkins.settingsIcon, "Settings Menu"), CapComSkins.textureButton))
			{
				if (settings == null)
					settings = gameObject.AddComponent<CapComSettingsWindow>();
				settings.Visible = !settings.Visible;
			}

			r.x += 32;
			r.width = r.height = 22;

			if (GUI.Button(r, "X", CapComSkins.textureButton))
			{
				unlockControls();
				Visible = false;
			}
		}

		private void menuBar(int id)
		{
			Rect r = new Rect(90, 0, 360, 20);

			maxContracts = getMaxContracts();
			string s = maxContracts < 1000000 ? string.Format("[{0} Max]", maxContracts) : "";
			GUI.Label(r, string.Format("{0} Active Contracts {1}", ContractSystem.Instance.GetActiveContractCount(), s), CapComSkins.headerText);

			r = new Rect(18, 22, 48, 40);
			Rect t = new Rect(-5, 20, 66, 44);

			if (GUI.Button(t, new GUIContent("", "Sort By Difficulty"), CapCom.Settings.sortMode == 0 ? CapComSkins.toggleOnButton : CapComSkins.toggleOffButton))
			{
				CapCom.Settings.sortMode = 0;
				CapCom.Settings.Save();
				sortContracts();
			}

			GUI.DrawTexture(r, CapComSkins.sortStars);

			r.x += 62;
			r.y -= 2;
			t.x += 66;
			t.width = 69;

			if (GUI.Button(t, new GUIContent("", "Sort By Rewards"), CapCom.Settings.sortMode == 1 || CapCom.Settings.sortMode == 2 || CapCom.Settings.sortMode == 3 ? CapComSkins.toggleOnButton : CapComSkins.toggleOffButton))
			{
				dropdown = !dropdown;
				sortMenu = !sortMenu;
			}

			r.y += 18;
			r.height = 24;
			r.width = 24;
			if (CapCom.Settings.sortMode == 1)
			{
				GUI.DrawTexture(r, CapComSkins.repRed);
			}
			else
			{
				GUI.DrawTexture(r, CapComSkins.fundsGreen);
			}

			r.x += 28;

			GUI.DrawTexture(r, CapCom.Settings.sortMode == 2 ? CapComSkins.repRed : CapComSkins.science);

			r.x -= 20;
			r.y -= 16;
			r.width = 34;
			r.height = 34;
			if (CapCom.Settings.sortMode == 0 || CapCom.Settings.sortMode == 4 || CapCom.Settings.sortMode == 5 || CapCom.Settings.sortMode == 6)
			{
				r.height = 24;
				r.width = 24;
				r.y += 4;
				r.x += 4;
				GUI.DrawTexture(r, CapComSkins.repRed);
				r.y -= 4;
				r.x -= 4;
			}
			else if (CapCom.Settings.sortMode == 1)
			{
				GUI.DrawTexture(r, CapComSkins.fundsGreen);
			}
			else if (CapCom.Settings.sortMode == 2)
			{
				GUI.DrawTexture(r, CapComSkins.science);
			}
			else
			{
				GUI.DrawTexture(r, CapComSkins.repRed);
			}

			r.x += 65;
			r.width = 56;
			r.height = 42;
			t.x += 70;
			t.width = 74;

			if (GUI.Button(t, new GUIContent("", "Sort By Agency"), CapCom.Settings.sortMode == 4 ? CapComSkins.toggleOnButton : CapComSkins.toggleOffButton))
			{
				CapCom.Settings.sortMode = 4;
				CapCom.Settings.Save();
				sortContracts();
			}

			GUI.DrawTexture(r, CapComSkins.flagTex);

			GUI.DrawTexture(r, CapComSkins.currentFlag);

			r.x += 74;
			r.width = 52;
			r.height = 40;
			t.x += 76;
			t.width = 69;

			if (GUI.Button(t, new GUIContent("", "Sort By Target Body"), CapCom.Settings.sortMode == 5 ? CapComSkins.toggleOnButton : CapComSkins.toggleOffButton))
			{
				CapCom.Settings.sortMode = 5;
				CapCom.Settings.Save();
				sortContracts();
			}

			GUI.DrawTexture(r, CapComSkins.sortPlanet);

			r.x += 69;
			r.width = 36;
			r.height = 40;
			t.x += 69;
			t.width = 56;

			if (GUI.Button(t, new GUIContent("", "Sort By Time Remaining"), CapCom.Settings.sortMode == 6 ? CapComSkins.toggleOnButton : CapComSkins.toggleOffButton))
			{
				CapCom.Settings.sortMode = 6;
				CapCom.Settings.Save();
				sortContracts();
			}

			GUI.DrawTexture(r, CapComSkins.sortTime);

			r.x = 337;
			r.y = 30;
			r.width = 30;
			r.height = 25;

			if (GUI.Button(r, new GUIContent(CapCom.Settings.ascending ? CapComSkins.orderAsc : CapComSkins.orderDesc, "Ascending/Descending Order"), CapComSkins.textureButton))
			{
				CapCom.Settings.ascending = !CapCom.Settings.ascending;
				CapCom.Settings.Save();
				sortContracts();
			}
		}

		private void contractTabs(int id)
		{
			GUILayout.BeginHorizontal();
				if (dropdown)
				{
					GUILayout.Label("Offered", listStyle(0), GUILayout.Width(98), GUILayout.Height(22));
					GUILayout.Label("Active", listStyle(1), GUILayout.Width(98), GUILayout.Height(22));
					GUILayout.Label("Completed", listStyle(2), GUILayout.Width(98), GUILayout.Height(22));
					GUILayout.Label(CapComSkins.progressIconOn, listStyle(3), GUILayout.Width(32), GUILayout.Height(32));

				}
				else
				{
					if (GUILayout.Button("Offered", listStyle(0), GUILayout.Width(98), GUILayout.Height(22)))
					{
						if (currentList != 0)
						{
							currentList = 0;
							sortContracts();

							selectContract(0);
						}
					}

					if (GUILayout.Button("Active", listStyle(1), GUILayout.Width(98), GUILayout.Height(22)))
					{
						if (currentList != 1)
						{
							currentList = 1;
							sortContracts();

							selectContract(0);
						}
					}

					if (GUILayout.Button("Completed", listStyle(2), GUILayout.Width(98), GUILayout.Height(22)))
					{
						if (currentList != 2)
						{
							currentList = 2;
							sortContracts();

							selectContract(0);
						}
					}

					if (GUILayout.Button(currentList == 3 ? CapComSkins.progressIconOn : CapComSkins.progressIconOff, listStyle(3), GUILayout.Width(32), GUILayout.Height(32)))
					{
						if (currentList != 3)
						{
							currentList = 3;
							updateProgress();
						}
					}
				}
			GUILayout.EndHorizontal();
		}

		private void contractList(int id)
		{
			cScroll = GUILayout.BeginScrollView(cScroll, false, true, GUILayout.Width(356));

			if (currentList == 3)
			{
				for (int i = 0; i < intervalNodes.Count; i++)
				{
					progressInterval p = intervalNodes[i];

					if (p == null)
						continue;

					if (p.Interval <= 1)
						continue;

					drawContractTitleBar(p.Descriptor + " Records", selectedProgress == i, i, progressAgency.LogoScaled);
				}

				if (progressParser.AnyStandard)
				{
					drawContractTitleBar("Standard Records", selectedProgress == 4, 4, progressAgency.LogoScaled);
				}

				if (progressParser.AnyPOI)
				{
					drawContractTitleBar("Point Of Interest Records", selectedProgress == 5, 5, progressAgency.LogoScaled);
				}

				for (int i = 0; i < bodyNodes.Count; i++)
				{
					progressBodyCollection p = bodyNodes[i];

					if (p == null)
						continue;

					if (!p.IsReached)
						continue;

					drawContractTitleBar(p.Body.bodyName + " Records", selectedProgress == (i + 6), (i + 6), progressAgency.LogoScaled);
				}
			}
			else
			{
				for (int i = 0; i < currentContracts.Count; i++)
				{
					if (currentContracts[i] == null)
						continue;

					drawContractTitleBar(currentContracts[i].Title, selectedContracts.Contains(currentContracts[i]), i, currentContracts[i].RootAgent.LogoScaled, true, currentContracts[i].Root.Prestige);
				}
			}
			GUILayout.EndScrollView();
		}

		private void drawContractTitleBar(string title, bool selected, int index, Texture2D logo, bool contract = false, Contract.ContractPrestige prestige = Contract.ContractPrestige.Trivial)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(75);
			if (dropdown)
			{
				GUILayout.Label(title, selected ? CapComSkins.titleButtonActive : CapComSkins.titleButton, GUILayout.Width(260), GUILayout.Height(46));
			}
			else
			{
				if (GUILayout.Button(title, selected ? CapComSkins.titleButtonActive : CapComSkins.titleButton, GUILayout.Width(260), GUILayout.Height(46)))
				{
					if (contract)
					{
						selectContract(index, multiSelectKeyDown);
						showAgency = false;
					}
					else
					{
						selectProgress(index);
					}
				}
			}
			Rect r = GUILayoutUtility.GetLastRect();
			GUILayout.EndHorizontal();
			GUILayout.Space(-4);

			r.width = 69;
			r.x -= 69;
			r.height = 46;
			if (!dropdown)
			{
				if (GUI.Button(r, "", CapComSkins.iconButton))
				{
					if (contract)
					{
						selectContract(index, multiSelectKeyDown);
						showAgency = false;
					}
					else
					{
						selectProgress(index);
					}
				}
			}
			GUI.DrawTexture(r, selected ? CapComSkins.titleButtonOnLeft : CapComSkins.titleButtonOffLeft);
			r.x += 10;
			r.y += 3;
			r.width = 58;
			r.height = 40;
			if (logo != null)
				GUI.DrawTexture(r, logo);
			r.width = 11;
			r.height = 40;
			r.x += 307;
			GUI.DrawTexture(r, contractPrestigeIcon(prestige));
		}

		private void currentContractControls(int id)
		{
			if (currentList == 3)
				return;

			if (HighLogic.LoadedSceneIsEditor)
				return;

			if (currentContract == null)
				return;

			if (currentContract.Root.ContractState == Contract.State.Offered)
			{
				Rect r = new Rect(WindowRect.width - 60, 35, 47, 49);

				bool active = !CapCom.Settings.activeLimit || ContractSystem.Instance.GetActiveContractCount() < maxContracts;

				if (GUI.Button(r, new GUIContent("", "Accept"), active ? CapComSkins.acceptButton : CapComSkins.acceptButtonGreyed))
				{
					if (active)
						acceptContract();
				}

				r.y += 65;

				if (GUI.Button(r, new GUIContent("", "Decline"), CapCom.Settings.forceDecline || selectedContracts.Any(c => c.CanBeDeclined) ? CapComSkins.declineButton : CapComSkins.declineButtonGreyed))
				{
					if (CapCom.Settings.forceDecline || selectedContracts.Any(c => c.CanBeDeclined))
					{
						if (CapCom.Settings.showDeclineWarning)
						{
							dropdown = true;
							warnDecline = true;
						}
						else
							declineContract();
					}
				}
			}
			else if (currentContract.Root.ContractState == Contract.State.Active)
			{
				Rect r = new Rect(WindowRect.width - 60, 100, 47, 49);

				if (GUI.Button(r, new GUIContent("", "Cancel"), CapCom.Settings.forceCancel || selectedContracts.Any(c => c.CanBeCancelled) ? CapComSkins.cancelButton : CapComSkins.cancelButtonGreyed))
				{
					if (CapCom.Settings.forceCancel || selectedContracts.Any(c => c.CanBeCancelled))
					{
						if (CapCom.Settings.showCancelWarning)
						{
							dropdown = true;
							warnCancel = true;
						}
						else
							cancelContract();
					}
				}
			}
		}

		private void currentContractHeader(int id)
		{
			if (currentContract == null && currentList != 3)
				return;

			Agent a = currentList == 3 ? progressAgency : currentContract.RootAgent;
			string title = currentList == 3 ? progressTitle(selectedProgress) : currentContract.Title;

			Rect r = new Rect(370, 33, 160, 106);

			if (GUI.Button(r, "", CapComSkins.iconButton))
			{
				showAgency = !showAgency;
				agentOfferedContracts.Clear();
				agentActiveContracts.Clear();
				foreach (contractContainer c in offeredContracts)
				{
					if (c.RootAgent == a)
					{
						if (c == currentContract)
							continue;

						agentOfferedContracts.Add(c);
					}
				}
				foreach (contractContainer c in activeContracts)
				{
					if (c.RootAgent == a)
					{
						if (c == currentContract)
							continue;

						agentActiveContracts.Add(c);
					}
				}
			}
			GUI.DrawTexture(r, CapComSkins.flagTex);
			r.x += 4;
			r.y += 5;
			r.width = 152;
			r.height = 95;
			if (a.Logo != null)
				GUI.DrawTexture(r, a.Logo);

			GUILayout.BeginHorizontal();
				GUILayout.Space(180);
				GUILayout.BeginVertical();
					GUILayout.Label(currentList == 3 ? "Record" : "Contract:", CapComSkins.headerText, GUILayout.Width(80));
					r = GUILayoutUtility.GetLastRect();
					r.x += 100;
					r.width = 60;
					r.height = 16;
					GUI.DrawTexture(r, contractPrestigeIcon(currentContract.Root.Prestige, false));
					GUILayout.Label(title, CapComSkins.titleText, GUILayout.Width(260));
					GUILayout.Label("Agent:", CapComSkins.headerText, GUILayout.Width(80));
					GUILayout.Label(a.Name, CapComSkins.titleText, GUILayout.Width(260));
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		private void currentProgressRecord(int id)
		{
			GUILayout.Space(10);

			infoScroll = GUILayout.BeginScrollView(infoScroll, GUILayout.Width(500));

			if (selectedProgress >= 0 && selectedProgress <= 3)
			{
				drawIntervalNode(selectedProgress);
			}
			else if (selectedProgress == 4)
			{
				drawStandardNodes();
			}
			else if (selectedProgress == 5)
			{
				drawPOINodes();
			}
			else if (selectedProgress >= 6)
			{
				drawBodyNode(selectedProgress - 6);
			}

			GUILayout.EndScrollView();
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
					r.x += 100;
					r.y += 2;
					r.width = 16;
					r.height = 16;
					if (GUI.Button(r, currentContract.ShowNote ? CapComSkins.notesMinusIcon : CapComSkins.notesPlusIcon, CapComSkins.textureButton))
						currentContract.ShowNote = !currentContract.ShowNote;
				}

				if (!CapCom.Settings.hideNotes || currentContract.ShowNote)
					GUILayout.Label(currentContract.Notes, CapComSkins.noteText);
			}

			if (currentContract.ParameterCount > 0)
			{
				GUILayout.Label("Objectives: ", CapComSkins.headerText, GUILayout.Width(100));

				for (int i = 0; i < currentContract.ParameterCount; i++)
				{
					parameterContainer cp = currentContract.getParameter(i);
					if (cp == null)
						continue;
					if (cp.Level > 4)
						continue;

					if (string.IsNullOrEmpty(cp.Title))
						continue;

					drawParameter(cp);
				}
			}

			GUILayout.Label("Rewards: ", CapComSkins.headerText, GUILayout.Width(80));

			sizedContent(currentContract.FundsAdvString, "", "", TransactionReasons.ContractAdvance);

			sizedContent(currentContract.FundsRewString, currentContract.SciRewString, currentContract.RepRewString, TransactionReasons.ContractReward);

			sizedContent(currentContract.FundsPenString, "", currentContract.RepPenString, TransactionReasons.ContractPenalty);

			if (currentContract.Root.ContractState == Contract.State.Offered && currentContract.DecPen > 0)
				sizedContent("", "", "- " + currentContract.DecPen.ToString("N0"), TransactionReasons.ContractDecline);

			GUILayout.EndScrollView();
		}

		private void drawIntervalNode(int i)
		{
			progressInterval interval = intervalNodes[i];

			if (interval == null)
				return;

			if (interval.Interval <= 1)
				return;

			GUILayout.Label(progressTitle(selectedProgress) + " Records:", CapComSkins.headerText, GUILayout.Width(200));

			for (int j = 1; j < interval.Interval; j++)
			{
				drawIntervalRecords(interval, j);
			}
		}

		private void drawStandardNodes()
		{
			GUILayout.Label(progressTitle(selectedProgress) + " Records:", CapComSkins.headerText, GUILayout.Width(200));

			for (int i = 0; i < standardNodes.Count; i++)
			{
				progressStandard standard = standardNodes[i];

				if (standard == null)
					continue;

				if (!standard.IsComplete)
					continue;

				drawStandardRecords(standard);
			}
		}

		private void drawPOINodes()
		{
			GUILayout.Label(progressTitle(selectedProgress) + " Records:", CapComSkins.headerText, GUILayout.Width(200));

			for (int i = 0; i < POInodes.Count; i++)
			{
				progressStandard POI = POInodes[i];

				if (POI == null)
					continue;

				if (!POI.IsComplete)
					continue;

				drawStandardRecords(POI);
			}
		}

		private void drawBodyNode(int i)
		{
			progressBodyCollection p = bodyNodes[i];

			if (p == null)
				return;

			if (!p.IsReached)
				return;

			GUILayout.Label(p.Body.bodyName + " Records:", CapComSkins.headerText, GUILayout.Width(200));

			for (int j = 0; j < bodySubNodes[i].Count; j++)
			{
				progressStandard BodyNode = bodySubNodes[i][j];

				if (BodyNode == null)
					continue;

				if (!BodyNode.IsComplete)
					continue;

				drawStandardRecords(BodyNode, BodyNode.Body.theName);
			}
		}

		private void drawIntervalRecords(progressInterval p, int index)
		{
			GUILayout.Label(string.Format(p.Descriptor + " Record {0}: {1}", index, p.getRecord(index)), CapComSkins.parameterText);

			GUILayout.Label("Rewards: ", CapComSkins.headerText, GUILayout.Width(80));

			sizedContent(p.getFundsString(index), p.getScienceString(index), p.getRepString(index), TransactionReasons.ContractReward);
		}

		private void drawStandardRecords(progressStandard p, string s = "")
		{
			GUILayout.Label(string.Format(p.Descriptor, s), CapComSkins.parameterText);

			if (!string.IsNullOrEmpty(p.Note))
			{
				GUILayout.Space(-7);
				GUILayout.Label(string.Format(p.Note, p.NoteReference, p.KSPDateString), CapComSkins.noteText);
			}

			GUILayout.Label("Rewards: ", CapComSkins.headerText, GUILayout.Width(80));

			sizedContent(p.FundsRewardString, p.SciRewardString, p.RepRewardString, TransactionReasons.ContractReward);
		}

		private void drawParameter(parameterContainer cp)
		{
			bool notes = !string.IsNullOrEmpty(cp.Notes());

			GUILayout.BeginHorizontal();
				if (notes && CapCom.Settings.hideNotes)
					GUILayout.Space(30 + cp.Level * 8);
				else
					GUILayout.Space(16 + cp.Level * 8);

				GUILayout.BeginVertical();
					GUILayout.Label(cp.Title, cp.Level == 0 ? CapComSkins.parameterText : CapComSkins.subParameterText);
					Rect b = GUILayoutUtility.GetLastRect();

					if (notes && CapCom.Settings.hideNotes)
					{
						b.x -= 28;
						b.height = 16;
						b.width = 16;
						if (GUI.Button(b, cp.ShowNote ? CapComSkins.notesMinusIcon :CapComSkins.notesPlusIcon, CapComSkins.textureButton))
							cp.ShowNote = !cp.ShowNote;

						b.y += 4;
						b.x += 16;
						b.height = 10;
						b.width = 10;
						GUI.DrawTexture(b, parameterStateIcon(cp.CParam));
					}
					else
					{
						b.x -= 14;
						b.y += 4;
						b.height = 12;
						b.width = 12;
						GUI.DrawTexture(b, parameterStateIcon(cp.CParam));
					}

					if (notes)
					{
						if (!CapCom.Settings.hideNotes || cp.ShowNote)
							GUILayout.Label(cp.Notes(true), CapComSkins.noteText);
					}

					sizedContent(cp.FundsRewString, cp.SciRewString, cp.RepRewString, TransactionReasons.ContractReward);

					sizedContent(cp.FundsPenString, "", cp.RepPenString, TransactionReasons.ContractPenalty);
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			for (int j = 0; j < cp.ParameterCount; j++)
			{
				parameterContainer subP = cp.getParameter(j);
				if (subP == null)
					continue;
				if (subP.Level > 4)
					continue;

				if (string.IsNullOrEmpty(subP.Title))
					continue;

				drawParameter(subP);
			}
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
				foreach (contractContainer c in agentOfferedContracts)
				{
					GUILayout.BeginHorizontal();
						GUILayout.Space(20);
						GUILayout.Label(c.Title, CapComSkins.agencyContractText);
					GUILayout.EndHorizontal();
				}
			}

			if (agentActiveContracts.Count > 0)
			{
				GUILayout.Label("Currently Active Contracts:", CapComSkins.headerText);
				foreach (contractContainer c in agentActiveContracts)
				{
					GUILayout.BeginHorizontal();
						GUILayout.Space(20);
						GUILayout.Label(c.Title, CapComSkins.agencyContractText);
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
					ddRect = new Rect(88, 68, 106, 100);
					GUI.Box(ddRect, "");
					Rect r = new Rect(ddRect.x + 5, ddRect.y + 5, 100, 20);
					GUI.Label(r, "Sort Options:", CapComSkins.titleText);
					for (int i = 1; i < 4; i++)
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
						Rect t = new Rect(r.x + 2, r.y + 2, 16, 16);
						GUI.DrawTexture(t, currencyIcon((Currency)(i - 1)));
					}
				}
				else if (warnCancel)
				{
					ddRect = new Rect(WindowRect.width - 230, 95, 160, 70);
					GUI.Box(ddRect, "");
					Rect r = new Rect(ddRect.x + 10, ddRect.y + 5, 140, 30);
					GUI.Label(r, "Cancel contract?", CapComSkins.warningText);
					r = new Rect(ddRect.x + 40, ddRect.y + 30, 80, 30);
					if (GUI.Button(r, "Confirm", CapComSkins.warningButton))
					{
						cancelContract();
						warnCancel = false;
						dropdown = false;
					}
				}
				else if (warnDecline)
				{
					ddRect = new Rect(WindowRect.width - 230, 95, 160, 70);
					GUI.Box(ddRect, "");
					Rect r = new Rect(ddRect.x + 10, ddRect.y + 5, 140, 30);
					GUI.Label(r, "Decline contract?", CapComSkins.warningText);
					r = new Rect(ddRect.x + 40, ddRect.y + 30, 80, 30);
					if (GUI.Button(r, "Confirm", CapComSkins.warningButton))
					{
						declineContract();
						warnDecline = false;
						dropdown = false;
					}
				}
				else
					dropdown = false;
			}
		}

		private void acceptContract()
		{
			int count = ContractSystem.Instance.GetActiveContractCount();
			maxContracts = getMaxContracts();

			foreach (contractContainer cc in selectedContracts)
			{
				if (!CapCom.Settings.activeLimit || count < maxContracts)
				{
					cc.Root.Accept();
					count++;
				}
			}

			if (selectedContracts.Count <= 1)
			{
				if (currentContract == null)
					selectContract(0);
				else if (contractIndex >= currentContracts.Count - 1)
					selectContract(0);
				else
					selectContract(contractIndex);
			}
			else
				selectContract(0);
		}

		private void cancelContract()
		{
			bool any = false;
			foreach (contractContainer cc in selectedContracts)
			{
				if (cc.Root.CanBeCancelled() || CapCom.Settings.forceCancel)
				{
					cc.Root.Cancel();
					any = true;
				}
			}

			if (!any)
				return;

			if (selectedContracts.Count <= 1)
			{
				if (currentContract == null)
					selectContract(0);
				else if (contractIndex >= currentContracts.Count - 1)
					selectContract(0);
				else
					selectContract(contractIndex);
			}
			else
				selectContract(0);
		}

		private void declineContract()
		{
			bool any = false;
			foreach (contractContainer cc in selectedContracts)
			{
				if (cc.Root.CanBeDeclined() || CapCom.Settings.forceDecline)
				{
					cc.Root.Decline();
					any = true;
				}
			}

			if (!any)
				return;

			if (selectedContracts.Count <= 1)
			{
				if (currentContract == null)
					selectContract(0);
				else if (contractIndex >= currentContracts.Count - 1)
					selectContract(0);
				else
					selectContract(contractIndex);
			}
			else
				selectContract(0);
		}

		private void selectContract(int i, bool selectMany = false)
		{
			contractIndex = i;

			if (!selectMany)
				selectedContracts.Clear();

			if (currentContracts.Count > 0)
				currentContract = currentContracts[contractIndex];
			else
				currentContract = null;

			if (selectMany && selectedContracts.Contains(currentContract))
				selectedContracts.Remove(currentContract);
			else
				selectedContracts.Add(currentContract);

			showAgency = false;
		}

		private void selectProgress(int i)
		{
			selectedProgress = i;
		}

		private void sizedContent(string funds, string sci, string rep, TransactionReasons type)
		{
			bool b1 = string.IsNullOrEmpty(funds);
			bool b2 = string.IsNullOrEmpty(sci);
			bool b3 = string.IsNullOrEmpty(rep);

			if (b1 && b2 && b3)
				return;

			Rect r = new Rect();

			rewardLabel(type, ref r);

			GUIStyle s;
			Vector2 sz = new Vector2();
			if (!b1)
			{
				r.width = 16;
				r.height = 16;
				GUI.DrawTexture(r, currencyIcon(Currency.Funds));
				s = currencyStyle(Currency.Funds);
				sz = s.CalcSize(new GUIContent(funds));

				r.x += 20;
				r.width = sz.x;
				r.height = sz.y;

				GUI.Label(r, funds, s);

				r.x += sz.x + 14;
			}

			if (!b2)
			{
				r.width = 16;
				r.height = 16;
				GUI.DrawTexture(r, currencyIcon(Currency.Science));
				s = currencyStyle(Currency.Science);
				sz = s.CalcSize(new GUIContent(sci));

				r.x += 20;
				r.width = sz.x;
				r.height = sz.y;

				GUI.Label(r, sci, s);

				r.x += sz.x + 14;
			}

			if (!b3)
			{
				r.width = 16;
				r.height = 16;
				GUI.DrawTexture(r, currencyIcon(Currency.Reputation));
				s = currencyStyle(Currency.Reputation);
				sz = s.CalcSize(new GUIContent(rep));

				r.x += 20;
				r.width = sz.x;
				r.height = sz.y;

				GUI.Label(r, rep, s);
			}
		}

		private void rewardLabel(TransactionReasons t, ref Rect r)
		{
			float right = 0;
			switch (t)
			{
				case TransactionReasons.ContractAdvance:
					{
						GUILayout.Label("Advance: ", CapComSkins.advance, GUILayout.Width(80));
						right = 62;
						break;
					}
				case TransactionReasons.ContractPenalty:
					{
						GUILayout.Label("Failure: ", CapComSkins.failure, GUILayout.Width(80));
						right = 50;
						break;
					}
				case TransactionReasons.ContractReward:
					{
						GUILayout.Label("Completion: ", CapComSkins.completion, GUILayout.Width(80));
						right = 76;
						break;
					}
				case TransactionReasons.ContractDecline:
					{
						GUILayout.Label("Decline: ", CapComSkins.failure, GUILayout.Width(80));
						right = 50;
						break;
					}
				default:
					return;
			}
			r = GUILayoutUtility.GetLastRect();
			r.x += right;
		}

		private GUIStyle listStyle(int i)
		{
			if (i == 3)
				return CapComSkins.textureButton;
			else
				return i == currentList ? CapComSkins.tabButton : CapComSkins.tabButtonInactive;
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

		private string progressTitle(int i)
		{
			switch (i)
			{
				case 0:
					return "Highest Altitude";
				case 1:
					return "Greatest Velocity";
				case 2:
					return "Greatest Distance";
				case 3:
					return "Greatest Depth";
				case 4:
					return "Standard Records";
				case 5:
					return "Points Of Interest";
				default:
					progressBodyCollection p = bodyNodes[i - 6];

					if (p == null)
						return "";

					return p.Body.theName + " Progress";
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
				case 5:
					return "Target Body";
				case 6:
					return "Time";
				default:
					return "";
			}
		}

	}
}
