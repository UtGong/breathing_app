using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BreathingGameController : MonoBehaviour
{
    public int currentStage = 1;
    public int breathCount = 0;
    public int preStateCount = 10;
    public int state1Count = 10;
    public int state2Count = 15;
    public int state3Count = 4;


    public AudioClip waterDropSound;
    public AudioClip mushroomGrowSound;
    public AudioSource mushroomAS;

    private float breathTimer = 0f;
    public float breathDuration = 8f; // 吸气4秒+保持4秒+呼气4秒+保持4秒

    public Animator MushroomAnimator;

    public Animator DropAnimator;
    
    //public UDPReceiver udpReceiver;
    private float previousIntensity = 0f;

    private bool isStart = false;

    public TextMeshProUGUI breathTxt;
    public TextMeshProUGUI curStateTxt;
    public TextMeshProUGUI breathCountTxt;

    public Slider ProgressSlider;

    void Update()
    {
        if (!isStart) return;
        
        switch (currentStage)
        {
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
            CreateWaterDrop();
            if (breathCount >= state2Count)
            {
                currentStage = 4;
                breathCount = 0;
                breathTimer = 0;
            }
        }
        GrowMushroom(breathCount/ state2Count);
    }

    void HandleMushroomShake()
    {
        breathTimer += Time.deltaTime;
        if (breathTimer >= breathDuration)
        {
            breathTimer = 0f;
            breathCount++;
            ShakeMushroom();
            CreateWaterDrop();
            if (breathCount >= state3Count)
            {
                Debug.Log("Game Finished!");
            }
        }
    }

    void CreateWaterDrop()
    {
        DropAnimator.SetTrigger("Drop");
        mushroomAS.clip = waterDropSound;
        mushroomAS.Play();
    }

    void GrowMushroom(float progress)
    {
        MushroomAnimator.Play("Base Layer.Grow", 0, progress); // 归一化时间
        // mushroomAS.clip = mushroomGrowSound;
        // mushroomAS.Play();
    }

    void ShakeMushroom()
    {
        // mushroom.GetComponent<Animator>().SetTrigger("Shake");
        // mushroomShakeSound.Play();
    }
}
