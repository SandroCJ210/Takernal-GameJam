using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Takernal/Data/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "Chaser Enemy";
    public float maxHealth = 30f;
    public float moveSpeed = 3f;
    public float damage = 10f;
    public Sprite sprite;
    public Color debugColor = Color.red;
}
