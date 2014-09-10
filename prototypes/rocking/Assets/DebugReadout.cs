using UnityEngine;
using System.Collections;

public class DebugReadout : MonoBehaviour {

	[SerializeField]
	private bool orientVertically = false;

	[SerializeField]
	private float targetValue = 35f;
	[SerializeField]
	private float targetRange = 20f;

	[SerializeField]
	private Transform indicator;

	[SerializeField]
	private Transform target;

	[SerializeField]
	private Transform leftBar;

	[SerializeField]
	private Transform rightBar;

	[SerializeField]
	private TextMesh textMesh;

	[SerializeField]
	private float highestValue = 100f;
	public float HighestValue {
		set { highestValue = value; }
	}

	private SpriteRenderer mySpriteRenderer;

	void OnEnable () {
		CacheVariables ();
		SetTarget(targetValue, targetRange);
	}

	void OnValidate () {
		CacheVariables ();
		SetTarget(targetValue, targetRange);
	}

	public float Value {
		set {
			indicator.position = GetPosOnBarFor(value);
			textMesh.text = (((int)(value * 10f))/10f).ToString();
		}
	}

	[SerializeField]
	private bool canBeRandomized = false;

	[SerializeField]
	private AnimationCurve randomDistribution;
	public void RandomizeTarget() {
		float v = randomDistribution.Evaluate(Random.value);
		SetTarget(v, 2f);
		Debug.Log("Randomizing: "+v.ToString());
	}

	void Update () {
		if (canBeRandomized) {
			if (Input.GetButtonDown("Fire1")) {
				RandomizeTarget();
			}
		}
	}

	public void SetTarget (float goal, float range) {
		target.position = GetPosOnBarFor(goal);
		rightBar.position = GetPosOnBarFor(goal+range/2f);
		leftBar.position = GetPosOnBarFor(goal-range/2f);
	}

	private Vector3 GetPosOnBarFor (float value) {
		var min = mySpriteRenderer.bounds.min;
        var max = mySpriteRenderer.bounds.max;
        Vector3 pos = target.position;
        if (orientVertically) {
			pos.y = (min.y + (max.y - min.y) * value/highestValue);
    	} else {
			pos.x = (min.x + (max.x - min.x) * value/highestValue);
    	}
		return pos;
	}

	private void CacheVariables () {
		mySpriteRenderer = GetComponent<SpriteRenderer>();
	}
}
