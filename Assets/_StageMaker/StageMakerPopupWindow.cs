using UnityEngine;
using System.Collections;
using UnityEditor;
using Newtonsoft.Json;

public class StageMakerPopupWindow : PopupWindowContent
{
	public Stage stage;

	public Unit unit;

	public StageMaker stageMaker;

	public override Vector2 GetWindowSize ()
	{
		return new Vector2 (200, 150);
	}

	public override void OnGUI (Rect rect)
	{
		if (unit != null) {
			GUILayout.Label (string.Format ("Unit: ({0},{1}), idx:{2}", unit.X, unit.Y, unit.Idx), EditorStyles.boldLabel);
			if (GUILayout.Button ("To JSON")) {
				Debug.Log (unit.Ser ());
			}

			if (unit.Cell.Type == CellType.START) {
				if (GUILayout.Button ("Choose Direction")) {
					stageMaker.StartCellChooseMode = true;
					stage.start_cell_choose_mode = true;
					this.editorWindow.Close ();
				}
			}
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