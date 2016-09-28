using UnityEngine;
using System.Collections;
using UnityEditor;
using Newtonsoft.Json;

public class StageMakerPopupWindow : PopupWindowContent
{
	public Stage stage;

	public Unit unit;

	public override Vector2 GetWindowSize ()
	{
		return new Vector2 (200, 150);
	}

	public override void OnGUI (Rect rect)
	{
		GUILayout.Label (string.Format ("Unit: ({0},{1}), idx:{2}", unit.X, unit.Y, unit.Idx), EditorStyles.boldLabel);
		if (GUILayout.Button ("To JSON")) {
			Debug.Log (JsonConvert.SerializeObject (unit));
		}
		GUILayout.FlexibleSpace ();
		if (GUILayout.Button ("Close")) {
			this.editorWindow.Close ();
		}
	}

	public override void OnOpen ()
	{
	}

	public override void OnClose ()
	{
	}
}