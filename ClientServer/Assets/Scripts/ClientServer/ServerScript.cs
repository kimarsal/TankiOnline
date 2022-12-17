using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerScript : MonoBehaviour
{
    public List<PlayerScript> players = new List<PlayerScript>();
    public int playersOnTeam1 = 0;
    public int playersOnTeam2 = 0;

    private void Start(){
        GameObject.FindGameObjectWithTag("Handler").GetComponent<ServerHandler>().serverScript = this;
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
        return true;
    }

    public string GetInfo(){
        string message = "INF";
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

    

}
