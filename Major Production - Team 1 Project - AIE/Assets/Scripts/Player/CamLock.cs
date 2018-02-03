using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamLock : MonoBehaviour
{
    public float mouseSensitivityY;
    public float mouseSensitivityX;
    public float controllerSensitivityY = 1; //Y axis for Controller Sensitivity
    public float controllerSensitivityX = 1.25f; //X axis for Controller Sensitivity
    public float cameraHeightAdjustment = 2; //where to place the camera
    public float cameraDistanceAdjustment = -2; //where to place the camera
    public float minCameraDistanceFromPlayer = 2f; //stops the player from going past the camera when moving backwards
    public float smoothTime = 0.3F; //how long to take to catch-up with the player rotation and movement
    public bool invert = false;

    private Vector3 velocity = Vector3.zero;

    private float VerticleAngleMinRotation = -45f; //making these private as > 45 causes the camera to flip
    private float VerticalAngleMaxRotation = 45f;
    private float minCameraDistance = 3f; //stops the player from going past the camera when moving backwards

    [HideInInspector]
    public float floatSpeedOfSid = 0.0f;
    private bool isController = false;
    public GameObject player;
    private Rigidbody playerrb;

    private RaycastHit rc;
    private float currentHorizontal = 0;
    private float currentVertical = 0;

    [HideInInspector] public bool isWin; //if the winscreen is on, then this will show the cursor

    void Start()
    {
        //All values for sensitivity are taken from the controls menu - UpdateSensTxt.cs - Jak
        mouseSensitivityX = UpdateSensTxt.mouseSensX;
        mouseSensitivityY = UpdateSensTxt.mouseSensY;
        controllerSensitivityX = UpdateSensTxt.controllerSensX;
        controllerSensitivityY = UpdateSensTxt.controllerSensY;
        invert = UpdateSensTxt.invertToSend;

        player = GameObject.FindGameObjectWithTag("Player");
        playerrb = player.GetComponent<Rigidbody>();
        currentHorizontal = player.transform.eulerAngles.y;
        currentVertical = player.transform.eulerAngles.x;
    }

    //Called from playerPossession
    private void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerrb = player.GetComponent<Rigidbody>();
        currentHorizontal = player.transform.eulerAngles.y;
        currentVertical = player.transform.eulerAngles.x;

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (isController == true)
                isController = false;
            else isController = true;
        }
        if (!invert)
        {
            if (isController == true)
            {
                currentHorizontal += Input.GetAxis("CameraHorizontal") * controllerSensitivityX;
                currentVertical -= Input.GetAxis("CameraVertical") * controllerSensitivityY;

                Cursor.lockState = CursorLockMode.Locked; //Lock cursor to center of screen and hide it - Jak
            }
            else
            {
                currentHorizontal += Input.GetAxis("Mouse X") * mouseSensitivityX;
                currentVertical -= Input.GetAxis("Mouse Y") * mouseSensitivityY;

                Cursor.lockState = CursorLockMode.Locked; //Lock cursor to center of screen and hide it - Jak
            }
        }
        else
        {
            if (isController == true)
            {
                currentHorizontal += Input.GetAxis("CameraHorizontal") * controllerSensitivityX;
                currentVertical += Input.GetAxis("CameraVertical") * controllerSensitivityY;

                Cursor.lockState = CursorLockMode.Locked; //Lock cursor to center of screen and hide it - Jak
            }
            else
            {
                currentHorizontal += Input.GetAxis("Mouse X") * mouseSensitivityX;
                currentVertical += Input.GetAxis("Mouse Y") * mouseSensitivityY;

                Cursor.lockState = CursorLockMode.Locked; //Lock cursor to center of screen and hide it - Jak
            }
        }
        currentVertical = Mathf.Clamp(currentVertical, VerticleAngleMinRotation, VerticalAngleMaxRotation);
    }

    private void FixedUpdate()
    {
        if (player.GetComponent<playerPossession>().isHidden())
        {
            Debug.Log("CamLock rotateAround");
            Camera.main.transform.LookAt(player.transform);
        }
        else
        {
            //calculate the amount to rotate the player
            Quaternion rotation = Quaternion.Euler(currentVertical, currentHorizontal, 0);

            //rotate the player
            player.transform.rotation = rotation;

            //adjust beacuse the players pivot point is at its base and the camera needs to be behind the player
            Vector3 adjustedPlayerPosition = player.transform.position + (player.transform.up * cameraHeightAdjustment);
            Vector3 adjustedCameraPosition = player.transform.forward * cameraDistanceAdjustment;
        }
    }
}