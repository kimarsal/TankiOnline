using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int TankId = 0;
    public int TeamId = 0;
    
    public Vector2 Position;
    public float BaseAngle;
    public float TurretAngle;

    public PlayerScript(int team){
        TeamId = team;
    } 

    public void SetTank(int tank){
        TankId = tank;
    }

    public void SetPosition(float x, float y)
    {
        Position = new Vector2(x, y);
    }

    public void SetBaseAngle(float angle)
    {
        BaseAngle = angle;
    }

    public void SetTurretAngle(float angle)
    {
        TurretAngle = angle;
    }
}
