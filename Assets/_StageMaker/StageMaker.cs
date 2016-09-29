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
	public static readonly string tag = "Stage Maker";
	public static readonly Color tagC = new Color (1f, .68f, 0f);

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
		Stage = null;
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

		if (Stage == null) {
			EditorGUILayout.HelpBox ("没有连接到 Hierarchy 中的 Stage", MessageType.Error);
		}


		tbh_gird_mode_idx = GUILayout.Toolbar (tbh_gird_mode_idx, tbh_gird_mode);

		EditorGUILayout.EndToggleGroup ();

		GUILayout.Space (20f);

		GUILayout.BeginHorizontal ();
		if (GUILayout.Button ("|<")) {
			choosed_stage_num = 1;
		}
		if (GUILayout.Button ("<")) {
			if (choosed_stage_num > 1)
				choosed_stage_num--;
		}
		choosed_stage_num = EditorGUILayout.DelayedIntField (choosed_stage_num, GUILayout.MaxWidth (80));
		if (GUILayout.Button (">")) {
			if (choosed_stage_num < 100)
				choosed_stage_num++;
		}
		if (GUILayout.Button (">|")) {
			choosed_stage_num = max_stage_num;
		}
		GUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal ();

		if (GUILayout.Button ("Load"))
			LoadFromFile ();
		if (GUILayout.Button ("Save"))
			SaveToFile ();

		GUILayout.EndHorizontal ();


		GUILayout.Space (20f);
		GUILayout.BeginHorizontal ();
		EditorGUILayout.PrefixLabel ("关卡文件");
		EditorGUILayout.SelectableLabel (stageNum2FileName (choosed_stage_num));
		GUILayout.EndHorizontal ();


		GUILayout.EndVertical ();
	}

	public string stageNum2FileName (int stage_num)
	{
		return string.Format ("stage_{0:000}", stage_num);
	}

	public void SaveToFile ()
	{
		if (Stage != null) {
			PRDebug.TagLog (tag, tagC, Stage.Ser ());
		}
	}

	public void LoadFromFile ()
	{
	}

	public int choosed_stage_num = 0;
	public int max_stage_num = 100;
}
