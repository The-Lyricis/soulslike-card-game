using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [SerializeField] private CardView cardViewHover;
    [SerializeField] private float scaleDuration = 0.2f;
    [SerializeField] private float hoverScale = 1.5f;

    private Vector3 originalScale;
    private Tween currentTween;

    protected override void Awake()
    {
        base.Awake();
        originalScale = cardViewHover.transform.localScale;
    }

    public void Show (Card card, Vector3 startPosition, Vector3 targetPosition)
    {
        // 取消之前的动画
        currentTween?.Kill();

        cardViewHover.gameObject.SetActive(true);
        cardViewHover.Setup(card);

        // 从起始位置和原始大小开始
        cardViewHover.transform.position = startPosition;
        cardViewHover.transform.localScale = originalScale;

        // 同时执行移动和缩放动画
        Sequence hoverSequence = DOTween.Sequence();
        hoverSequence.Join(cardViewHover.transform.DOMove(targetPosition, scaleDuration).SetEase(Ease.OutQuad));
        hoverSequence.Join(cardViewHover.transform.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutQuad));

        currentTween = hoverSequence;
    }

    public void Hide()
    {
        // 取消之前的动画
        currentTween?.Kill();

        // 直接隐藏，没有动画
        cardViewHover.gameObject.SetActive(false);
    }
}
