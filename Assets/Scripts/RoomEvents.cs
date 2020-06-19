using UnityEngine;

public class RoomEvents : MonoBehaviour
{
	internal delegate void DoorsOpenHandle(RoomPopulator roompop);

	internal event DoorsOpenHandle OnDoorsOpen;

	internal RoomPopulator ActiveRoom { get; set; }

	internal void DoorsOpened(RoomPopulator roomPopulator)
	{
		OnDoorsOpen?.Invoke(roomPopulator);
	}
}