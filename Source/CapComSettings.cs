#region license
/*The MIT License (MIT)
CapComSettings - Serializable object to store configuration options in an external file

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
		public bool tooltipsEnabled = true;
		[Persistent]
		public bool useKSPStyle = false;
		[Persistent]
		public int sortMode = 0;
		[Persistent]
		public bool ascending = true;
		[Persistent]
		public bool activeLimit = true;
		[Persistent]
		public bool forceDecline = false;
		[Persistent]
		public bool forceCancel = false;
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
		[Persistent]
		public KeyCode multiSelect = KeyCode.LeftControl;

		public CapComSettings (string file)
		{
			FilePath = file;
			NodeName = "CapCom/" + file + "/" + this.GetType().Name;

			Load();
		}
	}
}
