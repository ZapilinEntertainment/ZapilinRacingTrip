using UnityEngine;
using System.Collections;

public class push_button : MonoBehaviour {
	const float rot_speed=15;
	
	void Start() 
	{
		Collider c= gameObject.GetComponent<Collider>();
		if (c) c.isTrigger=true;
	}

	void Update () {
		transform.Rotate(Vector3.up*rot_speed*Time.deltaTime,Space.Self);
	}

	void OnTriggerEnter(Collider c) 
	{
		if (c.transform.root.gameObject==Global.myPlayer) 
		{
			Global.gmaster.Next();
			Global.effects_as.PlayOneShot(Global.gmaster.button_sound);
			if (Global.onServer) Global.myPlayer.SendMessage("SetBonus",new Vector2(2,3),SendMessageOptions.DontRequireReceiver);
		}
	}
}
