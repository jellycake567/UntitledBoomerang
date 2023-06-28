using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueEditor : EditorWindow
{
    private DialogueGraphView _graphView;
    private string _fileName = "New Dialogue";

    [MenuItem("Graph/Room Graph")]
    public static void ShowWindow()
    {
        DialogueEditor window = GetWindow<DialogueEditor>("Graph");
        window.titleContent = new GUIContent("Dialogue System");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolBar();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }

    private void ConstructGraphView()
    {
        _graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph"
        };

        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolBar()
    {
        Toolbar toolbar = new Toolbar();

        TextField fileNameTextField = new TextField("File Name");
        fileNameTextField.SetValueWithoutNotify(_fileName);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
        toolbar.Add(fileNameTextField);

        toolbar.Add(new Button(() => RequestDataOperation(true)) { text = "Save Data" });
        toolbar.Add(new Button(() => RequestDataOperation(false)) { text = "Load Data" });

        rootVisualElement.Add(toolbar);
    }

    private void RequestDataOperation(bool save)
    {
        GraphSaveUtility saveUtility = GraphSaveUtility.GetInstance(_graphView);
        if (save)
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
                return;
            }

            saveUtility.SaveGraph(_fileName);
        }
        else
        {
            saveUtility.LoadGraph();
        }
    }
}
