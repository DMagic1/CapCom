#region license
/*The MIT License (MIT)
CapComSkins - Store and initialize styles and textures

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
using UnityEngine;

namespace CapCom
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	class CapComSkins : CC_MBE
	{
		internal static GUISkin ccUnitySkin;

		//Window styles
		internal static GUIStyle newWindowStyle;
		internal static GUIStyle dropDown;

		//Button styles
		internal static GUIStyle titleButton;
		internal static GUIStyle titleButtonBehind;
		internal static GUIStyle textureButton;
		internal static GUIStyle tabButton;
		internal static GUIStyle tabButtonInactive;
		internal static GUIStyle menuButton;
		internal static GUIStyle keycodeButton;
		internal static GUIStyle warningButton;

		//Icon Button styles
		internal static GUIStyle iconButton;

		//Text label styles
		internal static GUIStyle headerText;
		internal static GUIStyle reassignText;
		internal static GUIStyle reassignCurrentText;
		internal static GUIStyle warningText;
		internal static GUIStyle titleText;
		internal static GUIStyle briefingText;
		internal static GUIStyle synopsisText;
		internal static GUIStyle parameterText;
		internal static GUIStyle subParameterText;
		internal static GUIStyle noteText;
		internal static GUIStyle smallText;
		internal static GUIStyle timerText;
		internal static GUIStyle agencyContractText;

		//Reward styles
		internal static GUIStyle advance;
		internal static GUIStyle completion;
		internal static GUIStyle failure;
		internal static GUIStyle funds;
		internal static GUIStyle rep;
		internal static GUIStyle sci;

		internal static Texture2D toolbarIcon;
		internal static Texture2D dropDownTex;
		internal static Texture2D windowTex;
		internal static Texture2D buttonHover;
		internal static Texture2D fundsGreen;
		internal static Texture2D repRed;
		internal static Texture2D science;
		internal static Texture2D orderAsc;
		internal static Texture2D orderDesc;
		internal static Texture2D goldStar;
		internal static Texture2D goldStarTwo;
		internal static Texture2D goldStarThree;
		internal static Texture2D goldStarVertical;
		internal static Texture2D goldStarTwoVertical;
		internal static Texture2D goldStarThreeVertical;
		internal static Texture2D settingsIcon;
		internal static Texture2D resizeHandle;
		internal static Texture2D checkBox;
		internal static Texture2D failBox;
		internal static Texture2D emptyBox;
		internal static Texture2D notesPlusIcon;
		internal static Texture2D notesMinusIcon;
		internal static Texture2D sortStars;
		internal static Texture2D sortRewards;
		internal static Texture2D sortAgents;
		internal static Texture2D toggleOn;
		internal static Texture2D toggleOff;
		internal static Texture2D toggleHoverOff;
		internal static Texture2D toggleHoverOn;
		internal static Texture2D currentFlag;

		//Mission Control Center texture map
		internal static Texture2D missionControlTexture;

		//Texture element locations on the mission control center texture map; values are normalized
		internal static Rect acceptButtonNormal = new Rect(0, 0.3671875f, 0.04296875f, 0.0439456125f);
		internal static Rect acceptButtonHover = new Rect(0.0478515625f, 0.3671875f, 0.04296875f, 0.0439456125f);
		internal static Rect acceptButtonActive = new Rect(0.095703125f, 0.3671875f, 0.04296875f, 0.0439456125f);
		internal static Rect acceptButtonInactive = new Rect(0, 0.4169921875f, 0.04296875f, 0.0439456125f);

		internal static Rect declineButtonNormal = new Rect(0.0478515625f, 0.4169921875f, 0.04296875f, 0.0439456125f);
		internal static Rect declineButtonHover = new Rect(0.095703125f, 0.4169921875f, 0.04296875f, 0.0439456125f);
		internal static Rect declineButtonActive = new Rect(0, 0.466796875f, 0.04296875f, 0.0439456125f);
		internal static Rect declineButtonInactive = new Rect(0.0478515625f, 0.466796875f, 0.04296875f, 0.0439456125f);

		internal static Rect cancelButtonNormal = new Rect(0.095703125f, 0.466796875f, 0.04296875f, 0.0439456125f);
		internal static Rect cancelButtonHover = new Rect(0, 0.5166015625f, 0.04296875f, 0.0439456125f);
		internal static Rect cancelButtonActive = new Rect(0.0478515625f, 0.5166015625f, 0.04296875f, 0.0439456125f);
		internal static Rect cancelButtonInactive = new Rect(0.095703125f, 0.5166015625f, 0.04296875f, 0.0439456125f);

		internal static Rect titleButtonFlagNormal = new Rect(0, 0.3125f, 0.0751953125f, 0.0498046875f);
		internal static Rect titleButtonFlagActive = new Rect(0, 0.2626953125f, 0.0751953125f, 0.0498046875f);

		internal static Rect titleButtonNormal = new Rect(0.0751953125f, 0.3125f, 0.0244140625f, 0.0498046875f);
		internal static Rect titleButtonActive = new Rect(0.0751953125f, 0.2626953125f, 0.0244140625f, 0.0498046875f);

		internal static Rect tabButtonNormalLeft = new Rect(0, 0.560546875f, 0.017578125f, 0.0224609375f);
		internal static Rect tabButtonNormalMiddle = new Rect(0.017578125f, 0.560546875f, 0.009765625f, 0.0224609375f);
		internal static Rect tabButtonNormalRight = new Rect(0.02734375f, 0.560546875f, 0.02734375f, 0.0224609375f);

		internal static Rect tabButtonHoverLeft = new Rect(0.0546875f, 0.560546875f, 0.017578125f, 0.0224609375f);
		internal static Rect tabButtonHoverMiddle = new Rect(0.0712890625f, 0.560546875f, 0.009765625f, 0.0224609375f);
		internal static Rect tabButtonHoverRight = new Rect(0.0810546875f, 0.560546875f, 0.0263671875f, 0.0224609375f);

		internal static Rect tabButtonActiveLeft = new Rect(0.9462890625f, 0.099609375f, 0.017578125f, 0.0224609375f);
		internal static Rect tabButtonActiveMiddle = new Rect(0.962890625f, 0.099609375f, 0.009765625f, 0.0224609375f);
		internal static Rect tabButtonActiveRight = new Rect(0.9736328125f, 0.099609375f, 0.0263671875f, 0.0224609375f);

		internal static Rect flagRect = new Rect(0, 0.1513671875f, 0.16796875f, 0.1103515625f);

		internal static int fontSize = 0;

		protected override void OnGUIOnceOnly()
		{
			windowTex = GameDatabase.Instance.GetTexture("CapCom/Textures/WindowTex", false);
			dropDownTex = GameDatabase.Instance.GetTexture("CapCom/Textures/DropDownTex", false);
			toolbarIcon = GameDatabase.Instance.GetTexture("CapCom/Textures/CapComAppIcon", false);
			buttonHover = GameDatabase.Instance.GetTexture("CapCom/Textures/ButtonHover", false);
			fundsGreen = GameDatabase.Instance.GetTexture("CapCom/Textures/FundsGreenIcon", false);
			repRed = GameDatabase.Instance.GetTexture("CapCom/Textures/RepRedIcon", false);
			science = GameDatabase.Instance.GetTexture("CapCom/Textures/ScienceIcon", false);
			orderAsc = GameDatabase.Instance.GetTexture("CapCom/Textures/OrderAsc", false);
			orderDesc = GameDatabase.Instance.GetTexture("CapCom/Textures/OrderDesc", false);
			goldStar = GameDatabase.Instance.GetTexture("CapCom/Textures/GoldStar", false);
			goldStarTwo = GameDatabase.Instance.GetTexture("CapCom/Textures/GoldStarTwo", false);
			goldStarThree = GameDatabase.Instance.GetTexture("CapCom/Textures/GoldStarThree", false);
			goldStarVertical = GameDatabase.Instance.GetTexture("CapCom/Textures/GoldStarVertical", false);
			goldStarTwoVertical = GameDatabase.Instance.GetTexture("CapCom/Textures/GoldStarTwoVertical", false);
			goldStarThreeVertical = GameDatabase.Instance.GetTexture("CapCom/Textures/GoldStarThreeVertical", false);
			settingsIcon = GameDatabase.Instance.GetTexture("CapCom/Textures/ToolbarSettingsIcon", false);
			resizeHandle = GameDatabase.Instance.GetTexture("CapCom/Textures/ResizeIcon", false);
			checkBox = GameDatabase.Instance.GetTexture("CapCom/Textures/CheckBoxIcon", false);
			failBox = GameDatabase.Instance.GetTexture("CapCom/Textures/FailBoxIcon", false);
			emptyBox = GameDatabase.Instance.GetTexture("CapCom/Textures/EmptyBoxIcon", false);
			notesPlusIcon = GameDatabase.Instance.GetTexture("CapCom/Textures/OpenNotesIcon", false);
			notesMinusIcon = GameDatabase.Instance.GetTexture("CapCom/Textures/CloseNotesIcon", false);
			sortStars = GameDatabase.Instance.GetTexture("CapCom/Textures/SortDifficultyIcon", false);
			sortRewards = GameDatabase.Instance.GetTexture("CapCom/Textures/SortRewardsIcon", false);
			sortAgents = GameDatabase.Instance.GetTexture("CapCom/Textures/SortRewardsIcon", false);

			toggleOn = CC_SkinsLibrary.DefKSPSkin.toggle.onNormal.background;
			toggleOff = CC_SkinsLibrary.DefKSPSkin.toggle.normal.background;
			toggleHoverOff = CC_SkinsLibrary.DefKSPSkin.toggle.hover.background;
			toggleHoverOn = CC_SkinsLibrary.DefKSPSkin.toggle.onHover.background;

			initializeSkins();
		}

		internal static void initializeSkins()
		{
			ccUnitySkin = CC_SkinsLibrary.CopySkin(CC_SkinsLibrary.DefSkinType.Unity);
			CC_SkinsLibrary.AddSkin("CCUnitySkin", ccUnitySkin);

			newWindowStyle = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.window);
			newWindowStyle.name = "WindowStyle";
			newWindowStyle.fontSize = 14;
			newWindowStyle.fontStyle = FontStyle.Bold;
			newWindowStyle.padding = new RectOffset(0, 1, 20, 12);
			newWindowStyle.normal.background = windowTex;
			newWindowStyle.focused.background = newWindowStyle.normal.background;
			newWindowStyle.onNormal.background = newWindowStyle.normal.background;

			dropDown = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.box);
			dropDown.name = "DropDown";
			dropDown.normal.background = dropDownTex;

			//Button Styles
			titleButton = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.button);
			titleButton.name = "TitleButton";
			titleButton.fontSize = 12 + fontSize;
			titleButton.wordWrap = true;
			titleButton.fontStyle = FontStyle.Bold;
			titleButton.normal.textColor = XKCDColors.ButterYellow;
			titleButton.padding = new RectOffset(2, 14, 2, 2);

			titleButtonBehind = new GUIStyle(titleButton);
			titleButtonBehind.hover.background = titleButtonBehind.normal.background;
			titleButtonBehind.hover.textColor = titleButtonBehind.normal.textColor;

			tabButton = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.button);
			tabButton.fontSize = 14 + fontSize;
			tabButton.fontStyle = FontStyle.Bold;
			tabButton.normal.textColor = XKCDColors.White;

			tabButtonInactive = new GUIStyle(tabButton);
			tabButtonInactive.fontSize = 12 + fontSize;
			tabButtonInactive.normal.textColor = XKCDColors.LightGrey;

			textureButton = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.button);
			textureButton.fontSize = 14;
			textureButton.fontStyle = FontStyle.Bold;
			textureButton.normal.background = CC_SkinsLibrary.DefUnitySkin.label.normal.background;
			textureButton.hover.background = buttonHover;
			textureButton.alignment = TextAnchor.MiddleCenter;
			textureButton.padding = new RectOffset(1, 1, 2, 2);

			menuButton = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.label);
			menuButton.fontSize = 12 + fontSize;
			menuButton.padding = new RectOffset(26, 2, 2, 2);
			menuButton.normal.textColor = XKCDColors.White;
			menuButton.hover.textColor = XKCDColors.AlmostBlack;
			Texture2D sortBackground = new Texture2D(1, 1);
			sortBackground.SetPixel(1, 1, XKCDColors.OffWhite);
			sortBackground.Apply();
			menuButton.hover.background = sortBackground;
			menuButton.alignment = TextAnchor.MiddleLeft;

			warningButton = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.button);
			warningButton.fontSize = 13 + fontSize;
			warningButton.fontStyle = FontStyle.Bold;
			warningButton.alignment = TextAnchor.MiddleCenter;

			iconButton = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.label);

			keycodeButton = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.button);

			//Label Styles
			headerText = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.label);
			headerText.fontSize = 13 + fontSize;
			headerText.fontStyle = FontStyle.Bold;
			headerText.alignment = TextAnchor.MiddleLeft;
			headerText.normal.textColor = XKCDColors.FadedOrange;

			warningText = new GUIStyle(headerText);
			warningText.alignment = TextAnchor.MiddleCenter;
			warningText.normal.textColor = XKCDColors.VomitYellow;

			reassignText = new GUIStyle(warningText);
			reassignText.normal.textColor = Color.white;

			reassignCurrentText = new GUIStyle(reassignText);
			reassignCurrentText.fontStyle = FontStyle.Bold;

			titleText = new GUIStyle(headerText);
			titleText.normal.textColor = XKCDColors.White;

			briefingText = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.label);
			briefingText.fontSize = 11 + fontSize;
			briefingText.alignment = TextAnchor.MiddleLeft;
			briefingText.normal.textColor = XKCDColors.KSPNeutralUIGrey;

			synopsisText = new GUIStyle(briefingText);
			synopsisText.fontSize = 12 + fontSize;
			synopsisText.fontStyle = FontStyle.Bold;

			parameterText = new GUIStyle(synopsisText);
			parameterText.normal.textColor = XKCDColors.Beige;

			subParameterText = new GUIStyle(parameterText);
			subParameterText.normal.textColor = XKCDColors.DarkBeige;

			timerText = new GUIStyle(synopsisText);
			timerText.normal.textColor = XKCDColors.OffWhite;

			noteText = new GUIStyle(synopsisText);
			noteText.fontStyle = FontStyle.Normal;
			noteText.normal.textColor = XKCDColors.TiffanyBlue;

			agencyContractText = new GUIStyle(synopsisText);
			agencyContractText.normal.textColor = XKCDColors.ButterYellow;

			smallText = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.label);
			smallText.fontSize = 11 + fontSize;
			smallText.alignment = TextAnchor.MiddleLeft;

			//Reward and Penalty Styles
			advance = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.label);
			advance.fontSize = 12 + fontSize;
			advance.fontStyle = FontStyle.Bold;
			advance.alignment = TextAnchor.MiddleLeft;
			advance.wordWrap = false;
			advance.normal.textColor = XKCDColors.DullYellow;

			completion = new GUIStyle(advance);
			completion.normal.textColor = XKCDColors.DustyGreen;

			failure = new GUIStyle(advance);
			failure.normal.textColor = XKCDColors.DustyRed;

			funds = new GUIStyle(advance);
			funds.fontStyle = FontStyle.Normal;
			funds.normal.textColor = XKCDColors.PaleOliveGreen;

			rep = new GUIStyle(funds);
			rep.normal.textColor = XKCDColors.BrownishYellow;

			sci = new GUIStyle(funds);
			sci.normal.textColor = XKCDColors.AquaBlue;

			//Add Default Styles
			CC_SkinsLibrary.List["CCUnitySkin"].window = new GUIStyle(newWindowStyle);
			CC_SkinsLibrary.List["CCUnitySkin"].box = new GUIStyle(dropDown);
			CC_SkinsLibrary.List["CCUnitySkin"].button = new GUIStyle(titleButton);

			CC_SkinsLibrary.AddStyle("CCUnitySkin", newWindowStyle);
			CC_SkinsLibrary.AddStyle("CCUnitySkin", dropDown);
			CC_SkinsLibrary.AddStyle("CCUnitySkin", titleButton);
		}
	}
}
