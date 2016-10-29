using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class GameMaster : NetworkBehaviour {
	const int MAX_PLAYERS=16;

	public GameObject[] waypoints;
	public int player_map_position=0;
	int players_count;
	int k;
	float start_time;
	int[] players_positions;
	List<int> finished;
	public List<float> results;
	bool[] ready;
	bool race_started=false;

	Texture blackboard_tx;
	Texture ind_on;
	Texture ind_off;
	Rect board_rect;
	Rect player_info_rect;
	Rect player_indicator_rect;

	const int laps_count=3;
	int lap=0;
	// Use this for initialization
	void Awake () 
	{
		ready=new bool[MAX_PLAYERS];
		for (byte i=0;i<MAX_PLAYERS;i++) ready[i]=false;
		Global.gmaster=this;
		if (isServer) Global.onServer=true;
	}

	void Start () {
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

	public void Next() 
	{
		player_map_position++;
		if (player_map_position>=waypoints.Length) 
		{
			player_map_position=0;
			lap++;
			if (lap==laps_count) 
			{
				CmdAddToFinished(Global.local_player_number);
				Global.myPlayer.SendMessage("Finish",SendMessageOptions.DontRequireReceiver);
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
		RpcIncreasePlayersCount();
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

	[Command]
	void CmdReadySignal (int x) 
	{
		ready[x]=true;
		RpcReadySignal(x);
	}
	[ClientRpc]
	void RpcReadySignal (int x)
	{
		if (isServer) return;
		ready[x]=true;
	}


	[ClientRpc]
	void RpcIncreasePlayersCount()
	{
		if (isServer) return;
		players_count++;
	}

	[Command]
	void CmdRaceStart()
	{
		Global.playable=true;
		race_started=true;
		start_time=Time.time;
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
			if (!ready[Global.local_player_number]) 
			{
				if (GUI.Button(r1,"Ready!")) CmdReadySignal(Global.local_player_number);
			}
			else 
			{
				if (isServer) 
				{
					if (GUI.Button(r1,"StartGame!")) CmdRaceStart();
				}
			}
		}
	}
		
}
