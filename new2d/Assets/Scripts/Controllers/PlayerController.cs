using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private int _lastMoveX = 0;
    private int _lastMoveY = 0;
    private Rigidbody2D _body;

    public PlayerStats stats;

    private static bool _alreadyExists = false;

    private void Start()
    {
        if (PlayerController._alreadyExists)
        {
            Destroy(this.gameObject);
        }
        else
        {
            PlayerController._alreadyExists = true;
            DontDestroyOnLoad(this.gameObject);

            _body = GetComponent<Rigidbody2D>();
            stats = GetComponent<PlayerStats>();
        }
    }

    private void FixedUpdate()
    {
        if (!NetworkController.isConnected)
        {
            return;
        }

        int moveX = Converters.ToValidMoveInput(Input.GetAxisRaw("Horizontal"));
        int moveY = Converters.ToValidMoveInput(Input.GetAxisRaw("Vertical"));

        if(!(_lastMoveX == moveX && _lastMoveY == moveY))
        {
            _lastMoveX = moveX;
            _lastMoveY = moveY;

            if (moveX == 0 && moveY == 0)
            {
                //send end move with id, transform.positiion.x and transform.position.y
                float posX = _body.transform.position.x;
                float posY = _body.transform.position.y;

                stats.x = posX;
                stats.y = posY;

                NetworkController.SendEndMovementMessage(stats.id, posX, posY);
            } else
            {
                //send init move with id, moveX and moveY
                NetworkController.SendMovementMessage(stats.id, moveX, moveY);
            }
        }
    }
}
