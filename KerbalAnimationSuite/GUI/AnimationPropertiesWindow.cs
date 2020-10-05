using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalAnimation
{
	public class AnimationPropertiesWindow : Window
	{
		//constructor
		public AnimationPropertiesWindow()
		{
			WindowTitle = "Animation Properties";
			WindowRect = new Rect(610f, Screen.height - 250f, 250f, 0f);
			ExpandHeight = true;

			addButtonColor = Colors.HexToColor(Colors.AddButtonColor);
			removeButtonColor = Colors.HexToColor(Colors.RemoveButtonColor);

			Suite.OnNewAnimationClip.Add(OnNewAnimationClip);
		}

		//gui values
		private Vector2 mixingTransformsScroll;
		private string mixingTransformAddName = "";
		private string mixingTransformAddError = "";
		private Dictionary<string, string> textBoxValues = new Dictionary<string, string>();
		private Dictionary<string, Color> textBoxColors = new Dictionary<string, Color>();
		private Color addButtonColor;
		private Color removeButtonColor;

		//Events
		private void OnNewAnimationClip(EditableAnimationClip clip)
		{
			if (clip == null)
				return;

			Debug.Log("OnNewAnimationClip! " + clip.Name);

			//set gui values
			if (textBoxValues.ContainsKey("DurationNumberSelector"))
				textBoxValues ["DurationNumberSelector"] = clip.Duration.ToString("####0.0##");
			else
				textBoxValues.Add("DurationNumberSelector", clip.Duration.ToString("####0.0##"));
			if (textBoxValues.ContainsKey("LayerNumberSelector"))
				textBoxValues ["LayerNumberSelector"] = clip.Layer.ToString("####0");
			else
				textBoxValues.Add("LayerNumberSelector", clip.Layer.ToString("####0"));
		}

		protected override void DrawWindow()
		{
			if (Suite.AnimationClip == null)
			{
				GUILayout.Label("<color=" + Colors.ErrorMessageColor + ">ERROR: AnimationClip is null!");
				return;
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label("Name: ", GUILayout.Width(70f));
			Suite.AnimationClip.Name = GUILayout.TextField(Suite.AnimationClip.Name, GUILayout.Width(200f));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Duration: ", GUILayout.Width(70f));
			Suite.AnimationClip.Duration = DrawNumberSelector("DurationNumberSelector", Suite.AnimationClip.Duration, 0f, 10000f, 0.1f, 1f);
			GUILayout.EndHorizontal();
			GUILayout.Space(2f);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Layer: ", GUILayout.Width(70f));
			Suite.AnimationClip.Layer = DrawNumberSelector("LayerNumberSelector", Suite.AnimationClip.Layer, 0, 100, 1);
			GUILayout.EndHorizontal();

			GUILayout.Space(12f);

			GUILayout.Label("<b>Mixing Transforms:</b>");
			mixingTransformsScroll = GUILayout.BeginScrollView(mixingTransformsScroll, GUILayout.MaxHeight(90f), GUILayout.MinHeight(40f));
			GUILayout.Space(3f);
			foreach (var mt in Suite.AnimationClip.MixingTransforms)
			{
				GUILayout.Space(2f);
				GUILayout.BeginHorizontal(skin.box);
				GUILayout.Label("<color=" + Colors.Information + ">" + mt + "</color>", GUILayout.Height(22f));
				GUI.backgroundColor = removeButtonColor;
				if (GUILayout.Button("<b>-</b>", GUILayout.Height(24f), GUILayout.Width(24f)))
				{
					Suite.AnimationClip.RemoveMixingTransform(mt);
				}
				GUI.backgroundColor = Color.white;
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
			GUILayout.BeginHorizontal();
			mixingTransformAddName = GUILayout.TextField(mixingTransformAddName, GUILayout.ExpandWidth(true));
			GUI.backgroundColor = addButtonColor;
			if (GUILayout.Button("<b>+</b>", GUILayout.Height(24f), GUILayout.Width(24f)))
			{
				if (mixingTransformAddName == null || mixingTransformAddName == "")
				{
					//do nothing
				}
				else
				{
					if (!Suite.AnimationNames.ContainsKey(mixingTransformAddName))
					{
						mixingTransformAddError = "Bone " + mixingTransformAddName + " does not exist.";
						mixingTransformAddName = "";
					}
					else
					{
						try
						{
							Suite.AnimationClip.AddMixingTransform(mixingTransformAddName);
							mixingTransformAddError = "";
							mixingTransformAddName = "";
						}
						catch(Exception e)
						{
							mixingTransformAddError = "ERROR: " + e.GetType().Name;
							mixingTransformAddName = "";
						}
					}
				}
			}
			GUI.backgroundColor = Color.white;
			GUILayout.EndHorizontal();

			//draw error message if needed
			if (mixingTransformAddError != null && mixingTransformAddError != "")
			{
				GUILayout.BeginVertical(skin.box);
				GUILayout.Label("<color=" + Colors.ErrorMessageColor + ">" + mixingTransformAddError + "</color>");
				GUILayout.EndVertical();
			}
		}
		public override void Update()
		{

		}

		//gui utils
		private float DrawNumberSelector(string uniqueName, float value, float min, float max, float increment, float shiftIncrement = 0f)
		{
			if (!textBoxValues.ContainsKey(uniqueName))
				textBoxValues.Add(uniqueName, value.ToString());
			if (!textBoxColors.ContainsKey(uniqueName))
				textBoxColors.Add(uniqueName, Color.white);

			if (shiftIncrement <= 0f)
				shiftIncrement = increment;

			string textBoxControlName = "NumberSelector_" + uniqueName;

			GUILayout.BeginHorizontal();

			float buttonValue = value;
			bool buttonPressed = false;
			float buttonIncrement = Event.current.shift ? shiftIncrement : increment;
			if (GUILayout.Button("<<", GUILayout.MaxWidth(40f), GUILayout.Height(24f)))
			{
				buttonValue -= buttonIncrement;
				buttonPressed = true;
			}

			GUI.SetNextControlName(textBoxControlName);
			GUI.color = textBoxColors [uniqueName];
			textBoxValues [uniqueName] = GUILayout.TextField(textBoxValues [uniqueName], GUILayout.Width(80f), GUILayout.ExpandWidth(false));
			GUI.color = Color.white;

			if (GUILayout.Button(">>", GUILayout.MaxWidth(40f), GUILayout.Height(24f)))
			{
				buttonValue += buttonIncrement;
				buttonPressed = true;
			}
			if (buttonPressed)
			{
				buttonValue = Mathf.Clamp(buttonValue, min, max);
				textBoxValues [uniqueName] = buttonValue.ToString("####0.0##");
				GUI.FocusControl("");
			}

			float textBoxNumber = buttonValue;
			if (GUI.GetNameOfFocusedControl() == textBoxControlName)
			{
				if (!float.TryParse(textBoxValues [uniqueName], out textBoxNumber))
					textBoxColors [uniqueName] = Color.red;
				else
					textBoxColors [uniqueName] = Color.white;
			}

			if (textBoxNumber < min || textBoxNumber > max)
			{
				textBoxColors [uniqueName] = Color.red;
			}

			GUILayout.EndHorizontal();

			return textBoxNumber;
		}
		private int DrawNumberSelector(string uniqueName, int value, int min, int max, int increment, int shiftIncrement = 0)
		{
			if (!textBoxValues.ContainsKey(uniqueName))
				textBoxValues.Add(uniqueName, value.ToString());
			if (!textBoxColors.ContainsKey(uniqueName))
				textBoxColors.Add(uniqueName, Color.white);

			if (shiftIncrement <= 0)
				shiftIncrement = increment;

			string textBoxControlName = "NumberSelectorInt_" + uniqueName;

			GUILayout.BeginHorizontal();

			int buttonValue = value;
			bool buttonPressed = false;
			int buttonIncrement = Event.current.shift ? shiftIncrement : increment;
			if (GUILayout.Button("<<", GUILayout.MaxWidth(40f), GUILayout.Height(24f)))
			{
				buttonValue -= buttonIncrement;
				buttonPressed = true;
			}

			GUI.SetNextControlName(textBoxControlName);
			GUI.color = textBoxColors [uniqueName];
			textBoxValues [uniqueName] = GUILayout.TextField(textBoxValues [uniqueName], GUILayout.Width(80f), GUILayout.ExpandWidth(false));
			GUI.color = Color.white;

			if (GUILayout.Button(">>", GUILayout.MaxWidth(40f), GUILayout.Height(24f)))
			{
				buttonValue += buttonIncrement;
				buttonPressed = true;
			}
			if (buttonPressed)
			{
				buttonValue = Mathf.Clamp(buttonValue, min, max);
				textBoxValues [uniqueName] = buttonValue.ToString("####0");
				GUI.FocusControl("");
			}

			int textBoxNumber = buttonValue;
			if (GUI.GetNameOfFocusedControl() == textBoxControlName)
			{
				if (!int.TryParse(textBoxValues [uniqueName], out textBoxNumber))
					textBoxColors [uniqueName] = Color.red;
				else
					textBoxColors [uniqueName] = Color.white;
			}

			if (textBoxNumber < min || textBoxNumber > max)
			{
				textBoxColors [uniqueName] = Color.red;
			}

			GUILayout.EndHorizontal();

			return textBoxNumber;
		}
	}
}

