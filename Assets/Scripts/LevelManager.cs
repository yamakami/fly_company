using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public struct LevelSettings
    {
        public string levelName;        
        public Sprite backgroundSprite; 
        public float goalYPosition;     
        public float spawnInterval;     
    }

    [Header("全ステージの設定（要素数を3にします）")]
    [SerializeField] LevelSettings[] levels;

    [Header("連動するコンポーネント")]
    [SerializeField] SpriteRenderer[] backgroundRenderers; 
    [SerializeField] PlayerController playerController;
    [SerializeField] BossTextSpawner bossTextSpawner;

    // 🛑 改善：静的変数（static）にすることで、SceneManager.LoadSceneをしてもステージ数がリセットされなくなります！
    private static int currentLevelIndex = 0; 

    void Start()
    {
        // シーンが読み込まれたら、記憶されているステージの設定を即座に適用する
        ApplyLevelSettings();
    }

    public void ApplyLevelSettings()
    {
        if (levels == null || levels.Length == 0 || currentLevelIndex >= levels.Length) return;

        LevelSettings currentLevel = levels[currentLevelIndex];
        Debug.Log($"🏢 【{currentLevel.levelName}】が始まりました！");

        if (backgroundRenderers != null && currentLevel.backgroundSprite != null)
        {
            foreach (SpriteRenderer renderer in backgroundRenderers)
            {
                if (renderer != null)
                {
                    renderer.sprite = currentLevel.backgroundSprite;
                }
            }
        }

        if (playerController != null)
        {
            playerController.SetGoalYPosition(currentLevel.goalYPosition);
        }

        if (bossTextSpawner != null)
        {
            bossTextSpawner.SetInitialSpawnInterval(currentLevel.spawnInterval);
        }
    }

    // 次のステージへ進むときも、シーンごとリロードして初期化をUnityに任せます！
    public void AdvanceToNextLevel()
    {
        currentLevelIndex++;

        // もし全3ステージをすべてクリアしていたら最初のステージに戻す
        if (currentLevelIndex >= levels.Length)
        {
            Debug.Log("🎉 祝・完全退職！全面クリア画面へ（今後UIを作成）");
            currentLevelIndex = 0;
        }

        if (bossTextSpawner != null)
        {
            bossTextSpawner.ResetSpawnerForNextLevel();
        }

        // ポジションリセット用の複雑な処理をすべて廃止し、シーンリロードで安全に初期化
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public bool IsMaxLevel()
    {
        return currentLevelIndex >= levels.Length - 1;
    }
}
