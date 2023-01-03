using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInput : MonoBehaviour
{
    public bool isTesting = false;
    public int playerId;

    [SerializeField]
    private Camera mainCamera;
    public Transform tankBase;
    public Transform tankTurret;
    public Transform tankCannon;

    /*public UnityEvent OnShoot = new UnityEvent();
    public UnityEvent<Vector2> OnMoveBody = new UnityEvent<Vector2>();
    public UnityEvent<Vector2> OnMoveTurret = new UnityEvent<Vector2>();*/

    private TankController tankController;

    public bool canItMove = true;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        tankController = tankBase.GetComponent<TankController>();
    }

    void Update()
    {
        if (isTesting)
        {
            GetBodyMovement();
            GetTurretMovement();
            GetShootingInput();
        }
    }

    private void GetShootingInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //OnShoot?.Invoke();
            tankController.HandleShoot();
        }
    }

    private void GetTurretMovement()
    {
        //OnMoveTurret?.Invoke(GetMousePositon());
        tankController.HandleTurretMovement(GetMousePositon());
    }

    public Vector2 GetMousePositon()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = mainCamera.nearClipPlane;
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        return mouseWorldPosition;
    }

    private void GetBodyMovement()
    {
        Vector2 movementVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        //OnMoveBody?.Invoke(movementVector.normalized);
        tankController.HandleMoveBody(movementVector);
    }

    public void GetTurretMovement(Vector2 mousePosition)
    {
        //OnMoveTurret?.Invoke(mousePosition);
        tankController.HandleTurretMovement(mousePosition);
    }

    public void GetBodyMovement(Vector2 movementVector)
    {
        //OnMoveBody?.Invoke(movementVector.normalized);
        tankController.HandleMoveBody(movementVector);
    }

    public bool TryToShoot()
    {
        //OnShoot?.Invoke();
        return tankController.HandleShoot();
    }
}