﻿using UnityEngine;
using System.Collections;

public class Rocker : MonoBehaviour {
	[SerializeField]
	private string controlAxis;

	public void SetControlAxis (string axis) {
		controlAxis = axis;
	}

	[SerializeField]
	private bool invertRotation = true;

	[SerializeField]
	private float rotationGravity = 5f;

	[SerializeField]
	private float rockingSpeed = 10f;

	[SerializeField]
	private float rotationFriction = 0.9f;

	[SerializeField]
	private float maxRotSpeed = 50f;

	[SerializeField]
	private AnimationCurve rotationGravityCurve;

	[SerializeField]
	private AnimationCurve scoreScaleCurve;

	[SerializeField]
	private AnimationCurve scorePositionCurve;

	private float currRotationSpeed = 0f;
	private float currRotation = 0f;

	private Transform shrimpTransform;

	// Cache Variables
	private Transform myTransform;

	void OnEnable () {
		myTransform = transform;
	}

	private bool measuredLeftHeight = false;
	private bool measuredRightHeight = false;


	[SerializeField]
	private float lbMaxAngle = 100f;

	private float timer = 0f;

	void OnDrawGizmos () {
		if (myTransform == null) {
			myTransform = transform;
		}
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(myTransform.position,1f);

		Gizmos.color = Color.green;
		DrawAngle(0f);

		Gizmos.color = Color.gray;
		DrawAngle(lbMaxAngle);

		Gizmos.color = Color.white;
		Gizmos.DrawLine(new Vector3(0,5,0), new Vector3(0,5,0) + Vector3.right * currRotationSpeed/5f);
	}

	void DrawAngle(float a) {
		float r = a * Mathf.Deg2Rad - Mathf.PI/2f;
		Gizmos.DrawLine(myTransform.position, myTransform.position + new Vector3(Mathf.Cos(r), Mathf.Sin(r)));
	}


	private struct StepData
	{
		public float rotationSpeed;
		public float rotation;

		public StepData(float rotationSpeed, float rotation)
		{
			this.rotationSpeed = rotationSpeed;
			this.rotation = rotation;
		}
	}

	private StepData SimulateStep (StepData initialStepData, float timeStep) {

		StepData result = new StepData(initialStepData.rotationSpeed, initialStepData.rotation);

		result.rotationSpeed *= rotationFriction;

		var delta = result.rotation + rotationGravity * timeStep;

		// Right Bottom
		if (delta >= 0f && delta < lbMaxAngle) {
			var heightFraction = (delta/lbMaxAngle);
			result.rotationSpeed -= rotationGravity * rotationGravityCurve.Evaluate(heightFraction) * timeStep;
		}

		// Top Left
		if (delta > 180f && delta < 360f - lbMaxAngle) {
			var heightFraction = (delta - 180f)/((360f - lbMaxAngle) - 180f);
			result.rotationSpeed -= rotationGravity * rotationGravityCurve.Evaluate(heightFraction) * timeStep;
		}

		delta = result.rotation - rotationGravity * timeStep;
		// Left Bottom
		if (delta > 360f - lbMaxAngle && delta < 360f) {
			var heightFraction = 1f - (delta - (360f - lbMaxAngle))/(360f - (360f - lbMaxAngle));
			result.rotationSpeed += rotationGravity * rotationGravityCurve.Evaluate(heightFraction) * timeStep;
		}

		// Top Right
		if (delta > lbMaxAngle && delta < 180f) {
			var heightFraction = 1f - (delta - lbMaxAngle)/(180f - lbMaxAngle);
			result.rotationSpeed += rotationGravity * rotationGravityCurve.Evaluate(heightFraction) * timeStep;
		}

		result.rotationSpeed = Mathf.Clamp(result.rotationSpeed, -maxRotSpeed, maxRotSpeed);

		result.rotation += result.rotationSpeed * Time.fixedDeltaTime;
		result.rotation = Mathf.Repeat(result.rotation, 360);

		return result;
	}

