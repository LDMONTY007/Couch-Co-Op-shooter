using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//Something I need to do is add a ton of
//actions that can be subscribed to which
//represent any of the different states and 
//actions that can occur to the player,
//so for example if the player is getting revived
//it'll inform the audio director of that and
//the players will comment on it.
//this will also allow us to do GOAP with the
//player so we can have AI players who know
//that is occuring so they don't have to deal with
//it as it's already being dealt with.

public class PlayerController : MonoBehaviour, IDamageable, IInteractible
{
    #region Hotbar

    [HideInInspector]
    public UnityEvent<int> onSlotValueChanged;

    //automatically loop slots
    private int _curSelectedSlot = 0;
    public int curSelectedSlot 
    { 
        get {  return _curSelectedSlot; } 
        set { 
            if (value > 4) 
            { _curSelectedSlot = 0; } 
            else if (value < 0) 
            { _curSelectedSlot = 4; } 
            else 
            { _curSelectedSlot = value; } 
            onSlotValueChanged.Invoke(_curSelectedSlot); } }


    public IUseable curUseable = null;

    public IUseable[] useables = new IUseable[5];

    #endregion

    #region score tracking
    private int _score = 0;

    //Make getters and setters in case we need
    //to subscribe to when this is modified. 
    public int score { get { return _score; } set { _score = value; } }

    private int _zombieKillCount = 0;

    //Make getters and setters in case we need
    //to subscribe to when this is modified. 
    public int zombieKillCount { get { return _zombieKillCount; }
        set 
        {
            //Update the score with the newly added zombies as part of it.
            score += (value - _zombieKillCount) * 5;

            _zombieKillCount = value;
            //Update the visual label for the zombie kill count.
            uiController.UpdateZombieKills(value);
            
        }
    }


    #endregion

    public Transform knockedCamTransform;
    public Transform defaultCamTransform;
    public GameObject standingModel;
    public GameObject knockedModel;


    public float moveSpeed = 5f;
    public float maxSpeed = 20f;

    Guid guid;

    Vector2 moveInput;

    float mouseLookSpeed = 10f;
    float controllerLookSpeed = 100f;
    Vector2 curLook;

    public float interactDist = 7.5f;

    public Renderer playerModel;

    private CharacterController controller;

    public Camera cam;

    public PrimaryWeapon curPrimaryWeapon;
    public SecondaryWeapon curSecondaryWeapon;
    public Throwable curThrowable;

    public Appliable curAppliable;
    public Consumable curConsumable;

    public Transform handTransform;
    public Transform backTransform;

    private PlayerInput playerInput;
    private InputAction jumpAction;
    private InputAction lookAction;
    private InputAction attackAction;
    private InputAction interactAction;
    private InputAction slotUpAction;
    private InputAction slotDownAction;

    float yVel = 0f;

    bool grounded = false;

    public PlayerUIController uiController;

    Rigidbody rb;

    private bool canMove = true;

    #region health vars
    [Header("Health Variables")]

    //This has to be a const so we can init the field with it properly.
    public const float maxHealth = 100;

    private float _curHealth = maxHealth;

    public float curHealth
    {
        get
        {
            return _curHealth;
        }

        set
        {
            _curHealth = Mathf.Max(value, 0);
            //LD Montello
            //Update the current health in the UI for the player.
            UpdateHealthUI();
            if (curHealth <= 0)
            {
                BecomeKnocked();
            }
        }
    }

    private float _knockedHealth = 300;

    public float knockedHealth
    {
        get
        {
            return _knockedHealth;
        }

        set
        {
            _knockedHealth = Mathf.Max(value, 0);
            //LD Montello
            //Update the current health in the UI for the player.
            UpdateHealthUI();
            if (_knockedHealth <= 0)
            {
                Die();
            }
        }
    }

    #endregion

    public bool invincible = false;

    private bool _stunned = false;

    //used after being hit to prevent the player from attacking immediately.
    private bool stunned
    {
        get
        {
            return _stunned;

        }
        set
        {
            //Update the UI for the stunned popup.
            //UIManager.Instance.playerUIManager.UpdateStunnedPopup(value);
            _stunned = value;
        }
    }

    private bool _knocked = false;

    //used after being hit to prevent the player from attacking immediately.
    private bool knocked
    {
        get
        {
            return _knocked;

        }
        set
        {
            //Update the UI for the stunned popup.
            //UIManager.Instance.playerUIManager.UpdateStunnedPopup(value);
            _knocked = value;
        }
    }

    [Header("Jump Parameters")]
    //this is just a rule to see if the player is allowed to jump.
    //that way during certain attacks we can disable it.
    public bool canJump = true;
    public float groundCheckDist = 0.1f;
    public int jumpCount = 1;
    public int jumpTotal = 1;
    [SerializeField] private bool jumpCanceled;
    [SerializeField] private bool jumping;
    public float jumpHeight = 5f;
    [SerializeField] private float buttonTime;
    [SerializeField] private float jumpTime;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float multiplier = 100f;
    public float timeToApex = 0.01f;
    public float timeToFall = 0.5f;

    //was the player launched?
    public bool didLaunch = false;

    //The gravity we return to 
    //after modifying gravity.
    float baseGravity = 9.81f;
    float gravity = 9.81f;
    float fallGravity = 9.81f;

    public bool useGravity = true;

    public bool doJump;

    /* private Coroutine jumpCoroutine;

     public bool jumpPressed = false;

     public bool isJumping = false;*/

    public Collider playerCollider;

    public bool isGrounded = false;

    public bool inAir => !jumping && !isGrounded;

    public Vector3 desiredMoveDirection;

    public Vector2 accumulatedVelocity = Vector2.zero;

    bool isOnWall = false;

    bool didLand = true;

    public float groundCheckScale = 0.8f;

    int playerMask;

    //coroutine references for ensuring no duplicates
    Coroutine iFramesRoutine = null;

    //terraria uses this number for iframes as do most games.
    public float iFrameTime = 0.67f;

    public void Die()
    {
        //TODO: Code dying.
        Debug.Log("DEAD");
        Destroy(gameObject);
    }

    public void init(Guid id, int index, Material mat, PlayerInfo p)
    {
        //Update UI label.
        uiController.UpdateIDLabel(index);

        guid = id;
        playerModel.material = mat;


        zombieKillCount = p.zombieKillCount;


    }

    bool didLoad = false;

    private void Awake()
    {

        playerMask = ~LayerMask.GetMask("Player", "IgnoreRaycast");

        rb = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        jumpAction = playerInput.actions["Jump"];
        lookAction = playerInput.actions["Look"];
        attackAction = playerInput.actions["Attack"];
        interactAction = playerInput.actions["Interact"];
        slotUpAction = playerInput.actions["SlotUp"];
        slotDownAction = playerInput.actions["SlotDown"];
    }

