using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;
using UnityEditor.UIElements;

public class DialogueNode : Node
{
    public struct Choice
    {
        public string text;

        public Choice(string text)
        {
            this.text = text;
        }
    }

    public string GUID;
    public string dialogueText;
    public string npcName;
    public List<Choice> choices = new List<Choice>();


    public bool isChoice;

    public DialogueNode(string GUID, string npcName, bool isChoice)
    {
        this.GUID = GUID;
        this.npcName = npcName;
        this.isChoice = isChoice;

        extensionContainer.AddToClassList("ds-node__extension-container");


        if (isChoice)
            choices.Add(new Choice("New Choice"));
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
        Button addButton = new Button();
        addButton.text = "Add Choice";

        mainContainer.Insert(1, addButton);

        foreach (Choice choice in choices)
        {
            Port port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            port.portName = "";

            Button deleteButton = new Button();
            deleteButton.text = "X";


            TextField choiceTextField = new TextField();
            choiceTextField.value = choice.text;

            choiceTextField.AddToClassList("ds-node__textfield");
            choiceTextField.AddToClassList("ds-node__choice-textfield");
            choiceTextField.AddToClassList("ds-node__textfield__hidden");

            port.Add(choiceTextField);
            port.Add(deleteButton);


            outputContainer.Add(port);
        }
    }
}
