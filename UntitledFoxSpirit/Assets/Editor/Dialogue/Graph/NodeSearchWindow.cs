using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private EditorWindow _window;
    private DialogueGraphView _graphView;

    private Texture2D _indentationIcon;

    public const string PLAYER = "Player";
    public const string NPC = "NPC";

    public void Configure(EditorWindow window, DialogueGraphView graphView)
    {
        _window = window;
        _graphView = graphView;

        //Transparent 1px indentation icon as a hack
        _indentationIcon = new Texture2D(1, 1);
        _indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
        _indentationIcon.Apply();
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Speakers"), 0),
            new SearchTreeEntry(new GUIContent(PLAYER, _indentationIcon))
            {
                level = 1,
                userData = PLAYER
            },
            new SearchTreeEntry(new GUIContent(NPC, _indentationIcon))
            {
                level = 1,
                userData = NPC
            },
        };

        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        //Editor window-based mouse position
        var mousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, context.screenMousePosition - _window.position.position);
        var graphMousePosition = _graphView.contentViewContainer.WorldToLocal(mousePosition);
        switch (SearchTreeEntry.userData)
        {
            case PLAYER:
                _graphView.CreateNode(PLAYER, graphMousePosition);
                return true;
            case NPC:
                _graphView.CreateNode(NPC, graphMousePosition);
                return true;
        }
        return false;
    }
}
