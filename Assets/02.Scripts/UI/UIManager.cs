using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject settingsPanelPrefab;
    [SerializeField] private GameObject confirmPanelPrefab;
    // 캔버스
    private Canvas _canvas;

    // 게임 화면의 UI 컨트롤러
    private GamePanelController _gamePanelController; 
    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        
    }

    public void SetGameTurn(Constants.PlayerColor playerTurnType)
    {
        // _gamePanelController.SetPlayerTurnPanel(playerTurnType);
    }
    public void OpenConfirmPanel(string msg /*ConfirmPanelController.OnConfirmButtonClicked onConfirmButtonClicked*/)
    {
        Debug.Log("OpenConfirmPanel");
    }
}