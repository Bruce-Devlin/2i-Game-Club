using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    #region Variables
    #region Movement Settings
    [Header("Movement Settings")]
    public bool canMove = true;
    public static float playerSpeedMultiplier = 1f; //A multipler added to the basePlayerSpeed.
    private static float basePlayerSpeed = 1f;

    [SerializeField]public float playerSpeed = basePlayerSpeed * playerSpeedMultiplier;
    [SerializeField]public float currMovementSpeed;

    private float sprintSpeed; 

    public ForceMode forceMode;
    public float MaximumVelocity = 1f;
    
    public float lookSpeed = 1f; //Horizontal rotation speed.
    private float currRotation = 0.0f;
    #endregion
    #region Player Parts
    [Header("Player Parts")]
    public Camera playerCamera;
    public GameObject playerBody;
    
    [SerializeField]public bool sprinting = false;
    private Vector3 lastPosition;
    [SerializeField]public bool isWalking()
    {
        bool result;
        Vector3 newPos = this.transform.position;
        if (newPos.x.ToString("0.0") != lastPosition.x.ToString("0.0") || newPos.z.ToString("0.0") != lastPosition.z.ToString("0.0"))
        {
            lastPosition = newPos;
            return true;
        }
        else return false;
        
    }

    float distToGround = 1f;
    [SerializeField]public bool isGrounded()
    {
        int layer_mask = LayerMask.GetMask("Floor");

        bool result = Physics.Raycast(playerBody.transform.position, -Vector3.up, distToGround + 5f, layer_mask);
        return result;
    }
    #endregion
    #endregion

    #region Unity Methods
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Start()
    {
        
    }

    void Update()
    {
        lastPosition = playerBody.transform.position;

        #region PlayerLook
        //We capture the mouse Axis X and multiply it with our lookSpeed to determine where to move the PlayerBody rotation
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        currRotation += mouseX;
        playerBody.transform.eulerAngles = new Vector3(0, currRotation, 0.0f);
        #endregion
    }

    void FixedUpdate()
    {
        #region PlayerMovement
        sprintSpeed = playerSpeed * 2;
        Rigidbody rb = playerBody.GetComponent<Rigidbody>();
        if (canMove && isGrounded())
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                sprinting = true;
                currMovementSpeed = sprintSpeed;
            }
            else
            {
                sprinting = false;
                currMovementSpeed = playerSpeed;
            }

            if (Input.GetKey(KeyCode.W))
            {
                rb.AddForce(playerBody.transform.forward * currMovementSpeed, forceMode);
            }

            if (Input.GetKey(KeyCode.A))
            {
                rb.AddForce(-playerBody.transform.right * currMovementSpeed, forceMode);
            }

            if (Input.GetKey(KeyCode.S))
            {
                rb.AddForce(-1 * playerBody.transform.forward * currMovementSpeed, forceMode);
            }

            if (Input.GetKey(KeyCode.D))
            {
                rb.AddForce(playerBody.transform.right * currMovementSpeed, forceMode);
            }

            float tmpMaxVolocity = MaximumVelocity;

            if (sprinting) tmpMaxVolocity *= 2;

            rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -tmpMaxVolocity, tmpMaxVolocity),
                              rb.velocity.y,
                              Mathf.Clamp(rb.velocity.z, -tmpMaxVolocity, tmpMaxVolocity));
        }
        #endregion
    }
    #endregion
    #region Custom Methods

    ///<summary>
    /// A method for logging messages.
    ///</summary>
    ///<param name="txt"> The text you would like to display in this logged message.</param>
    ///<param name="type"> Which type would you like to display this logged message as? ("log"/"warn"/"error"/"assert")</param>
    public static void Log(string txt, string type = "log")
    {
        switch (type)
        {
            case "log" : Debug.Log(txt); break;
            case "warn" : Debug.LogWarning(txt); break;
            case "error" : Debug.LogError(txt); break;
            case "assert" : Debug.LogAssertion(txt); break;
        }
    }
    #endregion
}
