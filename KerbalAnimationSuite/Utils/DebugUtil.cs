using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalAnimation
{
	public static class DebugUtil
	{
		public static void PrintFSM(KerbalEVA eva)
		{
			var fsm = eva.fsm;
			Debug.Log("CurrentState: " + fsm.CurrentState.name);

			var type = fsm.GetType();
			Debug.Log("Type: " + type.Name);
			var statesF = type.GetField("States", BindingFlags.NonPublic | BindingFlags.Instance);
			List<KFSMState> states = (List<KFSMState>)statesF.GetValue(fsm);

			foreach (var state in states)
			{
				if (state == null)
				{
					Debug.LogWarning("null state found, skipping");
					continue;
				}
				Debug.Log("State: " + state.name + " : " + state.updateMode.ToString());
				if (state.StateEvents == null)
				{
					Debug.LogWarning("No state events list found");
					continue;
				}
				foreach (var evt in state.StateEvents)
				{
					if (evt == null)
					{
						Debug.LogWarning("null evt found, skipping");
						continue;
					}
					Debug.Log("----- Event: " + evt.name + ": => " + (evt.GoToStateOnEvent != null ? evt.GoToStateOnEvent.name : "N/A") + " : " + evt.updateMode.ToString());
				}
			}
		}
		public static void PrintAnimationStates(Animation animation)
		{
			foreach (AnimationState state in animation)
			{
				Debug.Log(state.name + ": Layer: " + state.layer + ", WrapMode: " + state.wrapMode.ToString() + ", BlendMode: " + state.blendMode.ToString() + ", Enabled: " + state.enabled + ", Speed: " + state.speed + ", Length: " + state.length);
			}
		}
		public static void PrintTransform(Transform t, bool printComponents = false, int level = 0)
		{
			string indent = "";
			for (int i = 0; i < level; i++)
				indent += "   |";
			Debug.Log(indent + t.name);
			if (printComponents)
				PrintComponents(t, level);
			foreach (Transform child in t)
			{
				PrintTransform(child, printComponents, level + 1);
			}
		}
		public static void PrintComponents(Transform t, int level = 0)
		{
			string indent = "";
			for (int i = 0; i < level; i++)
				indent += "   |";
			indent += " - ";
			foreach (var component in t.GetComponents<Component>())
			{
				if (component.GetType() != typeof(Transform))
					Debug.Log(indent + component.GetType().Name);
			}
		}
	}
}

