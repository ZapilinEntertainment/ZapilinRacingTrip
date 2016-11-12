using UnityEngine;
using System.Collections;

public class hurt_trigger : MonoBehaviour {
	public float damage=10;
	public bool explosive=false;

	void OnTriggerEnter (Collider c) 
	{
		if (!Global.onServer) return;

		if (explosive) 
		{
			Collider[] cdr=Physics.OverlapSphere(transform.position,transform.localScale.x*2);
			foreach (Collider cd in cdr) 
			{
				if (cd.transform.root.GetComponent<Rigidbody>()) 
				{
					cd.transform.root.SendMessage("ApplyDamage",damage*(1-Vector3.Distance(transform.position,cd.transform.position)/transform.localScale.x),SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}
}
