using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalAnimation
{
	public static class ConfigurationUtils
	{
		//animation names
		public static void LoadAnimationNames()
		{
			Debug.Log("Loading animation_hierarchy.dat...");
			ConfigNode node = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/KerbalAnimationSuite/Config/animation_hierarchy.dat");
			KerbalAnimationSuite.Instance.AnimationNames.Clear();
			foreach (ConfigNode.Value value in node.values)
			{
				KerbalAnimationSuite.Instance.AnimationNames.Add(value.name, value.value);
			}
			//set the clip's animation names so that we won't have 2 seperate instances of animation names
			KerbalAnimationClip.AnimationNames = KerbalAnimationSuite.Instance.AnimationNames;
			Debug.Log("[assembly: " + Assembly.GetExecutingAssembly().GetName().Name + "]: animation_hierarchy.dat was loaded");
		}
		public static void SaveAnimationNames(string url)
		{
			ConfigNode node = new ConfigNode();
			foreach (var name in KerbalAnimationSuite.Instance.AnimationNames)
			{
				node.AddValue(name.Key, name.Value);
			}
			node.Save(KSPUtil.ApplicationRootPath + "GameData/" + url + ".dat");
		}
		public static void RebuildAnimationNames(SelectedKerbalEVA eva)
		{
			if (eva.Joints01Transform == null)
			{
				Debug.Log("joints01 is null, cannot populate animation names");
				return;
			}

			string prefix = "globalMove01";
			KerbalAnimationSuite.Instance.AnimationNames = new Dictionary<string, string>();

			PopulateAnimationNamesRecursive(eva.Joints01Transform, prefix);
		}
		private static void PopulateAnimationNamesRecursive(Transform t, string prefix)
		{
			prefix += "/" + t.name;
			KerbalAnimationSuite.Instance.AnimationNames.Add(t.name, prefix);
			foreach (Transform child in t)
			{
				PopulateAnimationNamesRecursive(child, prefix);
			}
		}

		//readable names
		public static void LoadReadableNames()
		{
			ConfigNode node = GameDatabase.Instance.GetConfigNodes("KerbalAnimationSuiteReadableNames") [0];

			KerbalAnimationSuite.Instance.ReadableNames = new Dictionary<string, string>();
			foreach (ConfigNode.Value value in node.values)
			{
				//replace + with space
				string name = value.name.Replace('+', ' ');
				KerbalAnimationSuite.Instance.ReadableNames.Add(name, value.value);
			}
		}
	}
}

