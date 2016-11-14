using UnityEngine;
using System.Collections;

public static class Global {
	public static int gui_piece=16;
	public static int deadline_damage=10;

	public static bool playable=false;
	public static bool onServer=false;
	public static bool multiplayer=true;

	public static GameObject myPlayer;
	public static Camera cam;
	public static ParticleSystem player_explosion;

	public static GameMaster gmaster;
	public static ServiceMenu service_menu;

	public static Material red_material;
	public static Material star_material;
	public static Material grey_material;
	public static AudioSource effects_as;

	public static void PlayerExplosionRequest (Vector3 p)
	{
		player_explosion.transform.position=p;
		player_explosion.Emit(200);
	}
}
