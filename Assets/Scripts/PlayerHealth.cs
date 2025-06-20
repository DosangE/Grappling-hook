using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("플레이어 체력")]
    [SerializeField]
    public int health = 5;

    [Header("무적시간 (초)")]
    [SerializeField] private float invincibleTime = 0.5f; // 깜빡임 애니메이션 길이에 맞춤

    private Animator animator;
    private bool isInvincible = false;
    public Action<int> OnHealthChanged;

    private void Start()
    {
        animator = GetComponent<Animator>();
        OnHealthChanged?.Invoke(health);
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return; 
        
        health -= damage;
        health = Mathf.Max(0, health);

        if (GameManager.instance != null && GameManager.instance.healthText != null)
        {
            GameManager.instance.healthText.text = "HP: " + health;
        }
        animator.SetTrigger("Hit");
        StartCoroutine(InvincibleRoutine());
        if (health <= 0)
        {
            GameManager.instance.GameOver();
        }

        OnHealthChanged?.Invoke(health);
    }
    private System.Collections.IEnumerator InvincibleRoutine()
    {
        isInvincible = true;

        yield return new WaitForSeconds(invincibleTime); // 애니메이션 길이와 맞춤

        isInvincible = false;
    }
}