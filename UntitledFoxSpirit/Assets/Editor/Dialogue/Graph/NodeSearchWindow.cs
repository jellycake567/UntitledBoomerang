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

    public const string DIALOGUE = "Dialogue";
    public const string DIALOGUE_CHOICE = "Dialogue Choice";

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
            new SearchTreeGroupEntry(new GUIContent("Nodes"), 0),
            new SearchTreeEntry(new GUIContent(DIALOGUE, _indentationIcon))
            {
                level = 1,
                userData = DIALOGUE
            },
            new SearchTreeEntry(new GUIContent(DIALOGUE_CHOICE, _indentationIcon))
            {
                level = 1,
                userData = DIALOGUE_CHOICE
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
            case DIALOGUE:
                _graphView.CreateNode("Name", graphMousePosition, false);
                return true;
            case DIALOGUE_CHOICE:
                _graphView.CreateNode("Name", graphMousePosition, true);
                return true;
        }
        return false;
    }
}
