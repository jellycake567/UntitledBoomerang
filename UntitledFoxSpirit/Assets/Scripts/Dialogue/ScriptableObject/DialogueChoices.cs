using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueChoices
{
    public string text;
    public string portGUID;
    public string targetGUID;

    public DialogueChoices(string text, string guid)
    {
        this.text = text;
        portGUID = guid;
    }
}
