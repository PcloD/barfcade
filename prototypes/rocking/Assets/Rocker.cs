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

	// Cache Variables
	private Transform myTransform;

	void OnEnable () {
		myTransform = transform;
		speedReadout.HighestValue = maxRotSpeed;
	}

	void OnValidate () {
		if (speedReadout != null) speedReadout.HighestValue = maxRotSpeed;
	}

	private bool measuredLeftHeight = false;
	private bool measuredRightHeight = false;
	void FixedUpdate () {
		var horiz = Input.GetAxisRaw("Horizontal");
		if (invertRotation) horiz *= -1;
		currRotationSpeed += horiz * rockingSpeed * Time.fixedDeltaTime;

		currRotationSpeed *= rotationFriction;

		var delta = currRotation + rotationGravity * Time.fixedDeltaTime;

		if (delta >= 0f && delta < 90f) {
			var heightFraction = (delta/90f);
			currRotationSpeed -= rotationGravity * rotationGravityCurve.Evaluate(heightFraction) * Time.fixedDeltaTime;
		}

		if (delta > 180f && delta < 270f) {
			var heightFraction = (delta - 180f)/(270f - 180f);
			currRotationSpeed -= rotationGravity * rotationGravityCurve.Evaluate(heightFraction) * Time.fixedDeltaTime;
		}


		delta = currRotation - rotationGravity * Time.fixedDeltaTime;
		if (delta > 270f && delta < 360f) {
			var heightFraction = 1f - (delta - 270f)/(360f - 270f);
			currRotationSpeed += rotationGravity * rotationGravityCurve.Evaluate(heightFraction) * Time.fixedDeltaTime;
		}
		if (delta > 90f && delta < 180f) {
			var heightFraction = 1f - (delta - 90f)/(180f - 90f);
			currRotationSpeed += rotationGravity * rotationGravityCurve.Evaluate(heightFraction) * Time.fixedDeltaTime;
		}

		currRotationSpeed = Mathf.Clamp(currRotationSpeed, -maxRotSpeed, maxRotSpeed);

		if (((delta >= 0f && delta < 90f) || (delta > 180f && delta < 270f)) && currRotationSpeed < 0f && !measuredRightHeight) {
			heightReadout.Value = currRotation;
			measuredRightHeight = true;
			measuredLeftHeight = false;
		}

		if (((delta > 270f && delta < 360f) || (delta > 90f && delta < 180f)) && currRotationSpeed > 0f && !measuredLeftHeight) {
			heightReadout.Value = (360f-currRotation);
			measuredLeftHeight = true;
			measuredRightHeight = false;
		}

		if ((currRotation > 0f && currRotation + currRotationSpeed * Time.fixedDeltaTime <= 0f) || // bottom of swing from left
			(currRotation < 360f && currRotation + currRotationSpeed * Time.fixedDeltaTime >= 360f)) {// bottom of swing from right
			// Highest Speed happens at pendulum bottom
			speedReadout.Value = Mathf.Abs(currRotationSpeed);

			measuredLeftHeight = false;
			measuredRightHeight = false;
		}

		currRotation += currRotationSpeed * Time.fixedDeltaTime;
		currRotation = Mathf.Repeat(currRotation, 360);

		Quaternion targetRotation = Quaternion.Euler(0, 0, currRotation);
		myTransform.rotation = targetRotation;
	}
}