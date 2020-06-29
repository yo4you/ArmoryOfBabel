/// <summary>
/// used to store callbacks, when the activator is activated 
/// </summary>
public class NodeActivationCallBack
{
	// the node that instigated the callback
	public Node Activatee;
	// the node needs to be called upon resolution of the callback conditions
	public Node Activator;

	public NodeActivationCallBack(Node activatee, Node activator)
	{
		Activatee = activatee;
		Activator = activator;
	}
}