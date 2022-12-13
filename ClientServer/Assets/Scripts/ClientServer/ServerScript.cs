using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerScript : MonoBehaviour
{
    public ServerHandler serverHandler;
    public List<PlayerScript> players = new List<PlayerScript>();
    public int playersOnTeam1 = 0;
    public int playersOnTeam2 = 0;

    private void Start(){
        serverHandler = GameObject.FindGameObjectWithTag("Handler").GetComponent<ServerHandler>();
        serverHandler.serverScript = this;
    }
    
    public bool ChooseTeam(string message, int from){
        int team = int.Parse(message.Substring(3, 4));
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

    public bool ChooseTank(string message, int from){
        int tank = int.Parse(message.Substring(3, 4));
        foreach(PlayerScript p in players)
        {
            if (p.TankId == tank) return false;
        }
        PlayerScript player = players.Find(players => players.Id == from);
        player.SetTank(tank);
        return true;
    }

    public void SendInfo(int id){
        string message = "INF";
        for(int i = 0; i < players.Count; i++){
            message += players[i].TeamId;
            message += players[i].TankId;
        }
        for(int i = players.Count-1; i < 4; i++){
            message += "0";
            message += "0";
        }
        serverHandler.SendToClient(id, message);
    }

    

}
