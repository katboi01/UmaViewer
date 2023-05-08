using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class CameraOrbit : MonoBehaviour
{
    public static CameraOrbit instance;
    public int CameraMode = 0;
    public int AAMode = 0;

    [Header("Light")]
    public GameObject Light;

    public Dropdown CameraModeDropdown;

    public Dropdown AAModeDropdown;

    public GameObject CameraTargetHelper;
    public Vector3 TargetCenter;

    EventSystem eventSystem;
    bool controlOn;


    [Header("Orbit Camera")]
    public GameObject OrbitCamSettingsTab;
    public Slider OrbitCamFovSlider;
    public Slider OrbitCamZoomSlider;
    public Slider OrbitCamZoomSpeedSlider;
    public Slider OrbitCamTargetHeightSlider;
    public Slider OrbitCamHeightSlider;
    public Slider OrbitCamRotationSlider;
    public Slider OrbitCamSpeedSlider;
    float camDistMin = 1, camDistMax = 15;

    [Header("Free Camera")]
    public GameObject FreeCamSettingsTab;
    public Slider FreeCamFovSlider;
    public Slider FreeCamRotationSlider;
    public Slider FreeCamMoveSpeedSlider;
    public Slider FreeCamRotateSpeedSlider;
    bool FreeCamLeft = false;
    bool FreeCamRight = false;
    private Quaternion lookRotation;

    void Start()
    {
        OrbitCamZoomSlider.minValue = camDistMin;
        OrbitCamZoomSlider.maxValue = camDistMax;

        eventSystem = EventSystem.current;

        lookRotation = transform.localRotation;
        instance = this;
    }

    void Update()
    {
        if (CameraMode != CameraModeDropdown.value)
        {
            switch (CameraMode) //old
            {
                case 0:
                    OrbitCamSettingsTab.SetActive(false);
                    break;
                case 1:
                    FreeCamSettingsTab.SetActive(false);
                    break;
            }
            switch (CameraModeDropdown.value) //new
            {
                case 0:
                    OrbitCamSettingsTab.SetActive(true);
                    break;
                case 1:
                    FreeCamSettingsTab.SetActive(true);
                    lookRotation.y = transform.eulerAngles.y;
                    break;
            }
            CameraMode = CameraModeDropdown.value;
        }

        if (AAMode != AAModeDropdown.value)
        {
            var postprocessLayerMain = Camera.main.GetComponent<PostProcessLayer>();
            var postprocessLayerPreview = UmaViewerBuilder.Instance.AnimationCamera.GetComponent<PostProcessLayer>();
            postprocessLayerMain.antialiasingMode = postprocessLayerPreview.antialiasingMode = (PostProcessLayer.Antialiasing)AAModeDropdown.value;
            Debug.Log($"Switched AntiAliasing to {(PostProcessLayer.Antialiasing)AAModeDropdown.value}");
            AAMode = AAModeDropdown.value;
        }
        switch (CameraModeDropdown.value)
        {
            case 0:
                OrbitAround();
                OrbitLight();
                break;
            case 1: FreeCamera(); break;
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

        Camera.main.fieldOfView = OrbitCamFovSlider.value;
        Vector3 position = transform.position;

        if (Input.GetMouseButtonDown(0) && !eventSystem.IsPointerOverGameObject())
        {
            controlOn = true;
        }
        if (Input.GetMouseButton(0) && controlOn)
        {
            position -= Input.GetAxis("Mouse X") * transform.right * OrbitCamSpeedSlider.value;
            OrbitCamHeightSlider.value -= Input.GetAxis("Mouse Y") * OrbitCamSpeedSlider.value;
        }
        else
        {
            controlOn = false;
        }

        if (!eventSystem.IsPointerOverGameObject())
        {
            OrbitCamZoomSlider.value -= Input.mouseScrollDelta.y * OrbitCamZoomSpeedSlider.value;
        }

        float camDist = OrbitCamZoomSlider.value;
        OrbitCamHeightSlider.maxValue = camDist + 1 - camDist * 0.2f;
        OrbitCamHeightSlider.minValue = -camDist + 1 + camDist * 0.2f;

        Vector3 target = TargetCenter + OrbitCamTargetHeightSlider.value * Vector3.up; //set target offsets

        position.y = TargetCenter.y + OrbitCamHeightSlider.value; //set camera height
        transform.position = position;  //set final position of camera at target
        transform.LookAt(target); //look at target position
        transform.Rotate(0, 0, OrbitCamRotationSlider.value);
        transform.position = target - transform.forward * camDist; //move away from target
    }

    /// <summary>
    /// Free Camera, Unity-Style
    /// </summary>
    void FreeCamera()
    {
        Camera.main.fieldOfView = FreeCamFovSlider.value;
        float moveSpeed = FreeCamMoveSpeedSlider.value;
        float rotateSpeed = FreeCamRotateSpeedSlider.value;

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
        transform.localRotation = Quaternion.Euler(lookRotation.x, lookRotation.y, FreeCamRotationSlider.value);

        if (Input.GetMouseButtonDown(1) && !eventSystem.IsPointerOverGameObject())
        {
            FreeCamRight = true;
        }
        if (Input.GetMouseButton(1) && FreeCamRight)
        {
            transform.position += Input.GetAxis("Mouse X") * transform.right * Time.deltaTime * moveSpeed;
            transform.position += Input.GetAxis("Mouse Y") * transform.up * Time.deltaTime * moveSpeed;
        }
        else
        {
            FreeCamRight = false;
        }

    }
    #endregion
}
