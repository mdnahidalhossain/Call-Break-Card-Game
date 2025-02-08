using System.IO;
using UnityEngine;

[System.Serializable]
public class ScoreData
{
    public float player1Score;
    public float player2Score;
    public float player3Score;
    public float player4Score;
}

public class ScoreManager : MonoBehaviour
{
    //private static string filePath;

    //void Awake()
    //{
    //    filePath = Application.persistentDataPath + "/scoreData.json";
    //}

    private static string filePath = Application.persistentDataPath + "/scoreData.json";
    public static void SaveScores(float player1, float player2, float player3, float player4)
    {
        ScoreData data = new ScoreData
        {
            player1Score = player1,
            player2Score = player2,
            player3Score = player3,
            player4Score = player4
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);

        Debug.Log("Scores saved to: " + filePath);
    }

    public static ScoreData LoadScores()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            ScoreData data = JsonUtility.FromJson<ScoreData>(json);
            return data;
        }
        else
        {
            Debug.LogWarning("No previous scores found!");
            return new ScoreData();
        }
    }
}
