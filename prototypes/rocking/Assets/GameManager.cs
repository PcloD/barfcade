using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using CarthageGames.Extensions.Color;

public class GameManager : MonoBehaviour {

	public static GameManager g;

	[SerializeField]
	private AnimationCurve temporalDistribution;

	[SerializeField]
	private float visualizationRange = 9f;

	[SerializeField]
	private Color neutralColor;
	[SerializeField]
	private Color goodColor;
	[SerializeField]
	private Color badColor;

	[SerializeField]
	private GameObject shrimpTacoPrefab;

	[SerializeField]
	private GameState[] gameStates;

	private ShrimpValues[] shrimpQueue;

	[System.Serializable]
	private class GameState {
		public Camera myCamera;
		public Rocker myRocker;
		public ShrimpValues myShrimpValues;
		public int shrimpIndex = 0;
		public string controlAxis;
		public Transform tacoSpawnTransform;
		public float metric;

		public float collectionOffset;
		public Transform myScoreTransform;
	}

	[System.Serializable]
	private class ShrimpValues {
		public float desiredMetric = 0f;
		public float metricRange = 0f;
	}

	void OnEnable () {
		if (g == null) {
			g = this;
			shrimpQueue = GenerateAllShrimp(1000); // XXX: Add as needed!
		} else {
			Debug.LogError("Trying to create a second GameManager!");
			Destroy(gameObject);
		}
	}

	private ShrimpValues GenerateOneShrimp() {
		ShrimpValues newShrimpValues = new ShrimpValues();

		float desiredMetric = temporalDistribution.Evaluate(Random.value);
		newShrimpValues.desiredMetric = desiredMetric;
		newShrimpValues.metricRange = 4; // TODO: Think about altering this to create more or less sensitive shrimp

		return newShrimpValues;
	}

	private ShrimpValues[] GenerateAllShrimp(int count) {
		ShrimpValues[] allValues = new ShrimpValues[count];
		for (int i = 0; i < count; i++) {
			allValues[i] = GenerateOneShrimp();
		}
		return allValues;
	}

	private void SpawnTacoForGameState(GameState gameState) {
		GameObject taco = Instantiate(shrimpTacoPrefab, gameState.tacoSpawnTransform.position, Quaternion.identity) as GameObject;
		gameState.myRocker = taco.GetComponent<Rocker>();
		gameState.myRocker.SetControlAxis(gameState.controlAxis);
	}

	private bool gameRunning = false;
	public bool IsGameRunning {
		get { return gameRunning; }
	}

	public void StartGame() {
		gameRunning = true;
		for (int playerIndex = 0; playerIndex < gameStates.Length; playerIndex++) {
			gameStates[playerIndex].myShrimpValues = shrimpQueue[0];
		}
	}

	[SerializeField]
	private float speedToTarget = 10f;
	[SerializeField]
	private float speedToAwake = 2f;
	void FixedUpdate () {
		for (int playerIndex = 0; playerIndex < gameStates.Length; playerIndex++) {
			GameState gameState = gameStates[playerIndex];

			Camera c = gameState.myCamera;
			ShrimpValues shrimp = shrimpQueue[gameState.shrimpIndex];
			float metric = gameState.myRocker.EvaluationMetric;
			float desiredMetric = shrimp.desiredMetric;
			float metricRange = shrimp.metricRange;

			gameState.metric = metric;
			if (metric > desiredMetric + metricRange/2f) {
				c.backgroundColor = ColorExtensions.Slerp(goodColor, badColor, Mathf.Clamp01((metric - desiredMetric - metricRange/2f)/(desiredMetric+visualizationRange)));
			} else {
				c.backgroundColor = ColorExtensions.Slerp(neutralColor, goodColor, Mathf.Clamp01(metric/(desiredMetric - metricRange/2f)));
			}

			if (InRange(gameState.myRocker.RightMetric, desiredMetric, metricRange) && InRange(gameState.myRocker.LeftMetric, desiredMetric, metricRange)) {
				Debug.Log("Asleep!");
				StartNewRoundForGameState(gameState);
			}

		}
	}

	private void StartNewRoundForGameState(GameState gameState) {
		gameState.myRocker.StartMovingToScoreAt(gameState.myScoreTransform.position + Vector3.right * gameState.collectionOffset * gameState.shrimpIndex);

		SpawnTacoForGameState(gameState);
		gameState.shrimpIndex++;
		gameState.myShrimpValues = shrimpQueue[gameState.shrimpIndex];
		gameState.metric = 0f;
	}

	private bool InRange(float m, float d, float range) {
		return m >= d - range/2f && m <= d + range/2f;
	}

}