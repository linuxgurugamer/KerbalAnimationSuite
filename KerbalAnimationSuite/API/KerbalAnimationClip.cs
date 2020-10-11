using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

public class KerbalAnimationClip
{
	public static Dictionary<string, string> AnimationNames = null;

	//constructors
	public KerbalAnimationClip()
	{
	}

	//loading
	public void LoadFromURL(string url)
	{
		if (!url.EndsWith(".anim")) url += ".anim";
		string fullPath = KSPUtil.ApplicationRootPath + "GameData/" + url;
		ConfigNode node = ConfigNode.Load(fullPath);
		LoadAndBuild(node);
	}
	public void LoadFromPath(string fullPath)
	{
		if (!fullPath.EndsWith(".anim")) fullPath += ".anim";
		ConfigNode node = ConfigNode.Load(fullPath);
		LoadAndBuild(node);
	}
	public void LoadFromConfig(ConfigNode node)
	{
		LoadAndBuild(node);
	}
	protected void LoadAndBuild(ConfigNode node)
	{
		if (AnimationNames == null) LoadAnimationNames();

		Load(node);
		BuildAnimationClip();
	}
	protected void LoadAnimationNames()
	{
		foreach (var filePath in Directory.GetFiles(KSPUtil.ApplicationRootPath + "GameData/", "*animation_hierarchy*", SearchOption.AllDirectories))
		{
			ConfigNode node = ConfigNode.Load(filePath);
			AnimationNames = new Dictionary<string, string>();
			foreach (ConfigNode.Value value in node.values)
			{
				AnimationNames.Add(value.name, value.value);
			}
			return;
		}
		Debug.LogError("[assembly: " + Assembly.GetExecutingAssembly().GetName().Name + "]: " + "Animation Hierarchy not found");
	}

	//implicit operators
	public static implicit operator AnimationClip(KerbalAnimationClip clip)
	{
		return clip.clip;
	}
	public static implicit operator string(KerbalAnimationClip clip)
	{
		return clip.name;
	}

	//private values
	protected string name;
	protected int layer = 0;
	protected float duration;
	protected AnimationClip clip;

	//public properties
	/// <summary>
	/// The name of the animation.
	/// </summary>
	public string Name
	{
		get
		{
			return this.name;
		}
	}
	/// <summary>
	/// The animation's layer. Look at the unity documentation for more information.
	/// </summary>
	public int Layer
	{
		get
		{
			return this.layer;
		}
	}
	/// <summary>
	/// The length in seconds of the animation.
	/// </summary>
	public float Duration
	{
		get
		{
			return this.duration;
		}
	}

	public AnimationClip Clip
	{
		get
		{
			return this.clip;
		}
	}

	//public methods
	/// <summary>
	/// Initialize the animation clip in the kerbal part.
	/// </summary>
	/// <param name="animation">The Animation object to be used. Should be the animation property of any PartModule attached to a kerbalEVA/kerbalEVAfemale part</param>
	/// <param name="transform">The Transform object to be used as the skeleton. Should be the transform property of any PartModule attached to a kerbalEVA/kerbalEVAfemale part</param>
	public void Initialize(Animation animation, Transform transform)
	{
		Initialize(animation, transform, null);
	}
	protected void Initialize(Animation animation, Transform transform, string newName = null)
	{
		if (clip == null)
		{
			Debug.LogError("clip is null. Cannot initialize animation " + (name == null ? "NULL" : name));
			return;
		}
		animation.RemoveClip(Name);
		if (newName != null) name = newName;
		animation.AddClip(clip, Name);
		animation[Name].layer = Layer;
		foreach (var mt in MixingTransforms)
		{
			if (AnimationNames.ContainsKey(mt) && transform.Find(AnimationNames[mt]) != null && transform != null) animation[Name].AddMixingTransform(transform.Find(AnimationNames[mt]));
			else Debug.LogError("[assembly: " + Assembly.GetExecutingAssembly().GetName().Name + "]: animation mixing transform " + mt + " from animation " + Name + " does not exist, or could not be found.");
		}
	}

	//loading data
	#region loading data
	protected List<KerbalKeyframe> Keyframes = new List<KerbalKeyframe>();

	protected Dictionary<string, AnimationCurve> RotationWCurves = new Dictionary<string, AnimationCurve>();
	protected Dictionary<string, AnimationCurve> RotationXCurves = new Dictionary<string, AnimationCurve>();
	protected Dictionary<string, AnimationCurve> RotationYCurves = new Dictionary<string, AnimationCurve>();
	protected Dictionary<string, AnimationCurve> RotationZCurves = new Dictionary<string, AnimationCurve>();
	protected Dictionary<string, AnimationCurve> PositionXCurves = new Dictionary<string, AnimationCurve>();
	protected Dictionary<string, AnimationCurve> PositionYCurves = new Dictionary<string, AnimationCurve>();
	protected Dictionary<string, AnimationCurve> PositionZCurves = new Dictionary<string, AnimationCurve>();

