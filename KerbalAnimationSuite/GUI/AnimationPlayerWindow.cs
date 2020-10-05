using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace KerbalAnimation
{
	public class AnimationPlayerWindow : Window
	{
		//constructor
		public AnimationPlayerWindow()
		{
			WindowTitle = "Animation Player";
			WindowRect = new Rect(Screen.width - 325f, 25f, 280f, 0f);
			ExpandHeight = true;
			Loop = false;
		}

		//animations
		private List<KerbalAnimationClip> Clips = null;
		public KerbalAnimationClip GetNumberKeyClip(int index)
		{
			if (index >= Clips.Count)
				return null;
			else
				return Clips [NumberKeyClips [index]];
		}

		//gui values
		private Dictionary<string, string> textBoxValues = new Dictionary<string, string>();
		private Vector2 scroll;
		public static bool Loop
		{
			get;
			private set;
		}

		public int[] NumberKeyClips = new int[10];

		protected override void DrawWindow()
		{
			if (Clips.Count > 0)
			{
				scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(320f), GUILayout.ExpandWidth(true));
				for (int i = 0; i < Clips.Count; i++)
				{
					int nameValue = i + 1;
					if (nameValue > 9)
						nameValue = 0;
					NumberKeyClips [i] = DrawClipSelector("NumberKey" + nameValue.ToString(), nameValue.ToString(), NumberKeyClips [i]);
				}

				GUILayout.Label("<color=" + Colors.Information + ">Press the numbers 0-9(not on the numpad) to play the selected animations. Hold left shift to play the animation on all kerbals instead of just the active one</color>");

				GUILayout.EndScrollView();
			}
			Loop = GUILayout.Toggle(Loop, "Loop?");
			if (GUILayout.Button("Reload Animations"))
			{
				ReloadAnimations();
			}
		}
		public override void Update()
		{
			if (Clips == null)
				ReloadAnimations();
		}

		//gui methods
		private int DrawClipSelector(string uniqueName, string name, int index)
		{
			if (!textBoxValues.ContainsKey(uniqueName))
				textBoxValues.Add(uniqueName, Clips[index].Name);

			string textBoxControlName = "ClipSelector_" + uniqueName;

			GUILayout.BeginHorizontal();

			GUILayout.Label("<color=" + Colors.Information + ">" + name + ":</color>", GUILayout.Width(30f));

			bool buttonPressed = false;
			int buttonValue = index;
			int buttonIncrement = 1;
			if (GUILayout.Button("<<", GUILayout.MaxWidth(40f), GUILayout.Height(24f)))
			{
				buttonValue -= buttonIncrement;
				buttonPressed = true;
			}

			//text field
			GUI.SetNextControlName(textBoxControlName);
			GUILayout.TextField(textBoxValues [uniqueName], GUILayout.Width(160f));

			if (GUILayout.Button(">>", GUILayout.MaxWidth(40f), GUILayout.Height(24f)))
			{
				buttonValue += buttonIncrement;
				buttonPressed = true;
			}
			if (buttonPressed)
			{
				if (buttonValue < 0)
					buttonValue = Clips.Count - 1;
				else if (buttonValue >= Clips.Count)
					buttonValue = 0;

				textBoxValues [uniqueName] = Clips[buttonValue].Name;
				GUI.FocusControl("");
			}

			GUILayout.EndHorizontal();

			return buttonValue;
		}

		//utility methods
		public void ReloadAnimations()
		{
			Clips = new List<KerbalAnimationClip>();
			foreach (var path in Directory.GetFiles(KSPUtil.ApplicationRootPath + "GameData/", "*.anim", SearchOption.AllDirectories))
			{
				KerbalAnimationClip clip = new KerbalAnimationClip();
				clip.LoadFromPath(path);
				Clips.Add(clip);
				Debug.Log("KerbalAnimationClip " + clip.Name + " loaded from " + path);
			}

			int length = 10;
			if (Clips.Count < 10)
				length = Clips.Count;
			for (int i = 0; i < length; i++)
			{
				NumberKeyClips [i] = i;
			}

			AnimationPlayerWindowHost.Instance.OnReloadAnimationClips.Fire(Clips);
		}
	}
}

