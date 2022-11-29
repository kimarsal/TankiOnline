using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerScript : MonoBehaviour
{
    public ServerHandler serverHandler;
    public List<JugadorScript> js = new List<JugadorScript>();
    public int nTeam1 = 0;
    public int nTeam2 = 0;
    // Start is called before the first frame update

    void Awake(){
        serverHandler = GameObject.FindGameObjectWithTag("Handler").GetComponent<ServerHandler>();
        serverHandler.serverScript = this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ReceiveTeam(string message, int from){
        int n = int.Parse(message.Substring(3, 4));
        if (n == 1){
            nTeam1 += 1;
        }
        else if(n == 2){
            nTeam2 += 1;
        }
        js.Add(new JugadorScript(from, n));
        serverHandler.SendToAll(message);
    }

    public void ReceiveSendTank(string message, int from){
        int n = int.Parse(message.Substring(3, 4));
        JugadorScript j = js.Find(js => js.Id == from);
        j.JugadorScriptTankId(n);
        serverHandler.SendToAll(message);
    }

    public void EnviaINF(){
        Debug.Log("entra");
        string inf = "INF";
        for(int i = 0; i < js.Count; i++){
            inf += js[i].TeamId;
            inf += js[i].TankId;
        }
        for(int i = js.Count-1; i < 4; i++){
            inf += "0";
            inf += "0";
        }
        serverHandler.SendToAll(inf);
    }

    

}
