using UnityEngine;
using System.Collections;

public class Autopilot : MonoBehaviour {
	int my_point=0;
	public Transform nearest_point;
	Vector3 inpos,inpos2;
	float d,angular_speed,a;

	const float CAPTURE_DISTANCE=15;
	const int BRAKING_DISTANCE=20;
	const int CAPTURING_SPEED=20;
	const float ACCELERATING_ANGLE=4;

	public playerController pc;

	void Start () 
	{
		nearest_point=GetNearestPoint(Global.gmaster.waypoints[my_point]);
	}

	void Update () {
		if (!Global.playable||!nearest_point) return;
		d=Vector3.Distance(transform.position,nearest_point.position);
		a=Vector3.Angle(transform.forward,nearest_point.position-transform.position);
		if (d<CAPTURE_DISTANCE) 
		{
			my_point++;
			if (my_point>=Global.gmaster.waypoints.Length) my_point=0;
			nearest_point=GetNearestPoint(Global.gmaster.waypoints[my_point]);
			Global.gmaster.players_positions[pc.myNumber]=my_point;
		}
		else 
		{
			if (d>BRAKING_DISTANCE) 
			{
				if (pc.speed<pc.maxspeed) 
				{
					if (pc.speed>CAPTURING_SPEED/2) 
					{
						if (pc.speed_dir!=1&&a<ACCELERATING_ANGLE) pc.CmdSpeedVector(1);
					}
					else 
					{
						if (pc.speed_dir!=1) pc.CmdSpeedVector(1);
					}
				}
				else 
				{
					if (pc.speed_dir==1) pc.CmdSpeedVector(0);
				}
			}
			else 
			{
				if (pc.speed>CAPTURING_SPEED) 
				{
					if (pc.speed_dir!=-1) pc.CmdSpeedVector(-1);
				}
				else 
				{
					if (pc.speed_dir==-1) pc.CmdSpeedVector(0);
					else {
						if (pc.speed_dir==0) pc.CmdSpeedVector(1);
					}
				}
			}
		}
			
		angular_speed=pc.angle_acceleration*Time.deltaTime;
		inpos=transform.InverseTransformPoint(nearest_point.position);

		//Y-AXIS ROTATION
		inpos2=inpos; inpos2.y=0;
		a=Vector3.Angle(Vector3.forward,inpos2);
		if (a>=angular_speed)
		{
			if (inpos2.x>0)
			{
				if (pc.yrot<1) pc.CmdYRotateVector(1);
			}
			else
			{
				if (pc.yrot>-1) pc.CmdYRotateVector(-1);
			}
		}
		else
		{
			if (pc.yrot!=0) pc.CmdYRotateVector(0);
		}
		//X-AXIS ROTATION
		inpos2=inpos; inpos2.x=0;
		a=Vector3.Angle(Vector3.forward,inpos2);
		if (a>angular_speed)
		{
			if (inpos2.y>0)
			{
				if (pc.xrot>-1) pc.CmdXRotateVector(-1);
			}
			else
			{
				if (pc.xrot<1) pc.CmdXRotateVector(1);
			}
		}
		else
		{
			if (pc.xrot!=0) pc.CmdXRotateVector(0);
		}
			
	}

	Transform GetNearestPoint(GameObject g) 
	{
		float dist=40000;
		int c=g.transform.childCount;
		Transform tc,st=null;
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
					st=tc;
				}
			}
		}
		return(st);
	}
}
