using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
static public class SceneViewDrawFPS  {

    static int sampleCount = 60;
    static int[] fpsData = new int[sampleCount];
    static int index;

    static int highestFPS;
    static int averageFPS;
    static int lowestFPS = int.MaxValue;

    private const string MENU_NAME = "Tools/FPS Display";

    [MenuItem(MENU_NAME)]
    static void Switch() {
        EditorPrefs.SetBool("FPSDisplay",!EditorPrefs.GetBool("FPSDisplay"));
        SetCheckmark();
    }

    static void SetCheckmark() {
        //Debug.Log(EditorPrefs.GetBool("FPSDisplay"));
        Menu.SetChecked(MENU_NAME, EditorPrefs.GetBool("FPSDisplay"));
    }

    static SceneViewDrawFPS()
    {
        SceneView.onSceneGUIDelegate += DrawFPS;
        EditorApplication.update += Update;
        //EditorApplication.delayCall += SetCheckmark;
    }

    static void Update()
    {
        if (!EditorPrefs.GetBool("FPSDisplay")) return;

        //reset fps data
        if (index >= sampleCount) {
            index = 0;
            highestFPS = 0;
            lowestFPS = int.MaxValue;
        }
        //caculate fps
        fpsData[index++] = (int)(1f / Time.unscaledDeltaTime);

        int sum = 0;
        for (int i = 0; i < sampleCount; i++)
        {

            sum += fpsData[i];
            if (fpsData[i] > highestFPS)
                highestFPS = fpsData[i];
            if (fpsData[i] < lowestFPS)
                lowestFPS = fpsData[i];
        }
        averageFPS = sum / sampleCount;
    }

    static void DrawFPS(SceneView sceneView) {
        if (!EditorPrefs.GetBool("FPSDisplay")) return;
        SetCheckmark();

        Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(0, 0, 150, 120));
                GUI.color = Color.green;
                GUILayout.Label(string.Format("Highest FPS:{0}", highestFPS));

                GUI.color = Color.yellow;
                GUILayout.Label(string.Format("Average FPS:{0}", averageFPS));

                GUI.color = Color.red;
                GUILayout.Label(string.Format("Lowest FPS:{0}", lowestFPS));

        GUILayout.EndArea();
        Handles.EndGUI();

        sceneView.Repaint();
    }
}
