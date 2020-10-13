using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI;

namespace KerbalAnimation
{
	public class AnimationWindow : Window
	{
		public static string ExportFullPath = KSPUtil.ApplicationRootPath + "GameData/KerbalAnimationSuite/Output";
		public static string ExportURL = "KerbalAnimationSuite/Output";

		//contructor
		public AnimationWindow()
		{
			SetupGUIStyles();

			//properties window
			Properties = new AnimationPropertiesWindow();
			OnGUI += DrawProperties;

			//load window
			animationLoad = new AnimationLoadWindow(loadAnimation, () => AnimationLoadOpen = false); ;
			OnGUI += DrawAnimationLoad;

			//get rgb colors from hex colors in Colors class
			KeyframeColor = Colors.HexToColor(Colors.KeyframeColor);
			SelectedKeyframeColor = Colors.HexToColor(Colors.SelectedKeyframeColor);
			ActiveTimeIndicatorColor = Colors.HexToColor(Colors.KSPLabelOrange);
			GreyedTimeIndicatorColor = Colors.HexToColor(Colors.Grey);

			WindowRect = new Rect(5f, 600f, 600f, 0f);
			WindowTitle = "Animation Editor";
			ExpandHeight = true;

			TimeIndicatorIcon = GameDatabase.Instance.GetTexture("KerbalAnimationSuite/Icons/timeline_arrow", false);
			if (TimeIndicatorIcon == null) TimeIndicatorIcon = Texture2D.whiteTexture;

			KeyframeIcon = GameDatabase.Instance.GetTexture("KerbalAnimationSuite/Icons/keyframe_icon", false);
			if (KeyframeIcon == null) KeyframeIcon = Texture2D.whiteTexture;

			PlayButtonNormal = GameDatabase.Instance.GetTexture("KerbalAnimationSuite/Icons/play_normal", false);
			if (PlayButtonNormal == null) PlayButtonNormal = Texture2D.whiteTexture;

			PlayButtonHover = GameDatabase.Instance.GetTexture("KerbalAnimationSuite/Icons/play_hover", false);
			if (PlayButtonHover == null) PlayButtonHover = Texture2D.whiteTexture;

			PlayButtonActive = GameDatabase.Instance.GetTexture("KerbalAnimationSuite/Icons/play_active", false);
			if (PlayButtonActive == null) PlayButtonActive = Texture2D.whiteTexture;

			//subscribe to the onNewAnimationClip event
			Suite.OnNewAnimationClip.Add(OnNewAnimationClip);
		}

		//animation
		public EditableAnimationClip animationClip {get{return Suite.AnimationClip;}}

		private KerbalAnimationClip.KerbalKeyframe currentKeyframe;
		public bool KeyframeSelected {get{return currentKeyframe != null;}}

		//textures
		private Texture2D TimeIndicatorIcon;
		private Texture2D KeyframeIcon;
		private Texture2D PlayButtonNormal;
		private Texture2D PlayButtonHover;
		private Texture2D PlayButtonActive;

		//gui values
		private Color SelectedKeyframeColor;
		private Color KeyframeColor;
		private Color ActiveTimeIndicatorColor;
		private Color GreyedTimeIndicatorColor;
		private float timeIndicatorTime = 0f;
		private string tooltip = "";

		private Rect timelineRect;
		private Rect selectedKeyframeRect;
		private List<Rect> otherKeyframeRects = new List<Rect>();
		private Rect timeIndicatorRect;
		private Rect timeIndicatorSliderRect;
		private Rect addKeyframeRect;
		private Rect copyKeyframeRect;
		private Rect moveKeyframeRect;
		private Rect deleteKeyframeRect;
		private Rect resetKeyframeRect;

		private SelectedBone previousSelectedBone;
		private KerbalAnimationClip.KerbalKeyframe previousSelectedKeyframe;

		//animation properties window
		public AnimationPropertiesWindow Properties;
		public bool AnimationPropertiesOpen = false;

