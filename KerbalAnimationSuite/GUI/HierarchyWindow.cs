using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalAnimation
{
	public class HierarchyWindow : Window
	{
		//constructor
		public HierarchyWindow()
		{
			WindowRect = new Rect(5f, 60f, 560f, 525f);
			WindowTitle = "Bone Hierarchy";
			//ExpandWidth = true;
		}

		public bool ShowRawHierarchy = false;

		//private gui values
		private Vector2 hierarchyScroll;
		private Vector2 boneSelectionScroll;
		private bool boneSelected = false;

		protected override void DrawWindow()
		{
			if (GUILayout.Button(ShowRawHierarchy ? "Show Bone Selection" : "Show Hierarchy Tree"))
			{
				ShowRawHierarchy = !ShowRawHierarchy;
			}

			if (ShowRawHierarchy)
			{
				hierarchyScroll = GUILayout.BeginScrollView(hierarchyScroll);

				DrawHierarchy(Suite.Kerbal.Joints01Transform, 0);

				GUILayout.EndScrollView();

				if (GUILayout.Button("Print Hierarchy"))
				{
					DebugUtil.PrintTransform(Suite.Kerbal.Part.transform);
				}
			}
			else
			{
				boneSelectionScroll = GUILayout.BeginScrollView(boneSelectionScroll, GUILayout.ExpandWidth(true));

				//base
				GUILayout.Label("<b>Global:</b>");
				GUILayout.BeginHorizontal();

				GUILayout.BeginVertical(skin.box);
				DrawBoneSelection("bn_spA01");
				GUILayout.EndVertical();

				GUILayout.EndHorizontal();
				GUILayout.Space(10f);

				//legs
				GUILayout.Label("<b>Legs:</b>");
				GUILayout.BeginHorizontal();

				GUILayout.BeginVertical(skin.box);
				DrawBoneSelection("bn_l_hip01");
				DrawBoneSelection("bn_l_knee_b01");
				DrawBoneSelection("bn_l_foot01");
				DrawBoneSelection("bn_l_ball01");
				GUILayout.EndVertical();

				GUILayout.BeginVertical(skin.box);
				DrawBoneSelection("bn_r_hip01");
				DrawBoneSelection("bn_r_knee_b01");
				DrawBoneSelection("bn_r_foot01");
				DrawBoneSelection("bn_r_ball01");
				GUILayout.EndVertical();

				GUILayout.EndHorizontal();
				GUILayout.Space(10f);

				//arms
				GUILayout.Label("<b>Arms:</b>");
				GUILayout.BeginHorizontal();

				GUILayout.BeginVertical(skin.box);
				DrawBoneSelection("bn_l_shld01");
				DrawBoneSelection("bn_l_arm01 1");
				DrawBoneSelection("bn_l_elbow_a01");
				DrawBoneSelection("bn_l_elbow_b01");
				DrawBoneSelection("bn_l_wrist01");
				GUILayout.Space(5f);
				GUILayout.Label("<color=" + Colors.Orange + ">Left Hand</color>");
				DrawBoneSelection("bn_l_mid_a01");
				DrawBoneSelection("bn_l_mid_b01");
				DrawBoneSelection("bn_l_thumb_a01");
				DrawBoneSelection("bn_l_thumb_b01");
				DrawBoneSelection("bn_l_thumb_c01");
				GUILayout.EndVertical();

				GUILayout.BeginVertical(skin.box);
				DrawBoneSelection("bn_r_shld01");
				DrawBoneSelection("bn_r_arm01 1");
				DrawBoneSelection("bn_r_elbow_a01");
				DrawBoneSelection("bn_r_elbow_b01");
				DrawBoneSelection("bn_r_wrist01");
				GUILayout.Space(5f);
				GUILayout.Label("<color=" + Colors.Orange + ">Right Hand</color>");
				DrawBoneSelection("bn_r_mid_a01");
				DrawBoneSelection("bn_r_mid_b01");
				DrawBoneSelection("bn_r_thumb_a01");
				DrawBoneSelection("bn_r_thumb_b01");
				DrawBoneSelection("bn_r_thumb_c01");
				GUILayout.EndVertical();

				GUILayout.EndHorizontal();
				GUILayout.Space(10f);

				//spine/neck
				GUILayout.Label("<b>Spine:</b>");
				GUILayout.BeginHorizontal();

				GUILayout.BeginVertical(skin.box);
				DrawBoneSelection("bn_spB01");
				DrawBoneSelection("bn_spc01");
				DrawBoneSelection("bn_spD01");
				DrawBoneSelection("bn_neck01");
				DrawBoneSelection("bn_headPivot_b01");
				DrawBoneSelection("bn_helmet01");
				GUILayout.EndVertical();

				GUILayout.EndHorizontal();
				GUILayout.Space(10f);

				//face
				GUILayout.Label("<b>Face:</b>");
				GUILayout.BeginHorizontal();

				GUILayout.BeginVertical(skin.box);
				DrawBoneSelection("bn_lowerJaw01");
				DrawBoneSelection("bn_upperJaw01");
				GUILayout.Space(5f);
				DrawBoneSelection("bn_lowerTeeth01");
				DrawBoneSelection("bn_upperTeet01");
				GUILayout.Space(5f);
				GUILayout.Label("<color=" + Colors.Orange + ">Eyes</color>");
				GUILayout.BeginHorizontal();
				DrawBoneSelection("jntDrv_l_eye01");
				DrawBoneSelection("jntDrv_r_eye01");
				GUILayout.EndHorizontal();
				GUILayout.Space(10f);
				GUILayout.Label("<color=" + Colors.Orange + ">Mouth</color>");
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical();
				GUILayout.Label("<color=" + Colors.Orange + ">Lower Lip</color>");
				DrawBoneSelection("bn_l_mouthCorner01");
				DrawBoneSelection("bn_l_mouthLow_d01");
				DrawBoneSelection("bn_l_mouthLow_c01");
				DrawBoneSelection("bn_l_mouthLow_b01");
				DrawBoneSelection("bn_l_mouthLowMid_a01");
				DrawBoneSelection("bn_r_mouthLow_b01");
				DrawBoneSelection("bn_r_mouthLow_c01");
				DrawBoneSelection("bn_r_mouthLow_d01");
				DrawBoneSelection("bn_r_mouthCorner01");
				GUILayout.EndVertical();

				GUILayout.BeginVertical();
				GUILayout.Label("<color=" + Colors.Orange + ">Upper Lip</color>");
				DrawBoneSelection("bn_l_mouthUp_d01");
				DrawBoneSelection("bn_l_mouthUp_c01");
				DrawBoneSelection("bn_l_mouthUp_b01");
				DrawBoneSelection("bn_l_mouthUpMid_a01");
				DrawBoneSelection("bn_r_mouthUp_b01");
				DrawBoneSelection("bn_r_mouthUp_c01");
				DrawBoneSelection("bn_r_mouthUp_d01");
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();

				GUILayout.EndVertical();

				GUILayout.EndHorizontal();
				GUILayout.Space(10f);

				GUILayout.EndScrollView();
			}

			GUI.DragWindow();
		}
		public override void Update()
		{

		}

		//GUI Util
		void DrawBoneSelection(string boneName)
		{
			string name = Suite.ReadableNames.ContainsKey(boneName) ? Suite.ReadableNames[boneName] : boneName;

			if (boneSelected && Suite.CurrentBone != null && Suite.CurrentBone.Name == boneName)
			{
				boneSelected = GUILayout.Toggle(boneSelected, "<color=" + Colors.SelectedColor + ">" + name + "</color>", skin.button);
				if (!boneSelected)
				{
					Suite.CurrentBone = null;
				}
			}
			else if (GUILayout.Button(name))
			{
				string animationName = Suite.AnimationNames.ContainsKey(boneName) ? Suite.AnimationNames[boneName] : "none found :C";
				var boneTransform = Suite.AnimationNames.ContainsKey(boneName) ? Suite.Kerbal.transform.Find(Suite.AnimationNames[boneName]) : null;
				if (boneTransform == null)
				{
					Debug.LogError("Null bone: " + boneName + " at " + animationName);
				}
				else
				{
					Suite.CurrentBone = new SelectedBone(boneTransform);
					boneSelected = true;
					Debug.Log("bone " + boneName + " selected at " + animationName);
				}
			}
		}
		void DrawHierarchy(Transform t, int level)
		{
			foreach (Transform child in t)
			{
				GUILayout.BeginVertical(skin.box);

				string indent = "";
				for (int i = 0; i < level; i++) indent += " |";
				string name = Suite.ReadableNames.ContainsKey(child.name) ? Suite.ReadableNames[child.name] : null;
				if (Suite.AnimationNames.ContainsKey(child.name)) name = "<color=" + Colors.Orange + ">" + name + "</color>";

				GUILayout.BeginHorizontal();
				GUILayout.Label(indent + child.name + " (" + name + ")");

				//TODO: reimpliment this later once the animation window is done
//				if (animPage == 1 && animationOpen)
//				{
//					GUI.backgroundColor = Color.green;
//					if (GUILayout.Button("+", GUILayout.Width(30f)))
//					{
//						if (KerbalAnimationSuite.AnimationNames.ContainsKey(child.name))
//						{
//							animationClip.AddMixingTransform(KerbalAnimationSuite.AnimationNames[child.name]);
//							addMTErrorText = "";
//							tempMTToAdd = "";
//						}
//						else
//						{
//							addMTErrorText = "Bone " + child.name + " not found!";
//							tempMTToAdd = "";
//						}
//					}
//					GUI.backgroundColor = Color.white;
//				}

				GUILayout.EndHorizontal();
				GUILayout.EndVertical();

				DrawHierarchy(child, level + 1);
			}
		}
	}
}

