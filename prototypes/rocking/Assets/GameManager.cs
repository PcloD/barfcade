using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using CarthageGames.Extensions.Color;

public class GameManager : MonoBehaviour {

	public static GameManager g;

	[SerializeField]
	private GameObject winTextPrefab;

	[SerializeField]
	private Camera globalCamera;

	private float timer = 25f; // SET TIME FOR GAME HERE

	[SerializeField]
	private int simulateSuccessCount = 10;

	[SerializeField]
	private TextMesh timeDisplay;

	[SerializeField]
	private AnimationCurve temporalDistribution;

	[SerializeField]
	private AnimationCurve metricCurve;

	[SerializeField]
	private AnimationCurve eatingCurve;

	[SerializeField]
	private float visualizationRange = 9f;

	[SerializeField]
	private float initialMetricRange = 8f;

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

		public Transform myMonster;
		public Transform myMonsterMandible;
		public Transform myMonsterMouthCheckpoint;
		public Transform myMonsterMouth;

		public float closedRotation = 367.72f;
		public float openRotation = 336.1285f;

		public List<GameObject> scoreTacos = new List<GameObject>();
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

	private ShrimpValues GenerateOneShrimp(int index, int count) {
		ShrimpValues newShrimpValues = new ShrimpValues();

		float desiredMetric = temporalDistribution.Evaluate(Random.value);
		newShrimpValues.desiredMetric = desiredMetric;
		newShrimpValues.metricRange = metricCurve.Evaluate(index/count);

		return newShrimpValues;
	}

