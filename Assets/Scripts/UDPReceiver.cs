using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;
    public int port = 12345; // 设置UDP接收的端口号
    public float breathingIntensity = 0f; // 接收的呼吸强度
    private bool isStartGame = false;
    public BreathingGameController BreathController;
    
    
    [Header("Breathing Parameters")]
    [SerializeField] private float minBreathDuration = 4f; // 最小呼吸持续时间
    [SerializeField] private float maxBreathDuration = 6f; // 最大呼吸持续时间
    [SerializeField] private float breathingThreshold = 0.5f; // 判断呼吸转折点的阈值
    
    private Queue<float> breathingData = new Queue<float>();
    private float lastPeakTime;
    private float lastValue;
    private bool isInhaling;
    private bool threadRunning;

    void Start()
    {
        udpClient = new UdpClient(port);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveData()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
        while (true)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string message = Encoding.ASCII.GetString(data);
                breathingIntensity = float.Parse(message);
                if (breathingIntensity > 0 && !isStartGame)
                {
                    BreathController.StartGame();
                    isStartGame = true;
                }
                if (float.TryParse(message, out float breathValue))
                {
                    // 使用主线程处理数据
                    MainThreadDispatcher.Instance.Enqueue(() => ProcessBreathingData(breathValue));
                }
                Debug.Log("Received breath intensity: " + breathingIntensity);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error receiving UDP data: " + e.Message);
            }
        }
    }
    
    private void ProcessBreathingData(float newValue)
    {
        breathingData.Enqueue(newValue);
        if (breathingData.Count > 10) // 保持最近10个数据点
        {
            breathingData.Dequeue();
        }

        // 检测呼吸转折点
        if (IsBreathingPeak(newValue))
        {
            float currentTime = Time.time;
            float breathDuration = currentTime - lastPeakTime;
            lastPeakTime = currentTime;

            // 判断呼吸是否稳定
            bool isStable = IsBreathingStable(breathDuration);
            OnBreathingStateChanged(isStable, breathDuration);
        }

        lastValue = newValue;
    }
    
    private void OnBreathingStateChanged(bool isStable, float duration)
    {
        string breathingType = isInhaling ? "吸气" : "呼气";
        Debug.Log($"{breathingType} 持续时间: {duration:F2}秒, 状态: {(isStable ? "稳定" : "不稳定")}");
        
        // 通知BreathController
        if (BreathController != null)
        {
            BreathController.OnBreathingStateChanged(isStable, duration);
        }
    }
    
    private bool IsBreathingStable(float breathDuration)
    {
        return breathDuration >= minBreathDuration && breathDuration <= maxBreathDuration;
    }

    
    private bool IsBreathingPeak(float currentValue)
    {
        if (breathingData.Count < 3) return false;

        // 使用简单的峰值检测
        float[] values = breathingData.ToArray();
        int lastIndex = values.Length - 1;

        if (isInhaling)
        {
            // 检测吸气结束（峰值）
            if (values[lastIndex] < values[lastIndex - 1])
            {
                isInhaling = false;
                return true;
            }
        }
        else
        {
            // 检测呼气结束（谷值）
            if (values[lastIndex] > values[lastIndex - 1])
            {
                isInhaling = true;
                return true;
            }
        }

        return false;
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null)
            receiveThread.Abort();
        udpClient.Close();
    }
}