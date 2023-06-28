using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;


public class GraphSaveUtility : MonoBehaviour
{
    private DialogueGraphView _targetGraphView;
    private DialogueContainer _containerCache;

    private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<DialogueNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

    public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            _targetGraphView = targetGraphView
        };
    }

    #region SaveGraph

    public void SaveGraph(string fileName)
    {
        // If there are no connections don't save
        if (!Edges.Any())
        {
            EditorUtility.DisplayDialog("Save Failed", "No connections detected!", "OK");
            return;
        }

        DialogueContainer roomContainer = ScriptableObject.CreateInstance<DialogueContainer>();

        #region Get Connections

        List<NodeLinkData> nodeLinks = new List<NodeLinkData>();

        bool startNodeConnected = false;

        // Get edges that are connected to an input port
        Edge[] connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        for (int i = 0; i < connectedPorts.Count(); i++)
        {
            // Check if entry node is connected
            if (connectedPorts[i].output.node == Nodes.Find(x => x.EntryPoint))
            {
                startNodeConnected = true;
            }

            // Get both nodes the edge is connected to
            DialogueNode outputNode = (connectedPorts[i].output.node as DialogueNode);
            DialogueNode inputNode = (connectedPorts[i].input.node as DialogueNode);

            // Create connection data
            nodeLinks.Add(new NodeLinkData
            {
                BaseNodeGuid = outputNode.GUID,
                PortName = connectedPorts[i].output.portName,
                TargetNodeGuid = inputNode.GUID
            });
        }

        if (!startNodeConnected)
        {
            EditorUtility.DisplayDialog("Save Failed", "Start node is not connected!", "OK");
            return;
        }

        #endregion

        #region Save Nodes

        // Save all nodes
        foreach (DialogueNode roomNode in Nodes)
        {
            roomContainer.RoomNodeData.Add(new DialogueNodeData
            {
                Guid = roomNode.GUID,
                RoomText = roomNode.dialogueText,
                Position = roomNode.GetPosition().position,
                Connections = nodeLinks.Where(x => x.BaseNodeGuid == roomNode.GUID).ToList()
            });
        }

        #endregion

        // If there is no resources folder create one
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        AssetDatabase.DeleteAsset($"Assets/Resources/{fileName}.asset");
        AssetDatabase.CreateAsset(roomContainer, $"Assets/Resources/{fileName}.asset");
        AssetDatabase.SaveAssets();
    }

    #endregion

    #region LoadGraph

    public void LoadGraph()
    {
        string filePath = EditorUtility.OpenFilePanel("Room Graph", "Assets/Resources", "asset");

        if (!string.IsNullOrEmpty(filePath))
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);

            // Check if file exists
            _containerCache = Resources.Load<DialogueContainer>(fileName);
            if (_containerCache == null)
            {
                EditorUtility.DisplayDialog("File not found", "Target room graph file does not exists!", "OK");
                return;
            }

            ClearGraph();
            CreateNodes();
            ConnectNodes();
        }
    }

    private void ClearGraph()
    {
        // Set entry points guid back from the save. Discard existing guid
        //Nodes.Find(x => x.EntryPoint).GUID = _containerCache.NodeLinks[0].BaseNodeGuid;

        foreach (DialogueNode node in Nodes)
        {
            // Remove edges that is connected to this node
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));

            // Then remove node
            _targetGraphView.RemoveElement(node);
        }

    }

    private void CreateNodes()
    {
        foreach (DialogueNodeData nodeData in _containerCache.RoomNodeData)
        {
            DialogueNode tempNode;

            tempNode = _targetGraphView.CreateDialogueNode(nodeData.RoomText, Vector2.zero);
            

            tempNode.GUID = nodeData.Guid;
            tempNode.SetPosition(new Rect(nodeData.Position, _targetGraphView.DefaultNodeSize));
            _targetGraphView.AddElement(tempNode);
        }
    }

    private void ConnectNodes()
    {
        // Loop through nodes on graph
        for (int i = 0; i < Nodes.Count; i++)
        {
            // Get all output connections connected to this node
            List<NodeLinkData> connections = _containerCache.RoomNodeData.First(x => x.Guid == Nodes[i].GUID).Connections;
            for (int j = 0; j < connections.Count; j++)
            {
                string targetNodeGuid = connections[j].TargetNodeGuid;
                DialogueNode targetNode = Nodes.First(x => x.GUID == targetNodeGuid);

                // Link ports
                LinkNodes(Nodes[i].outputContainer[0].Q<Port>(), (Port)targetNode.inputContainer[0]);
            }
        }
    }

    private void LinkNodes(Port output, Port input)
    {
        Edge tempEdge = new Edge
        {
            output = output,
            input = input
        };

        tempEdge?.input.Connect(tempEdge);
        tempEdge?.output.Connect(tempEdge);

        _targetGraphView.Add(tempEdge);
    }

    #endregion
}
