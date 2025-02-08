using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetPlayerBid : MonoBehaviour
{
    public GameObject cardDealPanel;  // Reference to the card deal panel
    public Button[] bidButtons;       // Array of bid buttons (13 buttons)
    public Button callButton;         // The call button to confirm the bid
    private int selectedBid = -1;     // Variable to store the selected bid

    // Event that other scripts can subscribe to when the bid is confirmed
    public delegate void OnBidConfirmed(int bidValue);
    public event OnBidConfirmed BidConfirmed;

    private List<CardUI>[] playerHands;

    public Text player3Bid;
    //public Text player1Bid, player2Bid, player4Bid;

    void Start()
    {
        // Set the panel inactive initially
        //cardDealPanel.SetActive(true);
    }

    public void ShowBiddingPanel()
    {
        StartCoroutine(StartBidding());
    }

    // Call this method when it's time to show the bidding UI
    private IEnumerator StartBidding()
    {
        yield return new WaitForSeconds(0.5f);

        cardDealPanel.SetActive(true);

        // Remove old listeners first to prevent stacking
        foreach (Button btn in bidButtons)
            btn.onClick.RemoveAllListeners();

        callButton.onClick.RemoveAllListeners();

        //Now add fresh listeners
        for (int i = 0; i < bidButtons.Length; i++)
        {
            int bidValue = i + 1;
            bidButtons[i].onClick.AddListener(() => SetBid(bidValue));
        }

        callButton.onClick.AddListener(ConfirmBid);
    }

    // Method to set the selected bid when a bid button is clicked
    private void SetBid(int bidValue)
    {
        selectedBid = bidValue;
        Debug.Log("Bid selected: " + selectedBid);

        // Optionally, you can visually update the selected bid (e.g., change button color or display the bid).
        
    }
    //private void SetAIBids()
    //{
    //    int[] aiBids = new int[3];

    //    // Generate AI bids for Player1, Player2, and Player4
    //    aiBids[0] = GenerateAIBid(0); // Player1
    //    aiBids[1] = GenerateAIBid(1); // Player2
    //    aiBids[2] = GenerateAIBid(3); // Player4

    //    // Display AI bids in UI
    //    player1Bid.text = "/ " + aiBids[0];
    //    player2Bid.text = "/ " + aiBids[1];
    //    player4Bid.text = "/ " + aiBids[2];

    //    // Notify listeners that AI bids are set
    //    BidConfirmed?.Invoke(aiBids[0]); // Player1
    //    BidConfirmed?.Invoke(aiBids[1]); // Player2
    //    BidConfirmed?.Invoke(aiBids[2]); // Player4
    //}

    //private int GenerateAIBid(int playerIndex)
    //{
    //    if (playerHands == null || playerHands.Length <= playerIndex) return 1;

    //    int bid = 0;

    //    foreach (CardUI card in playerHands[playerIndex])
    //    {
    //        int rank = GetCardRank(card);

    //        // AI strategy: Consider cards of rank 10 or higher as strong cards
    //        if (rank >= 10) bid++;
    //    }

    //    return Mathf.Clamp(bid, 1, 13); // Ensure valid bid range
    //}

    private int GetCardRank(CardUI card)
    {
        // Assuming CardUI has a method or property to get the rank
        return card.rank; // Replace with your actual card rank logic
    }

    // Method to confirm the bid when the call button is clicked
    private void ConfirmBid()
    {
        if (selectedBid == -1)
        {
            Debug.LogWarning("No bid selected!");
            return; // Do nothing if no bid is selected
        }

        // Raise the event to notify that the bid has been confirmed
        BidConfirmed?.Invoke(selectedBid);

        // Hide the panel after confirming the bid
        cardDealPanel.SetActive(false);

        player3Bid.text = "/ " + selectedBid.ToString();

        //SetAIBids();
    }
}