		// Load animation dialog
		public AnimationLoadWindow animationLoad;
		private bool AnimationLoadOpen = false;


		//gui styles
		private GUIStyle centeredText;
		private GUIStyle timelineStyle;
		private GUIStyle timeIndicatorSlider;
		private GUIStyle timeIndicatorSliderThumb;
		private GUIStyle keyframeButton;
		private void SetupGUIStyles()
		{
			centeredText = new GUIStyle(skin.label);
			centeredText.alignment = TextAnchor.MiddleCenter;

			timelineStyle = new GUIStyle(skin.horizontalSlider);
			timelineStyle.fixedHeight = 22f;

			Texture2D empty = new Texture2D(1, 1, TextureFormat.Alpha8, false);
			empty.SetPixel(0, 0, Color.clear);
			empty.Apply();

			timeIndicatorSlider = new GUIStyle(skin.horizontalSlider);
			timeIndicatorSliderThumb = new GUIStyle(skin.horizontalSliderThumb);
			timeIndicatorSlider.normal.background = empty;
			timeIndicatorSlider.active.background = empty;
			timeIndicatorSlider.focused.background = empty;
			timeIndicatorSlider.hover.background = empty;
			timeIndicatorSliderThumb.normal.background = empty;
			timeIndicatorSliderThumb.active.background = empty;
			timeIndicatorSliderThumb.focused.background = empty;
			timeIndicatorSliderThumb.hover.background = empty;
			keyframeButton = new GUIStyle(skin.button);
			keyframeButton.normal.background = empty;
			keyframeButton.active.background = empty;
			keyframeButton.focused.background = empty;
			keyframeButton.hover.background = empty;
		}

		//Events
		private void OnNewAnimationClip(EditableAnimationClip clip)
		{
			if (clip != null)
			{
				//set defaults as necesssary
				clip.WrapMode = WrapMode.ClampForever;
				if (clip.Duration == 0) clip.Duration = 1.0f;

				UpdateAnimationClip();
			}
		}

        public void OnEnableSuite()
        {
			AnimationPropertiesOpen = false;
			AnimationLoadOpen = false;
			Suite.CurrentBone = null;
			SetCurrentKeyframe(null, false);
		}

        //draw callbacks
        private void DrawProperties()
		{
			if (AnimationPropertiesOpen)
			{
				Properties.Draw();
			}
		}

		private void DrawAnimationLoad()
		{
			if (AnimationLoadOpen)
			{
				animationLoad.Draw();
			}
		}

