using UnityEngine;
using System.Collections;

public class playerControllerGUI : MonoBehaviour {
	public GameObject cam;
	public playerController pc;
	public GameObject cloud_emitter;
	public bool finished=false;
	Texture finish_tx,hor_line_frame,hor_blue_line;
	Rect results_rect,ready_button_rect,speedline_rect;
	public int result=0;

	// Use this for initialization
	void Start () {
		finish_tx=Resources.Load<Texture>("finish_tx");
		hor_line_frame=Resources.Load<Texture>("horizontal_bar_frame");
		hor_blue_line=Resources.Load<Texture>("horizontal_blue_line");

		int sw=Screen.width,sh=Screen.height,k=Global.gui_piece;
		results_rect=new Rect(sw/2-4*k,sh/2+2*k,8*k,k);
		ready_button_rect=new Rect(sw/2-k,sw/2-k,2*k,2*k);
		speedline_rect=new Rect(sw/2-1.5f*k,sh-k,3*k,k/2);
	}

	void Update () 
	{
		cloud_emitter.transform.position=transform.position;
		if (!Global.playable) return;
			if (Input.GetKeyDown(KeyCode.RightArrow)) pc.CmdAccelerate(false);
			else {
				if (Input.GetKeyDown(KeyCode.LeftArrow)) pc.CmdAccelerate(true);
			}

		if (pc.yrotate_vector==Vector3.zero)
		{
			if (Input.GetKeyDown("d")) 
			{
				if (pc.yrotate_vector!=Vector3.up) pc.CmdYRotateVector((byte)(1));
			}
			else
			{
				if (Input.GetKeyDown("a")) 
				{
					if (pc.yrotate_vector!=Vector3.down) pc.CmdYRotateVector((byte)(2));
				}
			}
		}
		else //уже вращается
		{
			if ((pc.yrotate_vector==Vector3.up&&Input.GetKeyUp("d"))||(pc.yrotate_vector==Vector3.down&&Input.GetKeyUp("a"))) pc.CmdYRotateVector(0);
		}
		// Y AXIS
		if (pc.xrotate_vector==Vector3.zero)
		{
			if (Input.GetKeyDown("w")) 
			{
				if (pc.xrotate_vector!=Vector3.left) pc.CmdXRotateVector((byte)(1));
			}
			else
			{
				if (Input.GetKeyDown("s")) 
				{
					if (pc.xrotate_vector!=Vector3.right) pc.CmdXRotateVector((byte)(2));
				}
			}
		}
		else //уже вращается
		{
			if ((pc.xrotate_vector==Vector3.left&&Input.GetKeyUp("w"))||(pc.xrotate_vector==Vector3.right&&Input.GetKeyUp("s"))) pc.CmdXRotateVector(0);
		}
		// Z AXIS
		if (pc.zrotate_vector==Vector3.zero)
		{
			if (Input.GetKeyDown("q")) 
			{
				if (pc.zrotate_vector!=Vector3.back) pc.CmdZRotateVector((byte)(1));
			}
			else
			{
				if (Input.GetKeyDown("e")) 
				{
					if (pc.zrotate_vector!=Vector3.forward) pc.CmdZRotateVector((byte)(2));
				}
			}
		}
		else //уже вращается
		{
			if ((pc.zrotate_vector==Vector3.back&&Input.GetKeyUp("q"))||(pc.zrotate_vector==Vector3.forward&&Input.GetKeyUp("e"))) pc.CmdZRotateVector(0);
		}
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
				Rect r=speedline_rect;
				float f=pc.speed/pc.maxspeed;
				GUI.DrawTexture(new Rect(r.x,r.y,f*r.width,r.height),hor_blue_line);
				GUI.DrawTexture(speedline_rect,hor_line_frame);
			}
		}
	}
}
