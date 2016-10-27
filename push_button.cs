using UnityEngine;
using System.Collections;

public class push_button : MonoBehaviour {
	const float rot_speed=15;
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(Vector3.up*rot_speed*Time.deltaTime,Space.Self);
	}
}
