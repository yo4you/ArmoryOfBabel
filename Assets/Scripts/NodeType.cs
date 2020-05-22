public static class NodeTypes
{
	public static NodeType[] Types = new NodeType[]
	{
		new NodeType("VAL",      "BtnVal.png",  "UI/ Treshold Input",       NodeBehaviour.SetState_ValNode),
		new NodeType("A",       "BtnA.png",     "Input/ A Button" ,         NodeBehaviour.SetState_GenericNode),
		new NodeType("B",       "BtnB.png",     "Input/ B Button" ,         NodeBehaviour.SetState_GenericNode),
		new NodeType("X",       "BtnX.png",     "Input/ X Button" ,         NodeBehaviour.SetState_GenericNode),
		new NodeType("R",       "BtnX.png",     "Input/ R Button" ,         NodeBehaviour.SetState_GenericNode),
		new NodeType("OUT",     "BtnOut.png",   "Output/ Activator" ,       NodeBehaviour.SetState_OutNode),
		new NodeType("OR",      "BtnOR.png",    "Logic Gate/ Or" ,          NodeBehaviour.SetState_OrNode),
		new NodeType("AND",     "BtnAnd.png",   "Logic Gate/ And" ,         NodeBehaviour.SetState_AndNode),
		new NodeType("NOT",     "BtnNot.png",   "Logic Gate/ Not" ,         NodeBehaviour.SetState_NotNode),
		new NodeType("UI",      "BtnUI.png",    "UI/ Element" ,             NodeBehaviour.SetState_UINode),
		new NodeType("HIT",     "BtnHit.png",   "Hit Response" ,            NodeBehaviour.SetState_HitNode),
		new NodeType("TYPE",    "Settings.png", "Output/ Property Type" ,   NodeBehaviour.SetState_GenericNode),
		new NodeType("SPD",     "menu.png",     "Output/ Property Speed" ,  NodeBehaviour.SetState_ValNode),
		new NodeType("DMG",     "starRate.png", "Output/ Property Damage",  NodeBehaviour.SetState_ValNode),
		new NodeType("AH",      "BtnAh.png",    "Input/ Hold A Button" ,    NodeBehaviour.SetState_GenericNode),
		new NodeType("BH",      "BtnBh.png",    "Input/ Hold B Button" ,    NodeBehaviour.SetState_GenericNode),
		new NodeType("XH",      "BtnXh.png",    "Input/ Hold X Button" ,    NodeBehaviour.SetState_GenericNode),
		new NodeType("UIC",     "BtnUIcap.png", "UI/ Element Capacity" ,    NodeBehaviour.SetState_GenericNode),
		new NodeType("DT",      "dt.png",       "Delta Time Value" ,        NodeBehaviour.SetState_ValNode),
		new NodeType("COPY",    "BtnCopy.png",  "UI/ Value Copy" ,          NodeBehaviour.SetState_CopyNode),
		new NodeType("SUM",     "BtnSUM.png",   "Summation Value" ,         NodeBehaviour.SetState_SumNode),
		new NodeType("MOV",     "knob.png",     "Output/ Movement" ,        NodeBehaviour.SetState_MoveNode),
		new NodeType("STAT",    "stat.png",     "Output/ Status Effect",    NodeBehaviour.SetState_ValNode),
		new NodeType("HP",      "hp.png",       "Output/ Hit Points",       NodeBehaviour.SetState_UINode),
		new NodeType("MULT",    "mult.png",     "Multiplier",               NodeBehaviour.SetState_GenericNode),
		new NodeType("TRESH",   null,           null,                       NodeBehaviour.SetState_TreshNode),
	};
}

public class NodeType
{
	public readonly string EditorImage;

	public readonly NodeHandleDelegate ExecutionFunction;

	public readonly string Menu;

	public readonly string Tag;

	public NodeType(string tag, string editorImage, string menu, NodeHandleDelegate executionFunction)
	{
		Tag = tag;
		EditorImage = editorImage;
		Menu = menu;
		ExecutionFunction = executionFunction;
	}

	public delegate void NodeHandleDelegate(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseState);
}
