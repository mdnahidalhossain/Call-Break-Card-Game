using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.XR;
using Unity.VisualScripting;
using static SetPlayerBid;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public GameObject cardPrefab;       // Assign the card prefab in the inspector
    public Sprite[] cardSprites;        // Assign the 52 card sprites in order
    public Transform[] playerHands;     // Array of player hand containers (e.g., Player1Hand, Player2Hand, etc.)
    public Transform deckParent;        // Parent transform for the deck

    private List<CardUI> deck = new List<CardUI>();  // List to store the deck of cards
    private List<List<CardUI>> playersHands = new List<List<CardUI>>();  // List to store players' hands

    private int currentPlayer = 0;
    public Transform centerPosition;    // Transform for the center of the game board
    private bool[] hasDrawn = new bool[4]; // Track whether a player has drawn their card
    private bool isPlayer3Turn = false; // Flag to track if it's Player 3's turn

    private string currentSuit; // Track the current suit being played
    private int highestRank;    // Track the highest rank of the current suit played
    private List<CardUI> playedCards = new List<CardUI>();  // List to track played cards
    private Dictionary<CardUI, int> playedCardOwners = new Dictionary<CardUI, int>(); // Map of played cards to their owners

    //private int drawnCount = 0;

    private int startPlayer = 0; // New variable to track the starting player

    //public GameObject cardDealPanel;

    public SetPlayerBid biddingSystem;

    private float bidValue = 0f;
    private float score1 = 0f;
    private float score2 = 0f;
    private float score3 = 0f;
    private float score4 = 0f;

    //public Text player1Score, player2Score, player3Score, player4Score;
    public Text[] playerScore;

    public GameObject scoreBoardPanel;
    public Text[] showPlayerTotalScore;

    private Dictionary<int, int> playerBids = new Dictionary<int, int>();
    public Text player1Bid, player2Bid, player4Bid;

    private bool isGameOver = false;
    private int[] cardsPlayedByPlayer = new int[4];



    void Start()
    {
        CreateDeck();
        ShuffleDeck();
        DealCards(13);  // Deal 13 cards to each player
        DisplayPlayerHands();

        //StartCoroutine(RandomAutoDrawPlayer1());

        // Subscribe to the BidConfirmed event from BiddingSystem
        biddingSystem.BidConfirmed += OnBidConfirmed;

        // Call the StartBidding method from the BiddingSystem when you're ready to show the bidding panel
        biddingSystem.ShowBiddingPanel();

        // Calculate and store bid values for AI players
        for (int i = 0; i < playersHands.Count; i++)
        {
            if (i != 2) // Skip Player 3 (manual player)
            {
                int bidValue = CalculateBidValue(playersHands[i]);
                playerBids[i] = bidValue;
                Debug.Log($"Player {i + 1} bid: {bidValue}");
            }
        }

        UpdateBidUI();
    }

    private int GetCardRank(CardUI card)
    {
        return card.rank == 1 ? 14 : card.rank; // Ace (1) becomes 14 for comparison
    }

    private void OnBidConfirmed(int bidValue)
    {
        Debug.Log("Bid confirmed: " + bidValue);

        // Now that the bid is confirmed, proceed to deal cards or other game logic
        //DealCards(13);

        StartCoroutine(RandomAutoDrawPlayer1());
    }

    private int CalculateBidValue(List<CardUI> playerHand)
    {
        //float bidValue = 0f;

        foreach (CardUI card in playerHand)
        {
            //Assign bid values based on card rank
            if (card.rank == 1 || card.rank == 13) // Ace or King
            {
                bidValue += 1;
            }
            // Add more conditions for other high-ranking cards if needed
            //if (card.suit == "Spades" && card.rank == 14 || card.rank == 13 || card.rank == 14) // Ace or King
            //{
            //    bidValue += 1;
            //}
        }

        return (int)(bidValue > 0 ? bidValue : 1);
    }

    private void UpdateBidUI()
    {
        // Update the UI Text elements with the calculated bid values
        if (player1Bid != null && playerBids.ContainsKey(0))
        {
            player1Bid.text = $"/ {playerBids[0]}";
        }
        if (player2Bid != null && playerBids.ContainsKey(1))
        {
            player2Bid.text = $"/ {playerBids[1]}";
        }
        if (player4Bid != null && playerBids.ContainsKey(3))
        {
            player4Bid.text = $"/ {playerBids[3]}";
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Check for left mouse button click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null) // Check if we hit something
            {
                Debug.Log("Clicked on: " + hit.collider.name);

                // Check if the clicked object has a CardUI component
                CardUI clickedCard = hit.collider.GetComponent<CardUI>();
                if (clickedCard != null)
                {
                    OnClickCard(clickedCard); // Call the card click handler
                }
            }
        }
    }

    

    // Create a standard deck of 52 cards
    void CreateDeck()
    {
        string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
        int spriteIndex = 0;

        foreach (string suit in suits)
        {
            for (int rank = 1; rank <= 13; rank++)
            {
                GameObject cardObj = Instantiate(cardPrefab, deckParent);
                CardUI card = cardObj.GetComponent<CardUI>();
                card.InitializeCard(suit, rank, cardSprites[spriteIndex]);
                deck.Add(card);
                spriteIndex++;
            }
        }
    }

    // Shuffle the deck using the Fisher-Yates algorithm
    void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            CardUI temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    // Deal cards to players
    void DealCards(int numCards)
    {
        for (int i = 0; i < playerHands.Length; i++)
        {
            playersHands.Add(new List<CardUI>());
        }

        int cardIndex = 0;
        for (int i = 0; i < numCards; i++)
        {
            foreach (var playerHand in playersHands)
            {
                if (cardIndex < deck.Count)
                {
                    playerHand.Add(deck[cardIndex]);
                    cardIndex++;
                }
            }
        }
    }

    // Display the cards in the players' hands with overlapping and shadows
    void DisplayPlayerHands()
    {
        float overlapOffsetX = 0f;  // Horizontal overlap offset

        float overlapOffsetXForP3 = 60f;  // Horizontal overlap offset

        for (int i = 0; i < playersHands.Count; i++)
        {
            Vector3 startPosition = Vector3.zero; // Default start position
            float offsetX = overlapOffsetX;

            if (i == 0) // Player 1
                startPosition = new Vector3(-200f, 100f, 0);
            else if (i == 1) // Player 2
                startPosition = new Vector3(-280f, 150f, 0);

            // Player 3
            else if (i == 2)
            {
                startPosition = new Vector3(-200f, 0f, 0);
                offsetX = overlapOffsetXForP3;
            } 
                
            else // Player 4
                startPosition = new Vector3(50f, 150f, 0);

            for (int j = 0; j < playersHands[i].Count; j++)
            {
                CardUI card = playersHands[i][j];
                card.transform.SetParent(playerHands[i]);

                float posX = startPosition.x + (j * offsetX);
                card.transform.localPosition = new Vector3(posX, startPosition.y, 0);

                card.transform.localRotation = Quaternion.Euler(0, 0, 0);

                SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
                sr.sortingOrder = j * 2 + 1;

                Transform shadowTransform = card.transform.Find("Shadow");
                if (shadowTransform != null)
                {
                    SpriteRenderer shadowSR = shadowTransform.GetComponent<SpriteRenderer>();
                    shadowSR.sortingOrder = sr.sortingOrder - 1;
                }

                card.gameObject.SetActive(true);
            }
        }
    }


    public void OnClickCard(CardUI clickedCard)
    {
        if (currentPlayer == 2) // Player 3 (manual)
        {
            if (playersHands[2].Contains(clickedCard))
            {
                // Check if any card has been played by other players (i.e., if board is empty or not)
                if (playedCards.Count > 0) // If any card has been played, Player 3 needs to follow the rules
                {
                    if (IsValidCardForPlayer3(clickedCard))
                    {
                        PlayCard(clickedCard);
                        playedCards.Add(clickedCard);
                    }
                    else
                    {
                        Debug.Log("Invalid card! Follow the rules.");
                    }
                }
                else // If no card has been played yet, Player 3 can choose any card
                {
                    ResetCardColorsForPlayer3();
                    PlayCard(clickedCard);
                    playedCards.Add(clickedCard);
                    Debug.Log("Player 3 played a card.");
                }
            }
        }
        else
        {
            Debug.Log("Not Player 3's turn.");
        }
    }


    public bool IsPlayerTurn(CardUI card)
    {
        return playersHands[currentPlayer].Contains(card);
    }

    void SwitchTurn()
    {
        currentPlayer = (currentPlayer + 1) % playerHands.Length;

        if (centerPosition.childCount < 4)
        {
            TriggerPlayerTurn(currentPlayer);
        }
        else
        {
            CheckAndClearCenter();

        }
    }

    void TriggerPlayerTurn(int player)
    {
        if (player == 0)
        {
            StartCoroutine(AutoDrawPlayer1());
        }
        else if (player == 1)
        {
            StartCoroutine(AutoDrawPlayer2());
        }
        else if (player == 2)
        {
            isPlayer3Turn = true;
            Debug.Log("Player3's turn (manual).");
            UpdateCardColorsForPlayer3();
        }
        else if (player == 3)
        {
            StartCoroutine(AutoDrawPlayer4());
        }
    }

    public void PlayCard(CardUI card)
    {
        if (playersHands[currentPlayer].Contains(card))
        {
            playersHands[currentPlayer].Remove(card);

            card.transform.SetParent(centerPosition);

            //Vector3[] playPositions = {
            //    new Vector3(0f, 1.6f, 0),
            //    new Vector3(-2.5f, 0f, 0),
            //    new Vector3(0f, -1.6f, 0),
            //    new Vector3(2.5f, 0f, 0)
            //};

            //card.transform.localPosition = playPositions[currentPlayer];
            //card.transform.localScale = Vector3.one / 8f;

            StartCoroutine(AnimateCardToCenter(card));

            if (currentSuit == null)
            {
                currentSuit = card.suit;
                highestRank = GetCardRank(card);
            }
            else if (card.suit == currentSuit && GetCardRank(card) > highestRank)
            {
                highestRank = GetCardRank(card);
            }

            // Record the played card details
            playedCardOwners[card] = currentPlayer;
            playedCards.Add(card);

            cardsPlayedByPlayer[currentPlayer]++;

            SwitchTurn();

            // Check if all players have played all their cards
            CheckForGameOver();

        }
    }

    private void CheckForGameOver()
    {
        // Check if all players have played 13 cards
        bool isGameOver = true;
        for (int i = 0; i < cardsPlayedByPlayer.Length; i++)
        {
            if (cardsPlayedByPlayer[i] < 13)
            {
                isGameOver = false;
                break;
            }
        }

        if (isGameOver)
        {
            Debug.Log("Game Over! All 13 cards have been played by each player.");
            // You can add additional game-over logic here, such as displaying a UI message or resetting the game.
            //scoreBoard.gameObject.SetActive(true);

            //ScoreManager.SaveScores(score1, score2, score3, score4);

            StartCoroutine(DealNewCard());
        }
    }

    private IEnumerator DealNewCard()
    {
        yield return new WaitForSeconds(2.0f);

        //scoreBoardPanel.gameObject.SetActive(true);

        ShowScoreboard();

        //Debug.Log("Starting new game. Dealing new cards.");

        //yield return new WaitForSeconds(3.0f);

        //CreateDeck();
        //ShuffleDeck();
        //DealCards(13);  // Deal 13 cards to each player
        //DisplayPlayerHands();
    }

    private IEnumerator AnimateCardToCenter(CardUI card)
    {
        // Target position and scale
        Vector3[] playPositions = {
        new Vector3(0f, 1.6f, 0),
        new Vector3(-2.5f, 0f, 0),
        new Vector3(0f, -1.6f, 0),
        new Vector3(2.5f, 0f, 0)
    };

        Vector3 startPosition = card.transform.position;
        Vector3 targetPosition = centerPosition.position + playPositions[currentPlayer];

        Vector3 startScale = card.transform.localScale;
        Vector3 targetScale = Vector3.one / 8f;

        float duration = 0.2f; // Time in seconds for the animation
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Smoothly move and scale the card
            card.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            card.transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / duration);

            yield return null;
        }

        // Ensure the final position and scale are exact
        card.transform.position = targetPosition;
        card.transform.localScale = targetScale;

        // Parent the card to the center position
        //card.transform.SetParent(centerPosition);

        // Update the suit and highest rank
        

        // Switch turn after the animation
        
    }

    bool IsValidCardForPlayer3(CardUI card)
    {
        // Step 1: Check for cards of the same suit as the currentSuit
        List<CardUI> sameSuitCards = playersHands[2].FindAll(c => c.suit == currentSuit);

        if (sameSuitCards.Count > 0)
        {
            // Validate card if it matches the current suit
            if (card.suit == currentSuit)
            {
                // If higher-ranked cards exist in the same suit, validate against them
                List<CardUI> higherCards = sameSuitCards.FindAll(c => GetCardRank(c) > highestRank);
                return higherCards.Count == 0 || higherCards.Contains(card);
            }
            return false; // Invalid if player has the same suit but chooses another card
        }

        // Step 2: If no cards of the same suit are available, check for Spades
        List<CardUI> spadeCards = playersHands[2].FindAll(c => c.suit == "Spades");
        if (spadeCards.Count > 0)
        {
            // Only Spades are valid in this scenario
            return card.suit == "Spades";
        }

        // Step 3: If neither the current suit nor Spades are available, any card is valid
        return true;
    }

    private void UpdateCardColorsForPlayer3()
    {
        foreach (CardUI card in playersHands[2]) // Iterate through Player3's hand
        {
            SpriteRenderer sr = card.GetComponent<SpriteRenderer>();

            if (IsValidCardForPlayer3(card))
            {
                // Restore the original color for valid cards
                sr.color = Color.white; // Default color
            }
            else
            {
                // Dim the color for invalid cards
                sr.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Dimmed color


            }
        }
    }

    public void ResetCardColorsForPlayer3()
    {
        foreach (CardUI card in playersHands[2]) // Iterate through Player3's hand
        {
            SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
            sr.color = Color.white; // Reset to default color
        }
    }

    void CheckAndClearCenter()
    {
        if (centerPosition.childCount == 4)
        {
            StartCoroutine(ClearCenter());
        }
    }

    public void DetermineWinner()
    {
        // Determine the round winner
        CardUI chosenCard = playedCards[0]; // The first card played
        string prioritySuit = chosenCard.suit;

        CardUI winningCard = chosenCard;
        int winningPlayer = playedCardOwners[chosenCard];
        int highestCardRank = GetCardRank(chosenCard);

        // Step 1: Check cards of the priority suit
        foreach (var card in playedCards)
        {
            if (card.suit == prioritySuit)
            {
                int cardRank = GetCardRank(card);
                if (cardRank > highestCardRank)
                {
                    highestCardRank = cardRank;
                    winningCard = card;
                    winningPlayer = playedCardOwners[card];
                }
            }
        }

        // Step 2: If no card from the priority suit is played, prioritize Spades
        if (winningCard.suit != "Spades")
        {
            foreach (var card in playedCards)
            {
                if (card.suit == "Spades")
                {
                    int cardRank = GetCardRank(card);
                    if (winningCard.suit != "Spades" || cardRank > highestCardRank)
                    {
                        highestCardRank = cardRank;
                        winningCard = card;
                        winningPlayer = playedCardOwners[card];
                    }
                }
            }
        }

        // Step 3: If no Spades are played, the highest-ranking card of the priority suit determines the winner
        if (winningCard.suit != prioritySuit && winningCard.suit != "Spades")
        {
            foreach (var card in playedCards)
            {
                int cardRank = GetCardRank(card);
                if (cardRank > highestCardRank)
                {
                    highestCardRank = cardRank;
                    winningCard = card;
                    winningPlayer = playedCardOwners[card];
                }
            }
        }

        // Log the round winner
        Debug.Log("Round Winner: Player " + (winningPlayer + 1) + " with " + winningCard.suit + " " + winningCard.rank);

        // Update startPlayer with the winner of the round
        if (winningPlayer == 2)
        {
            score3++;
            playerScore[2].text = score3.ToString();
            startPlayer = 2; // Set Player 3 as the start player for the next round
            Debug.Log("Player 3 won and will start the next round.");
        }
        else
        {
            startPlayer = winningPlayer; // Set the winner as the start player

            if (winningPlayer == 0)
            {
                score1++;
                //player1Score.text = score1.ToString();
                playerScore[0].text = score1.ToString();
            }
            else if (winningPlayer == 1)
            {
                score2++;
                //player2Score.text = score2.ToString();
                playerScore[1].text = score2.ToString();
            }
            else if (winningPlayer == 3)
            {
                score4++;
                //player4Score.text = score4.ToString();
                playerScore[3].text = score4.ToString();
            }
        }

        ScoreManager.SaveScores(score1, score2, score3, score4);

        CheckForGameOver();
    }

    private void ShowScoreboard()
    {
        if (scoreBoardPanel != null)
        {
            // Activate the scoreboard panel
            scoreBoardPanel.SetActive(true);

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

    IEnumerator ClearCenter()
    {
        yield return new WaitForSeconds(1f);

        DetermineWinner();

        playedCards.Clear();
        playedCardOwners.Clear();
        currentSuit = null;
        highestRank = 0;

        foreach (Transform card in centerPosition)
        {
            Destroy(card.gameObject);
        }

        //Debug.Log("Center cleared! Starting the next round...");

        yield return new WaitForSeconds(1.5f);

        StartNextRound();
        ResetCardColorsForPlayer3();
    }

    void StartNextRound()
    {
        // Use the winner of the previous round to determine who starts the new round
        currentPlayer = startPlayer; // Set the starting player for the round
        playedCards.Clear();
        playedCardOwners.Clear();
        currentSuit = null;
        highestRank = 0;

        Debug.Log($"Starting new round. Player {startPlayer + 1} starts.");
        //TriggerPlayerTurn(currentPlayer);

        // Let the winner play a card
        if (currentPlayer != 2) // If the winner is not Player3 (Player3 is index 2)
        {
            if (playersHands[currentPlayer].Count > 0)
            {
                PlayRandomCardForPlayer(currentPlayer); // Automatically draw a random card
            }
        }
        else
        {
            Debug.Log("Player 3 starts the round and will manually draw a card.");
            // Player3 will manually draw; no action needed here
            TriggerPlayerTurn(currentPlayer);
        }
    }

    private void PlayRandomCardForPlayer(int player)
    {
        if (playersHands[player].Count > 0)
        {
            // Choose a random card from the player's hand
            int randomIndex = Random.Range(0, playersHands[player].Count);
            //CardUI randomCard = playersHands[player][randomIndex];

            CardUI randomCard = null;

            List<CardUI> hand = playersHands[player];

            List<CardUI> aces = hand.FindAll(card => GetCardRank(card) == 14);

            if (aces.Count > 0)
            {
                // Randomly select an Ace if multiple are available
                randomCard = aces[Random.Range(0, aces.Count)];
            }
            else
            {
                // Step 2: If no Ace, draw any random card
                randomCard = hand[Random.Range(0, hand.Count)];
            }

            // Play the chosen card
            PlayCard(randomCard);
        }
    }



    IEnumerator RandomAutoDrawPlayer1()
    {
        yield return new WaitForSeconds(2f);

        // Get Player1's hand
        List<CardUI> hand = playersHands[0];
        CardUI chosenCard = null;

        // Step 1: Check for any Ace in the hand
        List<CardUI> aces = hand.FindAll(card => GetCardRank(card) == 14);

        if (aces.Count > 0)
        {
            // Randomly select an Ace if multiple are available
            chosenCard = aces[Random.Range(0, aces.Count)];
        }
        else
        {
            // Step 2: If no Ace, draw any random card
            chosenCard = hand[Random.Range(0, hand.Count)];
        }

        // Play the chosen card
        PlayCard(chosenCard);
        Debug.Log("Player1 played: " + chosenCard.suit + " " + chosenCard.rank);

        // Proceed to Player2's turn
        //StartCoroutine(AutoDrawPlayer2());
    }

    IEnumerator AutoDrawPlayer1()
    {
        yield return new WaitForSeconds(2f);

        AutoPlayCard(0);
    
    }


    IEnumerator AutoDrawPlayer2()
    {
        yield return new WaitForSeconds(1.5f);
        AutoPlayCard(1);

        currentPlayer = 2;
        Debug.Log("Player3's turn (manual).");

        //UpdateCardColorsForPlayer3();
    }

    IEnumerator AutoDrawPlayer4()
    {
        ResetCardColorsForPlayer3();

        yield return new WaitForSeconds(1.5f);

        AutoPlayCard(3);
        //StartCoroutine(AutoDrawPlayer1());
    }

    void AutoPlayCard(int playerIndex)
    {
        List<CardUI> hand = playersHands[playerIndex]; // Get the player's hand
        CardUI chosenCard = null;

        // Step 1: Check for cards of the same suit
        List<CardUI> sameSuitCards = hand.FindAll(card => card.suit == currentSuit);

        if (sameSuitCards.Count > 0)
        {
            // Check if there's an Ace (rank 14) of the same suit
            //chosenCard = sameSuitCards.Find(card => GetCardRank(card) == 14);
            sameSuitCards.Find(card => card.rank > highestRank);

            if (chosenCard == null)
            {
                // If no Ace, play the smallest card higher than the highest rank, or the smallest card in the same suit
                sameSuitCards.Sort((a, b) => GetCardRank(a).CompareTo(GetCardRank(b)));

                chosenCard = sameSuitCards.Find(card => GetCardRank(card) > highestRank) ?? sameSuitCards[0];
            }
        }
        else
        {
            // Step 3: No cards of the same suit, look for Spades
            List<CardUI> spadeCards = hand.FindAll(card => card.suit == "Spades");

            if (spadeCards.Count > 0)
            {
                // Sort Spades by rank in ascending order
                spadeCards.Sort((a, b) => GetCardRank(a).CompareTo(GetCardRank(b)));

                // Play the lowest Spades card
                chosenCard = spadeCards[0];
            }
            else
            {
                // Step 4: No Spades available, play any random card
                chosenCard = hand[Random.Range(0, hand.Count)];
            }
        }

        // Play the chosen card
        PlayCard(chosenCard);
        Debug.Log("Player" + (playerIndex + 1) + " played: " + chosenCard.suit + " " + chosenCard.rank);
    }
}