using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using PogoTools;
using System.IO;

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

	public Unit lastUnit;
	public bool StartCellChooseMode = false;

	private void SceneGUI (SceneView sceneView)
	{
		if (editEnable) {
			Selection.activeGameObject = Stage.gameObject;

			if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {

				if (StartCellChooseMode) {
					StartCellChooseMode = false;
					Stage.start_cell_choose_mode = false;

					mousePositionRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
					if (Physics.Raycast (mousePositionRay, out hit, float.MaxValue, 1 << LayerMask.NameToLayer ("raycast_collider"))) {
						Vector2 direction = ((Vector2)hit.point - lastUnit.Rect.center);
						lastUnit.Cell.Detail.Direction = direction;
						OnStageEdited ();
					}

				} else {
					if (Event.current.control) {
						// Ctrl + 左键
						Unit u = GetMousePositionUnit ();
						if (u != null) {
							if (CellType.None != u.Cell.Type) {
								u.ChangeCellTo (new Cell (){ Type = CellType.None });
								OnStageEdited ();
							}
						}
					} else {
						// 左键
						Unit u = GetMousePositionUnit ();
						if (u != null) {
							if (tbh_cell_type [tbh_gird_mode_idx] != u.Cell.Type) {

								Cell cell = new Cell (){ Type = tbh_cell_type [tbh_gird_mode_idx] };
								if (cell.Type == CellType.START) {
									cell.Detail = new CellDetail ();
								}

								u.ChangeCellTo (cell);
								OnStageEdited ();
							}
						}
					}
				}
			}

			if (Event.current.type == EventType.MouseDown && Event.current.button == 1) {
				Unit u = GetMousePositionUnit ();
				lastUnit = u;
				StartCellChooseMode = false;
				Stage.start_cell_choose_mode = false;

				// 右键
				PopupWindow.Show (
					new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y, 0f, 0f), 
					new StageMakerPopupWindow () {
						stage = Stage,
						unit = GetMousePositionUnit (),
						stageMaker = this
					}
				);
			}

			if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag) {

				if (StartCellChooseMode) {
					mousePositionRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
					if (Physics.Raycast (mousePositionRay, out hit, float.MaxValue, 1 << LayerMask.NameToLayer ("raycast_collider"))) {
						Stage.gizmo_current_point = hit.point;
						SceneView.RepaintAll ();
					}

					if (Stage.SMFocusUnit != lastUnit) {
						Stage.SMFocusUnit = lastUnit;
						SceneView.RepaintAll ();
					} else if (lastUnit == null) {
						Stage.SMFocusUnit = null;
					}
				} else {
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
		OnChoosedStageNumChanged ();
	}

	void CloseEdit ()
	{
		SceneView.onSceneGUIDelegate -= SceneGUI;
		Stage.SMFocusUnit = null;
		Stage = null;
	}

	public int last_choosed_stage_num = 0;

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
			EditorGUILayout.HelpBox ("没有连接到 Hierarchy 中的 Stage", MessageType.Warning);
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
		if (last_choosed_stage_num != choosed_stage_num) {
			OnChoosedStageNumChanged ();
		}
		last_choosed_stage_num = choosed_stage_num;
		GUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Load"))
			LoadFromFile ();
		if (GUILayout.Button ("Save"))
			SaveToFile ();
		GUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal ();
		bool temp_auto_load = GUILayout.Toggle (auto_load, "Auto Load");
		if (temp_auto_load != auto_load) {
			auto_load = temp_auto_load;
			OnChoosedStageNumChanged ();
		}
		bool temp_auto_save = GUILayout.Toggle (auto_save, "Auto Save");
		if (temp_auto_save != auto_save) {
			auto_save = temp_auto_save;
			OnStageEdited ();
		}

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

	public string stageNum2FileNameWithSuffix (int stage_num)
	{
		return string.Format ("stage_{0:000}.txt", stage_num);
	}

	public string StageDataRoot {
		get {
			string root = Path.Combine (Application.dataPath, "Resources/StageDatas");
			if (Directory.Exists (root) == false) {
				Directory.CreateDirectory (root);
			}
			return root;
		}
	}

	public string StageDataFullPath {
		get {
			return Path.Combine (StageDataRoot, stageNum2FileNameWithSuffix (choosed_stage_num));
		}
	}

	public string ZeroStageDataFullPath {
		get {
			return Path.Combine (StageDataRoot, stageNum2FileNameWithSuffix (0));
		}
	}

	public void SaveToFile (bool log = true)
	{
		if (Stage != null) {
			string data = Stage.Ser ();
			PRDebug.TagLog (tag, tagC, data);
			File.WriteAllText (StageDataFullPath, data);
			if (log)
				PRDebug.TagLog (tag, tagC, "Save to file: " + StageDataFullPath);
			AssetDatabase.Refresh ();
		}
	}

	public void LoadFromFile ()
	{
		if (Stage != null) {
			try {
				string data = File.ReadAllText (StageDataFullPath);
				PRDebug.TagLog (tag, tagC, data);
				Stage.Deser (data);
				PRDebug.TagLog (tag, tagC, "Load from file: " + StageDataFullPath);
			} catch (FileNotFoundException ex) {
				string data = File.ReadAllText (ZeroStageDataFullPath);
				Stage.Deser (data);
				PRDebug.TagLog (tag, tagC, "Do not have " + stageNum2FileNameWithSuffix (choosed_stage_num) + ".");
			}
		}
	}

	public int choosed_stage_num = 0;
	public int max_stage_num = 100;

	public bool auto_load = false;
	public bool auto_save = false;

	public void OnChoosedStageNumChanged ()
	{
		if (auto_load) {
			LoadFromFile ();
		}
	}

	public void OnStageEdited ()
	{
		if (auto_save) {
			SaveToFile (false);
		}
	}
}
