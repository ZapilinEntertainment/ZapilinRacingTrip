﻿using UnityEngine;
using System.Collections;

public class playerControllerGUI : MonoBehaviour {
	public GameObject cam;
	public playerController pc;
	public GameObject cloud_emitter;
	public bool finished=false;
	Texture finish_tx;
	Rect results_rect;
	public int result=0;

	// Use this for initialization
	void Start () {
		finish_tx=Resources.Load<Texture>("finish_tx");
		results_rect=new Rect(Screen.width/2-4*Global.gui_piece,Screen.height/2+2*Global.gui_piece,8*Global.gui_piece,Global.gui_piece);
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
				if (pc.zrotate_vector!=Vector3.forward) pc.CmdZRotateVector((byte)(1));
			}
			else
			{
				if (Input.GetKeyDown("e")) 
				{
					if (pc.zrotate_vector!=Vector3.back) pc.CmdZRotateVector((byte)(2));
				}
			}
		}
		else //уже вращается
		{
			if ((pc.zrotate_vector==Vector3.forward&&Input.GetKeyUp("q"))||(pc.zrotate_vector==Vector3.back&&Input.GetKeyUp("e"))) pc.CmdZRotateVector(0);
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
		GUILayout.Label(Global.playable.ToString());
		if (finished)
		{
			GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),finish_tx);
			GUI.Label(results_rect,"Ваш результат: место "+result.ToString());
			GUI.Label(new Rect(results_rect.x,results_rect.y+Global.gui_piece,results_rect.width,results_rect.height),"Ваше время: "+Global.gmaster.results[result]);
		}
	}
}
