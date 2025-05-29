using System;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("장애물 이동 속도")]
    [SerializeField]
    private float moveSpeed = 5f;
    
    [Range(0, 10)]
    [SerializeField]
    private int attackDamage = 1;
    
    private const float DestroyX = -15f; // 장애물 제거 위치

    void Update()
    {
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        
        // 일정 거리 지나면 자동 제거
        if (transform.position.x < DestroyX)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out PlayerHealth playerHealth))
            {
                playerHealth.TakeDamage(attackDamage);
            }
            else
            {
                Debug.LogWarning("PlayerHealth 컴포넌트가 없습니다.");
            }
            
            gameObject.SetActive(false);
        }
    }

    public void OnGettingFromPool()
    {
        throw new System.NotImplementedException();
    }

    public void OnReturningToPool()
    {
        throw new System.NotImplementedException();
    }
}
