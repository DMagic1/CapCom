using System;
using System.Collections.Generic;
using System.Linq;
using CapCom.Framework;
using CapCom.Toolbar;
using UnityEngine;

namespace CapCom
{
	class CapComSettingsWindow : CC_MBW
	{
		private bool controlLock;
		private bool hideBriefing, hideNotes, warnDecline, warnCancel, stockToolbar;
		private bool oldToolbar;
		private bool dropdown, dup, ddown, dleft, dright, daccept, ddecline;
		private KeyCode up, down, left, right, accept, decline;
		private Rect ddRect = new Rect();
		private const string lockID = "CapCom_LockID";


		protected override void Awake()
		{
			if (CapCom.Instance != null)
			{
				Rect r = CapCom.Instance.Window.WindowRect;
				WindowRect = new Rect(50, 50, 280, 240);
			}
			else
				WindowRect = new Rect(50, 50, 280, 240);

			WindowCaption = "CapCom Settings";
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(280), GUILayout.Height(240) };
			WindowStyle = CapComSkins.newWindowStyle;
			Visible = false;
			DragEnabled = true;
			ClampToScreen = false;

			CC_SkinsLibrary.SetCurrent("CCUnitySkin");

			InputLockManager.RemoveControlLock(lockID);
		}

		protected override void Start()
		{
			hideBriefing = CapCom.Settings.hideBriefing;
			hideNotes = CapCom.Settings.hideNotes;
			warnDecline = CapCom.Settings.showDeclineWarning;
			warnCancel = CapCom.Settings.showCancelWarning;
			oldToolbar = stockToolbar = CapCom.Settings.stockToolbar;
			up = CapCom.Settings.scrollUp;
			down = CapCom.Settings.scrollDown;
			left = CapCom.Settings.listLeft;
			right = CapCom.Settings.listRight;
			accept = CapCom.Settings.accept;
			decline = CapCom.Settings.cancel;
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

			if (!dropdown)
			{
				dup = false;
				ddown = false;
				dleft = false;
				dright = false;
				daccept = false;
				ddecline = false;
			}
		}

