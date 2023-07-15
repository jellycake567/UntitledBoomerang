using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;
using UnityEditor.UIElements;
using System;
using static DialogueNode;
using System.Reflection.Emit;

public class DialogueNode : Node
{
    public string GUID;
    public string dialogueText;
    public string npcName;
    public List<DialogueChoices> choices = new List<DialogueChoices>();

    DialogueGraphView graphView;

    public DialogueNode(string GUID, string npcName, DialogueGraphView graphView)
    {
        this.GUID = GUID;
        this.npcName = npcName;
        this.graphView = graphView;

        mainContainer.AddToClassList("ds-node__main-container");
        extensionContainer.AddToClassList("ds-node__extension-container");
    }

    public DialogueNode(string GUID, string npcName, DialogueGraphView graphView, bool isChoice)
    {
        this.GUID = GUID;
        this.npcName = npcName;
        this.graphView = graphView;

        mainContainer.AddToClassList("ds-node__main-container");
        extensionContainer.AddToClassList("ds-node__extension-container");

        choices.Add(new DialogueChoices("New Choice", Guid.NewGuid().ToString()));
    }

    public void Draw(Vector2 position, Vector2 size)
    {
        CreateNPCTextField();

        CreatePorts();

        CreateDialogueTextField();

        if (choices.Count > 0)
            CreateChoices();

        // Refresh node
        RefreshExpandedState();
        RefreshPorts();

        SetPosition(new Rect(position, size));
    }

    private void CreateNPCTextField()
    {
        TextField dialogueNameTextField = CreateTextField(npcName, callback =>
        {
            npcName = callback.newValue;
        });
        
        dialogueNameTextField.AddToClassList("ds-node__textfield");
        dialogueNameTextField.AddToClassList("ds-node__filename-textfield");
        dialogueNameTextField.AddToClassList("ds-node__textfield__hidden");

        titleContainer.Insert(0, dialogueNameTextField);
    }

    private void CreatePorts()
    {
        // Add ports
        Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        inputPort.portName = "Input";
        inputPort.name = "input";
        inputContainer.Add(inputPort);

        if (choices.Count > 0)
            return;

        Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        outputPort.portName = "Output";
        outputPort.name = "output";
        outputContainer.Add(outputPort);
    }

    private void CreateDialogueTextField()
    {
        VisualElement customDataContainer = new VisualElement();

        customDataContainer.AddToClassList("ds-node__custom-data-container");

        Foldout textFolout = new Foldout();
        textFolout.text = "Dialogue Text";

        TextField textField = CreateTextField(dialogueText, callback =>
        {
            dialogueText = callback.newValue;
        });

        textField.AddToClassList("ds-node__textfield");
        textField.AddToClassList("ds-node__quote-textfield");


        textFolout.Add(textField);
        customDataContainer.Add(textFolout);
        extensionContainer.Add(customDataContainer);
    }

    private void CreateChoices()
    {
        Button addButton = CreateButton("Add Choice", () =>
        {
            string newGUID = Guid.NewGuid().ToString();

            DialogueChoices choice = new DialogueChoices("New Choice", newGUID);

            Port port = CreateChoicePort(choice, newGUID);

            choices.Add(choice);

            outputContainer.Add(port);
        });

        addButton.AddToClassList("ds-node__button");

        mainContainer.Insert(1, addButton);

        foreach (DialogueChoices choice in choices)
        {
            Port port = CreateChoicePort(choice, choice.guid);

            outputContainer.Add(port);
        }
    }


    Button CreateButton(string text, Action onClick = null)
    {
        Button button = new Button(onClick);
        button.text = text;

        return button;
    }

    Port CreateChoicePort(DialogueChoices choice, string guid)
    {
        Port port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        port.portName = "";
        port.name = guid;

        Button deleteButton = CreateButton("X", () =>
        {
            if (choices.Count == 1)
                return;

            if (port.connected)
                graphView.DeleteElements(port.connections);

            choices.Remove(choice);


            graphView.RemoveElement(port);
        });

        deleteButton.AddToClassList("ds-node__button");

        TextField choiceTextField = CreateTextField(choice.text, callback =>
        {
            choice.text = callback.newValue;
        });

        choiceTextField.AddToClassList("ds-node__textfield");
        choiceTextField.AddToClassList("ds-node__choice-textfield");
        choiceTextField.AddToClassList("ds-node__textfield__hidden");

        port.Add(choiceTextField);
        port.Add(deleteButton);
        return port;
    }

    TextField CreateTextField(string text, EventCallback<ChangeEvent<string>> onValueChanged = null)
    {
        TextField textField = new TextField();
        textField.value = text;

        if (onValueChanged != null)
        {
            textField.RegisterValueChangedCallback(onValueChanged);
        }

        return textField;
    }
}
