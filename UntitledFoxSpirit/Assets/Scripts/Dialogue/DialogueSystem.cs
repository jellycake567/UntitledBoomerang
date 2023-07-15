using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Transactions;
using UnityEditor;
using System.Runtime.InteropServices.ComTypes;
using NUnit.Framework.Interfaces;
using UnityEditor.MemoryProfiler;
using System;

public class DialogueSystem : MonoBehaviour
{ 
    public DialogueContainer dialogue;
    public TextMeshPro dialogueText;
    public TextMeshPro npcName;

    DialogueNodeData startNode;

    List<DialogueNodeData> nextNodes = new List<DialogueNodeData>();

    List<DialogueNodeData> nodeList;

    Dictionary<DialogueChoices, string> choiceList = new Dictionary<DialogueChoices, string>();


    void LoadDialogue()
    {
        nodeList = dialogue.DialogueNodeData;
        startNode = nodeList.First(x => x.isStartNode == true);

        // Grab all nodes that have choices
        List<DialogueNodeData> choiceNode = nodeList.Where(x => x.Choices.Count > 0).ToList();




    }


    void LoadNextNodes(DialogueNodeData currentNode)
    {
        
        foreach (NodeLinkData connnection in currentNode.Connections)
        {
            // Loop through nodeList to find node with correct guid (TargetNodeGuid)
            nextNodes.Add(nodeList.First(x => x.Guid == connnection.TargetNodeGuid));
        }
       
        if (currentNode.Choices.Count > 0)
        {

        }
    }


    void LoadNode(DialogueNodeData node)
    {
        npcName.text = node.Npc;
        dialogueText.text = node.DialogueText;
    }

}
