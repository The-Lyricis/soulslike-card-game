using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;

public class HandManager : MonoBehaviour
{
    [SerializeField] private int maxHandSize;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private SplineContainer handSplineContainer;
    [SerializeField] private Transform cardSpawnPoint;
    private List<GameObject> handCards = new();

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) DrawCard();
    }
    
    private void DrawCard()
    {
        if(handCards.Count >= maxHandSize)
        {
            Debug.Log("我的手牌满了!");
            return;
        }

        GameObject cardInstance = Instantiate(cardPrefab, cardSpawnPoint.position, cardSpawnPoint.rotation);
        handCards.Add(cardInstance);
        UpdateCardPosition();
    }

    private void UpdateCardPosition()
    {
        if(handCards.Count == 0) return;
        float cardSpacing = 1f/maxHandSize;
        float firstCardPosition = 0.5f - (handCards.Count-1)*cardSpacing / 2;
        Spline spline = handSplineContainer.Spline;

        for (int i = 0; i < handCards.Count; i++)
        {
            float position = firstCardPosition + i*cardSpacing;//线上位置
            Vector3 worldPosition = spline.EvaluatePosition(position);//世界坐标

            Vector3 forward = spline.EvaluateTangent(position);//前进方向
            Vector3 upward = spline.EvaluateUpVector(position);//垂直方向
            Quaternion rotation = Quaternion.LookRotation(upward,Vector3.Cross(upward,forward).normalized);

            handCards[i].transform.DOMove(worldPosition, 0.25f);//第一个参数是目标位置
            handCards[i].transform.DOLocalRotateQuaternion(rotation, 0.25f);
        }
    }

    // // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }
}
