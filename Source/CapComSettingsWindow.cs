#region license
/*The MIT License (MIT)
CapComSettingsWindow - In-game window to control configuration options

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
using CapCom.Toolbar;
using UnityEngine;

namespace CapCom
{
	class CapComSettingsWindow : CC_MBW
	{
		private bool controlLock;
		private bool hideBriefing, hideNotes, warnDecline, warnCancel, stockToolbar, tooltips, style;
		private bool oldToolbar, oldTooltips, oldStyle;
		private bool dropdown, dup, ddown, dleft, dright, daccept, ddecline, dmulti;
		private KeyCode up, down, left, right, accept, decline, multiSelect;
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

			InputLockManager.RemoveControlLock(lockID);
		}

		protected override void Start()
		{
			hideBriefing = CapCom.Settings.hideBriefing;
			hideNotes = CapCom.Settings.hideNotes;
			warnDecline = CapCom.Settings.showDeclineWarning;
			warnCancel = CapCom.Settings.showCancelWarning;
			oldToolbar = stockToolbar = CapCom.Settings.stockToolbar;
			oldTooltips = tooltips = CapCom.Settings.tooltipsEnabled;
			//oldStyle = style = CapCom.Settings.useKSPStyle;
			up = CapCom.Settings.scrollUp;
			down = CapCom.Settings.scrollDown;
			left = CapCom.Settings.listLeft;
			right = CapCom.Settings.listRight;
			accept = CapCom.Settings.accept;
			decline = CapCom.Settings.cancel;
			multiSelect = CapCom.Settings.multiSelect;
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
				else if (WindowRect.Contains(mousePos) && dropdown && !controlLock)
				{
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, lockID);
					controlLock = true;
				}
				else if ((!WindowRect.Contains(mousePos) || !dropdown) && controlLock)
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
				dup = false;
				ddown = false;
				dleft = false;
				dright = false;
				daccept = false;
				ddecline = false;
				dmulti = false;
			}
		}

		protected override void DrawWindow(int id)
		{
			CapCom.Settings.hideBriefing = GUILayout.Toggle(CapCom.Settings.hideBriefing, "Hide Mission Briefing Text", GUILayout.Width(170));
			CapCom.Settings.hideNotes = GUILayout.Toggle(CapCom.Settings.hideNotes, "Hide Mission Notes", GUILayout.Width(140));
			CapCom.Settings.showDeclineWarning = GUILayout.Toggle(CapCom.Settings.showDeclineWarning, "Warn on Decline", GUILayout.Width(125));
			CapCom.Settings.showCancelWarning = GUILayout.Toggle(CapCom.Settings.showCancelWarning, "Warn on Cancel", GUILayout.Width(125));
			tooltips = GUILayout.Toggle(tooltips, "Toolips", GUILayout.Width(70));
			//style = GUILayout.Toggle(style, "Use KSP Style", GUILayout.Width(120));
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
			GUILayout.Label("Multi Select:", GUILayout.Width(100));
			GUILayout.Label(multiSelect.ToString(), GUILayout.Width(100));
			if (!dropdown)
			{
				if (GUILayout.Button(multiSelect.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100)))
				{
					dropdown = true;
					dmulti = true;
				}
			}
			else
				GUILayout.Label(multiSelect.ToString(), CapComSkins.keycodeButton, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Save", CapComSkins.warningButton, GUILayout.Width(60)))
			{
				hideBriefing = CapCom.Settings.hideBriefing;
				hideNotes = CapCom.Settings.hideNotes;
				warnDecline = CapCom.Settings.showDeclineWarning;
				warnCancel = CapCom.Settings.showCancelWarning;
				CapCom.Settings.tooltipsEnabled = tooltips;
				//CapCom.Settings.useKSPStyle = style;
				CapCom.Settings.stockToolbar = stockToolbar;
				CapCom.Settings.Save();
				CapCom.Settings.scrollUp = up;
				CapCom.Settings.scrollDown = down;
				CapCom.Settings.listLeft = left;
				CapCom.Settings.listRight = right;
				CapCom.Settings.accept = accept;
				CapCom.Settings.cancel = decline;
				CapCom.Settings.multiSelect = multiSelect;
				Visible = false;
			}
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Cancel", CapComSkins.warningButton, GUILayout.Width(60)))
			{
				CapCom.Settings.hideBriefing = hideBriefing;
				CapCom.Settings.hideNotes = hideNotes;
				CapCom.Settings.showDeclineWarning = warnDecline;
				CapCom.Settings.showCancelWarning = warnCancel;
				tooltips = CapCom.Settings.tooltipsEnabled;
				//style = CapCom.Settings.useKSPStyle;
				up = CapCom.Settings.scrollUp;
				down = CapCom.Settings.scrollDown;
				left = CapCom.Settings.listLeft;
				right = CapCom.Settings.listRight;
				accept = CapCom.Settings.accept;
				decline = CapCom.Settings.cancel;
				multiSelect = CapCom.Settings.multiSelect;

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

			if (oldTooltips != tooltips)
			{
				oldTooltips = tooltips;
				CapCom.Instance.Window.TooltipsEnabled = tooltips;
			}

			//if (oldStyle != style)
			//{
			//	oldStyle = style;
			//	if (style)
			//	{
			//		CapComSkins.initializeKSPSkins();
			//		CC_SkinsLibrary.SetCurrent("CCKSPSkin");
			//	}
			//	else
			//	{
			//		CapComSkins.initializeUnitySkins();
			//		CC_SkinsLibrary.SetCurrent("CCUnitySkin");
			//	}

			//	WindowStyle = CapComSkins.newWindowStyle;
			//	CapCom.Instance.Window.WindowStyle = CapComSkins.newWindowStyle;
			//}

			if (dropdown && Event.current.type == EventType.mouseDown && !ddRect.Contains(Event.current.mousePosition))
			{
				resetKey();
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
				r.x += 20;
				r.width = 80;
				if (GUI.Button(r, "Accept", CapComSkins.warningButton))
				{
					dropdown = false;
					return;
				}
				r.x += 120;
				if (GUI.Button(r, "Cancel", CapComSkins.warningButton))
				{
					dropdown = false;
					resetKey();
					return;
				}

				KeyCode k = getKey();
				if (k == KeyCode.None)
					return;

				setKey(k);
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
			else if (dmulti)
				return multiSelect.ToString();
			return "";
		}

		private void setKey(KeyCode k)
		{
			if (dup)
				up = k;
			else if (ddown)
				down = k;
			else if (dleft)
				left = k;
			else if (dright)
				right = k;
			else if (daccept)
				accept = k;
			else if (ddecline)
				decline = k;
			else if (dmulti)
				multiSelect = k;
		}

		private void resetKey()
		{
			if (dup)
				up = CapCom.Settings.scrollUp;
			else if (ddown)
				down = CapCom.Settings.scrollDown;
			else if (dleft)
				left = CapCom.Settings.listLeft;
			else if (dright)
				right = CapCom.Settings.listRight;
			else if (daccept)
				accept = CapCom.Settings.accept;
			else if (ddecline)
				decline = CapCom.Settings.cancel;
			else if (dmulti)
				multiSelect = CapCom.Settings.multiSelect;
		}

	}
}
