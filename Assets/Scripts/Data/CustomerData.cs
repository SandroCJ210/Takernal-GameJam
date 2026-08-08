using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCustomer", menuName = "Takernal/Data/CustomerData")]
public class CustomerData : ScriptableObject
{
    public string customerName;
    public Sprite avatar;
    public List<string> likedTags = new List<string>();
    public List<string> dislikedTags = new List<string>();
    public float maxPatience = 30f;
    public RewardData reward;
}
