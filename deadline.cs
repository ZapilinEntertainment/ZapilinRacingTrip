using UnityEngine;
using System.Collections;

public class deadline : MonoBehaviour {
	public int length=0;
	public float speed=30;
	float cpos=0;
	float da=0;
	bool forward=true;
	Vector3 startpos;

	// Update is called once per frame
	void Update () {
		if (length==0) return;
		da=speed*Time.deltaTime;
		if (forward) 
		{
			if (cpos+da>=length) {da=length-cpos;forward=false;}
			transform.Translate(Vector3.forward*da,Space.Self);
			cpos+=da;
		}
		else
		{
			if (cpos-da<=0) {da=cpos;forward=true;}
			transform.Translate(Vector3.back*da,Space.Self);
			cpos-=da;
		}
	}

	void OnTriggerEnter (Collider c) 
	{
		if (!Global.onServer) return;
		c.transform.root.SendMessage("ApplyDamage",Global.deadline_damage,SendMessageOptions.DontRequireReceiver);
	} 
}
