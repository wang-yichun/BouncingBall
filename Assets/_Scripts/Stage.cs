using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using PogoTools;
using System.Linq;
using UniRx;
using HedgehogTeam.EasyTouch;
using DG.Tweening;

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

	[HideInInspector]
	public Transform DynamicContainer;

	public Unit SMFocusUnit;
	public Unit SMUnit_LB;
	public Unit SMUnit_RT;

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

	public Unit[] world2Units (Vector2 worldPos, int brushWidth, int brushHeight)
	{
		if (Units == null)
			Initialize ();

		float nor_x = worldPos.x + TotalRect.width / 2f;
		float nor_y = worldPos.y + TotalRect.height / 2f;

		if (nor_x >= 0 && nor_x < TotalRect.width && nor_y >= 0 && nor_y < TotalRect.height) {
			// Get X
			List<int> xs = new List<int> ();
			if (brushWidth % 2 == 0) {
				int x = Mathf.FloorToInt ((worldPos.x + TotalRect.width / 2f + UnitRect.width / 2f) / UnitRect.width);
				for (int i = 0; i < brushWidth; i++) {
					int x1 = i - brushWidth / 2 + x;
					if (x1 >= 0 && x1 < UnitCountX)
						xs.Add (x1);
				}
			} else {
				int x = Mathf.FloorToInt ((worldPos.x + TotalRect.width / 2f) / UnitRect.width);
				for (int i = 0; i < brushWidth; i++) {
					int x1 = i - brushWidth / 2 + x;
					if (x1 >= 0 && x1 < UnitCountX)
						xs.Add (x1);
				}
			}

			// Get Y
			List<int> ys = new List<int> ();
			if (brushHeight % 2 == 0) {
				int y = Mathf.FloorToInt ((worldPos.y + TotalRect.height / 2f + UnitRect.height / 2f) / UnitRect.height);
				for (int i = 0; i < brushHeight; i++) {
					int y1 = i - brushHeight / 2 + y;
					if (y1 >= 0 && y1 < UnitCountY)
						ys.Add (y1);
				}
			} else {
				int y = Mathf.FloorToInt ((worldPos.y + TotalRect.height / 2f) / UnitRect.height);
				for (int i = 0; i < brushHeight; i++) {
					int y1 = i - brushHeight / 2 + y;
					if (y1 >= 0 && y1 < UnitCountY)
						ys.Add (y1);
				}
			}

//			int y = Mathf.FloorToInt ((worldPos.y + TotalRect.height / 2f) / UnitRect.height);

			List<Unit> units = new List<Unit> ();
			for (int n = 0; n < ys.Count; n++) {
				int y = ys [n];
				for (int m = 0; m < xs.Count; m++) {
					int x = xs [m];
					int idx = xy2idx (x, y);
					units.Add (Units [idx]);
				}
			}
			return units.ToArray ();
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
		DynamicContainer = transform.FindChild ("DynamicContainer");

		CleanBrickContainer ();
		CleanDynamicContainer ();

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
					Cell = new Cell (){ Type = CellType.NONE },
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

	public void CleanDynamicContainer ()
	{
		while (DynamicContainer.childCount > 0) {
			DestroyImmediate (DynamicContainer.GetChild (0).gameObject);	
		}
	}


	public bool start_cell_choose_mode;
	public Vector2 gizmo_current_point;

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

			Gizmos.color = Color.yellow;
			if (SMUnit_LB != null && SMUnit_RT != null) {
				Gizmos.DrawLine (
					new Vector3 (SMUnit_LB.Rect.x - 100f, SMUnit_LB.Rect.y, 0f),
					new Vector3 (SMUnit_LB.Rect.x + 100f, SMUnit_LB.Rect.y, 0f)
				);
				Gizmos.DrawLine (
					new Vector3 (SMUnit_LB.Rect.x, SMUnit_LB.Rect.y - 100f, 0f),
					new Vector3 (SMUnit_LB.Rect.x, SMUnit_LB.Rect.y + 100f, 0f)
				);
				Gizmos.DrawLine (
					new Vector3 (SMUnit_RT.Rect.x + SMUnit_RT.Rect.width - 100f, SMUnit_RT.Rect.y + SMUnit_RT.Rect.height, 0f),
					new Vector3 (SMUnit_RT.Rect.x + SMUnit_RT.Rect.width + 100f, SMUnit_RT.Rect.y + SMUnit_RT.Rect.height, 0f)
				);
				Gizmos.DrawLine (
					new Vector3 (SMUnit_RT.Rect.x + SMUnit_RT.Rect.width, SMUnit_RT.Rect.y + SMUnit_RT.Rect.height - 100, 0f),
					new Vector3 (SMUnit_RT.Rect.x + SMUnit_RT.Rect.width, SMUnit_RT.Rect.y + SMUnit_RT.Rect.height + 100, 0f)
				);
			}

			if (start_cell_choose_mode) {
				Gizmos.color = Color.blue;
				Gizmos.DrawLine (gizmo_current_point, SMFocusUnit.Rect.center);
			} else {
				if (SMFocusUnit.Cell.Type == CellType.START) {
					Gizmos.color = Color.blue;
					Gizmos.DrawLine (SMFocusUnit.Rect.center, SMFocusUnit.Rect.center + SMFocusUnit.Cell.Detail.Direction);
				}
			}
		}
	}

	public string MakePrefabName (CellType cellType)
	{
		string name = null;
		switch (cellType) {
		case CellType.NONE:
			name = "";
			break;
		case CellType.START:
			name = "ingame_spray";
			break;
		case CellType.BRICK:
			name = "ingame_" + 1;
			break;
		case CellType.STAR:
			name = "ingame_" + 2;
			break;
		case CellType.FINISH:
			name = "ingame_vortex";
			break;
		case CellType.BALL:
			name = "ingame_0";
			break;
		default:
			name = "";
			break;
		}
		return name;
	}


	private Vector2 cell_outside_offset_l = new Vector2 (-5f, 0f);
	private Vector2 cell_outside_offset_r = new Vector2 (5f, 0f);

	public void OnUnitCellChanged (Unit unit, Cell newCell, Cell oldCell)
	{
		bool need_add = false;
		if (unit.GO.Count == 0) {
			if (newCell.Type != CellType.NONE) {
				need_add = true;
			}
		} else {
			if (oldCell != newCell) {
				List<GameObject> gos = unit.GO.FindAll (_ => _.name == MakePrefabName (oldCell.Type));
				for (int i = 0; i < gos.Count; i++) {
					GameObject go = gos [i];

					if (remove_anim_on) {
						Rigidbody2D r = go.GetComponentInChildren<Rigidbody2D> ();
						if (r != null) {
							r.isKinematic = true;
						}
						Collider2D c = go.GetComponentInChildren<Collider2D> ();
						if (c != null) {
							c.isTrigger = true;
						}

						Vector2 offset = new Vector2 (
							                 UnityEngine.Random.Range (-.5f, .5f),
							                 UnityEngine.Random.Range (-1f, -.5f)
						                 );
							
						go.transform.DOLocalJump (unit.Rect.center + offset, .5f, 1, 1f).OnComplete (() => {
							Destroy (go);
						});

						SpriteRenderer[] srs = go.GetComponentsInChildren<SpriteRenderer> ();
						for (int i0 = 0; i0 < srs.Length; i0++) {
							SpriteRenderer sr = srs [i0];
							sr.DOFade (0f, 1f);
						}

						unit.GO.Remove (go);
					} else {
						unit.GO.Remove (go);
						DestroyImmediate (go);
					}
				}

				if (newCell.Type != CellType.NONE) {
					need_add = true;
				}
			}
		}

		if (need_add) {
			int prefab_idx = -1;
			switch (newCell.Type) {
			case CellType.NONE:
				break;
			case CellType.START:
				prefab_idx = 0;
				break;
			case CellType.BRICK:
				prefab_idx = 1;
				break;
			case CellType.STAR:
				prefab_idx = 2;
				break;
			case CellType.GEM:
				prefab_idx = 3;
				break;
			case CellType.FINISH:
				prefab_idx = 4;
				break;
			case CellType.FIXED:
				prefab_idx = 6;
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}

			if (prefab_idx != -1) {
				GameObject go = Instantiate<GameObject> (UnitPrefab [prefab_idx]);
				unit.GO.Insert (0, go);
				go.transform.SetParent (BrickContainer);

				if (this.begin_anim_on) {
					if ((unit.Y) % 2 == 0) {
						float du = (this.UnitCountX - unit.X) * .04f + (this.UnitCountY - unit.Y) * .1f;
						if (begin_anim_time < du)
							begin_anim_time = du;
						SpriteRenderer[] ss = go.GetComponentsInChildren<SpriteRenderer> ();
						for (int i = 0; i < ss.Length; i++) {
							ss [i].color = new Color (1f, 1f, 1f, 0f);
							ss [i].DOFade (1f, du / 2f);
						}
						go.transform.position = unit.Rect.center + cell_outside_offset_l;
						go.transform.DOMove (unit.Rect.center, du).SetEase (Ease.OutBack);
					} else {
						float du = unit.X * .04f + (this.UnitCountY - unit.Y) * .1f;
						if (begin_anim_time < du)
							begin_anim_time = du;
						SpriteRenderer[] ss = go.GetComponentsInChildren<SpriteRenderer> ();
						for (int i = 0; i < ss.Length; i++) {
							ss [i].color = new Color (1f, 1f, 1f, 0f);
							ss [i].DOFade (1f, du / 2f);
						}
						go.transform.position = unit.Rect.center + cell_outside_offset_r;
						go.transform.DOMove (unit.Rect.center, du).SetEase (Ease.OutBack);
					}
				} else {
					go.transform.position = unit.Rect.center;
				}

				go.name = MakePrefabName (newCell.Type);

				if (go.CompareTag ("Vortex")) {
					Vortex vortex = go.GetComponent<Vortex> ();
					vortex.NeedCount = newCell.Detail.VortexNeedCount;
					vortex.CurrentCount = 0;
				}

				if (go.CompareTag ("Spray")) {
					Spray spray = go.GetComponent<Spray> ();
					spray.SetDirection (newCell.Detail.Direction);
				}
			}
		}
	}

	public void OnStarCollected (Star star, Unit unit)
	{
		collected_star_count++;
	}

	public void OnEnterVortex (Vortex vortex)
	{
		enter_vortex_count++;
	}

	public void OnVortexFull (Vortex vortex)
	{
		full_vortex_count++;

		if (full_vortex_count == VortexNeedFull) {
			OnGameWin ();
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
			if (u.Cell.Type != CellType.NONE) {
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

		begin_anim_time = 0f;
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
		current = GameObject.FindGameObjectWithTag ("Stage").GetComponent<Stage> ();

		StopGame ();
		remove_anim_on = true;
		EasyTouchSubscribe ();
//		LoadStage ();
	}

	public int stage_num;
	public bool need_reload;

	public bool begin_anim_on;
	public bool remove_anim_on;

	private float begin_anim_time;

	public bool is_win;


	private static Stage current;

	public static Stage Current {
		get {
			return current;
		}
	}

	public void LoadStage ()
	{
		begin_anim_on = true;
		is_win = false;

		if (spray_disp_0 != null) {
			spray_disp_0.Dispose ();
		}
		if (spray_disp_1 != null) {
			spray_disp_1.Dispose ();
		}

		TextAsset ta = Resources.Load<TextAsset> (string.Format ("StageDatas/stage_{0:000}", stage_num));
		this.Deser (ta.text);

//		for (int i = 0; i < BrickContainer.childCount; i++) {
//			Transform t = BrickContainer.GetChild (i);
//			if (t.gameObject.name == "ingame_0") {
//				t.gameObject.SetActive (false);
//			}
//		}

		StartUnitList = Units.Where (_ => _.Cell.Type == CellType.START).ToList ();

		VortexNeedFull = Units.Where (_ => _.Cell.Type == CellType.FINISH).Count ();

		Units.Where (_ => _.Cell.Type == CellType.STAR).ToList ().ForEach (u0 => {
			Star star = u0.GO [0].GetComponent<Star> ();
			star.Unit = u0;
		});

		Units.Where (_ => _.Cell.Type == CellType.FINISH).ToList ().ForEach (u0 => {
			Vortex vortex = u0.GO [0].GetComponent<Vortex> ();
			vortex.Unit = u0;
		});

		need_reload = false;
	}


	public int collected_star_count;
	public int enter_vortex_count;
	public int full_vortex_count;

	IDisposable spray_disp_0;
	IDisposable spray_disp_1;

	public void StartGame ()
	{
		if (need_reload) {
			EasyTouchUnsubscribe ();
			LoadStage ();
		}
		need_reload = true;

		collected_star_count = 0;
		enter_vortex_count = 0;
		full_vortex_count = 0;

		for (int i = 0; i < StartUnitList.Count; i++) {
			Unit u = StartUnitList [i];

			// idx:5, Ball
			GameObject go = UnitPrefab [5];

			spray_disp_0 = Observable.Timer (TimeSpan.FromSeconds (begin_anim_time)).Subscribe (_0 => {

				begin_anim_on = false;
				remove_anim_on = true;

				EasyTouchSubscribe ();

				spray_disp_1 = Observable.Interval (TimeSpan.FromSeconds (1)).Subscribe (_ => {
					GameObject real = Instantiate<GameObject> (go);
					real.transform.SetParent (DynamicContainer);
					real.transform.position = u.Rect.center;
					real.SetActive (true);
					Rigidbody2D r2d = real.GetComponent<Rigidbody2D> ();
					r2d.AddForce (u.Cell.Detail.Direction * 100f);

					Spray spray = u.GO [0].GetComponent<Spray> ();
					spray.OnSpray ();

				}).AddTo (go);
			}).AddTo (go);
		}
	}

	public void StopGame ()
	{
		if (spray_disp_0 != null) {
			spray_disp_0.Dispose ();
		}
		if (spray_disp_1 != null) {
			spray_disp_1.Dispose ();
		}

		EasyTouchUnsubscribe ();

		TextAsset ta = Resources.Load<TextAsset> (string.Format ("StageDatas/stage_{0:000}", 0));
		this.Deser (ta.text);
	}

	public void OnGameWin ()
	{
		PRDebug.TagLog ("Stage", Color.cyan, "OnGameWin()");
		if (spray_disp_0 != null) {
			spray_disp_0.Dispose ();
		}
		if (spray_disp_1 != null) {
			spray_disp_1.Dispose ();
		}
		AnimClearStage ();
		is_win = true;

		MenuManagerEx.Instance.GoBack ();
	}

	void EasyTouchSubscribe ()
	{
		EasyTouch.On_TouchStart += EasyTouch_On_TouchStart;
		EasyTouch.On_TouchDown += EasyTouch_On_TouchDown;
		EasyTouch.On_TouchUp += EasyTouch_On_TouchUp;
	}

	void EasyTouchUnsubscribe ()
	{
		EasyTouch.On_TouchStart -= EasyTouch_On_TouchStart;
		EasyTouch.On_TouchDown -= EasyTouch_On_TouchDown;
		EasyTouch.On_TouchUp -= EasyTouch_On_TouchUp;
	}

	RaycastHit hit;

	CellType fingerCellType;

	void EasyTouch_On_TouchStart (Gesture gesture)
	{
		Ray r = Camera.main.ScreenPointToRay (gesture.position);
		if (Physics.Raycast (r, out hit, float.MaxValue, 1 << LayerMask.NameToLayer ("raycast_collider"))) {
			Unit u = this.world2unit ((Vector2)hit.point);
//			PRDebug.Log (u.Ser (), Color.yellow);

			if (u != null) {
				if (u.Cell.Type == CellType.BRICK) {
					u.ChangeCellTo (new Cell (){ Type = CellType.NONE });
					fingerCellType = CellType.NONE;
				} else if (u.Cell.Type == CellType.NONE) {
					u.ChangeCellTo (new Cell (){ Type = CellType.BRICK });
					fingerCellType = CellType.BRICK;

					if (is_win) {
						u.ChangeCellTo (new Cell (){ Type = CellType.NONE });
					}
				}
			}
		}
	}

	void EasyTouch_On_TouchDown (Gesture gesture)
	{
		Ray r = Camera.main.ScreenPointToRay (gesture.position);
		if (Physics.Raycast (r, out hit, float.MaxValue, 1 << LayerMask.NameToLayer ("raycast_collider"))) {
			Unit u = this.world2unit ((Vector2)hit.point);
//			PRDebug.Log (u.Ser (), Color.yellow);

			if (u != null) {
				if (u.Cell.Type == CellType.BRICK && fingerCellType == CellType.NONE) {
					u.ChangeCellTo (new Cell (){ Type = CellType.NONE });
				} else if (u.Cell.Type == CellType.NONE && fingerCellType == CellType.BRICK) {
					u.ChangeCellTo (new Cell (){ Type = CellType.BRICK });

					if (is_win) {
						u.ChangeCellTo (new Cell (){ Type = CellType.NONE });
					}
				}
			}
		}
	}

	void EasyTouch_On_TouchUp (Gesture gesture)
	{
	}

	List<Unit> StartUnitList;
	public int VortexNeedFull;

	void OnGUI ()
	{
		if (GUILayout.Button ("START")) {
			need_reload = true;
			StartGame ();
		}
		if (GUILayout.Button ("STOP")) {
			StopGame ();
		}
	}

	void AnimClearStage ()
	{
		for (int i = 0; i < Units.Length; i++) {
			Unit u = Units [i];
			u.ChangeCellTo (new Cell (){ Type = CellType.NONE });
		}
	}
}
