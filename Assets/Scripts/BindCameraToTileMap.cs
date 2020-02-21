using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BindCameraToTileMap : MonoBehaviour
{
	private Camera _camera;
	private Vector3 _cameraSize;
	private float _cameraOrthoSize;
	private Vector3 _cameraMinBound;
	private Vector3 _cameraMaxBound;
	private bool _tileMapsDirty;
	private IReadOnlyList<Tilemap> _tileMaps = new List<Tilemap>();

	public Vector3 Target { get; internal set; }
	public IReadOnlyList<Tilemap> TileMaps
	{
		get => _tileMaps;
		set
		{
			_tileMaps = value;
			_tileMapsDirty = true;
		}
	}

	private void Start()
	{
		_camera = Camera.main;
		TileMaps = new List<Tilemap>(FindObjectsOfType<Tilemap>());
	}

	private void RecalculateBounds()
	{
		List<Vector3> minimums = new List<Vector3>();
		List<Vector3> maximum = new List<Vector3>();
		
		foreach (var tilemap in TileMaps)
		{
			tilemap.CompressBounds();
		}

		foreach (var tilemap in TileMaps)
		{
			minimums.Add(tilemap.CellToWorld(tilemap.cellBounds.min));
			maximum.Add(tilemap.CellToWorld(tilemap.cellBounds.max));
		}

		_cameraMinBound = MathUtils.MinBound(minimums.ToArray());
		_cameraMaxBound = MathUtils.MaxBound(maximum.ToArray());
	}

	private void LateUpdate()
	{
		if (_cameraOrthoSize != _camera.orthographicSize)
		{
			CalculateCamSize();
		}
		if (_tileMapsDirty)
		{
			RecalculateBounds();
			_tileMapsDirty = false;
		}

		var cameraMin = Target - _cameraSize;
		var cameraMax = Target + _cameraSize;

		var offsetTransform = Target;
		for (int i = 0; i < 2; i++)
		{
			if (cameraMin[i] < _cameraMinBound[i])
			{
				offsetTransform[i] = _cameraMinBound[i] + _cameraSize[i];
			}
			if (cameraMax[i] > _cameraMaxBound[i])
			{
				offsetTransform[i] = _cameraMaxBound[i] - _cameraSize[i];
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
