using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
public enum TyransState
{
    Idle,
    Jumping,
    DoubleJumping,
    Falling,
    Dashing,
    WallSlide,
    EndlessDash,
    EndlessDashLeft,
    EndlessDashRight,
    DashHop,
    SuperDash,
    Grapple,
    TetherJump,
}


public class TyranMovementScript : MonoBehaviour
{
    private TyransState currentState;

    // Define state-specific variables and parameters
    [SerializeField] private float speed = 12.5f;
    [SerializeField] private float jumpingPower = 13f;
    [SerializeField] private float DoublejumpingPower = 11.5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform FrontCheck;
    [SerializeField] private Transform SuperDashCheck;
    [SerializeField] private Transform GrappleCheck;
    [SerializeField] private Transform GrappleCancelCheck;
    [SerializeField] private float GrounndCheckDistance = 0.1f;
    [SerializeField] private float SuperDashCheckDistance = 0.6f;
    [SerializeField] private float wallCheckDistance = 0.4f;
    [SerializeField] private float FrontCheckDistance = 0.4f;
    [SerializeField] private float CancelGrappleCircle = 0.8f;
    [SerializeField]  Vector2 boxSize = new Vector2(2f, 1f);
    [SerializeField] private float GrapplePointRadius = 2f;
    [SerializeField] private float fallMultiplier = 9.3f;
    [SerializeField] private float maxFallVelocity = 50;
    [SerializeField] private float jumpVelocityFall = 10f;
    [SerializeField] private float dashspeed = 25f;
    [SerializeField] private float dashduration = 0.2f;
    [SerializeField] private float WallJumpduration = 0.15f;
    [SerializeField] private float dashcooldown = 1f;
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float cancel = 0.5f;
    [SerializeField] private float wallJumpControlTimer = 0;
    [SerializeField] private float wallJumpControlDelay = 2f;
    [SerializeField] private float wallJumpControlFactor = 0.5f;
    [SerializeField] private float xWallForce = 28f;
    [SerializeField] private float yWallForce = 9f;
    [SerializeField] private float EndlessForce = 23f;
    [SerializeField] private float dashHopJumpStrength = 11f;
    [SerializeField] private float DownEndlessDashStraightSpeed = 3f;
    [SerializeField] private float AfterDashSpeed = 19f;
    [SerializeField] private float EndlessDashExitSpeed = 23f;
    [SerializeField] private float SuperDashAfterMathSpeed = 35f;
    [SerializeField] private float WallJumpAfterMathSpeed = 25f;
    [SerializeField] private float AfterDashDuration = 0.5f;
    [SerializeField] private float distanceInFront = 3f; // Distance in front of the player where the projectile will appear
    [SerializeField] private float grappleReleaseTime = 2f; // Time after which the grapple is released
    [SerializeField] private float springDampingRatio = 0.7f; // Damping ratio for the SpringJoint2D
    [SerializeField] private float springFrequency = 5f; // Frequency for the SpringJoint2D
    [SerializeField] private float grappleDuration = 5f; // Frequency for the SpringJoint2D
    [SerializeField] private float zipCooldownDuration = 2f; 
    [SerializeField] private float grappleForce = 5f; // Frequency for the SpringJoint2D
    [SerializeField] private float grappleRange = 5f;
    [SerializeField] private float grappleSpeed = 40f; // Speed at which the grapple extends
    [SerializeField] private float grapplePullSpeed = 23f; // Pull speed when grappling
    [SerializeField] private float ropeSpeed = 5f; // Speed at which the rope shoots out
    [SerializeField] private float swingForce = 6f;
    [SerializeField] private float swingForceM = 20f;
    [SerializeField] private float swingSpeed = 5f;
    [SerializeField] private float previousAngle = 0f;
    [SerializeField] private float smoothingFactor = 0.1f;
    [SerializeField] private float maxGrappleDistance = 0f;
    [SerializeField] private float speedThreshold = 10f;
    [SerializeField] private float maxMomentum = 5f;
    [SerializeField] private float targetAngle = -90f; // Straight down
    [SerializeField] private float maxResistanceAngle = -90f;
    [SerializeField] private float angleThreshold = 20f; // Range around the target angle
    [SerializeField] private float swingSpeedMultiplier = 10f; // Base multiplier for swinging speed buildup
    [SerializeField] private float resistanceMultiplier = 0.5f;
    [SerializeField] private float currentSwingSpeed;
    public LineRenderer lineRenderer; // Reference to the LineRenderer
    
    

