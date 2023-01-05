using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour
{
    public Rigidbody2D rb2d;
    private Vector2 movementVector;
    public float maxSpeed=10;
    public float rotationSpeed=100;
    public float turretRotationSpeed=150;
    public Transform turretParent;
    public GameObject bullet;
    bool CanShoot = true;
    public Transform Canon;
    public float firerate = 0.5f;

    private void Awake()
    {
        rb2d=GetComponent<Rigidbody2D>();
    }

    public bool HandleShoot()
    {
        if (CanShoot) 
        {
            //disparar bala si pot
            //Instantiate(bullet, Canon.position, Canon.rotation);
            GameObject bala = (GameObject)Instantiate(bullet, Canon.position, Canon.rotation);

            bala.GetComponent<Bullet>().SetParams(new Vector2(Canon.position.x - transform.position.x, Canon.position.y - transform.position.y));
            StartCoroutine(Shooting(firerate));
            return true;
        }
        return false;
    }

    public void HandleMoveBody(Vector2 movementVector)
    {
        this.movementVector=movementVector;
    }

    public void HandleTurretMovement(Vector2 pointerPosition)
    {
        var turretDirection=(Vector3)pointerPosition-turretParent.position;
        var desiredAngle=Mathf.Atan2(turretDirection.y,turretDirection.x)*Mathf.Rad2Deg;
        var rotationStep =turretRotationSpeed*Time.deltaTime;
        turretParent.rotation=Quaternion.RotateTowards(turretParent.rotation,Quaternion.Euler(0,0,desiredAngle-90),rotationStep);
    }

    private void FixedUpdate(){
        rb2d.velocity=(Vector2)transform.up*movementVector.y*maxSpeed*Time.fixedDeltaTime;
        rb2d.MoveRotation(transform.rotation*Quaternion.Euler(0,0,-movementVector.x*rotationSpeed*Time.fixedDeltaTime));
    }
    
    IEnumerator Shooting(float time)
    {
        CanShoot = false;

        yield return new WaitForSeconds(time);

        CanShoot = true;
    }
}