using System;
using UnityEngine;

namespace KerbalAnimation
{
	public class MasterWindow : Window
	{
		public MasterWindow()
		{
			AllowDrag = false;
			ExpandHeight = true;
			WindowRect = new Rect(Screen.width - 320f, 5f, 250f, 0f); //height is zero because it expands
			WindowTitle = "Kerbal Animation Suite";
		}

		public bool AnimationOpen = true;
		public bool ManipulationOpen = true;
		public bool HierarchyOpen = true;

		protected override void DrawWindow()
		{
			GUILayout.BeginVertical(skin.scrollView);
			GUILayout.Space(10f);

			AnimationOpen = GUILayout.Toggle(AnimationOpen, "Animation", skin.button);
			if (!Suite.Kerbal.IsAnimationPlaying && Suite.Animation.KeyframeSelected)
			{
				HierarchyOpen = GUILayout.Toggle(HierarchyOpen, "Bone Hierarchy", skin.button);
			}
			else
			{
				GUI.enabled = false;
				GUILayout.Toggle(false, "Bone Hierarchy", skin.button);
				GUI.enabled = true;
			}
			if (!Suite.Kerbal.IsAnimationPlaying && Suite.Animation.KeyframeSelected)
			{
				ManipulationOpen = GUILayout.Toggle(ManipulationOpen, "Manipulation", skin.button);
			}
			else
			{
				GUI.enabled = false;
				GUILayout.Toggle(false, "Manipulation", skin.button);
				GUI.enabled = true;
			}
			GUILayout.Space(3f);

			Suite.Settings.AllowEditorMusic = GUILayout.Toggle(Suite.Settings.AllowEditorMusic, "Play Music?");

			GUILayout.EndVertical();
		}
		public override void Update()
		{

		}
	}
}

