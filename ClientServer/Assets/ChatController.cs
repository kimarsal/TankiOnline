using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

public class ChatController : MonoBehaviour 
{
    public TMP_InputField ChatInputField;

    public TMP_Text ChatDisplayOutput;

    public Scrollbar ChatScrollbar;

    private ServerHandler sh = null;
    private ClientHandler ch = null;

    private void Awake()
    {
        var g = GameObject.FindWithTag("Handler");
        sh = g.GetComponent<ServerHandler>();
        ch = g.GetComponent<ClientHandler>();
    }

    void OnEnable()
    {
        ChatInputField.onSubmit.AddListener(SendChat);
    }

    void OnDisable()
    {
        ChatInputField.onSubmit.RemoveListener(SendChat);
    }

    private void SendChat(string text)
    {
        AddChatToChatOutput(text);
        if (sh != null)
            sh.SendToAll(text);
        else if (ch != null)
            ch.SendToServer(text);
    }
    
    public void AddStatusToChatOutput(string newText)
    {
        AddToChatOutput("STATUS: " + newText);
    }


    public void AddChatToChatOutput(string newText)
    {
        AddToChatOutput("CHAT: " + newText);
    }

    public void AddToChatOutput(string newText)
    {
        // Clear Input Field
        ChatInputField.text = string.Empty;

        var timeNow = System.DateTime.Now;

        string formattedInput = "[<#FFFF80>" + timeNow.Hour.ToString("d2") + ":" + timeNow.Minute.ToString("d2") + ":" + timeNow.Second.ToString("d2") + "</color>] " + newText;

        if (ChatDisplayOutput != null)
        {
            // No special formatting for first entry
            // Add line feed before each subsequent entries
            if (ChatDisplayOutput.text == string.Empty)
                ChatDisplayOutput.text = formattedInput;
            else
                ChatDisplayOutput.text += "\n" + formattedInput;
        }

        // Keep Chat input field active
        ChatInputField.ActivateInputField();

        // Set the scrollbar to the bottom when next text is submitted.
        ChatScrollbar.value = 0;
    }

}