	//mixing transforms
	protected List<string> MixingTransforms = new List<string>();
	protected void AddMixingTransform(string name)
	{
		MixingTransforms.Add(name);
	}
	protected void RemoveMixingTransform(string name)
	{
		if (MixingTransforms.Contains(name)) MixingTransforms.Remove(name);
	}

	protected AnimationClip BuildAnimationClip()
	{
		clip = new AnimationClip();
		clip.legacy = true;
		clip.wrapMode = WrapMode.Loop;

		//populate dictionaries with curves
		RotationWCurves.Clear();
		RotationXCurves.Clear();
		RotationYCurves.Clear();
		RotationZCurves.Clear();
		PositionXCurves.Clear();
		PositionYCurves.Clear();
		PositionZCurves.Clear();
		foreach (string animationName in AnimationNames.Values)
		{
			RotationWCurves.Add(animationName, new AnimationCurve());
			RotationXCurves.Add(animationName, new AnimationCurve());
			RotationYCurves.Add(animationName, new AnimationCurve());
			RotationZCurves.Add(animationName, new AnimationCurve());

			PositionXCurves.Add(animationName, new AnimationCurve());
			PositionYCurves.Add(animationName, new AnimationCurve());
			PositionZCurves.Add(animationName, new AnimationCurve());
		}

		//populate curves with keyframe values
		foreach (var keyframe in Keyframes)
		{
			foreach (string animationName in AnimationNames.Values)
			{
				RotationWCurves[animationName].AddKey(keyframe.Time, keyframe.GetValue(animationName, KAS_ValueType.RotW));
				RotationXCurves[animationName].AddKey(keyframe.Time, keyframe.GetValue(animationName, KAS_ValueType.RotX));
				RotationYCurves[animationName].AddKey(keyframe.Time, keyframe.GetValue(animationName, KAS_ValueType.RotY));
				RotationZCurves[animationName].AddKey(keyframe.Time, keyframe.GetValue(animationName, KAS_ValueType.RotZ));
				PositionXCurves[animationName].AddKey(keyframe.Time, keyframe.GetValue(animationName, KAS_ValueType.PosX));
				PositionYCurves[animationName].AddKey(keyframe.Time, keyframe.GetValue(animationName, KAS_ValueType.PosY));
				PositionZCurves[animationName].AddKey(keyframe.Time, keyframe.GetValue(animationName, KAS_ValueType.PosZ));
			}
		}

		//set curves to clip
		foreach (string animationName in AnimationNames.Values)
		{
			clip.SetCurve(animationName, typeof(Transform), "localRotation.w", RotationWCurves[animationName]);
			clip.SetCurve(animationName, typeof(Transform), "localRotation.x", RotationXCurves[animationName]);
			clip.SetCurve(animationName, typeof(Transform), "localRotation.y", RotationYCurves[animationName]);
			clip.SetCurve(animationName, typeof(Transform), "localRotation.z", RotationZCurves[animationName]);

			clip.SetCurve(animationName, typeof(Transform), "localPosition.x", PositionXCurves[animationName]);
			clip.SetCurve(animationName, typeof(Transform), "localPosition.y", PositionYCurves[animationName]);
			clip.SetCurve(animationName, typeof(Transform), "localPosition.z", PositionZCurves[animationName]);
		}

		clip.EnsureQuaternionContinuity();
		return clip;
	}
	#endregion

	//IO
	#region IO
	public const int FileTypeVersion = 1;

	protected void Load(ConfigNode node)
	{
		try
		{
			this.name = node.GetValue("Name");
			this.duration = float.Parse(node.GetValue("Duration"));
			if (node.HasValue("Layer")) this.layer = int.Parse(node.GetValue("Layer"));
			else this.layer = 5;

			ConfigNode mtNode = node.GetNode("MIXING_TRANSFORMS");
			foreach (var mt in mtNode.GetValues("MixingTransform"))
			{
				AddMixingTransform(mt);
			}

			ConfigNode keyframesNode = node.GetNode("KEYFRAMES");
			foreach (var keyframeNode in keyframesNode.GetNodes("KEYFRAME"))
			{
				KerbalKeyframe keyframe = new KerbalKeyframe(this);
				keyframe.NormalizedTime = float.Parse(keyframeNode.GetValue("NormalizedTime"));

				foreach (string animationName in AnimationNames.Values)
				{
					if (!keyframeNode.HasValue(animationName)) continue;

					string allValues = keyframeNode.GetValue(animationName);
					string[] values = allValues.Split(' ');

					float rotW = float.Parse(values[0]);
					float rotX = float.Parse(values[1]);
					float rotY = float.Parse(values[2]);
					float rotZ = float.Parse(values[3]);
					float posX = float.Parse(values[4]);
					float posY = float.Parse(values[5]);
					float posZ = float.Parse(values[6]);

					keyframe.SetValue(rotW, animationName, KAS_ValueType.RotW);
					keyframe.SetValue(rotX, animationName, KAS_ValueType.RotX);
					keyframe.SetValue(rotY, animationName, KAS_ValueType.RotY);
					keyframe.SetValue(rotZ, animationName, KAS_ValueType.RotZ);
					keyframe.SetValue(posX, animationName, KAS_ValueType.PosX);
					keyframe.SetValue(posY, animationName, KAS_ValueType.PosY);
					keyframe.SetValue(posZ, animationName, KAS_ValueType.PosZ);
				}

				Keyframes.Add(keyframe);
			}
			Debug.Log("KerbalAnimationClip " + this.name + " was loaded successfully.");
		}
		catch(Exception e)
		{
			Debug.LogError("ERROR ENCOUNTERED LOADING ANIMATION");
			Debug.LogException(e);
		}
	}
	#endregion

