using System;
using System.Collections.Generic;
using UnityEngine;

using ClickThroughFix;

namespace KerbalAnimation
{
	public abstract class Window
	{
		protected delegate void OnGUICallback();

		protected Window()
		{
			OnGUI = new OnGUICallback(DrawMainWindow);
		}

		protected Rect WindowRect;
		protected string WindowTitle = "";
		protected bool AllowDrag = true;
		protected bool ExpandWidth = false;
		protected bool ExpandHeight = false;

		//abstract methods
		protected abstract void DrawWindow();
		public abstract void Update();

		protected int ID = Guid.NewGuid().ToString().GetHashCode();
		protected OnGUICallback OnGUI = null;

		public void Draw()
		{
			GUI.skin = skin;
			OnGUI.Invoke();
		}
		private void DrawMainWindow()
		{
			WindowRect = ClickThruBlocker.GUILayoutWindow(ID, WindowRect, WindowDelegate, WindowTitle, GUILayout.ExpandWidth(ExpandWidth), GUILayout.ExpandHeight(ExpandHeight));
		}
		private void WindowDelegate(int id)
		{
			DrawWindow();
			if (AllowDrag)
				GUI.DragWindow();
		}

		public KerbalAnimationSuite Suite
		{
			get{return KerbalAnimationSuite.Instance;}
		}
		public GUISkin skin
		{
			get{return Suite.skin;}
		}
	}
}

