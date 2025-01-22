using UnityEngine;

public class CardClickHandler : MonoBehaviour
{
    private DeckManager deckManager;
    private CardUI card;

    public void Initialize(DeckManager manager, CardUI cardUI)
    {
        deckManager = manager;
        card = cardUI;
    }

    void OnMouseDown()
    {
        //deckManager.PlayCard(card);
    }
}
