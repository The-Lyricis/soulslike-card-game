using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Content;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text cost;
    [SerializeField] private SpriteRenderer imageSR;
    [SerializeField] private GameObject wrapper;

    public Card Card { get; private set; }

    public void Setup(Card card)
    {
        Card = card;
        title.text = card.Title;
        description.text = card.Description;
        cost.text = card.Cost.ToString();
        imageSR.sprite = card.Image;
    }

    void OnMouseEnter()
    {
        wrapper.SetActive(false);//将原卡牌图像禁用

        // 起始位置是当前卡牌位置，目标位置是悬停显示位置
        Vector3 startPos = transform.position;
        Vector3 targetPos = new(transform.position.x-0.5f, -2.9f, 0);

        CardViewHoverSystem.Instance.Show(Card, startPos, targetPos);

        // 尝试查找父级的 HandView，如果在手牌中则触发展开效果
        HandView handView = GetComponentInParent<HandView>();
        if (handView != null)
        {
            int cardIndex = handView.GetCardIndex(this);
            if (cardIndex >= 0)
            {
                handView.SpreadCards(cardIndex);
            }
        }
    }

    void OnMouseExit()
    {
        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);

        // 尝试查找父级的 HandView，如果在手牌中则恢复位置
        HandView handView = GetComponentInParent<HandView>();
        if (handView != null)
        {
            handView.ResetCardSpread();
        }
    }
}
