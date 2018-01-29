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
            incrementor += speed;
            currentPos = Vector3.Lerp(startPos, endPos, incrementor);
            currentPos.y += lerpHeight * Mathf.Sin(Mathf.Clamp01(incrementor) * Mathf.PI);
            transform.position = currentPos;

            Vector3 curPos = new Vector3(currentPos.x, currentPos.y + 2, currentPos.z);            
            Camera.main.transform.position = curPos;
            Debug.Log("Tracking");
        }
    }
}
