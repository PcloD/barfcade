using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DefenderManager : MonoBehaviour {

	public enum Phase {
		Recording,
		Duplicating
	}

	[System.Serializable]
	public enum Dir {
		Up,
		Down,
		Left,
		Right
	}

	[SerializeField]
	private List<Dir> directionQueue = new List<Dir>();

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		bool up = Input.GetButtonDown("P1_Up");
		bool down = Input.GetButtonDown("P1_Down");
		bool left = Input.GetButtonDown("P1_Left");
		bool right = Input.GetButtonDown("P1_Right");
		if (up) {
			directionQueue.Add(Dir.Up);
		}
		if (down) {
			directionQueue.Add(Dir.Down);
		}
		if (left) {
			directionQueue.Add(Dir.Left);
		}
		if (right) {
			directionQueue.Add(Dir.Right);
		}
	}
}