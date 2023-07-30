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
using System.Reflection;

public class DialogueSystem : MonoBehaviour
{
    public PlayerInput playerInputScript;

    public DialogueContainer dialogue;
    public TextMeshProUGUI npcName;
    public TextMeshProUGUI dialogueText;

    public GameObject parentChoiceObject;
    public GameObject choicePrefab;

    DialogueNodeData currentNode;

    List<DialogueNodeData> nextNodes = new List<DialogueNodeData>();

    List<DialogueChoices> currentChoices = new List<DialogueChoices>();

    List<DialogueNodeData> nodeList;

    Dictionary<DialogueChoices, string> choiceList = new Dictionary<DialogueChoices, string>();

    bool isChoice = false;

    void Start()
    {
        LoadDialogue(); // should be called when starting dialogue

        playerInputScript.GetComponent<PlayerInput>().enabled = false;
    }

    void Update()
    {
        if (!isChoice && Input.GetKeyDown(KeyCode.Mouse0))
        {
            NextDialogue();
        }
    }

    void LoadDialogue()
    {
        nodeList = dialogue.DialogueNodeData;
        currentNode = nodeList.First(x => x.isStartNode == true);

        LoadNodeUI(currentNode);
    }

    void ClearChoices()
    {
        for (int i = 0; i < parentChoiceObject.transform.childCount; i++)
        {
            GameObject child = parentChoiceObject.transform.GetChild(i).gameObject;

            Destroy(child);
        }
    }

    void LoadNodeUI(DialogueNodeData currentNode)
    {
        isChoice = false;

        ClearChoices();

        npcName.text = currentNode.Npc;
        dialogueText.text = currentNode.DialogueText;

        Transform newChoiceUI = parentChoiceObject.transform;
        int index = 0;

        foreach (DialogueChoices choice in currentNode.Choices)
        {
            isChoice = true;

            // Create choice ui
            newChoiceUI = Instantiate(choicePrefab, newChoiceUI.transform).transform;
            newChoiceUI.parent = parentChoiceObject.transform;

            newChoiceUI.position = newChoiceUI.position - new Vector3(0, 100f, 0);

            newChoiceUI.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;

            newChoiceUI.GetComponent<Button>().onClick.AddListener(delegate { NextDialogue(choice); });

            index++;
            currentChoices.Add(choice);
        }
    }

    void NextDialogue(DialogueChoices index = null)
    {
        DialogueNodeData nextNode;

        if (isChoice)
        {
            // Get next node through choice picked
            DialogueChoices nextChoice = currentChoices.First(x => x == index);
            nextNode = nodeList.First(x => x.Guid == nextChoice.targetGUID);
        }
        else
        {
            // Get next node through connection
            string targetNodeGuid = currentNode.Connections[0].TargetNodeGuid;
            nextNode = nodeList.First(x => x.Guid == targetNodeGuid);
        }

        
        currentNode = nextNode;
        LoadNodeUI(nextNode);
    }


    
}
