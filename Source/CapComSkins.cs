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
		internal static GUIStyle textureButton;
		internal static GUIStyle tabButton;

		//Text label styles
		internal static GUIStyle missionText;
		internal static GUIStyle headerText;
		internal static GUIStyle titleText;
		internal static GUIStyle briefingText;
		internal static GUIStyle synopsisText;
		internal static GUIStyle parameterText;
		internal static GUIStyle noteText;

		//Reward styles
		internal static GUIStyle advance;
		internal static GUIStyle reward;
		internal static GUIStyle penalty;

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
			titleButton.fontSize = 14 + fontSize;
			titleButton.wordWrap = true;
			titleButton.fontStyle = FontStyle.Bold;
			titleButton.normal.textColor = XKCDColors.FadedYellow;

			tabButton = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.button);
			tabButton.name = "TabButton";
			tabButton.fontSize = 12 + fontSize;
			tabButton.normal.textColor = XKCDColors.FadedRed;

			missionText = new GUIStyle(CC_SkinsLibrary.DefUnitySkin.label);
			missionText.name = "MissionText";
			missionText.richText = true;

			CC_SkinsLibrary.List["CCUnitySkin"].window = new GUIStyle(newWindowStyle);
			CC_SkinsLibrary.List["CCUnitySkin"].box = new GUIStyle(dropDown);
			CC_SkinsLibrary.List["CCUnitySkin"].button = new GUIStyle(titleButton);
			CC_SkinsLibrary.List["CCUnitySkin"].label = new GUIStyle(missionText);

			CC_SkinsLibrary.AddStyle("CCUnitySkin", newWindowStyle);
			CC_SkinsLibrary.AddStyle("CCUnitySkin", dropDown);
			CC_SkinsLibrary.AddStyle("CCUnitySkin", titleButton);
			CC_SkinsLibrary.AddStyle("CCUnitySkin", missionText);
		}
	}
}
