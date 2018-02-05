using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SmoothFollowWithCameraBumper : MonoBehaviour
{

    [SerializeField]
    public Transform target = null;
    [SerializeField]
    public float distance;
    [SerializeField]
    private float height = 1.0f;
    [SerializeField]
    private float damping = 5.0f;
    [SerializeField]
    private bool smoothRotation = true;
    [SerializeField]
    private float rotationDamping = 10.0f;

    [SerializeField]
    public Vector3 targetLookAtOffset; // allows offsetting of camera lookAt, very useful for low bumper heights

    [SerializeField]
    private float bumperDistanceCheck = 2.5f; // length of bumper ray
    [SerializeField]
    private float bumperCameraHeight = 1.0f; // adjust camera height while bumping
    [SerializeField]
    private Vector3 bumperRayOffset; // allows offset of the bumper ray from target origin

    [HideInInspector]public playerPossession playerPosRef;
    
    //Camera Orbiting during hide
    [HideInInspector]public Transform CameraTransform;
    [HideInInspector]public Transform CameraParent; //Parent transform for camera to pivot around
    private float currentHorizontal = 0;
    private float currentVertical = 0;

    /// <Summary>
    /// If the target moves, the camera should child the target to allow for smoother movement. DR
    /// </Summary>
    private void Awake()
    {
        //Camera.main.transform.parent = target;
        //playerRef = target.GetComponent<playerPossession>();
    }

    //Setting camera orbit transforms for hide
    private void Start()
    {
        playerPosRef = Camera.main.GetComponent<CamLock>().player.GetComponent<playerPossession>();
        CameraTransform = this.transform; //This script should be attached to the camera, so we grab its transform
        //CameraParent = this.transform.parent;
        //CREATE NEW EMPTY PIVOT OBJECT AT TARGET HIDE LOCATION
    }

    private void FixedUpdate()
    {
        Vector3 wantedPosition = target.TransformPoint(0, height, -distance);

        // check to see if there is anything behind the target
        RaycastHit hit;
        Vector3 back = target.transform.TransformDirection(-1 * Vector3.forward);

        // cast the bumper ray out from rear and check to see if there is anything behind
        if (Physics.Raycast(target.TransformPoint(bumperRayOffset), back, out hit, bumperDistanceCheck)
            && hit.transform != target) // ignore ray-casts that hit the user. DR
        {
            Ray theRayToCamera = new Ray(target.position, (hit.point - target.position));

            Vector3 theHitPositionMinusABit = theRayToCamera.GetPoint((hit.distance * 0.8f));



            // clamp wanted position to hit position

            wantedPosition.x = theHitPositionMinusABit.x;

            wantedPosition.z = theHitPositionMinusABit.z;

            wantedPosition.y = Mathf.Lerp(hit.point.y + bumperCameraHeight, wantedPosition.y, Time.deltaTime * damping);

        }

        transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * damping);

        Vector3 lookPosition = target.TransformPoint(targetLookAtOffset);

        if (smoothRotation)
        {
            Quaternion wantedRotation = Quaternion.LookRotation(lookPosition - transform.position, target.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation, Time.deltaTime * rotationDamping);
        }
        else
            transform.rotation = Quaternion.LookRotation(lookPosition - transform.position, target.up);

    }

    private void LateUpdate()
    {
        //This code orbits the camera during hide mode
        if (playerPosRef.isHidden())
        {
            CameraParent = this.gameObject.transform.parent;
            currentHorizontal = Camera.main.GetComponent<CamLock>().currentHorizontal;
            currentVertical = Camera.main.GetComponent<CamLock>().currentVertical;

            //Actual Camera Rig Transformations
            Quaternion rotation = Quaternion.Euler(currentVertical, currentHorizontal, 0);

            //Have to rotate the invis pivot object during "hide mode" so it doesnt break the clipping code
            //reupdate the target when we leave hide mode NOTE
            target = this.CameraParent;
            this.CameraParent.rotation = Quaternion.Lerp(this.CameraParent.rotation, rotation, Time.deltaTime * rotationDamping);

            //if (this.CameraTransform.localPosition.z != this.distance * -1f)
            //{
            //        //    Debug.Log("Weird distance check called");
            //    this.CameraTransform.localPosition = new Vector3(0f, 0f, Mathf.Lerp(this.CameraTransform.localPosition.z, this.distance * -1f, Time.deltaTime * 6f));
            //}
        }
    }

    static Vector3 ninetyPercentPoint(Vector3 start, Vector3 end)
    {
        Vector3 result = new Vector3((start.x + end.x) * 0.1f, (start.y + end.y) * 0.1f, (start.z + end.z) * 0.1f);
        return result;
    }
}


