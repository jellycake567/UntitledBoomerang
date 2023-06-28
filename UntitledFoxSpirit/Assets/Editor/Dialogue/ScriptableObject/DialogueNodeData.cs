using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNodeData
{
    public string Guid;
    public string RoomText;
    public Vector2 Position;
    public List<NodeLinkData> Connections;
}