    private int currentDashCount = 1; // Current dash count available

    [SerializeField] private float CoyoyeTime = 0.2f;
    [SerializeField] private float currentCoyoteTime;
    [SerializeField] private float EndLessDashtimer = 100f;
    [SerializeField] private int maxDashHopCount = 1; // Maximum number of dash hops allowed
    [SerializeField] private float grappleCheckInterval = 0.1f; // Check every 0.1 seconds
    [SerializeField] private float nextGrappleCheckTime = 0f;

    private float originalGravityScale;
    private float originalSpeed;
    private Vector2 originalVelocity;
    private bool incoyoyotime;
    private bool walking = true;
    private bool IsWallsliding;
    private bool isFacingRight = true;
    private bool isWalled;
    private bool isWallJumping;
    private int currentDashHopCount;
    private bool isGrounded;
    private bool isTouchingFront;
    private bool CanSuperDash;
    private bool CancelGrapple;
    private bool CanGrapple;
    private int doubleJumpCounter = 1;
    private bool isUpKeyPressed;
    private bool isDownKeyPressed;
    private bool isLeftKeyPressed;
    private bool isRightKeyPressed;
    private bool isDashing = false;
    private bool isEndlessDashingDown = false;
    private bool isEndlessDashingLeftRight = false;
    private Coroutine currentEndlessDashCoroutine = null;
    private bool canGrapple = true;
    bool requiresMaxDistance = false; // Tracks if smoother clamping is active
    private float grappleCooldownDuration = 0.5f; // Cooldown period of 0.5 seconds
    private float SpringConnectionOffset = 2f;
    private bool isZipping = false;
    private bool isSwinging = false;
    private Rigidbody2D rb;
    private SpringJoint2D springJoint;
   

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentState = TyransState.Idle;
        originalGravityScale = rb.gravityScale;
        currentCoyoteTime = CoyoyeTime;
        currentDashHopCount = maxDashHopCount;
        originalSpeed = speed; // Store the original speed

        springJoint = GetComponent<SpringJoint2D>();
        springJoint.enabled = false; // Disable it initially

