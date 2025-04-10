/*****************************************************************************
// File Name : PlayerController.cs
// Author : Nicholas Williams
// Creation Date : March 25, 2025
//
// Brief Description : This script handles pretty much everything related to controls and player movement.
*****************************************************************************/
using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] GameObject player;
    private Rigidbody rb;

    [SerializeField] private PlayerInput playerInput;
    private InputAction punchAction;
    private InputAction dashAction;
    private InputAction restartAction;
    private InputAction quitAction;
    private InputAction hideHudAction;

    [SerializeField] private float jumpValue;
    [SerializeField] private float playerSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float boost;
    [SerializeField] private float boostMaxSpeed;
    private float finalMaxSpeed;
    private float timer;
    private bool speedUp;
    private bool slowing;
    private float moveX;
    private float moveZ;
    [SerializeField] private Transform orientation;
    private Vector3 moveDirection;
    private Vector3 flatVel;

    [SerializeField] private GameObject punchHitbox;
    private bool punching;

    private int dashes;
    private int doubleJumps;
    private bool dashing;
    public bool resetDash;
    private bool wallRunning;
    [SerializeField] private bool grounded;
    [SerializeField] private TMP_Text display;
    [SerializeField] private TMP_Text controls;
    [SerializeField] private TMP_Text death;
    private bool HUD;
    [SerializeField] private PhysicMaterial groundMaterial;
    [SerializeField] private Transform spawnPoint;

    /// <summary>
    /// Initializs everything
    /// </summary>
    void Start()
    {
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        playerInput.currentActionMap.Enable();
        punchAction = playerInput.currentActionMap.FindAction("Punch");
        punchAction.started += PunchAction_Started;
        dashAction = playerInput.currentActionMap.FindAction("Dash");
        dashAction.started += DashAction_Started;
        restartAction = playerInput.currentActionMap.FindAction("Restart");
        restartAction.started += RestartAction_Started;
        quitAction = playerInput.currentActionMap.FindAction("Quit");
        quitAction.started += QuitAction_Started;
        hideHudAction = playerInput.currentActionMap.FindAction("HideControls");
        hideHudAction.started += HUDAction_Started;
        finalMaxSpeed = maxSpeed;
        boostMaxSpeed = maxSpeed * boost;
        ControlTextEnable();

        display.text = "Jumps - " + doubleJumps + "<br>Dashes - " + dashes;
        grounded = true;
        resetDash = true;
    }

    /// <summary>
    /// Sets movement input values to floats
    /// </summary>
    /// <param name="iValue"></param>
    void OnMove(InputValue iValue)
    {
        Vector2 inputMovent = iValue.Get<Vector2>();
        moveX = inputMovent.x;
        moveZ = inputMovent.y;
    }

    /// <summary>
    /// Jumps if on ground, double jumps if not
    /// </summary>
    void OnJump()
    {
        if (grounded)
            Jump();
        else
            DoubleJump();
    }

    /// <summary>
    /// Jumps
    /// </summary>
    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpValue, rb.velocity.z);
    }

    /// <summary>
    /// Calls punch method
    /// </summary>
    /// <param name="context"></param>
    private void PunchAction_Started(InputAction.CallbackContext context)
    {
        if (punching == false)
            StartCoroutine(Punch());
    }

    /// <summary>
    /// Calls dash method
    /// </summary>
    /// <param name="context"></param>
    private void DashAction_Started(InputAction.CallbackContext context)
    {
        StartCoroutine(Dash());
    }

    /// <summary>
    /// Reloads the scene
    /// </summary>
    /// <param name="context"></param>
    private void RestartAction_Started(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    /// <param name="context"></param>
    private void QuitAction_Started(InputAction.CallbackContext context)
    {
        Application.Quit();
    }

    /// <summary>
    /// Turns display of controls on and off
    /// </summary>
    /// <param name="context"></param>
    private void HUDAction_Started(InputAction.CallbackContext context)
    {
        if (HUD)
        {
            ControlTextDisable();
        }
        else
        {
            ControlTextEnable();
        }
    }

    /// <summary>
    /// Displays controls
    /// </summary>
    private void ControlTextEnable()
    {
        HUD = true;
        controls.text = "P to hide controls<br>WASD to move<br>Space to jump<br>F to punch<br>" +
            "Punch blocks to gain speed and abilities<br>Punch airborne blocks to bounce upwards<br>" +
            "Shift to dash<br>Run along walls to wall run<br>R to restart<BR>Escape to quit";
    }

    /// <summary>
    /// Disables controls display
    /// </summary>
    private void ControlTextDisable()
    {
        HUD = false;
        controls.text = "P to show controls";
    }

    /// <summary>
    /// Kills the player by making them unable to move and enabling a death screen
    /// </summary>
    public void Kill()
    {
        death.text = "you died lol<br>R to restart";
        maxSpeed = 0;
        doubleJumps = 0;
        dashes = 0;
        jumpValue = 0;
        playerSpeed = 0;
        rb.useGravity = false;
        rb.velocity = new Vector3(0, 0, 0);
    }

    /// <summary>
    /// Enables the arm and hitbox, effectively punching
    /// </summary>
    /// <returns></returns>
    private IEnumerator Punch()
    {
        punching = true;
        punchHitbox.SetActive(true);
        yield return new WaitForSeconds(.33f);
        punching = false;
        punchHitbox.SetActive(false);
    }

    /// <summary>
    /// Calls parry method
    /// </summary>
    public void Parry()
    {
        if (timer == 0)
        {
            timer = timer + 3;
            StartCoroutine(_Parry());
        }
        else
            timer = timer + 3;
    }

    /// <summary>
    /// Doubles player speed for 3 seconds
    /// </summary>
    /// <returns></returns>
    private IEnumerator _Parry()
    {
        speedUp = true;
        maxSpeed = boostMaxSpeed;
        while (timer > 0)
        {
            yield return new WaitForSeconds(1);
            timer--;
        }
        speedUp = false;
    }

    /// <summary>
    /// Bounces player in the air when punching an airborne crystal
    /// </summary>
    public void BounceParry()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpValue * 1.75f, rb.velocity.z);
    }

    /// <summary>
    /// Adds a double jump if the player has less than 3
    /// </summary>
    public void AddJumps()
    {
        if (doubleJumps < 3)
        {
            doubleJumps++;
            display.text = "Jumps - " + doubleJumps + "<br>Dashes - " + dashes;
        }
    }

    /// <summary>
    /// Adds a dash if the player has less than 3
    /// </summary>
    public void AddDashes()
    {
        if (dashes < 3)
        {
            dashes++;
            display.text = "Jumps - " + doubleJumps + "<br>Dashes - " + dashes;
        }
    }

    /// <summary>
    /// Dashes if the player has dashes available and subtracts one
    /// </summary>
    /// <returns></returns>
    private IEnumerator Dash()
    {
        if (dashes > 0 && wallRunning == false)
        {
            dashing = true;
            dashes--;
            display.text = "Jumps - " + doubleJumps + "<br>Dashes - " + dashes;
            Vector3 temp = rb.velocity;
            rb.velocity = transform.forward * maxSpeed * 3f;
            yield return new WaitForSeconds(.25f);
            dashing = false;
            if (resetDash)
                rb.velocity = temp;
        }

    }

    /// <summary>
    /// Double jumps if the player has double jumps available and subtracts one
    /// </summary>
    private void DoubleJump()
    {
        if (doubleJumps > 0 && wallRunning == false)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpValue * 1.5f, rb.velocity.z);
            doubleJumps--;
            display.text = "Jumps - " + doubleJumps + "<br>Dashes - " + dashes;
        }
    }

    /// <summary>
    /// Bounces the player up when they walk into/jump on a mushroom
    /// </summary>
    private void Boioioing()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpValue * 2f, rb.velocity.z);
        StartCoroutine(dashBoing());
    }

    private IEnumerator dashBoing()
    {
        resetDash = false;
        yield return new WaitForSeconds(.4f);
        resetDash = true;
    }

    /// <summary>
    /// When the player is falling fast enough, they can parry the ground to bounce back up
    /// (serves no real purpose i only added it because a friend asked me to ill probably remove it)
    /// </summary>
    public void GravityParry()
    {
        if (rb.velocity.y <= -9f)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.velocity = new Vector3(rb.velocity.x, jumpValue * 3f, rb.velocity.z);
        }
    }

    /// <summary>
    /// Handles grounded state and destroys crystals that the player walks into without granting bonus speed
    /// Also kills the player if they touch a death plane
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
            grounded = true;
        if (collision.gameObject.tag == "Death Plane")
            Kill();
    }

    /// <summary>
    /// Checks if the player is on a wall and not on the ground
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Wall" && grounded == false)
        {
            wallRunning = true;
        }
    }

    /// <summary>
    /// Ungrounds and "unwalls" the player
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
            grounded = false;
        if (collision.gameObject.tag == "Wall")
            wallRunning = false;
    }

    /// <summary>
    /// Calls the boioioing (mushroom bounce) method when the player touches a mushroom
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Mushroom")
            Boioioing();

        if (other.gameObject.tag == "Jump Crystal")
        {
            AddJumps();
            //if (speedUp)
                //timer = timer + 3;
            Destroy(other.gameObject);
        }
        if (other.gameObject.tag == "Dash Crystal")
        {
            AddDashes();
            //if (speedUp)
                //timer = timer + 3;
            Destroy(other.gameObject);
        }
    }

    private IEnumerator loseSpeed()
    {
        slowing = true;
        yield return new WaitForSeconds(0.1f);
        maxSpeed = maxSpeed - 0.35f;
        slowing = false;
    }

    /// <summary>
    /// Most movement stuff is handled in Update() 
    /// It broke when I put it in FixedUpdate()
    /// </summary>
    void Update()
    { 
        if (grounded && !speedUp)
        {
            while (maxSpeed > finalMaxSpeed && !slowing)
            {
                StartCoroutine(loseSpeed());
            }
            if (maxSpeed < 3.5f)
                maxSpeed = 3.5f;
        }

        //When the player is not grounded, the ground has no friction
        //It's reapplied when the player touches the ground to fix a bug
        if (grounded)
        {
            groundMaterial.staticFriction = 4.5f;
            groundMaterial.dynamicFriction = 4.5f;
        }
        else
        {
            groundMaterial.staticFriction = 0f;
            groundMaterial.dynamicFriction = 0f;
        }

        //Wallrunning
        if (wallRunning && flatVel.magnitude >= maxSpeed - .1f && moveZ >= 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        }
    }

    private void FixedUpdate()
    {
        move();
    }


    /* saving this in case i need it later
       gets the player's local forward and sideways velocity
     
        playerForward = player.transform.forward;
        playerSideways = player.transform.right;
        forwardVelocity = playerForward * Vector3.Dot(playerForward, rb.velocity);
        sidewaysVelocity = playerSideways * Vector3.Dot(playerSideways, rb.velocity);
        forwardSpeed = forwardVelocity.magnitude;
        sidewaysSpeed = sidewaysVelocity.magnitude;
    */

    private void move()
    {
        gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x,
            _camera.transform.eulerAngles.y, gameObject.transform.eulerAngles.z);

        //Moves player, more sluggish in the air
        moveDirection = orientation.forward * moveZ + orientation.right * moveX;
        if (grounded)
            rb.AddForce(moveDirection.normalized * playerSpeed * 30f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * playerSpeed * 15f, ForceMode.Force);

        //Hard caps speed
        flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > maxSpeed && !dashing)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
}