    public void HandleSlots()
    {
        if (slotUpAction.GetButtonDown())
        {
            curSelectedSlot++;
            //repeat last action until
            //there is not an empty slot.
            //this basically skips over any empty slots.
            while (useables[curSelectedSlot] == null)
            {
                curSelectedSlot++;
            }
        }
        
        if (slotDownAction.GetButtonDown())
        {
            curSelectedSlot--;
            //repeat last action until
            //there is not an empty slot.
            //this basically skips over any empty slots.
            while (useables[curSelectedSlot] == null)
            {
                curSelectedSlot--;
            }
        }

        switch (curSelectedSlot)
        {
            case 0:
                SwapCurrentUseable(curPrimaryWeapon);
                break;
            case 1:
                SwapCurrentUseable(curSecondaryWeapon);
                break;
            case 2:
                SwapCurrentUseable(curThrowable);
                break;
            case 3:
                SwapCurrentUseable(curAppliable);
                break;
            case 4:
                SwapCurrentUseable(curConsumable);
                break;


        }
    }

    public void OnUseableDestroyed(IUseable useable, int slot)
    {
        Debug.Log("DESTROYED");

        //Renove the slot icon for the current slot where the item was destroyed.
        uiController.OnUpdateSlotIcon(null, slot);

        //Cast to component to do a proper reference comparison.
        if ((curUseable as Component) == (useable as Component))
        {
            //Go back to slot 0 when the currently held useable is destroyed.
            curSelectedSlot = 0;
            //Make sure to swap the current useable and
            //don't try to access the useable that was destroyed.
            SwapCurrentUseableIgnoreLastUseable(curPrimaryWeapon);

            
        }
    }

    public void SwapCurrentUseable(IUseable useable)
    {
        //if this useable is null don't finish executing this method.
        //We cast to Component as null check won't work correctly otherwise.
        if ((useable as Component) == null)
        {
            return;
        }

        //make sure the curUseable wasn't destroyed before trying
        //to set it's position.
        if (curUseable != null && (curUseable as Component) != null)
        //any weapon not currently held in the players
        //hand is on their back.
        (curUseable as Component).transform.SetParent(backTransform, false);
        //Put the new useable in the player's hand.
        (useable as Component).transform.SetParent(handTransform, false);
        //set current useable.
        curUseable = useable;
    }

    public void SwapCurrentUseableIgnoreLastUseable(IUseable useable)
    {
        //if this useable is null don't finish executing this method.
        //We cast to Component as null check won't work correctly otherwise.
        if ((useable as Component) == null)
        {
            return;
        }

        //Put the new useable in the player's hand.
        (useable as Component).transform.SetParent(handTransform, false);
        //set current useable.
        curUseable = useable;
    }

    public void HandleUI()
    {
        //Set revive panel visibility.
        uiController.ShowRevivePanel(isGettingRevived);
    }

    public void UpdateHealthUI()
    {
        //Update the health bar.
        uiController.UpdateHealthBar();
    }

    public void OnMove(InputAction.CallbackContext callback)
    {
        moveInput = callback.ReadValue<Vector2>();
    }

    private void GroundedCheck()
    {
        #region isGroundedCheck
        //isGrounded = Physics.BoxCast(transform.position, this.GetComponent<Collider>().bounds.size, -transform.up, Quaternion.identity, groundCheckDist, playerMask);
        //isGrounded = Physics.Raycast(transform.position, -transform.up, this.GetComponent<Collider>().bounds.extents.y + groundCheckDist, playerMask);
        Collider[] colliders = Physics.OverlapBox(transform.position + (-transform.up * this.GetComponent<Collider>().bounds.size.y / 2) + (-transform.up * groundCheckDist), new Vector3(GetComponent<Collider>().bounds.size.x * groundCheckScale, 0.1f, GetComponent<Collider>().bounds.size.z * groundCheckScale), transform.rotation, playerMask);
        if (colliders.Length > 0)
        {
            //if we were jumping or in the air,
            //then we landed.
            if (!didLand)
            {
                didLand = true;
                OnLanded();
            }

            isGrounded = true;
            //Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red, 1f);

            //Call on landed.

        }
        else
        {
            //when we are no longer grounded,
            //say that we didn't land.
            if (didLand == true)
            {
                didLand = false;
            }

            isGrounded = false;
        }
        /*isGrounded = Physics.BoxCast(transform.position, this.GetComponent<Collider>().bounds.size, -transform.up, out RaycastHit hitInfo, Quaternion.identity, this.GetComponent<Collider>().bounds.extents.y + groundCheckDist, playerMask);*/

        //Ray ray = new Ray(transform.position, -transform.up);
        //isGrounded = GetComponent<Collider>().Raycast(ray, out RaycastHit hitinfo, groundCheckDist);
        #endregion
    }

    private void JumpUpdateLogic()
    {
        doJump |= (jumpAction.WasPressedThisFrame() && jumpCount > 0 && !jumping && !stunned && canJump);

        if (isGrounded)
        {
            //if we were launched, say we are no longer launched.
            if (didLaunch)
            {
                didLaunch = false;
            }

            //reset jump count and jump canceled, and gravity
            //when not jumping and grounded.
            if (!jumping && didLand)
            {
                jumpCount = jumpTotal;
                jumpCanceled = false;
                //set gravity back to base.
                gravity = baseGravity;
                //Debug.Log("BACK TO BASE".Color("Green"));
                //animator.SetTrigger("landing");


            }
            //reset dash count when grounded, and not dashing.
/*            if (!dashing)
            {
                dashCount = dashTotal;
            }*/
        }

        //increase jump time while jumping
        if (jumping)
        {
            jumpTime += Time.deltaTime;
        }

        if (jumping && !jumpCanceled)
        {
            //If we stop giving input for jump cancel jump so we can have a variable jump.
            //Also if we were launched, ignore this check.
            if (!jumpAction.IsPressed() && !didLaunch)
            {
                jumpCanceled = true;
                Debug.Log("JUMP CANCELED".Color("Orange"));
                //gravity = fallGravity;
            }

            //This check should execute even when launched because
            //it handles knowing when we've reached the "apex" of our jump/arc. 
            //When we reach our projected time stop jumping and begin falling.
            if (jumpTime >= buttonTime)
            {
                Debug.Log("JUMP CANCELED BY BUTTON TIME".Color("Green"));
                //pause the editor
                //Debug.Break();
                jumpCanceled = true;
                Debug.Log("JUMP CANCELED".Color("Orange"));
                //set gravity back to fall gravity
                gravity = fallGravity;
                //gravity = baseGravity;

                //if we were launched, say we are no longer launched.
                if (didLaunch)
                {
                    didLaunch = false;
                }

                //jumpDist = Vector2.Distance(transform.position, ogJump); //Not needed, just calculates distance from where we started jumping to our highest point in the jump.
                //jumpDist = transform.position.y - ogJump.y;
            }
        }

        if (jumpCanceled)
        {
            jumping = false;
            Debug.Log("JUMP CANCELED".Color("Red"));
        }
    }

