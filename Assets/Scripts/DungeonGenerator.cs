using SAP2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

/// <summary>
/// this is a simple class for generating a set of tilemap rooms into a dungeon shape
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
	// direction offsets stored in NESW order
	private Vector2Int[] _directions = new Vector2Int[] {
		new Vector2Int(0,1),
		new Vector2Int(1,0),
		new Vector2Int(0,-1),
		new Vector2Int(-1,0)
	};

	[SerializeField]
	private GameObject[] _enemyPrefabs;

	[SerializeField]
	[Tooltip("the maximum amount of times the prefabs will be spawned")]
	private int _maxRooms = 10;

	[SerializeField]
	[Tooltip("the maximum amount of times the system will try to spawn these rooms")]
	private int _maxSpawnTries = 100;

	private SAP2DPathfinder _pathFinder;

	[SerializeField] private LayerMask _pathFindingLayerMask;

	private Transform _player;

	[SerializeField]
	[Tooltip("prefabs we're using to spawn the dungeon out of")]
	private GameObject[] _prefabs;

	// stores all the rooms we've already spawned
	private Dictionary<Vector2Int, GameObject> _roomGrid = new Dictionary<Vector2Int, GameObject>();

	[SerializeField]
	private bool _spawnEnemies;

	public IEnumerator StartColliderCalc()
	{
		yield return new WaitForFixedUpdate();
		_pathFinder.CalculateColliders();
	}

	/// <summary>
	/// Creates doorways connecting the adjacent rooms to the ones at the cursor position
	/// </summary>
	/// <param name="cursor">the cursor position of the room that should be connected</param>
	private void CreateDoorWaysAtPosition(Vector2Int cursor)
	{
		// check each direction relative to the room we've just spawned
		for (int i = 0; i < _directions.Length; i++)
		{
			// if there's a room adjacent to our room break down the doorway in that direction
			if (_roomGrid.TryGetValue(cursor + _directions[i], out GameObject adjenctRoom))
			{
				var adjenctRoomPop = adjenctRoom.GetComponent<RoomPopulator>();

				var ids = adjenctRoom.GetComponent<RoomDataIdentifier>();
				ids.FixDoors();
				if (adjenctRoomPop)
				{
					adjenctRoomPop.DoorsToOpen.Add(ids.Doorways[(i + 2) % 4]);
				}
				// TODO set the rooms that should be enabled and disabled here
				var Roompop = _roomGrid[cursor].GetComponent<RoomPopulator>();
				if (Roompop)
				{
					Roompop.AdjenctRooms.Add(adjenctRoom);
					Roompop.DoorsToOpen.Add(_roomGrid[cursor].GetComponent<RoomDataIdentifier>().Doorways[i]);
					Roompop.DoorsToOpen.Add(ids.Doorways[(i + 2) % 4]);
				}
			}
		}
	}

	private void Start()
	{
		_pathFinder = FindObjectOfType<SAP2DPathfinder>();
		_player = FindObjectOfType<PlayerMovement>().gameObject.transform;
		// the scale of individual cells
		var cellSize = GetComponent<Grid>().cellSize;
		// the "cursor" is where we are in the dungeon while we're generating it, ensuring each new room is connected to the last
		var cursor = new Vector2Int();
		var min = new Vector2Int(99999, 99999);
		var max = new Vector2Int(-99999, -99999);
		List<Tilemap> tilemaps = new List<Tilemap>();
		// iterate trough our tries so the dungeon generator doesn't halt
		for (int i = 0; i < _maxSpawnTries; i++)
		{
			// pick a random cardinal direction to move our cursor into
			var cursor_offset = _directions[Random.Range(0, _directions.Length)];

			// don't overlap rooms
			if (!_roomGrid.ContainsKey(cursor))
			{
				var go = Instantiate(_prefabs[Random.Range(0, _prefabs.Length)], transform);

				// contains refrences in the prefab so we can identify the doors/walls and disable them as needed
				var roomData = go.GetComponent<RoomDataIdentifier>();
				tilemaps.Add(roomData.GetWalls());
				Vector3 size = roomData.GetWallSize();
				size.z = 100f;
				// the position in worldspace our new room should be generated at
				Vector3 pos = new Vector3(
					size.x * cellSize.x * cursor.x,
					size.y * cellSize.y * cursor.y,
					0);

				go.transform.position = pos;
				var roomPop = go.AddComponent<RoomPopulator>();
				roomPop.Player = _player;
				var bounds = roomData.GetBounds();
				bounds.center += pos;
				var extends = bounds.extents;
				extends.z = 100;
				bounds.extents = extends;
				roomPop.Bounds = bounds;
				roomPop.PathFinder = _pathFinder;
				if (_spawnEnemies)
				{
					for (int j = 0; j < Random.Range(1, 3); j++)
					{
						var enemy = Instantiate(_enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)], bounds.center + (Vector3)(Random.insideUnitCircle * Random.Range(0f, .4f)), new Quaternion());
						roomPop.Enemies.Add(enemy.GetComponent<SAP2DAgent>());
						enemy.SetActive(false);
					}
				}

				_roomGrid.Add(cursor, go);
				if (!bounds.Contains(_player.transform.position))
				{
					roomPop.gameObject.SetActive(false);
				}
			}

			if (_roomGrid.Count > _maxRooms)
			{
				break;
			}
			cursor += cursor_offset;
		}
		foreach (var gridPoint in _roomGrid)
		{
			CreateDoorWaysAtPosition(gridPoint.Key);
		}

		UpdatePathFinderData(cellSize, tilemaps);
	}

	private void UpdatePathFinderData(Vector3 cellSize, List<Tilemap> tilemaps)
	{
		List<Vector3> minimums = new List<Vector3>();
		List<Vector3> maximum = new List<Vector3>();
		foreach (var tilemap in tilemaps)
		{
			tilemap.CompressBounds();
			minimums.Add(tilemap.CellToWorld(tilemap.cellBounds.min));
			maximum.Add(tilemap.CellToWorld(tilemap.cellBounds.max));
		}
		var minBound = MathUtils.MinBound(minimums.ToArray());
		var maxBound = MathUtils.MaxBound(maximum.ToArray());

		var sizeInCells = (maxBound - minBound) / cellSize.x;
		_pathFinder.RemoveGrid(_pathFinder.GetGrid(0));
		_pathFinder.AddGrid((int)sizeInCells.x, (int)sizeInCells.y);
		var pfGrid = _pathFinder.GetGrid(0);
		pfGrid.Position = minBound;
		pfGrid.ObstaclesLayer = _pathFindingLayerMask;
		pfGrid.TileDiameter = cellSize.x;
		StartCoroutine(StartColliderCalc());
	}
}