		protected override void DrawWindow(int id)
		{
			CapCom.Settings.hideBriefing = GUILayout.Toggle(CapCom.Settings.hideBriefing, "Hide Mission Briefing Text", GUILayout.Width(170));
			CapCom.Settings.hideNotes = GUILayout.Toggle(CapCom.Settings.hideNotes, "Hide Mission Notes", GUILayout.Width(140));
			CapCom.Settings.showDeclineWarning = GUILayout.Toggle(CapCom.Settings.showDeclineWarning, "Warn on Decline", GUILayout.Width(125));
			CapCom.Settings.showCancelWarning = GUILayout.Toggle(CapCom.Settings.showCancelWarning, "Warn on Cancel", GUILayout.Width(125));
			if (ToolbarManager.ToolbarAvailable)
				stockToolbar = GUILayout.Toggle(stockToolbar, "Use Stock App Launcher", GUILayout.Width(160));

			GUILayout.BeginHorizontal();
			GUILayout.Label("Scroll Up:", GUILayout.Width(100));
			GUILayout.Label(up.ToString(), GUILayout.Width(100));
			if (!dropdown)
			{
				if (GUILayout.Button(up.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100)))
				{
					dropdown = true;
					dup = true;
				}
			}
			else
				GUILayout.Label(up.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Scroll Down:", GUILayout.Width(100));
			GUILayout.Label(down.ToString(), GUILayout.Width(100));
			if (!dropdown)
			{
				if (GUILayout.Button(down.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100)))
				{
					dropdown = true;
					ddown = true;
				}
			}
			else
				GUILayout.Label(down.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Scroll Left:", GUILayout.Width(100));
			GUILayout.Label(left.ToString(), GUILayout.Width(100));
			if (!dropdown)
			{
				if (GUILayout.Button(left.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100)))
				{
					dropdown = true;
					dleft = true;
				}
			}
			else
				GUILayout.Label(left.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Scroll Right:", GUILayout.Width(100));
			GUILayout.Label(right.ToString(), GUILayout.Width(100));
			if (!dropdown)
			{
				if (GUILayout.Button(right.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100)))
				{
					dropdown = true;
					dright = true;
				}
			}
			else
				GUILayout.Label(right.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Accept:", GUILayout.Width(100));
			GUILayout.Label(accept.ToString(), GUILayout.Width(100));
			if (!dropdown)
			{
				if (GUILayout.Button(accept.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100)))
				{
					dropdown = true;
					daccept = true;
				}
			}
			else
				GUILayout.Label(accept.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Cancel/Decline:", GUILayout.Width(100));
			GUILayout.Label(decline.ToString(), GUILayout.Width(100));
			if (!dropdown)
			{
				if (GUILayout.Button(decline.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100)))
				{
					dropdown = true;
					ddecline = true;
				}
			}
			else
				GUILayout.Label(decline.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Save", CapComSkins.tabButton, GUILayout.Width(60)))
			{
				hideBriefing = CapCom.Settings.hideBriefing;
				hideNotes = CapCom.Settings.hideNotes;
				warnDecline = CapCom.Settings.showDeclineWarning;
				warnCancel = CapCom.Settings.showCancelWarning;
				CapCom.Settings.stockToolbar = stockToolbar;
				CapCom.Settings.Save();
				CapCom.Settings.scrollUp = up;
				CapCom.Settings.scrollDown = down;
				CapCom.Settings.listLeft = left;
				CapCom.Settings.listRight = right;
				CapCom.Settings.accept = accept;
				CapCom.Settings.cancel = decline;
				Visible = false;
			}
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Cancel", CapComSkins.tabButton, GUILayout.Width(60)))
			{
				CapCom.Settings.hideBriefing = hideBriefing;
				CapCom.Settings.hideNotes = hideNotes;
				CapCom.Settings.showDeclineWarning = warnDecline;
				CapCom.Settings.showCancelWarning = warnCancel;
				up = CapCom.Settings.scrollUp;
				down = CapCom.Settings.scrollDown;
				left = CapCom.Settings.listLeft;
				right = CapCom.Settings.listRight;
				accept = CapCom.Settings.accept;
				decline = CapCom.Settings.cancel;

				stockToolbar = CapCom.Settings.stockToolbar;
				Visible = false;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			popUp(id);
		}

		protected override void DrawWindowPost(int id)
		{
			if (oldToolbar != stockToolbar)
			{
				oldToolbar = stockToolbar;
				if (stockToolbar)
				{
					CapCom.Instance.StockToolbar = gameObject.AddComponent<CC_StockToolbar>();
					if (CapCom.Instance.Toolbar != null)
					{
						Destroy(CapCom.Instance.Toolbar);
					}
				}
				else
				{
					CapCom.Instance.Toolbar = gameObject.AddComponent<CC_Toolbar>();
					if (CapCom.Instance.StockToolbar != null)
					{
						Destroy(CapCom.Instance.StockToolbar);
					}
				}
			}

			if (dropdown && Event.current.type == EventType.mouseDown && !ddRect.Contains(Event.current.mousePosition))
			{
				dropdown = false;
			}
		}

		private void popUp(int id)
		{
			if (dropdown)
			{
				ddRect = new Rect(20, 150, 260, 100);
				GUI.Box(ddRect, "", CapComSkins.dropDown);
				Rect r = new Rect(ddRect.x + 10, ddRect.y + 5, 240, 25);
				GUI.Label(r, "Press any key to reassign:", CapComSkins.reassignText);
				r.y += 30;
				GUI.Label(r, reassigning(), CapComSkins.reassignCurrentText);
				r.y += 30;
				r.x += 70;
				r.width = 80;
				if (GUI.Button(r, "Cancel", CapComSkins.warningButton))
				{
					dropdown = false;
					return;
				}

				KeyCode k = getKey();
				if (k == KeyCode.None)
					return;

				if (dup)
				{
					up = k;
					dropdown = false;
					dup = false;
				}
				else if (ddown)
				{
					down = k;
					dropdown = false;
					ddown = false;
				}
				else if (dleft)
				{
					left = k;
					dropdown = false;
					dleft = false;
				}
				else if (dright)
				{
					right = k;
					dropdown = false;
					dright = false;
				}
				else if (daccept)
				{
					accept = k;
					dropdown = false;
					daccept = false;
				}
				else if (ddecline)
				{
					decline = k;
					dropdown = false;
					ddecline = false;
				}
				else
					dropdown = false;
			}
		}

		private KeyCode getKey()
		{
			Event e = Event.current;

			if (e.isKey)
				return e.keyCode;

			return KeyCode.None;
		}

		private string reassigning()
		{
			if (dup)
				return up.ToString();
			else if (ddown)
				return down.ToString();
			else if (dleft)
				return left.ToString();
			else if (dright)
				return right.ToString();
			else if (daccept)
				return accept.ToString();
			else if (ddecline)
				return decline.ToString();
			return "";
		}

	}
}
