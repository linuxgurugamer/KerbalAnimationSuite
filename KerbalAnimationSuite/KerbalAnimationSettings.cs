using System;
using UnityEngine;

namespace KerbalAnimation
{
	public class KerbalAnimationSettings
	{
		//constructor
		public KerbalAnimationSettings()
		{
			//load settings
			Load();

			//subscribe to save event
			GameEvents.onGameStateSaved.Add(OnGameStateSaved);
		}
		~KerbalAnimationSettings()
		{
			GameEvents.onGameStateSaved.Remove(OnGameStateSaved);
		}

		//path
		public static string Path = KSPUtil.ApplicationRootPath + "GameData/KerbalAnimationSuite/Settings.dat";

		//save/load
		public void Load()
		{
			ConfigNode node = ConfigNode.Load(Path);
			if (node == null)
			{
				Save();
				node = ConfigNode.Load(Path);
			}

			if (node.HasValue("AllowEditorMusic")) bool.TryParse(node.GetValue("AllowEditorMusic"), out AllowEditorMusic);
			if (node.HasValue("UseAltSkin")) bool.TryParse(node.GetValue("UseAltSkin"), out UseAltSkin);

		}
		public void Save()
		{
			ConfigNode node = new ConfigNode("KerbalAnimationSuite_Settings");

			node.AddValue("AllowEditorMusic", AllowEditorMusic);
			node.AddValue("UseAltSkin", UseAltSkin);

			node.Save(Path);
		}

		//Events
		private void OnGameStateSaved(Game game)
		{
			Debug.Log("Saving KerbalAnimationSuiteSettings...");
			Save();
		}

		//Settings
		public bool AllowEditorMusic = true;
		public bool UseAltSkin = true;
	}
}

