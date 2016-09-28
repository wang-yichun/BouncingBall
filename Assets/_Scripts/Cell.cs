using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public enum CellType
{
	None,
	START,
	BRICK,
	STAR,
	FINISH
}

[JsonObject (MemberSerialization.OptIn)]
public class Cell
{
	[JsonProperty ("t")]
	public CellType Type;
}