using UnityEngine;
using System.Collections;

public class IslandConstructor : MonoBehaviour {
	const int MAX_VERTICES=70;
	const int MAX_LEVELS=30;
	int numbers=0;

	public Material island_material;

	void Start () 
	{
		ConstructIsland(100);
	}

	void ConstructIsland (float radius) 
	{
		GameObject island=new GameObject ("island"+numbers.ToString());
		numbers++;
		Mesh m=new Mesh();
		MeshFilter mf=island.AddComponent<MeshFilter>();
		MeshRenderer mr=island.AddComponent<MeshRenderer>();

		int vertices_on_level=MAX_VERTICES;
		int levels=MAX_LEVELS;
		int count=2+vertices_on_level*levels;
		int index=1,i=0;
		float  angle=0,angle_step=Mathf.PI/vertices_on_level*2,height,length,fl;

		Vector3[] v=new Vector3[count];
		Vector2[] uvs=new Vector2[count];
		//vertices generation
		v[0]=new Vector3(0,radius,0);
		v[count-1]=new Vector3(0,-radius,0);
		for (i=0;i<levels;i++) 
		{
			height=1-(i+1)*2.0f/(levels+2);
			if (height<-1) height=-1;
			length=1-Mathf.Abs(height);
			angle=0;
			for (int j=0;j<vertices_on_level;j++)
			{
				index=1+i*vertices_on_level+j;
				v[index].x=Mathf.Cos(angle)*radius*length;
				v[index].z=Mathf.Sin(angle)*radius*length;
				v[index].y=height*radius;
				fl=j; fl/=vertices_on_level-1;print (fl);
				uvs[index].x=fl;
				fl=i+1;fl/=levels;
				uvs[index].y=fl;
				angle+=angle_step;
				index++;
			}
		}
		//triangles generation
		int[] tris=new int[6*vertices_on_level+(levels-1)*vertices_on_level*6];
		index=0;
		//upper part
		for (i=1;i<vertices_on_level;i++) 
		{
				tris[index]=0;
				tris[index+1]=i+1;
				tris[index+2]=i;
				index+=3;
		}
		tris[index]=0;
		tris[index+1]=1;
		tris[index+2]=vertices_on_level;
		index+=3;
		//main part
		if (1>0) {
			index=3*vertices_on_level;
		for (i=0;i<levels-1;i++)
		{
			tris[index]=i*vertices_on_level+1;
			tris[index+1]=(i+1)*vertices_on_level+1;
			tris[index+2]=(i+2)*vertices_on_level;
			index+=3;

			for (int j=1;j<vertices_on_level;j++)
			{
				tris[index]=i*vertices_on_level+j;
				tris[index+1]=(i+1)*vertices_on_level+j+1;
				tris[index+2]=(i+1)*vertices_on_level+j;

				tris[index+3]=i*vertices_on_level+j;
				tris[index+4]=i*vertices_on_level+j+1;
					tris[index+5]=(i+1)*vertices_on_level+j+1;
				index+=6;
			}
			
			tris[index]=(i+1)*vertices_on_level;
			tris[index+1]=i*vertices_on_level+1;
			tris[index+2]=(i+2)*vertices_on_level;
			index+=3;
		}
		}
		if (1>0) {
		//bottom part
		int last=v.Length-1;
			for (i=0;i<vertices_on_level-1;i++) 
		{
				tris[index]=v.Length-1-(vertices_on_level-i);
				tris[index+1]=v.Length-(vertices_on_level-i);
				tris[index+2]=last;
				index+=3;
		}
			tris[index]=last-1;
			tris[index+1]=last-vertices_on_level;
			tris[index+2]=last;
		}
		m.vertices=v;
		m.triangles=tris;
				m.uv=uvs;
	

		mf.mesh=m;
		mr.material=island_material;
	}
}
