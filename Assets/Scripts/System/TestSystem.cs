using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSystem : MonoBehaviour
{
    [SerializeField] private HandView handView;
    [SerializeField] private Transform cardSpawnPoint;
    [SerializeField] private CardData cardData;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Card card = new(cardData);
            CardView cardView = CardViewCreator.Instance.CreateCardView(card, cardSpawnPoint.position, cardSpawnPoint.rotation);
            StartCoroutine(handView.AddCardToHand(cardView));
        }
    }
}
