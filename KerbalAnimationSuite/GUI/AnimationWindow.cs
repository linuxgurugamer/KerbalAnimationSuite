using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

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

			//get rgb colors from hex colors in Colors class
			KeyframeColor = Colors.HexToColor(Colors.KeyframeColor);
			SelectedKeyframeColor = Colors.HexToColor(Colors.SelectedKeyframeColor);

			WindowRect = new Rect(0f, Screen.height - 300f, 600f, 0f);
			WindowTitle = "Animation";
			ExpandHeight = true;

			TimeIndicatorIcon = GameDatabase.Instance.GetTexture("KerbalAnimationSuite/Icons/timeline_arrow", false);
			if (TimeIndicatorIcon == null)
				TimeIndicatorIcon = Texture2D.whiteTexture;

			KeyframeIcon = GameDatabase.Instance.GetTexture("KerbalAnimationSuite/Icons/keyframe_icon", false);
			if (KeyframeIcon == null)
				KeyframeIcon = Texture2D.whiteTexture;

			PlayButtonNormal = GameDatabase.Instance.GetTexture("KerbalAnimationSuite/Icons/play_normal", false);
			if (PlayButtonNormal == null)
				PlayButtonNormal = Texture2D.whiteTexture;

			PlayButtonHover = GameDatabase.Instance.GetTexture("KerbalAnimationSuite/Icons/play_hover", false);
			if (PlayButtonHover == null)
				PlayButtonHover = Texture2D.whiteTexture;

			PlayButtonActive = GameDatabase.Instance.GetTexture("KerbalAnimationSuite/Icons/play_active", false);
			if (PlayButtonActive == null)
				PlayButtonActive = Texture2D.whiteTexture;

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
		private float timeIndicatorTime = 0f;
		private string tooltip = "";
		private string loadURL = "KerbalAnimationSuite/Presets/Bleh";

		private Rect timelineRect;
		private Rect selectedKeyframeRect;
		private List<Rect> otherKeyframeRects = new List<Rect>();
		private Rect timeIndicatorRect;
		private Rect timeIndicatorSliderRect;
		private Rect addKeyframeRect;
		private Rect copyKeyframeRect;
		private Rect moveKeyframeRect;
		private Rect deleteKeyframeRect;

		//animation properties window
		public AnimationPropertiesWindow Properties;
		public bool AnimationPropertiesOpen = false;

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
				//set defaults
				clip.WrapMode = WrapMode.ClampForever;
				clip.Duration = 1f;

				UpdateAnimationClip();
			}
		}

		//draw callbacks
		private void DrawProperties()
		{
			if (AnimationPropertiesOpen)
			{
				Properties.Draw();
			}
		}

		protected override void DrawWindow()
		{
			//utils
			var mousePos = Event.current.mousePosition;

			//Timeline
			GUILayout.Label("<b><color=" + Colors.Orange + ">Timeline</color></b>", centeredText, GUILayout.ExpandWidth(true));
			GUILayout.Space(25f);

			GUILayout.BeginHorizontal(timelineStyle);
			GUILayout.EndHorizontal();
			timelineRect = GUILayoutUtility.GetLastRect();

			//refresh keyframes rects
			otherKeyframeRects.Clear();
			if (!KeyframeSelected)
				selectedKeyframeRect = default(Rect);

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
				if (Suite.Kerbal.IsAnimationPlaying)
					continue;

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

			GUI.DrawTexture(timeIndicatorRect, TimeIndicatorIcon);

			//only add the invisible slider when the animation is not playing
			if (!Suite.Kerbal.IsAnimationPlaying)
			{
				timeIndicatorTime = GUI.HorizontalSlider(timeIndicatorSliderRect, timeIndicatorTime, 0f, 1f, timeIndicatorSlider, timeIndicatorSliderThumb);
			}

			//only allow the window to be dragged if the mouse is not over certain components
			AllowDrag = !((timelineRect.Contains(mousePos) || timeIndicatorRect.Contains(mousePos) || timeIndicatorSliderRect.Contains(mousePos)));

			//only set the time when the animation is not playing, and there is no selected keyframe
			if (!Suite.Kerbal.IsAnimationPlaying && !KeyframeSelected)
			{
				animationClip.SetAnimationTime(timeIndicatorTime);
			}

			GUILayout.Space(20f);

			//editor should only be displayed when the animation is not playing
			if (!Suite.Kerbal.IsAnimationPlaying)
			{
				//button toolbar
				GUILayout.BeginHorizontal();

				if (GUILayout.Button("Add Keyframe", GUILayout.ExpandWidth(false)))
				{
					Debug.Log("creating new keyframe at " + timeIndicatorTime);
					var keyframe = animationClip.CreateKeyframe(timeIndicatorTime); //create and write it at the time indicator's time
					keyframe.Write(Suite.Kerbal.transform, timeIndicatorTime);
					UpdateAnimationClip();
					SetCurrentKeyframe(keyframe);
				}
				addKeyframeRect = GUILayoutUtility.GetLastRect();

				GUILayout.Space(10f);

				if (currentKeyframe != null)
				{
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					if (GUILayout.Button("Copy Keyframe", GUILayout.ExpandWidth(true)))
					{
						Debug.Log("copying keyframe at " + currentKeyframe.NormalizedTime + " to " + timeIndicatorTime);
						var keyframe = animationClip.CreateKeyframe(currentKeyframe.NormalizedTime); //create and write it at the current keyframe's time, then move it
						keyframe.Write(Suite.Kerbal.transform, currentKeyframe.NormalizedTime);
						keyframe.NormalizedTime = timeIndicatorTime;
						UpdateAnimationClip();
						SetCurrentKeyframe(keyframe);
					}
					copyKeyframeRect = GUILayoutUtility.GetLastRect();
					if (GUILayout.Button("Move Keyframe", GUILayout.ExpandWidth(true)))
					{
						Debug.Log("moving keyframe at " + currentKeyframe.NormalizedTime + " to " + timeIndicatorTime);
						currentKeyframe.NormalizedTime = timeIndicatorTime; //set the time to the indicator's time
						UpdateAnimationClip();
					}
					moveKeyframeRect = GUILayoutUtility.GetLastRect();
					if (GUILayout.Button("Delete Keyframe", GUILayout.ExpandWidth(true)) || Input.GetKeyDown(KeyCode.Delete))
					{
						Debug.Log("deleting keyframe at " + currentKeyframe.NormalizedTime);
						animationClip.RemoveKeyframe(currentKeyframe); //remove selected keyframe
						UpdateAnimationClip();
						SetCurrentKeyframe(null, false);
					}
					deleteKeyframeRect = GUILayoutUtility.GetLastRect();
					GUILayout.EndHorizontal();
				}

				GUILayout.EndHorizontal();

				//tooltips
				if (addKeyframeRect.Contains(mousePos))
					tooltip = "Adds a new keyframe at the <color=" + Colors.Orange + ">Time Indicator's</color> position";
				else if (copyKeyframeRect.Contains(mousePos))
					tooltip = "Adds a new keyframe identical to the selected one at the <color=" + Colors.Orange + ">Time Indicator's</color> position";
				else if (moveKeyframeRect.Contains(mousePos))
					tooltip = "Moves the selected keyframe to the <color=" + Colors.Orange + ">Time Indicator's</color> position";
				else if (deleteKeyframeRect.Contains(mousePos))
					tooltip = "Deletes the selected keyframe";
				else if (timeIndicatorRect.Contains(mousePos))
					tooltip = "The <color=" + Colors.Orange + ">Time Indicator</color>";
				else if (timelineRect.Contains(mousePos))
					tooltip = "The <color=" + Colors.Orange + ">Timeline</color>";
				else if (otherKeyframeRects.Where(r => r.Contains(mousePos)).Count() > 0)
					tooltip = "A <color=" + Colors.KeyframeColor + ">keyframe</color>. Click it to select it";
				else if (selectedKeyframeRect.Contains(mousePos))
					tooltip = "The <color=" + Colors.SelectedKeyframeColor + ">selected keyframe</color>. Click it to deselect it";
				else
					tooltip = "";

				GUILayout.Space(2f);
				GUILayout.BeginVertical(skin.box);
				GUILayout.Label("<color=" + Colors.Information + ">" + tooltip + "</color>");
				GUILayout.EndVertical();
			}

			if (!Suite.Kerbal.IsAnimationPlaying && GUILayout.Button("Play"))
			{
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
			}
			GUILayout.Space(10f);

			//only draw if animation is not playing
			if (!Suite.Kerbal.IsAnimationPlaying)
			{
				AnimationPropertiesOpen = GUILayout.Toggle(AnimationPropertiesOpen, "Properties", skin.button);

				GUILayout.BeginHorizontal();
				if (animationClip.Keyframes.Count <= 0)
				{
					if (GUILayout.Button("Load", GUILayout.Width(160f)))
					{
						try
						{
							if (!animationClip.Load(loadURL))
							{
								Debug.LogError("failed to load animation from " + loadURL);
							}
							else
								Suite.OnNewAnimationClip.Fire(animationClip);
						}
						catch(Exception e)
						{
							Debug.LogError("Caught exception while loading animation: " + e.GetType());
							Debug.LogException(e);
							//reset clip to erase any damage done when loading
							Suite.AnimationClip = new EditableAnimationClip(Suite.Kerbal);
						}
					}
					GUILayout.Label("<b>Load from:</b> <color=" + Colors.Information + "> GameData/</color>", GUILayout.ExpandWidth(true));
					loadURL = GUILayout.TextField(loadURL);
				}
				else
				{
					if (GUILayout.Button("Save"))
					{
						Directory.CreateDirectory(ExportFullPath);
						animationClip.Save(ExportURL);
					}
				}
				GUILayout.EndHorizontal();
			}
		}

		public override void Update()
		{
			//only have a current bone if there is a keyframe selected
			if (!KeyframeSelected && Suite.CurrentBone != null)
				Suite.CurrentBone = null;

			//update properties
			Properties.Update();
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
				currentKeyframe = keyframe;
				animationClip.SetAnimationTime(keyframe.NormalizedTime);
				timeIndicatorTime = keyframe.NormalizedTime;
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

