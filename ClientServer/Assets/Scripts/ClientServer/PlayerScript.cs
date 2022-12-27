using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int TankId = 0;
    public int TeamId = 0;

    public Vector2 PreviousPosition;
    public float PreviousBaseAngle;
    public float PreviousTurretAngle;

    public Vector2 FuturePosition;
    public float FutureBaseAngle;
    public float FutureTurretAngle;

    public PlayerScript(int team)
    {
        TeamId = team;
    }

    public void SetTank(int tank)
    {
        TankId = tank;
    }
}