	private ShrimpValues[] GenerateAllShrimp(int count) {
		ShrimpValues[] allValues = new ShrimpValues[count];
		for (int i = 0; i < count; i++) {
			allValues[i] = GenerateOneShrimp(i, count);
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

	private bool endSequenceStarted = false;

	private int firstBlood = -1;
	void FixedUpdate () {
		if (!gameRunning) {
			timeDisplay.text = ""; // Hide time in menu
			return;
		}

		timer -= Time.fixedDeltaTime;
		timeDisplay.text = Mathf.Floor(timer + 0.5f).ToString();
		if (timer <= 0f) {
			timeDisplay.text = ""; // Hide time in menu
			if (!endSequenceStarted) {
				StartCoroutine(KillTacos ());
				StartCoroutine(MoveInAndEat ());
				endSequenceStarted = true;
			}
			return;
		}

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

			if (InRange(gameState.myRocker.RightMetric, desiredMetric, metricRange) && InRange(gameState.myRocker.LeftMetric, desiredMetric, metricRange) ||
				simulateSuccessCount > 0) {
				if (simulateSuccessCount > 0) {
					simulateSuccessCount--;
				}
				// Debug.Log("Asleep!");

				if (firstBlood < 0) { // HACK TO MAKE FIRST TIME WINNER WIN
					firstBlood = playerIndex;
				}

				gameState.scoreTacos.Add(gameState.myRocker.gameObject);
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

	private IEnumerator KillTacos () {
		YieldInstruction wait = new WaitForFixedUpdate();
		float counter = 0.5f;
		while (counter > 0f) {
			yield return wait;
			for (int playerIndex = 0; playerIndex < gameStates.Length; playerIndex++) {
				GameState gameState = gameStates[playerIndex];
				counter -= Time.fixedDeltaTime;
				gameState.myRocker.transform.localScale = Vector3.one * counter/0.5f;
			}
		}
		for (int playerIndex = 0; playerIndex < gameStates.Length; playerIndex++) {
			GameState gameState = gameStates[playerIndex];
			Destroy(gameState.myRocker.gameObject);
		}
	}

	private IEnumerator MoveInAndEat () {
		YieldInstruction wait = new WaitForFixedUpdate();
		float counter = 0f;
		float duration = 1f;
		List<Vector3> initialPositions = new List<Vector3>();
		for (int playerIndex = 0; playerIndex < gameStates.Length; playerIndex++) {
			GameState gameState = gameStates[playerIndex];
			initialPositions.Add(gameState.myMonster.position);
		}

		while (counter < duration) {
			yield return wait;
			counter += Time.fixedDeltaTime;
			for (int playerIndex = 0; playerIndex < gameStates.Length; playerIndex++) {
				GameState gameState = gameStates[playerIndex];
				Vector3 initialPosition = initialPositions[playerIndex];
				Vector3 screenCoords = globalCamera.ScreenToWorldPoint(new Vector3(Screen.width * playerIndex, 0, globalCamera.nearClipPlane)); // XXX: SUPER BAD HACK
				Vector3 difference = new Vector3(screenCoords.x, initialPosition.y, initialPosition.z) - initialPosition;
				gameState.myMonster.transform.position = eatingCurve.Evaluate(counter/duration) * difference + initialPosition;
			}
		}
		yield return StartCoroutine(EatTacos ());
	}

	private IEnumerator EatTacos () {
		YieldInstruction wait = new WaitForFixedUpdate();

		int maxIndex = 0;
		int currMaxScore = 0;
		bool timeBonus = false;

		float counter = 0f;
		float duration = 2f;
		List<Vector3[]> initialPositions = new List<Vector3[]>();
		for (int playerIndex = 0; playerIndex < gameStates.Length; playerIndex++) {

			GameState gameState = gameStates[playerIndex];
			if (gameState.scoreTacos.Count > currMaxScore) {
				currMaxScore = gameState.scoreTacos.Count;
				maxIndex = playerIndex;
			} else {
				if (gameState.scoreTacos.Count == currMaxScore) {
					// TIE
					if (gameState.scoreTacos.Count == 0) {
						maxIndex = Random.Range(0,1);
					} else {
						maxIndex = firstBlood;
						timeBonus = true;
					}
				}
			}

			initialPositions.Add(new Vector3[gameState.scoreTacos.Count]);
			for (int tacoIndex = 0; tacoIndex < gameState.scoreTacos.Count; tacoIndex++) {
				initialPositions[playerIndex][tacoIndex] = gameState.scoreTacos[tacoIndex].transform.position;
			}
		}

		GameState winningGameState = gameStates[maxIndex];
		Instantiate(winTextPrefab, winningGameState.tacoSpawnTransform.position, Quaternion.identity);


		while (counter < duration) {
			yield return wait;
			float checkpointFraction = 0.5f;
			for (int playerIndex = 0; playerIndex < gameStates.Length; playerIndex++) {
				GameState gameState = gameStates[playerIndex];
				counter += Time.fixedDeltaTime;
				for (int tacoIndex = 0; tacoIndex < gameState.scoreTacos.Count; tacoIndex++) {
					float fraction = counter/(duration - tacoIndex * duration/(gameState.scoreTacos.Count));
					Vector3 initialPosition;
					Vector3 checkpoint = gameState.myMonsterMouthCheckpoint.position;
					Vector3 mouth = gameState.myMonsterMouth.position;
					Vector3 difference;
					if (fraction >= checkpointFraction) {
						float consumeFraction = (fraction-checkpointFraction)/(1f-checkpointFraction);
						// if (playerIndex == 1) {
						// 	Debug.Log(consumeFraction);
						// }
						float mouthFraction = Mathf.Repeat(consumeFraction,1f);//(fraction/gameState.scoreTacos.Count)/((1f - checkpointFraction)/gameState.scoreTacos.Count);
						float rotDifference = gameState.openRotation - gameState.closedRotation;
						Quaternion targetRotation = Quaternion.Euler(0, 0, eatingCurve.Evaluate(mouthFraction) * rotDifference + gameState.closedRotation);
						gameState.myMonsterMandible.rotation = targetRotation;
						difference = (mouth - checkpoint);
						gameState.scoreTacos[tacoIndex].transform.position = eatingCurve.Evaluate(consumeFraction) * difference + checkpoint;
					} else {
						initialPosition = initialPositions[playerIndex][tacoIndex];
						difference = (checkpoint - initialPosition);
						gameState.scoreTacos[tacoIndex].transform.position = eatingCurve.Evaluate(fraction/checkpointFraction) * difference + initialPosition;
					}
				}
			}
		}

		yield return StartCoroutine(QuitOnComplete());
	}

	private IEnumerator QuitOnComplete () {
		YieldInstruction wait = new WaitForFixedUpdate();
		float counter = 0f;
		float duration = 1f;

		while (counter < duration) {
			yield return wait;
			counter += Time.fixedDeltaTime;
		}

		Application.Quit();
	}

	private bool InRange(float m, float d, float range) {
		return m >= d - range/2f && m <= d + range/2f;
	}

}