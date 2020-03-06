﻿using UnityEngine;
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

	private enum Direction
	{
		NORTH, EAST, SOUTH, WEST
	}

	public GameObject[] Doorways => _doorways;

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
