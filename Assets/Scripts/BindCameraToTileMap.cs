using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BindCameraToTileMap : MonoBehaviour
{
	private Camera _camera;
	private Vector3 _cameraSize;
	private float _cameraOrthoSize;
	public Vector3 Target { get; internal set; }
	public List<Tilemap> TileMaps { get; set; } = new List<Tilemap>();
	private void Start()
	{
		_camera = Camera.main;
	}

	private void CombinedBounds(out Vector3 min, out Vector3 max)
	{
		List<Vector3> minimums = new List<Vector3>();
		List<Vector3> maximum = new List<Vector3>();

		foreach (var tilemap in TileMaps)
		{
			minimums.Add(tilemap.CellToWorld(tilemap.cellBounds.min));
			maximum.Add(tilemap.CellToWorld(tilemap.cellBounds.max));
		}

		min = MathUtils.MinBound(minimums.ToArray());
		max = MathUtils.MaxBound(maximum.ToArray());
	}

	private void LateUpdate()
	{

		TileMaps = new List<Tilemap>();
		TileMaps.AddRange(FindObjectsOfType<Tilemap>());
		TileMaps.RemoveAll((Tilemap map) => !map.enabled);
		foreach (var tilemap in TileMaps)
		{
			tilemap.CompressBounds();
		}


		transform.position = Target;
		if (_cameraOrthoSize != _camera.orthographicSize)
		{
			CalculateCamSize();
		}
		CombinedBounds(out Vector3 cameraMinBound, out Vector3 cameraMaxBound);
		foreach (var tilemap in TileMaps)
		{

		}

		var cameraMin = transform.position - _cameraSize;
		var cameraMax = transform.position + _cameraSize;

		var offsetTransform = transform.position;
		for (int i = 0; i < 2; i++)
		{
			if (cameraMin[i] < cameraMinBound[i])
			{
				offsetTransform[i] = cameraMinBound[i] + _cameraSize[i];
			}
			if (cameraMax[i] > cameraMaxBound[i])
			{
				offsetTransform[i] = cameraMaxBound[i] - _cameraSize[i];
			}
		}
		transform.position = offsetTransform;
	}

	private void CalculateCamSize()
	{
		_cameraOrthoSize = _camera.orthographicSize;
		var cameraBound = _camera.ViewportToWorldPoint(new Vector3(1, 1, _camera.nearClipPlane));
		_cameraSize = new Vector3(
				cameraBound.x - _camera.transform.position.x,
				cameraBound.y - _camera.transform.position.y,
				0
		);
	}
}
