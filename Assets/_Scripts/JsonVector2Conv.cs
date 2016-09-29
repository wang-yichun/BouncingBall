using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class JsonVector2Conv: JsonConverter
{
	#region implemented abstract members of JsonConverter

	public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
	{
		Vector2 v = (Vector2)value;
		writer.WriteRawValue (string.Format ("[{0},{1}]", v.x, v.y));
	}

	public override object ReadJson (JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
	{
		float[] fs = serializer.Deserialize<float[]> (reader);
		return new Vector2 (fs [0], fs [1]);
	}

	public override bool CanConvert (System.Type objectType)
	{
		return objectType == typeof(Vector2);
	}

	#endregion


}
