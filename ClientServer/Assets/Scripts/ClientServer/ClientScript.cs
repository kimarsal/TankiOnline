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
    public Button[] teamButtons;
    public Button[] tankButtons;
    public List<PlayerScript> players = new List<PlayerScript>();

    private int playersOnTeam1;
    private int playersOnTeam2;

    private enum ClientState { ChoosingTeam, ChoosingTank, Playing };
    private ClientState clientState;

    private void Start()
    {
        clientHandler = GameObject.FindGameObjectWithTag("Handler").GetComponent<ClientHandler>();
        clientHandler.clientScript = this;

        chooseTeamContainer.SetActive(true);
        chooseTankContainer.SetActive(false);
        waitingForPlayersText.SetActive(false);

        clientState = ClientState.ChoosingTeam;
    }

    public void ReceiveInfo(string message) //ex: INF12110000
    {
        for (int i = 0; i < 4; i++)
        {
            int team = int.Parse(message.Substring(3 + i * 2, 1));
            int tank = int.Parse(message.Substring(4 + i * 2, 1));
            if (team != 0)
            {
                if (team == 1) playersOnTeam1++;
                else playersOnTeam2++;
                players.Add(new PlayerScript(i, team));

                if (tank != 0)
                {
                    PlayerScript player = players.Find(players => players.Id == i);
                    player.SetTank(tank);
                    tankButtons[i - 1].interactable = false;
                }
            }
        }

        UpdateTeamButtons();
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
        teamButtons[0].enabled = false;
        teamButtons[1].enabled = false;
        clientHandler.SendToServer("CTE" + (team));
    }

    public void TeamIsChosen(int team)
    {
        if (team == 1) playersOnTeam1++;
        else playersOnTeam2++;
        UpdateTeamButtons();
    }

    public void ChooseTank(int tank)
    {
        tankButtons[0].enabled = false;
        tankButtons[1].enabled = false;
        tankButtons[2].enabled = false;
        tankButtons[3].enabled = false;
        clientHandler.SendToServer("CTA" + (tank));
    }

    public void TankIsChosen(int tank)
    {
        tankButtons[tank - 1].interactable = false;
    }

    public void OkOrNot(bool ok)
    {
        switch (clientState)
        {
            case ClientState.ChoosingTeam:
                if (ok)
                {
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
                    chooseTankContainer.SetActive(false);
                    waitingForPlayersText.SetActive(true);
                    clientState = ClientState.Playing;
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

    public void MovePlayer(string message) //ex: POS0+5.00-8.00
    {
        int player = int.Parse(message.Substring(3, 1));
        float x = float.Parse(message.Substring(4, 5));
        float y = float.Parse(message.Substring(9, 5));

        players.Find(players => players.Id == player).SetPosition(x, y);
    }

    public void RotatePlayerBase(string message) //ex: ROB0180
    {
        int player = int.Parse(message.Substring(3, 1));
        int angle = int.Parse(message.Substring(4, 3));

        players.Find(players => players.Id == player).SetBaseAngle(angle);
    }

    public void RotatePlayerTurret(string message) //ex: ROT0180
    {
        int player = int.Parse(message.Substring(3, 1));
        int angle = int.Parse(message.Substring(4, 3));

        players.Find(players => players.Id == player).SetTurretAngle(angle);
    }


}
