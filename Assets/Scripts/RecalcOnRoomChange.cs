using SAP2D;
using UnityEngine;

public class RecalcOnRoomChange : MonoBehaviour
{
	private SAP2DPathfinder _pathFinder;

	private void RecalcOnRoomChange_OnDoorsOpen(RoomPopulator roompop)
	{
		_pathFinder.CalculateColliders();
	}

	private void Start()
	{
		_pathFinder = GetComponent<SAP2DPathfinder>();
		FindObjectOfType<RoomEvents>().OnDoorsOpen += RecalcOnRoomChange_OnDoorsOpen;
	}
}
