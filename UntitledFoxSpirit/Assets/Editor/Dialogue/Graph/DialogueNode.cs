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
    public struct Choices
    {
        public string text;

        public Choices(string text)
        {
            this.text = text;
        }
    }
    

    public string GUID;
    public string dialogueText;
    public string npcName;
    public List<Choices> choices = new List<Choices>();

    public bool isChoice;

    DialogueGraphView graphView;

    public DialogueNode(string GUID, string npcName, bool isChoice, DialogueGraphView graphView)
    {
        this.GUID = GUID;
        this.npcName = npcName;
        this.isChoice = isChoice;
        this.graphView = graphView;

        mainContainer.AddToClassList("ds-node__main-container");
        extensionContainer.AddToClassList("ds-node__extension-container");


        if (isChoice)
            choices.Add(new Choices("New Choice"));
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
            Choices choice = new Choices("New Choice");

            Port port = CreateChoicePort(choice);

            choices.Add(choice);

            outputContainer.Add(port);
        });

        addButton.AddToClassList("ds-node__button");

        mainContainer.Insert(1, addButton);

        foreach (Choices choice in choices)
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

    Port CreateChoicePort(Choices choice)
    {
        Port port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        port.portName = "";

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

        TextField choiceTextField = new TextField();
        choiceTextField.value = choice.text;

        choiceTextField.AddToClassList("ds-node__textfield");
        choiceTextField.AddToClassList("ds-node__choice-textfield");
        choiceTextField.AddToClassList("ds-node__textfield__hidden");

        port.Add(choiceTextField);
        port.Add(deleteButton);
        return port;
    }
}
