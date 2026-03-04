
using UnityEngine;

/// <summary>
/// 实际上是CardData的实例，作为没有还没有变成View要素的，卡片实体
/// </summary>
public class Card
{
    private readonly CardData data;
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int Cost { get; private set; }
    public Sprite Image { get; private set; }


    public Card(CardData cardData)
    {
        data = cardData;
        Title = cardData.Title;
        Description = cardData.Description;
        Cost = cardData.Cost;
        Image = cardData.Image;
    }
}
