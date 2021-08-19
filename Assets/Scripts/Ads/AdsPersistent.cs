using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Analytics;

public class AdsPersistent : MonoBehaviour, IUnityAdsListener
{
    public static AdsPersistent instance;

#if UNITY_ANDROID
    private string playStoreID = "3927335";
#endif

#if UNITY_IOS
        private string appStoreID = "3927334";
#endif

    private string interstitialAd = "video";
    private string rewardedAd = "rewardedVideo";

    public bool isTestAd;
    public bool isTargetPlayStore;

    private System.Action rewardFunction = null;

    private int volumeBeforeAd;

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        InitRegistry();
        DontDestroyOnLoad(this);
    }

    private void InitRegistry()
    {
        Advertisement.AddListener(this);

#if UNITY_IOS
        Advertisement.Initialize(appStoreID, isTestAd);
#endif

#if UNITY_ANDROID
        Advertisement.Initialize(playStoreID, isTestAd);
#endif
    }

    public void PlayInterstitialAd()
    {
        if (!Advertisement.IsReady(interstitialAd))
        {
            Debug.Log("ad not ready");
            return;
        }

        Advertisement.Show(interstitialAd);
    }

    public void PlayRewardedAd(System.Action onComplete)
    {
        if (!Advertisement.IsReady(rewardedAd))
        {
            Debug.Log("ad not ready");
            return;
        }

        rewardFunction = onComplete;
        Advertisement.Show(rewardedAd);
    }

    public void OnUnityAdsReady(string placementId)
    {
        //throw new System.NotImplementedException();
    }

    public void OnUnityAdsDidError(string message)
    {
        //throw new System.NotImplementedException();
        Debug.LogWarning(message);
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        //throw new System.NotImplementedException();

        volumeBeforeAd = (int)AudioManager.instance.masterVolume;
        AudioManager.instance.UpdateMasterVolume(0);
        Analytics.CustomEvent("AttemptedAd", new Dictionary<string, object>
        {
            { "Ad_ID",  placementId }
        });
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        //throw new System.NotImplementedException();
        AudioManager.instance.UpdateMasterVolume(volumeBeforeAd);
        switch (showResult)
        {
            case ShowResult.Failed:
                {

                }
                break;

            case ShowResult.Skipped:
                {

                }
                break;

            case ShowResult.Finished:
                {
                    if (placementId == rewardedAd)
                    {
                        if (rewardFunction != null)
                        {
                            rewardFunction();
                            if (SimpleController.instance.ignoreInput == true)
                                SimpleController.instance.ignoreInput = false;
                        }
                    }
                }
                break;

        }

        Analytics.CustomEvent("CompletedAd", new Dictionary<string, object>
        {
            { "Ad_Result", showResult },
            { "Ad_ID", placementId }
        });
    }
}
