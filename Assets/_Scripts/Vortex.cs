using UnityEngine;
using System.Collections;

public class Vortex : MonoBehaviour
{
	public Unit Unit;

	public int NeedCount;
	public int CurrentCount;
	public bool IsFull;

	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.gameObject.CompareTag ("Ball")) {
			if (CurrentCount < NeedCount) {
				CurrentCount++;

				Stage.Current.OnEnterVortex (this);

				if (CurrentCount == NeedCount) {
					IsFull = true;
					Stage.Current.OnVortexFull (this);
					this.gameObject.SetActive (false);
					Unit.VortexFull ();
				}

				Destroy (other.gameObject);
			}

		}
	}
}
