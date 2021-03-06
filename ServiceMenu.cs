﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class ServiceMenu : MonoBehaviour {
	Rect exit_button_rect;

	public int playing_radius=10000;
	// Use this for initialization
	void Start () {
		exit_button_rect=new Rect(Screen.width-2*Global.gui_piece,0,2*Global.gui_piece,Global.gui_piece);
		Global.red_material=Resources.Load<Material>("barrier");
		Global.service_menu=this;
	}
	
	void OnGUI () 
	{
		if (GUI.Button(exit_button_rect,"Exit")) 
		{
			Global.playable=false;
			Global.gmaster.MakeDisconnect();
		}
	}
}
