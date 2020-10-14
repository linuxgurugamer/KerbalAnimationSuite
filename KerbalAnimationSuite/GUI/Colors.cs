using System;
using UnityEngine;

namespace KerbalAnimation
{
	public static class Colors
	{
		public const string DefaultMessageColor = "#1ef962";
		public const string ErrorMessageColor = "#ff0022";
		public const string Orange = "orange";
		public const string KSPLabelOrange = "#ffa500";
		public const string Grey = "#9ba6aa";
		public const string Information = "#ffffff";
		public const string KSPLabelGreen = "#b7fe00";
		public const string SelectedColor = "#1fe62c";
		public const string KeyframeColor = "#009aff";
		public const string SelectedKeyframeColor = "#ff6600";
		public const string AddButtonColor = "#00ff00";
		public const string RemoveButtonColor = "#ff0000";

		public static string ColorToHex(Color color)
		{
			return "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		}
		public static Color HexToColor(string hex)
		{
			if (!hex.StartsWith("#")) hex = "#" + hex;
			byte r = byte.Parse(hex.Substring(1,2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(3,2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(5,2), System.Globalization.NumberStyles.HexNumber);
			return new Color32(r,g,b, 255); //implicitly casts to Color
		}
	}
}

