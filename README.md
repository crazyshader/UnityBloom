# UnityBloom
Unity Bloom效果的五种实现方式，原理分析和性能对比，对项目提供使用决策。

环境：魅族Note5 GPU Mali-T860 联发科p10

场景 | 平均FPS
---|---
UnityBloom | 24
KinoBloom | 29
AmplifyBloom | 28
FastMobileBloom | 30
UltimateBloom | 30

参考资料
- https://www.youtube.com/watch?v=xWKo50WS2qs
- https://software.intel.com/en-us/blogs/2014/07/15/an-investigation-of-fast-real-time-gpu-based-image-blur-algorithms
- https://static.docs.arm.com/100140/0302/arm_guide_for_unity_developers_optimizing_mobile_gaming_graphics_100140_0302_00_en.pdf?_ga=2.80780304.62981651.1547376579-1587329921.1547376579
- https://github.com/keijiro/KinoBloom?1547462930302
- https://github.com/Unity-Technologies/PostProcessing/wiki/Bloom
- https://github.com/PcloD/Unity_BlurPostProcessSample
- https://eternity429.wordpress.com/2017/08/16/blur%E5%AF%A6%E4%BD%9C%E8%88%87%E6%87%89%E7%94%A8/
- http://www.cgiso.com/forum.php?mod=viewthread&tid=1725
- https://github.com/hdmmY/Bloom-Effect-Unity
- https://github.com/nobnak/GaussianBlurUnity
- https://github.com/a3geek/Bloom/blob
- https://community.arm.com/graphics/b/blog/posts/post-processing-effects-for-mobile-at-gdc18
- https://community.arm.com/graphics/b/blog/posts/post-processing-effects-on-mobile-optimization-and-alternatives

注意：在Unity5.4.5上面后处理和抗锯齿需要开启Use 32-bit Display Buffer，否则显示出现问题，后来验证在unity2018.3没有问题，只是效果缺失

Unity 关于崩坏3Bloom（部分泛光）效果实现原理
- https://blog.csdn.net/SnoopyNa2Co3/article/details/85246042
- https://blog.csdn.net/SnoopyNa2Co3/article/details/87947553
- https://blog.csdn.net/SnoopyNa2Co3/article/details/88075047

- https://github.com/gam0022/image-effect-stencil-test
- http://qiankanglai.me/2015/03/07/unity-posteffect-stencil/
- https://cowlevel.net/article/1917881
- https://answers.unity.com/questions/621279/using-the-stencil-buffer-in-a-post-process.html
- https://forum.unity.com/threads/unity-5-6-0f3-empty-stencil-buffer-onrenderimage.473444/
- https://zhuanlan.zhihu.com/p/31126847
