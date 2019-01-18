using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public Text CurSceneName;

    private string[] m_SceneNames = {
        "UnityBloom",
        "KinoBloom",
        "AmplifyBloom",
        "FastMobileBloom",
        "UltimateBloom",
        "BlurPostProcess"
    };
    private string m_InitScene = "SceneLoader";

    void Start()
    {
        SceneManager.LoadSceneAsync(m_SceneNames[0], LoadSceneMode.Additive);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string loadScene = scene.name;
        CurSceneName.text = "Scene:" + loadScene;
        Debug.LogFormat("OnSceneLoaded:{0}", loadScene);
        if (loadScene == m_InitScene) return;

        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(scene);
        if (activeScene != null && activeScene.name != m_InitScene)
        {
            SceneManager.UnloadSceneAsync(activeScene);
        }
    }

    public void SceneLoad(GameObject obj)
    {
        string sceneName = m_SceneNames[int.Parse(obj.name)];
        Scene activeScene = SceneManager.GetActiveScene();
        if (sceneName != activeScene.name)
        {
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }
    }
}
