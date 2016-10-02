using UnityEngine;
using System.Collections;

public class Star : MonoBehaviour
{

	void OnTriggerEnter2D (Collider2D other)
	{
		this.gameObject.SetActive (false);
		Stage.Current.OnStarCollected (this);
	}
}
