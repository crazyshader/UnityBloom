using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutlinePostProcess : MonoBehaviour {
    static private OutlinePostProcess instance;
    static public OutlinePostProcess Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(OutlinePostProcess)) as OutlinePostProcess;

                if (!instance)
                    instance = Camera.main.gameObject.AddComponent<OutlinePostProcess>();
                instance.Init();
            }
            return instance;
        }
        private set
        {
            if (value == null)
                Destroy(instance);
            instance = value;
        }
    }
    
    public bool enable,pixelBase,occluder,alphaDepth;
    Camera postProcessCam, maskCam;

    [SerializeField]
    private Color outlineColor = new Color(1,.2f,0,1);
    public Color OutlineColor
    {
        get { return outlineColor; }
        set
        {
            outlineColor = value;
            postMat.SetColor("_OutlineColor", value);
        }
    }

    [SerializeField,Range(1, 16)]
    private int resolutionReduce = 2;
    public int ResolutionReduce
    {
        get { return resolutionReduce; }
        set
        {
            resolutionReduce = value;
            GetTempRenderTexture();
        }
    }
    [SerializeField, Range(1, 10)]
    private int interation = 1;

    [SerializeField,Range(0, 10)]
    private float offset = 1, colorIntensity = 3;

    //if need ingore some layer,just edit this list.
    public string[] ignoreLayerName = new string[] {
        "Outline"
        , "Water"
        , "TransparentFX"
        ,"UI"
    };
    int[] ignoreLayerIndex;
    int offsetID, maskMapID, intensityID;
    bool isRuntime;

    [SerializeField, Header("Debug")]
    private RenderTexture maskTexture;
    [SerializeField]
    private RenderTexture tempRT1, tempRT2;
    [SerializeField]
    private Material postMat,flatColor,grabDepth,blur;
    [SerializeField]
    private RawImage mask, temp1, temp2;

    void OnValidate()
    {
        if (!isRuntime) return;
        OutlineColor = outlineColor;
        ResolutionReduce = resolutionReduce;
        AttachToRawImage();
    }
    void Start()
    {
        Init();
    }

    void Init()
    {
        if (Instance != this) Destroy(this);
        if (isRuntime) return;
        isRuntime = true;

        //set ignore layer
        ignoreLayerIndex = new int[ignoreLayerName.Length];
        for (int i = 0; i < ignoreLayerName.Length; i++)
        {
            ignoreLayerIndex[i] = (1 << LayerMask.NameToLayer(ignoreLayerName[i]));
        }

        postProcessCam = Camera.main;
        postMat = new Material(Shader.Find("Hide/OutlinePostprocess"));
        flatColor = new Material(Shader.Find("Hide/FlatColor"));
        grabDepth = new Material(Shader.Find("Hide/GrabDepth"));
        blur = new Material(Shader.Find("Hide/KawaseBlurPostProcess"));

        //translate string to ID , better speed.
        offsetID = Shader.PropertyToID("_Offset");
        maskMapID = Shader.PropertyToID("_MaskTex");
        intensityID = Shader.PropertyToID("_Intensity");

        //set up outline camera
        maskCam = new GameObject().AddComponent<Camera>();
        maskCam.transform.SetParent(postProcessCam.transform);
        maskCam.gameObject.name = "OutlineRenderCamera";
        maskCam.enabled = false;
        maskCam.CopyFrom(postProcessCam);
        maskCam.clearFlags = CameraClearFlags.Nothing;
        maskCam.backgroundColor = Color.black;
        maskCam.renderingPath = RenderingPath.Forward;
        maskCam.cullingMask = 1 << LayerMask.NameToLayer("Outline");
        maskCam.allowHDR = false;

        maskTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 16, RenderTextureFormat.R8);
        maskCam.targetTexture = maskTexture;

        GetTempRenderTexture();
        OnValidate();
    }

    RenderTexture KawaseBlur(RenderTexture from, RenderTexture to)
    {
        bool swich = true;
        for (int i = 0; i < interation; i++)
        {
            ClearBuffer(swich ? to : from);
            blur.SetFloat(offsetID, i+offset);
            Graphics.Blit(swich ? from : to, swich ? to : from, blur);
            swich = !swich;
        }
        return swich ? from : to;
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination);

        if (!enable) return;
        //GL Clear to reset temporary texture content.
        ClearBuffer(maskTexture);
        //this way to clear camera target temporary texture.  
        //maskCam.clearFlags = CameraClearFlags.Color;
        //maskCam.backgroundColor = Color.black;

        CopyCameraSetting(postProcessCam,maskCam);

        //render mask map
        if (occluder)
        {
            int total = 0;
            foreach (var t in ignoreLayerIndex)
                total += t;
            maskCam.cullingMask = ~(total);
            maskCam.RenderWithShader(grabDepth.shader,alphaDepth ? "" : "RenderType");
            //maskCam.clearFlags = CameraClearFlags.Nothing;
        }

        maskCam.cullingMask = 1 << LayerMask.NameToLayer("Outline");
        //setup mask material
        postMat.SetTexture(maskMapID, maskTexture);
        postMat.SetFloat(intensityID, colorIntensity);
        if (pixelBase)
        {
            maskCam.RenderWithShader(null, "");
            ClearBuffer(tempRT1);
            Graphics.Blit(maskTexture, tempRT1, flatColor, 0);
            //blur
            Graphics.Blit(KawaseBlur(tempRT1, tempRT2), destination, postMat,0);//clip mask
        }
        else {
            maskCam.RenderWithShader(flatColor.shader, "RenderType");
            ClearBuffer(tempRT1);
            Graphics.Blit(maskTexture, tempRT1);
            Graphics.Blit(KawaseBlur(tempRT1, tempRT2), destination, postMat, 0);//clip mask
        }

    }

    void CopyCameraSetting(Camera form, Camera to)
    {
        to.fieldOfView = form.fieldOfView;
        to.nearClipPlane = form.nearClipPlane;
        to.farClipPlane = form.farClipPlane;
        to.rect = form.rect;
    }

    void AttachToRawImage()
    {
        try
        {
            mask.texture = maskTexture;
            temp1.texture = tempRT1;
            temp2.texture = tempRT2;
        }
        catch { }
    }

    void OnDestroy()
    {
        RenderTexture.ReleaseTemporary(tempRT1);
        RenderTexture.ReleaseTemporary(tempRT2);
    }

    void GetTempRenderTexture()
    {
        OnDestroy();
        tempRT1 = RenderTexture.GetTemporary(maskTexture.width / resolutionReduce, maskTexture.height / resolutionReduce, 0, RenderTextureFormat.R8);
        tempRT2 = RenderTexture.GetTemporary(maskTexture.width / resolutionReduce, maskTexture.height / resolutionReduce, 0, RenderTextureFormat.R8);
    }

    void ClearBuffer(RenderTexture rt)
    {
        Graphics.SetRenderTarget(rt);
        GL.Clear(true, true, Color.black);
    }
}