using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class playerController : NetworkBehaviour {
	[SyncVar]
	public float speed=0;
	public float acceleration=3;
	public float angle_acceleration=6;
	public  float maxspeed=50;
	public bool accelerating=false;

	public GameObject camera_pref;
	public GameObject head;
	public GameObject fin; //хвостовой плавник
	public GameObject flipper; //боковые плавники
	public GameObject screw; //винт

	public Vector3 head_look_vector;
	public Vector3 xrotate_vector;
	public Vector3 yrotate_vector;
	[SyncVar] Vector3 realpos;
	[SyncVar] Quaternion realrot;

	AudioSource propeller_as;
	AudioSource byke_as;

	public bool multiplayer=true;
	public bool last_pedal_left=false;
	float time_to_stop=0;

	void Start () 
	{
		if (isLocalPlayer) 
		{
			GameObject c=Instantiate(camera_pref) as GameObject;
			c.transform.parent=transform;
			c.transform.localPosition=new Vector3(0,-10,0);
			c.transform.localRotation=Quaternion.Euler(Vector3.zero);
			playerControllerGUI pcg=gameObject.AddComponent<playerControllerGUI>();
			pcg.cam=c;
			head.SetActive(false);
			pcg.pc=this;
		}
	}

	void Update () 
	{
		if (time_to_stop>0)
		{
			time_to_stop-=Time.deltaTime;
			if (time_to_stop<=0) 
			{
				time_to_stop=0;
				accelerating=false;
			}
		}
		if (speed!=0) 
		{
		transform.Translate(Vector3.forward*Time.deltaTime*speed,Space.Self);
			if (yrotate_vector!=Vector3.zero) 
			{
				transform.Rotate(yrotate_vector*Time.deltaTime*angle_acceleration*speed/10,Space.Self);
			}
			if (xrotate_vector!=Vector3.zero) 
			{
				transform.Rotate(xrotate_vector*Time.deltaTime*angle_acceleration*speed/10,Space.Self);
			}
			screw.transform.Rotate(Vector3.forward*speed*Time.deltaTime*5,Space.Self);
			if (propeller_as&&!propeller_as.enabled) propeller_as.enabled=true;
		}
		else 
		{
			if (propeller_as.enabled) propeller_as.enabled=false;
		}
		fin.transform.localRotation=Quaternion.Euler(0,-15*yrotate_vector.y,0);
		flipper.transform.localRotation=Quaternion.Euler(15*xrotate_vector.x,0,0);
		if (!isServer)
		{
			float a=Vector3.Distance(transform.position,realpos);
			if (a>1) 
			{
				transform.position+=(realpos-transform.position).normalized*Time.deltaTime;
			}
			a=Quaternion.Angle(transform.rotation,realrot);
			if (a>1) 
			{
				transform.rotation=Quaternion.RotateTowards(transform.rotation,realrot,Time.deltaTime);
			}
		}
		else
		{
			realpos=transform.position;
			realrot=transform.rotation;
		}
		if (!isLocalPlayer) 
		{
			head.transform.rotation=Quaternion.Euler(head_look_vector);
		}
	}

	[Command]
	public void CmdAccelerate (bool left) 
	{
		if (accelerating) 
		{
			if (left!=last_pedal_left)
			{
				speed+=acceleration*Time.deltaTime;
				time_to_stop=(maxspeed-speed)/maxspeed*2;
			}
			else
			{
				accelerating=false;
				time_to_stop=0;
			}
			last_pedal_left=left;
		}
		else //start moving
		{
			last_pedal_left=left;
			accelerating=true;
			speed+=acceleration*Time.deltaTime;
			time_to_stop=(maxspeed-speed)/maxspeed*2;
		}
	}


	[Command]
	public void CmdHeadRotation (Vector3 rt) 
	{
		head_look_vector=rt;
		if (multiplayer) RpcHeadRotation(rt);
	}
	[ClientRpc]
	public void RpcHeadRotation (Vector3 rt)
	{
		if (isServer) return;
		head_look_vector=rt;
	}

	[Command]
	public void CmdXRotateVector (byte x)
	{
		switch (x) 
		{
		case 0: xrotate_vector=Vector3.zero;break;
		case 1: xrotate_vector=Vector3.left;break;
		case 2: xrotate_vector=Vector3.right;break;
		}
		if (multiplayer )RpcXRotateVector(x);
	}
	[ClientRpc]
	void RpcXRotateVector (byte x)
	{
		switch (x) 
		{
		case 0: xrotate_vector=Vector3.zero;break;
		case 1: xrotate_vector=Vector3.left;break;
		case 2: xrotate_vector=Vector3.right;break;
		}
	}
	[Command]
	public void CmdYRotateVector (byte x)
	{
		switch (x) 
		{
		case 0: yrotate_vector=Vector3.zero;break;
		case 1: yrotate_vector=Vector3.up;break;
		case 2: yrotate_vector=Vector3.down;break;
		}
		if (multiplayer ) RpcYRotateVector(x);
	}
	[ClientRpc]
	void RpcYRotateVector (byte x)
	{
		switch (x) 
		{
		case 0: yrotate_vector=Vector3.zero;break;
		case 1: yrotate_vector=Vector3.up;break;
		case 2: yrotate_vector=Vector3.down;break;
		}
	}

}
