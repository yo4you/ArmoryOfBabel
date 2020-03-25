using SAP2D;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class RoomPopulator : MonoBehaviour
{
	private bool _activated;
	private EnemyCounterUI _enemyCounter;
	private bool _finished;
	private RoomEvents _roomEvents;
	public List<GameObject> AdjenctRooms { get; set; } = new List<GameObject>();
	public Bounds Bounds { get; set; }
	public List<GameObject> DoorsToOpen { get; set; } = new List<GameObject>();

	public List<GameObject> Enemies { get; set; } = new List<GameObject>();

	public SAP2DPathfinder PathFinder { get; internal set; }
	public Transform Player { get; set; }

	public void OpenDoors()
	{
		foreach (var room in AdjenctRooms)
		{
			room.SetActive(true);
		}
		foreach (var door in DoorsToOpen)
		{
			door.SetActive(false);
		}

		_finished = true;
		PathFinder.CalculateColliders();
		_roomEvents.DoorsOpened(this);
	}

	public void Step()
	{
		if (_finished)
		{
			return;
		}

		var containsPlayer = Bounds.Contains(Player.transform.position);
		if (!_activated && containsPlayer)
		{
			_enemyCounter.SetVisible(true);
			Activate();
		}
		else if (_activated && !containsPlayer)
		{
			_enemyCounter.SetVisible(false);
			DeActivateAI();
		}
		else if (_activated && containsPlayer)
		{
			_enemyCounter.SetVisible(true);
			_enemyCounter.SetTally(Enemies.Count(e => e.activeSelf), Enemies.Count);
		}

		if (_activated && (!Enemies.Any(i => i.activeSelf)))
		{
			_enemyCounter.SetVisible(false);
			OpenDoors();
		}
	}

	private void Activate()
	{
		_activated = true;
		foreach (var enemy in Enemies)
		{
			enemy.SetActive(true);
		}
	}

	private void DeActivateAI()
	{
	}

	private void Start()
	{
		_roomEvents = FindObjectOfType<RoomEvents>();
		_enemyCounter = FindObjectOfType<EnemyCounterUI>();
	}

	private void Update()
	{
		Step();
	}
}
