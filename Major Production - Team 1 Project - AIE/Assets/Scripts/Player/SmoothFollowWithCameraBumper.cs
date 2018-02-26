using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SmoothFollowWithCameraBumper : MonoBehaviour
{

    public Transform target = null; //Camera's target
    public float distance; //Distance maintained from target
    public float height = 1.0f; //Height of Camera

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
    
    //public void setTarget(Transform newTarget) { target = newTarget; } //setTarget will be used in CamLock.cs to refollow the new target

    //Used in CurveToTarget.cs - Retargets the camera on possession - fixes the issue of two snapping
    [HideInInspector] public bool updatePosition = true;
    
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
        //Debug.Log(target);
        Vector3 wantedPosition = target.TransformPoint(0, height, -distance);
        Vector3 lookPosition = target.TransformPoint(targetLookAtOffset);

        // check to see if there is anything behind the target
        RaycastHit hit;
        Vector3 back = Vector3.Normalize(wantedPosition - lookPosition); //  target.transform.TransformDirection(-1 * Vector3.forward);

        // cast the bumper ray out from rear and check to see if there is anything behind
        if (Physics.Raycast(lookPosition, back, out hit, bumperDistanceCheck)
            && (hit.transform.GetComponent<Collider>().tag != "Player"))//hit.transform.tag != target.tag) // ignore ray-casts that hit the user. DR
        {
            //Debug.Log("Transform Hit: " + hit.transform.tag);
            //Debug.Log("Target: " + target.tag);

            Ray theRayToCamera = new Ray(lookPosition, wantedPosition - lookPosition); //Presetting a ray

            wantedPosition = theRayToCamera.GetPoint((hit.distance * 0.8f)); //Finds a point on the ray that's been colliding with an item
            //Vector3 theHitPositionMinusABit = theRayToCamera.GetPoint((hit.distance * 0.8f));
            //// clamp wanted position to hit position

            //wantedPosition.x = theHitPositionMinusABit.x;
            //wantedPosition.z = theHitPositionMinusABit.z;
            //wantedPosition.y = theHitPositionMinusABit.y; // Mathf.Lerp(hit.point.y + bumperCameraHeight, wantedPosition.y, Time.deltaTime * damping);

        }

        //if (updatePosition)
            transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * damping); //Slowly transition from current camera position to wanted camera position based on deltaTime * damping (Set earlier)

        //Debug.DrawLine(lookPosition, wantedPosition);

        if (smoothRotation) //If smooth rotation's enabled
        {
            Quaternion wantedRotation = Quaternion.LookRotation(lookPosition - transform.position, target.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation, Time.deltaTime * rotationDamping); //Dampen the speed of rotation based of "rotationDamping"
        }
        else
            transform.rotation = Quaternion.LookRotation(lookPosition - transform.position, target.up); //Rotate the camera without damping
    }
    
    private void LateUpdate()
    {
        //This code orbits the camera during hide mode
        if (playerPosRef.IsHidden() && playerPosRef.GetComponent<Rigidbody>().IsSleeping() == true) 
        {
            CameraParent = this.gameObject.transform.parent;
            currentHorizontal = Camera.main.GetComponent<CamLock>().currentHorizontal;
            currentVertical = Camera.main.GetComponent<CamLock>().currentVertical;

            //Create rotation
            Quaternion rotation = Quaternion.Euler(currentVertical, currentHorizontal, 0);

            if (CameraParent != null)
            {
                target = CameraParent;
                this.CameraParent.rotation = Quaternion.Lerp(this.CameraParent.rotation, rotation, Time.deltaTime * rotationDamping);
            }
        }
    }
    
    static Vector3 ninetyPercentPoint(Vector3 start, Vector3 end) //Calculates a point on the edge of the collided item, for repositioning purposes
    {
        Vector3 result = new Vector3((start.x + end.x) * 0.1f, (start.y + end.y) * 0.1f, (start.z + end.z) * 0.1f);
        return result;
    }
}
