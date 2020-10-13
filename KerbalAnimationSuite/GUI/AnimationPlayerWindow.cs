using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using Contracts.Predicates;
using System.Text.RegularExpressions;

namespace KerbalAnimation
{
	public class AnimationPlayerWindow : Window
	{
		//constructor
		public AnimationPlayerWindow()
		{
			WindowTitle = "Animation Player";
			WindowRect = new Rect(Screen.width - 380f, 25f, 300f, 0f);
			ExpandHeight = true;
			//ExpandWidth = true;

			Loop = false;

			NumberKeyClips = new Dictionary<GameObject, int[]>();
			ActiveKerbals = new List<GameObject>();
			AllKerbals = new List<GameObject>();

			// Init global key clips
			for (int i = 0; i < 10; i++)
			{
				GlobalNumberKeyClips[i] = i;
			}
		}

		//events
		public EventData<List<KerbalAnimationClip>> OnReloadAnimationClips = new EventData<List<KerbalAnimationClip>>("OnReloadAnimationClips");

		// Active Kerbal
		public GameObject SelectedKerbal;

		//animations
		private List<KerbalAnimationClip> Clips = null;
		public KerbalAnimationClip GetNumberKeyClip(GameObject kerbal, int index)
		{
			if (index >= Clips.Count) return null;

			if (UseKerbalSpecificAnimations)
            {
				return Clips[NumberKeyClips[kerbal][index]];
			}
			else
            {
				return Clips[GlobalNumberKeyClips[index]];
			}
		}

		public bool ShouldAnimateKerbal(GameObject kerbalObj, bool shiftDown, bool isActiveVessel)
        {
			if (UseKerbalSpecificAnimations)
            {
				return (ActiveKerbals.Contains(kerbalObj) && ((kerbalObj == SelectedKerbal) || shiftDown));
            }
			else
            {
				return isActiveVessel || shiftDown;
			}
        }

		//gui values
		private Dictionary<string, string> textBoxValues = new Dictionary<string, string>();
		//private Vector2 scroll;
		public static bool Loop {get; private set;}
		public static bool UseKerbalSpecificAnimations { get; private set; }

		// Dictionary of the bindings for each kerbal
		private Dictionary<GameObject, int[]> NumberKeyClips;
		private List<GameObject> ActiveKerbals;
		private List<GameObject> AllKerbals;

		private int[] GlobalNumberKeyClips = new int[10];

		public void AddKerbal(GameObject kerbalObj)
        {
			NumberKeyClips.Add(kerbalObj, new int[10]);
			for (int i = 0; i < 10; i++)
			{
				NumberKeyClips[kerbalObj][i] = i;
			}

			ActiveKerbals.Add(kerbalObj);
			AllKerbals.Add(kerbalObj);

			if (!SelectedKerbal) SelectedKerbal = kerbalObj;
		}
		public void RemoveKerbal(GameObject kerbalObj)
		{
			NumberKeyClips.Remove(kerbalObj);
			ActiveKerbals.Remove(kerbalObj);
			AllKerbals.Remove(kerbalObj);

			if (SelectedKerbal == kerbalObj)
            {
				if (AllKerbals.Count > 0)
                {
					SelectedKerbal = AllKerbals.First();
				}
				else
                {
					SelectedKerbal = null;
                }
            }
		}

		protected override void DrawWindow()
		{
			if (Clips == null)
			{
				ReloadAnimations();
			}
			if (AllKerbals.Count > 0)
			{
				if (Clips.Count > 0)
				{
					UseKerbalSpecificAnimations = GUILayout.Toggle(UseKerbalSpecificAnimations, "Kerbal-Specific Animations");

					if (UseKerbalSpecificAnimations)
					{
						// Ensure there is a selected kerbal
						if (SelectedKerbal == null)
                        {
							SelectedKerbal = AllKerbals.First();
						}

						GUILayout.Space(5f);

						GUILayout.BeginVertical(skin.box);
						GUILayout.Space(10f);
						SelectedKerbal = DrawKerbalSelector();
						GUILayout.Space(5f);

						// Kerbal active toggle
						bool isKerbalEnabled = ActiveKerbals.Contains(SelectedKerbal);
						isKerbalEnabled = GUILayout.Toggle(isKerbalEnabled, "Animate This Kerbal");
						// Maintain active list state
						if (isKerbalEnabled && !ActiveKerbals.Contains(SelectedKerbal)) ActiveKerbals.Add(SelectedKerbal);
						else if (!isKerbalEnabled && ActiveKerbals.Contains(SelectedKerbal)) ActiveKerbals.Remove(SelectedKerbal);

						GUILayout.Space(3f);
						GUILayout.EndVertical();
					}

					GUILayout.Space(10f);

					GUILayout.BeginVertical(skin.box);
					GUILayout.Space(6f);
					// Draw clip selection
					for (int i = 0; (i < Clips.Count) && (i < 10); i++)
					{
						int nameValue = i + 1;
						if (nameValue > 9) nameValue = 0;

						if (UseKerbalSpecificAnimations) NumberKeyClips[SelectedKerbal][i] = DrawClipSelector("NumberKey" + nameValue.ToString(), nameValue.ToString(), NumberKeyClips[SelectedKerbal][i]);
						else GlobalNumberKeyClips[i] = DrawClipSelector("NumberKey" + nameValue.ToString(), nameValue.ToString(), GlobalNumberKeyClips[i]);
					}
					GUILayout.Space(3f);
					GUILayout.EndVertical();

					// Only show Global reset button if kerbal-specific
					if (UseKerbalSpecificAnimations)
					{
						if (GUILayout.Button("Reset To Global Settings"))
						{
							for (int i = 0; i < 10; i++)
							{
								NumberKeyClips[SelectedKerbal][i] = GlobalNumberKeyClips[i];
							}
						}
					}
					GUILayout.Space(3f);

					GUILayout.Label("<color=" + Colors.Information + ">Press Alt + the numbers 0-9 (not on the numpad) to play the selected animations. Hold left shift to play the animation on all enabled kerbals instead of just the active one</color>");

					//GUILayout.EndScrollView();
				}
				Loop = GUILayout.Toggle(Loop, "Loop Animations");
				if (GUILayout.Button("Reload Animations"))
				{
					ReloadAnimations();
				}
			}
			else
            {
				GUILayout.Space(10f);
				GUILayout.Label("<color=" + Colors.Orange + ">No animatable Kerbals in scene. Only Kerbals on EVA or in an External Command Chair can be animated.</color>");
				GUILayout.Space(10f);
			}
		}
		public override void Update()
		{
			if (Clips == null) ReloadAnimations();
		}

