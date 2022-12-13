using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Camera mainCamera;
    public float turretRotSpeed = 150;
    bool CanShoot = true;
    public float firerate = 0.5f;

    public Transform Canon;
    public GameObject bullet;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GetTurretMovement();
        GetShootingInput();
    }

    private void GetShootingInput() {
        if (Input.GetMouseButtonDown(0) && CanShoot) 
        {
            //disparar bala si pot
            Instantiate(bullet, Canon.position, Canon.rotation);
            StartCoroutine(Shooting(firerate));
        }
    }

    private void GetTurretMovement()
    {
        //GetMousePosition
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = mainCamera.nearClipPlane;
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        //print(mouseWorldPosition);

        var turretDirection = (Vector3)mouseWorldPosition - transform.position;
        var desoredAngle = Mathf.Atan2(turretDirection.y, turretDirection.x) * Mathf.Rad2Deg;
        var rotationStep = turretRotSpeed * Time.deltaTime;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, desoredAngle), rotationStep);
    }

    IEnumerator Shooting(float time)
    {
        CanShoot = false;

        yield return new WaitForSeconds(time);

        CanShoot = true;
    }

}
