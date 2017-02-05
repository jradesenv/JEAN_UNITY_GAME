using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {

    private Rigidbody2D objectToMove;
    private BaseEntity baseEntity; 
    private Vector2 velocity;
    private float speed = 6;
    //private bool isMovingLeft = false;
    //private bool isMovingRight = false;
    //private bool isMovingUp = false;
    //private bool isMovingDown = false;

    private int moveX = 0;
    private int moveY = 0;

    // Use this for initialization
    void Start() {
        objectToMove = GetComponent<Rigidbody2D>();
        baseEntity = GetComponent<BaseEntity>();
        NetworkController.ListenObjectMovement(baseEntity.id, OnMovementReceiveHandler, OnEndMovementReceiveHandler);
    }

    void OnDestroy()
    {
        NetworkController.UnlistenObjectMovement(baseEntity.id);
    }

    private void OnMovementReceiveHandler(int newMoveX, int newMoveY)
    {
        this.moveX = newMoveX;
        this.moveY = newMoveY;
    }

    private void OnEndMovementReceiveHandler(float posX, float posY)
    {
        this.moveX = 0;
        this.moveY = 0;
        this.objectToMove.transform.position = new Vector2(posX, posY);
    }

    void FixedUpdate()
    {
        Vector2 input = new Vector2(moveX, moveY);
        Vector2 direction = input.normalized;
        velocity = direction * speed;
        objectToMove.position += velocity * Time.fixedDeltaTime;
    }
}
