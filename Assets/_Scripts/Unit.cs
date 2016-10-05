using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[JsonObject (MemberSerialization.OptIn)]
public class Unit : ISer
{
	public Stage Stage;

	public int X;
	public int Y;

	[JsonProperty ("i")]
	public int Idx;

	public Rect Rect;

	[JsonProperty ("c")]
	public Cell Cell;

	public List<GameObject> GO;

	public void ChangeCellTo (Cell cell)
	{
		Cell oc = this.Cell;
		Stage.OnUnitCellChanged (this, cell, oc);
		this.Cell = cell;
	}

	public void StarCollect ()
	{
		Cell = new Cell () { Type = CellType.None };
	}

	#region ISer implementation

	public string Ser ()
	{
		return JsonConvert.SerializeObject (this, Formatting.None, new JsonSerializerSettings{ NullValueHandling = NullValueHandling.Ignore });
	}

	public void Deser (string json)
	{
		Unit u = CreateByJSON (json);
		X = u.X;
		Y = u.Y;
		Cell = u.Cell;

		GO = new List<GameObject> ();
		Stage = null;
	}

	#endregion

	public static Unit CreateByJSON (string json)
	{
		return JsonConvert.DeserializeObject<Unit> (json);
	}
}