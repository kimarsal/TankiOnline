using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerScript : MonoBehaviour
{
    private ServerHandler serverHandler;

    public List<PlayerScript> players = new List<PlayerScript>();
    public Dictionary<int, PlayerInput> playerInputs = new Dictionary<int, PlayerInput>();
    public int playersOnTeam1 = 0;
    public int playersOnTeam2 = 0;

    public Transform[] team1Spawns;
    public Transform[] team2Spawns;
    public GameObject[] tankPrefabs;
    private bool isTeam1Spawn1Taken;
    private bool isTeam2Spawn1Taken;
    private Dictionary<int, Dictionary<string, bool>> playerKeys;

    private void Start(){
        serverHandler = GameObject.FindGameObjectWithTag("Handler").GetComponent<ServerHandler>();
        serverHandler.serverScript = this;
        playerKeys = new Dictionary<int, Dictionary<string, bool>>();
        for (int i = 1; i <= 4; i++)
        {
            playerKeys.Add(i, new Dictionary<string, bool>());
            playerKeys[i].Add("W", false);
            playerKeys[i].Add("A", false);
            playerKeys[i].Add("S", false);
            playerKeys[i].Add("D", false);
        }
    }

    private void Update()
    {
        int horizontal = 0, vertical = 0;
        for (int i = 1; i <= 4; i++)
        {
            if (playerKeys[i]["W"])
            {
                vertical++;
            }
            if (playerKeys[i]["A"])
            {
                horizontal--;
            }
            if (playerKeys[i]["S"])
            {
                vertical--;
            }
            if (playerKeys[i]["D"])
            {
                horizontal++;
            }
            if (playerInputs.ContainsKey(i))
            {
                playerInputs[i].GetBodyMovement(new Vector2(horizontal, vertical));
                if (horizontal != 0 || vertical != 0)
                {
                    Vector2 pos = playerInputs[i].transform.GetChild(0).position;
                    string message = "POS" + i;
                    message += pos.x < 0 ? "-" : "+";
                    message += Mathf.Abs(pos.x).ToString("F2");
                    message += pos.y < 0 ? "-" : "+";
                    message += Mathf.Abs(pos.y).ToString("F2");
                    serverHandler.SendToAll(message);
                }
            }
            horizontal = vertical = 0;
        }
    }

    public bool ChooseTeam(string message, int from){
        int team = int.Parse(message.Substring(3, 1));
        if (team == 1 && playersOnTeam1 < 2){
            playersOnTeam1++;
            players.Add(new PlayerScript(from, team));
            return true;
        }
        else if(team == 2 && playersOnTeam2 < 2){
            playersOnTeam2++;
            players.Add(new PlayerScript(from, team));
            return true;
        }
        return false;
    }

    public bool ChooseTank(string message, int from)
    {
        int tank = int.Parse(message.Substring(4, 1));
        foreach(PlayerScript p in players)
        {
            if (p.TankId == tank) return false;
        }
        players[from - 1].SetTank(tank);
        playerInputs[from] = Instantiate(tankPrefabs[tank - 1], players[from - 1].TeamId == 1 ? team1Spawns[isTeam1Spawn1Taken ? 1 : 0] : team2Spawns[isTeam2Spawn1Taken ? 1 : 0]).GetComponent<PlayerInput>();
        if (players[from - 1].TeamId == 1) isTeam1Spawn1Taken = true;
        else isTeam2Spawn1Taken = true;
        return true;
    }

    public string GetInfo(int id){
        string message = "INF" + id.ToString();
        for(int i = 0; i < players.Count; i++){
            message += players[i].TeamId;
            message += players[i].TankId;
        }
        for(int i = players.Count-1; i < 4; i++){
            message += "0";
            message += "0";
        }
        return message;
    }

    public void ChangeKeyState(int clientId, string key, bool pressed)
    {
        playerKeys[clientId][key] = pressed;
    }

    

}
