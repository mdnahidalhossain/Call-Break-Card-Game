using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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

    private int drawnCount = 0;

    private int startPlayer = 0; // New variable to track the starting player



    void Start()
    {
        CreateDeck();
        ShuffleDeck();
        DealCards(13);  // Deal 13 cards to each player
        DisplayPlayerHands();

        StartCoroutine(RandomAutoDrawPlayer1());
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

    private int GetCardRank(CardUI card)
    {
        return card.rank == 1 ? 14 : card.rank; // Ace (1) becomes 14 for comparison
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
        float overlapOffsetX = 60f;  // Horizontal overlap offset

        for (int i = 0; i < playersHands.Count; i++)
        {
            Vector3 startPosition = Vector3.zero; // Default start position
            float offsetX = overlapOffsetX;

            if (i == 0) // Player 1
                startPosition = new Vector3(-200f, 100f, 0);
            else if (i == 1) // Player 2
                startPosition = new Vector3(-280f, 150f, 0);
            else if (i == 2) // Player 3
                startPosition = new Vector3(-200f, 0f, 0);
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

            Vector3[] playPositions = {
                new Vector3(0f, 1.6f, 0),
                new Vector3(-2.5f, 0f, 0),
                new Vector3(0f, -1.6f, 0),
                new Vector3(2.5f, 0f, 0)
            };

            card.transform.localPosition = playPositions[currentPlayer];
            card.transform.localScale = Vector3.one / 8f;

            // Update the suit and highest rank
            if (currentSuit == null)
            {
                currentSuit = card.suit;
                highestRank = GetCardRank(card);
            }
            else if (card.suit == currentSuit && GetCardRank(card) > highestRank)
            {
                highestRank = GetCardRank(card);
            }

            playedCardOwners[card] = currentPlayer; // Record the player who played the card
            playedCards.Add(card);

            //CheckAndClearCenter();

            SwitchTurn();
        }
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
        CardUI winningCard = null;
        int winningPlayer = -1;
        int highestCardRank = 0;

        // Step 2: If no card from the current suit is played, prioritize Spades
        if (winningCard == null)
        {
            foreach (var card in playedCards)
            {
                if (card.suit == "Spades")
                {
                    int cardRank = GetCardRank(card);

                    if (winningCard == null || cardRank > highestCardRank)
                    {
                        highestCardRank = cardRank;
                        winningCard = card;
                        winningPlayer = playedCardOwners[card];
                    }
                }
            }
        }

        // Step 3: If no Spades are played, pick the highest-ranking card regardless of suit
        if (winningCard == null)
        {
            foreach (var card in playedCards)
            {
                int cardRank = GetCardRank(card);

                if (winningCard == null || cardRank > highestCardRank)
                {
                    highestCardRank = cardRank;
                    winningCard = card;
                    winningPlayer = playedCardOwners[card];
                }
            }
        }

        Debug.Log("Round Winner: Player " + (winningPlayer + 1) + " with " + winningCard.suit + " " + winningCard.rank);

        // Update startPlayer with the winner of the round
        if (winningPlayer == 2)
        {
            startPlayer = 2; // Set Player 3 as the start player for the next round
            Debug.Log("Player 3 won and will start the next round.");
        }
        else
        {
            startPlayer = winningPlayer; // Set the winner as the start player
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
        TriggerPlayerTurn(currentPlayer);
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