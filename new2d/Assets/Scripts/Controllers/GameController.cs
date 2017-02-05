using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static PlayerStatsDTO playerStats = new PlayerStatsDTO();
    public GameObject playerObject;

    private static bool _alreadyExists = false;

    private void Start()
    {
        if(_alreadyExists)
        {
            Destroy(this.gameObject);
        } else
        {
            GameController._alreadyExists = true;
            DontDestroyOnLoad(this.gameObject);
            StartCoroutine("SpawnPlayer");
        }
    }

    public void SpawnPlayer()
    {
        playerObject = Instantiate(playerObject, transform.position, transform.rotation);
        playerObject.transform.position = new Vector3(GameController.playerStats.x, GameController.playerStats.y, 0f);
        playerObject.name = GameController.playerStats.id;

        PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
        playerStats.id = GameController.playerStats.id;
        playerStats.playerName = GameController.playerStats.playerName;
        playerStats.x = GameController.playerStats.x;
        playerStats.y = GameController.playerStats.y;
    }
}
