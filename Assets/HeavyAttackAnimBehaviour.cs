using System;
using UnityEngine;

public class HeavyAttackAnimBehaviour : StateMachineBehaviour
{
	private readonly static Lazy<PlayerAttackControl> _attackControl = new Lazy<PlayerAttackControl>(() => FindObjectOfType<PlayerAttackControl>());

	[SerializeField]
	[Range(0f, 1f)]
	private float _queuePeriod = 0.5f;

	private bool _triggeredQueueAction = false;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_triggeredQueueAction = false;
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_attackControl.Value.CanQueue = false;
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);

		if (!_triggeredQueueAction)
		{
			if ((stateInfo.normalizedTime % 1f) > _queuePeriod)
			{
				_triggeredQueueAction = true;
				_attackControl.Value.CanQueue = true;
			}
		}
	}

	// OnStateMove is called right after Animator.OnAnimatorMove()
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    // Implement code that processes and affects root motion
	//}
}
