using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 10f;
    
    private float horizontalMovement;
    private float verticalMovement;
    
    private Vector3 mousePosition;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        MovePlayer();
        RotatePlayer();
    }

    private void MovePlayer()
    {
        horizontalMovement = transform.position.x + movementSpeed * Input.GetAxisRaw("Horizontal") * Time.deltaTime;
        verticalMovement = transform.position.y + movementSpeed * Input.GetAxisRaw("Vertical") * Time.deltaTime;
        
        transform.position = new Vector2(horizontalMovement, verticalMovement);
    }

    // rotates player to mouse position
    private void RotatePlayer()
    {
        float angle = Mathf.Atan2((mousePosition.y - transform.position.y), (mousePosition.x - transform.position.x)) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
    }
    
}