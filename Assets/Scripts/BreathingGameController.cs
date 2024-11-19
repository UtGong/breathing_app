using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;

public class BreathingGameController : MonoBehaviour
{
    public int currentStage = 0;
    public int breathCount = 0;
    public int preStateCount = 10;
    public int state1Count = 10;
    public int state2Count = 15;
    public int state3Count = 4;

    private int unstableBreathCount = 0;
    private bool isInPhase3 = false;
    public AudioSource GuideAudioSourceB;
    public float transitionDuration;
    [Header("Environment References")]
    [SerializeField] private OVRPassthroughLayer passthroughLayer;

    public GameObject[] state2List;
    public GameObject[] state3List;


    public AudioClip waterDropSound;
    public AudioClip mushroomGrowSound;
    public AudioSource mushroomAS;

    public GameObject guideVideoS1;
    public VideoPlayer guideVideoPlayer1;

    public AudioClip guideAudioS2;
    public AudioClip guideAudioS3;
    public AudioClip guideAudioS4;
    private bool isWatchingVideo;
    public GameObject GuideAudioController;

    private float breathTimer = 0f;
    public float breathDuration = 5f; // 吸气4秒+保持4秒+呼气4秒+保持4秒

    public Animator MushroomAnimator;

    public Animator DropAnimator;

    public Animator State4DropAnimator;
    
    //public UDPReceiver udpReceiver;
    private float previousIntensity = 0f;

    private bool isStart = false;

    public TextMeshProUGUI breathTxt;
    public TextMeshProUGUI curStateTxt;
    public TextMeshProUGUI breathCountTxt;

    public Slider ProgressSlider;

    public GameObject VRView;
    public GameObject PassthroughView;
    private bool isPhase3 = false;

    void Update()
    {
        if (!isStart) return;
        
        switch (currentStage)
        {
            case 0:
                HandleGuideVideo();
                break;
            case 1:
                HandleBreathingTraining();
                break;
            case 2:
                HandleWaterDropFormation();
                break;
            case 3:
                HandleMushroomGrowth();
                break;
            case 4:
                HandleMushroomShake();
                break;
        }
        curStateTxt.text ="stage:" + currentStage ;
        breathCountTxt.text = "breath count:"+breathCount;
        breathTxt.text = "breath timer:"+breathTimer;
        
        ProgressSlider.maxValue = breathDuration;
        ProgressSlider.value = breathTimer;
    }

    public void StartGame()
    {
        isStart = true;
    }

