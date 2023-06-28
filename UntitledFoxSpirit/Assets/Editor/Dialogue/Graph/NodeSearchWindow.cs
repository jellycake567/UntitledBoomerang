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

    public const string BASIC_ROOM = "Basic Room";
    public const string CHALLENGE_ROOM = "Challenge Room";
    public const string BOSS_ROOM = "Boss Room";
    public const string TELEPORTER_ROOM = "Teleporter Room";

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
            new SearchTreeGroupEntry(new GUIContent("Rooms"), 0),
            new SearchTreeEntry(new GUIContent(BASIC_ROOM, _indentationIcon))
            {
                level = 1,
                userData = BASIC_ROOM
            },
            new SearchTreeEntry(new GUIContent(CHALLENGE_ROOM, _indentationIcon))
            {
                level = 1,
                userData = CHALLENGE_ROOM
            },
            new SearchTreeEntry(new GUIContent(BOSS_ROOM, _indentationIcon))
            {
                level = 1,
                userData = BOSS_ROOM
            },
            new SearchTreeEntry(new GUIContent(TELEPORTER_ROOM, _indentationIcon))
            {
                level = 1,
                userData = TELEPORTER_ROOM
            }
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
            case BASIC_ROOM:
                _graphView.CreateNode(BASIC_ROOM, graphMousePosition);
                return true;
            case CHALLENGE_ROOM:
                _graphView.CreateNode(CHALLENGE_ROOM, graphMousePosition);
                return true;
            case BOSS_ROOM:
                _graphView.CreateNode(BOSS_ROOM, graphMousePosition);
                return true;
            case TELEPORTER_ROOM:
                _graphView.CreateNode(TELEPORTER_ROOM, graphMousePosition);
                return true;
        }
        return false;
    }
}
