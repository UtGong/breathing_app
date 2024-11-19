using UnityEngine;
using UnityEngine.XR;

public class PassthroughManager : MonoBehaviour
{
    private OVRPassthroughLayer passthroughLayer;
    private OVRManager ovrManager;
    
    void Start()
    {
        // 获取OVRPassthroughLayer组件
        passthroughLayer = GetComponent<OVRPassthroughLayer>();
        ovrManager = GetComponent<OVRManager>();
        
        // 初始化Passthrough
        InitializePassthrough();
    }

    private void InitializePassthrough()
    {
        // 确保OVR运行时已初始化
        if (OVRManager.isHmdPresent)
        {
            // 启用Passthrough
            OVRManager.instance.isInsightPassthroughEnabled = true;
            
            // 设置Passthrough初始参数
            if (passthroughLayer != null)
            {
                passthroughLayer.hidden = true; // 初始时隐藏
                passthroughLayer.colorMapEditorBrightness = 0f;
                passthroughLayer.colorMapEditorContrast = 1f;
                passthroughLayer.edgeRenderingEnabled = true;
                passthroughLayer.edgeColor = new Color(1f, 1f, 1f, 1f);
            }
        }
    }

    // 控制Passthrough的显示和隐藏
    public void ShowPassthrough()
    {
        if (passthroughLayer != null)
        {
            passthroughLayer.hidden = false;
        }
    }

    public void HidePassthrough()
    {
        if (passthroughLayer != null)
        {
            passthroughLayer.hidden = true;
        }
    }

    // 控制Passthrough的透明度过渡
    public void SetPassthroughOpacity(float opacity)
    {
        if (passthroughLayer != null)
        {
            passthroughLayer.colorMapEditorBrightness = opacity;
        }
    }
}