	//embedded types
	#region emdedded types
	public class KerbalKeyframe
	{
		public KerbalKeyframe(KerbalAnimationClip animClip)
		{
			this.clip = animClip;
		}
			
		KerbalAnimationClip clip;
		public float NormalizedTime = 0f;
		public float Time
		{
			get{return NormalizedTime * clip.Duration;}
		}

		Dictionary<string, float> RotationW = new Dictionary<string, float>();
		Dictionary<string, float> RotationX = new Dictionary<string, float>();
		Dictionary<string, float> RotationY = new Dictionary<string, float>();
		Dictionary<string, float> RotationZ = new Dictionary<string, float>();

		Dictionary<string, float> PositionX = new Dictionary<string, float>();
		Dictionary<string, float> PositionY = new Dictionary<string, float>();
		Dictionary<string, float> PositionZ = new Dictionary<string, float>();

		private string DebugPrintTransform(Transform transform, int level=0, string path="")
        {
			path += "/" + transform.name;
			string output = "".PadLeft(level * 4) + path + "\n";
			for (int i = 0; i < transform.childCount; i++)
            {
				output += DebugPrintTransform(transform.GetChild(i), (level + 1), path);
            }
			return output;
        }

		public void Write(Transform transform, float time)
		{
			this.Clear();
			this.NormalizedTime = time;
			foreach (string name in AnimationNames.Values)
			{
				Transform t = transform.Find(name);

				// Ignore HeadCollider as its path differs between male and female kerbals
				if (name.ToLower().Contains("headcollider")) continue;

				if (t == null)
				{
					Debug.LogError("[" + Assembly.GetExecutingAssembly().GetName().Name + "]: t is null at " + name);
					string output = DebugPrintTransform(transform);
					Debug.LogError("[" + Assembly.GetExecutingAssembly().GetName().Name + "]: transform dump follows\n " + output);


				}
				Quaternion quatRot = t.localRotation;
				RotationW.Add(name, quatRot.w);
				RotationX.Add(name, quatRot.x);
				RotationY.Add(name, quatRot.y);
				RotationZ.Add(name, quatRot.z);

				Vector3 localPos = t.localPosition;
				PositionX.Add(name, localPos.x);
				PositionY.Add(name, localPos.y);
				PositionZ.Add(name, localPos.z);
			}
		}

		public void Clear()
		{
			RotationW.Clear();
			RotationX.Clear();
			RotationY.Clear();
			RotationZ.Clear();
			PositionX.Clear();
			PositionY.Clear();
			PositionZ.Clear();
			this.NormalizedTime = 0f;
		}

		public void SetValue(float value, string animationName, KAS_ValueType type)
		{
			switch(type)
			{
			case KAS_ValueType.RotW:
				RotationW[animationName] = value;
				break;
			case KAS_ValueType.RotX:
				RotationX[animationName] = value;
				break;
			case KAS_ValueType.RotY:
				RotationY[animationName] = value;
				break;
			case KAS_ValueType.RotZ:
				RotationZ[animationName] = value;
				break;
			case KAS_ValueType.PosX:
				PositionX[animationName] = value;
				break;
			case KAS_ValueType.PosY:
				PositionY[animationName] = value;
				break;
			case KAS_ValueType.PosZ:
				PositionZ[animationName] = value;
				break;
			default:
				break;
			}
		}

		public float GetValue(string animationName, KAS_ValueType type)
		{
			try
			{
				switch(type)
				{
				case KAS_ValueType.RotW:
					return RotationW[animationName];
				case KAS_ValueType.RotX:
					return RotationX[animationName];
				case KAS_ValueType.RotY:
					return RotationY[animationName];
				case KAS_ValueType.RotZ:
					return RotationZ[animationName];
				case KAS_ValueType.PosX:
					return PositionX[animationName];
				case KAS_ValueType.PosY:
					return PositionY[animationName];
				case KAS_ValueType.PosZ:
					return PositionZ[animationName];
				default:
					return 0f;
				}
			}
			catch(KeyNotFoundException e)
			{
				//ignore exceptions on colliders
				if (animationName.ToLower().Contains("collider")) return 0f;

				Debug.LogError("key not found: " + animationName);
				Debug.LogException(e);
			}
			return 0f;
		}
	}
	public enum KAS_ValueType
	{
		RotW, RotX, RotY, RotZ, PosX, PosY, PosZ
	}
	#endregion
}

