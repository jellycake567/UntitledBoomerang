using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueChoices
{
    public string text;
    public string guid;

    public DialogueChoices(string text, string guid)
    {
        this.text = text;
        this.guid = guid;
    }
}
