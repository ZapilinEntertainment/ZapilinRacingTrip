using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameMaster : NetworkBehaviour {
	const int MAX_PLAYERS=16;

	public GameObject[] waypoints;
	public GameObject compass;
	public Vector3 start_pos=new Vector3(0,0,-1500);
	public int player_map_position=0;
	public int local_player_number=-1;
	int players_count;
	int k;
	float start_time;
	int[] players_positions;
	List<int> finished;
	public List<float> results;
	public bool[] ready;
	public bool race_started=false;
	bool this_player_finished=false;

	Texture blackboard_tx;
	Texture ind_on;
	Texture ind_off;
	Rect board_rect;
	Rect player_info_rect;
	Rect player_indicator_rect;
	public AudioClip button_sound;
	public AudioClip[] ambient;
	public NetworkManager nm;

	const int laps_count=3;
	int lap=0;
	// Use this for initialization
	void Awake () 
	{
		ready=new bool[MAX_PLAYERS];
		for (byte i=0;i<MAX_PLAYERS;i++) ready[i]=false;
		Global.gmaster=this;
		if (isServer) Global.onServer=true;
		nm=GameObject.Find("networkManager").GetComponent<NetworkManager>();
	}

	void Start () {
		button_sound=Resources.Load<AudioClip>("button_sound");
		blackboard_tx=Resources.Load<Texture>("blackboard_tx");
		ind_on=Resources.Load<Texture>("indicator_on");
		ind_off=Resources.Load<Texture>("indicator_off");

		k=Global.gui_piece;
		board_rect=new Rect(Screen.width-3*k,k,3*k,k/2);
		player_info_rect=new Rect(Screen.width-3*k,k,2*k,k/2);
		player_indicator_rect=new Rect(Screen.width-k/2,k,k/2,k/2);

		for (int i=0;i<waypoints.Length;i++)
		{
			if (i!=player_map_position)	waypoints[i].SetActive(false); else waypoints[i].SetActive(true);
		}
	}

	void Update () 
	{
		if (!Global.playable) return;
		if (compass&&!this_player_finished&&waypoints.Length>0) 
		{
			compass.transform.LookAt(waypoints[player_map_position].transform.position);
		}
	}

	public void MakeDisconnect() 
	{
		nm=GameObject.Find("networkManager").GetComponent<NetworkManager>();
		if (nm==null) Application.Quit();
		else
		{
			if (isServer) nm.StopHost();
			else nm.StopClient();
			StopAllCoroutines();
			SceneManager.LoadScene(0);
			Destroy(this);
		}
	}

	public void MakeReady(int x) 
	{
		ready[x]=true;
		string s="";
		foreach (bool b in ready) 
		{
			if (b) s+='1'; else s+='0';
		}
		RpcReadySignal(s);
	}

	public void Next() 
	{
		player_map_position++;
		if (player_map_position>=waypoints.Length) 
		{
			player_map_position=0;
			lap++;
			if (lap==laps_count) 
			{
				CmdAddToFinished(Global.gmaster.local_player_number);
				Global.myPlayer.SendMessage("Finish",SendMessageOptions.DontRequireReceiver);
				this_player_finished=true;
				foreach (GameObject g in waypoints) g.SetActive(false);
				return;
			}
		}
		for (int i=0;i<waypoints.Length;i++)
		{
			if (i!=player_map_position)	waypoints[i].SetActive(false); else waypoints[i].SetActive(true);
		}
	}
		
	public int GetNumber() 
	{
		if (!isServer) return (-1);
		players_count++;
		if (players_count>1) Global.multiplayer=true;
		RpcSetPlayersCount(players_count);
		return (players_count-1);
	}

	[Command]
	void CmdAddToFinished (int x)
	{
		finished.Add(x);
		float t=Time.time-start_time;
		results.Add(t);
		RpcAddToFinished(x,t);
	}
	[ClientRpc]
	void RpcAddToFinished( int x,float t)
	{
		if (isServer) return;
		finished.Add(x);
		results.Add(t);
	}
		
	[ClientRpc]
	void RpcReadySignal (string status)
	{
		if (isServer) return;
		for (int i=0;i<status.Length;i++)
		{
			if (status[i]=='1') ready[i]=true; else ready[i]=false;
		}
	}


	[ClientRpc]
	void RpcSetPlayersCount( int x)
	{
		if (isServer) return;
		players_count = x;
		if (players_count>1) Global.multiplayer=true;
	}
		
	void RaceStart()
	{
		if (!isServer) return;
		Global.playable=true;
		race_started=true;
		start_time=Time.time;
		GameObject[] players=GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject g in players) 
		{
			g.SendMessage("StartRace",SendMessageOptions.DontRequireReceiver);
		}
		RpcRaceStart();
	}
	[ClientRpc]
	void RpcRaceStart ()
	{
		if (isServer) return;
		start_time=Time.time;
		Global.playable=true;
		race_started=true;
	}
		

	void OnGUI () 
	{
		GUILayout.Label(local_player_number.ToString());
		k=Global.gui_piece;
		if (!race_started) 
		{
			board_rect.height=players_count*k/2+k;
			GUI.DrawTexture(board_rect,blackboard_tx);
			Rect r1=player_info_rect;
			Rect r2=player_indicator_rect;
			for( int i=0; i<players_count;i++)
			{
				GUI.Label(r1,"Player"+i.ToString()); r1.y+=k;
				if (ready[i]) GUI.DrawTexture(r2,ind_on);
				else GUI.DrawTexture(r2,ind_off);
				r2.y+=k;
			}
				if (isServer) 
				{
					if (ready[local_player_number]) {if (GUI.Button(r1,"StartGame!")) RaceStart();}
					//if (GUI.Button(new Rect(0,2*k,2*k,k),"AddBot"))
					//{
					//	Network.Instantiate(nm.playerPrefab,Vector3.zero,Quaternion.identity,0);
					//}
				}
				
		}

	}
		
}