    public void OnLanded()
    {
        Debug.Log("Landed");
        //TODO:
        //Play landing particles.
    }

    private void HandleLook()
    {
        if (knocked)
        {
            cam.transform.position = knockedCamTransform.position;
        }
        else
        {
            cam.transform.position = defaultCamTransform.position;
        }

            var mouseInput = lookAction.ReadValue<Vector2>();

        //apply mouse input to our current look vector using deltaTime and look speed.
        curLook.y -= mouseInput.y * GetCorrespondingLookSensitivity(playerInput.GetDevice<InputDevice>()) * Time.deltaTime;
        curLook.x += mouseInput.x * GetCorrespondingLookSensitivity(playerInput.GetDevice<InputDevice>()) * Time.deltaTime;

        //clamp to max and min look angle.
        curLook.y = Mathf.Clamp(curLook.y, -80f, 80f);

        //rotate the camera up and down
        cam.transform.localRotation = Quaternion.Euler(curLook.y, 0f, 0f);

        //set rotation for the body.
        //transform.localRotation = Quaternion.Euler(0f, curLook.x, 0f);
    }

    public void HandleAttack()
    {
        if (attackAction.GetButtonDown())
        {
            if (curUseable != null)
            {
                //if the appliable is selected,
                //make sure to assign a reference
                //to the target player.
                if (curSelectedSlot == 3)
                {
                    Ray r = new Ray(cam.transform.position, cam.transform.forward);
                    //We don't use player mask because players are also interactibles.
                    //so make sure we raycast from the player's collider instead.
                    if (Physics.Raycast(r, out var hitInfo, interactDist))
                    {
                        //check that we're looking at a player.
                        PlayerController target = hitInfo.transform.gameObject.GetComponent<PlayerController>();
                        if (target != null)
                        {
                            //assign the target
                            (curUseable as Appliable).targetPlayer = target;
                            //call use.
                            curUseable.Use();
                        }
                    }


                }
                //for any other slot just use the item.
                else
                {
                    //call use.
                    curUseable.Use();
                }


            }
            else
            {
                //Throw error to console if there was no weapon assigned yet.
                Debug.LogWarning("No useable has been assigned to this player. Player " + guid.ToString());
            }
        }
        else if (!attackAction.GetButton())
        {
            if (curSelectedSlot == 3)
            {
                curUseable.CancelUse();
            }
        }

        
        
    }

    public void HandleJumping()
    {
        //if the player can't move
        //return here.
        if (!canMove)
        {
            return;
        }

        /*        if (dashing)
                {
                    return;
                }*/

        if (doJump)
        {
            //if we were launched, say we are no longer launched,
            //this lets us start a jump right after being launched.
            if (didLaunch)
            {
                didLaunch = false;
            }

            //say we didn't yet land.
            didLand = false;

            //I did the work out and 2 * h / t = gravity so I'm going to do that.
            gravity = 2 * jumpHeight / timeToApex;
            fallGravity = 2 * jumpHeight / timeToFall;

            float projectedHeight = timeToApex * gravity / 2f;
            Debug.Log(timeToApex + " " + projectedHeight + " " + gravity);
            Debug.Log(("Projected Height " + projectedHeight).ToString().Color("Cyan"));

            doJump = false;
            jumpCount--;
            float jumpForce;

            jumpForce = Mathf.Sqrt(2f * gravity * jumpHeight) * rb.mass; //multiply by mass at the
            //end so that it reaches the height regardless of weight.

            //divide by 2 so we get the amount of time to reach the apex of the jump.
            buttonTime = (jumpForce / (rb.mass * gravity));
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            jumpTime = 0;
            jumping = true;
            jumpCanceled = false;

            //invoke OnJump if methods are subscribed to it.
            //OnJump?.Invoke();
        }

        //Where I learned this https://www.youtube.com/watch?v=7KiK0Aqtmzc
        //This is what gives us consistent fall velocity so that jumping has the correct arc.
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);

