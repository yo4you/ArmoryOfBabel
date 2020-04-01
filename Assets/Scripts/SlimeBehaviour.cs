using SAP2D;
using UnityEngine;

public class SlimeBehaviour : MonoBehaviour
{
	private SAP2DAgent _agent;

	[SerializeField]
	private float _projectileSpeed;

	private Rigidbody2D _rb;

	[SerializeField]
	private float _shootInterval;

	[SerializeField]
	private ProjectileBehaviour _shot;

	[SerializeField]
	private float _swirfAmplitude;

	[SerializeField]
	private float _swirfSpeed;

	private void FixedUpdate()
	{
		var dist = (Vector2)(_agent.Target.position - transform.position);
		var perp_dist = new Vector2(-dist.y, dist.x);
		perp_dist *= Mathf.Sin(Time.fixedTime * _swirfSpeed) * _swirfAmplitude;
		_rb.MovePosition(_rb.position + perp_dist * _agent.MovementSpeed * Time.fixedDeltaTime);
	}

	private void OnDisable()
	{
		CancelInvoke();
	}

	private void OnEnable()
	{
		InvokeRepeating("Shoot", Random.Range(0, _shootInterval), _shootInterval);
	}

	private void Shoot()
	{
		var pos = transform.position;
		var dir = (_agent.Target.position - pos).normalized;
		var projectile = Instantiate(_shot, pos, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dir)));
		projectile.MoveDir = Vector2.right * _projectileSpeed;
	}

	private void Start()
	{
		_agent = GetComponent<SAP2DAgent>();
		_rb = GetComponent<Rigidbody2D>();
	}
}
