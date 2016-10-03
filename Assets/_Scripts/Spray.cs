using UnityEngine;
using System.Collections;

public class Spray : MonoBehaviour
{

	public Vector2 Direction;

	public Transform roll;
	public Transform indictor;

	public bool HasInitialized = false;

	void Start ()
	{
		Init ();
	}

	void Init ()
	{
		HasInitialized = true;
		roll = transform.FindChild ("spray_roll");
		indictor = transform.FindChild ("spray_indicator");
	}

	public void SetDirection (Vector2 direction)
	{
		if (!HasInitialized) {
			Init ();
		}
		Direction = direction;
		indictor.up = Direction;
	}

	public void OnSpray() {
		Animator animator_roll = roll.GetComponent<Animator> ();
		animator_roll.Play ("OnSpray");
	}

}
