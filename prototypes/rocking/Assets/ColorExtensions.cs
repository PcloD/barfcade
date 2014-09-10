// Adapted from http://wiki.unity3d.com/index.php/Colorx

using UnityEngine;
using System.Collections;

namespace CarthageGames.Extensions.Color
{
	public static class ColorExtensions
	{
		// These functions are accessible from any Color struct.  
		// (Put this script in the Plugins folder for Javascript access)

		public static UnityEngine.Color Slerp (this UnityEngine.Color a, UnityEngine.Color b, float t)
		{
			return (HSBColor.Lerp (HSBColor.FromColor (a), HSBColor.FromColor (b), t)).ToColor ();
		}
		
		// To use the following functions, you must input the Color you wish to change.
		// Example:
		// myColor.H(180, ref myColor);
		
		// You can either manipulate the values on a scale from 0 to 360 or
		// on a scale from 0 to 1.
		
		public static void H (this UnityEngine.Color c, int hue0to360, ref UnityEngine.Color thisColor)
		{
			HSBColor temp = HSBColor.FromColor (c);
			temp.h = (hue0to360 / 360.0f);
			thisColor = HSBColor.ToColor (temp);
		}
		
		public static void H (this UnityEngine.Color c, float hue0to1, ref UnityEngine.Color thisColor)
		{
			HSBColor temp = HSBColor.FromColor (thisColor);
			temp.h = hue0to1;
			thisColor = HSBColor.ToColor (temp);
		}
		
		public static void S (this UnityEngine.Color c, int saturation0to360, ref UnityEngine.Color thisColor)
		{
			HSBColor temp = HSBColor.FromColor (thisColor);
			temp.s = saturation0to360 / 360.0f;
			thisColor = HSBColor.ToColor (temp);
		}
		
		public static void S (this UnityEngine.Color c, float saturation0to1, ref UnityEngine.Color thisColor)
		{
			HSBColor temp = HSBColor.FromColor (thisColor);
			temp.s = saturation0to1;
			thisColor = HSBColor.ToColor (temp);
		}
		
		public static void B (this UnityEngine.Color c, int brightness0to360, ref UnityEngine.Color thisColor)
		{
			HSBColor temp = HSBColor.FromColor (thisColor);
			temp.b = brightness0to360 / 360.0f;
			thisColor = HSBColor.ToColor (temp);
		}
		
		public static void B (this UnityEngine.Color c, float brightness0to1, ref UnityEngine.Color thisColor)
		{
			HSBColor temp = HSBColor.FromColor (thisColor);
			temp.b = brightness0to1;
			thisColor = HSBColor.ToColor (temp);
		}
	}
}