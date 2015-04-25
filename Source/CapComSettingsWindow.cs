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
		private string up, down, left, right, accept, decline;
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
			hideBriefing = CapCom.Instance.Settings.hideBriefing;
			hideNotes = CapCom.Instance.Settings.hideNotes;
			warnDecline = CapCom.Instance.Settings.showDeclineWarning;
			warnCancel = CapCom.Instance.Settings.showCancelWarning;
			oldToolbar = stockToolbar = CapCom.Instance.Settings.stockToolbar;
			up = CapCom.Instance.Settings.scrollUp.ToString();
			down = CapCom.Instance.Settings.scrollDown.ToString();
			left = CapCom.Instance.Settings.listLeft.ToString();
			right = CapCom.Instance.Settings.listRight.ToString();
			accept = CapCom.Instance.Settings.accept.ToString();
			decline = CapCom.Instance.Settings.cancel.ToString();
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
			CapCom.Instance.Settings.hideBriefing = GUILayout.Toggle(CapCom.Instance.Settings.hideBriefing, "Hide Mission Briefing Text", GUILayout.Width(170));
			CapCom.Instance.Settings.hideNotes = GUILayout.Toggle(CapCom.Instance.Settings.hideNotes, "Hide Mission Notes", GUILayout.Width(140));
			CapCom.Instance.Settings.showDeclineWarning = GUILayout.Toggle(CapCom.Instance.Settings.showDeclineWarning, "Warn on Decline", GUILayout.Width(125));
			CapCom.Instance.Settings.showCancelWarning = GUILayout.Toggle(CapCom.Instance.Settings.showCancelWarning, "Warn on Cancel", GUILayout.Width(125));
			if (ToolbarManager.ToolbarAvailable)
				stockToolbar = GUILayout.Toggle(stockToolbar, "Use Stock App Launcher", GUILayout.Width(160));

			GUILayout.BeginHorizontal();
			GUILayout.Label("Scroll Up:", GUILayout.Width(100));
			GUILayout.Label(CapCom.Instance.Settings.scrollUp.ToString(), GUILayout.Width(100));
			if (!dropdown)
			{
				if (GUILayout.Button(CapCom.Instance.Settings.scrollUp.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100)))
				{
					dropdown = true;
					dup = true;
				}
			}
			else
				GUILayout.Label(CapCom.Instance.Settings.scrollUp.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Scroll Down:", GUILayout.Width(100));
			GUILayout.Label(CapCom.Instance.Settings.scrollDown.ToString(), GUILayout.Width(100));
			if (!dropdown)
			{
				if (GUILayout.Button(CapCom.Instance.Settings.scrollDown.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100)))
				{
					dropdown = true;
					ddown = true;
				}
			}
			else
				GUILayout.Label(CapCom.Instance.Settings.scrollDown.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Scroll Left:", GUILayout.Width(100));
			GUILayout.Label(CapCom.Instance.Settings.listLeft.ToString(), GUILayout.Width(100));
			if (!dropdown)
			{
				if (GUILayout.Button(CapCom.Instance.Settings.listLeft.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100)))
				{
					dropdown = true;
					dleft = true;
				}
			}
			else
				GUILayout.Label(CapCom.Instance.Settings.listLeft.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Scroll Right:", GUILayout.Width(100));
			GUILayout.Label(CapCom.Instance.Settings.listRight.ToString(), GUILayout.Width(100));
			if (!dropdown)
			{
				if (GUILayout.Button(CapCom.Instance.Settings.listRight.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100)))
				{
					dropdown = true;
					dright = true;
				}
			}
			else
				GUILayout.Label(CapCom.Instance.Settings.listRight.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Accept:", GUILayout.Width(100));
			GUILayout.Label(CapCom.Instance.Settings.accept.ToString(), GUILayout.Width(100));
			if (!dropdown)
			{
				if (GUILayout.Button(CapCom.Instance.Settings.accept.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100)))
				{
					dropdown = true;
					daccept = true;
				}
			}
			else
				GUILayout.Label(CapCom.Instance.Settings.accept.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Cancel/Decline:", GUILayout.Width(100));
			GUILayout.Label(CapCom.Instance.Settings.cancel.ToString(), GUILayout.Width(100));
			if (!dropdown)
			{
				if (GUILayout.Button(CapCom.Instance.Settings.cancel.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100)))
				{
					dropdown = true;
					ddecline = true;
				}
			}
			else
				GUILayout.Label(CapCom.Instance.Settings.cancel.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Save", GUILayout.Width(60)))
			{
				hideBriefing = CapCom.Instance.Settings.hideBriefing;
				hideNotes = CapCom.Instance.Settings.hideNotes;
				warnDecline = CapCom.Instance.Settings.showDeclineWarning;
				warnCancel = CapCom.Instance.Settings.showCancelWarning;
				CapCom.Instance.Settings.stockToolbar = stockToolbar;
				CapCom.Instance.Settings.Save();
				up = CapCom.Instance.Settings.scrollUp.ToString();
				down = CapCom.Instance.Settings.scrollDown.ToString();
				left = CapCom.Instance.Settings.listLeft.ToString();
				right = CapCom.Instance.Settings.listRight.ToString();
				accept = CapCom.Instance.Settings.accept.ToString();
				decline = CapCom.Instance.Settings.cancel.ToString();
				Visible = false;
			}
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Cancel", GUILayout.Width(60)))
			{
				CapCom.Instance.Settings.hideBriefing = hideBriefing;
				CapCom.Instance.Settings.hideNotes = hideNotes;
				CapCom.Instance.Settings.showDeclineWarning = warnDecline;
				CapCom.Instance.Settings.showCancelWarning = warnCancel;
				CapCom.Instance.Settings.scrollUp = (KeyCode)System.Enum.Parse(typeof(KeyCode), up);
				CapCom.Instance.Settings.scrollDown = (KeyCode)System.Enum.Parse(typeof(KeyCode), down);
				CapCom.Instance.Settings.listLeft = (KeyCode)System.Enum.Parse(typeof(KeyCode), left);
				CapCom.Instance.Settings.listRight = (KeyCode)System.Enum.Parse(typeof(KeyCode), right);
				CapCom.Instance.Settings.accept = (KeyCode)System.Enum.Parse(typeof(KeyCode), accept);
				CapCom.Instance.Settings.cancel = (KeyCode)System.Enum.Parse(typeof(KeyCode), decline);
				stockToolbar = CapCom.Instance.Settings.stockToolbar;
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
				ddRect = new Rect(20, 150, 100, 80);
				GUI.Box(ddRect, "", CapComSkins.dropDown);

				KeyCode k = getKey();
				if (k == KeyCode.None)
					return;

				if (dup)
				{
					CapCom.Instance.Settings.scrollUp = k;
					dropdown = false;
					dup = false;
				}
				else if (ddown)
				{
					CapCom.Instance.Settings.scrollDown = k;
					dropdown = false;
					ddown = false;
				}
				else if (dleft)
				{
					CapCom.Instance.Settings.listLeft = k;
					dropdown = false;
					dleft = false;
				}
				else if (dright)
				{
					CapCom.Instance.Settings.listRight = k;
					dropdown = false;
					dright = false;
				}
				else if (daccept)
				{
					CapCom.Instance.Settings.accept = k;
					dropdown = false;
					daccept = false;
				}
				else if (ddecline)
				{
					CapCom.Instance.Settings.cancel = k;
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

	}
}
