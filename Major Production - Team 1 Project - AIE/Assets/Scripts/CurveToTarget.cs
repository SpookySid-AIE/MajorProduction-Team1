using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveToTarget : MonoBehaviour {


    public GameObject target;
    public float lerpHeight = 4f;
    public float speed = 10f;
    public float particleLaunchDelay;
    public float destroyTimer = 2.0f;

    private float incrementor = 0;
    private float particleLaunchTimer;
    private Vector3 startPos;
    private Vector3 currentPos;
    private Vector3 endPos;

    public bool finishedAnim = false;

    private void Start()
    {
        particleLaunchTimer = Time.time + particleLaunchDelay;
        startPos = transform.position;
        currentPos = startPos;
        endPos = target.transform.position;
        Destroy(gameObject, destroyTimer);
    }

    void Update()
    {
        if (Time.time >= particleLaunchTimer && finishedAnim != true)
        {
            //Debug.Log("Transition Running..");
            //finishedAnim = false;

            //Old one before 6.3.18
            //incrementor += speed;
            //currentPos = Vector3.Lerp(startPos, endPos, incrementor);
            ////currentPos.y += lerpHeight * Mathf.Sin(Mathf.Clamp01(incrementor) * Mathf.PI);
            //transform.position = currentPos;

            //New
            transform.position = Vector3.MoveTowards(transform.position, endPos, Time.deltaTime * 4);
            Camera.main.GetComponent<SmoothFollowWithCameraBumper>().target = transform;

            //Through testing i found that the distance between the target and Camera should always be less than 2.3, so if errors in transitions, this could be why
            //float dist = Vector3.Distance(Camera.main.transform.position, Camera.main.GetComponent<SmoothFollowWithCameraBumper>().wantedPosition);
            //float dist = Vector3.Distance(Camera.main.transform.position, target.transform.position);
            //Debug.DrawLine(Camera.main.transform.position, Camera.main.GetComponent<SmoothFollowWithCameraBumper>().wantedPosition, Color.green, 100f);

            //Debug.Log(dist);

            if (transform.position == endPos)
            {
                //Debug.Log("Anim end");
                finishedAnim = true;
            }
            //if (dist <= 2.3f && finishedAnim == false)
            //{
            //    //Camera.main.GetComponent<SmoothFollowWithCameraBumper>().updatePosition = false;
            //    Debug.Log("We here boi");
            //    //finishedAnim = true;
            //    //Camera.main.GetComponent<SmoothFollowWithCameraBumper>().updatePosition = true;

            //    // Camera.main.GetComponent<SmoothFollowWithCameraBumper>().target = target.transform;
            //    //Camera.main.GetComponent<CamLock>().player = target.gameObject;
            //    //Camera.main.GetComponent<CamLock>().enabled = false;
            //    //Debug.Log("Curve.cs After: " + finishedAnim);
            //}
        }
    }
}