	private bool Predict(out float predictedPeak, float horiz) {
		predictedPeak = 0f;
		StepData simData = new StepData(currRotationSpeed, currRotation);
		StepData lastSimData = new StepData(currRotationSpeed, currRotation);

		for (float t = 0f; t < 40f; t += Time.fixedDeltaTime) {
			float speedUpDir = Mathf.Sign(simData.rotation - lastSimData.rotation);
			simData.rotationSpeed += horiz * speedUpDir * rockingSpeed * Time.fixedDeltaTime;
			simData = SimulateStep(simData, Time.fixedDeltaTime);
			if (Mathf.Sign(simData.rotationSpeed) != Mathf.Sign(lastSimData.rotationSpeed)) {
				if (simData.rotation > 180f) {
					predictedPeak = 360f-simData.rotation;
				} else {
					predictedPeak = simData.rotation;
				}
				return true;
			}
			lastSimData = new StepData(simData.rotationSpeed, simData.rotation);
		}
		return false;
	}

	private float speedAtBottom = 0f;
	private float evaluationMetric = 0f;

	public float EvaluationMetric {
		get { return evaluationMetric; }
	}

	public bool ReachedLeft {
		get { return measuredRightHeight; }
	}
	public bool ReachedRight {
		get { return measuredLeftHeight; }
	}

	public float LeftMetric = 0f;
	public float RightMetric = 0f;

	void FixedUpdate () {
		if (!GameManager.g.IsGameRunning) {
			return;
		}

		var horiz = Input.GetAxisRaw(controlAxis);
		if (invertRotation) horiz *= -1;

		currRotationSpeed += horiz * rockingSpeed * Time.fixedDeltaTime;

		StepData result = SimulateStep(new StepData(currRotationSpeed, currRotation), Time.fixedDeltaTime);
		currRotationSpeed = result.rotationSpeed;
		currRotation = result.rotation;

		float minPrediction = 0f;
		float neutPrediction = 0f;
		float maxPrediction = 0f;

		if (Predict(out minPrediction, -1f) && Predict(out neutPrediction, 0f) && Predict(out maxPrediction, 1f)) {
			float avgMetric = (minPrediction + neutPrediction + maxPrediction)/3f;
			evaluationMetric = avgMetric;
			// Debug.Log("Predicted Min: "+ minPrediction.ToString());
			// Debug.Log("Predicted Neutral: "+ neutPrediction.ToString());
			// Debug.Log("Predicted Max: "+ maxPrediction.ToString());
		}

		var delta = currRotation - rotationGravity * Time.fixedDeltaTime;

		timer += Time.fixedDeltaTime;

		if (((delta >= 0f && delta < lbMaxAngle) || (delta > 180f && delta < 360f - lbMaxAngle)) && currRotationSpeed < 0f && !measuredRightHeight) {
			// bgVisualizer.SetHeight(currRotation);

			measuredRightHeight = true;
			measuredLeftHeight = false;

			RightMetric = evaluationMetric;
		}

		if (((delta > 360f - lbMaxAngle && delta < 360f) || (delta > lbMaxAngle && delta < 180f)) && currRotationSpeed > 0f && !measuredLeftHeight) {
			// bgVisualizer.SetHeight(360f-currRotation);

			measuredLeftHeight = true;
			measuredRightHeight = false;

			LeftMetric = evaluationMetric;
		}

		if ((currRotation > 0f && currRotation + currRotationSpeed * Time.fixedDeltaTime <= 0f) || // bottom of swing from left
			(currRotation < 360f && currRotation + currRotationSpeed * Time.fixedDeltaTime >= 360f)) {// bottom of swing from right

			speedAtBottom = currRotationSpeed;

			timer = 0f;

			measuredLeftHeight = false;
			measuredRightHeight = false;
		}

		Quaternion targetRotation = Quaternion.Euler(0, 0, currRotation);
		myTransform.rotation = targetRotation;
	}

	private bool isMovingToScore = false;
	public void StartMovingToScoreAt(Vector3 position) {
		if (!isMovingToScore) {
			isMovingToScore = true;
			StartCoroutine(MoveToScoreAt(position));
		}
	}

	private IEnumerator MoveToScoreAt (Vector3 position) {
		float duration = 0.6f;
		Vector3 initialPosition = myTransform.position;
		float elapsed = 0f;
		YieldInstruction wait = new WaitForFixedUpdate();
		while ((myTransform.position - position).sqrMagnitude > Mathf.Epsilon) {
			elapsed += Time.fixedDeltaTime;
			myTransform.position = initialPosition + scorePositionCurve.Evaluate(elapsed/duration) * (position - initialPosition);
			myTransform.localScale = Vector3.one * scoreScaleCurve.Evaluate(elapsed/duration);
			yield return wait;
		}
	}
}