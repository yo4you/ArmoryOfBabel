using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// This class is used for room prefabs in our dungeon generator to identify parts of the prefab
/// </summary>
public class RoomDataIdentifier : MonoBehaviour
{
	[SerializeField]
	[Tooltip("the parts that are removed when a room is connected, order in N E S W")]
	private GameObject[] _doorways;

	[SerializeField]
	[Tooltip("the part with the colliders")]
	private GameObject _walls;

	private Tilemap _wallsMap;

	private enum Direction
	{
		NORTH, EAST, SOUTH, WEST
	}

	public GameObject[] Doorways => _doorways;

	public void FixDoors()
	{
		foreach (var door in _doorways)
		{
			door.GetComponent<Tilemap>().CompressBounds();
		}
	}

	public Bounds GetBounds()
	{
		var tilemap = GetWalls();
		tilemap.CompressBounds();
		return tilemap.localBounds;
	}

	public Tilemap GetWalls()
	{
		return _wallsMap ?? _walls.GetComponent<Tilemap>();
	}

	/// <summary>
	/// compresses the bounds and returns the actual size of the collider part of this prefab
	/// </summary>
	/// <returns></returns>
	public Vector3Int GetWallSize()
	{
		var tilemap = GetWalls();
		tilemap.CompressBounds();
		return tilemap.size;
	}
}
