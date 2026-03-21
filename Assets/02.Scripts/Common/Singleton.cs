using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    T prefab = Resources.Load<T>("prefabs/" + typeof(T).Name);

                    if (prefab != null)
                    {
                        _instance = Instantiate(prefab).GetComponent<T>();
                    }
                    else
                    {
                        Debug.LogError($"{typeof(T).Name} 프리팹을 찾을 수 없습니다.");
                    }
                }
            }
            return _instance;
        }
    }

    virtual protected void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoad;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    protected abstract void OnSceneLoad(Scene scene, LoadSceneMode mode);

    protected virtual void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }
}
