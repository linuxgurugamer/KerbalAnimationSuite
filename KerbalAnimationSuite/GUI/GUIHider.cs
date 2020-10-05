using System;
using UnityEngine;

namespace KerbalAnimation
{
	[KSPAddon(KSPAddon.Startup.Instantly, false)]
	public sealed class GUIHider : MonoBehaviour
	{
		public static bool ShowUI //set by pressing F2
		{get; private set;}

		void Start()
		{
			//set defaults
			ShowUI = true;

			//add GameEvents
			GameEvents.onShowUI.Add(OnShowUI);
			GameEvents.onHideUI.Add(OnHideUI);
		}

		private void OnShowUI()
		{
			ShowUI = true;
		}
		private void OnHideUI()
		{
			ShowUI = false;
		}
	}
}

