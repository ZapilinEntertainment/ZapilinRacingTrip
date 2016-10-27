using UnityEngine;
using System.Collections;

public class playerControllerGUI : MonoBehaviour {
	public GameObject cam;
	public playerController pc;

	// Use this for initialization
	void Start () {
	
	}

	void Update () 
	{
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

		if (pc.xrotate_vector==Vector3.zero)
		{
			if (Input.GetKeyDown("w")) 
			{
				if (pc.xrotate_vector!=Vector3.right) pc.CmdXRotateVector((byte)(1));
			}
			else
			{
				if (Input.GetKeyDown("s")) 
				{
					if (pc.xrotate_vector!=Vector3.left) pc.CmdXRotateVector((byte)(2));
				}
			}
		}
		else //уже вращается
		{
			if ((pc.xrotate_vector==Vector3.right&&Input.GetKeyUp("w"))||(pc.xrotate_vector==Vector3.left&&Input.GetKeyUp("s"))) pc.CmdXRotateVector(0);
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
		if (!Global.playable) return;
		if (pc.accelerating) {
			GUI.Label(new Rect(Screen.width/2-32,Screen.height/2-32,160,32),"last pedal was left:"+pc.last_pedal_left.ToString());
		}
	}
}
