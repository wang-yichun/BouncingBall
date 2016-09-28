using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using PogoTools;

//[InitializeOnLoad]
//public static class StageMakerStartup
//{
//	static StageMakerStartup ()
//	{
//		StageMaker.Init ();
//	}
//}

public class StageMaker : EditorWindow
{
	
	[MenuItem ("BB/StageMaker... &`")]
	public static void Init ()
	{
		StageMaker window = (StageMaker)EditorWindow.GetWindow (typeof(StageMaker));  
		window.Show ();
		window.editEnable = false;
	}

	Ray mousePositionRay;
	RaycastHit hit;

	Unit GetMousePositionUnit ()
	{
		mousePositionRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
		if (Physics.Raycast (mousePositionRay, out hit, float.MaxValue, 1 << LayerMask.NameToLayer ("raycast_collider"))) {
			return Stage.world2unit (hit.point);
		}
		return null;
	}

	private void SceneGUI (SceneView sceneView)
	{
		if (editEnable) {
			Selection.activeGameObject = Stage.gameObject;

			if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
				// 左键
				Unit u = GetMousePositionUnit ();
				if (u != null) {

					if (tbh_cell_type [tbh_gird_mode_idx] != u.Cell.Type) {
						u.ChangeCellTo (new Cell (){ Type = tbh_cell_type [tbh_gird_mode_idx] });
					}
				}

				PRDebug.TagLog ("Ethan Debug", Color.cyan, "EventType.MouseDown");

//				PRDebug.Log ("EventType.MouseDown", Color.yellow);
//				PRDebug.Log ("Close Color", null);
			}

			if (Event.current.type == EventType.MouseDown && Event.current.button == 1) {
				// 右键
//				Unit u = GetMousePositionUnit ();
//				if (u.Cell.Type != CellType.None) {
//					u.ChangeCellTo (new Cell (){ Type = CellType.None });
//				}
				PopupWindow.Show (
					new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y, 0f, 0f), 
					new StageMakerPopupWindow () {
						stage = Stage,
						unit = GetMousePositionUnit ()
					}
				);
			}

			if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag) {
				Unit u = GetMousePositionUnit ();
				if (Stage.SMFocusUnit != u) {
					Stage.SMFocusUnit = u;
					SceneView.RepaintAll ();
				} else if (u == null) {
					Stage.SMFocusUnit = null;
				}
			}
		}
	}

	public bool editEnable = false;

	private static string[] tbh_gird_mode = new string[] { "空", "球", "地形方块", "星星", "终点" };
	public int tbh_gird_mode_idx;
	public CellType[] tbh_cell_type = new CellType[] {
		CellType.None,
		CellType.START,
		CellType.BRICK,
		CellType.STAR,
		CellType.FINISH
	};

	public Stage Stage;
	public Transform BrickContainer;

	void OpenEdit ()
	{
		SceneView.onSceneGUIDelegate += SceneGUI;
		Stage = GameObject.Find ("Stage").GetComponent<Stage> ();
		BrickContainer = Stage.transform.FindChild ("BrickContainer").transform;
	}

	void CloseEdit ()
	{
		SceneView.onSceneGUIDelegate -= SceneGUI;
		Stage.SMFocusUnit = null;
	}

	void OnGUI ()
	{
		GUILayout.BeginVertical ();
		bool tempEditEnable = EditorGUILayout.BeginToggleGroup ("开启编辑", editEnable);

		if (tempEditEnable != editEnable) {
			editEnable = tempEditEnable;
			if (editEnable) {
				OpenEdit ();
			} else {
				CloseEdit ();
			}
		}

		tbh_gird_mode_idx = GUILayout.Toolbar (tbh_gird_mode_idx, tbh_gird_mode);

		EditorGUILayout.EndToggleGroup ();

		GUILayout.Space (20f);

		if (GUILayout.Button ("保存")) {
			string path = EditorUtility.SaveFilePanelInProject ("保存文件", "level_001", "byte", "将关卡文件保存");
//			Debug.Log ("Path: " + path);
//			Debug.Log (Stage.Ser ());
			PRDebug.Log (Stage.Ser ());
		}

		if (GUILayout.Button ("读取")) {
			string path = EditorUtility.OpenFilePanel ("加载文件", Application.dataPath, "byte");
			Debug.Log ("Path: " + path);
		}

		GUILayout.EndVertical ();
	}
}
