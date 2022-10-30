using UnityEditor;
using UnityEngine;

namespace RootMotion {

	// Custom drawer for the LargeHeader attribute
	[CustomPropertyDrawer (typeof (LargeHeader))]
	public class LargeHeaderDrawer : DecoratorDrawer 
	{
		// Used to calculate the height of the box
		public static Texture2D lineTex = null;
		private GUIStyle style;
		
		LargeHeader largeHeader { get { return ((LargeHeader) attribute); } }

		// Get the height of the element
		public override float GetHeight () 
		{
			return base.GetHeight () * 2f;
		}
		
		
		// Override the GUI drawing for this attribute
		public override void OnGUI (Rect pos) 
		{	
			// Get the color the line should be
			Color color = Color.white;
			switch (largeHeader.color.ToString().ToLower())
			{
			case "white": color = Color.white; break;
			case "red": color = Color.red; break;
			case "blue": color = Color.blue; break;
			case "green": color = Color.green; break;
			case "gray": color = Color.gray; break;
			case "grey": color = Color.grey; break;
			case "black": color = Color.black; break;
			}

			color *= 0.7f;

			style = new GUIStyle(GUI.skin.label);
			style.fontSize = 16;
			style.fontStyle = FontStyle.Normal;
			style.alignment = TextAnchor.LowerLeft;
			GUI.color = color;

			Rect labelRect = pos;
			//labelRect.y += 10;
			EditorGUI.LabelField(labelRect, largeHeader.name, style);

			GUI.color = Color.white;
		}
	}
}