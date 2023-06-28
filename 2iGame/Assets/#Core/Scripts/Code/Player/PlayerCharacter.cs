using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    #region Variables
    public bool debug = false;
    private List<LineRenderer> d_lineRenderers = new List<LineRenderer>();
    [Range(1,20)]public float simSpeed = 20;


    #region Combat

    #endregion


    #region Movement Settings
    [Header("Movement Settings")]
    private Vector3 lastPosition;
    private Vector3 lastMousePosition = Vector3.zero;
    public bool canMove = true;
    private bool camYLocked = true;
    public static float playerSpeedMultiplier = 1f; //A multipler added to the basePlayerSpeed.
    private static float basePlayerSpeed = 1f;
    public float gravityForce = 1f;

    public float playerSpeed = basePlayerSpeed * playerSpeedMultiplier;
    private float currMovementSpeed;

    private float sprintSpeed; 
    bool sprinting = false;

    public ForceMode forceMode;
    public float MaximumVelocity = 1f;
    
    public float lookSpeed = 1f; //Horizontal rotation speed.
    private float currRotationX;
    private float currRotationY;
    public float bobbingAmount = 0.05f;
    float defaultPosY = 0;
    float timer = 0;
    #endregion

    
    #region Player Parts
    [Header("Player Parts")]
    public Camera playerCamera;
    public GameObject userInterface;
    public PlayerWeapon equippedWeapon; 
    [HideInInspector]public Rigidbody rb;
    #endregion
 

    #region Methods
    /// <summary>
    /// Determines if the player is currently walking or moving
    /// </summary>
    /// <returns>True if the player currently moving in any direction</returns>
    public bool isWalking()
    {
        if (rb.velocity.magnitude > 0.5)
        {
            return true;
        }
        else return false;
    }

    float distToGround = 1f;
    /// <summary>
    /// Determines if the player is currently grounded.
    /// </summary>
    /// <returns>True if player is currently on top of a floor (with Layer "Floor")</returns>
    public bool isGrounded()
    {
        int layer_mask = LayerMask.GetMask("Floor");

        bool result = Physics.Raycast(this.transform.position, -Vector3.up, distToGround + 0.1f, layer_mask);
        return result;
    }
    #endregion
    #endregion

    #region Unity Methods
    void Awake()
    {
        //Lock in the cursor and assign our Rigidbody
        Cursor.lockState = CursorLockMode.Locked;
        rb = this.GetComponent<Rigidbody>();
    }

    void Start()
    {
        currRotationX = this.transform.eulerAngles.y;
        currRotationY = playerCamera.transform.localEulerAngles.x;

        defaultPosY = playerCamera.transform.localPosition.y;
    }

    void Update()
    {
        //Last Position is saved at the start of every frame
        lastPosition = this.transform.position;

        //Handlers
        HandlePlayerInput();
        Log(isGrounded());
    }

    void FixedUpdate()
    {
        //Handlers
        HandlePlayerHealth();
        HandlePlayerMovement();
        HandlePlayerLook();
        HandlePlayerBobbing();
    }
    #endregion

    #region Handlers
    void HandlePlayerHealth()
    {

    }

    /// <summary>
    /// Handles the player input for movement (WASD) along with sprinting
    /// </summary>
    void HandlePlayerMovement()
    {
        sprintSpeed = playerSpeed * 3 / 2;        
        
        if (canMove && isGrounded())
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                sprinting = true;
                currMovementSpeed = sprintSpeed;
            }
            else
            {
                currMovementSpeed = playerSpeed;
                sprinting = false;
            }

            if (Input.GetKey(KeyCode.W))
            {
                rb.AddForce(this.transform.forward * currMovementSpeed, forceMode);
            }

            if (Input.GetKey(KeyCode.A))
            {
                rb.AddForce(-this.transform.right * currMovementSpeed, forceMode);
            }

            if (Input.GetKey(KeyCode.S))
            {
                rb.AddForce(-1 * this.transform.forward * currMovementSpeed, forceMode);
            }

            if (Input.GetKey(KeyCode.D))
            {
                rb.AddForce(this.transform.right * currMovementSpeed, forceMode);
            }

            float tmpMaxVolocity = MaximumVelocity;

            if (sprinting) tmpMaxVolocity *= 2;

            rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -tmpMaxVolocity, tmpMaxVolocity),
                              rb.velocity.y,
                              Mathf.Clamp(rb.velocity.z, -tmpMaxVolocity, tmpMaxVolocity));
        }
        else if (!isGrounded() && canMove)
        {
            rb.AddForce(-this.transform.up * gravityForce, forceMode);
        }
    }
   
    /// <summary>
    /// Handles the mouse input for player looking
    /// </summary>
    void HandlePlayerLook()
    {
        if (Input.mousePosition != lastMousePosition)
        {
            camYLocked = false;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            currRotationY = 0f;
            camYLocked = true;
        }

        if (!camYLocked)
        {
            //We capture the mouse Axises and multiply it by our lookSpeed, we then use this to rotate the player body (this) and the Camera
            currRotationX += Input.GetAxis("Mouse X") * lookSpeed;
            currRotationY -= Input.GetAxis ("Mouse Y") * lookSpeed;
    
            currRotationY = Mathf.Clamp(currRotationY, -30f, 30f);
        }
        else 
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                currRotationX -= lookSpeed / 2;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                currRotationX += lookSpeed / 2;
            }
        }

        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, Mathf.LerpAngle(this.transform.eulerAngles.y, currRotationX, simSpeed * Time.deltaTime), this.transform.eulerAngles.z);;
        playerCamera.transform.localEulerAngles = new Vector3(Mathf.LerpAngle(playerCamera.transform.localEulerAngles.x, currRotationY, simSpeed * Time.deltaTime), playerCamera.transform.localEulerAngles.y, playerCamera.transform.localEulerAngles.z);
    }

    void HandlePlayerBobbing()
    {
        if(isWalking())
        {
            timer += Time.deltaTime * simSpeed;
            playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, defaultPosY + Mathf.Sin(timer) * bobbingAmount, playerCamera.transform.localPosition.z);
        }
        else
        {
            timer = 0;
            playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, Mathf.Lerp(playerCamera.transform.localPosition.y, defaultPosY, Time.deltaTime * simSpeed), playerCamera.transform.localPosition.z);
        }
    }

    /// <summary>
    /// Handles player input for interactions
    /// </summary>
    void HandlePlayerInput()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.UpArrow)) Fire();
    }
    #endregion

    #region Custom Methods
    /// <summary>
    /// A method to fire from the current player
    /// </summary>
    public void Fire()
    {
        Log("BANG!");
        var ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (debug)
            {
                d_lineRenderers.Add(DrawLine(this.transform.position, new Vector3(hit.point.x, hit.point.y, hit.point.z)));
            }
        }
    }
    #endregion

    #region Debug
    ///<summary>
    /// A method for logging messages.
    ///</summary>
    ///<param name="obj"> The object you would like to display in this logged message.</param>
    ///<param name="type"> Which type would you like to display this logged message as? ("log"/"warn"/"error"/"assert")</param>
    public static void Log(object obj, string type = "log")
    {
        switch (type)
        {
            case "log" : Debug.Log(obj); break;
            case "warn" : Debug.LogWarning(obj); break;
            case "error" : Debug.LogError(obj); break;
            case "assert" : Debug.LogAssertion(obj); break;
        }
    }

    public static LineRenderer DrawLine(Vector3 startPos, Vector3 endPos, float width = 0.2f, float[] RGBAcolor = null)
    {
        GameObject lRendObj = (new GameObject("debug_linerenderer"));
        LineRenderer lRend = lRendObj.AddComponent(typeof(LineRenderer)) as LineRenderer;

        lRend.material = new Material(Shader.Find("Diffuse"));

        if (RGBAcolor == null) RGBAcolor = new float[] {1,0,0,1};
        Color lColor = new Color(RGBAcolor[0],RGBAcolor[1],RGBAcolor[2],RGBAcolor[3]);

        lRend.startColor = lColor;
        lRend.endColor = lColor;
                
        lRend.startWidth = width;
        lRend.endWidth = width;
        lRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        lRend.SetPosition(0, startPos);
        lRend.SetPosition(1, endPos);
        return lRend;
    }

    #endregion
}