        originalGravityScale = rb.gravityScale;

       

    }

    // Update is called once per frame
    void Update()
    {

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, GrounndCheckDistance, groundLayer);
        isWalled = Physics2D.OverlapCircle(wallCheck.position, wallCheckDistance, groundLayer);
        isTouchingFront = Physics2D.OverlapCircle(FrontCheck.position, FrontCheckDistance, groundLayer);
        CancelGrapple = Physics2D.OverlapBox(GrappleCancelCheck.position, boxSize, 0f, groundLayer);

        CanSuperDash = Physics2D.OverlapCircle(SuperDashCheck.position, SuperDashCheckDistance, groundLayer);


        isUpKeyPressed = Input.GetKey(KeyCode.UpArrow);
        isDownKeyPressed = Input.GetKey(KeyCode.DownArrow);
        isLeftKeyPressed = Input.GetKey(KeyCode.LeftArrow);
        isRightKeyPressed = Input.GetKey(KeyCode.RightArrow);

        float horizontalInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);

        walking = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow);




        if (horizontalInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && isFacingRight)
        {
            Flip();
        }

        incoyoyotime = currentCoyoteTime > 0f;

        if (!walking)
        {
            speed = originalSpeed;
        }

        if (isEndlessDashingDown)
        {
            speed = DownEndlessDashStraightSpeed;
        }

        if (isEndlessDashingLeftRight)
        {
            speed = EndlessDashExitSpeed;
        }

        if (WallJumpduration < 0f)
        {
            speed = WallJumpAfterMathSpeed;
        }



        if (isGrounded && !isDashing)
        {
            doubleJumpCounter = 1;

            currentCoyoteTime = CoyoyeTime;

            currentDashHopCount = 1;

            currentDashCount = 1;

            speed = originalSpeed;

            isEndlessDashingLeftRight = false;

            isEndlessDashingDown = false;
        }
        else
        {
            ChangeState(TyransState.Falling);
        }

        if (Input.GetButton("Grapple"))
        {
            isSwinging = true;
        }




            if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded || currentCoyoteTime > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
                ChangeState(TyransState.Jumping);
                currentCoyoteTime = 0f; // Reset coyote time after jumping
            }
        }


        if (Input.GetButtonDown("Jump") && !incoyoyotime)
        {
            ChangeState(TyransState.DoubleJumping);
        }





        if (isTouchingFront && walking && !isGrounded)
        {
            ChangeState(TyransState.WallSlide);
        }



        if (Input.GetButtonDown("Dash") && !isDownKeyPressed)
        {
            ChangeState(TyransState.Dashing);


        }


        if (Input.GetButtonDown("Dash") && isDownKeyPressed && !isLeftKeyPressed && !isRightKeyPressed)
        {
            ChangeState(TyransState.EndlessDash);


        }



        if (Input.GetButtonDown("Dash") && isRightKeyPressed && isDownKeyPressed)
        {
            ChangeState(TyransState.EndlessDashRight);

        }

        if (Input.GetButtonDown("Dash") && isLeftKeyPressed && isDownKeyPressed)
        {
            ChangeState(TyransState.EndlessDashLeft);
        }

        if ((isEndlessDashingDown || isEndlessDashingLeftRight) && currentDashHopCount > 0 && Input.GetButtonDown("Jump"))
        {
            ChangeState(TyransState.DashHop);
        }



        if (Input.GetButtonDown("Dash") && CanSuperDash && isEndlessDashingDown)
        {
            ChangeState(TyransState.SuperDash);

        }




        if (Input.GetButtonDown("Grapple"))
        {
            ChangeState(TyransState.Grapple);
        }


        if (Input.GetButtonDown("Jump") && isZipping)
        {
            ChangeState(TyransState.TetherJump);
        }





        switch (currentState)
        {
            case TyransState.Idle:
                HandleIdleState();
                break;

            case TyransState.Falling:
                HandleFallingState();
                break;


            case TyransState.Jumping:
                HandleJumpingState();
                currentCoyoteTime = 0f;
                break;

            case TyransState.WallSlide:
                HandleWallSlideState();
                currentCoyoteTime = 0f;
                break;


            case TyransState.DoubleJumping:
                HandleDoubleJumpingState();
                break;

            case TyransState.Dashing:
                HandleDashingState();
                currentCoyoteTime = 0f;
                break;

            case TyransState.EndlessDash:
                HandleEndlessDashState();
                currentCoyoteTime = 0f;
                break;

            case TyransState.EndlessDashLeft:
                HandleEndlessDashLeftState();
                currentCoyoteTime = 0f;
                break;

            case TyransState.EndlessDashRight:
                HandleEndlessDashRightState();
                currentCoyoteTime = 0f;
                break;

            case TyransState.DashHop:
                HandleDashHopState();
                currentCoyoteTime = 0f;
                break;

            case TyransState.SuperDash:
                HandleSuperDashState();
                currentCoyoteTime = 0f;
                break;


            case TyransState.Grapple:
                HandleGrappleState();
                currentCoyoteTime = 0f;
                break;


            case TyransState.TetherJump:
                HandleTetherJumpState();
                currentCoyoteTime = 0f;
                break;

        }


        void ChangeState(TyransState newState)
        {
            if (newState == TyransState.WallSlide)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.998f); // Halve the upward velocity
            }



            currentState = newState; // Update the current state
        }

        void HandleIdleState()
        {
            currentDashHopCount = maxDashHopCount;
        }



        void HandleFallingState()
        {

            if (Input.GetButtonDown("Jump") && !isSwinging)
            {
                ChangeState(TyransState.Jumping);
                return; // Exit the HandleFallingState to prevent further processing
            }

            if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * cancel);
            }

            if (rb.velocity.y < jumpVelocityFall)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * fallMultiplier * Time.deltaTime;

                if (rb.velocity.y < -maxFallVelocity)
                {
                    rb.velocity = new Vector2(rb.velocity.x, -maxFallVelocity);
                }
            }

            currentCoyoteTime -= Time.deltaTime;

            if (currentCoyoteTime <= 0f)
            {
                // Coyote time depleted, transition to another state if needed
                ChangeState(TyransState.Idle); // Change to Idle or Falling or any other appropriate state
                currentCoyoteTime = 0f; // Ensure that coyote time stays at 0
                return;
            }

            if (isGrounded)
            {
                ChangeState(TyransState.Idle);
            }
        }

        void HandleJumpingState()
        {


            if (Input.GetButtonDown("Jump"))
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
                ChangeState(TyransState.Jumping);
            }

            if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * cancel);

            }
        }


        void HandleDoubleJumpingState()
        {


            if (!isGrounded && !isEndlessDashingDown && !isEndlessDashingLeftRight && (Input.GetButtonDown("Jump") && (doubleJumpCounter > 0)))
            {
                // Double jump when in the air and the counter is greater than 0
                rb.velocity = Vector2.up * DoublejumpingPower;
                doubleJumpCounter--;
            }
        }

        void HandleWallSlideState()
        {
            {
                if (Input.GetButtonDown("Jump"))
                {
                    HandleWallJump();
                }

                rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -1f, float.MaxValue));
            }

            void HandleWallJump()
            {
                // Determine direction based on the player's facing direction
                Vector2 wallJumpDirection = isFacingRight ? Vector2.left : Vector2.right;

                StartCoroutine(PerformWallJump(wallJumpDirection));
            }

            IEnumerator PerformWallJump(Vector2 direction)
            {
                float timer = 0f;

                // Apply wall jump forces over the duration
                while (timer < WallJumpduration)
                {
                    rb.velocity = new Vector2(direction.x * xWallForce, yWallForce);
                    timer += Time.deltaTime;

                    if (Input.GetButtonUp("Jump"))
                    {
                        rb.velocity = Vector2.zero;
                        yield break;
                    }

                    yield return null;
                }

                rb.velocity = Vector2.zero;
                rb.gravityScale = originalGravityScale;
                ChangeState(TyransState.Falling);
            }

        }





        void HandleDashingState()
        {
            if (currentEndlessDashCoroutine != null && currentDashCount > 0)
            {
                StopCoroutine(currentEndlessDashCoroutine);
                currentEndlessDashCoroutine = null;
                isEndlessDashingDown = false; // Reset any flags or states related to endless dash
                isEndlessDashingLeftRight = false;
            }
            if (currentDashCount > 0)
            {
                float dashX = Input.GetAxisRaw("Horizontal");
                float dashY = Input.GetAxisRaw("Vertical");

                Vector2 dashDirection = new Vector2(dashX, dashY).normalized;



                if (isFacingRight && !isRightKeyPressed && !isUpKeyPressed)
                {
                    dashDirection = Vector2.right;
                }
                else if (!isFacingRight && !isLeftKeyPressed && !isUpKeyPressed)
                {
                    dashDirection = Vector2.left;
                }

                if (isUpKeyPressed && isRightKeyPressed && !isDownKeyPressed)
                {
                    dashDirection = new Vector2(dashX, dashY);
                }
                else if (isUpKeyPressed && isLeftKeyPressed && !isDownKeyPressed)
                {
                    dashDirection = new Vector2(dashX, dashY);
                }
                else if (isUpKeyPressed)
                {
                    dashDirection = Vector2.up;
                }
                else if (isLeftKeyPressed && !isDownKeyPressed)
                {
                    dashDirection = Vector2.left;
                }
                else if (isRightKeyPressed && !isDownKeyPressed)
                {
                    dashDirection = Vector2.right;
                }




                rb.AddForce(dashDirection * dashspeed * dashForce);


                currentDashCount--;

                if (isGrounded)
                {
                    StartCoroutine(DashCooldown());
                    ChangeState(TyransState.Idle);
                }




                originalVelocity = rb.velocity;

                StartCoroutine(PerformDash(dashDirection));

            }
            IEnumerator PerformDash(Vector2 dashDirection)
            {
                isDashing = true;
                rb.gravityScale = 0f; // Save the original gravity scale

                float dashTimer = 0f;
                while (dashTimer < dashduration)
                {
                    Vector2 dashVelocity = dashDirection * dashspeed;
                    rb.velocity = dashVelocity;

                    dashTimer += Time.deltaTime;
                    yield return null;
                }
                speed = AfterDashSpeed;

                rb.velocity = Vector2.zero;
                rb.gravityScale = originalGravityScale; // Restore the original gravity scale
                isDashing = false;
            }

            IEnumerator DashCooldown()
            {
                yield return new WaitForSeconds(dashcooldown);
                isDashing = false;


                ExitDashState();
            }

            void ExitDashState()
            {
                // Handle transition to Idle or Jumping state based on the player's condition
                if (!isGrounded)
                {
                    ChangeState(TyransState.Falling);
                }



            }
        }

        void HandleDashHopState()
        {
            if (currentEndlessDashCoroutine != null)
            {
                StopCoroutine(currentEndlessDashCoroutine);
                currentEndlessDashCoroutine = null;
            }

            StartCoroutine(DashHop());

            IEnumerator DashHop()
            {
                rb.velocity = new Vector2(rb.velocity.x, dashHopJumpStrength);
                currentDashHopCount--;
                isEndlessDashingDown = false;
                isEndlessDashingLeftRight = false;

                yield break; // Exit the coroutine early
            }
        }

        void HandleSuperDashState()
        {
            if (currentEndlessDashCoroutine != null)
            {
                StopCoroutine(currentEndlessDashCoroutine);
                currentEndlessDashCoroutine = null;
            }

            if (Input.GetButtonDown("Dash") && isRightKeyPressed)
            {
                StartCoroutine(SuperDash());
            }



            IEnumerator SuperDash()
            {

                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
                isEndlessDashingDown = false;

                yield break; // Exit the coroutine early
            }
        }


        void HandleEndlessDashState()
        {
            if (currentEndlessDashCoroutine != null)
            {
                StopCoroutine(currentEndlessDashCoroutine);
            }

            if (Input.GetButtonDown("Dash") && isDownKeyPressed)
            {
                currentEndlessDashCoroutine = StartCoroutine(DashDown());
            }



            IEnumerator DashDown()
            {
                isEndlessDashingDown = true;
                float timer = 0f;
                bool cancelEndlessDash = false;

                while (timer < EndLessDashtimer && !cancelEndlessDash)
                {
                    if (Input.GetButtonDown("Jump") && currentDashHopCount > 0)
                    {
                        HandleDashHopState(); // Allow dash hop during endless dash
                        yield break; // Exit the coroutine early
                    }

                    if (Input.GetButtonDown("Dash") && !isDownKeyPressed && currentDashCount > 0 && !CanSuperDash)
                    {
                        HandleDashingState(); // Allow dash hop during endless dash
                        yield break; // Exit the coroutine early
                    }

                    if (Input.GetButtonDown("Dash") && CanSuperDash)
                    {
                        HandleSuperDashState(); // Allow dash hop during endless dash
                        yield break; // Exit the coroutine early
                    }

                    rb.velocity = new Vector2(rb.velocity.x, -EndlessForce);
                    timer += Time.deltaTime;

                    if (isGrounded)
                    {
                        cancelEndlessDash = true;
                    }

                    yield return null;
                }

                // Stop moving after dash duration
                rb.velocity = Vector2.zero;
                isEndlessDashingDown = false;


                if (cancelEndlessDash)
                {
                    ChangeState(TyransState.Idle);
                }
            }
        }

        void HandleEndlessDashLeftState()
        {
            if (currentEndlessDashCoroutine != null)
            {
                StopCoroutine(currentEndlessDashCoroutine);
            }



            if (Input.GetButtonDown("Dash") && isLeftKeyPressed && isDownKeyPressed)
            {
                currentEndlessDashCoroutine = StartCoroutine(DashDownLeft());
            }

            IEnumerator DashDownLeft()
            {
                isEndlessDashingLeftRight = true;
                float timer = 0f;
                bool cancelEndlessDash = false;

                while (timer < EndLessDashtimer && !cancelEndlessDash)
                {
                    if (Input.GetButtonDown("Jump") && currentDashHopCount > 0)
                    {
                        HandleDashHopState(); // Allow dash hop during endless dash
                        yield break; // Exit the coroutine early
                    }

                    if (Input.GetButtonDown("Dash") && !isDownKeyPressed && currentDashCount > 0 && !CanSuperDash)
                    {
                        HandleDashingState(); // Allow dash hop during endless dash
                        yield break; // Exit the coroutine early
                    }

                    if (Input.GetButtonDown("Dash") && CanSuperDash)
                    {
                        HandleSuperDashState(); // Allow dash hop during endless dash
                        yield break; // Exit the coroutine early
                    }

                    rb.velocity = new Vector2(-EndlessForce, -EndlessForce);
                    timer += Time.deltaTime;

                    if (isGrounded)
                    {
                        cancelEndlessDash = true;
                    }

                    yield return null;
                }

                // Stop moving after dash duration
                rb.velocity = Vector2.zero;
                isEndlessDashingLeftRight = false;

                if (cancelEndlessDash)
                {
                    ChangeState(TyransState.Idle);
                }
            }
        }

        void HandleEndlessDashRightState()
        {
            if (currentEndlessDashCoroutine != null)
            {
                StopCoroutine(currentEndlessDashCoroutine);
            }

            if (Input.GetButtonDown("Dash") && isRightKeyPressed && isDownKeyPressed)
            {
                currentEndlessDashCoroutine = StartCoroutine(DashDownRight());
            }

            IEnumerator DashDownRight()
            {
                isEndlessDashingLeftRight = true;
                float timer = 0f;
                bool cancelEndlessDash = false;

                while (timer < EndLessDashtimer && !cancelEndlessDash)
                {
                    if (Input.GetButtonDown("Jump") && currentDashHopCount > 0)
                    {
                        HandleDashHopState(); // Allow dash hop during endless dash
                        yield break; // Exit the coroutine early
                    }

                    if (Input.GetButtonDown("Dash") && !isDownKeyPressed && currentDashCount > 0 && !CanSuperDash)
                    {
                        HandleDashingState(); // Allow dash hop during endless dash
                        yield break; // Exit the coroutine early
                    }

                    if (Input.GetButtonDown("Dash") && CanSuperDash)
                    {
                        HandleSuperDashState(); // Allow dash hop during endless dash
                        yield break; // Exit the coroutine early
                    }

                    rb.velocity = new Vector2(EndlessForce, -EndlessForce);
                    timer += Time.deltaTime;

                    if (isGrounded)
                    {
                        cancelEndlessDash = true;
                    }

                    yield return null;
                }

                // Stop moving after dash duration
                rb.velocity = Vector2.zero;
                isEndlessDashingLeftRight = false;

                if (cancelEndlessDash)
                {
                    ChangeState(TyransState.Idle);
                }
            }
        }

        void HandleGrappleState()
        {
            // Check if the player can grapple and if the grapple button was just pressed
            if (!canGrapple || !Input.GetButtonDown("Grapple")) return;

            // Start the grapple process
            StartCoroutine(ExtendGrapple());
        }

        IEnumerator ExtendGrapple()
        {
            canGrapple = false; // Disable grappling during the coroutine
            lineRenderer.enabled = true; // Enable the line renderer

            Vector2 currentGrapplePosition = transform.position; // Initialize with player's current position
            Vector2 tetherDirection = GetTetherDirection(); // Get the direction to extend the grapple

            float maxGrappleRange = grappleRange; // Maximum range the grapple can extend
            float grappleDistance = 0f; // Current distance of the grapple

            RaycastHit2D hit = new RaycastHit2D(); // Initialize the hit variable
            bool hitSomething = false; // Boolean to track if we've hit something

            // Extend the grapple in a straight line
            while (grappleDistance < maxGrappleRange)
            {
                grappleDistance += grappleSpeed * Time.deltaTime; // Increment the grapple distance
                currentGrapplePosition = (Vector2)transform.position + tetherDirection * grappleDistance; // Update grapple position based on player's current position

                // Perform a raycast to detect collision from the player's current position
                hit = Physics2D.Raycast(transform.position, tetherDirection, grappleDistance, groundLayer);

                if (hit.collider != null) // Check if the grapple hit something
                {
                    hitSomething = true; // Set flag to true
                    currentGrapplePosition = hit.point; // Set the grapple point to where it hit
                    break; // Exit the loop since we hit something
                }

                // Update the line renderer positions
                lineRenderer.SetPosition(0, transform.position); // Start point follows player's current position
                lineRenderer.SetPosition(1, currentGrapplePosition); // Extend the line

                yield return null; // Wait for the next frame
            }

            if (grappleDistance > maxGrappleRange)
                grappleDistance = maxGrappleRange;

            // If we hit something, pull the player towards it
            if (hitSomething)
            {
                yield return StartCoroutine(PullPlayerToGrapplePoint(currentGrapplePosition));
            }
            else
            {
                // If we didn't hit anything, just exit the grapple
                ExitGrappleState();
            }
        }

        IEnumerator PullPlayerToGrapplePoint(Vector2 grapplePoint)
        {
            float fixedGrappleDistance = Vector2.Distance(transform.position, grapplePoint); // Fixed max distance
            rb.gravityScale = 0f; // Disable gravity for smoother motion

            float inputHoldTime = 0f; // Tracks how long input is held

            while (Vector2.Distance(transform.position, grapplePoint) > 0.1f) // While not at grapple point
            {
                // Exit grapple state if the player presses grapple or jump buttons
                if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Grapple"))
                {
                    ExitGrappleState();
                    yield break;
                }

                Vector2 toGrapple = grapplePoint - (Vector2)transform.position; // Vector to grapple point
                float currentDistance = toGrapple.magnitude; // Current distance from grapple point

                if (Input.GetButton("Grapple"))
                {
                    currentSwingSpeed = rb.velocity.magnitude;
                    Debug.Log("Current Swing Speed: " + currentSwingSpeed);


                    // Maintain a fixed distance when the grapple button is held
                    Vector2 clampedPosition = grapplePoint - toGrapple.normalized * fixedGrappleDistance;
                    if (currentDistance > fixedGrappleDistance)
                    {
                        rb.position = clampedPosition; // Reset position to maintain fixed distance
                    }

                    isSwinging = true;
                    inputHoldTime += Time.deltaTime; // Increase the hold timer


                    // Clamp player's position to stay within fixed grapple distance
                    Vector2 clampedPositionn = grapplePoint - toGrapple.normalized * Mathf.Min(currentDistance, fixedGrappleDistance);
                    rb.position = clampedPosition;

                    // Swing logic: Apply forces based on input
                    Vector2 perpendicularLeft = Vector2.Perpendicular(toGrapple).normalized;
                    Vector2 perpendicularRight = -Vector2.Perpendicular(toGrapple).normalized;

                    float angle = CalculateAngle(grapplePoint, transform.position);
                    float smoothedAngle = SmoothAngle(angle); // Apply smoothing if needed
                    Debug.Log("Current Smoothed Swing Angle: " + smoothedAngle);

                    float CalculateAngle(Vector2 from, Vector2 to)
                    {
                        Vector2 direction = (to - from).normalized;
                        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    }

                    float SmoothAngle(float currentAngle)
                    {
                        // Smooth the angle using linear interpolation
                        float smoothedAngle = Mathf.Lerp(previousAngle, currentAngle, smoothingFactor);
                        previousAngle = smoothedAngle; // Update previousAngle for the next frame
                        return smoothedAngle;
                    }

                    float currentAngle = CalculateAngle(grapplePoint, transform.position);
                    float angleDifference = Mathf.Abs(targetAngle - currentAngle);
                    float resistanceFactor = Mathf.Clamp01(angleDifference / maxResistanceAngle);
                    float adjustedSwingForce = swingForce * (1f - (resistanceFactor * resistanceMultiplier));

                    // Check if the current angle is within the threshold
                    if (angleDifference <= angleThreshold)
                    {
                        // Calculate a speed multiplier based on proximity to the target angle
                        float proximityFactor = 1f - (angleDifference / angleThreshold); // Closer = higher factor (0 to 1)
                        float dynamicSwingForce = swingForce * (1f + (proximityFactor * (swingSpeedMultiplier - 1f)));

                        // Apply adjusted swing forces based on input
                        if (Input.GetAxisRaw("Horizontal") < 0) // Moving left
                        {
                            rb.AddForce(perpendicularLeft * dynamicSwingForce, ForceMode2D.Force);
                        }
                        else if (Input.GetAxisRaw("Horizontal") > 0) // Moving right
                        {
                            rb.AddForce(perpendicularRight * dynamicSwingForce, ForceMode2D.Force);
                        }

                        Debug.Log("Swing Speed Multiplier: " + (1f + (proximityFactor * (swingSpeedMultiplier - 1f))));
                    }
                    else
                    {
                        if (Input.GetAxisRaw("Horizontal") < 0) // Moving left
                        {
                            rb.AddForce(perpendicularLeft * adjustedSwingForce, ForceMode2D.Force);
                        }
                        else if (Input.GetAxisRaw("Horizontal") > 0) // Moving right
                        {
                            rb.AddForce(perpendicularRight * adjustedSwingForce, ForceMode2D.Force);
                        }
                    }



                    // Limit swing speed
                    if (rb.velocity.magnitude > speed)
                    {
                        rb.velocity = rb.velocity.normalized * speed;
                    }

                    if (requiresMaxDistance)
                    {
                        // Check if player is out of the angle range or moving horizontally
                        if ((angle < -160f || angle > -20) || Mathf.Abs(rb.velocity.x) > 0.1f)
                        {
                            // Allow transition back to strict clamping if max distance is reached
                            if (currentDistance >= maxGrappleDistance)
                            {
                                requiresMaxDistance = false; // Reset to stricter clamping
                            }
                        }
                        }

                     if (angle != -90f)
                    {
                        if (!requiresMaxDistance)
                        {
                            // Activate smoother clamping and store max distance
                            requiresMaxDistance = true;
                            maxGrappleDistance = currentDistance;
                        }

                        if (!Input.GetKey("f"))
                        {


                            clampedPosition = grapplePoint - toGrapple.normalized * Mathf.Min(currentDistance, fixedGrappleDistance);
                            rb.position = clampedPosition;
                        } // Smoother clamping logic

                        if (Input.GetKey("f"))
                        {
                            if (Input.GetAxisRaw("Horizontal") < 0) // Moving left
                            {
                                rb.AddForce(perpendicularLeft * swingForce * 20, ForceMode2D.Force); // Double the force
                            }
                            else if (Input.GetAxisRaw("Horizontal") > 0) // Moving right
                            {
                                rb.AddForce(perpendicularRight * swingForce * 20, ForceMode2D.Force); // Double the force
                            }

                        } 

                    }
                    else
                    {
                        // Check if player has reached max distance to transition back to stricter clamping
                        if (requiresMaxDistance && currentDistance >= maxGrappleDistance)
                        {
                            requiresMaxDistance = false; // Allow transition back to strict clamping
                        }

                        if (!requiresMaxDistance)
                        {
                            // Stricter clamping logic
                            if (currentDistance > fixedGrappleDistance)
                            {
                                 clampedPosition = grapplePoint - toGrapple.normalized * fixedGrappleDistance;
                                rb.position = clampedPosition;
                            }
                        }
                    }

                    // Apply centripetal force to keep the swing smooth
                    Vector2 centripetalForce = toGrapple.normalized * (swingSpeed * swingSpeed / currentDistance);
                    rb.AddForce(centripetalForce, ForceMode2D.Force);

                    // Limit swing speed
                    if (rb.velocity.magnitude > speed)
                    {
                        rb.velocity = rb.velocity.normalized * speed;
                    }

                    // Dash logic
                    if (Input.GetButton("Dash"))
                    {
                        Vector2 dashDirection = rb.velocity.normalized; // Direction of current movement
                        if (isSwinging)
                        {
                            // Add perpendicular dash force if swinging
                            dashDirection = Vector2.Perpendicular(toGrapple).normalized;
                            dashDirection *= Input.GetAxisRaw("Horizontal") > 0 ? 1 : -1; // Adjust for left or right
                        }

                        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);

                        // Optional: Clamp the velocity to avoid excessive speed
                        if (rb.velocity.magnitude > dashForce)
                        {
                            rb.velocity = rb.velocity.normalized * dashForce;
                        }
                    }
                }
                else
                {
                    // Pull player toward the grapple point when the button is not held
                    isSwinging = false; // Disable swinging
                    Vector2 direction = toGrapple.normalized;
                    rb.velocity = direction * grapplePullSpeed; // Pull the player directly
                }



                // Update line renderer to follow the player's position
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, grapplePoint);

                yield return null; // Wait for the next frame
            }

            // Stop player movement when they reach the grapple point
            rb.velocity = Vector2.zero;

            // Exit the grapple state
            ExitGrappleState();
            canGrapple = true; // Re-enable grappling
        }
        Vector2 GetTetherDirection()
        {
            float tetherX = Input.GetAxisRaw("Horizontal");
            float tetherY = Input.GetAxisRaw("Vertical");

            // Create a normalized direction vector for grappling
            return new Vector2(tetherX, tetherY).normalized;
        }

        void ExitGrappleState()
        {
            rb.velocity = Vector2.zero; // Reset player velocity
            rb.gravityScale = originalGravityScale; // Restore gravity
            lineRenderer.enabled = false; // Disable the line renderer
            canGrapple = true; // Re-enable grappling for the next input press

            if (Input.GetButtonDown("Jump"))
            {
                StartCoroutine(GrappleLeap());
            }

        }

        IEnumerator GrappleLeap()

        {
            // Apply an upward force to simulate a leap
            float leapForce = 20f; // Adjust this value for how powerful you want the leap to be

            rb.velocity = new Vector2(rb.velocity.x, leapForce); // Add a vertical boost while keeping the horizontal velocity

            // Optionally, you can have a delay here if you want the leap to last for a set time
            yield return new WaitForSeconds(0.2f); // Wait for 0.2 seconds for the leap effect to take place

            // After the leap, you can stop the coroutine
        }


        void HandleTetherJumpState()
        {

            rb.velocity = new Vector2(dashForce, dashForce);



        }



    }



    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector2 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }



}