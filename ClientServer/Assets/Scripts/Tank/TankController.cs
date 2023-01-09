using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class TankController : MonoBehaviour
{
    public enum TankType { BlueTank, GreenTank, RedTank, WhiteTank };
    public TankType tankType;

    public Rigidbody2D rb2d;
    private Vector2 movementVector;
    public float maxSpeed=10;
    public float rotationSpeed=100;
    public float turretRotationSpeed=150;
    public Transform turretParent;
    public Transform Canon;
    public Transform Canon2;
    public Transform Canon3;
    public GameObject bulletPrefab;
    public GameObject minePrefab;

    private bool CanShoot = true;
    private bool CanShootSpecial = true;
    private float firerate = 0.5f;
    private float specialFirerate = 2f;


    private void Awake()
    {
        rb2d=GetComponent<Rigidbody2D>();
    }

    public bool HandleShoot()
    {
        if (CanShoot)
        {
            SpawnBullet();
            StartCoroutine(Shooting());
            return true;
        }
        return false;
    }

    public bool HandleShootSpecial()
    {
        if (CanShoot && CanShootSpecial)
        {
            switch (tankType)
            {
                case TankType.BlueTank: Instantiate(minePrefab, transform.position, transform.rotation); break;
                case TankType.GreenTank: StartCoroutine(Spurt()); break;
                case TankType.RedTank: SpawnBullet(true); StartCoroutine(Shooting()); break;
                case TankType.WhiteTank: SpawnBullet(true); StartCoroutine(Shooting()); break;
            }

            StartCoroutine(ShootingSpecial());
            return true;
        }
        return false;
    }

    private void SpawnBullet(bool special = false)
    {
        Bullet bullet = Instantiate(bulletPrefab, Canon.position,Canon.rotation).GetComponent<Bullet>();
        bullet.SetParams(GetBulletInitialVelocity(Canon));
        if (special)
        {
            if(tankType == TankType.WhiteTank)
            {
                bullet.transform.localScale *= 2f;
                bullet.nBounces = bullet.MAX_BOUNCES;
            }
            else if (tankType == TankType.RedTank)
            {
                Bullet bullet2 = Instantiate(bulletPrefab, Canon2.position, Canon2.rotation).GetComponent<Bullet>();
                bullet2.SetParams(GetBulletInitialVelocity(Canon2));
                Bullet bullet3 = Instantiate(bulletPrefab, Canon3.position, Canon3.rotation).GetComponent<Bullet>();
                bullet3.SetParams(GetBulletInitialVelocity(Canon3));
            }
        }
    }

    public Vector2 GetBulletInitialVelocity(Transform canon)
    {
        return new Vector2(canon.position.x - transform.position.x, canon.position.y - transform.position.y);
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

    private void FixedUpdate()
    {
        rb2d.velocity=(Vector2)transform.up*movementVector.y*maxSpeed*Time.fixedDeltaTime;
        rb2d.MoveRotation(transform.rotation*Quaternion.Euler(0,0,-movementVector.x*rotationSpeed*Time.fixedDeltaTime));
    }

    IEnumerator Spurt()
    {
        for(int i = 0; i < 3; i++)
        {
            SpawnBullet();
            yield return new WaitForSeconds(0.2f);
        }
        StartCoroutine(Shooting());
    }
    
    IEnumerator Shooting()
    {
        CanShoot = false;

        yield return new WaitForSeconds(firerate);

        CanShoot = true;
    }

    IEnumerator ShootingSpecial()
    {
        CanShootSpecial = false;

        yield return new WaitForSeconds(specialFirerate);

        CanShootSpecial = true;
    }
}