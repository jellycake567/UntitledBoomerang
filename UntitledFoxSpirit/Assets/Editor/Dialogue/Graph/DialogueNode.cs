using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

public class DialogueNode : Node
{
    public string GUID;
    public string dialogueText;
    public bool EntryPoint;

    public DialogueNode(string GUID, string DialogueText, bool EntryPoint = false)
    {
        this.GUID = GUID;
        this.dialogueText = DialogueText;
        this.EntryPoint = EntryPoint;

        title = DialogueText;
    }
}
