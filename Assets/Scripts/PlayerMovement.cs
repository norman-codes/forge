using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float runSpeed = 10f; //float to scale the run speed
    [SerializeField] float jumpSpeed = 5f; //float to set the jump speed
    [SerializeField] float climbSpeed = 5f; //float to scale the climb speed
    [SerializeField] Vector2 deathKick = new Vector2 (10f, 10f); //sets vector for death animation
    [SerializeField] GameObject bullet; //object for bullet
    [SerializeField] Transform gun; //transform from where the bullet is being instantiated

    [SerializeField] TMP_InputField inputField;

    Vector2 moveInput; //variable that will store the input that is being given
    Rigidbody2D myRigidbody; //rigidBody variable that represents the character
    Animator myAnimator; //Animator variable that represents the animator
    CapsuleCollider2D myBodyCollider; //var that represents the player collision hurtbox
    BoxCollider2D myFeetCollider; //var that represents the players feet, prevents wall jumping
    float gravityScaleAtStart; //var that will store the gravity constant used
    bool isAlive = true; //is character alive
    bool isInputEnabled;
    private string inputText;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>(); //on init, set variable to the rigidbody component
        myAnimator = GetComponent<Animator>(); //on init, set var to the Animator component, "catch" reference
        myBodyCollider = GetComponent<CapsuleCollider2D>(); //on init, catch capsulecollider ref 
        myFeetCollider = GetComponent<BoxCollider2D>(); //on init, catch boxcollider ref
        gravityScaleAtStart = myRigidbody.gravityScale; //init gravityScale to current player gravity   
        inputField.interactable = false;
    }

    void Update()
    {
        if (!isAlive) {return;} //checks if player is alive
        Run(); //on every update time, run method Run()
        FlipSprite(); //on every update time, flip sprite along y-axis if running the other way
        ClimbLadder(); //on every update time, climb the ladder
        Die(); //calls the die function
        TextInput();
        GetTextInput();
    }

    void OnFire(InputValue value) //method called when bullet is fired
    {
        if (!isAlive) {return;}
        Instantiate(bullet, gun.position, transform.rotation);
    }
 
    void OnMove(InputValue value) //method to store the input into the Vector2 variable we created, the log it
    {
        if (!isAlive) {return;} //checks if player is alive
        moveInput = value.Get<Vector2>(); //gets the value, stores it into moveInput
    }

    void OnJump(InputValue value)
    {
        if (!isAlive) {return;} //checks if player is alive
        if(!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"))) {return;} //if not touching ground, dont proceed
        if(value.isPressed) //is the jump button pressed?
        {
            myRigidbody.velocity += new Vector2 (0f, jumpSpeed);
        }
    }
    void Run() //lets the player move horizontally
    {
        Vector2 playerVelocity = new Vector2 (moveInput.x * runSpeed, myRigidbody.velocity.y); //sets x and y movement speed, for x move runSpeed times faster, for y just keep same velocity you currently have
        myRigidbody.velocity = playerVelocity; //set velocity of player to the playerVelocity value
        
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon; //checks if player is moving or not
        myAnimator.SetBool("isRunning", playerHasHorizontalSpeed); //sets the isRunning bool based on if player is moving or not
    }

    void FlipSprite() //flips the player sprite
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon; //checks if player is moving or not
        if (playerHasHorizontalSpeed) //if player is moving, transform
        {
            transform.localScale = new Vector2 (Mathf.Sign(myRigidbody.velocity.x), 1f); //gets sign based on x velocity, uses it to transform the scale 
        }
    }

    void ClimbLadder() //climbs the ladder. Added note, set as trigger to prevent collision with player
    {
        if(!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing"))) //if not touching ground, dont proceed
        {
            myRigidbody.gravityScale = gravityScaleAtStart; //set gravity back to init gravity
            myAnimator.SetBool("isClimbing", false); //isClimbing set to false
            return;
        }
        
        Vector2 climbVelocity = new Vector2 (myRigidbody.velocity.x, moveInput.y * climbSpeed); //sets x and y movement speed, for x move runSpeed times faster, for y just keep same velocity you currently have
        myRigidbody.velocity = climbVelocity; //set velocity of player to the playerVelocity value
        myRigidbody.gravityScale = 0f; //set gravity to 0 to prevent falling

        bool playerHasVerticalSpeed = Mathf.Abs(myRigidbody.velocity.y) > Mathf.Epsilon; //checks if player is moving or not
        myAnimator.SetBool("isClimbing", playerHasVerticalSpeed); //sets isClimbing to if player is moving up ladder or not

    }

    void Die()
    {
        if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Hazards"))) //is player touching enemy?
        {
            isAlive = false; //no longer alive
            myAnimator.SetTrigger("Dying"); //trigger dying state
            myRigidbody.velocity = deathKick; //trigger death animation
            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }

    void TextInput()
    {
        Collider2D[] colliders = Physics2D.OverlapCapsuleAll(transform.position, new Vector2(1f, 2f), CapsuleDirection2D.Horizontal, 0f, LayerMask.GetMask("anvil"));

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("anvil") && Input.GetKeyDown(KeyCode.T))
            {
                isInputEnabled = !isInputEnabled; // Toggle text input state
                if (isInputEnabled)
                {         
                    inputField.interactable = true;   
                    inputField.text = "";
                    string text = inputField.text;
                    Console.WriteLine(text);
                }
            }
            //inputField.interactable = false;
            string text1 = inputField.text;
            Console.WriteLine(text1);
        }
        string text2 = inputField.text;
        Console.WriteLine(text2);
    }

    void GetTextInput()
    {
        if(Input.GetKeyDown(KeyCode.Return) && inputField.text != "")
        {
            Console.WriteLine(inputField.text);
        }
    }
}
