using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNodeData
{
    public string Guid;
    public string Npc;
    public string DialogueText;
    public Vector2 Position;
    public bool isStartNode;
    public List<NodeLinkData> Connections;
    public List<DialogueChoices> Choices = new List<DialogueChoices>();
}
