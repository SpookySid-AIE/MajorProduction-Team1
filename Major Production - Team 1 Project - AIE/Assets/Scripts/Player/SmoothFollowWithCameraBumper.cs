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

    /// <Summary>
    /// If the target moves, the camera should child the target to allow for smoother movement. DR
    /// </Summary>
    private void Awake()
    {
        //Camera.main.transform.parent = target;
        //Already attached to main camera, don't need to do this here'
    }

    private void FixedUpdate()
    {
        //Get desired position/rotation IN WORLD SPACE and lerp.
        Debug.Log(target);
        Vector3 wantedPosition = target.TransformPoint(0, height, -distance);
        Vector3 lookPosition = target.TransformPoint(targetLookAtOffset);

        // check to see if there is anything behind the target
        RaycastHit hit;
        Vector3 back = Vector3.Normalize(wantedPosition - lookPosition); //  target.transform.TransformDirection(-1 * Vector3.forward);

        // cast the bumper ray out from rear and check to see if there is anything behind
        if (Physics.Raycast(lookPosition, back, out hit, bumperDistanceCheck)
            && hit.transform != target) // ignore ray-casts that hit the user. DR
        {
            Ray theRayToCamera = new Ray(lookPosition, wantedPosition - lookPosition );

            Vector3 theHitPositionMinusABit = theRayToCamera.GetPoint((hit.distance * 0.8f));
            // clamp wanted position to hit position

            wantedPosition.x = theHitPositionMinusABit.x;
            wantedPosition.z = theHitPositionMinusABit.z;
            wantedPosition.y = Mathf.Lerp(hit.point.y + bumperCameraHeight, wantedPosition.y, Time.deltaTime * damping);

        }

        transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * damping);

        Debug.DrawLine(lookPosition, wantedPosition);
        

        if (smoothRotation)
        {
            Quaternion wantedRotation = Quaternion.LookRotation(lookPosition - transform.position, target.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation, Time.deltaTime * rotationDamping);
        }
        else
            transform.rotation = Quaternion.LookRotation(lookPosition - transform.position, target.up);
    }
    static Vector3 ninetyPercentPoint(Vector3 start, Vector3 end)
    {
        Vector3 result = new Vector3((start.x + end.x) * 0.1f, (start.y + end.y) * 0.1f, (start.z + end.z) * 0.1f);
        return result;
    }
}


