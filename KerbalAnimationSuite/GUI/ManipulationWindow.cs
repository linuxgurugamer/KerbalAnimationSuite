using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalAnimation
{
	public class ManipulationWindow : Window
	{
		public ManipulationWindow()
		{
			SetupGUIStyles();
			WindowRect = new Rect((Screen.width / 2f) + 600f, (Screen.height / 2f) - 300f, 500f, 300f);
			WindowTitle = "Manipulation";
			ExpandHeight = true;

			Suite.OnBoneSelected.Add(OnBoneSelected);
		}

		public SelectedBone Bone
		{
			get{return Suite.CurrentBone;}
			set{Suite.CurrentBone = value;}
		}

		public Vector3 Rotation;
		public Vector3 Position;

		//private GUI values
		//private Vector2 manipulationScroll;
		private Dictionary<string, string> textBoxValues = new Dictionary<string, string>();
		private Dictionary<string, Color> textBoxColors = new Dictionary<string, Color>();

		private Color sliderErrorColor = new Color(1f, 0f, 0f);
		private Color sliderFocusColor = new Color(1f, 1f, 1f);

		//gui styles
		private GUIStyle manipulationSliderStyle;
		private GUIStyle manipulationSliderThumbStyle;
		private void SetupGUIStyles()
		{
			//estimate the height of the text box
			float height = skin.textField.CalcHeight(new GUIContent("0.0"), 120f);
			int marginTop = skin.textField.margin.top;
			int marginBottom = skin.textField.margin.bottom;

			manipulationSliderStyle = new GUIStyle(skin.horizontalSlider);
			manipulationSliderStyle.fixedHeight = height;
			manipulationSliderStyle.margin.top = marginTop;
			manipulationSliderStyle.margin.bottom = marginBottom;
			manipulationSliderThumbStyle = new GUIStyle(skin.horizontalSliderThumb);
			manipulationSliderThumbStyle.fixedHeight = height;
		}

		public override void Update()
		{
			if (Suite.CurrentBone != null)
			{
				Suite.CurrentBone.Position = Position;
				Suite.CurrentBone.Rotation = Rotation;
			}
		}
		private void OnBoneSelected(SelectedBone bone)
		{
			if (bone == null)
			{
				Position = Vector3.zero;
				Rotation = Vector3.zero;
			}
			else
			{
				Position = bone.Position;
				Rotation = bone.Rotation;
			}
		}

		protected override void DrawWindow()
		{
			//manipulationScroll = GUILayout.BeginScrollView(manipulationScroll);

			if (Suite.CurrentBone != null)
            {
				WindowTitle = "Manipulation - " + (Suite.ReadableNames.ContainsKey(Suite.CurrentBone.Name) ? Suite.ReadableNames[Suite.CurrentBone.Name] : Suite.CurrentBone.Name);
			}
			else
            {
				WindowTitle = "Manipulation (No Bone Selected)";
				GUI.enabled = false;
			}

			GUILayout.Label("Rotation");
			Rotation.x = DrawManipulationSlider("RX", "X", Rotation.x, 0f, 360f);
			Rotation.y = DrawManipulationSlider("RY", "Y", Rotation.y, 0f, 360f);
			Rotation.z = DrawManipulationSlider("RZ", "Z", Rotation.z, 0f, 360f);
			GUILayout.Space(20f);

			GUILayout.Label("Relative Position");
			Position.x = DrawManipulationSlider("PX", "X", Position.x, -0.5f, 0.5f);
			Position.y = DrawManipulationSlider("PY", "Y", Position.y, -0.5f, 0.5f);
			Position.z = DrawManipulationSlider("PZ", "Z", Position.z, -0.5f, 0.5f);

			GUILayout.Space(10f);
			if (GUILayout.Button("Toggle Helmet"))
			{
				Suite.Kerbal.HasHelmet = !Suite.Kerbal.HasHelmet;
			}

			//GUILayout.EndScrollView();

			GUI.enabled = true;

			GUI.DragWindow();
		}

		private float DrawManipulationSlider(string uniqueName, string name, float value, float min, float max)
		{
			value = Mathf.Clamp(value, min, max);
			if (!textBoxValues.ContainsKey(uniqueName)) textBoxValues.Add(uniqueName, value.ToString());
			if (!textBoxColors.ContainsKey(uniqueName)) textBoxColors.Add(uniqueName, Color.white);

			GUILayout.BeginHorizontal();

			GUILayout.Label("<b><color=" + Colors.Orange + ">" + name + ":</color></b>");

			//gui focus stuff
			string textBoxControlName = "ManipulationTextBox_" + uniqueName;
			string sliderControlName = "ManipulationSlider_" + uniqueName;
			string focusedControl = GUI.GetNameOfFocusedControl();

			GUI.SetNextControlName(textBoxControlName);
			GUI.color = textBoxColors[uniqueName];
			textBoxValues[uniqueName] = GUILayout.TextField(textBoxValues[uniqueName], GUILayout.Width(120f));
			GUI.color = Color.white;

			//make the color slightly darker if it's focused
			if (focusedControl == textBoxControlName) textBoxColors[uniqueName] = sliderFocusColor;
			else textBoxColors[uniqueName] = Color.white;

			float parsedTextBoxFloat;
			bool parsedTextBox = true;
			if (!float.TryParse(textBoxValues[uniqueName], out parsedTextBoxFloat))
			{
				//make the color red if it fails to parse the text box
				textBoxColors[uniqueName] = sliderErrorColor;
				parsedTextBox = false;
			}
			if (parsedTextBox && (parsedTextBoxFloat < min || parsedTextBoxFloat > max))
			{
				//also make the color red if the value is not within the specified contraints
				textBoxColors[uniqueName] = sliderErrorColor;
				parsedTextBox = false;
			}

			float sliderValue = value;
			if (parsedTextBox && focusedControl == textBoxControlName)
			{
				sliderValue = parsedTextBoxFloat;
			}

			GUI.SetNextControlName(sliderControlName);
			sliderValue = GUILayout.HorizontalSlider(sliderValue, min, max, manipulationSliderStyle, manipulationSliderThumbStyle, GUILayout.ExpandWidth(true));

			//take focus off of the text box if it's focused, and the slider has been clicked
			Rect sliderRect = GUILayoutUtility.GetLastRect();
			if (focusedControl == textBoxControlName && sliderRect.Contains(Event.current.mousePosition) && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
			{
				GUI.FocusControl(sliderControlName);
			}

			if (focusedControl != textBoxControlName)
			{
				textBoxValues[uniqueName] = sliderValue.ToString("##0.0###");
			}

			GUILayout.EndHorizontal();

			return sliderValue;
		}
	}
}

