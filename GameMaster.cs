using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameMaster : NetworkBehaviour {
	const int MAX_PLAYERS=15;
	const int LAPS_COUNT=3;

	public GameObject[] players;
	public GameObject[] waypoints;
	public GameObject compass;
	public Vector3 start_pos=new Vector3(0,0,-1500);
	public int player_map_position=0;
	public int local_player_number=-1;
	int players_count;
	int i,k;

	public float respawn_time=3;
	float start_time;

	public int[] players_positions;
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

	int lap=0;
	// Use this for initialization
	void Awake () 
	{
		ready=new bool[MAX_PLAYERS];
		players=new GameObject[MAX_PLAYERS];
		players_positions=new int[MAX_PLAYERS];
		for (byte i=0;i<MAX_PLAYERS;i++) ready[i]=false;

		Global.gmaster=this;
		Global.star_material=Resources.Load<Material>("star");
		Global.grey_material=Resources.Load<Material>("grey");
		Global.player_explosion=Instantiate(Resources.Load<ParticleSystem>("player_explosion")) as ParticleSystem;

		nm=GameObject.Find("networkManager").GetComponent<NetworkManager>();
	}

	void Start () {
		button_sound=Resources.Load<AudioClip>("button_sound");
		blackboard_tx=Resources.Load<Texture>("blackboard_tx");
		ind_on=Resources.Load<Texture>("indicator_on");
		ind_off=Resources.Load<Texture>("indicator_off");

		k=Screen.height/16;
		board_rect=new Rect(Screen.width-3*k,k,3*k,k/2);
		player_info_rect=new Rect(Screen.width-3*k,k,2*k,k/2);
		player_indicator_rect=new Rect(Screen.width-k/2,k,k/2,k/2);

		int i=0;
		for ( i=0;i<waypoints.Length;i++)
		{
			if (i!=player_map_position)	waypoints[i].SetActive(false); else waypoints[i].SetActive(true);
		}
		i=player_map_position+1;
		if (i>=waypoints.Length) i=0;
		for (int j=0;j<waypoints[i].transform.childCount;j++)
		{
			waypoints[i].SetActive(true);
			waypoints[i].transform.GetChild(j).SendMessage("MarkYourself",true,SendMessageOptions.DontRequireReceiver);
		}
		if (isServer) Global.onServer=true;
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
			else 
			{
				if (local_player_number!=-1)	 CmdMakeDisconnect(local_player_number);
				nm.StopClient();
			}
			StopAllCoroutines();
			SceneManager.LoadScene(0);
			Destroy(this);
		}
	}

	[Command] 
	void CmdMakeDisconnect (int i) 
	{
		if (players[i]!=null) 
		{
			NetworkServer.Destroy(players[i]);
			players_count--;
			RpcSetPlayersCount(players_count);
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
			if (lap==LAPS_COUNT) 
			{
				CmdAddToFinished(Global.gmaster.local_player_number);
				Global.myPlayer.SendMessage("Finish",SendMessageOptions.DontRequireReceiver);
				this_player_finished=true;
				foreach (GameObject g in waypoints) g.SetActive(false);
				return;
			}
		}
		players_positions[local_player_number]=player_map_position;

		int prev_pos=player_map_position-1; if (prev_pos<0) prev_pos=waypoints.Length-1; 
		int next_pos=player_map_position+1; if (next_pos>=waypoints.Length&&lap<LAPS_COUNT) {next_pos=0;}
		waypoints[prev_pos].SetActive(false);
		waypoints[player_map_position].BroadcastMessage("MarkYourself",false,SendMessageOptions.DontRequireReceiver);
		waypoints[next_pos].SetActive(true);
		waypoints[next_pos].BroadcastMessage("MarkYourself",true,SendMessageOptions.DontRequireReceiver);
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
		Rect r1=player_info_rect;
		r1.y+=k/2;
		if (!race_started) 
		{
			board_rect.height=(players_count+1)*r1.height;
			GUI.DrawTexture(board_rect,blackboard_tx,ScaleMode.StretchToFill);
			Rect r2=player_indicator_rect;
			for(i=0; i<players_count;i++)
			{
				GUI.Label(r1,"Player"+i.ToString()); 
				if (ready[i]) GUI.DrawTexture(r2,ind_on);
				else GUI.DrawTexture(r2,ind_off);
				r1.y+=r1.height;
				r2.y=r1.y;
			}
				if (isServer) 
				{
					if (ready[local_player_number]) {if (GUI.Button(r1,"StartGame!")) RaceStart();}
				if (players_count<MAX_PLAYERS)
				{
					if (GUI.Button(new Rect(0,2*k,2*k,k),"AddBot"))
						{
						GameObject g=Instantiate(nm.playerPrefab) as GameObject;
						g.GetComponent<playerController>().start_with_autopilot=true;
						NetworkServer.Spawn(g);
						}
				}		
			}
		}
		else
		{
			r1.width=3*k;
			for (i=0;i<players_count;i++)
			{
				if (players[i]==null) continue;
					GUI.Label(r1,"player "+i.ToString()+": "+players_positions[i].ToString()+"/"+waypoints.Length.ToString());
				r1.y+=r1.height;
			}
		}
	}
		
}
