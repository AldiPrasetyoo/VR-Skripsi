using UnityEngine;
using System;
using System.IO;

public class FileLogger : MonoBehaviour
{
    private static string logFile;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CreateLogger()
    {
        GameObject go = new GameObject("FileLogger");
        DontDestroyOnLoad(go);
        go.AddComponent<FileLogger>();
    }

    private void Awake()
    {
        logFile = Path.Combine(Application.persistentDataPath, "DebugLog.txt");

        File.WriteAllText(
            logFile,
            "========================================\n" +
            "VR DEBUG LOG\n" +
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
            "\n========================================\n\n");

        Application.logMessageReceived += HandleLog;

        Debug.Log("File Logger Started");
        Debug.Log("Log Path : " + logFile);
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string condition, string stackTrace, LogType type)
    {
        try
        {
            File.AppendAllText(
                logFile,
                $"[{DateTime.Now:HH:mm:ss}] [{type}] {condition}\n");

            if (type == LogType.Error ||
                type == LogType.Exception ||
                type == LogType.Assert)
            {
                File.AppendAllText(logFile, stackTrace + "\n");
            }
        }
        catch
        {
            // jangan sampai logger bikin game crash
        }
    }
}