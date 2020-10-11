using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using KSP.UI.Screens;

using UnityEngine;
using UnityEngine.UI;
using ToolbarControl_NS;

namespace KerbalAnimation
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class KerbalAnimationSuite : MonoBehaviour
	{
		public static KerbalAnimationSuite Instance;

		public bool IsAnimating
		{
			get{return Kerbal != null ? Kerbal.IsAnimating : false;}
		}

		public GUISkin skin = HighLogic.Skin;

		public Dictionary<string, string> ReadableNames = new Dictionary<string, string>();
		public Dictionary<string, string> AnimationNames = new Dictionary<string, string>();

		//main values
		private EditableAnimationClip _animationClip;
		public EditableAnimationClip AnimationClip
		{
			get {return _animationClip;}
			set
			{
				_animationClip = value;
				OnNewAnimationClip.Fire(value);
			}
		}

		private SelectedKerbalEVA _kerbal;
		public SelectedKerbalEVA Kerbal
		{
			get{return _kerbal;}
			set
			{
				_kerbal = value;
				OnKerbalSelected.Fire(value);
			}
		}

		private SelectedBone _currentBone;
		public SelectedBone CurrentBone
		{
			get{return _currentBone;}
			set
			{
				_currentBone = value;
				OnBoneSelected.Fire(value);
			}
		}

		//Settings
		public KerbalAnimationSettings Settings;

		//Events
		public EventData<SelectedBone> OnBoneSelected = new EventData<SelectedBone>("OnBoneSelected");
		public EventData<SelectedKerbalEVA> OnKerbalSelected = new EventData<SelectedKerbalEVA>("OnKerbalEVASelected");
		public EventData<EditableAnimationClip> OnNewAnimationClip = new EventData<EditableAnimationClip>("OnNewAnimationClip");

		//Music
		public MusicLogicWrapper MusicWrapper;
		public bool MusicIsPlaying
		{
			get {return MusicWrapper.MusicIsPlaying;}
			set
			{
				if (value && Settings.AllowEditorMusic) MusicWrapper.StartPlaylist(0.5f);
				else MusicWrapper.StopPlaylist(0.5f);
			}
		}

		//GUI

		//Windows
		public MasterWindow Master;
		public HierarchyWindow Hierarchy;
		public ManipulationWindow Manipulation;
		public AnimationWindow Animation;

        //Button
        //private static ApplicationLauncherButton Button;
        ToolbarControl toolbarControl;

		private void Awake()
		{
			Instance = this;

			//load settings
			Settings = new KerbalAnimationSettings();

			//instantiate windows
			Master = new MasterWindow();
			Hierarchy = new HierarchyWindow();
			Manipulation = new ManipulationWindow();
			Animation = new AnimationWindow();
		}

        internal const string MODID = "KerbalAnimationsuite_NS";
        internal const string MODNAME = "Kerbal Animation Suite";
        private void Start()
		{
			//load animation data
			ConfigurationUtils.LoadAnimationNames();
			ConfigurationUtils.LoadReadableNames();

			//music
			MusicWrapper = new MusicLogicWrapper();

            //add AppLauncher button
#if false
            var buttonTexture = GameDatabase.Instance.GetTexture("KerbalAnimationSuite/Icons/button", false);
			Button = ApplicationLauncher.Instance.AddModApplication(EnableAnimationSuite, DisableAnimationSuite, null, null, null, null, ApplicationLauncher.AppScenes.FLIGHT, buttonTexture);
#endif

            toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(EnableAnimationSuite, DisableAnimationSuite,
                ApplicationLauncher.AppScenes.FLIGHT,
                MODID,
                "kerbalAnimationsuiteButton",
                "KerbalAnimationSuite/Icons/button-38",
                "KerbalAnimationSuite/Icons/button-24",
                MODNAME
            );
        }
		private void OnDestroy()
		{
            if (toolbarControl != null)
            {
                toolbarControl.OnDestroy();
                Destroy(toolbarControl);
            }
#if false
            //remove AppLauncher button
            if (Button != null) ApplicationLauncher.Instance.RemoveModApplication(Button);
#endif
		}

		public void EnableAnimationSuite()
		{
			var vessel = FlightGlobals.ActiveVessel;
			if (vessel.evaController == null)
			{
				ScreenMessages.PostScreenMessage(new ScreenMessage("<color=" + Colors.DefaultMessageColor + ">Active vessel must be an EVA to use the Animation Suite</color>", 3f, ScreenMessageStyle.UPPER_CENTER));

				//set the button back to false
				toolbarControl.SetFalse(false);
				return;
			}

			Kerbal = new SelectedKerbalEVA(vessel.evaController);
			AnimationClip = new EditableAnimationClip(Kerbal);
			Animation.OnEnableSuite();

			if (!Kerbal.EnterAnimationMode())
			{
				//wipe the state
				Kerbal = null;
				AnimationClip = null;
				CurrentBone = null;

                //set the button back to false if it failed
                toolbarControl.SetFalse(false);
				return;
			}

			MusicIsPlaying = true;
		}
		public void DisableAnimationSuite()
		{
			MusicIsPlaying = false;

			if (Kerbal != null) Kerbal.ExitAnimationMode();

			//wipe the state
			Kerbal = null;
			AnimationClip = null;
			CurrentBone = null;
		}

		void Update()
		{
			if (Kerbal != null && Kerbal.IsAnimating)
			{
				Master.Update();
				Hierarchy.Update();
				Manipulation.Update();
				Animation.Update();

				//stop music if setting is set to false, and music is still playing
				if (!Settings.AllowEditorMusic && MusicWrapper.MusicIsPlaying)
				{
					MusicIsPlaying = false;
				}
				//start music if setting is set to true, and music is not playing
				if (Settings.AllowEditorMusic && !MusicWrapper.MusicIsPlaying)
				{
					MusicIsPlaying = true;
				}
			}
		}

		void OnGUI()
		{
			//don't draw when F2 is pressed
			if (!GUIHider.ShowUI) return;

			if (Kerbal != null && Kerbal.IsAnimating)
			{
				Master.Draw();
				if (Master.HierarchyOpen && !Kerbal.IsAnimationPlaying && Animation.KeyframeSelected) Hierarchy.Draw();
				if (Master.ManipulationOpen && !Kerbal.IsAnimationPlaying && Animation.KeyframeSelected) Manipulation.Draw();
				if (Master.AnimationOpen) Animation.Draw();
			}
		}
	}
}

