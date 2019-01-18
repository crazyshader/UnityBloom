using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class KawaseBlurCommandBuffer : MonoBehaviour {

    CommandBuffer buffer;
    int _SrceenTexture, _Temp1, _Temp2,_Offset;
    [Range(2,16)]
    public int downSample = 8;

    [Range(0, 12)]
    public int interation = 3;

    [Range(0, 2)]
    public float offset = 0.782f;
    
    [SerializeField, Header("Debug")]
    Material blur;

    void Awake()
    {
        buffer = new CommandBuffer();
        _SrceenTexture = Shader.PropertyToID("_SrceenTexture");
        _Temp1 = Shader.PropertyToID("_Temp1");
        _Temp2 = Shader.PropertyToID("_Temp2");
        _Offset = Shader.PropertyToID("_Offset");

        blur = new Material(Shader.Find("Hide/KawaseBlurPostProcess"));
        blur.hideFlags = HideFlags.HideAndDontSave;
    }

    void OnWillRenderObject()
    {
        var act = gameObject.activeInHierarchy && enabled;
        if (!act)
            return;
 
        OnDisable();

        buffer = new CommandBuffer();
        buffer.name = "KawaseBlur FX";

        buffer.GetTemporaryRT(_SrceenTexture, -1, -1, 0, FilterMode.Bilinear);
        buffer.Blit(BuiltinRenderTextureType.CurrentActive, _SrceenTexture);

        buffer.GetTemporaryRT(_Temp1, -downSample, -downSample, 0, FilterMode.Bilinear);
        buffer.GetTemporaryRT(_Temp2, -downSample, -downSample, 0, FilterMode.Bilinear);

        buffer.Blit(_SrceenTexture, _Temp1);
        buffer.ReleaseTemporaryRT(_SrceenTexture);

        bool swich = true;
        for (int i = 0; i < interation; i++)
        {
            blur.SetFloat(_Offset, (i >> downSample) + offset);
            buffer.Blit(swich ? _Temp1 : _Temp2, swich ? _Temp2 : _Temp1, blur);
            swich = !swich;
        }
        buffer.SetGlobalTexture("_GrabBlurTexture", swich ? _Temp1 : _Temp2);

        Camera.main.AddCommandBuffer(CameraEvent.AfterSkybox, buffer);
    }

    void OnDisable()
    {
        if (buffer != null && Camera.main != null)
            Camera.main.RemoveCommandBuffer(CameraEvent.AfterSkybox, buffer);
    }
}
