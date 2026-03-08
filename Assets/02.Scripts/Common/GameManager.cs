using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] StageData _stageData;
    public StageData StageData => _stageData;
    
    [SerializeField]
    private GameObject _moveablePrefab;
    public GameObject MoveablePrefab => _moveablePrefab;
    [SerializeField] private Vector3 _moveableScale;
    public Vector3 MoveableScale => _moveableScale;
    
    private GameLogic _gameLogic;
    public GameLogic GameLogic =>  _gameLogic;
    
    private Constants.PlayerType _gamePlayerType;
    public Constants.PlayerType GamePlayerType  => _gamePlayerType;
    
    private Constants.GameType _gameType;
    
    
    
    private BoardController _boardController;
    public BoardController BoardController => _boardController;
    
    private MoveChecker _moveChecker;
    public MoveChecker MoveChecker => _moveChecker;

    // temp
    void Awake()
    {
        InitGameScene();
    }
    
    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoad");
        
        // 게임씬일 때
        if (scene.name == "03.Game")
        {
            _gameLogic?.Dispose();
            InitGameScene();
        }
    }

    private void InitGameScene()
    {
        // TODO: 실제 gameType, stageData 가져오기
        _gameType = Constants.GameType.LocalDualPlay;
        
        _boardController = FindFirstObjectByType<BoardController>();
        if (_boardController == null)
            Debug.LogError("_boardController == null");
        _moveChecker = new MoveChecker();
        NewGameLogic();
        _boardController.LoadStage(_stageData);
    }

    private void NewGameLogic()
    {
        // 게임로직에 게임타입, 보드컨트롤러 전달
        _gameLogic = new GameLogic(_gameType, _boardController);
        Debug.Log("<color=yellow>GameLogic 생성됨</color>");
    }

    // Game scene 으로 전환
    public void ChangeToGameScene()
    {
        SceneManager.LoadScene("03.Game");
    }

    // Main scene 으로 전환
    public void ChangeToMainScene()
    {
        SceneManager.LoadScene("02.Main");
    }

    // SplashScreen scene 으로 전환
    public void ChangeToSplashScene()
    {
        SceneManager.LoadScene("01.SplashScreen");
    }
}
