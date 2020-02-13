using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomDataIdentifier : MonoBehaviour
{
	enum Direction
	{
		NORTH,EAST,SOUTH,WEST
	}

	[SerializeField]
	GameObject[] _doorways;
	[SerializeField]
	GameObject _walls;

	public Vector3Int GetWallSize()
	{
		var tilemap = _walls.GetComponent<Tilemap>();
		return tilemap.size;
	}

}
