using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitControlMenu : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKey(KeyCode.Space) || Input.GetButton("A Button") && this.gameObject.activeSelf == true)
        {
            //this.gameObject.SetActive(false);
            SceneManager.LoadScene(1);

        }

    }
}
