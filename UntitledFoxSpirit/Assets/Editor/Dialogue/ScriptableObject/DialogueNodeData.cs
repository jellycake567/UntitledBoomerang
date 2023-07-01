﻿using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNodeData
{

    public string Guid;
    public string DialogueText;
    public Vector2 Position;
    public List<NodeLinkData> Connections;
    public List<DialogueChoices> choices = new List<DialogueChoices>();
    public bool isChoice;
    public bool isStartNode;
}
