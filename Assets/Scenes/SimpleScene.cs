using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleScene : MonoBehaviour
{
    private int preScene = 1;
    private int nextScene = 2;

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
        if (scene.buildIndex == 2)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(nextScene));
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(preScene));
        }
    }

    public void LoadArrayScene()
    {
        SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
    }
}
