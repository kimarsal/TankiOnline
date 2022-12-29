using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClientScript : MonoBehaviour
{
    private ClientHandler clientHandler;

    public GameObject chooseTeamContainer;
    public GameObject chooseTankContainer;
    public GameObject waitingForPlayersText;
    public GameObject gameIsFullText;
    public GameObject startGameText;
    public Button[] teamButtons;
    public Button[] tankButtons;

    public Dictionary<int, PlayerInput> playerInputs = new Dictionary<int, PlayerInput>();
    public Dictionary<int, PlayerScript> players = new Dictionary<int, PlayerScript>();
    public Transform[] team1Spawns;
    public Transform[] team2Spawns;
    public GameObject[] tankPrefabs;
    public GameObject[] bulletPrefabs;

    private int playersOnTeam1;
    private int playersOnTeam2;
    private int chosenTanks;
    private bool isTeam1Spawn1Taken;
    private bool isTeam2Spawn1Taken;

    private int playerId;
    private int selectedTeam;
    private int selectedTank;

    private enum ClientState { ChoosingTeam, ChoosingTank, Waiting, Playing };
    private ClientState clientState;

    private bool hasGameStarted = true;
    private float timeSinceLastUpdate = 0f;
    private float updateDuration = 0f;

    private float timeSinceLastMouseUpdate = 0f;

    public List<Bullet> bulletList = new List<Bullet>();

    #region Lobby

    private void Start()
    {
        clientHandler = GameObject.FindGameObjectWithTag("Handler").GetComponent<ClientHandler>();
        clientHandler.clientScript = this;

        chooseTeamContainer.SetActive(true);
        chooseTankContainer.SetActive(false);
        waitingForPlayersText.SetActive(false);
        gameIsFullText.SetActive(false);
        startGameText.SetActive(false);

        clientState = ClientState.ChoosingTeam;
    }

    public void ReceiveInfo(string message) //ex: INF312110000
    {
        playerId = int.Parse(message.Substring(3, 1));
        for (int i = 0; i < 4; i++)
        {
            int team = int.Parse(message.Substring(4 + i * 2, 1));
            int tank = int.Parse(message.Substring(5 + i * 2, 1));
            if (team != 0)
            {
                AddPlayer(i + 1, team);

                if (tank != 0)
                {
                    AddTank(i + 1, tank);
                }
            }
        }

        if (chosenTanks == 4)
        {
            chooseTeamContainer.SetActive(false);
            gameIsFullText.SetActive(true);
        }
        else
        {
            UpdateTeamButtons();
        }
    }

    private void UpdateTeamButtons()
    {
        teamButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "Team 1\n("+playersOnTeam1+"/2)";
        teamButtons[0].interactable = playersOnTeam1 < 2;

        teamButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = "Team 2\n(" + playersOnTeam2 + "/2)";
        teamButtons[1].interactable = playersOnTeam2 < 2;
    }

    public void ChooseTeam(int team)
    {
        selectedTeam = team;
        teamButtons[0].enabled = false;
        teamButtons[1].enabled = false;
        clientHandler.SendToServer("CTE" + playerId.ToString() + team.ToString());
    }

    public void TeamIsChosen(int player, int team)
    {
        AddPlayer(player, team);
        UpdateTeamButtons();
    }

    public void ChooseTank(int tank)
    {
        selectedTank = tank;
        tankButtons[0].enabled = false;
        tankButtons[1].enabled = false;
        tankButtons[2].enabled = false;
        tankButtons[3].enabled = false;
        clientHandler.SendToServer("CTA" + playerId.ToString() + tank.ToString());
    }

    public void TankIsChosen(int player, int tank)
    {
        AddTank(player, tank);
        if(chosenTanks == 4)
        {
            if (clientState == ClientState.Waiting)
            {
                StartGame();
            }
            else
            {
                if (clientState == ClientState.ChoosingTeam) chooseTeamContainer.SetActive(false);
                else if (clientState == ClientState.ChoosingTank) chooseTankContainer.SetActive(false);
                gameIsFullText.SetActive(true);
            }
        }
    }

    private void AddPlayer(int player, int team)
    {
        players.Add(player, new PlayerScript(team));
        if (team == 1) playersOnTeam1++;
        else playersOnTeam2++;
    }

    private void AddTank(int player, int tank)
    {
        tankButtons[tank - 1].interactable = false;
        players[player].SetTank(tank);
        playerInputs.Add(player, Instantiate(tankPrefabs[tank - 1], players[player].TeamId == 1 ? team1Spawns[isTeam1Spawn1Taken ? 1 : 0] : team2Spawns[isTeam2Spawn1Taken ? 1 : 0]).GetComponent<PlayerInput>());
        if (players[player].TeamId == 1) isTeam1Spawn1Taken = true;
        else isTeam2Spawn1Taken = true;
        chosenTanks++;
    }

    public void OkOrNot(bool ok)
    {
        switch (clientState)
        {
            case ClientState.ChoosingTeam:
                if (ok)
                {
                    AddPlayer(playerId, selectedTeam);
                    chooseTeamContainer.SetActive(false);
                    chooseTankContainer.SetActive(true);
                    clientState = ClientState.ChoosingTank;
                }
                else
                {
                    teamButtons[0].enabled = true;
                    teamButtons[1].enabled = true;
                }
                break;
            case ClientState.ChoosingTank:
                if (ok)
                {
                    AddTank(playerId, selectedTank);

                    chooseTankContainer.SetActive(false);
                    if(chosenTanks == 4)
                    {
                        StartGame();
                    }
                    else
                    {
                        waitingForPlayersText.SetActive(true);
                        clientState = ClientState.Waiting;
                    }
                }
                else
                {
                    tankButtons[0].enabled = true;
                    tankButtons[1].enabled = true;
                    tankButtons[2].enabled = true;
                    tankButtons[3].enabled = true;
                }
                break;
        }
    }

    private void StartGame()
    {
        hasGameStarted = true;
        clientState = ClientState.Playing;
        waitingForPlayersText.SetActive(false);
        startGameText.SetActive(true);
    }

    #endregion

    #region Game

    private void Update()
    {
        if (hasGameStarted)
        {
            timeSinceLastUpdate += Time.deltaTime;
            timeSinceLastMouseUpdate += Time.deltaTime;
            float percentageComplete = timeSinceLastUpdate / updateDuration;
            MoveTanks(percentageComplete);
            MoveBullets(percentageComplete);
            CheckForKeys();
        }
    }

    private void MoveTanks(float percentageComplete)
    {
        for (int i = 1; i <= 4; i++)
        {
            if (playerInputs.ContainsKey(i))
            {
                playerInputs[i].tankBase.position = Vector3.Lerp(players[i].PreviousPosition, players[i].FuturePosition, percentageComplete);
                playerInputs[i].tankBase.rotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(players[i].PreviousBaseAngle, players[i].FutureBaseAngle, percentageComplete));
                playerInputs[i].tankTurret.rotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(players[i].PreviousTurretAngle, players[i].FutureTurretAngle, percentageComplete));
            }
        }
    }

    private void MoveBullets(float percentageComplete)
    {
        for (int i = 0; i < bulletList.Count; i++)
        {
            bulletList[i].transform.position = Vector3.Lerp(bulletList[i].PreviousPosition, bulletList[i].FuturePosition, percentageComplete);
            bulletList[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(bulletList[i].PreviousAngle, bulletList[i].FutureAngle, percentageComplete));
        }
    }

    private void CheckForKeys()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            clientHandler.SendToServer("KYDW");
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            clientHandler.SendToServer("KYDA");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            clientHandler.SendToServer("KYDS");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            clientHandler.SendToServer("KYDD");
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            clientHandler.SendToServer("KYUW");
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            clientHandler.SendToServer("KYUA");
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            clientHandler.SendToServer("KYUS");
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            clientHandler.SendToServer("KYUD");
        }

        if (Input.GetMouseButtonDown(0))
        {
            clientHandler.SendToServer("SHN");
        }

        if (timeSinceLastMouseUpdate > 0.1f && playerInputs.ContainsKey(playerId) && (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0))
        {
            timeSinceLastMouseUpdate = 0f;
            Vector2 pos = playerInputs[playerId].GetMousePositon();
            string message = "MMC";
            message += pos.x < 0 ? "-" : "+";
            message += Mathf.Abs(pos.x).ToString("F2").PadLeft(5, '0');
            message += pos.y < 0 ? "-" : "+";
            message += Mathf.Abs(pos.y).ToString("F2").PadLeft(5, '0');
            clientHandler.SendToServer(message);
        }
    }

    public void UpdateTanks(string message) //UDT+00.00-00.00180180...
    {
        updateDuration = timeSinceLastUpdate;
        timeSinceLastUpdate = 0;
        for(int i = 0; i < 4; i++)
        {
            if (playerInputs.ContainsKey(i + 1))
            {
                float x = float.Parse(message.Substring(3 + i * 18, 6));
                float y = float.Parse(message.Substring(9 + i * 18, 6));
                playerInputs[i + 1].tankBase.position = players[i + 1].PreviousPosition = players[i + 1].FuturePosition;
                players[i + 1].FuturePosition = new Vector2(x, y);

                float baseAngle = float.Parse(message.Substring(15 + i * 18, 3));
                players[i + 1].PreviousBaseAngle = players[i + 1].FutureBaseAngle;
                players[i + 1].FutureBaseAngle = baseAngle;
                playerInputs[i + 1].tankBase.rotation = Quaternion.Euler(0, 0, players[i + 1].PreviousBaseAngle);

                float turretAngle = float.Parse(message.Substring(18 + i * 18, 3));
                players[i + 1].PreviousTurretAngle = players[i + 1].FutureTurretAngle;
                players[i + 1].FutureTurretAngle = turretAngle;
                playerInputs[i + 1].tankTurret.rotation = Quaternion.Euler(0, 0, players[i + 1].PreviousTurretAngle);
            }
        }
    }

    public void UpdateBullets(string message) //UDB+00.00-00.00180...
    {
        if (message.Length == 3) return;
        for (int i = 0; i < bulletList.Count; i++)
        {
            float x = float.Parse(message.Substring(3 + i * 15, 6));
            float y = float.Parse(message.Substring(9 + i * 15, 6));
            bulletList[i].transform.position = bulletList[i].PreviousPosition = bulletList[i].FuturePosition;
            bulletList[i].FuturePosition = new Vector2(x, y);

            float baseAngle = float.Parse(message.Substring(15 + i * 15, 3));
            bulletList[i].PreviousAngle = bulletList[i].FutureAngle;
            bulletList[i].FutureAngle = baseAngle;
            bulletList[i].transform.rotation = Quaternion.Euler(0, 0, bulletList[i].PreviousAngle);
        }
    }

    public void TankShot(int player)
    {
        bulletList.Add(Instantiate(bulletPrefabs[players[player].TankId - 1], playerInputs[player].tankCannon.position, playerInputs[player].tankCannon.rotation).GetComponent<Bullet>());
    }

    public void BulletIsDestroyed(int bullet)
    {
        Destroy(bulletList[bullet].gameObject);
        bulletList.RemoveAt(bullet);
    }

    #endregion

}
