using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("로비 UI")]
    [SerializeField]
    private GameObject lobbyUI;
    
    [Header("Setting UI")]
    [SerializeField]
    private GameObject settingUI;
    
    [Header("Setting Button")]
    [SerializeField]
    private Button settingButton;
    
    [Header("Return Button")]
    [SerializeField]
    private Button returnButton;
    
    private void Awake()
    {
        if (lobbyUI == null || settingUI == null || settingButton == null)
        {
            Debug.LogError("로비 UI 또는 설정 UI가 할당되지 않았습니다. UI를 설정해주세요.");
            return;
        }
        
        // Setting 버튼 클릭 이벤트 등록
        settingButton.onClick.AddListener(OnClickSettingButton);
        // Return 버튼 클릭 이벤트 등록
        returnButton.onClick.AddListener(OnClickReturnButton);
        
        // 초기 로비 UI 활성화
        lobbyUI.SetActive(true);
        settingUI.SetActive(false);
    }

    private void OnClickSettingButton()
    {
        // 로비 UI 비활성화, 설정 UI 활성화
        lobbyUI.SetActive(false);
        settingUI.SetActive(true);
        
        // Return 버튼 활성화
        returnButton.gameObject.SetActive(true);
        settingButton.gameObject.SetActive(false);
    }

    private void OnClickReturnButton()
    {
        // 설정 UI 비활성화, 로비 UI 활성화
        settingUI.SetActive(false);
        lobbyUI.SetActive(true);
        
        // Return 버튼 비활성화, Setting 버튼 활성화
        returnButton.gameObject.SetActive(false);
        settingButton.gameObject.SetActive(true);
    }
}