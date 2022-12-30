using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerScript : MonoBehaviour
{
    private ServerHandler serverHandler;

    public Dictionary<int, PlayerScript> players = new Dictionary<int, PlayerScript>();
    public Dictionary<int, PlayerInput> playerInputs = new Dictionary<int, PlayerInput>();
    public int playersOnTeam1 = 0;
    public int playersOnTeam2 = 0;

    public Transform[] team1Spawns;
    public Transform[] team2Spawns;
    public GameObject[] tankPrefabs;

    private bool isTeam1Spawn1Taken;
    private bool isTeam2Spawn1Taken;
    private Dictionary<int, Dictionary<string, bool>> playerKeys;
    private Dictionary<int, Vector2> playerMouseCursorPositions;
    public List<Bullet> bulletList;
    private float timeSinceLastUpdate = 0f;

    private void Start()
    {
        serverHandler = GameObject.FindGameObjectWithTag("Handler").GetComponent<ServerHandler>();
        serverHandler.serverScript = this;

        playerKeys = new Dictionary<int, Dictionary<string, bool>>();
        playerMouseCursorPositions = new Dictionary<int, Vector2>();

        for (int i = 1; i <= 4; i++)
        {
            playerKeys.Add(i, new Dictionary<string, bool>());
            playerKeys[i].Add("W", false);
            playerKeys[i].Add("A", false);
            playerKeys[i].Add("S", false);
            playerKeys[i].Add("D", false);

            playerMouseCursorPositions.Add(i, Vector2.zero);
        }

        bulletList = new List<Bullet>();
    }

    private void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if(timeSinceLastUpdate > 0.05f)
        {
            timeSinceLastUpdate = 0f;
            UpdateTanks();
            UpdateBullets();
        }

        int horizontal = 0, vertical = 0;
        for (int i = 1; i <= 4; i++)
        {
            if (playerKeys[i]["W"])
            {
                vertical++;
            }
            if (playerKeys[i]["A"])
            {
                horizontal--;
            }
            if (playerKeys[i]["S"])
            {
                vertical--;
            }
            if (playerKeys[i]["D"])
            {
                horizontal++;
            }
            if (playerInputs.ContainsKey(i)) //Prescindible en el joc final
            {
                playerInputs[i].GetBodyMovement(new Vector2(horizontal, vertical));
                playerInputs[i].GetTurretMovement(playerMouseCursorPositions[i]);
            }
            horizontal = vertical = 0;
        }

        for(int i = 0; i < bulletList.Count; i++)
        {
            bulletList[i].transform.Translate(Vector3.up * bulletList[i].speed * Time.deltaTime);
        }
    }

    private void UpdateTanks()
    {
        string message = "UDT";
        for (int i = 1; i <= 4; i++)
        {
            if (playerInputs.ContainsKey(i)) //Prescindible en el joc final
            {
                Vector2 pos = playerInputs[i].tankBase.position;
                message += pos.x < 0 ? "-" : "+";
                message += Mathf.Abs(pos.x).ToString("F2").PadLeft(5, '0');
                message += pos.y < 0 ? "-" : "+";
                message += Mathf.Abs(pos.y).ToString("F2").PadLeft(5, '0');

                float angleBase = playerInputs[i].tankBase.rotation.eulerAngles.z % 360;
                message += Mathf.FloorToInt(angleBase).ToString().PadLeft(3, '0');
                float angleTurret = playerInputs[i].tankTurret.rotation.eulerAngles.z % 360;
                message += Mathf.FloorToInt(angleTurret).ToString().PadLeft(3, '0');
            }
            else
            {
                message += "+00.00+00.00000000";
            }
        }
        serverHandler.SendToAll(message);
    }

    public void UpdateBullets()
    {
        string message = "UDB";
        for (int i = 0; i < bulletList.Count; i++)
        {
            Vector2 pos = bulletList[i].transform.position;
            message += pos.x < 0 ? "-" : "+";
            message += Mathf.Abs(pos.x).ToString("F2").PadLeft(5, '0');
            message += pos.y < 0 ? "-" : "+";
            message += Mathf.Abs(pos.y).ToString("F2").PadLeft(5, '0');

            float angle = bulletList[i].transform.rotation.eulerAngles.z % 360;
            message += Mathf.FloorToInt(angle).ToString().PadLeft(3, '0');
        }
        if(bulletList.Count > 0) serverHandler.SendToAll(message);
    }

    public bool ChooseTeam(string message, int from){
        int team = int.Parse(message.Substring(4, 1));
        if (team == 1 && playersOnTeam1 < 2){
            playersOnTeam1++;
            players.Add(from, new PlayerScript(team));
            return true;
        }
        else if(team == 2 && playersOnTeam2 < 2){
            playersOnTeam2++;
            players.Add(from, new PlayerScript(team));
            return true;
        }
        return false;
    }

    public bool ChooseTank(string message, int from)
    {
        int tank = int.Parse(message.Substring(4, 1));
        foreach(KeyValuePair<int, PlayerScript> p in players)
        {
            if (p.Value.TankId == tank) return false;
        }
        players[from].SetTank(tank);
        playerInputs.Add(from, Instantiate(tankPrefabs[tank - 1], players[from].TeamId == 1 ? team1Spawns[isTeam1Spawn1Taken ? 1 : 0] : team2Spawns[isTeam2Spawn1Taken ? 1 : 0]).GetComponent<PlayerInput>());
        if (players[from].TeamId == 1) isTeam1Spawn1Taken = true;
        else isTeam2Spawn1Taken = true;
        return true;
    }

    public string GetInfo(int id){
        string message = "INF" + id.ToString();
        for(int i = 1; i <= 4; i++){
            if (players.ContainsKey(i))
            {
                message += players[i].TeamId;
                message += players[i].TankId;
            }
            else
            {
                message += "00";
            }
        }
        return message;
    }

    public void ChangeKeyState(int from, string key, bool pressed)
    {
        playerKeys[from][key] = pressed;
    }

    public void MoveMouseCursor(int from, string message)
    {
        float x = float.Parse(message.Substring(3, 6));
        float y = float.Parse(message.Substring(9, 6));
        playerMouseCursorPositions[from] = new Vector2(x, y);
    }

    public void Shoot(int from)
    {
        if (playerInputs.ContainsKey(from))
        {
            playerInputs[from].Shoot();
            serverHandler.SendToAll("TSH" + from.ToString());
        }
    }

    public void BulletIsDestroyed(Bullet bullet){
        int index = bulletList.IndexOf(bullet);
        bulletList.RemoveAt(index);
        serverHandler.SendToAll("BID"+index);
    }

}
