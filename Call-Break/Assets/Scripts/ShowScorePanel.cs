using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowScorePanel : MonoBehaviour
{
    //bool isButtonClicked = false;
    //bool isPanelActive = false;
    public GameObject scoreBoardPanel;
    private ScoreData scoreData;

    public Text[] showPlayerTotalScore;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowScoreBoardOnClick()
    {
        //scoreBoardPanel.SetActive(!scoreBoardPanel.activeSelf);

        if (scoreBoardPanel != null)
        {
            // Activate the scoreboard panel
            //scoreBoardPanel.SetActive(true);
            scoreBoardPanel.SetActive(!scoreBoardPanel.activeSelf);

            // Load the saved scores
            ScoreData scores = ScoreManager.LoadScores();

            // Update the UI with the total scores
            if (showPlayerTotalScore.Length >= 4)
            {
                showPlayerTotalScore[0].text = scores.player1Score.ToString();
                showPlayerTotalScore[1].text = scores.player2Score.ToString();
                showPlayerTotalScore[2].text = scores.player3Score.ToString();
                showPlayerTotalScore[3].text = scores.player4Score.ToString();
            }
        }
    }
}
