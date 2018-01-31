////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <29/07/17>                               
// Brief: <Handles the main game loop + Timer>  
////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    private static GameManager m_instance = null;

    public static GameManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    // [Header("Time in seconds until lose screen.")]public float timer; //Timer till lose screen
    // [Header("How much score to show win screen.")] public int maxMischeif;
    [HideInInspector] public int NPCcount;

    //Gameplay UI
    // private Text txt_timerText;
    private Text txt_npcCount;
    private Text txt_playerHealth;

    //Win/Lose Canvas
    public Canvas canvasWinOrLose;
    // public Canvas canvasPause;
    private RectTransform winText; //Transforms of the child text objects
    private RectTransform loseText;
    private List<RectTransform> textList;

    private bool gameover;
    // private float timeLeft;
    private bool paused;
    private bool win;

    private playerController player;

    private void Awake()
    {
        //Check if instance already exists
        if (m_instance == null)
            m_instance = this;

        //If instance already exists and it's not this:
        else if (m_instance != this)
            Destroy(gameObject);

        gameover = false;
        win = false;


        if (canvasWinOrLose == null)
            Debug.LogError("CanvasWinOrLose has not been set on GameManager.cs. Please set through the editor window.");
        
        canvasWinOrLose.gameObject.SetActive(false);
        //canvasPause.gameObject.SetActive(false);

    }

    // Use this for initialization
    void Start() {



        //txt_timerText = GameObject.Find("Timer").GetComponent<Text>();
        txt_npcCount = GameObject.Find("NPC's Left").GetComponent<Text>();
        //txt_playerHealth = GameObject.Find("Health").GetComponent<Text>();
        //player = gameObject.GetComponentInParent<playerController>();

        //Load the TextObjects from canvasWinOrLose
        foreach (RectTransform text in canvasWinOrLose.gameObject.GetComponentsInChildren<RectTransform>(true))
        {
            if (text.name == "WinText")
                winText = text;
            if (text.name == "LoseText")
                loseText = text;
        }

        //Debug.Log(UpdateSensTxt.mouseSensX);
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Escape))
            Quit();

        //timeLeft = timer;
        txt_npcCount.text = NPCcount.ToString();
        //txt_playerHealth.text = player.health.ToString() + "%";

        //Pause the game - kinda
        //if (Input.GetKey(KeyCode.Escape) && !win)
        //{
        //    Time.timeScale = 0; //slow the game to a hault
        //    canvasPause.gameObject.SetActive(true);
        //    Camera.main.GetComponent<CamLock>().enabled = false;
        //    player.gameObject.GetComponent<playerCannonBall>().enabled = false;
        //    player.gameObject.GetComponent<playerPossession>().enabled = false;
        //    player.gameObject.GetComponent<AudioSource>().enabled = false;
        //    player.gameObject.GetComponent<PlayerController>().enabled = false;

        //    AgentController[] agents = GameObject.FindObjectsOfType<AgentController>();

        //    foreach (AgentController agent in agents)
        //        agent.gameObject.GetComponent<script_ProtonBeam_v5>().enabled = false;


        //    paused = true;
        //}

        //Winscreen
        if (NPCcount <= 0)
        {
            NPCcount = 0;
        //    win = true;
           canvasWinOrLose.gameObject.SetActive(true);
           winText.gameObject.SetActive(true);

            Camera.main.GetComponent<CamLock>().enabled = false;
            Cursor.lockState = CursorLockMode.None;
        //    player.gameObject.GetComponent<playerCannonBall>().enabled = false;
        //    player.gameObject.GetComponent<playerPossession>().enabled = false;
        //    player.gameObject.GetComponent<AudioSource>().enabled = false;
            gameObject.GetComponentInParent<CharacterController>().enabled = false;
            gameObject.GetComponentInParent<playerController>().enabled = false;

            //    AgentController[] agents = GameObject.FindObjectsOfType<AgentController>();

            //    foreach (AgentController agent in agents)
            //        agent.gameObject.GetComponent<script_ProtonBeam_v5>().enabled = false;
        }
        //else
        //{
        //    timeLeft = timer - Time.time; //Countdown the timer
        //}

        //Gameover State
        if(gameObject.GetComponentInParent<playerController>().Ectoplasm <= 0)
        {
            gameObject.GetComponentInParent<playerController>().Ectoplasm = 0;
            canvasWinOrLose.gameObject.SetActive(true);
            loseText.gameObject.SetActive(true);
            Camera.main.GetComponent<CamLock>().enabled = false;
            Cursor.lockState = CursorLockMode.None;
            gameObject.GetComponentInParent<CharacterController>().enabled = false;
            gameObject.GetComponentInParent<playerController>().enabled = false;
        }

    } //End update


    //void Gameover()
    //{
    //    timeLeft = 0;
    //    player.health = 0;
    //    txt_timerText.text = "0:00";
    //    txt_playerHealth.text = player.health.ToString() + "%";
    //    //Disable Scripts here
    //    canvasWinOrLose.gameObject.SetActive(true);
    //    loseText.gameObject.SetActive(true);
    //    Time.timeScale = 0;
    //    player.GetComponent<PlayerController>().enabled = false;        
    //}

    //public void ResumeGameplay()
    //{
    //    if (paused == true)
    //    {
    //        canvasPause.gameObject.SetActive(false);
    //        Camera.main.GetComponent<CamLock>().enabled = true;
    //        player.gameObject.GetComponent<playerCannonBall>().enabled = true;
    //        player.gameObject.GetComponent<playerPossession>().enabled = true;
    //        player.gameObject.GetComponent<AudioSource>().enabled = true;
    //        player.gameObject.GetComponent<PlayerController>().enabled = true;

    //        AgentController[] agents = GameObject.FindObjectsOfType<AgentController>();

    //        foreach (AgentController agent in agents)
    //            agent.gameObject.GetComponent<script_ProtonBeam_v5>().enabled = true;

    //        Time.timeScale = 1;
    //        paused = true;
    //    }
    //}


    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}//End Class
