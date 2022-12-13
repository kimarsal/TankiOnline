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
        int team, tank;
        switch (op)
        {
            case "INF": //ex: INF12110000
                int playersOnTeam1 = 0, playersOnTeam2 = 0;
                bool[] takenTanks = new bool[4];
                for(int i = 0; i < 4; i++)
                {
                    team = int.Parse(message.Substring(3 + i * 2, 1));
                    tank = int.Parse(message.Substring(4 + i * 2, 1));
                    if (team != 0)
                    {
                        if (team == 1) playersOnTeam1++;
                        else playersOnTeam2++;

                        if (tank != 0)
                        {
                            takenTanks[tank - 1] = true;
                        }
                    }
                }
                clientScript.SetTeams(playersOnTeam1, playersOnTeam2, takenTanks);
                break;
            case "CTE": //ex: CTE1
                team = int.Parse(message.Substring(3, 1));
                clientScript.TeamIsChosen(team);
                break;
            case "CTA": //ex: CTA1
                tank = int.Parse(message.Substring(3, 1));
                clientScript.TankIsChosen(tank);
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
