﻿using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalAnimation
{
#if false
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class EVAModuleAdder : MonoBehaviour
	{
		void Start()
		{
			KerbalEVAUtility.AddPartModule("AnimationPlayerModule");
		}
	}
#endif

	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class AnimationPlayerWindowHost : MonoBehaviour
	{
		public static AnimationPlayerWindowHost Instance {get; private set;}

		public AnimationPlayerWindow Player;
		public static bool GUIOpen = false;

		//events
		public EventData<List<KerbalAnimationClip>> OnReloadAnimationClips = new EventData<List<KerbalAnimationClip>>("OnReloadAnimationClips");

		private void Awake()
		{
			Instance = this;
		}
		private void Start()
		{
			Player = new AnimationPlayerWindow();
		}
		private void OnGUI()
		{
			//return if we are not in a valid state to draw
			if (!GUIHider.ShowUI || !HighLogic.LoadedSceneIsFlight || KerbalAnimationSuite.Instance.IsAnimating)
			{
				return;
			}

			if (GUIOpen)
			{
				Player.Draw();
			}
		}

        private void Update()
		{
			//return if we are not in a valid state to update
			if (!HighLogic.LoadedSceneIsFlight || KerbalAnimationSuite.Instance.IsAnimating) return;

			Player.Update();
		}
	}

	public class AnimationPlayerModule : PartModule
	{
		public string OpenGUIName = "Open Animation Player";
		public string CloseGUIName = "Close Animation Player";

		private Animation _animation;
		public Animation animation
		{
			get
			{
				if (_animation == null)
				{
					_animation = GetComponent<Animation>();
				}
				return _animation;
			}
		}

		//lifetime
		public override void OnStart(StartState state)
		{
			AnimationPlayerWindowHost.Instance.OnReloadAnimationClips.Add(OnReloadAnimationClips);
			AnimationPlayerWindowHost.Instance.Player.AddKerbal(animation.gameObject);
		}
		private void OnDestroy()
        {
			// Avoid dereferencing null Player on initial load
			if (AnimationPlayerWindowHost.Instance.Player != null) AnimationPlayerWindowHost.Instance.Player.RemoveKerbal(animation.gameObject);
		}
		public override void OnUpdate()
		{
			Events["ToggleGUI"].guiName = AnimationPlayerWindowHost.GUIOpen ? CloseGUIName : OpenGUIName;

			for (int i = 0; i < 10; i++)
			{
				string buttonName = (i + 1).ToString();
				if (i >= 9) buttonName = "0";
				if (Input.GetKey(buttonName) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
				{
					bool shift = Input.GetKey(KeyCode.LeftShift);

					// Only animate the proper kerbals
					if (!AnimationPlayerWindowHost.Instance.Player.ShouldAnimateKerbal(animation.gameObject, shift, (FlightGlobals.ActiveVessel == vessel)))
					{
						continue;
					}
					var clip = AnimationPlayerWindowHost.Instance.Player.GetNumberKeyClip(animation.gameObject, i);
					if (clip != null && !animation.IsPlaying(clip.Name))
					{
						if (AnimationPlayerWindow.Loop) PlayAnimation(clip.Name, WrapMode.Loop);
						else PlayAnimation(clip.Name, WrapMode.Once);
					}
				}
			}
		}

		public void PlayAnimation(string name, WrapMode wrapMode)
		{
			var state = animation[name];
			if (state == null || animation.GetClip(name) == null) return;
			state.wrapMode = wrapMode;
			animation.CrossFade(name, 0.2f * state.length, PlayMode.StopSameLayer);
		}

		//events
		private void OnReloadAnimationClips(List<KerbalAnimationClip> clips)
		{
			//initialize all of the clips with this kerbal
			foreach (var clip in clips)
			{
				clip.Initialize(animation, transform);
			}
		}

		//KSPEvents
		[KSPEvent(guiName = "Open GUI", guiActiveUnfocused = true, guiActive = true)]
		public void ToggleGUI()
		{
			AnimationPlayerWindowHost.GUIOpen = !AnimationPlayerWindowHost.GUIOpen;
		}

		
	}
}

