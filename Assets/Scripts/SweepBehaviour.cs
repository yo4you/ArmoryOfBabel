using UnityEngine;

public class SweepBehaviour : HitBoxBehaviour
{
	[SerializeField]
	private float _knockBackForce = default;

	[SerializeField]
	private float _stunTime = default;

	public override void OnCollisionEnter2D(UnityEngine.Collision2D collision)
	{
		base.OnCollisionEnter2D(collision);
		var stunnable = collision.collider.gameObject.GetComponent<IStunnable>();
		if (stunnable != null)
		{
			stunnable.Stun();
			GetComponent<FollowPlayer>().Player.GetComponent<StunLifetimeUtil>()?.StartStun(stunnable, _stunTime);
			collision.collider.gameObject?.GetComponent<Rigidbody2D>().AddForce(
				(collision.collider.transform.position - transform.position).normalized * _knockBackForce
				);
		}
	}
}