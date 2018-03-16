using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SmoothFollowWithCameraBumper : MonoBehaviour
{

    public Transform target = null; //Camera's target
    public float distance; //Distance maintained from target
    public float height = 1.0f; //Height of Camera
    public Vector3 camera_origin = new Vector3(0.0f, 0.3f, 0.0f); //target to move camera towards, to move away from hit with back raycast

    [SerializeField]
    private float damping = 5.0f; //How much the lerp should be slowed
    [SerializeField]
    private bool smoothRotation = true; //Whether or not smooth rotation is enabled
    [SerializeField]
    private float rotationDamping = 10.0f; //Damping on camera rotation
     
    [SerializeField]
    public Vector3 targetLookAtOffset; // allows offsetting of camera lookAt, very useful for low bumper heights

    [SerializeField]
    private float bumperDistanceCheck = 2.5f; // length of bumper ray
    [SerializeField]
    private float bumperCameraHeight = 1.0f; // adjust camera height while bumping
    [SerializeField]
    private Vector3 bumperRayOffset; // allows offset of the bumper ray from target origin

    [HideInInspector]public playerPossession playerPosRef; //A reference to the players position
    
    //Camera Orbiting during hide
    [HideInInspector]public Transform CameraTransform; //Cameras transform
    [HideInInspector]public Transform CameraParent; //Parent transform for camera to pivot around

    //Made public for debugging
    [Header("---DEBUGGING---")]
    public float currentHorizontal = 0;
    public float currentVertical = 0;
    public Vector3 wantedPosition;
    public bool enableRotation = true;

    //public void setTarget(Transform newTarget) { target = newTarget; } //setTarget will be used in CamLock.cs to refollow the new target
    
    /// <Summary>
    /// If the target moves, the camera should child the target to allow for smoother movement. DR
    /// </Summary>
    private void Awake()
    {
        //Camera.main.transform.parent = target;
        //Already attached to main camera, don't need to do this here'
    }

    //Setting camera orbit transforms for hide
    private void Start()
    {
        playerPosRef = Camera.main.GetComponent<CamLock>().player.GetComponent<playerPossession>();
        CameraTransform = this.transform; //This script should be attached to the camera, so we grab its transform
        //CameraParent = this.transform.parent;
        //CREATE NEW EMPTY PIVOT OBJECT AT TARGET HIDE LOCATION
    }

    private void Update()
    {
        //Get desired position/rotation IN WORLD SPACE and lerp.
        wantedPosition = target.TransformPoint(0, height, -distance);
        Vector3 lookPosition = target.TransformPoint(targetLookAtOffset);
        var bumperDistanceCheck = (wantedPosition - lookPosition).magnitude;

        Vector3 back = Vector3.Normalize(wantedPosition - lookPosition); //  target.transform.TransformDirection(-1 * Vector3.forward);

        Debug.DrawLine(lookPosition, lookPosition + back * bumperDistanceCheck, new Color(1.0f, 0.0f, 1.0f));

        RaycastHit rayToCameraInfo;
        Vector3 target_origin = target.TransformPoint(camera_origin);
        Vector3 wantedVector = wantedPosition - target_origin;
        Debug.DrawLine(target_origin, wantedPosition, new Color(1.0f, 0.5f, 0.0f));
        bool ray_to_camera_has_hit = Physics.Raycast(target_origin, wantedVector, out rayToCameraInfo);

        if (ray_to_camera_has_hit && wantedVector.magnitude > rayToCameraInfo.distance && rayToCameraInfo.transform.tag != "Player")
        {
            Debug.DrawLine(wantedPosition, wantedPosition + wantedVector * rayToCameraInfo.distance, new Color(0, 1, 1));
            wantedPosition = target_origin + wantedVector.normalized * (rayToCameraInfo.distance);
        }

        //if (enableRotation)
        transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * damping); //Slowly transition from current camera position to wanted camera position based on deltaTime * damping (Set earlier)

        //if(enableRotation)
        transform.rotation = Quaternion.LookRotation(back * -1.0f, target.up); //Rotate the camera without damping
    }

    private void LateUpdate()
    {
        //This code orbits the camera during hide mode
        if (playerPosRef.IsHidden())
        {
            CameraParent = this.gameObject.transform.parent;
            currentHorizontal = Camera.main.GetComponent<CamLock>().currentHorizontal;
            currentVertical = Camera.main.GetComponent<CamLock>().currentVertical;

            //Create rotation
            Quaternion rotation = Quaternion.Euler(currentVertical, currentHorizontal, 0);

            if (CameraParent != null)
            {
                target = CameraParent;
                this.CameraParent.rotation = rotation/*Quaternion.Lerp(this.CameraParent.rotation, rotation, Time.deltaTime * rotationDamping)*/;
            }
        }
    }
    
    static Vector3 ninetyPercentPoint(Vector3 start, Vector3 end) //Calculates a point on the edge of the collided item, for repositioning purposes
    {
        Vector3 result = new Vector3((start.x + end.x) * 0.1f, (start.y + end.y) * 0.1f, (start.z + end.z) * 0.1f);
        return result;
    }
}

