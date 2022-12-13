using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerHandler : MonoBehaviour
{
    public bool test = false;
    public ServerScript serverScript;
    private NetworkHelper networkHelper;

    private ChatController chatController;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public bool StartServer(int localPort)
    {
        networkHelper = FindObjectOfType<NetworkHelper>();
        networkHelper.onHostAdded.AddListener(ServerStarted);
        networkHelper.onHostRemoved.AddListener(ServerStopped);
        networkHelper.onConnectClient.AddListener(ClientConnected);
        networkHelper.onDisconnectClient.AddListener(ClientDisconnected);
        networkHelper.onMessageReceivedFrom.AddListener(ReceiveMessage);
        return networkHelper.MakeServer(localPort);
    }

    private List<int> ConnectedClients => networkHelper.connectionIds;

    private void ServerStarted()
    {
        if (!test) SceneManager.LoadScene("Server");
    }

    private void ServerStopped()
    {
    }

    private void ClientConnected(int id)
    {
        SendToClient(id, serverScript.GetInfo());
    }

    private void ClientDisconnected(int arg0)
    {
    }

    private void WriteOnChat(string message)
    {
        if(chatController == null)
        {
            chatController = GameObject.FindWithTag("Chat").GetComponent<ChatController>();
        }
        chatController.AddChatToChatOutput(message);
    }

    private void ReceiveMessage(string message, int from)
    {
        // Do things here
        string op = message.Substring(0, 3);
        switch(op){
            case "CTE": //ex: CTE1
                if (serverScript.ChooseTeam(message, from))
                {
                    SendToAllExcept(message, from);
                    SendToClient(from, "YOK");
                    WriteOnChat("Player " + from + " joined team " + message.Substring(3, 4));
                }
                else
                {
                    SendToClient(from, "NOK");
                }
                break;
            case "CTA": //ex: CTA1
                if (serverScript.ChooseTank(message, from))
                {
                    SendToAllExcept(message, from);
                    SendToClient(from, "YOK");
                    WriteOnChat("Player " + from + " chose tank " + message.Substring(3, 4));
                }
                else
                {
                    SendToClient(from, "NOK");
                }
                break;
            case "POS": case "ROB": case "ROT":
                SendToAllExcept(message, from);
                break;

        }
        
        // Example: Print message on chat
        //GameObject.FindWithTag("Chat").GetComponent<ChatController>().AddChatToChatOutput(from + " -> " + message);

        // Example: relay all messages
        //SendToAllExcept(from + " -> " + message, from);

    }

    public void SendToClient(int id, string message)
    {
        networkHelper.SendToOne(id, message);
    }

    public void SendToAllExcept(string message, int exceptId)
    {
        networkHelper.SendToAllExcept(message, exceptId);
    }

    public void SendToAll(string message)
    {
        networkHelper.SendToAll(message);

    }

}
