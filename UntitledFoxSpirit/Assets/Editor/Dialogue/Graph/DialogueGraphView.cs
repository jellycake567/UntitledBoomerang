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
    public const string PLAYER = "Player";
    public const string NPC = "NPC";

    public readonly Vector2 DefaultNodeSize = new Vector2(250f, 200f);

    private NodeSearchWindow _searchWindow;

    public DialogueGraphView(DialogueEditor editorWindow)
    {
        // Background lines color
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
        //AddToClassList("node");

        styleSheets.Add(Resources.Load<StyleSheet>("NodeViewStyle"));

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

            DialogueNode bNode = baseNode as DialogueNode;
            if (bNode.choices.Count > 0)
            {
                string portGUID = edge.output.name;
                DialogueChoices choice = bNode.choices.First(x => x.portGUID == portGUID);
                
                DialogueNode tNode = targetNode as DialogueNode;

                choice.targetGUID = tNode.GUID;
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

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiablePorts = new List<Port>();
        ports.ForEach((port) =>
        {
            if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                compatiablePorts.Add(port);
        });

        return compatiablePorts;
    }

    #region Nodes

    public void CreateNode(string nodeName, Vector2 position, bool choice)
    {
        DialogueNode dialogueNode = choice ? new DialogueNode(Guid.NewGuid().ToString(), nodeName, this, true) : new DialogueNode(Guid.NewGuid().ToString(), nodeName, this);

        dialogueNode.Draw(position, DefaultNodeSize);

        AddElement(dialogueNode);
    }

    #endregion
}
