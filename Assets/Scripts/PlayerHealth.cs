using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("플레이어 체력")]
    [SerializeField]
    public int health = 5;

    public void TakeDamage(int damage)
    {
        health -= damage;
        health = Mathf.Max(0, health);

        Debug.Log("현재 체력: " + health);

        if (GameManager.instance != null && GameManager.instance.healthText != null)
        {
            GameManager.instance.healthText.text = "HP: " + health;
        }

        if (health <= 0)
        {
            GameManager.instance.GameOver();
        }
    }
}