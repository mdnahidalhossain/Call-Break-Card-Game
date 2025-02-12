using UnityEngine;

public class CardUI : MonoBehaviour
{
    public string suit; // e.g., "Hearts", "Spades"
    public int rank;    // 1 for Ace, 11 for Jack, 12 for Queen, 13 for King
    public Sprite cardSprite; // Sprite for the card face

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = cardSprite;
    }

    // Method to initialize the card
    public void InitializeCard(string cardSuit, int cardRank, Sprite sprite)
    {
        suit = cardSuit;
        rank = cardRank;
        cardSprite = sprite;
    }
}
