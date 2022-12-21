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
    public List<PlayerScript> players = new List<PlayerScript>();
    public Transform[] team1Spawns;
    public Transform[] team2Spawns;
    public GameObject[] tankPrefabs;

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

    private void Update()
    {
        if (hasGameStarted)
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
        }
    }

    public void ReceiveInfo(string message) //ex: INF12110000
    {
        playerId = int.Parse(message.Substring(3, 1));
        for (int i = 0; i < 4; i++)
        {
            int team = int.Parse(message.Substring(4 + i * 2, 1));
            int tank = int.Parse(message.Substring(5 + i * 2, 1));
            if (team != 0)
            {
                AddPlayer(team);

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
        clientHandler.SendToServer("CTE" + team.ToString());
    }

    public void TeamIsChosen(int team)
    {
        AddPlayer(team);
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

    private void AddPlayer(int team)
    {
        if (team == 1) playersOnTeam1++;
        else playersOnTeam2++;
        players.Add(new PlayerScript(players.Count, team));
    }

    private void AddTank(int player, int tank)
    {
        tankButtons[tank - 1].interactable = false;
        players[player - 1].SetTank(tank);
        playerInputs[player] = Instantiate(tankPrefabs[tank - 1], players[player - 1].TeamId == 1 ? team1Spawns[isTeam1Spawn1Taken ? 1 : 0] : team2Spawns[isTeam2Spawn1Taken ? 1 : 0]).GetComponent<PlayerInput>();
        if (players[player - 1].TeamId == 1) isTeam1Spawn1Taken = true;
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
                    AddPlayer(selectedTeam);
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

    public void MovePlayer(string message) //ex: POS1+5.00-8.00
    {
        int player = int.Parse(message.Substring(3, 1));
        float x = float.Parse(message.Substring(4, 5));
        float y = float.Parse(message.Substring(9, 5));

        playerInputs[player].transform.GetChild(0).position = new Vector2(x, y);
        players[player - 1].SetPosition(x, y);
    }

    public void RotatePlayerBase(string message) //ex: ROB1180
    {
        int player = int.Parse(message.Substring(3, 1));
        int angle = int.Parse(message.Substring(4, 3));

        players[player - 1].SetBaseAngle(angle);
    }

    public void RotatePlayerTurret(string message) //ex: ROT1180
    {
        int player = int.Parse(message.Substring(3, 1));
        int angle = int.Parse(message.Substring(4, 3));

        players[player - 1].SetTurretAngle(angle);
    }


}
