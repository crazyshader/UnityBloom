using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlurPostProcess : MonoBehaviour {

    public enum Kernel {
        Gauss,
        Kawase,
        Dual,
    }
    [SerializeField]
    private Kernel type = Kernel.Dual;
    public Kernel Type
    {
        get { return type; }
        set
        {
            type = value;
            switch (type)
            {
                case Kernel.Gauss:
                    blur = new Material(Shader.Find("Hide/GaussBlurPostProcess"));
                    break;
                case Kernel.Kawase:
                    blur = new Material(Shader.Find("Hide/KawaseBlurPostProcess"));
                    break;
                case Kernel.Dual:
                    blur = new Material(Shader.Find("Hide/DualBlurPostProcess"));
                    break;
            }
        }
    }
    public void SetKernel(int type) {
        Type = (Kernel)type;
    }

    [SerializeField, Range(1, 16)]
    private int resolutionReduce = 4;
    public float ResolutionReduce
    {
        get { return resolutionReduce; }
        set
        {
            resolutionReduce = (int)value;
            if (type == Kernel.Dual)
            {
                ReleaseRT();
                tempsRT = new RenderTexture[interation+1];
                tempsRT[0] = RenderTexture.GetTemporary(width / resolutionReduce, height / resolutionReduce, 16);
                for (int i = 1; i <= interation; i++)
                {
                    tempsRT[i] = RenderTexture.GetTemporary(tempsRT[i - 1].width / 2, tempsRT[i - 1].height / 2, 16);
                }
            }
            else
            {
                ReleaseRT();
                tempsRT = new RenderTexture[2];
                tempsRT[0] = RenderTexture.GetTemporary(width / resolutionReduce, height / resolutionReduce, 16);
                tempsRT[1] = RenderTexture.GetTemporary(tempsRT[0].width, tempsRT[0].height, 16);
            }
        }
    }

    [SerializeField,HideInInspector,Range(0.01f, 10)]
    private float sigma = 1;
    public float Sigma
    {
        get { return sigma; }
        set
        {
            sigma = value;
            blur.SetFloatArray(_Weights, CaculateWeights());
            blur.SetFloat(_Totalweight, CaculateTotalWeight());
        }
    }
    [SerializeField,Range(1, 12)]
    private int interation = 1;
    public float Interation
    {
        get { return interation;}
        set { interation = (int)value;
            OnValidate();
        }
    }

    [SerializeField, Range(0, 10)]
    private float offset = 1;
    public float Offset
    {
        get { return offset; }
        set
        {
            offset = value;
            blur.SetFloatArray(_Weights, CaculateWeights());
            blur.SetFloat(_Totalweight, CaculateTotalWeight());
        }
    }

    [SerializeField, Header("Debug")]
    Material blur;
    [SerializeField]
    RenderTexture[] tempsRT = new RenderTexture[2];
    int _Offset, _Totalweight, _Weights;
    int width = 1920, height = 1080;

    void Start()
    {
        width = Screen.width;
        height = Screen.height;
        //translate string to ID , better speed.
        _Offset = Shader.PropertyToID("_Offset");
        _Totalweight = Shader.PropertyToID("_Totalweight");
        _Weights = Shader.PropertyToID("_Weights");

        OnValidate();
    }

    void GaussBlur(RenderTexture destination)
    {
        for (int i = 0; i < interation; i++)
        {
            ClearBuffer(tempsRT[1]);
            blur.SetVector(_Offset, Vector2.right * offset / resolutionReduce);//h
            Graphics.Blit(tempsRT[0], tempsRT[1], blur);

            ClearBuffer(tempsRT[0]);
            blur.SetVector(_Offset, Vector2.up * offset / resolutionReduce);//v
            Graphics.Blit(tempsRT[1], tempsRT[0], blur);
        }
        Graphics.Blit(tempsRT[0], destination);
    }

    void KawaseBlur(RenderTexture destination)
    {
        bool swich = true;
        for (int i = 0; i < interation; i++)
        {
            ClearBuffer(swich ? tempsRT[1] : tempsRT[0]);
            blur.SetFloat(_Offset, i/resolutionReduce+Offset);
            Graphics.Blit(swich ? tempsRT[0] : tempsRT[1], swich ? tempsRT[1] : tempsRT[0], blur);
            swich = !swich;
        }
        Graphics.Blit(swich ? tempsRT[0] : tempsRT[1], destination);
    }

    void DualBlur(RenderTexture source,RenderTexture destination)
    {
        //down sample
        blur.SetFloat(_Offset, Offset);
        for (int i = 0; i < interation; i++)
        {
            ClearBuffer(tempsRT[i+1]);
            Graphics.Blit(tempsRT[i], tempsRT[i+1], blur,0);
        }
        
        //up sample
        for (int i = interation; i > 1; i--)
        {
            ClearBuffer(tempsRT[i - 1]);
            Graphics.Blit(tempsRT[i], tempsRT[i-1], blur, 1);
        }
        
        Graphics.Blit(tempsRT[1], destination, blur, 1);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.SetRenderTarget(tempsRT[0]);
        GL.Clear(true, true, Color.black);

        Graphics.Blit(source, tempsRT[0]);
        switch (type)
        {
            case Kernel.Gauss:
                GaussBlur(destination); break;
            case Kernel.Kawase:
                KawaseBlur(destination); break;
            case Kernel.Dual:
                DualBlur(source, destination); break;
        }
    }

    public void OnValidate()
    {
        ResolutionReduce = resolutionReduce;
        Type = type;
        Sigma = sigma;
        Offset = offset;
    }

    void OnDestroy()
    {
        ReleaseRT(); 
    }

    //Support method
    float CaculateWeight(float sigma, float r)//use gauss math.
    {
        return 0.39894f / sigma * sigma * Mathf.Pow(2.718f, -r * r / (2 * sigma * sigma));
    }

    float[] CaculateWeights()
    {
        return new float[]
        {
            CaculateWeight(sigma,0)
            ,CaculateWeight(sigma,1 * Offset)
            ,CaculateWeight(sigma,2 * Offset)
        };
    }

    float CaculateTotalWeight()
    {
        return CaculateWeight(sigma, 0) + CaculateWeight(sigma, 1 * Offset) * 2 + CaculateWeight(sigma, 2 * Offset) * 2;
    }

    void CopyCameraSetting(Camera form, Camera to)
    {
        to.fieldOfView = form.fieldOfView;
        to.nearClipPlane = form.nearClipPlane;
        to.farClipPlane = form.farClipPlane;
        to.rect = form.rect;
    }

    void ClearBuffer(RenderTexture rt) {
        Graphics.SetRenderTarget(rt);
        GL.Clear(true, true, Color.black);
    }

    void ReleaseRT() {
        foreach (var item in tempsRT)
            RenderTexture.ReleaseTemporary(item);
    }

}
