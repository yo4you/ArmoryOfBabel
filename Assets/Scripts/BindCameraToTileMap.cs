using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// this class is used to ensure the camera stays in the bounds of the generated tilemap dungeon
/// </summary>
public class BindCameraToTileMap : MonoBehaviour
{
	private Camera _camera;
	private Vector3 _cameraMaxBound;

	// these two bounds represent the rectangle the camera should stay in in world space
	private Vector3 _cameraMinBound;

	private float _cameraOrthoSize;
	private Vector3 _cameraSize;

	// the tilemaps that are currendly being viewed, our camera will be stuck in their bounds
	private IReadOnlyList<Tilemap> _tileMaps = new List<Tilemap>();

	private bool _tileMapsDirty;

	// the subject the camera is following (the player)
	public Vector3 Target_Position { get; internal set; }

	public IReadOnlyList<Tilemap> TileMaps
	{
		get => _tileMaps;
		set
		{
			_tileMaps = value;
			_tileMapsDirty = true;
		}
	}

	public void Step()
	{
		// recalculate the camera corners in world space if the camera's size changes
		if (_cameraOrthoSize != _camera.orthographicSize)
		{
			_cameraOrthoSize = _camera.orthographicSize;
			CalculateCamSize();
		}
		if (_tileMapsDirty)
		{
			RecalculateBounds();
			_tileMapsDirty = false;
		}
		// we assign two corners relative to our target position
		var cameraMin = Target_Position - _cameraSize;
		var cameraMax = Target_Position + _cameraSize;
		// set the future position to the target position then correct if it it's out of bound
		var future_position = Target_Position;
		for (int i = 0; i < 2; i++)
		{
			if (cameraMin[i] < _cameraMinBound[i])
			{
				future_position[i] = _cameraMinBound[i] + _cameraSize[i];
			}
			if (cameraMax[i] > _cameraMaxBound[i])
			{
				future_position[i] = _cameraMaxBound[i] - _cameraSize[i];
			}
		}
		transform.position = future_position;
	}

	public void UpdateTileMaps()
	{
		TileMaps = new List<Tilemap>(FindObjectsOfType<Tilemap>());
	}

	private void BindCameraToTileMap_OnDoorsOpen(RoomPopulator roompop)
	{
		UpdateTileMaps();
	}

	/// <summary>
	/// stores the corners of the camera view in world space
	/// </summary>
	private void CalculateCamSize()
	{
		var cameraBound = _camera.ViewportToWorldPoint(new Vector3(1, 1, _camera.nearClipPlane));
		_cameraSize = new Vector3(
				cameraBound.x - _camera.transform.position.x,
				cameraBound.y - _camera.transform.position.y,
				0
		);
	}

	/// <summary>
	/// calculate the bounds, a rectangle that's drawn tightly around the loaded tilemaps
	/// </summary>
	private void RecalculateBounds()
	{
		// lower left corners of the tilemaps
		List<Vector3> minimums = new List<Vector3>();
		// upper right corners of the tilemaps
		List<Vector3> maximum = new List<Vector3>();

		foreach (var tilemap in TileMaps)
		{
			// tilemaps come with a bunch of empty space, this will ensure the bounds we get are wrapped tightly
			tilemap.CompressBounds();

			minimums.Add(tilemap.CellToWorld(tilemap.cellBounds.min));
			maximum.Add(tilemap.CellToWorld(tilemap.cellBounds.max));
		}
		// we only store the smallest values of minbound and the highest of maxbound
		_cameraMinBound = MathUtils.MinBound(minimums.ToArray());
		_cameraMaxBound = MathUtils.MaxBound(maximum.ToArray());
	}

	private void Start()
	{
		_camera = Camera.main;
		FindObjectOfType<RoomEvents>().OnDoorsOpen += BindCameraToTileMap_OnDoorsOpen; ;
		UpdateTileMaps();
	}
}
