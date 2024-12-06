using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


public class VideoPlayerController : MonoBehaviour
{
    [SerializeField] string videoFileName;
    [SerializeField] string nextSceneName;
    public Color loadToColor = Color.white;

    // Start is called before the first frame update
    void Start()
    {
        PlayVideo();
    }

    public void PlayVideo()
    {
        VideoPlayer videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer)
        {
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
            Debug.Log(videoPath);
            videoPlayer.url = videoPath;
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        // Load the Main Menu scene after the intro video ends
        Debug.Log("Scene video ended, loading main menu...");
        Initiate.Fade(nextSceneName, loadToColor, 0.5f);
        //SceneManager.LoadScene("MainMenuScene");  // Replace with your main menu scene name
    }
}