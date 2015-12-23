using UnityEngine;

public class WorldController : MonoBehaviour
{
	// Path to game resources, mod folder and map name
	public static string GamePath = "K:/Games/Half-Life 2", ModName = "/hl2/";
	public static string MapName = "background01";

	// Path's to directories with textures / materials
	public readonly static string DefaultTexPath = GamePath + ModName + "/materials/";
	public readonly static string PakTexPath = Application.persistentDataPath + "/" + MapName + "_pakFile/materials/";

	// Current path to directory with textures / materials
	public static string CurrentTexPath = DefaultTexPath;

	public const float WorldScale = 0.0254f;

	public void Start()
	{
		// Load map after initialize scene
		SourceBspLoader.LoadBSP ();
	}

	public static Vector3 SwapZY(Vector3 inp)
	{
		return new Vector3 (-inp.x, inp.z, -inp.y);
	}
}
