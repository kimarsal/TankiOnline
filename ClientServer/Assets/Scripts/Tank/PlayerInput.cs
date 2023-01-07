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
    public Transform MinePos;
    public GameObject Mine;

    public String color="";

    /*public UnityEvent OnShoot = new UnityEvent();
    public UnityEvent<Vector2> OnMoveBody = new UnityEvent<Vector2>();
    public UnityEvent<Vector2> OnMoveTurret = new UnityEvent<Vector2>();*/

    private TankController tankController;
    private bool doubleshoot=false;
    private float abilitytime=0;
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
            /*if(color=="Blue")GetMinesInput();
            if(color=="Red")GetdoubleshootInput();*/
            GetBodyMovement();
            GetTurretMovement();
            GetShootingInput();
            /*if(doubleshoot){
                abilitytime+=Time.deltaTime;
            }
            if(abilitytime>4){
                doubleshoot=false;
                abilitytime=0;
            }*/
        }
    }
    private void GetdoubleshootInput(){
        if (Input.GetKeyDown("space"))
        {
            doubleshoot=true;
        }
    }
    private void GetMinesInput(){
        if (Input.GetKeyDown("space"))
        {
            Instantiate(Mine, MinePos.position, tankBase.rotation);
        }
    }

    private void GetShootingInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            tankController.HandleShoot(/*doubleshoot*/);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            tankController.HandleShootSpecial(color);
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
        //if(isTesting) return tankController.HandleShoot(doubleshoot);
        return tankController.HandleShoot();
    }

    public bool TryToShootSpecial()
    {
        return tankController.HandleShootSpecial(color);
    }
}