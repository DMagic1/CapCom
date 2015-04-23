using System;
using System.Collections.Generic;
using System.Linq;
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
		public int sortMode = 0;
		[Persistent]
		public float windowHeight = 600;
		[Persistent]
		public Vector2 windowPosition = new Vector2(50, 50);
		[Persistent]
		public float windowSize = 0;

		public CapComSettings (string file)
		{
			FilePath = file;
			NodeName = "CapCom/" + file + "/" + this.GetType().Name;

			Load();
		}

		public override void OnEncodeToConfigNode()
		{

		}

		public override void OnDecodeFromConfigNode()
		{
			
		}

	}
}
