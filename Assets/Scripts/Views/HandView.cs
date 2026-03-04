using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;

public class HandView : MonoBehaviour
{
    [SerializeField] private int maxHandSize;
    [SerializeField] private float moveDuration;
    [SerializeField] private float leftSpreadOffset = 0.05f;  // 左侧卡牌的展开偏移
    [SerializeField] private float rightSpreadOffset = 0.05f; // 右侧卡牌的展开偏移
    [SerializeField] private float minCardSpacing = 0.03f; // 卡牌之间的最小间距，优先级高于spreadOffset
    [SerializeField] private float edgeMargin = 0.02f; // 边缘保护距离，防止到达0或1时的角度突变

    [SerializeField] private SplineContainer handSplineContainer;
    private List<CardView> handCards = new();
    private int hoveredCardIndex = -1;

    public IEnumerator AddCardToHand(CardView cardView)
    {
        if(handCards.Count >= maxHandSize)
        {
            Debug.Log("我的手牌满了!");
            // 在销毁前先杀死所有DOTween动画，防止访问已销毁的Transform
            cardView.transform.DOKill();
            Destroy(cardView.gameObject);
            yield break;
        }

        handCards.Add(cardView);
        // 将卡牌设置为HandView的子物体，以便GetComponentInParent能找到
        cardView.transform.SetParent(transform);
        yield return UpdateCardPositions(moveDuration);
    }

    public int GetCardIndex(CardView cardView)
    {
        return handCards.IndexOf(cardView);
    }

    public void SpreadCards(int hoveredIndex)
    {
        if (hoveredIndex < 0 || hoveredIndex >= handCards.Count) return;

        hoveredCardIndex = hoveredIndex;
        UpdateCardPositionsWithSpread(moveDuration * 0.5f);
    }

    public void ResetCardSpread()
    {
        hoveredCardIndex = -1;
        UpdateCardPositionsWithSpread(moveDuration * 0.5f);
    }

    private IEnumerator UpdateCardPositions(float duration)
    {
        if(handCards.Count == 0) yield break;

        float cardSpacing = 1f/maxHandSize;
        float firstCardPosition = 0.5f - (handCards.Count-1)*cardSpacing / 2;

        // 应用边缘保护
        firstCardPosition = Mathf.Max(firstCardPosition, edgeMargin);
        float lastCardPosition = firstCardPosition + (handCards.Count-1)*cardSpacing;
        lastCardPosition = Mathf.Min(lastCardPosition, 1f - edgeMargin);

        // 如果需要，重新调整间隔以适应边缘保护
        if (handCards.Count > 1)
        {
            cardSpacing = (lastCardPosition - firstCardPosition) / (handCards.Count - 1);
        }

        Spline spline = handSplineContainer.Spline;

        for (int i = 0; i < handCards.Count; i++)
        {
            float position = firstCardPosition + i*cardSpacing;//线上位置

            // 最终安全检查
            position = Mathf.Clamp(position, edgeMargin, 1f - edgeMargin);

            Vector3 worldPosition = spline.EvaluatePosition(position);//世界坐标

            Vector3 forward = spline.EvaluateTangent(position);//前进方向
            Vector3 upward = spline.EvaluateUpVector(position);//垂直方向
            Quaternion rotation = Quaternion.LookRotation(upward,Vector3.Cross(upward,forward).normalized);

            // handCards[i].transform.DOMove(worldPosition + transform.position + 0.01f * i * Vector3.back, duration);
            // handCards[i].transform.DORotate(rotation.eulerAngles, duration);
            handCards[i].transform.DOMove(worldPosition, duration);//第一个参数是目标位置
            handCards[i].transform.DOLocalRotateQuaternion(rotation, duration);
        }
        yield return new WaitForSeconds(duration);
    }

