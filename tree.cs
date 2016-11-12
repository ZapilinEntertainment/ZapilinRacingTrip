using UnityEngine;
using System.Collections;

public class tree : MonoBehaviour {
	public Transform mesh;
	public GameObject sprite;
	bool optimized=false;
	public float maxsize=5;
	public float minsize=0.8f;
	bool invisible=false;
	public Quaternion rotateTo;
	// Use this for initialization
	void Start () {
		transform.parent=null;
		mesh.transform.localScale=Vector3.one*Random.Range(minsize,maxsize);;
		transform.rotation=Quaternion.Euler(0,Random.value*360,0);
		sprite=Instantiate(sprite,transform.position,transform.rotation) as GameObject;
		sprite.transform.parent=transform;
		sprite.transform.localScale=mesh.transform.localScale;
		sprite.SetActive(false);
	}

	void Update () {
		if (2>1) return;
		float d=Vector3.Distance(Global.myPlayer.transform.position,transform.position);
		if (d>4000) {
			if (d>6000) {
				if (!invisible) {invisible=true;mesh.gameObject.SetActive(false);sprite.SetActive(false);}
			}
			else {
				if (invisible) {
					invisible=false;mesh.gameObject.SetActive(false);
					sprite.SetActive(true);
					optimized=true;}
			}
			if (!optimized) {
				mesh.gameObject.SetActive(false);
				sprite.SetActive(true);
				optimized=true;
			}
		}
		else {
			if (optimized) {
				mesh.gameObject.SetActive(true);
				sprite.SetActive(false);
				optimized=false;
			}
		}
		if (rotateTo!=transform.rotation) transform.rotation=Quaternion.RotateTowards(transform.rotation,rotateTo,60*Time.deltaTime);
	}

}
