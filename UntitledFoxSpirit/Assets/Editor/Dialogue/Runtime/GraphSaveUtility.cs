using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System;

public class GraphSaveUtility
{
    private DialogueGraphView _targetGraphView;
    private DialogueContainer _containerCache;

    private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<DialogueNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

    private DialogueNodeData startNode;

    List<Port> Ports => _targetGraphView.ports.ToList();

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

        #region Check for starting node

        List<Port> inputPorts = Ports.Where(x => x.name == "input").ToList();
        List<Port> portsNotConnected = inputPorts.Where(x => x.connected == false).ToList();

        

        if (portsNotConnected.Count > 1)
        {
            EditorUtility.DisplayDialog("Save Failed", "Only one node should have no input connection", "OK");
            return;
        }

        DialogueNode node = portsNotConnected[0].node as DialogueNode;

        startNode = new DialogueNodeData
        {
            Guid = node.GUID,
        };

        #endregion


        DialogueContainer dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();

        #region Get Connections

        List<NodeLinkData> nodeLinks = new List<NodeLinkData>();

        // Get edges that are connected to an input port
        Edge[] connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        for (int i = 0; i < connectedPorts.Count(); i++)
        {
            // Get both nodes the edge is connected to
            DialogueNode outputNode = (connectedPorts[i].output.node as DialogueNode);
            DialogueNode inputNode = (connectedPorts[i].input.node as DialogueNode);
                
            // Create connection data
            nodeLinks.Add(new NodeLinkData
            {
                BaseNodeGuid = outputNode.GUID,
                PortName = connectedPorts[i].output.portName,
                TargetNodeGuid = inputNode.GUID,
                ChoiceGUID = connectedPorts[i].output.name,
            });;
        }


        #endregion

        #region Save Nodes

        // Save all nodes
        foreach (DialogueNode dialogueNode in Nodes)
        {
            DialogueNodeData newNode = new DialogueNodeData
            {
                Guid = dialogueNode.GUID,
                DialogueText = dialogueNode.dialogueText,
                Npc = dialogueNode.npcName,
                Position = dialogueNode.GetPosition().position,
                Connections = nodeLinks.Where(x => x.BaseNodeGuid == dialogueNode.GUID).ToList(),
                Choices = dialogueNode.choices,
            };

            // Starting node
            if (dialogueNode.GUID == startNode.Guid)
            {
                newNode.isStartNode = true;
            }

            dialogueContainer.DialogueNodeData.Add(newNode);
        }

        #endregion

        // If there is no resources folder create one
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        AssetDatabase.DeleteAsset($"Assets/Resources/{fileName}.asset");
        AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
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
        foreach (DialogueNodeData nodeData in _containerCache.DialogueNodeData)
        {
            DialogueNode tempNode = new DialogueNode(Guid.NewGuid().ToString(), nodeData.DialogueText, nodeData.isChoice, _targetGraphView);
            tempNode.Draw(nodeData.Position, _targetGraphView.DefaultNodeSize);
            tempNode.GUID = nodeData.Guid;

            _targetGraphView.AddElement(tempNode);
        }
    }

    private void ConnectNodes()
    {
        // Loop through nodes on graph
        for (int i = 0; i < Nodes.Count; i++)
        {
            // Get all output connections connected to this node
            List<NodeLinkData> connections = _containerCache.DialogueNodeData.First(x => x.Guid == Nodes[i].GUID).Connections;
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
