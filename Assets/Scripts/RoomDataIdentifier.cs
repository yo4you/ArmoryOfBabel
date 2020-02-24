using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
/// <summary>
/// This class is used for room prefabs in our dungeon generator to identify parts of the prefab
/// </summary>
public class RoomDataIdentifier : MonoBehaviour
{
	enum Direction
	{
		NORTH,EAST,SOUTH,WEST
	}

	[SerializeField][Tooltip("the parts that are removed when a room is connected, order in N E S W")]
	GameObject[] _doorways;
	[SerializeField][Tooltip("the part with the colliders")]
	GameObject _walls;

	public GameObject[] Doorways { get => _doorways;  }
	/// <summary>
	/// compresses the bounds and returns the actual size of the collider part of this prefab
	/// </summary>
	/// <returns></returns>
	public Vector3Int GetWallSize()
	{
		var tilemap = _walls.GetComponent<Tilemap>();
		tilemap.CompressBounds();
		return tilemap.size;
	}

}