		//gui methods
		private int DrawClipSelector(string uniqueName, string name, int index)
		{
			if (!textBoxValues.ContainsKey(uniqueName)) textBoxValues.Add(uniqueName, Clips[index].Name);

			string textBoxControlName = "ClipSelector_" + uniqueName;

			GUILayout.BeginHorizontal();

			GUILayout.Label("<b><color=" + Colors.Orange + ">" + name + ":</color></b>", GUILayout.Width(15f));

			bool buttonPressed = false;
			int buttonValue = index;
			int buttonIncrement = 1;
			if (GUILayout.Button("<", GUILayout.MaxWidth(40f), GUILayout.Height(24f)))
			{
				buttonValue -= buttonIncrement;
				buttonPressed = true;
			}

			//text field
			GUI.SetNextControlName(textBoxControlName);
			GUILayout.TextField(textBoxValues[uniqueName], GUILayout.ExpandWidth(true));

			if (GUILayout.Button(">", GUILayout.MaxWidth(40f), GUILayout.Height(24f)))
			{
				buttonValue += buttonIncrement;
				buttonPressed = true;
			}
			if (buttonPressed)
			{
				if (buttonValue < 0) buttonValue = Clips.Count - 1;
				else if (buttonValue >= Clips.Count) buttonValue = 0;
				GUI.FocusControl("");
			}
			textBoxValues[uniqueName] = Clips[buttonValue].Name;

			GUILayout.EndHorizontal();

			return buttonValue;
		}

		private GameObject DrawKerbalSelector()
		{
			string textBoxControlName = "KerbalSelector";

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

			//GUILayout.Label("<color=" + Colors.Information + ">Selected Kerbal:</color>", GUILayout.Width(30f));

			if (AllKerbals.Count == 1) GUI.enabled = false;
			bool buttonPressed = false;
			int buttonValue = AllKerbals.IndexOf(SelectedKerbal);
			int buttonIncrement = 1;
			if (GUILayout.Button("<", GUILayout.MaxWidth(40f), GUILayout.Height(24f)))
			{
				buttonValue -= buttonIncrement;
				buttonPressed = true;
			}
			GUI.enabled = true;

			// Use just Kerbal's name if possible
			string displayName = SelectedKerbal.name;
			Regex match = new Regex(@"\([^\)]+\)");
			if (match.IsMatch(SelectedKerbal.name)) displayName = SelectedKerbal.name.Split('(', ')')[1];

			//text field
			GUI.SetNextControlName(textBoxControlName);
			GUILayout.TextField(displayName, GUILayout.ExpandWidth(true));

			if (AllKerbals.Count == 1) GUI.enabled = false;
			if (GUILayout.Button(">", GUILayout.MaxWidth(40f), GUILayout.Height(24f)))
			{
				buttonValue += buttonIncrement;
				buttonPressed = true;
			}
			if (buttonPressed)
			{
				if (buttonValue < 0) buttonValue = AllKerbals.Count - 1;
				else if (buttonValue >= AllKerbals.Count) buttonValue = 0;
				GUI.FocusControl("");
			}
			GUI.enabled = true;

			GUILayout.EndHorizontal();

			return AllKerbals[buttonValue];
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

			AnimationPlayerWindowHost.Instance.OnReloadAnimationClips.Fire(Clips);
		}
	}
}

