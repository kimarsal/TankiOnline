using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JugadorScript : MonoBehaviour
{

    public int TankId = 0;
    public int TeamId = 0;
    public int Id = 0;
    // Start is called before the first frame update
   
    public JugadorScript(int id, int n){
        Id = id;
        TeamId = n;
    } 
    public void JugadorScriptTankId(int n){
        TankId = n;
    }
}
