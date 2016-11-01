using UnityEngine;
using System.Collections;

public static class Global {
	public static int gui_piece=16;
	public static int deadline_damage=10;

	public static bool playable=false;
	public static bool onServer=false;
	public static bool multiplayer=true;

	public static GameObject myPlayer;
	public static GameMaster gmaster;
	public static Material red_material;
	public static AudioSource effects_as;
}
