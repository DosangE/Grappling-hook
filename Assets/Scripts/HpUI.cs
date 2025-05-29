using TMPro;
using UnityEngine;

public class HpUI : MonoBehaviour
{
    [Header("플레이어 체력")]
    [SerializeField]
    private PlayerHealth playerHealth;
    
    private TextMeshProUGUI _healthText;
    
    private void Awake()
    {
        _healthText = GetComponent<TextMeshProUGUI>();
        if (_healthText == null)
        {
            Debug.LogError("TextMeshProUGUI 컴포넌트를 찾을 수 없습니다. 이 스크립트는 TextMeshProUGUI 컴포넌트가 있는 GameObject에 추가되어야 합니다.");
        }
        
        playerHealth.OnHealthChanged = UpdateHealthText;
    }

    private void UpdateHealthText(int health)
    {
        if (_healthText == null)
        {
            return;
        }

        _healthText.text = "HP: " + health;
    }
}