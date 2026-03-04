using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//在Unity中显示
[CreateAssetMenu(menuName = "Data/Card")]
public class CardData : ScriptableObject
{
    //作为Property，实际上是方法接口，只是看着像Field，于是大写开头！
    [field: SerializeField] public string Title {get; private set;}
    [field: SerializeField] public string Description {get; private set;}
    [field: SerializeField] public int Cost {get; private set;}
    [field: SerializeField] public Sprite Image {get; private set;}
    [field: SerializeField] public List<CardLabel> Labels {get; private set;}
}
