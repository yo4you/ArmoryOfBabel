using UnityEngine;

public abstract class StatusEffect : MonoBehaviour
{
	protected bool _active;

	[SerializeField]
	private GameObject _visual;

	public GameObject Visual { get => _visual; internal set => _visual = value; }

	public virtual void Apply(Enemy enemy)
	{
		_active = true;
	}

	private void OnDestroy()
	{
		if (Visual)
		{
			Destroy(Visual);
		}
	}
}