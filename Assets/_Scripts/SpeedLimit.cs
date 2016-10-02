using UnityEngine;
using System.Collections;

public class SpeedLimit : MonoBehaviour
{
	Rigidbody2D r2d;

	float maxVelocity = 10;

	void Start ()
	{
		r2d = GetComponent<Rigidbody2D> ();
	}

	void FixedUpdate ()
	{
		if (r2d.velocity.magnitude > maxVelocity) {
			r2d.velocity = r2d.velocity.normalized * maxVelocity;
		}
	}
}
