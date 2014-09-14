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
			for (int i = 0; i < rockingTacos.Count; i++) {
				rockingTacos[i].StartGame();
			}
			Destroy(menuCamera);
		}
	}
}