    private void UpdateCardPositionsWithSpread(float duration)
    {
        if(handCards.Count == 0) return;

        Spline spline = handSplineContainer.Spline;

        // 计算基础布局参数
        float cardSpacing = 1f/maxHandSize;
        float firstCardPosition = 0.5f - (handCards.Count-1)*cardSpacing / 2;

        // 应用边缘保护
        firstCardPosition = Mathf.Max(firstCardPosition, edgeMargin);
        float lastCardPosition = firstCardPosition + (handCards.Count-1)*cardSpacing;
        lastCardPosition = Mathf.Min(lastCardPosition, 1f - edgeMargin);

        // 如果需要，重新调整间隔以适应边缘保护
        if (handCards.Count > 1)
        {
            cardSpacing = (lastCardPosition - firstCardPosition) / (handCards.Count - 1);
        }

        for (int i = 0; i < handCards.Count; i++)
        {
            float position;

            // 如果有卡牌被悬停，使用动态间隔
            if (hoveredCardIndex >= 0 && hoveredCardIndex < handCards.Count)
            {
                float hoverPosition = firstCardPosition + hoveredCardIndex * cardSpacing;
                int leftCount = hoveredCardIndex;
                int rightCount = handCards.Count - hoveredCardIndex - 1;

                if (i < hoveredCardIndex)
                {
                    // 左侧卡牌：重新分配到左侧区间
                    if (leftCount > 0)
                    {
                        // 计算左侧可用空间，使用独立的 leftSpreadOffset
                        float actualLeftOffset = leftSpreadOffset;

                        // 确保最末端的卡至少移动 leftCount * minSpacing 的距离
                        // 左侧最末端的卡原本在 hoverPosition - cardSpacing
                        // Spread 后在 hoverPosition - actualLeftOffset
                        // 移动距离应该至少是 leftCount * minSpacing
                        float minMovement = leftCount * minCardSpacing;
                        float originalEndPosition = hoverPosition - cardSpacing;
                        float targetEndPosition = hoverPosition - actualLeftOffset;
                        float currentMovement = originalEndPosition - targetEndPosition;

                        if (currentMovement < minMovement)
                        {
                            // 需要增加 offset 以确保最小移动距离
                            actualLeftOffset = cardSpacing + minMovement;
                        }

                        // 左侧起点：如果距离边缘还有空间，向左扩展
                        float leftStart = Mathf.Max(edgeMargin, firstCardPosition - leftCount * minCardSpacing);
                        float leftEnd = hoverPosition - actualLeftOffset;
                        float leftSpace = leftEnd - leftStart;

                        // 检查是否满足 minCardSpacing
                        float requiredSpace = (leftCount - 1) * minCardSpacing;
                        bool useCornerMode = false; // 标记是否使用角落模式

                        if (leftSpace < requiredSpace)
                        {
                            // 空间不足，检查左侧起点是否还能向左移动
                            if (leftStart > edgeMargin)
                            {
                                // 还有空间，将起点再向左移动一个 offset 的距离
                                leftStart = Mathf.Max(edgeMargin, leftStart - actualLeftOffset);
                                leftSpace = leftEnd - leftStart;
                                useCornerMode = true; // 启用角落模式
                            }

                            // 如果还是不够，调整 offset
                            if (leftSpace < requiredSpace)
                            {
                                actualLeftOffset = Mathf.Max(0, hoverPosition - leftStart - requiredSpace);
                                leftEnd = hoverPosition - actualLeftOffset;
                            }
                        }

                        // 根据模式分配位置
                        if (leftCount > 1)
                        {
                            if (useCornerMode)
                            {
                                // 角落模式：保留 0.6 * offset 的空间给紧邻 hover 的卡牌，其余平分
                                float reservedSpace = 0.5f * actualLeftOffset;
                                float adjustedLeftEnd = leftEnd - reservedSpace;
                                float leftSpacing = (adjustedLeftEnd - leftStart) / (leftCount - 1);
                                position = leftStart + i * leftSpacing;
                            }
                            else
                            {
                                // 正常模式：平均分配空间
                                float leftSpacing = (leftEnd - leftStart) / (leftCount - 1);
                                position = leftStart + i * leftSpacing;
                            }
                        }
                        else
                        {
                            position = leftEnd;
                        }
                    }
                    else
                    {
                        position = hoverPosition;
                    }
                }
                else if (i > hoveredCardIndex)
                {
                    // 右侧卡牌：重新分配到右侧区间
                    if (rightCount > 0)
                    {
                        // 计算右侧可用空间，使用独立的 rightSpreadOffset
                        float actualRightOffset = rightSpreadOffset;

                        // 确保最末端的卡至少移动 rightCount * minSpacing 的距离
                        // 右侧最开始的卡原本在 hoverPosition + cardSpacing
                        // Spread 后在 hoverPosition + actualRightOffset
                        // 移动距离应该至少是 rightCount * minSpacing
                        float minMovement = rightCount * minCardSpacing;
                        float originalStartPosition = hoverPosition + cardSpacing;
                        float targetStartPosition = hoverPosition + actualRightOffset;
                        float currentMovement = targetStartPosition - originalStartPosition;

                        if (currentMovement < minMovement)
                        {
                            // 需要增加 offset 以确保最小移动距离
                            actualRightOffset = cardSpacing + minMovement;
                        }

                        float rightStart = hoverPosition + actualRightOffset;
                        // 右侧终点：如果距离边缘还有空间，向右扩展
                        float rightEnd = Mathf.Min(1f - edgeMargin, lastCardPosition + rightCount * minCardSpacing);
                        float rightSpace = rightEnd - rightStart;

                        // 检查是否满足 minCardSpacing
                        float requiredSpace = (rightCount - 1) * minCardSpacing;
                        bool useCornerMode = false; // 标记是否使用角落模式

                        if (rightSpace < requiredSpace)
                        {
                            // 空间不足，检查右侧终点是否还能向右移动
                            if (rightEnd < 1f - edgeMargin)
                            {
                                // 还有空间，将终点再向右移动一个 offset 的距离
                                rightEnd = Mathf.Min(1f - edgeMargin, rightEnd + actualRightOffset);
                                rightSpace = rightEnd - rightStart;
                                useCornerMode = true; // 启用角落模式
                            }

                            // 如果还是不够，调整 offset
                            if (rightSpace < requiredSpace)
                            {
                                actualRightOffset = Mathf.Max(0, rightEnd - hoverPosition - requiredSpace);
                                rightStart = hoverPosition + actualRightOffset;
                            }
                        }

                        // 根据模式分配位置
                        if (rightCount > 1)
                        {
                            if (useCornerMode)
                            {
                                // 角落模式：保留 0.6 * offset 的空间给紧邻 hover 的卡牌，其余平分
                                float reservedSpace = 0.5f * actualRightOffset;
                                float adjustedRightStart = rightStart + reservedSpace;
                                float rightSpacing = (rightEnd - adjustedRightStart) / (rightCount - 1);
                                position = adjustedRightStart + (i - hoveredCardIndex - 1) * rightSpacing;
                            }
                            else
                            {
                                // 正常模式：平均分配空间
                                float rightSpacing = (rightEnd - rightStart) / (rightCount - 1);
                                position = rightStart + (i - hoveredCardIndex - 1) * rightSpacing;
                            }
                        }
                        else
                        {
                            position = rightStart;
                        }
                    }
                    else
                    {
                        position = hoverPosition;
                    }
                }
                else
                {
                    // 悬停卡牌保持原位置
                    position = hoverPosition;
                }
            }
            else
            {
                // 没有悬停时的正常布局
                position = firstCardPosition + i * cardSpacing;
            }

            // 最终安全检查，确保在有效范围内
            position = Mathf.Clamp(position, edgeMargin, 1f - edgeMargin);

            Vector3 worldPosition = spline.EvaluatePosition(position);
            Vector3 forward = spline.EvaluateTangent(position);
            Vector3 upward = spline.EvaluateUpVector(position);
            Quaternion rotation = Quaternion.LookRotation(upward,Vector3.Cross(upward,forward).normalized);

            handCards[i].transform.DOMove(worldPosition, duration);
            handCards[i].transform.DOLocalRotateQuaternion(rotation, duration);
        }
    }

}
