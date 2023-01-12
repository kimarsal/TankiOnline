using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Drawing;
using static TankController;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;

public class ClientScript : MonoBehaviour
{
    private ClientHandler clientHandler;

    private enum ClientState { ChoosingTeam, ChoosingTank, Waiting, Playing };
    private ClientState clientState;

    [Header("UI")]
    public GameObject chooseTeamContainer;
    public Button[] teamButtons;
    public GameObject chooseTankContainer;
    public Button[] tankButtons;
    public GameObject waitingForPlayersText;
    public GameObject gameIsFullText;
    public GameObject startGameText;
    public GameObject team1WinsText;
    public GameObject team2WinsText;
    public AudioSource musicSource;
    public AudioSource tankSource;

    [Header("Gameplay")]
    public Transform[] team1Spawns;
    public Transform[] team2Spawns;
    public GameObject[] tankPrefabs;
    public GameObject[] bulletPrefabs;
    public GameObject minePrefab;
    public GameObject tankExplosion;
    public GameObject bulletExplosion;

    public Dictionary<int, PlayerInput> playerInputs = new Dictionary<int, PlayerInput>();
    public Dictionary<int, PlayerScript> players = new Dictionary<int, PlayerScript>();
    public List<Bullet> bulletList = new List<Bullet>();
    public List<Mine> mineList = new List<Mine>();
    public List<ObjetoDestruible> objectList;

    private int playersOnTeam1;
    private int playersOnTeam2;
    private bool isTeam1Spawn1Taken;
    private bool isTeam2Spawn1Taken;

    private int playerId;
    private int selectedTeam;
    private int selectedTank;

    private bool isGameOn = true;
    private float timeSinceLastUpdate = 0f;
    private float timeSinceLastMouseUpdate = 0f;
    private float updateDuration = 0f;
    private bool isPlayerMoving = false;

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

        musicSource.Play();
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

        if (playerInputs.Count == 4)
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
        if(playerInputs.Count == 4)
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
        int team = players[player].TeamId;
        tankButtons[tank - 1].interactable = false;
        players[player].SetTank(tank);

        Transform spawn = team == 1 ? team1Spawns[isTeam1Spawn1Taken ? 1 : 0] : team2Spawns[isTeam2Spawn1Taken ? 1 : 0];
        if (players[player].TeamId == 1) isTeam1Spawn1Taken = true;
        else isTeam2Spawn1Taken = true;

        PlayerInput playerInput = Instantiate(tankPrefabs[tank - 1], spawn).GetComponent<PlayerInput>();
        playerInput.playerId = player;
        players[player].PreviousPosition = spawn.position;
        playerInputs.Add(player, playerInput);
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
                    if(playerInputs.Count == 4)
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
        isGameOn = true;
        clientState = ClientState.Playing;
        waitingForPlayersText.SetActive(false);
        StartCoroutine(ShowStartGameText());

