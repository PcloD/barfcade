using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuNavigation : MonoBehaviour {

	[SerializeField]
	private Camera menuCamera;

	[SerializeField]
	private List<Rocker> rockingTacos = new List<Rocker>();

	void Update () {
		if (Input.anyKeyDown) {
			GameManager.g.StartGame();
			Destroy(menuCamera);
		}
	}
}