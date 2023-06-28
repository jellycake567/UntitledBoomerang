using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.GraphView;

public class DialogueGraphView : GraphView
{
    public const string BASIC_ROOM = "Basic Room";
    public const string CHALLENGE_ROOM = "Challenge Room";
    public const string BOSS_ROOM = "Boss Room";
    public const string TELEPORTER_ROOM = "Teleporter Room";

    public readonly Vector2 DefaultNodeSize = new Vector2(200f, 250f);

    private NodeSearchWindow _searchWindow;

    public DialogueGraphView(DialogueEditor editorWindow)
    {
        // Background lines color
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
        AddToClassList("node");

        // Allows zoom in and out
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        // Add background lines
        GridBackground grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        AddSearchWindow(editorWindow);

        graphViewChanged = OnGraphChange;
    }

    private GraphViewChange OnGraphChange(GraphViewChange change)
    {
        if (change.edgesToCreate != null)
        {
            // Get edge created
            Edge edge = change.edgesToCreate[0];

            // Get target node
            Node targetNode = edge.input.node;

            // Get base node
            Node baseNode = edge.output.node;

            // Get all target node's input connection
            List<Edge> connections = edges.ToList().Where(x => x.input.node == targetNode).ToList();

            // Check for duplicated edges
            foreach (Edge connection in connections)
            {
                if (connection.output.node == baseNode && connection.input.node == targetNode)
                {
                    EditorUtility.DisplayDialog("Error", "Cannot create duplicate edge!", "OK");
                    change.edgesToCreate.Clear();
                    return change;
                }
            }
        }

        return change;
    }


    #region Search Window

    private void AddSearchWindow(DialogueEditor editorWindow)
    {
        _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _searchWindow.Configure(editorWindow, this);

        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
    }

    #endregion

    #region Ports

    private Port GeneratePort(DialogueNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiablePorts = new List<Port>();
        ports.ForEach((port) =>
        {
            if (startPort != port && startPort.node != port.node)
                compatiablePorts.Add(port);
        });

        return compatiablePorts;
    }

    #endregion

    #region Nodes

    public DialogueNode GenerateEntryPointNode()
    {
        DialogueNode node = new DialogueNode(Guid.NewGuid().ToString(), "START", true);

        node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

        VisualElement text = new TextElement();
        node.inputContainer.Add(text);

        // Create output port
        Port outputPort = GeneratePort(node, Direction.Output, Port.Capacity.Multi);
        outputPort.portName = "";
        node.outputContainer.Add(outputPort);

        node.name = "root";
        node.styleSheets.Add(Resources.Load<StyleSheet>("NodeViewStyle"));

        // Refresh node
        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(425f, 0, 100f, 150f));

        return node;
    }

    public void CreateNode(string nodeName, Vector2 position)
    {
        AddElement(CreateRoomNode(nodeName, position));
    }

    public DialogueNode CreateRoomNode(string nodeName, Vector2 position)
    {
        DialogueNode roomNode = new DialogueNode(Guid.NewGuid().ToString(), nodeName);

        // Add ports
        AddPorts(roomNode);

        // Room type
        roomNode.name = RoomType(nodeName);
        roomNode.styleSheets.Add(Resources.Load<StyleSheet>("NodeViewStyle"));

        // Refresh node
        roomNode.RefreshExpandedState();
        roomNode.RefreshPorts();

        roomNode.SetPosition(new Rect(position, DefaultNodeSize));
        return roomNode;
    }

    private string RoomType(string nodeName)
    {
        switch (nodeName)
        {
            case BASIC_ROOM:
                return "basic";
            case CHALLENGE_ROOM:
                return "challenge";
            case BOSS_ROOM:
                return "boss";
            case TELEPORTER_ROOM:
                return "teleporter";
        }

        return "basic";
    }

    private void AddPorts(DialogueNode roomNode)
    {
        CreateInputPorts(roomNode, Port.Capacity.Multi);
        CreateOutputPorts(roomNode, Port.Capacity.Multi);
    }

    private void CreateInputPorts(DialogueNode roomNode, Port.Capacity capacity)
    {
        Port inputPort = GeneratePort(roomNode, Direction.Input, capacity);
        inputPort.portName = "";
        roomNode.inputContainer.Add(inputPort);
    }

    private void CreateOutputPorts(DialogueNode roomNode, Port.Capacity capacity)
    {
        Port outputPort = GeneratePort(roomNode, Direction.Output, capacity);
        outputPort.portName = "";
        roomNode.outputContainer.Add(outputPort);
    }

    

    #endregion
}
