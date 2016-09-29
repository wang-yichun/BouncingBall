using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

[JsonObject (MemberSerialization.OptIn)]
public class Stage : MonoBehaviour, ISer
{
	public bool NeedInitialize;

	public GameObject[] UnitPrefab;

	[JsonProperty ("ur")]
	[JsonConverter (typeof(JsonRectConv))]
	public Rect UnitRect;

	[JsonProperty ("ucx")]
	public int UnitCountX;

	[JsonProperty ("ucy")]
	public int UnitCountY;

	[HideInInspector]
	public int UnitCount;
	[HideInInspector]
	public Rect TotalRect;

	[HideInInspector]
	public Unit[] Units;

	[HideInInspector]
	public Transform BrickContainer;

	public Unit SMFocusUnit;

	public int xy2idx (int x, int y)
	{
		return y * UnitCountX + x;
	}

	public Unit world2unit (Vector2 worldPos)
	{
		if (Units == null)
			Initialize ();

		float nor_x = worldPos.x + TotalRect.width / 2f;
		float nor_y = worldPos.y + TotalRect.height / 2f;

		if (nor_x >= 0 && nor_x < TotalRect.width && nor_y >= 0 && nor_y < TotalRect.height) {
			int x = Mathf.FloorToInt ((worldPos.x + TotalRect.width / 2f) / UnitRect.width);
			int y = Mathf.FloorToInt ((worldPos.y + TotalRect.height / 2f) / UnitRect.height);
			int idx = xy2idx (x, y);
			if (idx >= 0 && idx < UnitCount) {
				return Units [idx];
			}
		}
		return null;
	}

	public void Traverse (Action<Unit> action)
	{
		for (int i = 0; i < UnitCount; i++) {
			action.Invoke (Units [i]);
		}
	}

	public void Initialize ()
	{
		NeedInitialize = false;

		BrickContainer = transform.FindChild ("BrickContainer");

		CleanBrickContainer ();

		UnitCount = UnitCountX * UnitCountY;
		TotalRect = new Rect (0f, 0f, UnitRect.width * UnitCountX, UnitRect.height * UnitCountY);
		TotalRect.x = -TotalRect.width / 2f;
		TotalRect.y = -TotalRect.height / 2f;

		Units = new Unit[UnitCount];

		for (int y = 0; y < UnitCountY; y++) {
			for (int x = 0; x < UnitCountX; x++) {
				int idx = xy2idx (x, y);
				Rect rect = new Rect ();
				rect.x = x * UnitRect.width + TotalRect.x;
				rect.y = y * UnitRect.height + TotalRect.y;
				rect.width = UnitRect.width;
				rect.height = UnitRect.height;

				Units [idx] = new Unit () {
					Stage = this,
					X = x,
					Y = y,
					Idx = idx,
					Rect = rect,
					Cell = new Cell (){ Type = CellType.None },
					GO = new List<GameObject> ()
				};
			}
		}
	}

	public void CleanBrickContainer ()
	{
		while (BrickContainer.childCount > 0) {
			DestroyImmediate (BrickContainer.GetChild (0).gameObject);	
		}
	}

	void OnDrawGizmosSelected ()
	{
		if (Units == null || NeedInitialize) {
			Initialize ();	
		}

		Traverse (u => {
			Gizmos.color = Color.gray;
			Gizmos.DrawWireCube (u.Rect.center, new Vector3 (u.Rect.width, u.Rect.height, 1f));

		});

		if (SMFocusUnit != null) {

			if (Units == null || NeedInitialize) {
				Initialize ();	
			}

			Gizmos.color = Color.green;
			Gizmos.DrawLine (
				new Vector3 (SMFocusUnit.Rect.x - 100f, SMFocusUnit.Rect.y, 0f),
				new Vector3 (SMFocusUnit.Rect.x + 100f, SMFocusUnit.Rect.y, 0f)
			);
			Gizmos.DrawLine (
				new Vector3 (SMFocusUnit.Rect.x, SMFocusUnit.Rect.y - 100f, 0f),
				new Vector3 (SMFocusUnit.Rect.x, SMFocusUnit.Rect.y + 100f, 0f)
			);
			Gizmos.DrawLine (
				new Vector3 (SMFocusUnit.Rect.x + SMFocusUnit.Rect.width - 100f, SMFocusUnit.Rect.y + SMFocusUnit.Rect.height, 0f),
				new Vector3 (SMFocusUnit.Rect.x + SMFocusUnit.Rect.width + 100f, SMFocusUnit.Rect.y + SMFocusUnit.Rect.height, 0f)
			);
			Gizmos.DrawLine (
				new Vector3 (SMFocusUnit.Rect.x + SMFocusUnit.Rect.width, SMFocusUnit.Rect.y + SMFocusUnit.Rect.height - 100, 0f),
				new Vector3 (SMFocusUnit.Rect.x + SMFocusUnit.Rect.width, SMFocusUnit.Rect.y + SMFocusUnit.Rect.height + 100, 0f)
			);
		}
	}

	public string MakePrefabName (CellType cellType)
	{
		string name = null;
		switch (cellType) {
		case CellType.None:
			name = "";
			break;
		case CellType.START:
			name = "ingame_" + 0;
			break;
		case CellType.BRICK:
			name = "ingame_" + 1;
			break;
		case CellType.STAR:
			name = "ingame_" + 2;
			break;
		case CellType.FINISH:
			name = "ingame_" + 3;
			break;
		default:
			name = "";
			break;
		}
		return name;
	}

	public void OnUnitCellChanged (Unit unit, Cell newCell, Cell oldCell)
	{
		bool need_add = false;
		if (unit.GO.Count == 0) {
			if (newCell.Type != CellType.None) {
				need_add = true;
			}
		} else {
			if (oldCell != newCell) {
				List<GameObject> gos = unit.GO.FindAll (_ => _.name == MakePrefabName (oldCell.Type));
				for (int i = 0; i < gos.Count; i++) {
					DestroyImmediate (gos [i]);
				}

				if (newCell.Type != CellType.None) {
					need_add = true;
				}
			}
		}

		if (need_add) {
			int prefab_idx = -1;
			if (newCell.Type == CellType.BRICK) {
				prefab_idx = 1;
			}

			if (prefab_idx != -1) {
				GameObject go = Instantiate<GameObject> (UnitPrefab [1]);
				unit.GO.Add (go);
				go.transform.SetParent (BrickContainer);
				go.transform.position = unit.Rect.center;
				go.name = MakePrefabName (newCell.Type);
			}
		}
	}

	#region ISer implementation

	[JsonProperty ("us")]
	List<Unit> preSerUnits;

	public string Ser ()
	{
		preSerUnits = new List<Unit> ();
		for (int i = 0; i < Units.Length; i++) {
			Unit u = Units [i];
			if (u.Cell.Type == CellType.BRICK) {
				preSerUnits.Add (u);
			}
		}
		return JsonConvert.SerializeObject (this, Formatting.Indented, new JsonUnitConv ());
	}

	public void Deser (string json)
	{
		Stage s = CreateByJSON (json);
		UnitRect = s.UnitRect;
		UnitCountX = s.UnitCountX;
		UnitCountY = s.UnitCountY;

		Initialize ();

		for (int i = 0; i < s.preSerUnits.Count; i++) {
			Unit u = s.preSerUnits [i];
			Units [u.Idx].ChangeCellTo (u.Cell);
		}
	}

	public static Stage CreateByJSON (string json)
	{
		return JsonConvert.DeserializeObject<Stage> (json);
	}

	#endregion

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
