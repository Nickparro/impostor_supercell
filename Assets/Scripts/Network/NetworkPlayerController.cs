using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class NetworkPlayerController : NetworkBehaviour
{
    [Header("References")]
    public Transform neckTransform;  
    public Transform cameraTransform;   

    [Header("Sensitivity")]
    public float horizontalSensitivity = 2f;  
    public float verticalSensitivity = 2f;    

    [Header("Rotation Limits")]
    public float minVerticalAngle = -45f;   
    public float maxVerticalAngle = 45f;   
    public float minNeckAngleZ = -45f;     
    public float maxNeckAngleZ = 45f;       

    private float verticalLookRotation = 0f;
    private float neckLookRotation = 0f;

    private Joystick lookJoystick;


    private NetworkVariable<Quaternion> neckRotation = new NetworkVariable<Quaternion>(
    writePerm: NetworkVariableWritePermission.Owner);


    private Vector2 lastTouchPosition;
    private bool isDragging = false;

    void Start()
    {
        if (!IsOwner)
        {
            cameraTransform.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            HandleTouchLook();
#if UNITY_EDITOR
            //if (Input.GetMouseButton(0))
            //{
            //    HandleMouseLook();
            //}
#else
        HandleTouchLook();
#endif
            neckTransform.localEulerAngles = new Vector3(verticalLookRotation, 0f, neckLookRotation);
            neckRotation.Value = neckTransform.localRotation;
        }
        else
        {
            neckTransform.localRotation = Quaternion.Lerp(
                neckTransform.localRotation,
                neckRotation.Value,
                Time.deltaTime * 10f);
        }
    }

    void HandleTouchLook()
    {
        Vector2 lookInput = new Vector2(lookJoystick.Horizontal, lookJoystick.Vertical);

        if (lookInput.magnitude > 0.1f) 
        {
            float sensitivity = 100f;
            neckLookRotation -= lookInput.x * sensitivity * Time.deltaTime;
            verticalLookRotation -= lookInput.y * sensitivity * Time.deltaTime;

            neckLookRotation = Mathf.Clamp(neckLookRotation, -30f, 30f);
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -1f, 30f);
        }
    }


    void HandleMouseLook()
    {

        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;

        neckLookRotation -= mouseX;
        neckLookRotation = Mathf.Clamp(neckLookRotation, minNeckAngleZ, maxNeckAngleZ);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, minVerticalAngle, maxVerticalAngle);
    }
}
