using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class playerController : NetworkBehaviour {
	[SyncVar]
	public float speed=0;
	public float acceleration=3;
	public float angle_acceleration=6;
	public  float maxspeed=50;
	public float propeller_sound_volume=0;

	public bool accelerating=false;

	public GameObject camera_pref;
	public GameObject head;
	public GameObject fin; //хвостовой плавник
	public GameObject flipper; //боковые плавники
	public GameObject screw; //винт
	public GameObject pedals;

	public Renderer[] colored_parts;
	public Collider c1;
	public Collider c2;

	public Vector3 head_look_vector;
	public Vector3 xrotate_vector;
	public Vector3 yrotate_vector;
	public Vector3 zrotate_vector;
	[SyncVar] Vector3 realpos;
	[SyncVar] Quaternion realrot;
	public int myNumber=0;

	public AudioSource propeller_as;
	public AudioSource byke_as;

	public bool multiplayer=true;
	public bool last_pedal_left=false;
	public bool stopping=false;
	float time_to_stop=0;
	float byke_sound_time=0;

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
			Global.myPlayer=c;
			pcg.cloud_emitter=Instantiate(Resources.Load<GameObject>("cloud_emitter"));
			CmdRequestAssignment();
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
		if (byke_sound_time>0) 
		{
			byke_sound_time-=Time.deltaTime;
			if (byke_sound_time<0)
			{
				byke_sound_time=0;
			}
			byke_as.volume=byke_sound_time;
		}

		if (propeller_sound_volume!=propeller_as.volume) 
		{
			if (propeller_sound_volume>propeller_as.volume) 
			{
				propeller_as.volume+=Time.deltaTime;
				if (!propeller_as.isPlaying) propeller_as.Play();
			}
			else
			{
				if (propeller_as.volume-Time.deltaTime>=0) propeller_as.volume-=Time.deltaTime;
				else {propeller_as.volume=0;propeller_as.Stop();}
			}
		}

		if (stopping) 
		{
			if (speed>0) 
			{
				speed-=acceleration*Time.deltaTime*3;
				if (speed<0) speed=0;
			}
		}
		if (speed!=0) 
		{
		transform.Translate(Vector3.forward*Time.deltaTime*speed,Space.Self);
			if (yrotate_vector!=Vector3.zero) 
			{
				transform.Rotate(yrotate_vector*Time.deltaTime*angle_acceleration,Space.Self);
			}
			if (xrotate_vector!=Vector3.zero) 
			{
				transform.Rotate(xrotate_vector*Time.deltaTime*angle_acceleration,Space.Self);
			}
			if (zrotate_vector!=Vector3.zero) 
			{
				transform.Rotate(zrotate_vector*Time.deltaTime*angle_acceleration,Space.Self);
			}
			screw.transform.Rotate(Vector3.forward*speed*Time.deltaTime*60,Space.Self);
		}
		else {
			propeller_sound_volume=0;
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


	void PaintMe (int x) 
	{
		if (colored_parts.Length==0) return;
			Color myColor;
			switch (myNumber) 
			{
			case 0:myColor=Color.red;break;
			case 1: myColor=Color.green;break;
			case 2: myColor=Color.blue;break;
			case 3: myColor=Color.cyan;break;
			case 4: myColor=Color.magenta;break;
			case 5: myColor=Color.yellow;break;
			case 6: myColor=Color.black;break;
			default: myColor=Color.gray;break;	
			}
			foreach (Renderer r in colored_parts) r.material.color=myColor;
	}

	[Command]
	void CmdRequestAssignment() 
	{
		int x=Global.gmaster.GetNumber();
		gameObject.name="player"+x.ToString();
		Global.local_player_number=x;
		transform.position=new Vector3(-200+x*100,0,-100*(x/4));
		c1.enabled=true;
		c2.enabled=true;
		RpcSetMyNumber(x);
		if (multiplayer) PaintMe(x);
	}
	[ClientRpc]
	void RpcSetMyNumber (int x) 
	{
		if (isServer) return;
		myNumber=x;
		Global.local_player_number=x;
		transform.position=new Vector3(-200+x*100,0,-100*(x/4));
		c1.enabled=true;
		c2.enabled=true;
		gameObject.name="player"+x.ToString();
		PaintMe(x);
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
				byke_sound_time=1;
				byke_as.Play();
			}
			else
			{
				accelerating=false;
				time_to_stop=0;
				propeller_sound_volume=0;
				byke_sound_time=0.3f;
				byke_as.Play();
			}
			last_pedal_left=left;
		}
		else //start moving
		{
			propeller_sound_volume=1;
			last_pedal_left=left;
			accelerating=true;
			speed+=acceleration*Time.deltaTime;
			time_to_stop=(maxspeed-speed)/maxspeed*2;
			byke_sound_time=1;
			byke_as.Play();
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
		if (isServer) return;
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
		if (isServer) return;
		switch (x) 
		{
		case 0: yrotate_vector=Vector3.zero;break;
		case 1: yrotate_vector=Vector3.up;break;
		case 2: yrotate_vector=Vector3.down;break;
		}
	}
	[Command]
	public void CmdZRotateVector (byte x)
	{
		switch (x) 
		{
		case 0: zrotate_vector=Vector3.zero;break;
		case 1: zrotate_vector=Vector3.forward;break;
		case 2: zrotate_vector=Vector3.back;break;
		}
		if (multiplayer ) RpcYRotateVector(x);
	}

	[ClientRpc]
	void RpcZRotateVector (byte x)
	{
		if (isServer) return;
		switch (x) 
		{
		case 0: zrotate_vector=Vector3.zero;break;
		case 1: zrotate_vector=Vector3.forward;break;
		case 2: zrotate_vector=Vector3.back;break;
		}
	}

	[Command]
	void CmdFinish (int x) 
	{
		stopping=true;
		RpcFinish(x);
	}
	[ClientRpc]
	void RpcFinish(int x)
	{
		if (isServer) return;
		stopping=true;
		if (isLocalPlayer) 
		{
			Global.playable=false;
			playerControllerGUI pcg=gameObject.GetComponent<playerControllerGUI>();
			if (pcg!=null) {
			pcg.finished=true;
			pcg.result=x;
			}
		}
	}

	public void Finish (int x) {
		CmdFinish(x);
	}
}
