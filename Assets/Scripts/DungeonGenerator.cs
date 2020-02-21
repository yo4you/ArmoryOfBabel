using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _prefabs;
	[SerializeField]
	private int _maxRooms = 10;
	[SerializeField]
	private int _maxSpawnTries = 100;
	Dictionary<Vector2Int, GameObject> _roomGrid = new Dictionary<Vector2Int,GameObject>();
	private Vector2Int[] _directions = new Vector2Int[] {
		new Vector2Int(0,1),
		new Vector2Int(1,0),
		new Vector2Int(0,-1),
		new Vector2Int(-1,0)
	};

	private void Start()
	{
		var cellSize = GetComponent<Grid>().cellSize;
		var cursor = new Vector2Int();
		
		for (int i = 0; i < _maxSpawnTries; i++)
		{

			var cursor_offset = _directions[Random.Range(0, _directions.Length)];
			
			if (!_roomGrid.ContainsKey(cursor))
			{
				var go = Instantiate(_prefabs[Random.Range(0, _prefabs.Length)], transform);
				foreach (var tilemap in go.GetComponents<Tilemap>())
				{
					tilemap.CompressBounds();
				}
				var roomData = go.GetComponent<RoomDataIdentifier>();
				Vector3 size = roomData.GetWallSize();
				Vector3 pos = new Vector3(
					size.x * cellSize.x * cursor.x,
					size.y * cellSize.y * cursor.y,
					0);
				go.transform.position = pos;
				_roomGrid.Add(cursor, go);
				CheckSurrounding(cursor);
			}

			if (_roomGrid.Count > _maxRooms)
			{
				return;
			}
			cursor += cursor_offset;
		}
	}

	private void CheckSurrounding(Vector2Int cursor)
	{
		for (int i = 0; i < _directions.Length; i++)
		{
			if (_roomGrid.TryGetValue(cursor + _directions[i], out GameObject go))
			{
				var ids = go.GetComponent<RoomDataIdentifier>();
				ids.Doorways[(i + 2) % 4].SetActive(false);
				_roomGrid[cursor].GetComponent<RoomDataIdentifier>().Doorways[i].SetActive(false);
			}
		}
	}
}
