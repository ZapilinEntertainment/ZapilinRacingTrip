using UnityEngine;
using System.Collections;

public class playerControllerGUI : MonoBehaviour {
	public GameObject cam;
	public playerController pc;
	public GameObject cloud_emitter;

	AudioSource ambient_as;
	float aas_volume=1;
	public Vector3 wind_vector=Vector3.back;

	public bool finished=false;
	bool collided=false;
	Texture finish_tx,hor_line_frame,hor_blue_line,red_screen;
	Rect results_rect,ready_button_rect,speedline_rect,allscreen_rect;
	public int result=0;

	// Use this for initialization
	void Start () {
		finish_tx=Resources.Load<Texture>("finish_tx");
		hor_line_frame=Resources.Load<Texture>("horizontal_bar_frame");
		hor_blue_line=Resources.Load<Texture>("horizontal_blue_line");
		red_screen=Resources.Load<Texture>("red_screen");

		int sw=Screen.width,sh=Screen.height,k=Global.gui_piece;
		results_rect=new Rect(sw/2-4*k,sh/2+2*k,8*k,k);
		ready_button_rect=new Rect(sw/2-k,sw/2-k,2*k,2*k);
		speedline_rect=new Rect(sw/2-1.5f*k,sh-k/2,3*k,k/2);
		allscreen_rect=new Rect(0,0,sw,sh);
		GameObject g=new GameObject("wind_as");
		g.transform.parent=cam.transform;
		ambient_as=g.AddComponent<AudioSource>();
		ambient_as.clip=Global.gmaster.ambient[((int)(Random.value*Global.gmaster.ambient.Length))];
		ambient_as.loop=false;
		ambient_as.spatialBlend=1f;
		ambient_as.Play();
		StartCoroutine(ChangeAmbientVolume(60*Random.value));
	}

	IEnumerator ChangeAmbientVolume(float t) 
	{
		yield return new WaitForSeconds(t);
		aas_volume=0.5f+0.5f*Random.value;
		if (!ambient_as.isPlaying) 
		{
			ambient_as.clip=Global.gmaster.ambient[((int)(Random.value*Global.gmaster.ambient.Length))];
			ambient_as.Play();
		}
		StartCoroutine(ChangeAmbientVolume(60*Random.value));
	}

	void Update () 
	{
		if (ambient_as) 
		{
			ambient_as.transform.localPosition=cam.transform.InverseTransformDirection(wind_vector*(-1));
			if (ambient_as.volume!=aas_volume) 
			{
				float av=ambient_as.volume;
				if (av<aas_volume) av+=Time.deltaTime;
				else av-=Time.deltaTime;
				if (Mathf.Abs(av-aas_volume)<=Time.deltaTime) av=aas_volume;
				if (av>1) av=1;
				if (av<0) av=0;
				ambient_as.volume=av;
			}
		}

		cloud_emitter.transform.position=transform.position;
		if (!Global.playable) return;
		if (Input.GetKeyDown(KeyCode.UpArrow)) 
		{
			pc.CmdSpeedVector(1);
		}
		else 
		{
			if (Input.GetKeyDown(KeyCode.DownArrow)) 
			{
				pc.CmdSpeedVector(-1);
			}
			else 
			{
				if (pc.speed_dir==1&&Input.GetKeyUp(KeyCode.UpArrow)||pc.speed_dir==-1&&Input.GetKeyUp(KeyCode.DownArrow)) pc.CmdSpeedVector(0);
			}
		}

		if (pc.yrot==0)
		{
			if (Input.GetKeyDown("d")) 
			{
				pc.CmdYRotateVector(1);
			}
			else
			{
				if (Input.GetKeyDown("a")) 
				{
					pc.CmdYRotateVector(-1);
				}
			}
		}
		else //уже вращается
		{
			if ((pc.yrot==1&&Input.GetKeyUp("d"))||(pc.yrot==-1&&Input.GetKeyUp("a"))) pc.CmdYRotateVector(0);
		}
		// X AXIS
		if (pc.xrot==0)
		{
			if (Input.GetKeyDown("w")) 
			{
				pc.CmdXRotateVector(-1);
			}
			else
			{
				if (Input.GetKeyDown("s")) 
				{
					pc.CmdXRotateVector(1);
				}
			}
		}
		else //уже вращается
		{
			if ((pc.xrot==-1&&Input.GetKeyUp("w"))||(pc.xrot==1&&Input.GetKeyUp("s"))) pc.CmdXRotateVector(0);
		}
		// Z AXIS
		if (pc.zrot==0)
		{
			if (Input.GetKeyDown("q")) 
			{
				pc.CmdZRotateVector(1);
			}
			else
			{
				if (Input.GetKeyDown("e")) 
				{
					pc.CmdZRotateVector(-1);
				}
			}
		}
		else //уже вращается
		{
			if ((pc.zrot==1&&Input.GetKeyUp("q"))||(pc.zrot==-1&&Input.GetKeyUp("e"))) pc.CmdZRotateVector(0);
		}

		pc.speedometer_str.transform.localRotation=Quaternion.Euler(0,0,90-pc.speed/pc.maxspeed*180);
	}



	// Update is called once per frame
	void LateUpdate () 
	{
		if (!Global.playable) return;
		float mx=Input.mousePosition.x/Screen.width;
		float my=Input.mousePosition.y/Screen.height;
		Vector3 head_rotateTo=new Vector3(-180*my+90,180*mx-90,0);
		if (head_rotateTo!=pc.head.transform.localRotation.eulerAngles) 
		{
			cam.transform.localRotation=Quaternion.Euler(head_rotateTo);
			if (pc.isClient) pc.CmdHeadRotation(cam.transform.forward);
			else pc.RpcHeadRotation(cam.transform.forward);
		}
	}

	void OnGUI () 
	{
		GUILayout.Label((pc.realpos-transform.position).ToString());
		if (finished)
		{
			GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),finish_tx);
			GUI.Label(results_rect,"Ваш результат: место "+result.ToString());
			GUI.Label(new Rect(results_rect.x,results_rect.y+Global.gui_piece,results_rect.width,results_rect.height),"Ваше время: "+Global.gmaster.results[result]);
		}
		else 
		{
			if (!Global.gmaster.race_started)
			{
				if (pc.myNumber!=-1&&!Global.gmaster.ready[Global.gmaster.local_player_number])
				if (GUI.Button(ready_button_rect,"READY!")) pc.CmdReadySignal(pc.myNumber);
			}
			else 
			{
				if (collided) GUI.DrawTexture(allscreen_rect,red_screen);
			}
		}
	}

	void OnCollisionEnter (Collision c) 
	{
		collided=true;
	}

	void OnCollisionExit(Collision c)
	{
		collided=false;
	}
}
