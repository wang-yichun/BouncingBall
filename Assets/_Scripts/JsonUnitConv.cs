using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class JsonUnitConv: JsonConverter
{
	#region implemented abstract members of JsonConverter

	public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
	{
		Unit unit = (Unit)value;
		writer.WriteRawValue (unit.Ser ());
	}

	public override object ReadJson (JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
	{
		return Unit.CreateByJSON (reader.ReadAsString ());
	}

	public override bool CanConvert (System.Type objectType)
	{
		return objectType == typeof(Unit);
	}

	#endregion


}
