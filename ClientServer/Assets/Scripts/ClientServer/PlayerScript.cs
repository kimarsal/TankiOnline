using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int TankId = 0;
    public int TeamId = 0;
    public int Id = 0;   

    public PlayerScript(int id, int n){
        Id = id;
        TeamId = n;
    } 

    public void JugadorScriptTankId(int n){
        TankId = n;
    }
}
