using UnityEngine;
using System.Collections;

public class hurtline : MonoBehaviour {

	public float length=500;
	public Transform look_object;
	public Vector3 fwd_dir=Vector3.forward;
	public float damage=10;
	RaycastHit rh;

	void Start () 
	{
		if (look_object!=null) 
		{
			fwd_dir=(look_object.position-transform.position).normalized;
			length=Vector3.Distance(transform.position,look_object.position);
		}
		LineRenderer lr=Instantiate(Resources.Load<LineRenderer>("energy_line_pref")) as LineRenderer;
		lr.SetPosition(0,transform.position);
		lr.SetPosition(1,transform.position+fwd_dir*length);
		lr.SetWidth(10,10);
	}

	void Update () 
	{
		if (!Global.playable||!Global.onServer) return;
		if (Physics.Raycast(transform.position,fwd_dir,out rh,length)) 
		{
			if (rh.collider.GetComponent<Rigidbody>()) 
			{
				rh.collider.transform.root.SendMessage("ApplyDamage",damage*Time.deltaTime,SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