    public void HandleGuideVideo()
    {
        if (!isWatchingVideo)
        {
            guideVideoS1.SetActive(true);
            isWatchingVideo = true;
            // 添加视频播放结束的回调
            guideVideoPlayer1.loopPointReached += OnVideoFinished;

            // 开始播放视频
            guideVideoPlayer1.Play();
        }
    }

    

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("视频播放完毕！");
        // 在这里执行你想要的行为
        isWatchingVideo = false;
        currentStage = 1;
        guideVideoS1.SetActive(false);
    }

    public void ResetGame()
    {
        isStart = false;
    }
    void HandleBreathingTraining()
    {
        breathTimer += Time.deltaTime;
        if (breathTimer >= breathDuration)
        {
            breathTimer = 0f;
            breathCount++;
            CreateWaterDrop();
            if (breathCount >= preStateCount)
            {
                currentStage = 2;
                breathCount = 0;
                breathTimer = 0;
            }
        }
    }

    void HandleWaterDropFormation()
    {
        if(GuideAudioController.activeSelf == false) GuideAudioController.SetActive(true);
        breathTimer += Time.deltaTime;
        if (breathTimer >= breathDuration)
        {
            breathTimer = 0f;
            breathCount++;
            CreateWaterDrop();
            if (breathCount >= state1Count)
            {
                currentStage = 3;
                breathCount = 0;
                breathTimer = 0;
            }
        }
    }

    void HandleMushroomGrowth()
    {
        breathTimer += Time.deltaTime;
        if (breathTimer >= breathDuration)
        {
            breathTimer = 0f;
            breathCount++;
            if (breathCount >= state2Count)
            {
                ActiveGameObjectList(state2List, false);
                ActiveGameObjectList(state3List, true);
                MushroomAnimator.Play("Grow");
                currentStage = 4;
                breathCount = 0;
                breathTimer = 0;
                GuideAudioController.SetActive(false);
            }
            else
            {
                CreateWaterDrop();
            }
        }
    }


    void HandleMushroomShake()
    {
        breathTimer += Time.deltaTime;
        if (breathTimer >= breathDuration && isPhase3 == false)
        {
            breathTimer = 0f;
            breathCount++;
            // ShakeMushroom();
            State4WaterDrop();
            if (breathCount >= state3Count)
            {
                Debug.Log("Game Finished!");
                StartCoroutine(TransitionToPassthrough());
                isPhase3 = true;
            }
        }
    }

    void State4WaterDrop()
    {
        State4DropAnimator.SetTrigger("DropState4");
        MushroomAnimator.Play("DropNew");
        mushroomAS.clip = waterDropSound;
        mushroomAS.Play();
    }

    void CreateWaterDrop()
    {
        DropAnimator.SetTrigger("Drop");
        mushroomAS.clip = waterDropSound;
        mushroomAS.Play();
    }

    void GrowMushroom(float progress)
    {
        Debug.Log("progress:" + progress);
        MushroomAnimator.Play("Base Layer.Grow", 0, progress); // 归一化时间
        // mushroomAS.clip = mushroomGrowSound;
        // mushroomAS.Play();
    }
    
    public void OnBreathingStateChanged(bool isStable, float duration)
    {
        if (!isInPhase3) return;

        if (!isStable)
        {
            unstableBreathCount++;
            HandleUnstableBreathing();
        }
    }

    private void HandleUnstableBreathing()
    {
        switch (unstableBreathCount)
        {
            case 1:
            case 2:
                // 播放提示音频
                GuideAudioSourceB.Play();
                break;
            case 3:
                // 返回阶段2
                StartCoroutine(TransitionToVirtualNature());
                unstableBreathCount = 0;
                break;
        }
    }

    private IEnumerator TransitionToPassthrough()
    {
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float alpha = elapsed / transitionDuration;
            // virtualEnvironment.GetComponent<Renderer>().material.SetFloat("_Alpha", 1 - alpha);
            VRView.SetActive(false);
            PassthroughView.SetActive(true);
            passthroughLayer.colorMapEditorBrightness = alpha;
            elapsed += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(Phase3_Passthrough());
    }
    
    private IEnumerator Phase3_Passthrough()
    {
        // isInPhase3 = true;
        unstableBreathCount = 0;
        
        yield return new WaitForSeconds(120f); // 2分钟

        isInPhase3 = false;
        // 进入下一个阶段或结束
    }

    private IEnumerator TransitionToVirtualNature()
    {
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float alpha = elapsed / transitionDuration;
            passthroughLayer.colorMapEditorBrightness = 1 - alpha;
            elapsed += Time.deltaTime;
            yield return null;
        }
        PassthroughView.SetActive(false);
        VRView.SetActive(true);
        isPhase3 = false;
        // 重新开始阶段2，但使用音频D
        GuideAudioSourceB.Play();
        // StartCoroutine(Phase2_VirtualNature());
    }
    
    // private IEnumerator Phase2_VirtualNature()
    // {
    //     currentPhase = ExercisePhase.VirtualNature;
    //     audioSourceC.Play();
    //     yield return new WaitForSeconds(100f);
    //     StartCoroutine(TransitionToPassthrough());
    // }

    void ActiveGameObjectList(GameObject[] list, bool active)
    {
        for (int i = 0; i < list.Length; i++)
        {
            list[i].SetActive(active);
        }
    }

    void ShakeMushroom()
    {
        MushroomAnimator.Play("DropNew");
        // mushroom.GetComponent<Animator>().SetTrigger("Shake");
        // mushroomShakeSound.Play();
    }
    
}
