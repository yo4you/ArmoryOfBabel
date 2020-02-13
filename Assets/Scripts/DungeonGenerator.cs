using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
	[SerializeField]
	GameObject[] _prefabs;

    void Start()
    {
		List<Vector3> placedPosition = new List<Vector3>();
		var cellSize = GetComponent<Grid>().cellSize;

		for (int i = 0; i < 5; i++)
		{
			var go = Instantiate(_prefabs[Random.Range(0,_prefabs.Length)], transform);
			var roomData = go.GetComponent<RoomDataIdentifier>();
			Vector3 size = roomData.GetWallSize();
			int xAxis = Random.Range(0, 1);
			size.x *= cellSize.x * xAxis;
			size.y *= cellSize.y * 1-xAxis;
			size *= Random.Range(0,1) == 0 ?
		}
    }
}
