using UnityEngine;
using System.Collections;

public class Ground : MonoBehaviour
{
	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.gameObject.name == "ingame_0(Clone)") {
			Destroy (other.gameObject);
		}
	}
}
