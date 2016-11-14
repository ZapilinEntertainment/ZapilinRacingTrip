using UnityEngine;
using System.Collections;

public class FallingZeppelin : MonoBehaviour {
	public float speed;
	public Transform screw;
	public Renderer[] rrs;
	Quaternion rotateTo;
	// Use this for initialization
	void Start () {
		rotateTo=Quaternion.LookRotation(Vector3.down,new Vector3(transform.forward.x,0,transform.forward.z));
		StartCoroutine(Timer());
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate(speed*Vector3.forward*Time.deltaTime,Space.Self);
		screw.transform.Rotate(Vector3.forward*speed*Time.deltaTime*60,Space.Self);
		transform.rotation=Quaternion.RotateTowards(transform.rotation,rotateTo,5*Time.deltaTime);
	}

	IEnumerator Timer () 
	{
		yield return new WaitForSeconds(10);
		Global.PlayerExplosionRequest(transform.position);
		yield return new WaitForSeconds(0.5f);
		Destroy(gameObject);
	}
}
