using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Analytics;

public class MenuAndSceneManager : MonoBehaviour
{
    public static MenuAndSceneManager instance;

    public Button VolumeOn;
    public Button VolumeOff;
    public Button CreditsButton;
    public GameObject Credits;

    public bool hasPlayedOnce = false;

    public int playCount = 0;

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        DontDestroyOnLoad(this.gameObject);


        //Main scene stuff
        SceneManager.sceneLoaded += OnLoadScene;
        OnLoadScene(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnLoadScene(Scene scene, LoadSceneMode lsm)
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            var playButton = GameObject.FindGameObjectWithTag("PlayButton").GetComponent<Button>();
            playButton.onClick.AddListener(StartGame);
            playButton.onClick.AddListener(() => AudioManager.instance.PlayStart());

            VolumeOn = GameObject.FindGameObjectWithTag("VolumeOn").GetComponent<Button>();
            VolumeOff = GameObject.FindGameObjectWithTag("VolumeOff").GetComponent<Button>();
            CreditsButton = GameObject.FindGameObjectWithTag("CreditsButton").GetComponent<Button>();

            VolumeOn.onClick.AddListener(() => ToggleSound(0));
            VolumeOff.onClick.AddListener(() => ToggleSound(1));
            Credits = GameObject.FindGameObjectWithTag("Credits");
            Credits.SetActive(false);
            CreditsButton.onClick.AddListener(() =>
            {
                Credits.SetActive(!Credits.activeInHierarchy);
            });
            ToggleSound((int)AudioManager.instance.masterVolume);
        }
    }

    void StartGame()
    {
        hasPlayedOnce = true;

        Analytics.CustomEvent("StartedGame");

        LoadScene(1);
    }

    public static void LoadScene(int index)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(index);

        if (index == 1)
        {
            instance.playCount++;
        }
    }

    public static int CheckSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    public void Clean()
    {
        //Destroy(this.gameObject.transform.root.gameObject);
    }

    public void QuitApplication()
    {


        Application.Quit();
    }

    public void ToggleSound(int state)
    {
        AudioManager.instance.ToggleSound(state);
        if (state == 0)
        {
            VolumeOn.gameObject.SetActive(false);
            VolumeOff.gameObject.SetActive(true);
        }
        else
        {
            VolumeOn.gameObject.SetActive(true);
            VolumeOff.gameObject.SetActive(false);
        }
    }

    private void OnApplicationQuit()
    {
        Analytics.CustomEvent("QuitApplication", new Dictionary<string, object>
        {
            { "Played_Once", hasPlayedOnce }
        });
    }
}
