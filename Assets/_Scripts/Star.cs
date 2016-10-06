using UnityEngine;
using System.Collections;

public class Star : MonoBehaviour
{
	public Unit Unit;

	void OnTriggerEnter2D (Collider2D other)
	{
		if (Stage.Current.ignoreStar)
			return;
		
		if (other.CompareTag ("Ball")) {
			this.gameObject.SetActive (false);
			Unit.StarCollect ();
			Stage.Current.OnStarCollected (this, Unit);
		}
	}
}