        musicSource.Play();
    }

    private IEnumerator ShowStartGameText()
    {
        startGameText.SetActive(true);
        yield return new WaitForSeconds(1);
        startGameText.SetActive(false);
    }

    #endregion

    #region Game

    private void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        timeSinceLastMouseUpdate += Time.deltaTime;
        float percentageComplete = timeSinceLastUpdate / updateDuration;
        MoveTanks(percentageComplete);
        MoveBullets(percentageComplete);
        MoveMines(percentageComplete);
        CheckForKeys();
    }

    private void MoveTanks(float percentageComplete)
    {
        for (int i = 1; i <= 4; i++)
        {
            if (playerInputs.ContainsKey(i))
            {
                playerInputs[i].tankController.transform.position = Vector3.Lerp(players[i].PreviousPosition, players[i].FuturePosition, percentageComplete);
                playerInputs[i].tankController.transform.rotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(players[i].PreviousBaseAngle, players[i].FutureBaseAngle, percentageComplete));
                playerInputs[i].tankController.turretParent.rotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(players[i].PreviousTurretAngle, players[i].FutureTurretAngle, percentageComplete));
            }
        }
    }

    private void MoveBullets(float percentageComplete)
    {
        for (int i = 0; i < bulletList.Count; i++)
        {
            bulletList[i].transform.position = Vector3.Lerp(bulletList[i].PreviousPosition, bulletList[i].FuturePosition, percentageComplete);
        }
    }

    private void MoveMines(float percentageComplete)
    {
        for (int i = 0; i < mineList.Count; i++)
        {
            mineList[i].transform.position = Vector3.Lerp(mineList[i].PreviousPosition, mineList[i].FuturePosition, percentageComplete);
        }
    }

    private void CheckForKeys()
    {
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

        if (!isGameOn) return;

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            clientHandler.SendToServer("SHS");
        }

        if(!isPlayerMoving && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            isPlayerMoving = true;
            StartCoroutine(ChangeTankVolume(1));
        }
        else if(isPlayerMoving && Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            isPlayerMoving = false;
            StartCoroutine(ChangeTankVolume(0));
        }
    }

    private IEnumerator ChangeTankVolume(float newVolume)
    {
        float previousVolume = tankSource.volume;
        float duration = 0.3f;
        float timePassed = 0f;

        while(timePassed <= duration)
        {
            float percentage = timePassed / duration;
            tankSource.volume = Mathf.Lerp(previousVolume, newVolume, percentage);
            timePassed += 0.05f;
            yield return new WaitForSeconds(0.05f);
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
                playerInputs[i + 1].tankController.transform.position = players[i + 1].PreviousPosition = players[i + 1].FuturePosition;
                players[i + 1].FuturePosition = new Vector2(x, y);

                float baseAngle = float.Parse(message.Substring(15 + i * 18, 3));
                players[i + 1].PreviousBaseAngle = players[i + 1].FutureBaseAngle;
                players[i + 1].FutureBaseAngle = baseAngle;
                playerInputs[i + 1].tankController.transform.rotation = Quaternion.Euler(0, 0, players[i + 1].PreviousBaseAngle);

                float turretAngle = float.Parse(message.Substring(18 + i * 18, 3));
                players[i + 1].PreviousTurretAngle = players[i + 1].FutureTurretAngle;
                players[i + 1].FutureTurretAngle = turretAngle;
                playerInputs[i + 1].tankController.turretParent.rotation = Quaternion.Euler(0, 0, players[i + 1].PreviousTurretAngle);
            }
        }
    }

    public void UpdateBullets(string message) //UDB+00.00-00.00...
    {
        int i = 0;
        while(3 + i * 12 < message.Length)
        {
            float x = float.Parse(message.Substring(3 + i * 12, 6));
            float y = float.Parse(message.Substring(9 + i * 12, 6));
            bulletList[i].transform.position = bulletList[i].PreviousPosition = bulletList[i].FuturePosition;
            bulletList[i].FuturePosition = new Vector2(x, y);
            i++;
        }
    }

    public void UpdateMines(string message) //UDM+00.00-00.00...
    {
        int i = 0;
        while (3 + i * 12 < message.Length)
        {
            float x = float.Parse(message.Substring(3 + i * 12, 6));
            float y = float.Parse(message.Substring(9 + i * 12, 6));
            mineList[i].transform.position = mineList[i].PreviousPosition = mineList[i].FuturePosition;
            mineList[i].FuturePosition = new Vector2(x, y);
            i++;
        }
    }

    public void TankShot(int player)
    {
        SpawnBullet(player);
    }

    public void TankShotSpecial(int player)
    {
        switch (playerInputs[player].tankController.tankType)
        {
            case TankType.BlueTank: Instantiate(minePrefab, playerInputs[player].MinePos.position, minePrefab.transform.rotation); /*TODO: treure habilitat de matar :)*/break;
            case TankType.GreenTank: StartCoroutine(Spurt(player)); break;
            case TankType.RedTank: SpawnBullet(player, true); break;
            case TankType.WhiteTank: SpawnBullet(player, true); break;
        }
    }

    private void SpawnBullet(int player, bool special = false)
    {
        Bullet bullet = Instantiate(bulletPrefabs[players[player].TankId - 1], playerInputs[player].tankController.Canon.position, playerInputs[player].tankController.Canon.rotation).GetComponent<Bullet>();
        bullet.SetParams(playerInputs[player].tankController.GetBulletInitialVelocity(playerInputs[player].tankController.Canon));
        bullet.PreviousPosition = bullet.FuturePosition = playerInputs[player].tankController.Canon.position;
        bulletList.Add(bullet);

        if (special)
        {
            if (playerInputs[player].tankController.tankType == TankType.WhiteTank)
            {
                bullet.transform.localScale *= 2f;
                bullet.nBounces = bullet.MAX_BOUNCES;
            }
            else if (playerInputs[player].tankController.tankType == TankType.RedTank)
            {
                Bullet bullet2 = Instantiate(bulletPrefabs[players[player].TankId - 1], playerInputs[player].tankController.Canon2.position, playerInputs[player].tankController.Canon2.rotation).GetComponent<Bullet>();
                bullet2.SetParams(playerInputs[player].tankController.GetBulletInitialVelocity(playerInputs[player].tankController.Canon2));
                bullet2.PreviousPosition = bullet2.FuturePosition = playerInputs[player].tankController.Canon2.position;
                bulletList.Add(bullet2);

                Bullet bullet3 = Instantiate(bulletPrefabs[players[player].TankId - 1], playerInputs[player].tankController.Canon3.position, playerInputs[player].tankController.Canon3.rotation).GetComponent<Bullet>();
                bullet3.SetParams(playerInputs[player].tankController.GetBulletInitialVelocity(playerInputs[player].tankController.Canon3));
                bullet3.PreviousPosition = bullet3.FuturePosition = playerInputs[player].tankController.Canon3.position;
                bulletList.Add(bullet3);
            }
        }
    }

    IEnumerator Spurt(int player)
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnBullet(player);
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void BulletIsDestroyed(int bullet)
    {
        Instantiate(bulletExplosion, bulletList[bullet].transform.position, Quaternion.identity);
        GameObject b = bulletList[bullet].gameObject;
        bulletList.RemoveAt(bullet);
        Destroy(b);
    }

    public void ObjectIsDestroyed(int objeto)
    {
        Instantiate(bulletExplosion, objectList[objeto].transform.position, Quaternion.identity);
        GameObject o = objectList[objeto].gameObject;
        objectList.RemoveAt(objeto);
        Destroy(o);
    }

    public void MineIsDestroyed(int mine)
    {
        Instantiate(bulletExplosion, mineList[mine].transform.position, Quaternion.identity);
        GameObject m = mineList[mine].gameObject;
        objectList.RemoveAt(mine);
        Destroy(m);
    }

    public void TankIsDestroyed(int player)
    {
        Instantiate(tankExplosion, playerInputs[player].transform.position, Quaternion.identity);

        PlayerInput playerInput;
        playerInputs.Remove(player, out playerInput);
        Destroy(playerInput.transform.parent.gameObject);

        if (!isGameOn) return;

        if (players[player].TeamId == 1)
        {
            playersOnTeam1--;
            if (playersOnTeam1 == 0)
            {
                GameOver(false);
            }
        }
        else
        {
            playersOnTeam2--;
            if (playersOnTeam2 == 0)
            {
                GameOver(true);
            }
        }
    }

    private void GameOver(bool team1Won)
    {
        isGameOn = false;
        if (team1Won) team1WinsText.SetActive(true);
        else team2WinsText.SetActive(true);

        clientHandler.SendToServer("KYUW");
        clientHandler.SendToServer("KYUA");
        clientHandler.SendToServer("KYUS");
        clientHandler.SendToServer("KYUD");

        musicSource.Stop();
    }

    #endregion

}