		protected override void DrawWindow()
		{
			if (AnimationLoadOpen) GUI.enabled = false;

			//utils
			var mousePos = Event.current.mousePosition;

			if (animationClip.Keyframes.Count > 0)
			{
				//Timeline
				GUILayout.Label("<b><color=" + Colors.Orange + ">Timeline - " + animationClip.Name + "</color></b>", centeredText, GUILayout.ExpandWidth(true));
				GUILayout.Space(10f);

				// This button should only be enabled when the animation is not playing
				GUILayout.BeginHorizontal();
				if (Suite.Kerbal.IsAnimationPlaying) GUI.enabled = false;

				//button toolbar
				if (GUILayout.Button("Add Keyframe Here", GUILayout.ExpandWidth(false)))
				{
					SelectedBone oldBone = Suite.CurrentBone;
					Suite.CurrentBone = null;

					Debug.Log("creating new keyframe at " + timeIndicatorTime);
					// This will save the old keyframe with the proper transform state
					if (currentKeyframe != null)
					{
						animationClip.SetAnimationTime(currentKeyframe.NormalizedTime);
						SetCurrentKeyframe(null);
					}

					// Ensure that the new keyframe is an interpolation at the indicator's time
					animationClip.SetAnimationTime(timeIndicatorTime);
					var keyframe = animationClip.CreateKeyframe();
					keyframe.Write(Suite.Kerbal.transform, timeIndicatorTime);
					UpdateAnimationClip();

					Suite.CurrentBone = oldBone;
					SetCurrentKeyframe(keyframe);
				}
				addKeyframeRect = GUILayoutUtility.GetLastRect();

				GUILayout.Space(10f);

				// These buttons should only be enabled when a keyframe is selected
				if (currentKeyframe == null) GUI.enabled = false;

				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				if (GUILayout.Button("Copy Keyframe Here", GUILayout.ExpandWidth(true)))
				{
					SelectedBone oldBone = Suite.CurrentBone;
					Suite.CurrentBone = null;

					Debug.Log("copying keyframe at " + currentKeyframe.NormalizedTime + " to " + timeIndicatorTime);
					var srcKeyFrame = currentKeyframe;
					// This will save the old keyframe with the proper transform state
					animationClip.SetAnimationTime(currentKeyframe.NormalizedTime);
					SetCurrentKeyframe(null); 

					// Create a new keyframe In the original's position
					var keyframe = animationClip.CreateKeyframe();
					keyframe.WriteCopy(Suite.Kerbal.transform, srcKeyFrame, srcKeyFrame.NormalizedTime);
					// Move it to the indicator
					keyframe.NormalizedTime = timeIndicatorTime;
					animationClip.SetAnimationTime(timeIndicatorTime);
					UpdateAnimationClip();

					Suite.CurrentBone = oldBone;
					SetCurrentKeyframe(keyframe);
				}
				copyKeyframeRect = GUILayoutUtility.GetLastRect();
				if (GUILayout.Button("Move Keyframe Here", GUILayout.ExpandWidth(true)))
				{
					Debug.Log("moving keyframe at " + currentKeyframe.NormalizedTime + " to " + timeIndicatorTime);
					currentKeyframe.NormalizedTime = timeIndicatorTime; //set the time to the indicator's time
					animationClip.SetAnimationTime(timeIndicatorTime);
					UpdateAnimationClip();
				}
				moveKeyframeRect = GUILayoutUtility.GetLastRect();
				GUILayout.EndHorizontal();

				GUILayout.EndHorizontal();

				GUI.enabled = true;

				GUILayout.Space(25f);

				GUILayout.BeginHorizontal(timelineStyle);
				GUILayout.EndHorizontal();
				timelineRect = GUILayoutUtility.GetLastRect();

				//refresh keyframes rects
				otherKeyframeRects.Clear();
				if (!KeyframeSelected) selectedKeyframeRect = default(Rect);

				//draw keyframes on timeline
				foreach (var keyframe in animationClip.Keyframes)
				{
					Color keyframeColor = keyframe == currentKeyframe ? SelectedKeyframeColor : KeyframeColor;
					Rect keyframeRect = new Rect((timelineRect.xMin + (keyframe.NormalizedTime * (timelineRect.width - 20f))), timelineRect.yMin, 20f, 20f);

					GUI.color = keyframeColor;
					GUI.DrawTexture(keyframeRect, KeyframeIcon);
					GUI.color = Color.white;

					//register keyframe rects for the tooltips
					if (keyframe == currentKeyframe)
					{
						selectedKeyframeRect = keyframeRect;
					}
					else
					{
						otherKeyframeRects.Add(keyframeRect);
					}

					//disallow keyframe selection if the animation is playing
					if (Suite.Kerbal.IsAnimationPlaying) continue;

					if (GUI.Button(keyframeRect, "", keyframeButton))
					{
						if (keyframe == currentKeyframe)
						{
							SetCurrentKeyframe(null); //deselect current keyframe
						}
						else
						{
							SetCurrentKeyframe(keyframe); //select other keyframe
						}
					}
				}

				//draw time indicator
				float tempTimeIndicatorPosition = Suite.Kerbal.IsAnimationPlaying ? animationClip.GetAnimationTime() : timeIndicatorTime;

				timeIndicatorRect = new Rect((timelineRect.xMin + (tempTimeIndicatorPosition * (timelineRect.width - 20f))), timelineRect.yMin - 23f, 20f, 20f);
				timeIndicatorSliderRect = new Rect(timelineRect.xMin, timelineRect.yMin - 23f, timelineRect.width, 16f);
				// Texture is drawn on later

				//only add the invisible slider when the animation is not playing
				if (!Suite.Kerbal.IsAnimationPlaying)
				{
					timeIndicatorTime = GUI.HorizontalSlider(timeIndicatorSliderRect, timeIndicatorTime, 0f, 1f, timeIndicatorSlider, timeIndicatorSliderThumb);
				}

				//only allow the window to be dragged if the mouse is not over certain components
				AllowDrag = !((timelineRect.Contains(mousePos) || timeIndicatorRect.Contains(mousePos) || timeIndicatorSliderRect.Contains(mousePos)));

				//only set the time when the animation is not playing, and no keyframe is selected
				if (!Suite.Kerbal.IsAnimationPlaying && !KeyframeSelected)
				{
					animationClip.SetAnimationTime(timeIndicatorTime);
				}

				GUILayout.Space(20f);

				// Keyframe-specific editor options should only be enabled when the animation is not playing and a keyframe is selected
				if ((Suite.Kerbal.IsAnimationPlaying) || (currentKeyframe == null)) GUI.enabled = false;

				//button toolbar
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Delete Keyframe", GUILayout.ExpandWidth(false)) || Input.GetKeyDown(KeyCode.Delete))
				{
					Debug.Log("deleting keyframe at " + currentKeyframe.NormalizedTime);
					animationClip.RemoveKeyframe(currentKeyframe); //remove selected keyframe
					UpdateAnimationClip();
					SetCurrentKeyframe(null, false);
				}
				deleteKeyframeRect = GUILayoutUtility.GetLastRect();

				GUILayout.Space(10f);

				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

				if (GUILayout.Button("Reset Keyframe Manipulations", GUILayout.ExpandWidth(false)))
				{
					Debug.Log("Reseting keyframe at " + currentKeyframe.NormalizedTime);
					var defaultFrame = animationClip.getDefaultFrame();
					if (defaultFrame != null)
                    {
						SelectedBone oldBone = Suite.CurrentBone;
						Suite.CurrentBone = null;

						var keyframeTime = currentKeyframe.NormalizedTime;
						animationClip.RemoveKeyframe(currentKeyframe); //remove selected keyframe
						UpdateAnimationClip();

						var keyframe = animationClip.CreateKeyframe(); //create and write it at the current keyframe's time
						keyframe.WriteCopy(Suite.Kerbal.transform, defaultFrame, keyframeTime);
						UpdateAnimationClip();
						SetCurrentKeyframe(keyframe);

						Suite.CurrentBone = oldBone;
					}
					else
                    {
						Debug.LogError("Error reseting keyframe, no default frame");
					}
				}
				resetKeyframeRect = GUILayoutUtility.GetLastRect();

				GUILayout.EndHorizontal();

				GUILayout.EndHorizontal();

				GUI.enabled = true;

				//tooltips
				if (addKeyframeRect.Contains(mousePos)) tooltip = "Adds a <color=" + Colors.KeyframeColor + ">new keyframe</color> at the <color=" + Colors.Orange + ">Time Indicator's</color> position";
				else if (copyKeyframeRect.Contains(mousePos)) tooltip = "Adds a <color=" + Colors.KeyframeColor + ">new keyframe</color> identical to the <color=" + Colors.SelectedKeyframeColor + ">selected keyframe</color> at the <color=" + Colors.Orange + ">Time Indicator's</color> position";
				else if (moveKeyframeRect.Contains(mousePos)) tooltip = "Moves the <color=" + Colors.SelectedKeyframeColor + ">selected keyframe</color> to the <color=" + Colors.Orange + ">Time Indicator's</color> position";
				else if (deleteKeyframeRect.Contains(mousePos)) tooltip = "Deletes the <color=" + Colors.SelectedKeyframeColor + ">selected keyframe</color>";
				else if (resetKeyframeRect.Contains(mousePos)) tooltip = "Resets all manipulations for the <color=" + Colors.SelectedKeyframeColor + ">selected keyframe</color> to default";
				else if (timeIndicatorRect.Contains(mousePos)) tooltip = "The <color=" + Colors.Orange + ">Time Indicator</color>";
				else if (timelineRect.Contains(mousePos)) tooltip = "The <color=" + Colors.Orange + ">Timeline</color>";
				else if (otherKeyframeRects.Where(r => r.Contains(mousePos)).Count() > 0) tooltip = "A <color=" + Colors.KeyframeColor + ">keyframe</color>. Click it to select it";
				else if (selectedKeyframeRect.Contains(mousePos)) tooltip = "The <color=" + Colors.SelectedKeyframeColor + ">selected keyframe</color>. Click it to deselect it";
				else tooltip = "";

				GUILayout.Space(2f);
				GUILayout.BeginVertical(skin.box);
				GUILayout.Label("<color=" + Colors.Information + ">" + tooltip + "</color>");
				GUILayout.EndVertical();

				// Render the time indicator with proper color
				if (addKeyframeRect.Contains(mousePos))
				{
					if (Suite.Kerbal.IsAnimationPlaying)
					{
						GUI.color = KeyframeColor;
					}
					else
                    {
						GUI.color = ActiveTimeIndicatorColor;
					}
					GUI.DrawTexture(timeIndicatorRect, TimeIndicatorIcon);
				}
				else if (copyKeyframeRect.Contains(mousePos) || moveKeyframeRect.Contains(mousePos))
                {
					if (Suite.Kerbal.IsAnimationPlaying || (currentKeyframe == null))
					{
						GUI.color = KeyframeColor;
					}
					else
					{
						GUI.color = ActiveTimeIndicatorColor;
					}
					GUI.DrawTexture(timeIndicatorRect, TimeIndicatorIcon);
				}
				else if (deleteKeyframeRect.Contains(mousePos) || resetKeyframeRect.Contains(mousePos))
				{
					if (Suite.Kerbal.IsAnimationPlaying || (currentKeyframe == null))
					{
						GUI.color = KeyframeColor;
					}
					else
					{
						GUI.color = GreyedTimeIndicatorColor;
					}
					GUI.DrawTexture(timeIndicatorRect, TimeIndicatorIcon);
				}
				else
				{
					if (Suite.Kerbal.IsAnimationPlaying || (currentKeyframe == null))
                    {
						GUI.color = KeyframeColor;
					}
					GUI.DrawTexture(timeIndicatorRect, TimeIndicatorIcon);
				}
				GUI.color = Color.white;

				if (!Suite.Kerbal.IsAnimationPlaying && GUILayout.Button("Play Animation"))
				{
					// Save the selection so it can be reselected later
					previousSelectedBone = Suite.CurrentBone;
					previousSelectedKeyframe = currentKeyframe;

					Suite.CurrentBone = null;
					SetCurrentKeyframe(null);
					animationClip.WrapMode = WrapMode.Loop;
					UpdateAnimationClip();
					animationClip.Play();
				}
				else if (Suite.Kerbal.IsAnimationPlaying && GUILayout.Button("Stop"))
				{
					animationClip.Stop();
					animationClip.WrapMode = WrapMode.ClampForever;
					UpdateAnimationClip();

					// Reselect the previous selection
					SetCurrentKeyframe(previousSelectedKeyframe);
					Suite.CurrentBone = previousSelectedBone;
					previousSelectedKeyframe = null;
					previousSelectedBone = null;
				}
				GUILayout.Space(10f);

				// These buttons are only enabled if animation is not playing
				if (Suite.Kerbal.IsAnimationPlaying) GUI.enabled = false;

				AnimationPropertiesOpen = GUILayout.Toggle(AnimationPropertiesOpen, "Show Properties", skin.button);

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Save"))
				{
					Directory.CreateDirectory(ExportFullPath);
					animationClip.Save(ExportURL);
					animationLoad.UpdateAnimations();
				}
				GUILayout.EndHorizontal();
			}
			else
            {
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("New Animation"))
                {
					timeIndicatorTime = 0;
					animationClip.SetAnimationTime(0);
					animationClip.saveDefaultFrame(Suite.Kerbal.transform);
					Debug.Log("New animation, creating new keyframe at start");
					var keyframe0 = animationClip.CreateKeyframe(); // create and write it at the start
					keyframe0.Write(Suite.Kerbal.transform, 0);
					UpdateAnimationClip();

					animationClip.SetAnimationTime(1);
					Debug.Log("New animation, creating new keyframe at end");
					var keyframe1 = animationClip.CreateKeyframe(); // create and write it at the end
					keyframe1.Write(Suite.Kerbal.transform, 1);
					UpdateAnimationClip();

					animationClip.SetAnimationTime(0);
					SetCurrentKeyframe(keyframe0, false);
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				AnimationLoadOpen = GUILayout.Toggle(AnimationLoadOpen, "Load Animation", skin.button);
				GUILayout.EndHorizontal();
			}

			if (AnimationLoadOpen) GUI.enabled = true;
		}