        if (localVel.y < 0 && inAir) //If we are in the air and at the top of the arc then apply our fall speed to make falling more game-like
        {
            //animator.SetBool("falling", true);
            //we don't multiply by mass because forceMode2D.Force includes that in it's calculation.
            //set gravity to be fallGravity.
            gravity = fallGravity;
            Vector3 jumpVec = -transform.up * (fallMultiplier - 1)/* * 100f * Time.deltaTime*/;
            rb.AddForce(jumpVec, ForceMode.Force);
        }
    }


    //this is used when we want to forcefully stop
    //jumping.
    public void StopJumping()
    {

        jumping = false;


        jumpCanceled = false;

        //set gravity back to fall gravity
        gravity = fallGravity;

        //set player y velocity to 0
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    }

    //used when doing heavy attacks which will propel the player,
    //for example a heavy downward slash from the sword will push the player
    //up into the air without counting as a jump.
    //TODO: Make this more configureable than it is with the time code from the jump function
    public void LaunchPlayer(Vector3 direction, float height = 30f, float timeToApex = 1, float timeToFall = 2)
    {

        //I did the work out and 2 * h / t = gravity so I'm going to do that.
        gravity = 2 * height / timeToApex;
        fallGravity = 2 * height / timeToFall;

        float projectedHeight = timeToApex * gravity / 2f;
        Debug.Log(timeToApex + " " + projectedHeight + " " + gravity);
        Debug.Log(("Projected Height " + projectedHeight).ToString().Color("Cyan"));

        doJump = false;
        //jumpCount--;
        float launchForce;

        launchForce = Mathf.Sqrt(2f * gravity * jumpHeight) * rb.mass; //multiply by mass at the
                                                                       //end so that it reaches the height regardless of weight.

        //divide by 2 so we get the amount of time to reach the apex of the jump.
        buttonTime = (launchForce / (rb.mass * gravity));
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(direction * launchForce, ForceMode.Impulse);
        jumpTime = 0;
        jumping = true;
        jumpCanceled = false;
        didLand = false;

        //Say we launched the player
        didLaunch = true;

        //rb.AddForce(direction.normalized * force, ForceMode.Impulse);
    }

    public void HandleMovement()
    {
        //if the player can't move
        //return here.
        if (!canMove)
        {
            return;
        }

/*        if (dashing)
        {
            return;
        }*/

        //We need to store the accumulated velocity and just take that and use vector projection on the normalized input to 
        //get some sort of accumulation going where we can just have the actual speed be moving in the direction of our input.
        //moveVector = (transform.forward * moveInput.y * lastVel.z) + (transform.right * moveInput.x * lastVel.x);

        //remove the player's addtion from the current velocity. We want the player's input to be constant, not a part of the force.
        //this means we have to remove it before doing anything else. 
        //if (moveInput.sqrMagnitude > 0) //only decrease if we want to move again. 
        //rb.linearVelocity -= lastMoveVector;

        //if (moveInput.sqrMagnitude > 0) //only decrease if we want to move again. 
        //Set last Move Vector so we can use it later. 
        //lastMoveVector = moveVector;

        //rb.linearVelocity += moveVector;

        //when stunned don't override x and y values
        //because the player is stunned so they can't move.
        if (!stunned)
        {
            //Axis aligned move, aligned with body axes via projection.
            Vector3 aaMove = (transform.forward.normalized * moveInput.y) + (transform.right.normalized * moveInput.x);

            //Add the y velocity for jumping to the movement.
            Vector3 finalMove = /*transform.up * yVel + */aaMove.normalized * moveSpeed;

            #region constant movement
            //Store yVelocity
            float yVel = rb.linearVelocity.y;
            rb.linearVelocity = finalMove;
            //Restore yVelocity
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, yVel, rb.linearVelocity.z);
            #endregion
        }



        ////The dot product check is for when we are
        ////turning on a dime and our velocity is the opposite direction
        ////of our desired velocity. 
        //if (Vector3.Dot(desiredMoveDirection, rb.linearVelocity) < 0 || moveInput.magnitude == 0 && /*!grappling.IsGrappling() &&*/ !isJumping && isGrounded && /*!dashPressed &&*/ !jumpPressed)
        //{
        //    //rb.linearVelocity *= 0;
        //    //Slow down very quickly but still make it look like it was gradual.
        //    if (slowToStopCoroutine == null)
        //    {
        //        slowToStopCoroutine = StartCoroutine(slowToStop());
        //    }
        //}
        ////Instant velocity changes depending on speed.
        ////When we start moving we want to immediately go 
        ////to our base walking speed so velocity change.
        ////Then from there on out we slowly approach running.
        //else if (isGrounded && /*!grappling.IsGrappling() &&*/ !isJumping && !jumpPressed)
        //{
        //    // Calculate normalized time for acceleration and deceleration
        //    // float accelerationTime = currentSpeed / maxSpeed;
        //    // float decelerationTime = 1f - accelerationTime;
        //    float accelerationTime = currentInputMoveTime / timeToMaxSpeed;
        //    float decelerationTime = 1f - accelerationTime;

        //    // Apply custom acceleration curve
        //    currentSpeed = accelerationCurve.Evaluate(rb.linearVelocity.magnitude/*accelerationTime*/);

        //    // Apply custom speed curve
        //    // currentSpeed = speedCurve.Evaluate(accelerationTime);

        //    Debug.DrawRay(transform.position, transform.TransformDirection(rb.linearVelocity), Color.blue);

        //    //Vector3 targetVelocity = desiredMoveDirection.normalized * maxSpeed;

        //    // Evaluate speed using animation curve, they highest value in the curve is 1 so 1 * maxspeed = maxspeed. 
        //    // This is how we gradually approach our maxSpeed.
        //    //float targetSpeed = speedCurve.Evaluate(rb.linearVelocity.magnitude / maxSpeed) * maxSpeed;
        //    Vector3 targetVelocity = desiredMoveDirection.normalized * currentSpeed;
        //    //Vector3 targetVelocity = desiredMoveDirection.normalized * maxSpeed;

        //    //AccelerateToward(targetVelocity);
        //    // Determine current acceleration based on current speed
        //    //float acceleration = Mathf.Lerp(initialAcceleration, maxAcceleration, rb.linearVelocity.magnitude / maxSpeed);

        //    // Calculate current velocity in desired direction
        //    //Vector3 currentVelocity = Vector3.Project(rb.linearVelocity, new Vector3(moveInput.x, 0f, moveInput.y));

        //    //CounterVelocity();
        //    //Doing targetVelocity - rb.linearVelocity * rb.mass / Time.fixedDeltaTime gives us an acceleration of sorts.
        //    //This is what makes it so no matter how fast you turn the velocity isn't decreased.
        //    //The LateUpdate() call is what sets the direction of velocity to always face the direction we are wanting
        //    //to move. 

        //    //The only problem here is doing targetVelocity - rb.linearVelocity basically gets rid of any speed generated
        //    //by doing a grapple or dash. We want to perserve it somehow.
        //    //targetVelocity = targetVelocity.normalized * (targetVelocity.magnitude + (rb.linearVelocity.magnitude));
        //    Vector3 force = (targetVelocity/* - rb.linearVelocity*/)/* * rb.mass / Time.fixedDeltaTime*/;
        //    //force *= acceleration;
        //    //force = Vector3.ClampMagnitude(force, maxSpeed);
        //    //rb.AddForce(force, ForceMode.VelocityChange);
        //    if (slowToStopCoroutine == null)
        //        rb.AddForce(force, ForceMode.Force);



        //    //rb.AddForce(movement, ForceMode.VelocityChange);
        //    //rb.AddForce(moveVector * movementMultiplier, ForceMode.Force);
        //    //rb.AddForce(AccumulatedVelocity, ForceMode.VelocityChange);
        //}
        //else
        //{
        //    if (slowToStopCoroutine == null)
        //        rb.AddForce(moveVector);
        //}

        //rb.AddForce(moveVector - GetComponent<Rigidbody>().velocity, ForceMode.VelocityChange);
        //rb.AddForce(moveVector);

        //rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);


    }

    public void HandleInteract()
    {
        //call when the interactAction is pressed down this frame (Not continuously held)
        if (interactAction.GetButtonDown())
        {
            Debug.Log("INTERACT!!!");
            OnInteract();
        }
        //if we're holding down the interact button.
        if (interactAction.GetButton() && !isHoldingInteract)
        {
            OnInteractHeld();
        }
        if (!interactAction.GetButton() && isHoldingInteract)
        {
            CancelInteractHold();
        }

    }

    private void HandleAnimations()
    {
        //Use the knocked model when knocked.
        if (knocked)
        {
            knockedModel.SetActive(true);
            standingModel.SetActive(false);
        }
        //Use the standing model when standing normally.
        else
        {
            knockedModel.SetActive(false);
            standingModel.SetActive(true);
        }
    }

    private void OnEnable()
    {
       

        //Add listener so the slot animation will work.
        if (uiController != null)
        {
            onSlotValueChanged.AddListener(uiController.OnSlotSwitched);
        }
    }

    private void OnDisable()
    {
        

        //Remove UI listener for the slot switching animation.
        if (uiController != null)
        {
            onSlotValueChanged.RemoveListener(uiController.OnSlotSwitched);
        }
    }

    private void Update()
    {
        

        HandleLook();

        HandleSlots();

        HandleAttack();

        HandleInteract();
        
        GroundedCheck();

        JumpUpdateLogic();


        HandleAnimations();

        HandleUI();

        //Move the character controller.
        //controller.Move(finalMove * Time.deltaTime);


    }

    private void FixedUpdate()
    {
        HandleMovement();
        
        HandleJumping();

        //set rotation for the body.
        rb.MoveRotation(Quaternion.Euler(0f, curLook.x, 0f));

        ApplyFinalMovements();
    }

    public void HandleGravity()
    {
        if (useGravity)
        {
            //Apply gravity, because gravity is not affected by mass and 
            //we can't use ForceMode.acceleration with 2D just multiply
            //by mass at the end. It's basically the same.
            //In unity it factors in mass for this calculation so 
            //multiplying by mass cancels out mass entirely.
            rb.AddForce(-transform.up * gravity * rb.mass);
        }

    }

    /// <summary>
    /// Called in late update, should only contain applications that occur after we are done calculating physics. 
    /// I.E. if we ended up doing custom gravity, call it here.
    /// </summary>
    public void ApplyFinalMovements()
    {
        //We need to check that the desiredMoveDirection vector isn't zero because otherwise it can zero out our velocity.
        if (isGrounded && /*!dashing &&*/ !jumping && /*!grappling.IsGrappling() && */desiredMoveDirection.normalized.sqrMagnitude > 0 && !stunned)
        {
            // Set the velocity directly to match the desired direction
            // Don't clamp the speed anymore as there isn't a good reason to do so.
            // Don't override the Y velocity.

            //store the current y velocity
            float tempY = rb.linearVelocity.y;
            //remove the Y value from velocity before we apply that to the forward momentum so we don't "steal" values from the vertical
            //momentum and add them to the forward momentum.
            Vector3 velWithoutY = rb.linearVelocity - new Vector3(0f, tempY, 0f);

            //Clamp without y to not prevent jump speed from slowing down,
            //just clamp xz plane movement.
            velWithoutY = Vector3.ClampMagnitude(velWithoutY, maxSpeed);

            //when turning on a dime instantly stop moving before continuing.
            if (Vector3.Dot(desiredMoveDirection, rb.linearVelocity) < 0)
            {
                rb.linearVelocity = new Vector3(0f, tempY, 0f);
            }
            else
            {
                rb.linearVelocity = desiredMoveDirection.normalized * velWithoutY.magnitude/*Mathf.Clamp(rb.linearVelocity.magnitude, 0f, maxSpeed)*/;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, tempY, rb.linearVelocity.z);
            }



            //if (Vector3.Dot(desiredMoveDirection, rb.linearVelocity) < 0)
            //{
            //    //currentSpeed = accelerationCurve.Evaluate(0f);//rb.linearVelocity.magnitude/*accelerationTime*/);
            //    //rb.linearVelocity -= rb.linearVelocity + desiredMoveDirection * 0.1f;

            //    /*rb.linearVelocity -= velWithoutY;
            //    rb.linearVelocity += velWithoutY * 0.01f;*/

            //    //Slow down very quickly but still make it look like it was gradual.
            //    if (slowToStopCoroutine == null)
            //    {
            //        slowToStopCoroutine = StartCoroutine(slowToStop());
            //    }

            //}

        }

        HandleGravity();
    }

    private float GetCorrespondingLookSensitivity(InputDevice device)
    {
        if (device is Gamepad)
        {
            return controllerLookSpeed;
        }
        if (device is Keyboard)
        {
            return mouseLookSpeed;
        }
        if (device is Mouse)
        {
            return mouseLookSpeed;
        }
        return mouseLookSpeed;
    }

    public void TakeDamage(float damage, float stunTime, GameObject other)
    {
        //if we're invincible, 
        //then exit this method.
        if (invincible)
        {
            return;
        }

        //When knocked only decrement
        //the knocked health
        //because curHealth will already
        //be 0.
        if (knocked)
        {
            knockedHealth -= damage;
        }
        else
        {
            // Apply the damage
            curHealth -= damage;
        }

            

        //TODO:
        //Add some screen shake

        //Add some knockback to the player from the hit.
    }

    public void StartIFrames()
    {
        //say the player is stunned.
        stunned = true;

        //if we're already invincible and
        //the iframes coroutine is currently
        //going, stop it, and create a new one.
        //Debug an error that this should never occur.
        if (invincible == true && iFramesRoutine != null)
        {
            StopCoroutine(iFramesRoutine);
            invincible = false;
            Debug.LogError("Player was damaged when in I-Frames, please check that enemies obey the rules of damage and only deal damage by calling TakeDamage.");
        }

        //start iframes coroutine
        iFramesRoutine = StartCoroutine(IFramesCoroutine());

    }

    public IEnumerator IFramesCoroutine()
    {
        stunned = true;

        invincible = true;
        float total = iFrameTime;
        float curTime = 0f;

        // Make modifications to IFrameTime as needed
/*        foreach (StatModifier mod in modifiers.Where(m => m.stat == StatModified.iFrameTime).ToList())
        {
            total = mod.makeModifications(total);
        }*/

        //cooldown for the sprite flickering.
        float flickerCooldown = 0.2f;


        //wait for the total iFrame time before
        //leaving invincibility.
        //Also flicker the 3D model while we do this.
        while (curTime < total)
        {
            curTime += Time.deltaTime;

            playerModel.gameObject.SetActive(!playerModel.gameObject.activeSelf);
            //wait until the cooldown to do the sprite flicker again.
            yield return new WaitForSeconds(flickerCooldown);
            //add the flicker cooldown to account for the time
            //we waited.
            curTime += flickerCooldown;

            /*//If we died, stop blinking.
            if (isDead)
            {
                break;
            }*/

            //Debug.LogWarning("FLICKER: " + curTime);

            //wait until the cooldown to do the sprite flicker again.
            yield return null;
        }

        //always make the animated model
        //visible after we finish flickering.
        playerModel.gameObject.SetActive(true);

        //after hitframes become hittable again.
        invincible = false;

        //we are no longer stunned
        stunned = false;


        //set iframes routine to null 
        //to indicate we have finished
        //as this will not happen automatically.
        iFramesRoutine = null;

        //exit coroutine.
        yield break;
    }

    public void TryPickupAnyItem()
    {
        Ray r = new Ray(cam.transform.position, cam.transform.forward);
        //We don't use player mask because players are also interactibles.
        //so make sure we raycast from the player's collider instead.
        if (Physics.Raycast(r, out var hitInfo, interactDist))
        { 
            //Primary Weapon
            PrimaryWeapon tempPrimaryWeapon = hitInfo.transform.gameObject.GetComponent<PrimaryWeapon>();
            //if the interacted object is a weapon, pick it up.
            if (tempPrimaryWeapon != null)
            {
                //Swap the current weapon with the one the player interacted with.
                SwapCurrentWeapon(tempPrimaryWeapon);
                useables[0] = tempPrimaryWeapon;
            }

            //Secondary Weapon

            SecondaryWeapon tempSecondaryWeapon = hitInfo.transform.gameObject.GetComponent<SecondaryWeapon>();
            //if the interacted object is a weapon, pick it up.
            if (tempSecondaryWeapon != null)
            {
                //Swap the current weapon with the one the player interacted with.
                SwapCurrentSecondaryWeapon(tempSecondaryWeapon);
                useables[1] = tempSecondaryWeapon;
            }

            //Throwable check
            Throwable tempThrowable = hitInfo.transform.gameObject.GetComponent<Throwable>();
            if (tempThrowable != null)
            {
                //Swap the current Throwable with the temp Throwable
                SwapCurrentThrowable(tempThrowable);
                useables[2] = tempThrowable;
            }

            //Appliable check
            Appliable tempAppliable = hitInfo.transform.gameObject.GetComponent<Appliable>();
            if (tempAppliable != null)
            {
                //Swap the current appliable with the temp appliable
                SwapCurrentAppliable(tempAppliable);
                useables[3] = tempAppliable;
            }

            //Consumable check
            Consumable tempConsumable = hitInfo.transform.gameObject.GetComponent<Consumable>();
            if (tempConsumable != null)
            {
                //Swap the current Consumable with the temp Consumable
                SwapCurrentConsumable(tempConsumable);
                useables[4] = tempConsumable;
            }
        }
    }

    public void OnInteract()
    {

        /*        Ray r = new Ray(cam.transform.position, cam.transform.forward);
                //We don't use player mask because players are also interactibles.
                //so make sure we raycast from the player's collider instead.
                if (Physics.Raycast(r, out var hitInfo, interactDist))
                {
                    //Depending on what slot is selected,
                    //we equip the item to the current selected slot.
                    switch (curSelectedSlot)
                    {
                        //Primary Weapon
                        case 0:
                            Weapon tempPrimaryWeapon = hitInfo.transform.gameObject.GetComponent<Weapon>();
                            //if the interacted object is a weapon, pick it up.
                            if (tempPrimaryWeapon != null)
                            {
                                //Swap the current weapon with the one the player interacted with.
                                SwapCurrentWeapon(tempPrimaryWeapon);
                            }
                            break;
                        //Secondary Weapon
                        case 1:
                            Weapon tempSecondaryWeapon = hitInfo.transform.gameObject.GetComponent<Weapon>();
                            //if the interacted object is a weapon, pick it up.
                            if (tempSecondaryWeapon != null)
                            {
                                //Swap the current weapon with the one the player interacted with.
                                SwapCurrentSecondaryWeapon(tempSecondaryWeapon);
                            }
                            break;
                        case 2:
                            //Throwable check
                            Throwable tempThrowable = hitInfo.transform.gameObject.GetComponent<Throwable>();
                            if (tempThrowable != null)
                            {
                                //Swap the current Throwable with the temp Throwable
                                SwapCurrentThrowable(tempThrowable);
                            }
                            break;
                        case 3:
                            //Appliable check
                            Appliable tempAppliable = hitInfo.transform.gameObject.GetComponent<Appliable>();
                            if (tempAppliable != null)
                            {
                                //Swap the current appliable with the temp appliable
                                SwapCurrentAppliable(tempAppliable);
                            }
                            break;
                        case 4:
                            //Consumable check
                            Consumable tempConsumable = hitInfo.transform.gameObject.GetComponent<Consumable>();
                            if (tempConsumable != null)
                            {
                                //Swap the current Consumable with the temp Consumable
                                SwapCurrentConsumable(tempConsumable);
                            }
                            break;


                    }
                }*/

        TryPickupAnyItem();

        Ray r = new Ray(cam.transform.position, cam.transform.forward);
        //We don't use player mask because players are also interactibles.
        //so make sure we raycast from the player's collider instead.
        if (Physics.Raycast(r, out var hitInfo, interactDist))
        {
            //ocheck if we interacted with an interactible.
            IInteractible interactible = hitInfo.transform.gameObject.GetComponent<IInteractible>();

            //if we actually hit an interactible.
            if (interactible != null)
                interactible.Interact();

        }








    
    }

    bool isHoldingInteract = false;
    IInteractible lastHeldInteractible = null;


    public void OnInteractHeld()
    {

        Debug.Log("HERE 1");
        //Don't execute this logic if we're already holding interact.
        if (isHoldingInteract)
            return;

        Debug.Log("HERE 2");




        Ray r = new Ray(cam.transform.position, cam.transform.forward);
        //We don't use player mask because players are also interactibles.
        if (Physics.Raycast(r, out var hitInfo, interactDist))
        {
            lastHeldInteractible = hitInfo.transform.gameObject.GetComponent<IInteractible>();

            Debug.Log(lastHeldInteractible);

            //if we actually hit an interactible.
            if (lastHeldInteractible != null)
            {
                Debug.Log("HOLD!");
                lastHeldInteractible.InteractHold();
                isHoldingInteract = true;
            }
        }
    }

    public void CancelInteractHold()
    {
        isHoldingInteract = false;
        lastHeldInteractible.InteractStopHold();
    }

    public void CreateNewPrimaryWeapon(GameObject g)
    {
        GameObject temp = Instantiate(g);

        //set the weapons parent transform to be this player.
        temp.transform.SetParent(handTransform, false);
        //Set to zero position so the transform is exactly where the hand is.
        temp.transform.localPosition = Vector3.zero;
        //set to no rotation (0, 0, 0);
        temp.transform.localRotation = Quaternion.identity;

        PrimaryWeapon pw = temp.GetComponent<PrimaryWeapon>();
        pw.parentPlayer = this;

        //assign the new current weapon.
        curPrimaryWeapon = temp.GetComponent<PrimaryWeapon>();
        useables[0] = curPrimaryWeapon;
        //Set the slot icon.
        uiController.OnUpdateSlotIcon(curPrimaryWeapon.icon, 0);
    }

    public void CreateNewSecondaryWeapon(GameObject g)
    {
        GameObject temp = Instantiate(g);

        //set the weapons parent transform to be this player.
        temp.transform.SetParent(backTransform, false);
        //Set to zero position so the transform is exactly where the hand is.
        temp.transform.localPosition = Vector3.zero;
        //set to no rotation (0, 0, 0);
        temp.transform.localRotation = Quaternion.identity;
        SecondaryWeapon sw = temp.GetComponent<SecondaryWeapon>();
        sw.parentPlayer = this;


        //assign the new current weapon.
        curSecondaryWeapon = temp.GetComponent<SecondaryWeapon>();
        useables[1] = curSecondaryWeapon;
        //Set the slot icon.
        uiController.OnUpdateSlotIcon(curSecondaryWeapon.icon, 1);
    }



    public void SwapCurrentWeapon(PrimaryWeapon w)
    {
        DropPrimaryWeapon(transform.position, Quaternion.identity);

        //Remove the rigidbody.
        Destroy(w.rb);
        //set the weapons parent transform to be this player.
        //we switch placement depending on if this item type is the currently selected item.
        w.transform.SetParent(curSelectedSlot == 0 ? handTransform : backTransform, false);
        //Set to zero position so the transform is exactly where the hand is.
        w.transform.localPosition = Vector3.zero;
        //set to no rotation (0, 0, 0);
        w.transform.localRotation = Quaternion.identity;
        //set us to be the parent player.
        w.parentPlayer = this;
        //Update the slot icon for this weapon.
        uiController.OnUpdateSlotIcon(w.icon, 0);

        //assign the new current weapon.
        curPrimaryWeapon = w;


        //invoke OnPickup if methods are subscribed to it.
        //OnPickup?.Invoke();
    }

    void DropPrimaryWeapon(Vector3 dropPos, Quaternion rot)
    {
        if (curPrimaryWeapon != null)
        {
            //set parent to be null
            //so that we can disconnect it from the player.
            curPrimaryWeapon.transform.parent = null;
            //put the old weapon at the position given.
            curPrimaryWeapon.transform.position = dropPos;
            //set to the same rotation as the prev weapon.
            curPrimaryWeapon.transform.localRotation = rot;
            //Add the rigidbody back
            curPrimaryWeapon.rb = curPrimaryWeapon.AddComponent<Rigidbody>();
            //don't collide with player.
            curPrimaryWeapon.rb.excludeLayers = LayerMask.GetMask("Player");
            //Set the parent player to null for this weapon.
            curPrimaryWeapon.parentPlayer = null;
        }
    }

    public void SwapCurrentSecondaryWeapon(SecondaryWeapon w)
    {
        DropSecondaryWeapon(transform.position, Quaternion.identity);

        //Remove the rigidbody.
        Destroy(w.rb);
        //set the SecondaryWeapons parent transform to be this player.
        //we switch placement depending on if this item type is the currently selected item.
        w.transform.SetParent(curSelectedSlot == 1 ? handTransform : backTransform, false);
        //Set to zero position so the transform is exactly where the hand is.
        w.transform.localPosition = Vector3.zero;
        //set to no rotation (0, 0, 0);
        w.transform.localRotation = Quaternion.identity;
        //set us to be the parent player.
        w.parentPlayer = this;
        //Update the slot icon for this weapon.
        uiController.OnUpdateSlotIcon(w.icon, 1);


        //assign the new current SecondaryWeapon.
        curSecondaryWeapon = w;


        //invoke OnPickup if methods are subscribed to it.
        //OnPickup?.Invoke();
    }

    void DropSecondaryWeapon(Vector3 dropPos, Quaternion rot)
    {
        if (curSecondaryWeapon != null)
        {
            //set parent to be null
            //so that we can disconnect it from the player.
            curSecondaryWeapon.transform.parent = null;
            //put the old SecondaryWeapon at the position given.
            curSecondaryWeapon.transform.position = dropPos;
            //set to the same rotation as the prev SecondaryWeapon.
            curSecondaryWeapon.transform.localRotation = rot;
            //Add the rigidbody back
            curSecondaryWeapon.rb = curSecondaryWeapon.AddComponent<Rigidbody>();
            //don't collide with player.
            curSecondaryWeapon.rb.excludeLayers = LayerMask.GetMask("Player");
            //Set the parent player to null for this weapon.
            curSecondaryWeapon.parentPlayer = null;
        }
    }

    public void SwapCurrentThrowable(Throwable t)
    {
        DropCurrentThrowable(transform.position, Quaternion.identity);

        //Remove the rigidbody.
        Destroy(t.rb);

        //set the Throwables parent transform to be this player.
        //we switch placement depending on if this item type is the currently selected item.
        t.transform.SetParent(curSelectedSlot == 2 ? handTransform : backTransform, false);
        //Set to zero position so the transform is exactly where the hand is.
        t.transform.localPosition = Vector3.zero;
        //set to no rotation (0, 0, 0);
        t.transform.localRotation = Quaternion.identity;

        //Update the slot icon for this throwable.
        uiController.OnUpdateSlotIcon(t.icon, 2);

        //assign the new current Throwable.
        curThrowable = t;

        //invoke OnPickup if methods are subscribed to it.
        //OnPickup?.Invoke();
    }

    void DropCurrentThrowable(Vector3 dropPos, Quaternion rot)
    {
        if (curThrowable != null)
        {
            //set parent to be null
            //so that we can disconnect it from the player.
            curThrowable.transform.parent = null;
            //put the old Throwable at the position given.
            curThrowable.transform.position = dropPos;
            //set to the same rotation as the prev Throwable.
            curThrowable.transform.localRotation = rot;
            //Add the rigidbody back
            curThrowable.rb = curThrowable.AddComponent<Rigidbody>();
            //don't collide with player.
            curThrowable.rb.excludeLayers = LayerMask.GetMask("Player");
        }
    }

    public void SwapCurrentAppliable(Appliable a)
    {
        DropCurrentAppliable(transform.position, Quaternion.identity);

        //Remove the rigidbody.
        Destroy(a.rb);
        //set the weapons parent transform to be this player.
        //we switch placement depending on if this item type is the currently selected item.
        a.transform.SetParent(curSelectedSlot == 3 ? handTransform : backTransform, false);
        //Set to zero position so the transform is exactly where the hand is.
        a.transform.localPosition = Vector3.zero;
        //set to no rotation (0, 0, 0);
        a.transform.localRotation = Quaternion.identity;
        a.parentController = this;
        //Add the onUseableDestroyed listener for the curAppliable
        a.onBeforeDestroy.AddListener(OnUseableDestroyed);

        //Update the slot icon for this appliable.
        uiController.OnUpdateSlotIcon(a.icon, 3);

        //assign the new current weapon.
        curAppliable = a;

        //invoke OnPickup if methods are subscribed to it.
        //OnPickup?.Invoke();
    }

    void DropCurrentAppliable(Vector3 dropPos, Quaternion rot)
    {
        if (curAppliable != null)
        { 
            //set parent to be null
            //so that we can disconnect it from the player.
            curAppliable.transform.parent = null;
            //put the old weapon at the position given.
            curAppliable.transform.position = dropPos;
            //set to the same rotation as the prev weapon.
            curAppliable.transform.localRotation = rot;
            //Add the rigidbody back
            curAppliable.rb = curAppliable.AddComponent<Rigidbody>();
            //don't collide with player.
            curAppliable.rb.excludeLayers = LayerMask.GetMask("Player");
            curAppliable.parentController = null;
            curAppliable.targetPlayer = null;

            //Remove the onUseableDestroyed listener for the curAppliable
            curAppliable.onBeforeDestroy.RemoveListener(OnUseableDestroyed);
        }
    }

    public void SwapCurrentConsumable(Consumable c)
    {
        DropCurrentConsumable(transform.position, Quaternion.identity);

        //Remove the rigidbody.
        Destroy(c.rb);
        //set the Consumables parent transform to be this player.
        //we switch placement depending on if this item type is the currently selected item.
        c.transform.SetParent(curSelectedSlot == 4 ? handTransform : backTransform, false);
        //Set to zero position so the transform is exactly where the hand is.
        c.transform.localPosition = Vector3.zero;
        //set to no rotation (0, 0, 0);
        c.transform.localRotation = Quaternion.identity;
        //Update the slot icon for this appliable.
        uiController.OnUpdateSlotIcon(c.icon, 4);


        //assign the new current Consumable.
        curConsumable = c;

        //invoke OnPickup if methods are subscribed to it.
        //OnPickup?.Invoke();
    }

    void DropCurrentConsumable(Vector3 dropPos, Quaternion rot)
    {
        if (curConsumable != null)
        {
            //set parent to be null
            //so that we can disconnect it from the player.
            curConsumable.transform.parent = null;
            //put the old Consumable at the position given.
            curConsumable.transform.position = dropPos;
            //set to the same rotation as the prev Consumable.
            curConsumable.transform.localRotation = rot;
            //Add the rigidbody back
            curConsumable.rb = curConsumable.AddComponent<Rigidbody>();
            //don't collide with player.
            curConsumable.rb.excludeLayers = LayerMask.GetMask("Player");
        }
    }

    public void LoadDataManually(GameData gameData)
    {
        //TODO: We need to figure out how to,
        //when loading data create a new player
        //automatically and match it's controller
        //to the player's data so when they load into the scene
        //they get the correct data matched with their controller.
        //Look at Niner-Knockout for how you coded that the first
        //time and reference it for your script.

        //only set this once per scene to indicate we already loaded data.
        didLoad = true;

        Debug.Log("LOAD DATA");

        //TODO: Add a null check for this player.
        //if the weapon is null, creat and load the default weapon.
        CreateNewPrimaryWeapon(DataPersistenceManager.instance.FindWeaponPrefab("Stake"));
        CreateNewSecondaryWeapon(DataPersistenceManager.instance.FindWeaponPrefab("Revolver"));
        
    }

    public void SaveDataManually(ref GameData gameData)
    {
        Debug.LogWarning("SAVE PLAYER DATA!");
        //Update the player's individual data in the game. 
        gameData.UpdatePlayerInfo(new PlayerInfo() { guid = guid.ToString(), zombieKillCount = zombieKillCount, controlScheme = playerInput.currentControlScheme, deviceID=playerInput.GetDevice<InputDevice>().deviceId, playerIndex = playerInput.playerIndex, splitScreenIndex = playerInput.splitScreenIndex, hasDevice = true });
    }

    private Coroutine knockedCoroutine = null;

    private void BecomeKnocked()
    {
        if (knockedCoroutine != null)
        {
            Debug.LogError("Tried to become knocked when already knocked!!!");
        }
        else
        {
            knockedCoroutine = StartCoroutine(KnockedCoroutine());
        }
    }

    float knockedHealthDecrementInterval = 1f;
    float knockedHealthDecrement = 10f;

    private IEnumerator KnockedCoroutine()
    {
        Debug.Log("Knocked");

        knocked = true;
        canMove = false;
        //freeze the character.
        rb.isKinematic = true;

        while (knockedHealth > 0)
        {
            //Wait interval seconds and then
            yield return new WaitForSeconds(knockedHealthDecrementInterval);
            //deal damage.
            knockedHealth -= knockedHealthDecrement;
            //Should take 30 seconds to die.

            
        }

        canMove = true;
        knocked = false;
        knockedCoroutine = null;
    }

    //called when the player is picked
    //up by another player.
    public void CancelKnocked()
    {
        StopCoroutine(knockedCoroutine);
        knockedCoroutine = null;
        knocked = false;
        canMove = true;
        //stop freeezing the character.
        rb.isKinematic = false;

        //When exiting knocked
        //don't just set current health to a constant.
        //instead we should just take whatever percent
        //of knocked health was left and give that
        //back to the player multiplied by some factor.
        curHealth = knockedHealth / 3 * 0.25f;
    }

    float _curReviveTime = 0f;

    //Used to update the revive slider as this updates.
    //do cur / total to get a 0-1 value representing the progress to reviving.
    float curReviveTime { get {  return _curReviveTime; } set { _curReviveTime = value; uiController.UpdateReviveSlider(value / totalReviveTime); } }
    float totalReviveTime = 5f;

    Coroutine reviveCoroutine = null;

    bool isGettingRevived = false;

    public IEnumerator ReviveCoroutine()
    {
        isGettingRevived = true;

        Debug.Log("START REVIVING");

        while (curReviveTime < totalReviveTime)
        {
            curReviveTime += Time.deltaTime;

            Debug.Log("REVIVING: " +  curReviveTime / totalReviveTime);

            yield return null;
        }

        isGettingRevived = false;
        //Reset knocked health and curRevive time.
        knockedHealth = 300;
        curReviveTime = 0f;


        CancelKnocked();
    }

    public void StartRevive()
    {
        reviveCoroutine = StartCoroutine(ReviveCoroutine());
    }

    public void CancelRevive()
    {
        StopCoroutine(reviveCoroutine);
        curReviveTime = 0;
        isGettingRevived = false;
        reviveCoroutine = null;
    }

    public void OnFocusEnter()
    {
        throw new NotImplementedException();
    }

    public void Interact()
    {

    }

    public void InteractHold()
    {
        if (knocked && !isGettingRevived)
        {
            StartRevive();
        }
    }

    public void InteractStopHold()
    {
        if (isGettingRevived)
        CancelRevive();
    }

    public void OnFocusLeave()
    {
        throw new NotImplementedException();
    }
}
