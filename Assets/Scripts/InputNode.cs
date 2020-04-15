internal class InputNode
{
	public bool Held = false;
	private Node _inputHeld;

	public Node Input { get; set; }

	public Node InputHeld
	{
		get => _inputHeld; set
		{
			Held = value != null;
			_inputHeld = value;
		}
	}

	public float InstigationTime { get; internal set; } = float.MaxValue;
}
