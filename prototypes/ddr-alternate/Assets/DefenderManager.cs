using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

public class DefenderManager : MonoBehaviour {

	[SerializeField]
	private GameObject left;
	[SerializeField]
	private GameObject right;
	[SerializeField]
	private GameObject up;
	[SerializeField]
	private GameObject down;

	[SerializeField]
	private List<Dir> directionQueue = new List<Dir>();

	[SerializeField]
	private GameObject spawnLoc;


	// Use this for initialization
	void Start () {

	}

	private GameObject SpawnDir(Dir dir) {
		switch (dir) {
			case Dir.Up:
				return Instantiate(up) as GameObject;
			case Dir.Left:
				return Instantiate(left) as GameObject;
			case Dir.Right:
				return Instantiate(right) as GameObject;
			case Dir.Down:
				return Instantiate(down) as GameObject;
		}
		return null; // Should never happen
	}

	private void AddCurrKeyToQueue() {
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

	// Update is called once per frame
	void Update () {
		AddCurrKeyToQueue();

		if (directionQueue.Count >= 10) {
			for (int i = 0; i < directionQueue.Count; i++) {
				SpawnDir(directionQueue[i]);
			}
			directionQueue.Clear();
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(spawnLoc.transform.position, 1f);
	}
}