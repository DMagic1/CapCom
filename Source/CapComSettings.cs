using CapCom.Framework;
using UnityEngine;

namespace CapCom
{
	public class CapComSettings : CC_ConfigNodeStorage
	{
		[Persistent]
		public bool stockToolbar = true;
		[Persistent]
		public bool hideBriefing = false;
		[Persistent]
		public bool hideNotes = false;
		[Persistent]
		public bool showDeclineWarning = true;
		[Persistent]
		public bool showCancelWarning = true;
		[Persistent]
		public int sortMode = 0;
		[Persistent]
		public bool ascending = true;
		[Persistent]
		public bool activeLimit = true;
		[Persistent]
		public float windowHeight = 600;
		[Persistent]
		public float windowPosX = 50;
		[Persistent]
		public float windowPosY = 50;
		[Persistent]
		public float windowSize = 0;
		[Persistent]
		public KeyCode scrollUp = KeyCode.UpArrow;
		[Persistent]
		public KeyCode scrollDown = KeyCode.DownArrow;
		[Persistent]
		public KeyCode listRight = KeyCode.RightArrow;
		[Persistent]
		public KeyCode listLeft = KeyCode.LeftArrow;
		[Persistent]
		public KeyCode accept = KeyCode.Return;
		[Persistent]
		public KeyCode cancel = KeyCode.Delete;

		public CapComSettings (string file)
		{
			FilePath = file;
			NodeName = "CapCom/" + file + "/" + this.GetType().Name;

			Load();
		}
	}
}
