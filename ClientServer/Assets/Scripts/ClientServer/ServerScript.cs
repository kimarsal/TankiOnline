using System;
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
    public List<Mine> mineList;
    public List<ObjetoDestruible> objectList;
    private float timeSinceLastUpdate = 0f;

    #region Lobby

    private void Start()
    {
        serverHandler = GameObject.FindGameObjectWithTag("Handler").GetComponent<ServerHandler>();
        serverHandler.serverScript = this;

        playerKeys = new Dictionary<int, Dictionary<string, bool>>();
        playerMouseCursorPositions = new Dictionary<int, Vector2>();

        bulletList = new List<Bullet>();
        mineList = new List<Mine>();
    }

    public string GetInfo(int id)
    {
        string message = "INF" + id.ToString();
        foreach(KeyValuePair<int, PlayerScript> pair in players)
        {
            message += pair.Key;
            message += pair.Value.TeamId;
            message += pair.Value.TankId;
        }
        for(int i = 0; i < 4 - players.Count; i++)
        {
            message += "000";
        }
        return message;
    }

    public bool ChooseTeam(string message, int from)
    {
        int team = int.Parse(message.Substring(4, 1));
        if (team == 1 && playersOnTeam1 < 2)
        {
            playersOnTeam1++;
            AddPlayer(from, team);
            return true;
        }
        else if (team == 2 && playersOnTeam2 < 2)
        {
            playersOnTeam2++;
            AddPlayer(from, team);
            return true;
        }
        return false;
    }

    private void AddPlayer(int from, int team)
    {
        players.Add(from, new PlayerScript(team));

        playerKeys.Add(from, new Dictionary<string, bool>());
        playerKeys[from].Add("W", false);
        playerKeys[from].Add("A", false);
        playerKeys[from].Add("S", false);
        playerKeys[from].Add("D", false);

        playerMouseCursorPositions.Add(from, Vector2.zero);
    }

    public bool ChooseTank(string message, int from)
    {
        int tank = int.Parse(message.Substring(4, 1));
        int team = players[from].TeamId;
        foreach (KeyValuePair<int, PlayerScript> p in players)
        {
            if (p.Value.TankId == tank) return false;
        }
        players[from].SetTank(tank);

        Transform spawn = team == 1 ? team1Spawns[isTeam1Spawn1Taken ? 1 : 0] : team2Spawns[isTeam2Spawn1Taken ? 1 : 0];
        if (team == 1) isTeam1Spawn1Taken = true;
        else isTeam2Spawn1Taken = true;

        PlayerInput playerInput = Instantiate(tankPrefabs[tank - 1], spawn.position, Quaternion.identity).GetComponent<PlayerInput>();
        playerInput.playerId = from;
        playerInputs.Add(from, playerInput);

        return true;
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

    #endregion

    #region Game

    private void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if(timeSinceLastUpdate > 0.05f)
        {
            timeSinceLastUpdate = 0f;
            UpdateTanks();
            UpdateBullets();
            UpdateMines();
        }

        MoveTanks();
    }

    private void MoveTanks()
    {
        foreach(KeyValuePair<int, PlayerInput> pair in playerInputs)
        {
            int horizontal = 0, vertical = 0;
            if (playerKeys[pair.Key]["W"])
            {
                vertical++;
            }
            if (playerKeys[pair.Key]["A"])
            {
                horizontal--;
            }
            if (playerKeys[pair.Key]["S"])
            {
                vertical--;
            }
            if (playerKeys[pair.Key]["D"])
            {
                horizontal++;
            }

            pair.Value.GetBodyMovement(new Vector2(horizontal, vertical));
            pair.Value.GetTurretMovement(playerMouseCursorPositions[pair.Key]);
        }
    }

    private void UpdateTanks()
    {
        string message = "UDT";
        foreach (KeyValuePair<int, PlayerInput> pair in playerInputs)
        {
            Vector2 pos = pair.Value.tankController.transform.position;
            message += pos.x < 0 ? "-" : "+";
            message += Mathf.Abs(pos.x).ToString("F2").PadLeft(5, '0');
            message += pos.y < 0 ? "-" : "+";
            message += Mathf.Abs(pos.y).ToString("F2").PadLeft(5, '0');

            float angleBase = pair.Value.tankController.transform.rotation.eulerAngles.z % 360;
            if (angleBase < 0) angleBase = 0;
            message += Mathf.FloorToInt(angleBase).ToString().PadLeft(3, '0');
            float angleTurret = pair.Value.tankController.turretParent.rotation.eulerAngles.z % 360;
            if (angleTurret < 0) angleTurret = 0;
            message += Mathf.FloorToInt(angleTurret).ToString().PadLeft(3, '0');
        }
        //message += "+00.00+00.00000000";
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
        }
        if(bulletList.Count > 0) serverHandler.SendToAll(message);
    }

    public void UpdateMines()
    {
        string message = "UDM";
        for (int i = 0; i < mineList.Count; i++)
        {
            Vector2 pos = mineList[i].transform.position;
            message += pos.x < 0 ? "-" : "+";
            message += Mathf.Abs(pos.x).ToString("F2").PadLeft(5, '0');
            message += pos.y < 0 ? "-" : "+";
            message += Mathf.Abs(pos.y).ToString("F2").PadLeft(5, '0');
        }
        if (mineList.Count > 0) serverHandler.SendToAll(message);
    }


    public void TryToShoot(int from)
    {
        if (playerInputs.ContainsKey(from))
        {
            if (playerInputs[from].TryToShoot())
            {
                serverHandler.SendToAll("SHN" + from.ToString());
            }
        }
    }

    public void TryToShootSpecial(int from)
    {
        if (playerInputs.ContainsKey(from))
        {
            if (playerInputs[from].TryToShootSpecial())
            {
                serverHandler.SendToAll("SHS" + from.ToString());
            }
        }
    }

    public void AddBullet(Bullet bullet)
    {
        bulletList.Add(bullet);
    }

    public void AddMine(Mine mine)
    {
        mineList.Add(mine);
    }

    public void RemoveMine(Mine mine)
    {
        mineList.Remove(mine);
    }

    public void InstantiateExplosion(Vector2 pos)
    {
        string message = "IEX";
        message += pos.x < 0 ? "-" : "+";
        message += Mathf.Abs(pos.x).ToString("F2").PadLeft(5, '0');
        message += pos.y < 0 ? "-" : "+";
        message += Mathf.Abs(pos.y).ToString("F2").PadLeft(5, '0');
        serverHandler.SendToAll(message);
    }

    public void BulletIsDestroyed(Bullet bullet)
    {
        int index = bulletList.IndexOf(bullet);
        if(index != -1)
        {
            bulletList.RemoveAt(index);
            Destroy(bullet.gameObject);
            serverHandler.SendToAll("BID" + index.ToString().PadLeft(2, '0'));
        }
    }

    public void ObjectIsDestroyed(ObjetoDestruible objeto)
    {
        int index = objectList.IndexOf(objeto);
        if(index != -1)
        {
            objectList.RemoveAt(index);
            Destroy(objeto.gameObject);
            serverHandler.SendToAll("OID" + index.ToString().PadLeft(2, '0'));
        }
    }

    public void MineIsDestroyed(Mine mine)
    {
        int index = mineList.IndexOf(mine);
        if (index != -1)
        {
            serverHandler.SendToAll("MID" + index.ToString());
        }
    }

    public void TankIsDestroyed(int player)
    {
        if (playerInputs.ContainsKey(player))
        {
            PlayerInput playerInput;
            playerInputs.Remove(player, out playerInput);
            Destroy(playerInput.gameObject);
            serverHandler.SendToAll("TID" + player.ToString());
        }
    }

    #endregion Game

}
