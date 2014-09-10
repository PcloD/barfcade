using UnityEngine;
using System.Collections;

public class Rocker : MonoBehaviour {
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
	private DebugReadout speedReadout;

	[SerializeField]
	private DebugReadout heightReadout;

	private float currRotationSpeed = 0f;
	private float currRotation = 0f;

	[SerializeField]
	private BackgroundVisualizer bgVisualizer;

	// Cache Variables
	private Transform myTransform;

	void OnEnable () {
		myTransform = transform;
		// speedReadout.HighestValue = maxRotSpeed;
	}

	void OnValidate () {
		// if (speedReadout != null) speedReadout.HighestValue = maxRotSpeed;
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


	private float speedAtBottom = 0f;
	void FixedUpdate () {
		var horiz = Input.GetAxisRaw("Horizontal");
		if (invertRotation) horiz *= -1;
		currRotationSpeed += horiz * rockingSpeed * Time.fixedDeltaTime;

		currRotationSpeed *= rotationFriction;

		var delta = currRotation + rotationGravity * Time.fixedDeltaTime;

		// Right Bottom
		if (delta >= 0f && delta < lbMaxAngle) {
			var heightFraction = (delta/lbMaxAngle);
			currRotationSpeed -= rotationGravity * rotationGravityCurve.Evaluate(heightFraction) * Time.fixedDeltaTime;
		}

		// Top Left
		if (delta > 180f && delta < 360f - lbMaxAngle) {
			var heightFraction = (delta - 180f)/((360f - lbMaxAngle) - 180f);
			currRotationSpeed -= rotationGravity * rotationGravityCurve.Evaluate(heightFraction) * Time.fixedDeltaTime;
		}


		delta = currRotation - rotationGravity * Time.fixedDeltaTime;
		// Left Bottom
		if (delta > 360f - lbMaxAngle && delta < 360f) {
			var heightFraction = 1f - (delta - (360f - lbMaxAngle))/(360f - (360f - lbMaxAngle));
			currRotationSpeed += rotationGravity * rotationGravityCurve.Evaluate(heightFraction) * Time.fixedDeltaTime;
		}

		// Top Right
		if (delta > lbMaxAngle && delta < 180f) {
			var heightFraction = 1f - (delta - lbMaxAngle)/(180f - lbMaxAngle);
			currRotationSpeed += rotationGravity * rotationGravityCurve.Evaluate(heightFraction) * Time.fixedDeltaTime;
		}

		currRotationSpeed = Mathf.Clamp(currRotationSpeed, -maxRotSpeed, maxRotSpeed);

		timer += Time.fixedDeltaTime;

		if (((delta >= 0f && delta < lbMaxAngle) || (delta > 180f && delta < 360f - lbMaxAngle)) && currRotationSpeed < 0f && !measuredRightHeight) {
			bgVisualizer.SetHeight(currRotation);
			// heightReadout.Value = currRotation;
			measuredRightHeight = true;
			measuredLeftHeight = false;

			// speedReadout.Value = Mathf.Abs(timer*10f);
			// bgVisualizer.SetTime(Mathf.Abs(timer*10f));
			// timer = 0f;
		}

		if (((delta > 360f - lbMaxAngle && delta < 360f) || (delta > lbMaxAngle && delta < 180f)) && currRotationSpeed > 0f && !measuredLeftHeight) {
			bgVisualizer.SetHeight(360f-currRotation);
			// heightReadout.Value = (360f-currRotation);
			measuredLeftHeight = true;
			measuredRightHeight = false;

			// speedReadout.Value = Mathf.Abs(timer*10f);
			// bgVisualizer.SetTime(Mathf.Abs(timer*10f));

			// timer = 0f;
		}

		if ((currRotation > 0f && currRotation + currRotationSpeed * Time.fixedDeltaTime <= 0f) || // bottom of swing from left
			(currRotation < 360f && currRotation + currRotationSpeed * Time.fixedDeltaTime >= 360f)) {// bottom of swing from right
			// Highest Speed happens at pendulum bottom
			// speedReadout.Value = Mathf.Abs(currRotationSpeed);

			speedAtBottom = currRotationSpeed;

			bgVisualizer.SetTime(Mathf.Abs(timer*10f));
			timer = 0f;

			measuredLeftHeight = false;
			measuredRightHeight = false;
		}


		// Attempting to predict speed at angle:
		

		bgVisualizer.SetFreqTime(Mathf.Abs(timer*10f));

		currRotation += currRotationSpeed * Time.fixedDeltaTime;
		currRotation = Mathf.Repeat(currRotation, 360);

		Quaternion targetRotation = Quaternion.Euler(0, 0, currRotation);
		myTransform.rotation = targetRotation;
	}
}