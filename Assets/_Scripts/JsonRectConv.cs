using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class JsonRectConv: JsonConverter
{
	#region implemented abstract members of JsonConverter

	public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
	{
		Rect rect = (Rect)value;
		writer.WriteRawValue (string.Format ("[{0},{1},{2},{3}]", rect.x, rect.y, rect.width, rect.height));
	}

	public override object ReadJson (JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
	{
		float[] fs = serializer.Deserialize<float[]> (reader);
		return new Rect (fs [0], fs [1], fs [2], fs [3]);
	}

	public override bool CanConvert (System.Type objectType)
	{
		return objectType == typeof(Rect);
	}

	#endregion


}
