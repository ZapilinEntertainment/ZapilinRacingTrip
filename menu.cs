using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


public class menu : MonoBehaviour {
	public NetworkManager nm;
	public string ip_string="127.0.0.1";
	string port_s="4444";
	public int port=4444;
	Rect ip_textfield_rect,port_textfield_rect,host_button_rect,connect_button_rect;

	void Start () 
	{
		Global.gui_piece=Screen.height/12;
		int sw=Screen.width,sh=Screen.height,k=Global.gui_piece;
		ip_textfield_rect=new Rect(sw/2-2*k,sh/2-k,4*k,k);
		port_textfield_rect=new Rect(sw/2-2*k,sh/2,4*k,k);
		host_button_rect=new Rect(sw/2-2*k,sh/2+k,2*k,k);
		connect_button_rect=new Rect(sw/2,sh/2+k,2*k,k);
		if (nm==null) nm=GameObject.Find("networkManager").GetComponent<NetworkManager>();
	}

	void OnGUI () 
	{
		ip_string=GUI.TextField(ip_textfield_rect,ip_string);
		port_s=GUI.TextField(port_textfield_rect,port_s);
		if (GUI.Button(host_button_rect,"Host")) 
		{
			if (nm==null) nm=GameObject.Find("networkManager").GetComponent<NetworkManager>();
			nm.onlineScene="scene0";
			nm.networkAddress=ip_string;
			int a=4444;
			if (int.TryParse(port_s,out a)) 
			{
				nm.networkPort=a;
				nm.StartHost();	
			}
		}
		if (GUI.Button(connect_button_rect,"Connect"))
		{
			if (nm==null) nm=GameObject.Find("networkManager").GetComponent<NetworkManager>();
			nm.onlineScene="scene0";
			nm.networkAddress=ip_string;
			int a=4444;
			if (int.TryParse(port_s,out a)) 
			{
				nm.networkPort=a;
				nm.StartClient();	
			}
		}
	}
}