		public override void Update()
		{
			//only have a current bone if there is a keyframe selected
			if (!KeyframeSelected && Suite.CurrentBone != null) Suite.CurrentBone = null;

			//update properties
			Properties.Update();
		}

		public void loadAnimation(string loadURL)
        {
			try
			{
				if (!animationClip.Load(loadURL))
				{
					Debug.LogError("failed to load animation from " + loadURL);
				}
				else
				{
					animationClip.saveDefaultFrame(Suite.Kerbal.transform);
					Suite.OnNewAnimationClip.Fire(animationClip);
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Caught exception while loading animation: " + e.GetType());
				Debug.LogException(e);
				//reset clip to erase any damage done when loading
				Suite.AnimationClip = new EditableAnimationClip(Suite.Kerbal);
			}
		}

		private void SetCurrentKeyframe(KerbalAnimationClip.KerbalKeyframe keyframe, bool saveOld = true)
		{
			//save old keyframe
			if (currentKeyframe != null && saveOld)
			{
				currentKeyframe.Write(Suite.Kerbal.transform, currentKeyframe.NormalizedTime);
				UpdateAnimationClip();
			}

			//set new keyframe
			if (keyframe != null)
			{
				SelectedBone oldBone = Suite.CurrentBone;
				Suite.CurrentBone = null;
				currentKeyframe = keyframe;
				animationClip.SetAnimationTime(keyframe.NormalizedTime);
				timeIndicatorTime = keyframe.NormalizedTime;
				Suite.CurrentBone = oldBone;
			}
			else
			{
				currentKeyframe = null;
			}
		}
		private void UpdateAnimationClip()
		{
			animationClip.BuildAnimationClip();
			animationClip.Initialize();
			DebugClip();
		}

		private void DebugClip()
		{
			Debug.Log("Clip info follows:");
			Debug.Log("Name: " + animationClip.Name);
			Debug.Log("Duration: " + animationClip.Duration);
			Debug.Log("Layer: " + animationClip.Layer);
			Debug.Log("Clip.length: " + animationClip.Clip.length);
			Debug.Log("Keyframes.Count: " + animationClip.Keyframes.Count);
			foreach (var keyframe in animationClip.Keyframes)
			{
				Debug.Log("Keyframe - NormalizedTime: " + keyframe.NormalizedTime);
			}
		}
	}
}

