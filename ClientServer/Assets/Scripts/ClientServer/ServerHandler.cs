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

    private void Start()
    {
        DontDestroyOnLoad(this);
        chatController = GameObject.FindWithTag("Chat").GetComponent<ChatController>();
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
        serverScript.SendInfo(id);
    }

    private void ClientDisconnected(int arg0)
    {
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
                    chatController.AddChatToChatOutput("Player " + from + " joined team " + message.Substring(3, 4));
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
                    chatController.AddChatToChatOutput("Player " + from + " chose tank " + message.Substring(3, 4));
                }
                else
                {
                    SendToClient(from, "NOK");
                }
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
