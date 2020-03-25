using UnityEngine;

public class RoomEvents : MonoBehaviour
{
	internal delegate void DoorsOpenHandle(RoomPopulator roompop);

	internal event DoorsOpenHandle OnDoorsOpen;

	internal void DoorsOpened(RoomPopulator roomPopulator)
	{
		OnDoorsOpen?.Invoke(roomPopulator);
	}
}
