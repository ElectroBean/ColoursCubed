using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Analytics;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool sandbox = false;

    #region Lives and scoring
    public int lives;
    public float score;
    public int difficultyMultiplier;
    public int scoreIncrement;

    public bool continuedLife = false;

    public UnityEngine.UI.Button continueButton;
    #endregion

    #region Gravity
    public Vector3 gravity;
    public Vector3 maxGravity;
    public Vector3 currentGravity;
    #endregion

    #region UI
    public TextMeshProUGUI scoreText;

    public GameObject gameplayUI;
    public GameObject pauseMenuUI;
    public GameObject gameOverUI;

    public TextMeshProUGUI gameoverScoreText;

    public TextMeshProUGUI hiscoreText;
    #endregion

    #region Game States

    public enum GameStates
    {
        Start,
        Playing,
        Paused,
        GameOver
    }

    public GameStates currentState = GameStates.Playing;

    #endregion

    #region difficulty
    public float scoreForMaxDifficulty;
    public GameObject ballPrefab;
    public Transform ballSpawnPoint;
    public GameObject secondBall;

    public int spawnEveryX;

    #region specials
    public bool specialEnabled = false;

    public GameObject colorChanger;
    public ColorChanger colorChangerCC;

    public GameObject suspenseBall;
    public SuspenseBall suspenseBallScript;
    #endregion
    #endregion

    #region Trackers

    Vector3 ballStart;
    GameObject ball;

    //for use when a color change ball is incoming
    public Color[] tempGetableColors;

    #endregion

    public List<Color> AllColors;

    private void Awake()
    {

    }

    private void Start()
    {
        #region singleton
        if (instance == null)
            instance = this;
        else
            Destroy(this);
        #endregion

        #region Gravity
        Physics.gravity = gravity;
        currentGravity = gravity;
        #endregion

        ball = GameObject.FindGameObjectWithTag("Player");
        ballStart = ball.transform.position;

        #region Button Listeners
        if (MenuAndSceneManager.instance != null)
        {
            GameObject[] homeButtons = GameObject.FindGameObjectsWithTag("HomeButton");
            for (int i = 0; i < homeButtons.Length; i++)
            {
                var tempHomeButton = homeButtons[i].GetComponent<UnityEngine.UI.Button>();
                tempHomeButton.onClick.AddListener(() => MenuAndSceneManager.LoadScene(0));
                tempHomeButton.onClick.AddListener(() => AudioManager.instance?.PlayStart());
                tempHomeButton.onClick.AddListener(() => MenuAndSceneManager.instance.Clean());
            }

            var reloadButton = GameObject.FindGameObjectWithTag("Reload").GetComponent<UnityEngine.UI.Button>();
            reloadButton.onClick.AddListener(() => MenuAndSceneManager.LoadScene(1));
            reloadButton.onClick.AddListener(() => AudioManager.instance?.PlayReplay());

            var menuButton = GameObject.FindGameObjectWithTag("MenuButton").GetComponent<UnityEngine.UI.Button>();
            menuButton.onClick.AddListener(() => AudioManager.instance?.PlayPause());

            //Continue button
            continueButton = GameObject.FindGameObjectWithTag("Continue").GetComponent<UnityEngine.UI.Button>();
            continueButton.onClick.AddListener(Continue);
        }
        #endregion

        secondBall = Instantiate(ballPrefab, new Vector3(100, 100, 0), Quaternion.identity);
        secondBall.SetActive(false);
        ChangeCurrentState(1);

        colorChanger = GameObject.FindGameObjectWithTag("ColorChanger");
        colorChangerCC = colorChanger.GetComponent<ColorChanger>();
        colorChanger.SetActive(false);

        suspenseBall = GameObject.FindGameObjectWithTag("SuspenseBall");
        suspenseBallScript = suspenseBall.GetComponent<SuspenseBall>();
        suspenseBall.SetActive(false);
        specialEnabled = false;
    }

    #region Scoring Functions
    private void UpdateUI()
    {
        scoreText.text = score.ToString();
    }

    public void AddPoints()
    {
        score += scoreIncrement;

        if (colorChanger.activeInHierarchy)
        {
            colorChangerCC.MoveDown();
        }

        if (!specialEnabled)
        {
            if (score % spawnEveryX == 0)
            {
                #region second ball
                //if (secondBall.activeInHierarchy == false)
                //{

                //    SpawnSecondBall();
                //    lives++;
                //}
                #endregion

                //choose which special to do
                if (Random.Range(0, 100.0f) > 50)
                {
                    colorChanger.SetActive(true);
                    colorChangerCC.Respawn();
                }
                else
                {
                    suspenseBall.SetActive(true);
                    suspenseBallScript.Respawn();
                }
                specialEnabled = true;
            }
        }

        DifficultyMultiplier();
        UpdateUI();
    }

    //called when the ball hits the floor
    //Checks color of ball and floor to see if they match
    public void CheckColor(GameObject ball)
    {

        var ballS = ball.GetComponent<BallScript>();
        //if the colors match
        if (ballS.getableColor.Equals(SimpleController.instance.getableColor))
        {
            //add points
            AddPoints();
            AudioManager.instance?.PlayBounce();

            //Handheld.Vibrate();
        }
        else
        {
            //do something here to lose a life
            if (sandbox)
                return;

            if (lives > 1)
            {
                ball.SetActive(false);
                secondBall = ball;
            }
            lives--;

            if (lives < 1)
            {
                ChangeCurrentState(3);
            }

            AudioManager.instance?.PlayDeath();
        }
    }
    #endregion


    public void ChangeCurrentState(int newState)
    {
        currentState = (GameStates)newState;

        //Use this to apply any new UI or settings relative to states
        switch (currentState)
        {
            case GameStates.Playing:
                {
                    //Set timescale to 1
                    Time.timeScale = 1;

                    SetGameplay(true);
                    SetMenu(false);
                    SetGameOver(false);
                }
                break;

            case GameStates.Paused:
                {
                    //Set time scale to 0
                    Time.timeScale = 0;

                    SetMenu(true);
                    SetGameplay(false);
                    SetGameOver(false);
                }
                break;

            case GameStates.GameOver:
                {
                    Time.timeScale = 0;
                    gameoverScoreText.text = "Your score: " + score.ToString();
                    SimpleController.instance.ignoreInput = true;


                    if (score > PlayerPrefs.GetFloat("Hiscore"))
                    {
                        Analytics.CustomEvent("BeatHiscore", new Dictionary<string, object>
                        {
                            { "Previous",  PlayerPrefs.GetFloat("Hiscore")},
                            { "New", score }
                        });
                        PlayerPrefs.SetFloat("Hiscore", score);
                    }

                    hiscoreText.text = "High score: " + PlayerPrefs.GetFloat("Hiscore");

                    SetGameOver(true);
                    SetGameplay(false);
                    SetMenu(false);

                    //if they haven't got their 1 save left, and the amount of times played is a multiple of 3
                    if (continuedLife == true && MenuAndSceneManager.instance.playCount % 3 == 0)
                    {
                        //50-50 chance of playing random ad
                        if (Random.Range(0, 101) <= 50)
                        {
                            AdsPersistent.instance.PlayInterstitialAd();
                        }
                    }

                    DeathAnalytics();
                }
                break;
        }

    }

    void SetMenu(bool state)
    {
        pauseMenuUI.SetActive(state);
    }

    void SetGameplay(bool state)
    {
        gameplayUI.SetActive(state);
    }

    void SetGameOver(bool state)
    {
        gameOverUI.SetActive(state);
    }

    void DifficultyMultiplier()
    {
        float percentage = score / scoreForMaxDifficulty;
        if (percentage == 0)
            return;
        if (percentage > 1)
            percentage = 1;
        Vector3 newGrav = maxGravity * percentage;
        currentGravity = gravity + newGrav;
        Physics.gravity = gravity + newGrav;
    }

    void SpawnSecondBall()
    {
        secondBall.transform.position = ballSpawnPoint.position;
        secondBall.SetActive(true);
        BallScript bs = secondBall.GetComponent<BallScript>();
        bs.isSecond = true;
        bs.Pause();
    }

    void Continue()
    {
        //StartCoroutine(delayInput());
        SimpleController.instance.ignoreInput = true;
        AdsPersistent.instance.PlayRewardedAd(AddLifeAndContinue);
    }

    void AddLifeAndContinue()
    {
        iTween.Stop(colorChanger);
        colorChanger.SetActive(false);
        lives++;

        //reset ball position
        ball.transform.position = ballStart;
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;

        //reset the cube
        SimpleController.instance.ResetCube();


        ChangeCurrentState(1);
        continuedLife = true;
        continueButton.gameObject.SetActive(false);

        //ball.GetComponent<BallScript>().getableColor = SimpleController.instance.getableColor;
        //ball.GetComponent<MeshRenderer>().material.color = SimpleController.instance.getableColor;
        ball.GetComponent<BallScript>().getableColor = SimpleController.instance.AllColors[4];
        ball.GetComponent<MeshRenderer>().material.color = SimpleController.instance.AllColors[4];
    }

    IEnumerator delayInput()
    {
        SimpleController.instance.ignoreInput = true;
        yield return new WaitForSeconds(1.0f);
        SimpleController.instance.ignoreInput = false;
    }

    private void OnApplicationQuit()
    {
        Analytics.CustomEvent("QuitInGame", new Dictionary<string, object>
        {
            { "Game_State", currentState },
            { "Score", score },
            { "Watched_Continue_Ad",  continuedLife}
        });
    }

    private void DeathAnalytics()
    {
        Analytics.CustomEvent("OnLose", new Dictionary<string, object>
        {
            { "Score", score },
            { "Gravity", currentGravity },
            { "Has_Continued", continuedLife }
        });
    }
}
