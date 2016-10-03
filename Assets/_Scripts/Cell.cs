using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public enum CellType
{
	None,
	START,
	BRICK,
	STAR,
	GEM,
	FINISH,
	BALL
}

[JsonObject (MemberSerialization.OptIn)]
public class Cell
{
	[JsonProperty ("t")]
	public CellType Type;

	[JsonProperty ("d")]
	public CellDetail Detail;
}

public class CellDetail
{
	[JsonProperty ("d")]
	[JsonConverter (typeof(JsonVector2Conv))]
	public Vector2 Direction;

	[JsonProperty("v")]
	public int VortexNeedCount;
}
