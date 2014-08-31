using UnityEngine;
using System.Collections;

public class TileFall : MonoBehaviour {
	[SerializeField]
	private float movementSpeed = 10f;

	void FixedUpdate () {
		Vector3 newPos = transform.position - Vector3.up * Time.fixedDeltaTime * movementSpeed;
		transform.position = newPos;
	}
}
