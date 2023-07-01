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

public class DialogueNode : Node
{

    public string GUID;
    public string dialogueText;
    public string npcName;
    public List<String> choices = new List<String>();


    public bool isChoice;

    public DialogueNode(string GUID, string npcName, bool isChoice)
    {
        this.GUID = GUID;
        this.npcName = npcName;
        this.isChoice = isChoice;

        mainContainer.AddToClassList("ds-node__main-container");
        extensionContainer.AddToClassList("ds-node__extension-container");


        if (isChoice)
            choices.Add("New Choice");
    }

    public void Draw(Vector2 position, Vector2 size)
    {
        CreateNPCTextField();

        CreatePorts();

        CreateDialogueTextField();

        if (isChoice)
            CreateChoices();

        // Refresh node
        RefreshExpandedState();
        RefreshPorts();

        SetPosition(new Rect(position, size));
    }

    private void CreateNPCTextField()
    {
        TextField dialogueNameTextField = new TextField();
        dialogueNameTextField.value = npcName;

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
        inputContainer.Add(inputPort);

        if (isChoice)
            return;

        Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        outputPort.portName = "Output";
        outputContainer.Add(outputPort);
    }

    private void CreateDialogueTextField()
    {
        VisualElement customDataContainer = new VisualElement();

        customDataContainer.AddToClassList("ds-node__custom-data-container");

        Foldout textFolout = new Foldout();
        textFolout.text = "Dialogue Text";

        TextField textField = new TextField();
        textField.value = dialogueText;

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
            Port port = CreateChoicePort("New Choice");

            choices.Add("New Choice");

            outputContainer.Add(port);
        });
        
        mainContainer.Insert(1, addButton);

        foreach (string choice in choices)
        {
            Port port = CreateChoicePort(choice);

            outputContainer.Add(port);
        }
    }


    Button CreateButton(string text, Action onClick = null)
    {
        Button button = new Button(onClick);
        button.text = text;

        return button;
    }

    Port CreateChoicePort(string choiceText)
    {
        Port port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        port.portName = "";

        Button deleteButton = new Button();
        deleteButton.text = "X";

        TextField choiceTextField = new TextField();
        choiceTextField.value = choiceText;

        choiceTextField.AddToClassList("ds-node__textfield");
        choiceTextField.AddToClassList("ds-node__choice-textfield");
        choiceTextField.AddToClassList("ds-node__textfield__hidden");

        port.Add(choiceTextField);
        port.Add(deleteButton);
        return port;
    }
}
