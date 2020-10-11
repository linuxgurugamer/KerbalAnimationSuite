using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace KerbalAnimation
{
	public class AnimationLoadWindow : Window
	{
		//constructor
		public AnimationLoadWindow(Action<string> loadCallback, Action cancelCallback)
		{
			WindowTitle = "Load Animation";
			WindowRect = new Rect(610f, 250f, 500f, 400f);
			ExpandHeight = true;
			//ExpandWidth = true;

			this.loadCallback = loadCallback;
			this.cancelCallback = cancelCallback;

			presetAnimPathMap = new Dictionary<string, string>();
			customAnimPathMap = new Dictionary<string, string>();

			UpdateAnimations();
		}

		//callback
		private Action<string> loadCallback;
		private Action cancelCallback;

		//gui values
		private Vector2 fileSelectScroll;
		private bool fileSelected = false;
		private string selectedFilePath;

		// Map of animation names to filepaths
		private Dictionary<string, string> presetAnimPathMap;
		private Dictionary<string, string> customAnimPathMap;


		protected override void DrawWindow()
		{
			GUILayout.Space(3f);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Refresh", GUILayout.ExpandWidth(false)))
			{
				UpdateAnimations();
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10f);

			fileSelectScroll = GUILayout.BeginScrollView(fileSelectScroll, GUILayout.ExpandWidth(true));

			GUILayout.Label("<b>Presets</b>");
			GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

			foreach (string friendlyName in presetAnimPathMap.Keys)
			{
				DrawFileSelector(friendlyName, true);
			}

			GUILayout.Label("<b>Custom</b>");

			foreach (string friendlyName in customAnimPathMap.Keys)
			{
				DrawFileSelector(friendlyName, false);
			}

			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			GUILayout.Space(15f);

			if (!fileSelected) GUI.enabled = false;
			if (GUILayout.Button("Load"))
			{
				loadCallback(selectedFilePath);
				cancelCallback();
				selectedFilePath = null;
				fileSelected = false;
			}
			GUI.enabled = true;
			if (GUILayout.Button("Cancel"))
			{
				cancelCallback();
				selectedFilePath = null;
				fileSelected = false;
			}

		}
		public override void Update()
		{

		}

		public void UpdateAnimations()
        {
			presetAnimPathMap.Clear();
			foreach (var path in Directory.GetFiles(KSPUtil.ApplicationRootPath + "GameData/KerbalAnimationSuite/Presets/", "*.anim"))
			{
				string friendlyName;
				// Get the actual animation name, if possible
				var node = ConfigNode.Load(path);
				if (node == null)
				{
					Debug.LogError("ConfigNode not found at " + path);
					// Fallback to filename
					friendlyName = path.Split('/').Last();
				}
				else
				{
					friendlyName = node.GetValue("Name");
				}

				presetAnimPathMap.Add(friendlyName, path);
			}

			customAnimPathMap.Clear();
			foreach (var path in Directory.GetFiles(KSPUtil.ApplicationRootPath + "GameData/KerbalAnimationSuite/Output/", "*.anim"))
			{
				string friendlyName;
				// Get the actual animation name, if possible
				var node = ConfigNode.Load(path);
				if (node == null)
				{
					Debug.LogError("ConfigNode not found at " + path);
					// Fallback to filename
					friendlyName = path.Split('/').Last();
				}
				else
				{
					friendlyName = node.GetValue("Name");
				}

				customAnimPathMap.Add(friendlyName, path);
			}
		}

		// gui utils
		void DrawFileSelector(string friendlyName, bool isPreset)
		{
			string path = (isPreset) ? presetAnimPathMap[friendlyName] : customAnimPathMap[friendlyName];

			if (fileSelected && (selectedFilePath == path))
			{
				fileSelected = GUILayout.Toggle(fileSelected, "<color=" + Colors.SelectedColor + ">" + friendlyName + "</color>", skin.button);
				if (!fileSelected)
				{
					selectedFilePath = null;
				}
			}
			else if (GUILayout.Button(friendlyName))
			{
				selectedFilePath = path;
				fileSelected = true;
			}
		}
	}
}

