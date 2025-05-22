using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("플레이어 체력")]
    [SerializeField]
    private int health = 5;

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("현재 체력: " + health);

        if (health <= 0)
        {
            // TODO: 플레이어 사망 처리
            GameManager.instance.GameOver();
        }
    }
}
