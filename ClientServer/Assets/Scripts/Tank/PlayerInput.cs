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
    public Transform MinePos;
    public GameObject Mine;
    public TankController tankController;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        tankController = transform.GetComponentInChildren<TankController>();
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
            tankController.HandleShoot();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            tankController.HandleShootSpecial();
        }
    }

    private void GetTurretMovement()
    {
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
        tankController.HandleMoveBody(movementVector);
    }

    public void GetTurretMovement(Vector2 mousePosition)
    {
        tankController.HandleTurretMovement(mousePosition);
    }

    public void GetBodyMovement(Vector2 movementVector)
    {
        tankController.HandleMoveBody(movementVector);
    }

    public bool TryToShoot()
    {
        return tankController.HandleShoot();
    }

    public bool TryToShootSpecial()
    {
        return tankController.HandleShootSpecial();
    }
}