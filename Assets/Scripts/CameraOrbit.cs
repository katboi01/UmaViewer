using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraOrbit : MonoBehaviour
{
    public static CameraOrbit instance;
    public int CameraMode = 0;

    [Header("Light")]
    public GameObject Light;

    public TMP_Dropdown CameraModeDropdown;

    public GameObject CameraTargetHelper;
    public Vector3 TargetCenter;

    public EventSystem eventSystem;
    bool controlOn;

    [Header("Orbit Camera")]
    float camDistMin = 1, camDistMax = 15;

    [Header("Free Camera")]
    bool FreeCamLeft = false;
    bool FreeCamRight = false;
    private Quaternion lookRotation;

    private UISettingsCamera CameraSettings => UmaViewerUI.Instance.CameraSettings;

    void Start()
    {
        CameraSettings.CameraDistance.minValue = camDistMin;
        CameraSettings.CameraDistance.maxValue = camDistMax;

        lookRotation = transform.localRotation;
        instance = this;
    }

    void Update()
    {
        if (HandleManager.InteractionInProgress) return;

        bool cameraModeChanged = CameraMode != CameraSettings.CameraMode;
        if (cameraModeChanged)
        {
            CameraMode = CameraSettings.CameraMode;
        }

        switch (CameraMode)
        {
            case 0:
                OrbitAround();
                OrbitLight();
                break;
            case 1:
                if (cameraModeChanged)
                {
                    lookRotation.y = transform.eulerAngles.y;
                }
                FreeCamera();
                break;
        }
    }

    #region PC controls
    /// <summary>
    /// Rotate Light Source
    /// </summary>
    private void OrbitLight()
    {
        if (Input.GetMouseButton(1))
        {
            Light.transform.Rotate(Input.GetAxis("Mouse X") * Vector3.up, Space.Self);
            Light.transform.Rotate(Input.GetAxis("Mouse Y") * Vector3.right, Space.Self);
        }
    }

    /// <summary>
    /// Orbit camera around world center
    /// </summary>
    void OrbitAround(bool aroundCharacter = false)
    {
        TargetCenter = Vector3.zero;

        Camera.main.fieldOfView = CameraSettings.FOV;
        Vector3 position = transform.position;

        if (Input.GetMouseButtonDown(0) && !eventSystem.IsPointerOverGameObject())
        {
            controlOn = true;
        }
        if (Input.GetMouseButton(0) && controlOn)
        {
            position -= Input.GetAxis("Mouse X") * transform.right * CameraSettings.MovementSpeed;
            CameraSettings.CameraHeight -= Input.GetAxis("Mouse Y") * CameraSettings.MovementSpeed;
        }
        else
        {
            controlOn = false;
        }

        if (!eventSystem.IsPointerOverGameObject())
        {
            CameraSettings.CameraDistance.value -= Input.mouseScrollDelta.y * CameraSettings.ZoomSpeed;
        }

        if ((Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.LeftControl)) && !eventSystem.IsPointerOverGameObject())
        {
            CameraTargetHelper.SetActive(true);
        }
        if ((Input.GetMouseButton(2) || Input.GetKey(KeyCode.LeftControl)) && CameraTargetHelper.activeSelf)
        {
            CameraSettings.TargetHeight -= Input.GetAxis("Mouse Y");
            CameraTargetHelper.transform.position = CameraSettings.TargetHeight * Vector3.up;
        }
        if (Input.GetMouseButtonUp(2) || Input.GetKeyUp(KeyCode.LeftControl))
        {
            CameraTargetHelper.SetActive(false);
        }

        float camDist = CameraSettings.CameraDistance.value;
        CameraSettings.CameraHeightSlider.maxValue = camDist + 1 - camDist * 0.2f;
        CameraSettings.CameraHeightSlider.minValue = -camDist + 1 + camDist * 0.2f;

        Vector3 target = TargetCenter + CameraSettings.TargetHeight * Vector3.up; //set target offsets

        position.y = TargetCenter.y + CameraSettings.CameraHeight; //set camera height
        transform.position = position;  //set final position of camera at target
        transform.LookAt(target); //look at target position
        transform.Rotate(0,0, CameraSettings.CameraRotation);
        transform.position = target - transform.forward * camDist; //move away from target
    }

    /// <summary>
    /// Free Camera, Unity-Style
    /// </summary>
    void FreeCamera()
    {
        Camera.main.fieldOfView = CameraSettings.FOV;
        float moveSpeed = CameraSettings.MovementSpeed;;
        float rotateSpeed = CameraSettings.RotationSpeed;

        if (Input.GetMouseButtonDown(0) && !eventSystem.IsPointerOverGameObject())
        {
            FreeCamLeft = true;
        }
        if (Input.GetMouseButton(0) && FreeCamLeft)
        {
            transform.position += Input.GetAxis("Vertical") * transform.forward * Time.deltaTime * moveSpeed;
            transform.position += Input.GetAxis("Horizontal") * transform.right * Time.deltaTime * moveSpeed;

            lookRotation.x -= Input.GetAxis("Mouse Y") * rotateSpeed;
            lookRotation.y += Input.GetAxis("Mouse X") * rotateSpeed;

            lookRotation.x = Mathf.Clamp(lookRotation.x, -90, 90);
        }
        else
        {
            FreeCamLeft = false;
        }
        transform.localRotation = Quaternion.Euler(lookRotation.x, lookRotation.y, CameraSettings.CameraRotation);

        if (Input.GetMouseButtonDown(1) && !eventSystem.IsPointerOverGameObject())
        {
            FreeCamRight = true;
        }
        if (Input.GetMouseButton(1) && FreeCamRight)
        {
            transform.position += Input.GetAxis("Mouse X") * transform.right  * Time.deltaTime * moveSpeed;
            transform.position += Input.GetAxis("Mouse Y") * transform.up * Time.deltaTime * moveSpeed;
        }
        else
        {
            FreeCamRight = false;
        }

    }
    #endregion
}
