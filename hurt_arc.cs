using UnityEngine;
using System.Collections;

public class hurt_arc : MonoBehaviour {
	public Transform[] corners;
	public float timer=5;
	public float damage=10;
	LineRenderer[] lr;
	// Use this for initialization
	void Start () {
		lr=new LineRenderer[corners.Length/2];
		LineRenderer lrp=Instantiate(Resources.Load<LineRenderer>("energy_line_pref")) as LineRenderer;
		lrp.SetWidth(15,15);
		for (int i=0;i<corners.Length/2;i++) 
		{
			lr[i]=Instantiate(lrp) as LineRenderer;
			lr[i].SetPosition(0,corners[i*2].position);
			lr[i].SetPosition(1,corners[i*2+1].position);
			lr[i].transform.parent=transform;
			lr[i].gameObject.SetActive(false);
		}
		Destroy(lrp);

	}


	
	// Update is called once per frame
	void Update () {
	
	}
}
