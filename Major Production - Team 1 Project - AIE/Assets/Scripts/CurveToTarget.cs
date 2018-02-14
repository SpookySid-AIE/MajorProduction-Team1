using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveToTarget : MonoBehaviour {


    public GameObject target;
    public float lerpHeight = 4.0f;
    public float speed = 0.04f;
    public float particleLaunchDelay;
    public float destroyTimer = 2.0f;

    private float incrementor = 0;
    private float particleLaunchTimer;
    private Vector3 startPos;
    private Vector3 currentPos;

    public bool finishedAnim = false;

    private void Start()
    {
        particleLaunchTimer = Time.time + particleLaunchDelay;
        startPos = transform.position;
        currentPos = startPos;

        Destroy(gameObject, destroyTimer);
    }

    void Update()
    {
        Vector3 endPos = target.transform.position;

        if (Time.time >= particleLaunchTimer)
        {
            //finishedAnim = false;
            incrementor += speed;
            currentPos = Vector3.Lerp(startPos, endPos, incrementor);
            currentPos.y += lerpHeight * Mathf.Sin(Mathf.Clamp01(incrementor) * Mathf.PI);
            transform.position = currentPos;

            Vector3 curPos = new Vector3(currentPos.x, currentPos.y + 2, currentPos.z);

            curPos.x += .5f;
            curPos.z += 1f;

            Camera.main.transform.position = curPos;

            //Through testing i found that the distance between the target and Camera should always be less than 2.3, so if errors in transitions, this could be why
            float dist = Vector3.Distance(Camera.main.transform.position, target.transform.position);

            if (dist <= 2.3f && finishedAnim == false)
            {
                //Camera.main.GetComponent<SmoothFollowWithCameraBumper>().updatePosition = false;
                finishedAnim = true;
                //Camera.main.GetComponent<SmoothFollowWithCameraBumper>().updatePosition = true;

                // Camera.main.GetComponent<SmoothFollowWithCameraBumper>().target = target.transform;
                //Camera.main.GetComponent<CamLock>().player = target.gameObject;
                //Camera.main.GetComponent<CamLock>().enabled = false;
                //Debug.Log("Curve.cs After: " + finishedAnim);
            }
        }
    }
}
