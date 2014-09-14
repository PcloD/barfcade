using UnityEngine;
using System.Collections;

using CarthageGames.Extensions.Color;

public class BackgroundVisualizer : MonoBehaviour {

	[SerializeField]
	private Camera myCamera;

	[SerializeField]
	private AnimationCurve temporalDistribution;

	[SerializeField]
	private float visualizationRange = 15f;

	[SerializeField]
	private float desiredTime = 15f;

	[SerializeField]
	private float tooHighTime = 30f;

	[SerializeField]
	private float temporalRange = 2f;

	[SerializeField]
	private float maximumHeight = 37f;

	[SerializeField]
	private Color neutralColor;
	[SerializeField]
	private Color goodColor;
	[SerializeField]
	private Color badColor;

	private float freqTimeMetric = 0f;
	private float timeMetric = 0f;
	private float heightMetric = 0f;

	private float goodness = 0f;
	private float target = 0f;

	public void SetTime (float t) {
		timeMetric = t; // TODO: FIXME
		CheckTime();
	}

	public void SetFreqTime (float t) {
		freqTimeMetric = t;
		CheckFreqTime();
	}

	public void SetHeight (float h) {
		heightMetric = h;
		CheckHeight();
	}

	[SerializeField]
	private float reward = 2f;
	[SerializeField]
	private float timePenalty = 3f;
	[SerializeField]
	private float heightPenalty = 10f;

	private void CheckHeight () {
		if (heightMetric > maximumHeight) {
			target -= heightPenalty;
			// Debug.Log("Height too high");
		}
	}

	private void CheckFreqTime () {
		// if ((freqTimeMetric > tooHighTime) &&
		// 	target - timePenalty >= 0f) {
		// 	target -= timePenalty;
		// 	// Debug.Log("Freq Time Far Too High, go neutral");
		// }
	}

	private void CheckTime () {
		if (timeMetric > 1f && timeMetric < desiredTime - temporalRange/2f) {
			// target -= timePenalty;
			//// Debug.Log("Time too low");
		}

		if (((timeMetric > desiredTime + temporalRange/2f)) ){ //&&
			// target - timePenalty >= 0f) {
			target -= timePenalty;
			// Debug.Log("Time Too High, go neutral");
		}

		if (((timeMetric < desiredTime - temporalRange/2f)) &&
			target - timePenalty >= 0f) {
			target -= timePenalty;
			// Debug.Log("Time Too Low, go neutral");
		}

		if (timeMetric >= desiredTime - temporalRange/2f &&
			timeMetric <= desiredTime + temporalRange/2f) {
			target += reward;
		}
	}

	public void RandomizeTarget() {
		float v = temporalDistribution.Evaluate(Random.value);
		desiredTime = v;
		goodness = 0f;
		target = 0f;
		// SetTarget(v, temporalRange);
		Debug.Log("Randomizing: "+v.ToString());
	}

	void Update () {
		if (Input.GetButtonDown("Fire1")) {
			RandomizeTarget();
		}
	}

	[SerializeField]
	private float tendToTargetSpeed = 0.05f;
	// [SerializeField]
	// private float tendToNeutralSpeed = 0.01f;
	[SerializeField]
	private float neutralFrictionFactor = 0.8f;
	private void FixedUpdate () {
		// // Debug.Log("GoodnessChange: "+(Mathf.Sign(target - goodness) * tendToTargetSpeed * Time.fixedDeltaTime).ToString());
		// if (Mathf.Abs(target - goodness) >= tendToTargetSpeed * Time.fixedDeltaTime) {
		// 	goodness += Mathf.Sign(target - goodness) * tendToTargetSpeed * Time.fixedDeltaTime;
		// } else {
		// 	goodness = target;
		// }
		// target *= neutralFrictionFactor;
		// // goodness += Mathf.Sign(0f - goodness) * tendToNeutralSpeed * Time.fixedDeltaTime;
		// goodness = Mathf.Clamp(goodness, -1f, 1f);

		// target = Mathf.Clamp(target, -2f, 2f);

		// if (goodness >= 0f) {
		// 	myCamera.backgroundColor = ColorExtensions.Slerp(neutralColor, goodColor, goodness);
		// } else {
		// 	myCamera.backgroundColor = ColorExtensions.Slerp(neutralColor, badColor, -goodness);
		// }

		if (freqTimeMetric > desiredTime + temporalRange/2f) {
			myCamera.backgroundColor = ColorExtensions.Slerp(goodColor, badColor, Mathf.Clamp01((freqTimeMetric - desiredTime - temporalRange/2f)/(desiredTime+visualizationRange)));
		} else {
			myCamera.backgroundColor = ColorExtensions.Slerp(neutralColor, goodColor, Mathf.Clamp01(freqTimeMetric/(desiredTime - temporalRange/2f)));
		}
	}

}
