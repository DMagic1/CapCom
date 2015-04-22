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
		internal static GUIStyle flagButton;

		//Text label styles
		internal static GUIStyle headerText;
		internal static GUIStyle titleText;
		internal static GUIStyle briefingText;
		internal static GUIStyle synopsisText;
		internal static GUIStyle parameterText;
		internal static GUIStyle subParameterText;
		internal static GUIStyle noteText;
		internal static GUIStyle smallText;

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
		internal static Texture2D fundsRed;
		internal static Texture2D repGreen;
		internal static Texture2D repRed;
		internal static Texture2D science;
		internal static Texture2D orderAsc;
		internal static Texture2D orderDesc;
		internal static Texture2D tooltipIcon;
		internal static Texture2D goldStar;
		internal static Texture2D goldStarTwo;
		internal static Texture2D goldStarThree;
		internal static Texture2D goldStarVertical;
		internal static Texture2D goldStarTwoVertical;
		internal static Texture2D goldStarThreeVertical;
		internal static Texture2D settingsIcon;
		internal static Texture2D flagBackDrop;
		internal static Texture2D resizeHandle;
		internal static Texture2D checkBox;
		internal static Texture2D failBox;

		internal static int fontSize = 0;

		protected override void OnGUIOnceOnly()
		{
			windowTex = GameDatabase.Instance.GetTexture("CapCom/Textures/WindowTex", false);
			dropDownTex = GameDatabase.Instance.GetTexture("CapCom/Textures/DropDownTex", false);
			toolbarIcon = GameDatabase.Instance.GetTexture("CapCom/Textures/CapComAppIcon", false);
			buttonHover = GameDatabase.Instance.GetTexture("CapCom/Textures/ButtonHover", false);
			fundsGreen = GameDatabase.Instance.GetTexture("CapCom/Textures/FundsGreenIcon", false);
			fundsRed = GameDatabase.Instance.GetTexture("CapCom/Textures/FundsRedIcon", false);
			repGreen = GameDatabase.Instance.GetTexture("CapCom/Textures/RepGreenIcon", false);
			repRed = GameDatabase.Instance.GetTexture("CapCom/Textures/RepRedIcon", false);
			science = GameDatabase.Instance.GetTexture("CapCom/Textures/ScienceIcon", false);
			orderAsc = GameDatabase.Instance.GetTexture("CapCom/Textures/OrderAsc", false);
			orderDesc = GameDatabase.Instance.GetTexture("CapCom/Textures/OrderDesc", false);
			tooltipIcon = GameDatabase.Instance.GetTexture("CapCom/Textures/ToolTipIcon", false);
			goldStar = GameDatabase.Instance.GetTexture("CapCom/Textures/GoldStar", false);
			goldStarTwo = GameDatabase.Instance.GetTexture("CapCom/Textures/GoldStarTwo", false);
			goldStarThree = GameDatabase.Instance.GetTexture("CapCom/Textures/GoldStarThree", false);
			goldStarVertical = GameDatabase.Instance.GetTexture("CapCom/Textures/GoldStarVertical", false);
			goldStarTwoVertical = GameDatabase.Instance.GetTexture("CapCom/Textures/GoldStarTwoVertical", false);
			goldStarThreeVertical = GameDatabase.Instance.GetTexture("CapCom/Textures/GoldStarThreeVertical", false);
			settingsIcon = GameDatabase.Instance.GetTexture("CapCom/Textures/ToolbarSettingsIcon", false);
			flagBackDrop = GameDatabase.Instance.GetTexture("CapCom/Textures/FlagBackDrop", false);
			resizeHandle = GameDatabase.Instance.GetTexture("CapCom/Textures/ResizeIcon", false);
			checkBox = GameDatabase.Instance.GetTexture("CapCom/Textures/CheckBoxIcon", false);
			failBox = GameDatabase.Instance.GetTexture("CapCom/Textures/FailBoxIcon", false);

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
			titleButton.normal.textColor = XKCDColors.FadedYellow;
			titleButton.padding = new RectOffset(2, 14, 2, 2);

			titleButtonBehind = new GUIStyle(titleButton);
			titleButtonBehind.hover.background = titleButtonBehind.normal.background;
			titleButtonBehind.hover.textColor = titleButtonBehind.normal.textColor;

			tabButton = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.button);
			tabButton.fontSize = 12 + fontSize;
			tabButton.fontStyle = FontStyle.Bold;
			tabButton.normal.textColor = XKCDColors.FadedRed;

			textureButton = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.button);
			textureButton.fontSize = 13;
			textureButton.normal.background = CC_SkinsLibrary.DefUnitySkin.label.normal.background;
			textureButton.hover.background = buttonHover;
			textureButton.padding = new RectOffset(1, 1, 2, 2);

			flagButton = new GUIStyle(textureButton);
			flagButton.normal.background = flagBackDrop;
			flagButton.hover.background = flagBackDrop;

			//Label Styles
			headerText = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.label);
			headerText.fontSize = 13 + fontSize;
			headerText.fontStyle = FontStyle.Bold;
			headerText.alignment = TextAnchor.MiddleLeft;
			headerText.normal.textColor = XKCDColors.FadedOrange;

			titleText = new GUIStyle(headerText);
			titleText.normal.textColor = XKCDColors.White;

			briefingText = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.label);
			briefingText.fontSize = 11 + fontSize;
			briefingText.alignment = TextAnchor.MiddleLeft;
			briefingText.normal.textColor = XKCDColors.OffWhite;

			synopsisText = new GUIStyle(briefingText);
			synopsisText.fontSize = 12 + fontSize;
			synopsisText.fontStyle = FontStyle.Bold;

			parameterText = new GUIStyle(synopsisText);
			parameterText.normal.textColor = XKCDColors.PaleGrey;

			subParameterText = new GUIStyle(parameterText);
			subParameterText.normal.textColor = XKCDColors.LightGrey;

			noteText = new GUIStyle(synopsisText);
			noteText.fontStyle = FontStyle.Normal;
			noteText.normal.textColor = XKCDColors.AquaBlue;

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
			funds.normal.textColor = XKCDColors.FreshGreen;

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
