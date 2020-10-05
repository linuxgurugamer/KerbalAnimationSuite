using System;
using UnityEngine;

namespace KerbalAnimation
{
	public class SelectedBone
	{
		public string Name
		{get{return Bone.name;}}
		public Transform Bone
		{get; private set;}
		public Vector3 Position
		{
			get{return Bone.localPosition;}
			set{Bone.localPosition = value;}
		}
		public Vector3 Rotation
		{
			get{return Bone.localRotation.eulerAngles;}
			set{Bone.localRotation = Quaternion.Euler(value);}
		}

		public SelectedBone(string name)
		{
			Bone = KerbalAnimationSuite.Instance.AnimationNames.ContainsKey(name) ? KerbalAnimationSuite.Instance.Kerbal.Part.transform.Find(KerbalAnimationSuite.Instance.AnimationNames [name]) : null;
			if (Bone == null)
				Debug.LogError("Null bone: " + name);
			else
				Debug.Log("bone " + name + " selected");
		}
		public SelectedBone(Transform bone)
		{
			Bone = bone;
			if (Bone == null)
				Debug.LogError("Null bone transform");
			else
				Debug.Log("bone " + Bone.name + " selected");
		}

		//implicit operators
		public static implicit operator Transform(SelectedBone selected)
		{
			return selected.Bone;
		}
	}
}

