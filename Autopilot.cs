using UnityEngine;
using System.Collections;

public class Autopilot : MonoBehaviour {
	int my_point=0;
	Vector3 nearest_point;
	Vector3 rotateTo;
	float d,angular_speed;

	const float CAPTURE_DISTANCE=5;
	const int BRAKING_DISTANCE=500;
	const int CAPTURING_SPEED=20;

	public playerController pc;

	void Start () 
	{
		nearest_point=GetNearestPoint(Global.gmaster.waypoints[my_point]);
	}

	void Update () {
		if (!Global.playable) return;
		rotateTo=Quaternion.LookRotation(nearest_point-transform.position,transform.TransformDirection(Vector3.up)).eulerAngles;
		d=Vector3.Distance(transform.position,nearest_point);
		if (d<CAPTURE_DISTANCE) 
		{
			my_point++;
			if (my_point>=Global.gmaster.waypoints.Length) my_point=0;
			nearest_point=GetNearestPoint(Global.gmaster.waypoints[my_point]);
		}
		else 
		{
			if (d>BRAKING_DISTANCE) 
			{
				if (pc.speed<pc.maxspeed) 
				{
					if (!pc.accelerating) pc.CmdAccelerate(true);
				}
				else 
				{
					if (pc.accelerating) pc.CmdAccelerate(false);
				}
			}
			else 
			{
				if (pc.speed>CAPTURING_SPEED) 
				{
					if (!pc.stopping) pc.CmdDesselerate(true);
				}
				else 
				{
					if (pc.stopping) pc.CmdDesselerate(false);
				}
			}
		}

		angular_speed=pc.angle_acceleration*Time.deltaTime;
		if (rotateTo.y>angular_speed) pc.CmdYRotateVector(1);
		else 
		{
				if (rotateTo.y<-angular_speed) pc.CmdYRotateVector(2);
				else 
				{
					if (pc.yrotate_vector!=Vector3.zero) pc.CmdYRotateVector(0);
				}
		}

		if (rotateTo.x>angular_speed) pc.CmdXRotateVector(2);
		else 
		{
			if (rotateTo.x<-angular_speed) pc.CmdXRotateVector(1);
			else 
			{
				if (pc.xrotate_vector!=Vector3.zero) pc.CmdXRotateVector(0);
			}
		}
			
	}

	Vector3 GetNearestPoint(GameObject g) 
	{
		float dist=40000;
		Vector3 p=g.transform.position;
		int c=g.transform.childCount;
		Transform tc;
		float dx;
		for (int i=0;i<c;i++) 
		{
			tc=g.transform.GetChild(i);
			if (tc) 
			{
				dx=Vector3.Distance(transform.position,tc.position);
				if (dx<=dist) 
				{
					dist=dx;
					p=tc.position;
				}
			}
		}
		return(p);
	}
}
