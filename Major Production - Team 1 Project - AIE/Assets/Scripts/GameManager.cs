////////////////////////////////////////////////////////////
// Author: <Jak Revai>                                     
// Date Created: <29/07/17>                               
// Brief: <Handles the main game loop + Timer>  
////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

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
    private int fontSize;
    private bool menuSkipBool = false;
    private Text txt_npcCount;
    private Text txt_playerHealth;
    private Vector3 screenDimension;
    private GameObject cursor;
    private GameObject pauseDirection;
    public GameObject sidReticle;
    public playerPossession pPossession;
    public GameObject ui;
    public GameObject br;
    public Material sidSkin;

    public GameObject StoryboardIntro;
    public GameObject StoryboardOutro;
    public GameObject[] uiShrinkElements;


    //Win/Lose Canvas
    public Canvas canvasWinOrLose;

    //Storing all UI objects for the mouse images
    [SerializeField]private GameObject itemSelect;
    [SerializeField]private GameObject hideNonScary;
    [SerializeField]private GameObject hideScary;
    [SerializeField]private GameObject hideScaryLureUsed;
    [SerializeField]private GameObject moveMode;

    public GameObject pauseMenu;
    public GameObject controlsMenu;
    public GameObject creditsMenu;
    public Image[] allStoryboardIntro;
    public Image[] allStoryboardOutro;

    //Public accessor methods to set the UI gameobjects/on/off
    public void EnableItemSelect(bool value) { itemSelect.SetActive(value); }
    public void EnableHideNonScary(bool value) { hideNonScary.SetActive(value); }
    public void EnableHideScary(bool value) { hideScary.SetActive(value); }
    public void EnableHideScaryLure(bool value) { hideScaryLureUsed.SetActive(value); }
    public void EnableMoveMode(bool value) { moveMode.SetActive(value); }

    //Public FMOD.EventRefs that need to be set for some sound effects to play
    [FMODUnity.EventRef] public string audioLure;
    [FMODUnity.EventRef] public string audioScare;
    [FMODUnity.EventRef] public string audioCivSpotted;
    [FMODUnity.EventRef] public string audioCivScared;
    [FMODUnity.EventRef] public string audioItemImpact;


    // public Canvas canvasPause;
    private RectTransform winText; //Transforms of the child text objects
    private RectTransform loseText;
    private List<RectTransform> textList;
    public bool isPaused;
    private bool gameover;
    private bool isStoryboardActive;
    // private float timeLeft;
    private bool paused;
    private bool win;

    private int currentStoryboardIntro = 1;
    private int currentStoryboardOutro = 3;

    [HideInInspector]public playerPossession player; //Reference will be updated once in playerPossess

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

        if (audioLure == null)
            Debug.LogError("audioLure has not been set on GameManager.cs. Please set the audio file in inspector.");

        if (audioScare == null)
            Debug.LogError("audioScare has not been set on GameManager.cs. Please set the audio file in inspector.");

        canvasWinOrLose.gameObject.SetActive(false);
        //canvasPause.gameObject.SetActive(false);

    }

    // Use this for initialization
    void Start() {

        // Comments are cool.
        isStoryboardActive = true;
        allStoryboardIntro = StoryboardIntro.GetComponentsInChildren<Image>();
        allStoryboardOutro = StoryboardOutro.GetComponentsInChildren<Image>();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<playerPossession>();
        cursor = GameObject.Find("BlackReticle");
        screenDimension = new Vector3(Screen.width, Screen.height);
        cursor.GetComponent<RectTransform>().position = new Vector3(screenDimension.x / 2, screenDimension.y / 2, cursor.GetComponent<RectTransform>().position.z);
        if (screenDimension.x > 1000)
            fontSize = 25;

        else fontSize = 50;

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

        NavMesh.avoidancePredictionTime = 4f;

        //Debug.Log(UpdateSensTxt.mouseSensX);
        Time.timeScale = 0;
    }
	
	// Update is called once per frame
	void Update () {
        if (!isPaused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isStoryboardActive)
                Pause();
            else
            {
                menuSkip();
            }
        }
        if (menuSkipBool)
        {
            if (screenDimension != new Vector3(Screen.width / 2, Screen.height / 2))
            {
                screenDimension = new Vector3(Screen.width / 2, Screen.height / 2);
                cursor.GetComponent<RectTransform>().position = new Vector3(screenDimension.x, screenDimension.y, cursor.GetComponent<RectTransform>().position.z);
                if (screenDimension.x > 900)
                    cursor.GetComponent<Text>().fontSize = 25;

                else cursor.GetComponent<Text>().fontSize = 50;
            }
        }

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
        if (!player.IsHidden()) //Might cause core loop issues unsure yet, need this to prevent unreferenced error because i move the camera
        {
            if (player.GetComponent<playerController>().GetEctoplasm <= 0)
            {
                Time.timeScale = 1;

                if (player.IsPossessed())
                    player.PossessedItem.GetComponent<playerPossession>().UnpossessItem();

                GameObject.FindGameObjectWithTag("Player").GetComponent<playerController>().GetEctoplasm = 100;
                GameObject.FindGameObjectWithTag("Player").transform.position = GameObject.Find("respawnPosition").transform.position;


            }            
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

    public void Pause()
    {
        pPossession = GameObject.FindGameObjectWithTag("Player").GetComponent<playerPossession>();
        if (isPaused == true)
        {
            sidReticle.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            foreach (GameObject g in uiShrinkElements)
            {
                g.GetComponent<Text>().fontSize = 50;
            }

            Time.timeScale = 1;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            pPossession.enabled = true;

            pauseMenu.SetActive(false);
            controlsMenu.SetActive(false);
            creditsMenu.SetActive(false);

            isPaused = false;
        }
        else
        {
            sidReticle.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            foreach (GameObject g in uiShrinkElements)
            {
                g.GetComponent<Text>().fontSize = 5000;
            }

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            pPossession.enabled = false;

            pauseMenu.SetActive(true);

            Time.timeScale = 0;

            isPaused = true;

        }
    }

    public void nextPanelIntro()
    {
        //int a = 0;
        //foreach (Transform t in allStoryboard)
        //{
        //    a++;
        //}
        //Debug.Log(a);

        if (allStoryboardIntro[currentStoryboardIntro].tag != "StoryboardEnd")
        {
            allStoryboardIntro[currentStoryboardIntro].enabled = false;

            currentStoryboardIntro++;

            allStoryboardIntro[currentStoryboardIntro].enabled = true;

            if (allStoryboardIntro[currentStoryboardIntro].GetComponentInChildren<Text>() != null)
            {
                foreach(Text t in allStoryboardIntro[currentStoryboardIntro].GetComponentsInChildren<Text>())
                t.enabled = true;
            }
        }
        else
        {
            menuSkip();
        }
    }

    public void nextPanelOutro()
    {


        if (allStoryboardOutro[currentStoryboardOutro].tag != "StoryboardEnd")
        {
            allStoryboardOutro[currentStoryboardOutro].enabled = false;

            if (allStoryboardOutro[currentStoryboardOutro].GetComponentInChildren<Text>() != null)
                allStoryboardOutro[currentStoryboardOutro].GetComponentInChildren<Text>().enabled = false;

            currentStoryboardOutro--;

            allStoryboardOutro[currentStoryboardOutro].enabled = true;

            if (allStoryboardOutro[currentStoryboardOutro].GetComponentInChildren<Text>() != null)
                allStoryboardOutro[currentStoryboardOutro].GetComponentInChildren<Text>().enabled = true;
        }
        else
        {
            GameObject.Find("MenuCanvas (2)").GetComponent<LoadScene>().Load(0);
        }
    }

    public void menuSkip()
    {
        GameObject.Find("Storyboard_Canvas_01").SetActive(false);

        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        br.GetComponent<Text>().fontSize = fontSize;
        sidReticle.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        foreach (GameObject g in uiShrinkElements)
        {
            g.GetComponent<Text>().fontSize = 50;
        }
        isPaused = false;
        isStoryboardActive = false;

        GameObject.Find("UI Canvas").GetComponent<Canvas>().enabled = true;
        menuSkipBool = true;
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}//End Class
