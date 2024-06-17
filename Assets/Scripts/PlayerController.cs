using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public Rigidbody _rb;

    public enum PlayerState
    {
        IDLE, JOGGING, RUNNING, SNEAKING, JUMPING, FALLING
    }

    public PlayerState currentState = PlayerState.IDLE;

    [Header(" Actions ")]
    public InputAction _moveAction;
    public InputAction _jumpAction;
    public InputAction _runAction;
    public InputAction _sneakAction;

    private bool _jumpPerformed = false;
    private bool _runPerformed = false;
    private bool _sneakPerformed = false;

    [Header("GroudChecker")]
    public Transform groundDetector;
    public Vector3 groundDetectorSize = new Vector3(0.6f, 0.1f, 0.6f);
    private bool _isGrounded = true;
    private Collider[] _floorCollider;
    public LayerMask _layerMask;

    //Switches
    public bool _isIdle;
    public bool _isJumping;
    public bool _isFalling;
    public bool _isJogging;
    public bool _isRunning;
    public bool _isSneaking;

    public Vector2 left_stick;

    private bool _canJump = true;
    private bool _canRun = true;
    private bool _canSneak = true;
    private bool _canMove = true;

    private float currentSpeed;

    public float jogSpeed = 5f;
    public float runSpeed = 8f;
    public float sneakSpeed = 3f;

    public float rayLength = 0.5f;


    private void OnEnable()
    {
        _moveAction.Enable();
        _jumpAction.Enable();
        _runAction.Enable();
        _sneakAction.Enable();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        GroundChecker();
        
        MoveInput();
        RunInput();
        JumpInput();
        SneakInput();
        

        OnStateUpdate();

    }

    public void OnStateEnter()
    {
        switch (currentState)
        {
            case PlayerState.IDLE:
                _isIdle = true;
                _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
                break;
            case PlayerState.JOGGING:
                _isJogging = true;
                currentSpeed = jogSpeed;
                break;
            case PlayerState.RUNNING:              
                _isRunning = true;
                currentSpeed = runSpeed;

                _canSneak = false;
                break;
            case PlayerState.SNEAKING:
                _isSneaking = true;
                currentSpeed = sneakSpeed;

                _canRun = false;
                break;
            case PlayerState.JUMPING:
                _isJumping = true;
                _jumpPerformed = false;
                _rb.AddForce(new Vector3(0, 5f, 0), ForceMode.VelocityChange);

                _canJump = false;
                break;
            case PlayerState.FALLING:
                _isFalling = true;

                _canJump = false;
                break;
            default:
                break;
        }
    }

    public void OnStateUpdate()
    {
        switch (currentState)
        {
            case PlayerState.IDLE:
                DistanceToGround();
                if (!_isGrounded)
                {
                    TransitionToState(PlayerState.FALLING);
                }
                else if (left_stick.magnitude > 0)
                {
                    TransitionToState(PlayerState.JOGGING);
                }
                else if (_jumpPerformed)
                {
                    TransitionToState(PlayerState.JUMPING);
                }
                break;
            case PlayerState.JOGGING:
                DistanceToGround();
                if (!_isGrounded)
                {
                    TransitionToState(PlayerState.FALLING);
                }
                else if (_runPerformed)
                {
                    TransitionToState(PlayerState.RUNNING);
                }
                else if (_sneakPerformed)
                {
                    TransitionToState(PlayerState.SNEAKING);
                }
                else if (_jumpPerformed)
                {
                    TransitionToState(PlayerState.JUMPING);
                }
                else if (left_stick.magnitude == 0)
                {
                    TransitionToState(PlayerState.IDLE);
                }
                break;
            case PlayerState.RUNNING:
                DistanceToGround();
                if (!_isGrounded)
                {
                    TransitionToState(PlayerState.FALLING);
                }
                else if (!_runPerformed)
                {
                    TransitionToState(PlayerState.JOGGING);
                }
                else if (_jumpPerformed)
                {
                    TransitionToState(PlayerState.JUMPING);
                }
                else if (left_stick.magnitude == 0)
                {
                    TransitionToState(PlayerState.IDLE);
                }
                break;
            case PlayerState.SNEAKING:
                DistanceToGround();
                if (!_isGrounded)
                {
                    TransitionToState(PlayerState.FALLING);
                }
                else if (!_sneakPerformed)
                {
                    TransitionToState(PlayerState.JOGGING);
                }
                else if (_jumpPerformed)
                {
                    TransitionToState(PlayerState.JUMPING);
                }
                else if (left_stick.magnitude == 0)
                {
                    TransitionToState(PlayerState.IDLE);
                }
                break;
            case PlayerState.JUMPING:
                if (_rb.velocity.y <= 0)
                {
                    TransitionToState(PlayerState.FALLING);
                }
                break;
            case PlayerState.FALLING:
                if (_isGrounded)
                {
                    TransitionToState(PlayerState.IDLE);
                }
                break;
            default:
                break;
        }
    }

    public void OnStateExit()
    {
        switch (currentState)
        {
            case PlayerState.IDLE:
                _isIdle = false;
                break;
            case PlayerState.JOGGING:
                _isJogging = false;
                break;
            case PlayerState.RUNNING:
                _isRunning = false;

                _canSneak = true;
                break;
            case PlayerState.SNEAKING:
                _isSneaking = false;

                _canRun = true;
                break;
            case PlayerState.JUMPING:
                _isJumping = false;

                _canJump = true;
                break;
            case PlayerState.FALLING:
                _isFalling = false;

                _canJump = true;
                break;
            default:
                break;
        }
    }

    public void TransitionToState(PlayerState state)
    {
        OnStateExit();
        currentState = state;
        OnStateEnter();
    }


    #region Input
    private void MoveInput()
    {
        left_stick = _moveAction.ReadValue<Vector2>();

        //if (_moveAction.WasPerformedThisFrame()) 
        if (left_stick.magnitude > 0f && _canMove)
        {
            //On met l'axe X en X pour se déplacer de gauche à droite
            //On ne touche pas l'axe Y pour ne oas ecraser la vitesse de chute
            //On met l'axe Y en Z pour se deplacer d'avant en arrière
            _rb.velocity = new Vector3(left_stick.x * currentSpeed, _rb.velocity.y, left_stick.y * currentSpeed) ;
        }

        //
    }

    private void JumpInput()
    {
        if (_jumpAction.WasPerformedThisFrame() && _canJump)
        {
            _jumpPerformed = true;
        }
    }

    private void SneakInput()
    {
        if (_sneakAction.phase == InputActionPhase.Started && _canSneak)
        {
            _sneakPerformed = true;
        }
        else
        {
            _sneakPerformed = false;
        }
    }

    private void RunInput()
    {
        if (_runAction.phase == InputActionPhase.Started && _canRun)
        {
            _runPerformed = true;
        }
        else
        {
            _runPerformed = false;
        }
    }

    private void GroundChecker()
    {

        _floorCollider = Physics.OverlapBox(groundDetector.position, groundDetectorSize, groundDetector.localRotation, _layerMask);

        if (_floorCollider.Length > 0)
        {
            _isGrounded = true;
            _rb.useGravity = false;

        }
        else
        {
            _isGrounded = false;
            _rb.useGravity = true;

        }

    }
    #endregion


    private void DistanceToGround()
    {
        if (Physics.Raycast( transform.position, Vector3.down, out RaycastHit hit, rayLength, _layerMask))
        {
            _rb.MovePosition(hit.point + new Vector3(0f, rayLength, 0f));

        }
    }

    private void OnDrawGizmos()
    {
        if (_isGrounded)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawCube(groundDetector.position, groundDetectorSize);

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, Vector3.down * rayLength);
    }
}
