using UnityEngine;
using System.Collections;

public class KeepInCameraMiddle : MonoBehaviour {

	[SerializeField]
	private Camera mainCamera;

	[SerializeField]
	private Camera myCamera;


	void Update () {
		Vector3 center = mainCamera.ScreenToWorldPoint( new Vector3(Screen.width/4 + Screen.width * myCamera.rect.x, Screen.height/2, myCamera.nearClipPlane) );
		transform.position = new Vector3(center.x,transform.position.y, transform.position.z);
	}
}
