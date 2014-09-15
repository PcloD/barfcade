using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuNavigation : MonoBehaviour {

	[SerializeField]
	private GameObject menu;

	void Update () {
		if (Input.anyKeyDown) {
			GameManager.g.StartGame();
			Destroy(menu);
		}
	}
}