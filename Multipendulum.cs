using UnityEngine;
using System.Collections;

public class Multipendulum : MonoBehaviour {
	GameObject[] spheres;
	LineRenderer[] lines;
	Vector3[] rot_direction;
	float[] speed;
	GameObject s;
	const int SPHERES_COUNT=8;

	void Start () 
	{
		rot_direction=new Vector3[SPHERES_COUNT];
		rot_direction[0]=Vector3.up;
		rot_direction[1]=Vector3.down;
		rot_direction[2]=Vector3.right;
		rot_direction[3]=Vector3.left;
		rot_direction[4]=new Vector3(1,1,0);
		rot_direction[5]=new Vector3(-1,1,0);
		rot_direction[6]=new Vector3(1,-1,0);
		rot_direction[7]=new Vector3(-1,-1,0);

		Vector3[] start_positions=new Vector3[SPHERES_COUNT];
		Vector3 ps=Random.insideUnitCircle;
		start_positions[0]=new Vector3(ps.x,0,ps.y);
		ps=Random.insideUnitCircle;
		start_positions[1]=new Vector3(ps.x,0,ps.y);
		ps=Random.insideUnitCircle;
		start_positions[2]=new Vector3(ps.x,ps.y,0);
		ps=Random.insideUnitCircle;
		start_positions[3]=new Vector3(ps.x,ps.y,0);
		start_positions[4]=Random.onUnitSphere;
		start_positions[5]=Random.onUnitSphere;
		start_positions[6]=Random.onUnitSphere;
		start_positions[7]=Random.onUnitSphere;

		int i,sz;
		spheres=new GameObject[SPHERES_COUNT];
		speed=new float[SPHERES_COUNT];
		GameObject spref=Instantiate(Resources.Load<GameObject>("energy_sphere_pref")) as GameObject;
		for (i=0;i<SPHERES_COUNT;i++)
		{
			spheres[i]=Instantiate(spref) as GameObject;
			sz=(int)(10+Random.value*50);
			spheres[i].transform.localScale=Vector3.one*sz;
			spheres[i].transform.position=transform.position+start_positions[i]*sz*10;
			hurt_trigger ht = spheres[i].AddComponent<hurt_trigger>();
			ht.damage=sz;
			speed[i]=60-sz;
		}
		Destroy(spref);

		LineRenderer lr_pref=Instantiate(Resources.Load<LineRenderer>("energy_line_pref")) as LineRenderer;
		lines=new LineRenderer[SPHERES_COUNT];
		for (i=0;i<SPHERES_COUNT;i++) 
		{
			lines[i]=Instantiate(lr_pref) as LineRenderer;
			lines[i].SetPosition(0,transform.position);
		}

	}

	void Update () 
	{
		if (!Global.playable) return;
		for (int i=0;i<SPHERES_COUNT;i++) 
		{
			s=spheres[i];
			if (s==null) continue;
			s.transform.RotateAround(transform.position,rot_direction[i],speed[i]*Time.deltaTime);
			lines[i].SetPosition(1,s.transform.position);
		}
	}
}
