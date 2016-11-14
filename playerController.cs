using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class playerController : NetworkBehaviour {
	[SyncVar]
	public float speed=0;
	public float acceleration=3;
	public float speed_bonus=1;
	public float bonus_time=0;
	public float angle_acceleration=6;
	public  float maxspeed=50;
	public float propeller_sound_volume=0;
	public int maxhp=100;
	public float hp;

	public GameObject camera_pref;
	public GameObject head;
	public GameObject fin; //хвостовой плавник
	public GameObject flipper; //боковые плавники
	public GameObject screw; //винт
	public GameObject pedals;
	public GameObject speedometer_str;
	public GameObject scene_center;

	public Renderer[] colored_parts;
	public Collider c1;
	public Collider c2;

	public Vector3 head_look_vector;
	public Vector3 realpos;

	Quaternion realrot;
	float last_sync_time;
	Vector3 last_forward;
	[SyncVar] public int myNumber=-1;

	public AudioSource propeller_as;
	AudioClip near_propeller_clip;
	AudioClip far_propeller_clip;
	public AudioSource byke_as;

	public bool start_with_autopilot=false;
	bool painted=false;
	bool propeller_far=false;
	float byke_sound_time=0;
	public sbyte speed_dir=0,xrot=0,yrot=0,zrot=0;

	void Start () 
	{
		far_propeller_clip=Resources.Load<AudioClip>("propeller_far_clip");
		near_propeller_clip=Resources.Load<AudioClip>("propeller_near_clip");
		if (isLocalPlayer&&!start_with_autopilot) 
		{
			GameObject c=Instantiate(camera_pref) as GameObject;
			c.transform.parent=transform;
			c.transform.localPosition=new Vector3(0,-10,0);
			c.transform.localRotation=Quaternion.Euler(Vector3.zero);
			Global.cam=c.GetComponent<Camera>();
			Global.effects_as=c.AddComponent<AudioSource>();
			playerControllerGUI pcg=gameObject.AddComponent<playerControllerGUI>();
			pcg.cam=c;
			head.SetActive(false);
			pcg.pc=this;
			Global.myPlayer=gameObject;
			c=Instantiate(Resources.Load<GameObject>("compass")) as GameObject;
			c.transform.parent=transform;
			c.transform.localRotation=Quaternion.Euler(Vector3.zero);
			c.transform.localPosition=new Vector3(0,-11,1.5f);
			Global.gmaster.compass=c;
			pcg.cloud_emitter=Instantiate(Resources.Load<GameObject>("cloud_emitter"));
			CmdRequestAssignment();
	}
		if (isServer) 
		{
			scene_center=GameObject.Find("scene_center");
			if (start_with_autopilot) 
			{
				Autopilot ap=gameObject.AddComponent<Autopilot>();
				ap.pc=this;
				AutoAssignment();
			}
		}
		hp=maxhp;
	}

	void Update () 
	{
		if (bonus_time>0) 
		{
			bonus_time-=Time.deltaTime;
			if (bonus_time<=0) 
			{
				bonus_time=0;
				speed_bonus=1;
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
		propeller_sound_volume=speed/maxspeed;
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

		if  (speed_dir!=0) speed+=speed_dir*acceleration*Time.deltaTime;
		if (speed<0) speed=0; if (speed>maxspeed) speed=maxspeed;
		if (speed!=0) 
		{
			if (yrot!=0) 
			{
				transform.Rotate(yrot*Vector3.up*Time.deltaTime*angle_acceleration,Space.Self);
			}
			if (xrot!=0) 
			{
				transform.Rotate(xrot*Vector3.right*Time.deltaTime*angle_acceleration,Space.Self);
			}
			if (zrot!=0) 
			{
				transform.Rotate(zrot*Vector3.forward*Time.deltaTime*angle_acceleration,Space.Self);
			}
			screw.transform.Rotate(Vector3.forward*speed*Time.deltaTime*60,Space.Self);
		}

		fin.transform.localRotation=Quaternion.Euler(0,-15*yrot,0);
		flipper.transform.localRotation=Quaternion.Euler(15*xrot,0,0);
		if (!isServer)
		{
			if (1>2) {
			Vector3 rpos=realpos;
			Vector3 correction_vector=last_forward;
			float y=yrot*angle_acceleration*(Time.time-last_sync_time);
			correction_vector.x=correction_vector.x*Mathf.Cos(y)-correction_vector.z*Mathf.Sin(y);
			correction_vector.z=correction_vector.x*Mathf.Sin(y)+correction_vector.z*Mathf.Cos(y);
			rpos=correction_vector.normalized*speed*(Time.time-last_sync_time);

			float a=Vector3.Distance(transform.position,realpos);
			if (a>=speed*Time.deltaTime) 
			{
				if (a>1000) transform.position=realpos+rpos;
				else	
				{
					transform.Translate((transform.forward*Time.deltaTime*speed+rpos)/2);
				}
			}
			a=Quaternion.Angle(transform.rotation,realrot);
			if (a>=angle_acceleration*Time.deltaTime) 
			{
				transform.rotation=Quaternion.RotateTowards(transform.rotation,realrot,Time.deltaTime);
			}
			}
			else transform.Translate(Vector3.forward*Time.deltaTime*speed*speed_bonus,Space.Self);
		}
		else //SERVER PART
		{
			transform.Translate(Vector3.forward*Time.deltaTime*speed*speed_bonus,Space.Self);
			if (scene_center) 
			{
				if (Vector3.Distance(transform.position,scene_center.transform.position)>Global.service_menu.playing_radius) 
				{
					transform.position=(scene_center.transform.position-transform.position)*1.99f;
					RpcTeleportation(transform.position);
				}
			}
			realpos=transform.position;
		}
		if (!isLocalPlayer) 
		{
			if (head_look_vector!=Vector3.zero) head.transform.rotation=Quaternion.LookRotation(head_look_vector);
			if (!painted) 
			{
				if (myNumber!=-1) PaintMe(myNumber);
			}
			float d=Vector3.Distance(transform.position,Global.myPlayer.transform.position);
			if (d>500) 
			{
				if (!propeller_far) 
				{
					propeller_as.clip=far_propeller_clip;
					propeller_far=true;
				}
			}
			else 
			{
				if (propeller_far)
				{
					propeller_as.clip=near_propeller_clip;
					propeller_far=false;
				}
			}
		}
	}


	Vector3 GetStartPos (int x) 
	{
		return (Global.gmaster.start_pos+new Vector3(x%5*50-250,x/5*50+50,0));
	}

	void PaintMe (int x) 
	{
		if (colored_parts.Length==0) return;
			Color myColor;
			switch (x) 
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
		painted=true;
	}

	public void StartRace() 
	{
		if (isServer) return;
		StartCoroutine(SyncCor());
	}
	IEnumerator SyncCor () 
	{
		yield return new WaitForSeconds(1);
		RpcSyncPos(transform.position,transform.rotation);
		StartCoroutine(SyncCor());
	}

	public void SetBonus (Vector3 b) 
	{
		if (!isServer) return;
		speed_bonus=b.x;
		bonus_time=b.y;
		RpcSetBonus(b.x,b.y);
	}
	[ClientRpc]
	void RpcSetBonus (float b, float t) 
	{
		if (isServer) return;
		speed_bonus=b;
		bonus_time=t;
	}

	[ClientRpc]
	void RpcSyncPos (Vector3 pos,Quaternion rot) 
	{
		if (isServer) return;
		realpos=pos;
		realrot=rot;
		last_sync_time=Time.time;
		last_forward=transform.forward;
	}
	[ClientRpc]
	void RpcTeleportation (Vector3 pos) {
		if (isServer) return;
		transform.position=pos;
		realpos=pos;
		StopCoroutine(SyncCor());
		StartCoroutine(SyncCor());
	}


	void AutoAssignment () 
	{
		if (!isServer) return;
		myNumber=Global.gmaster.GetNumber();
		gameObject.name="player"+myNumber.ToString();
		transform.position=GetStartPos(myNumber);
		realpos=transform.position;
		StartCoroutine(SyncCor());
		c1.enabled=true;
		c2.enabled=true;
		Global.gmaster.players[myNumber]=gameObject;
		RpcSetMyNumber(myNumber);
		if (Global.multiplayer) PaintMe(myNumber);
	}

	public void ApplyDamage(float h) 
	{
		if (!isServer) return;
		hp-=h;
		if (hp<=0) 
		{
			RpcDestruction();
		}
	}

	[ClientRpc]
	void RpcDestruction() 
	{
		c1.enabled=false;
		c2.enabled=false;
		speed_dir=0;
		xrot=0;
		yrot=0;
		zrot=0;
		speed_bonus=1;
		if (Global.gmaster.isLocalPlayer) Global.playable=false;
		for (int i=0;i<transform.childCount;i++)
		{
			transform.GetChild(i).gameObject.SetActive(false);
		}
		Global.cam.gameObject.SetActive(true);
		GameObject g = Instantiate(Resources.Load<GameObject>("falling_zeppelin"),transform.position,transform.rotation) as GameObject;
		FallingZeppelin fz=g.GetComponent<FallingZeppelin>();
		fz.speed=speed;
		speed=0;
		foreach (Renderer r in fz.rrs) 
		{
			r.material=colored_parts[0].material;
		}
	}

	[Command]
	void CmdRequestAssignment() 
	{
		myNumber=Global.gmaster.GetNumber();
		gameObject.name="player"+myNumber.ToString();
		if (isLocalPlayer) Global.gmaster.local_player_number=myNumber;
		transform.position=GetStartPos(myNumber);
		realpos=transform.position;
		StartCoroutine(SyncCor());
		c1.enabled=true;
		c2.enabled=true;
		Global.gmaster.local_player_number=myNumber;
		Global.gmaster.players[myNumber]=gameObject;
		RpcSetMyNumber(myNumber);
		if (Global.multiplayer) PaintMe(myNumber);
	}
	[ClientRpc]
	void RpcSetMyNumber (int x) 
	{
		if (isServer) return;
		myNumber=x;
		Global.gmaster.players[myNumber]=gameObject;
		if (isLocalPlayer) Global.gmaster.local_player_number=x;
		transform.position=Global.gmaster.start_pos+new Vector3(-200+x*100,0,-100*(x/4));
		c1.enabled=true;
		c2.enabled=true;
		gameObject.name="player"+x.ToString();
		PaintMe(x);
	}

	[Command]
	public void CmdSpeedVector (sbyte x)
	{
		if (speed_dir!=x) 
		{
			speed_dir=x;
			RpcSpeedVector(x);
		}
	}
	[ClientRpc]
	void RpcSpeedVector (sbyte x) 
	{
		speed_dir=x;
	}

	[ClientRpc]
	void RpcPropellerSound(bool x) 
	{
		if (x) propeller_sound_volume=1; else propeller_sound_volume=0;
	}


	[Command]
	public void CmdHeadRotation (Vector3 rt) 
	{
		head_look_vector=rt;
		if (Global.multiplayer) RpcHeadRotation(rt);
	}
	[ClientRpc]
	public void RpcHeadRotation (Vector3 rt)
	{
		if (isServer) return;
		head_look_vector=rt;
	}

	[Command]
	public void CmdXRotateVector (sbyte x)
	{
		if (x!=xrot)
		{
			xrot=x;
			if (Global.multiplayer )RpcXRotateVector(x);
		}
	}
	[ClientRpc]
	void RpcXRotateVector (sbyte x)
	{
		if (isServer) return;
		xrot=x;
	}
	[Command]
	public void CmdYRotateVector (sbyte y)
	{
		if (y!=yrot)
		{
			yrot=y;
			if (Global.multiplayer ) RpcYRotateVector(y);
		}
	}
	[ClientRpc]
	void RpcYRotateVector (sbyte y)
	{
		if (isServer) return;
		yrot=y;
	}
	[Command]
	public void CmdZRotateVector (sbyte z)
	{
		if (z!=zrot)
		{
			zrot=z;
			if (Global.multiplayer ) RpcYRotateVector(z);
		}
	}

	[ClientRpc]
	void RpcZRotateVector (sbyte z)
	{
		if (isServer) return;
		zrot=z;
	}

	[Command]
	void CmdFinish (int x) 
	{
		speed_dir=-1;
		RpcFinish(x);
	}
	[ClientRpc]
	void RpcFinish(int x)
	{
		if (isServer) return;
		speed_dir=-1;
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

	[Command]
	public void CmdReadySignal (int x) 
	{
		Global.gmaster.MakeReady(x);
	}

	public void Finish (int x) {
		CmdFinish(x);
	}
}
