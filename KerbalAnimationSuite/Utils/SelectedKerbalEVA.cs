using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalAnimation
{
	public class SelectedKerbalEVA
	{
		public KerbalEVA Kerbal {get; private set;}
		public KerbalFSM FSM {get{return Kerbal.fsm;}}
		public Part Part {get{return Kerbal.part;}}
		private Animation _animation;
		public Animation animation
		{
			get
			{
				if (_animation == null)
				{
					_animation = Kerbal.GetComponent<Animation>();
				}
				return _animation;
			}
		}
		public Transform transform {get{return Part.transform;}}

		public Transform Joints01Transform {get; private set;}
		public List<KFSMState> States { get; private set;}

		private bool _hasHelmet = true;
		public bool HasHelmet
		{
			get {return _hasHelmet;}
			set {SetHelmet(value);}
		}

		public bool IsAnimating {get; private set;}
		public bool IsAnimationPlaying {get{return animation.isPlaying;}}

		//constructor
		public SelectedKerbalEVA(KerbalEVA eva)
		{
			Kerbal = eva;
			Joints01Transform = Part.transform.Find("globalMove01/joints01");
			States = KerbalEVAUtility.GetEVAStates(eva);

			AddAnimationState();

			//set defaults
			IsAnimating = false;
		}

		//adds an FSM state to show that we are animating
		private void AddAnimationState()
		{
			if (States.Find(k => k.name == "KAS_Animation") == null)
			{
				KFSMState state = new KFSMState("KAS_Animation");
				state.updateMode = KFSMUpdateMode.MANUAL_TRIGGER;
				FSM.AddState(state);

				KFSMEvent enterEvent = new KFSMEvent("Enter KAS_Animation");
				enterEvent.GoToStateOnEvent = state;
				enterEvent.updateMode = KFSMUpdateMode.MANUAL_TRIGGER;
				var idleGrounded = States.Find(k => k.name == "Idle (Grounded)");
				FSM.AddEvent(enterEvent, idleGrounded);

				KFSMEvent exitEvent = new KFSMEvent("Exit KAS_Animation");
				exitEvent.GoToStateOnEvent = idleGrounded;
				exitEvent.updateMode = KFSMUpdateMode.MANUAL_TRIGGER;
				FSM.AddEvent(exitEvent, state);
			}
		}
        public static Mesh helmetMesh = null;
        public static Mesh visorMesh = null;
        public static Mesh flare1Mesh = null;
        public static Mesh flare2Mesh = null;

        //utility methods
        private void SetHelmet(bool value)
		{
#if false
            foreach (var rend in Kerbal.GetComponentsInChildren<Renderer>())
			{
				if (rend.name == "helmet" || rend.name == "visor" || rend.name == "flare1" || rend.name == "flare2")
				{
					rend.enabled = value;
				}
			}
#endif
            foreach (Renderer renderer in Kerbal.GetComponentsInChildren<Renderer>())
            {
                var smr = renderer as SkinnedMeshRenderer;

                if (smr != null)
                {
                    switch(smr.name)
                    {
                        case "helmet":
                            {
                                if (helmetMesh == null)
                                    helmetMesh = smr.sharedMesh;

                                smr.sharedMesh = value ? helmetMesh : null;
                            }
                            break;
                        case "visor":
                            {
                                if (visorMesh == null)
                                    visorMesh = smr.sharedMesh;

                                smr.sharedMesh = value ? visorMesh : null;
                            }
                            break;

                        case "flare1":
                            {
                                if (flare1Mesh == null)
                                    visorMesh = smr.sharedMesh;

                                smr.sharedMesh = value ? flare1Mesh : null;
                            }
                            break;
                        case "flare2":
                            {
                                if (flare2Mesh == null)
                                    flare2Mesh = smr.sharedMesh;

                                smr.sharedMesh = value ? flare2Mesh : null;
                            }
                            break;
                    }
                }
            }
            _hasHelmet = value;
		}

		//returns false if it failed to initialize animation mode
		public bool EnterAnimationMode()
		{
			//check if we can animate
			if (TimeWarp.CurrentRate != 1f)
			{
				ScreenMessages.PostScreenMessage("<color=" + Colors.DefaultMessageColor + ">You must not be in time warp to animate</color>", 2.5f, ScreenMessageStyle.UPPER_CENTER);
				TimeWarp.SetRate(0, true);
				return false;
			}
			if (FSM.CurrentState.name == "Idle (Grounded)")
			{
				var enter = FSM.CurrentState.StateEvents.Find(k => k.name == "Enter KAS_Animation");
				if (enter != null)
				{
					FSM.RunEvent(enter);
				}
				else
				{
					Debug.LogError("failed to run event: Enter KAS_Animation");
					ScreenMessages.PostScreenMessage("<color=" + Colors.ErrorMessageColor + ">Failed to open Kerbal Animation Suite!</color>", 2.5f, ScreenMessageStyle.UPPER_CENTER);
					return false;
				}
			}
			else
			{
				ScreenMessages.PostScreenMessage("<color=" + Colors.DefaultMessageColor + ">Kerbal must be standing on ground to animate</color>", 2.5f, ScreenMessageStyle.UPPER_CENTER);
				return false;
			}

			//stop any playing animations
			animation.playAutomatically = false;
			animation.Stop();

			//go up 10 units
			transform.position += transform.up * 10f;

			//freeze kerbal's physics
			foreach (var rb in Part.GetComponents<Rigidbody>())
			{
				rb.velocity = Vector3.zero;
				rb.constraints = RigidbodyConstraints.FreezeAll;
			}

			//lock input
			InputLockManager.SetControlLock(ControlTypes.EVA_INPUT | ControlTypes.TIMEWARP | ControlTypes.VESSEL_SWITCHING, "KerbalAnimationSuite_Lock");

			//this is obvious
			IsAnimating = true;

			//yay, we succeeded!
			return true;
		}
		public void ExitAnimationMode()
		{
			if (FSM.CurrentState.name == "KAS_Animation")
			{
				var exit = FSM.CurrentState.StateEvents.Find(k => k.name == "Exit KAS_Animation");
				if (exit != null)
					FSM.RunEvent(exit);
				else
					Debug.LogError("failed to run event: Exit KAS_Animation");
			}

			//set animation settings back to default
			animation.Stop();
			animation.playAutomatically = true;

			//set helmet back to default
			HasHelmet = true;

			//move back down onto ground
			//TODO: use a raycast sanity check
			transform.position -= transform.up * 9.75f;
			foreach (var rb in Part.GetComponents<Rigidbody>())
			{
				rb.velocity = Vector3.zero;
				rb.constraints = RigidbodyConstraints.None;
			}

			//remove the input lock
			InputLockManager.RemoveControlLock("KerbalAnimationSuite_Lock");

			IsAnimating = false;
		}

		//implicit operators
		public static implicit operator KerbalEVA(SelectedKerbalEVA selected)
		{
			return selected.Kerbal;
		}
	}
}

