using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static PlayerStatsDTO playerStats = new PlayerStatsDTO();
    public GameObject playerObject;
    public GameObject otherPlayerObject;

    private static bool _alreadyExists = false;
    public static GameController sharedInstance;
    private void Start()
    {
        if(_alreadyExists)
        {
            Destroy(this.gameObject);
        } else
        {
            GameController._alreadyExists = true;
            sharedInstance = this;
            DontDestroyOnLoad(this.gameObject);

            NetworkController.ListenUserConnectedEvent(onUserConnectedHandler);

            NetworkController.SendGetOtherPlayersMessage(GameController.playerStats.id);

            SpawnPlayer(playerObject, GameController.playerStats);
        }
    }

    private void OnDestroy()
    {
        if(this == GameController.sharedInstance)
        {
            NetworkController.UnlistenUserConnectedEvent();
        }
    }

    private void onUserConnectedHandler(string id, string name, float posX, float posY, Enums.CharacterClass characterClass)
    {
        PlayerStatsDTO stats = new PlayerStatsDTO()
        {
            id = id,
            playerName = name,
            x = posX,
            y = posY,
            characterClass = characterClass
        };
        SpawnPlayer(otherPlayerObject, stats);
    }

    public void SpawnPlayer(GameObject pObject, PlayerStatsDTO stats)
    {
        playerObject = Instantiate(pObject, transform.position, transform.rotation);
        playerObject.transform.position = new Vector3(stats.x, stats.y, 0f);
        playerObject.name = stats.id;

        PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
        playerStats.id = stats.id;
        playerStats.playerName = stats.playerName;
        playerStats.x = stats.x;
        playerStats.y = stats.y;
        playerStats.characterClass = stats.characterClass;
    }
}
