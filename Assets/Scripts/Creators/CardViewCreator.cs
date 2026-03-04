using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class CardViewCreator : Singleton<CardViewCreator>
{
    [SerializeField] private float scaleDuration;
    [SerializeField] private CardView cardViewPrefab;

    public CardView CreateCardView(Card card, Vector3 position, Quaternion rotation)
    {
        CardView cardViewInstance = Instantiate(cardViewPrefab, position, rotation);
        cardViewInstance.transform.localScale = Vector3.zero;
        cardViewInstance.transform.DOScale(Vector3.one, scaleDuration);

        cardViewInstance.Setup(card);
        
        return cardViewInstance;
    }

}
