using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ClientScript : MonoBehaviour
{
    private ClientHandler clientHandler;
    public GameObject chooseTeamContainer;
    public GameObject chooseTankContainer;
    public GameObject waitingForPlayersText;
    public Button[] teamButtons;
    public Button[] tankButtons;

    private int playersOnTeam1;
    private int playersOnTeam2;

    private void Start()
    {
        clientHandler = GameObject.FindGameObjectWithTag("Handler").GetComponent<ClientHandler>();
        clientHandler.clientScript = this;

        chooseTeamContainer.SetActive(true);
        chooseTankContainer.SetActive(false);
        waitingForPlayersText.SetActive(false);
    }

    public void SetTeams(int playersOnTeam1, int playersOnTeam2, bool[] takenTanks)
    {
        this.playersOnTeam1 = playersOnTeam1;
        this.playersOnTeam2 = playersOnTeam2;

        for(int i = 0; i < 4; i++)
        {
            tankButtons[i].enabled = !takenTanks[i];
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
        chooseTeamContainer.SetActive(false);
        chooseTankContainer.SetActive(true);
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
        chooseTankContainer.SetActive(false);
        waitingForPlayersText.SetActive(true);
        clientHandler.SendToServer("CTA" + (tank));
    }

    public void TankIsChosen(int tank)
    {
        tankButtons[tank - 1].interactable = false;
    }

    
}
