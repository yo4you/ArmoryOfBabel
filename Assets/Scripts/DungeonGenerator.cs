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
	[Tooltip("the maximum amount of times the prefabs will be spawned")]
	private int _maxRooms = 10;

	[SerializeField]
	[Tooltip("the maximum amount of times the system will try to spawn these rooms")]
	private int _maxSpawnTries = 100;

	private SAP2DPathfinder _pathFinder;
	[SerializeField] private LayerMask _pathFindingLayerMask;

	[SerializeField]
	[Tooltip("prefabs we're using to spawn the dungeon out of")]
	private GameObject[] _prefabs;

	// stores all the rooms we've already spawned
	private Dictionary<Vector2Int, GameObject> _roomGrid = new Dictionary<Vector2Int, GameObject>();

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
			if (_roomGrid.TryGetValue(cursor + _directions[i], out GameObject go))
			{
				_roomGrid[cursor].GetComponent<RoomDataIdentifier>().Doorways[i].SetActive(false);
				// get the adjacent room and break down the opposite facing doorway in that room
				var ids = go.GetComponent<RoomDataIdentifier>();
				ids.Doorways[(i + 2) % 4].SetActive(false);
			}
		}
	}

	private void Start()
	{
		// the scale of individual cells
		var cellSize = GetComponent<Grid>().cellSize;
		// the "cursor" is where we are in the dungeon while we're generating it, ensuring each new room is connected to the last
		var cursor = new Vector2Int();
		var min = new Vector2Int(99999, 99999);
		var max = new Vector2Int(-99999, -99999);
		// iterate trough our tries so the dungeon generator doesn't halt
		for (int i = 0; i < _maxSpawnTries; i++)
		{
			// pick a random cardinal direction to move our cursor into
			var cursor_offset = _directions[Random.Range(0, _directions.Length)];

			// don't overlap rooms
			if (!_roomGrid.ContainsKey(cursor))
			{
				var go = Instantiate(_prefabs[Random.Range(0, _prefabs.Length)], transform);
				foreach (var tilemap in go.GetComponents<Tilemap>())
				{
					// remove any dead space in the tilemaps so they fit tightly together
					tilemap.CompressBounds();
				}
				// contains refrences in the prefab so we can identify the doors/walls and disable them as needed
				var roomData = go.GetComponent<RoomDataIdentifier>();
				Vector3 size = roomData.GetWallSize();
				// the position in worldspace our new room should be generated at
				Vector3 pos = new Vector3(
					size.x * cellSize.x * cursor.x,
					size.y * cellSize.y * cursor.y,
					0);
				go.transform.position = pos;
				_roomGrid.Add(cursor, go);
				CreateDoorWaysAtPosition(cursor);
			}

			if (_roomGrid.Count > _maxRooms)
			{
				break;
			}
			cursor += cursor_offset;
		}
		UpdatePathFinderData(cellSize);
	}

	private void UpdatePathFinderData(Vector3 cellSize)
	{
		List<Vector3> minimums = new List<Vector3>();
		List<Vector3> maximum = new List<Vector3>();
		foreach (var tilemap in FindObjectsOfType<Tilemap>())
		{
			tilemap.CompressBounds();
			minimums.Add(tilemap.CellToWorld(tilemap.cellBounds.min));
			maximum.Add(tilemap.CellToWorld(tilemap.cellBounds.max));
		}
		var minBound = MathUtils.MinBound(minimums.ToArray());
		var maxBound = MathUtils.MaxBound(maximum.ToArray());

		var sizeInCells = (maxBound - minBound) / cellSize.x;
		_pathFinder = FindObjectOfType<SAP2DPathfinder>();
		_pathFinder.RemoveGrid(_pathFinder.GetGrid(0));
		_pathFinder.AddGrid((int)sizeInCells.x, (int)sizeInCells.y);
		var pfGrid = _pathFinder.GetGrid(0);
		pfGrid.Position = minBound;
		pfGrid.ObstaclesLayer = _pathFindingLayerMask;
		pfGrid.TileDiameter = cellSize.x;
		StartCoroutine(StartColliderCalc());
	}
}
