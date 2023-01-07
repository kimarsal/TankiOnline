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
    public GameObject bulletPrefab;
    private bool CanShoot = true;
    private bool CanShootSpecial = true;
    public Transform Canon;
    public float firerate = 0.5f;
    public float specialFirerate = 2f;
    public GameObject minePrefab;


    private void Awake()
    {
        rb2d=GetComponent<Rigidbody2D>();
    }

    public bool HandleShoot(bool d)
    {
        if (CanShoot) 
        {
            
            //disparar bala si pot
            //Instantiate(bullet, Canon.position, Canon.rotation);
            GameObject bala = (GameObject)Instantiate(bulletPrefab, Canon.position, Canon.rotation);

            bala.GetComponent<Bullet>().SetParams(new Vector2(Canon.position.x - transform.position.x, Canon.position.y - transform.position.y));
            if(d){
                GameObject bala2 = (GameObject)Instantiate(bulletPrefab, Canon.position, Canon.rotation);
                bala2.GetComponent<Bullet>().SetParams(new Vector2(Canon.position.x - transform.position.x, Canon.position.y - transform.position.y));
                GameObject bala3 = (GameObject)Instantiate(bulletPrefab, Canon.position, Canon.rotation);
                bala3.GetComponent<Bullet>().SetParams(new Vector2(Canon.position.x - transform.position.x, Canon.position.y - transform.position.y));
            }
            StartCoroutine(Shooting());
            return true;
        }
        return false;
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

    public bool HandleShootSpecial(string color)
    {
        if (CanShoot && CanShootSpecial)
        {
            switch (color)
            {
                case "Blue": Instantiate(minePrefab, transform.position, transform.rotation); break;
                case "Green": StartCoroutine(Spurt()); break;
                case "Red": SpawnBullet(false, -30); SpawnBullet(); SpawnBullet(false, 30); StartCoroutine(Shooting()); break;
                case "White": SpawnBullet(true); break;
            }

            StartCoroutine(ShootingSpecial());
            return true;
        }
        return false;
    }

    private void SpawnBullet(bool isMissile = false, float angleOffset = 0)
    {
        Bullet bullet = Instantiate(bulletPrefab, Canon.position, Quaternion.Euler(0, 0, Canon.rotation.z + angleOffset)).GetComponent<Bullet>();
        bullet.SetParams(new Vector2(Canon.position.x - transform.position.x, Canon.position.y - transform.position.y));
        if (isMissile)
        {
            bullet.transform.localScale *= 2f;
            bullet.nBounces = bullet.MAX_BOUNCES;
        }
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