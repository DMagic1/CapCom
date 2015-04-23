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
		private bool hideBriefing, hideNotes, stockToolbar;
		private bool oldToolbar;
		private const string lockID = "CapCom_LockID";


		protected override void Awake()
		{
			if (CapCom.Instance != null)
			{
				Rect r = CapCom.Instance.Window.WindowRect;
				WindowRect = new Rect(50, 50, 280, 140);
			}
			else
				WindowRect = new Rect(50, 50, 280, 140);

			WindowCaption = "CapCom Settings";
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(280), GUILayout.Height(140) };
			WindowStyle = CapComSkins.newWindowStyle;
			Visible = false;
			DragEnabled = true;
			ClampToScreen = false;
			TooltipMouseOffset = new Vector2d(-10, -25);

			CC_SkinsLibrary.SetCurrent("CCUnitySkin");

			InputLockManager.RemoveControlLock(lockID);
		}

		protected override void Start()
		{
			hideBriefing = CapCom.Instance.Settings.hideBriefing;
			hideNotes = CapCom.Instance.Settings.hideNotes;
			oldToolbar = stockToolbar = CapCom.Instance.Settings.stockToolbar;
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
			CapCom.Instance.Settings.hideBriefing = GUILayout.Toggle(CapCom.Instance.Settings.hideBriefing, "Hide Mission Briefing Text");
			CapCom.Instance.Settings.hideNotes = GUILayout.Toggle(CapCom.Instance.Settings.hideNotes, "Hide Mission Notes");
			if (ToolbarManager.ToolbarAvailable)
				stockToolbar = GUILayout.Toggle(stockToolbar, "Use Stock App Launcher");

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Save"))
			{
				hideBriefing = CapCom.Instance.Settings.hideBriefing;
				hideNotes = CapCom.Instance.Settings.hideNotes;
				CapCom.Instance.Settings.stockToolbar = stockToolbar;
				CapCom.Instance.Settings.Save();
				Visible = false;
			}

			if (GUILayout.Button("Cancel"))
			{
				CapCom.Instance.Settings.hideBriefing = hideBriefing;
				CapCom.Instance.Settings.hideNotes = hideNotes;
				stockToolbar = CapCom.Instance.Settings.stockToolbar;
				Visible = false;
			}
			GUILayout.EndHorizontal();
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
		}
	}
}
