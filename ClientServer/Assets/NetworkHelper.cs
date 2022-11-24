using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class NetworkHelper : MonoBehaviour
{
    #region UI Handlers

    private bool isServer = false;

    public bool MakeServer(int localPort)
    {
        isServer = true;
        return StartHost(localPort);
    }

    public bool ConnectToServer(int localPort, string remoteIP, int remotePort)
    {
        isServer = false;
        return StartClient(localPort, remoteIP, remotePort);
    }

    public void SendToAll(string text)
    {
        SendToAllExcept(text, -1);
    }

    public void SendToServer(string message)
    {
        SendToAll(message);
    }

    public void SendToAllExcept(string text, int except)
    {
        foreach (var conn in connectionIds)
        {
            if (conn != except)
            {
                SendToOne(conn, text);
            }
        }
    }

    [Serializable]
    public class OnMessageReceivedEvent : UnityEvent<string>
    {
    }

    public OnMessageReceivedEvent onMessageReceived;

    [Serializable]
    public class OnMessageReceivedFromEvent : UnityEvent<string, int>
    {
    }

    public OnMessageReceivedFromEvent onMessageReceivedFrom;

    [Serializable]
    public class OnStatusEvent : UnityEvent<string>
    {
    }

    public OnStatusEvent onStatusEvent;

    [Serializable]
    public class OnHostAddedEvent : UnityEvent
    {
    }

    public OnHostAddedEvent onHostAdded;

    [Serializable]
    public class OnHostRemovedEvent : UnityEvent
    {
    }

    public OnHostRemovedEvent onHostRemoved;

    [Serializable]
    public class OnConnectEvent : UnityEvent
    {
    }

    public OnConnectEvent onConnect;

    [Serializable]
    public class OnConnectClientEvent : UnityEvent<int>
    {
    }

    public OnConnectClientEvent onConnectClient;

    [Serializable]
    public class OnDisconnectEvent : UnityEvent
    {
    }

    public OnDisconnectEvent onDisconnect;

    [Serializable]
    public class OnDisconnectClientEvent : UnityEvent<int>
    {
    }

    public OnDisconnectClientEvent onDisconnectClient;

    public void Log(string message)
    {
        Debug.Log(message);
        onStatusEvent.Invoke(message);
    }

    public void LogError(string message)
    {
        Debug.LogError(message);
        onStatusEvent.Invoke("ERROR: " + message);
    }

    public List<int> connectionIds = new List<int>();

    private void OnConnected(int outHostId, int outConnectionId, NetworkError error)
    {
        if (error == NetworkError.Ok)
        {
            if (!isServer && connectionIds.Count > 0)
            {
                LogError("Attempted to connect more than once on a client!");
                NetworkTransport.Disconnect(outHostId, outConnectionId, out byte bError);
            }

            connectionIds.Add(outConnectionId);
            Log("Connected to " + outHostId + " with " + outConnectionId);
            onConnect.Invoke();
            onConnectClient.Invoke(outConnectionId);
        }
        else
        {
            LogError("Error connecting to " + outHostId);
        }
    }

    private void OnDisconnected(int outHostId, int outConnectionId, NetworkError error)
    {
        connectionIds.Remove(outConnectionId);
        Log("Disconnected to " + outHostId + " with " + outConnectionId);
        onDisconnect.Invoke();
        onDisconnectClient.Invoke(outConnectionId);
    }

    private void OnData(int outHostId, int outConnectionId, int outChannelId, byte[] buffer, int receivedSize, NetworkError error)
    {
        var message = new MemoryStream(buffer, 0, receivedSize);
        var formatter = new BinaryFormatter();
        var text = (string)formatter.Deserialize(message);
        Log("Received: " + text);
        onMessageReceived.Invoke(text);
        onMessageReceivedFrom.Invoke(text, outConnectionId);
    }

    #endregion

    #region Implementation

    private int hostId;
    private int channelId;
    private int connectionId;

    // Start is called before the first frame update
    void Start()
    {
        GlobalConfig gConfig = new GlobalConfig();
        gConfig.MaxPacketSize = 50000;
        NetworkTransport.Init(gConfig);
        hostId = -1;
        DontDestroyOnLoad(this);
    }

    private bool StartHost(int localPort)
    {
        if (hostId >= 0)
        {
            LogError("Host already started.");
            return false;
        }
        else
        {
            if (localPort <= 0 || localPort >= 65535)
            {
                LogError("Invalid local port " + localPort + ". Cannot continue.");
                return false;
            }

            ConnectionConfig config = new ConnectionConfig();
            config.PacketSize = 5000;
            channelId = config.AddChannel(QosType.Reliable);

            HostTopology topology = new HostTopology(config, 10);

            hostId = NetworkTransport.AddHost(topology, localPort);

            if (hostId < 0)
            {
                LogError("Could not create host. Port already used?");
                return false;
            }
            else
            {
                Log("Host initiated on port " + localPort + " with id " + hostId);
            }
        }

        onHostAdded.Invoke();

        return true;
    }

    private void StopHost()
    {
        if (hostId >= 0)
        {
            NetworkTransport.RemoveHost(hostId);
            onHostRemoved.Invoke();
        }
    }

    private bool StartClient(int localPort, string remoteIP, int remotePort)
    {
        if (!StartHost(localPort)) return false;

        if (hostId < 0) return false;

        if (remotePort <= 0 || remotePort >= 65535)
        {
            LogError("Invalid remote port " + remotePort + ". Cannot continue.");
            return false;
        }

        if (string.IsNullOrEmpty(remoteIP))
        {
            LogError("Invalid remote IP " + remoteIP + ". Cannot continue.");
            return false;
        }

        Log("Connecting to " + remoteIP + ":" + remotePort);

        connectionId = NetworkTransport.Connect(hostId, remoteIP, remotePort, 0, out var error);

        //If there is an error, output message error to the console
        if ((NetworkError)error != NetworkError.Ok)
        {
            LogError("Connect error: " + (NetworkError)error);
            return false;
        }

        return true;
    }

    public void SendToOne(int conn, string textInput)
    {
        byte error;
        byte[] buffer = new byte[1024];
        Stream message = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        //Serialize the message
        formatter.Serialize(message, textInput);

        //Send the message from the "client" with the serialized message and the connection information
        NetworkTransport.Send(hostId, conn, channelId, buffer, (int)message.Position, out error);

        //If there is an error, output message error to the console
        if ((NetworkError)error != NetworkError.Ok)
        {
            LogError("Message send error: " + (NetworkError)error);
        }
        else
        {
            Log("Sent " + textInput + " to " + conn);
        }
    }

    private void Update()
    {
        int outHostId;
        int outConnectionId;
        int outChannelId;
        byte[] buffer = new byte[1024];
        int receivedSize;
        byte error;

        var eventType = NetworkTransport.Receive(out outHostId, out outConnectionId, out outChannelId, buffer, buffer.Length, out receivedSize, out error);

        switch (eventType)
        {
            case NetworkEventType.ConnectEvent:
            {
                connectionId = outConnectionId;
                channelId = outChannelId;
                OnConnected(outHostId, outConnectionId, (NetworkError)error);
                break;
            }

            case NetworkEventType.DisconnectEvent:
            {
                OnDisconnected(outHostId, outConnectionId, (NetworkError)error);
                break;
            }

            case NetworkEventType.DataEvent:
            {
                OnData(outHostId, outConnectionId, outChannelId, buffer, receivedSize, (NetworkError)error);
                break;
            }

            case NetworkEventType.BroadcastEvent:
            {
                OnData(outHostId, outConnectionId, outChannelId, buffer, receivedSize, (NetworkError)error);
                break;
            }

            case NetworkEventType.Nothing:
                break;

            default:
                //Output the error
                LogError("Unknown network message type received: " + eventType);
                break;
        }
    }
    #endregion
}
