using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientHandler : MonoBehaviour
{
    public ClientScript clientScript;
    public bool test = false;
    private NetworkHelper networkHelper;

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public bool StartClient(int localPort, string remoteIP, int remotePort)
    {
        networkHelper = FindObjectOfType<NetworkHelper>();
        networkHelper.onConnect.AddListener(ConnectedToServer);
        networkHelper.onDisconnect.AddListener(DisconnectedFromServer);
        networkHelper.onMessageReceived.AddListener(ReceiveMessage);
        return networkHelper.ConnectToServer(localPort, remoteIP, remotePort);
    }

    private void ConnectedToServer()
    {
        if (!test) SceneManager.LoadScene("Client");
    }

    private void DisconnectedFromServer()
    {
    }

    private void ReceiveMessage(string message)
    {
        // Do things here
        string op = message.Substring(0, 3);
        switch (op)
        {
            case "INF":
                clientScript.ReceiveInfo(message);
                break;
            case "CTE": //ex: CTE11
                int team = int.Parse(message.Substring(3, 1));
                clientScript.TeamIsChosen(team);
                break;
            case "CTA": //ex: CTA11
                int player = int.Parse(message.Substring(3, 1));
                int tank = int.Parse(message.Substring(4, 1));
                clientScript.TankIsChosen(player, tank);
                break;
            case "YOK":
                clientScript.OkOrNot(true);
                break;
            case "NOK":
                clientScript.OkOrNot(false);
                break;
            case "POS":
                clientScript.MovePlayer(message);
                break;
            case "ROB":
                clientScript.RotatePlayerBase(message);
                break;
            case "ROT":
                clientScript.RotatePlayerTurret(message);
                break;
        }

        // Example: Print message on chat
        //GameObject.FindWithTag("Chat").GetComponent<ChatController>().AddChatToChatOutput(message);
    }

    public void SendToServer(string message)
    {
        networkHelper.SendToServer(message);
    }

}
