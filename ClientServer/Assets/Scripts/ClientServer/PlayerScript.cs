using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int TankId = 0;
    public int TeamId = 0;
    public int Id = 0;   

    public PlayerScript(int id, int team){
        Id = id;
        TeamId = team;
    } 

    public void SetTank(int tank){
        TankId = tank;
    }
}
