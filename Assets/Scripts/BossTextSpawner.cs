using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class BossTextSpawner : MonoBehaviour
{
    // インスペクターで色ごとのパラメータをきれいに管理するための構造体
    [System.Serializable]
    public struct TextTypeSettings
    {
        public string name;         // 管理用の名前（例：赤・高速）
        public Color32 color;       // 文字の色
        public float minSpeed;      // 最小速度
        public float maxSpeed;      // 最大速度
        public float minFontSize;   // 最小フォントサイズ
        public float maxFontSize;   // 最大フォントサイズ
    }

    [Header("生成するプレハブ")]
    [SerializeField] GameObject bossTextPrefab;

    [Header("生成バランス調整")]
    [SerializeField] float initialSpawnInterval = 5.0f; // スタート時の生成間隔（メートル）
    [SerializeField] float screenWidthBoundary = 3.0f;  // 左右の見えない壁の座標（画面端の基準）

    [Header("色ごとの性能カスタマイズ（黄・赤・紫の順に設定）")]
    [SerializeField] TextTypeSettings[] textTypeList = new TextTypeSettings[]
    {
        new TextTypeSettings { name = "黄（普通）", color = new Color32(255, 220, 0, 255), minSpeed = 2.0f, maxSpeed = 3.5f, minFontSize = 3.5f, maxFontSize = 4.5f },
        new TextTypeSettings { name = "赤（高速）", color = new Color32(255, 60, 60, 255),  minSpeed = 5.5f, maxSpeed = 8.0f, minFontSize = 3.0f, maxFontSize = 4.0f },
        new TextTypeSettings { name = "紫（巨大）", color = new Color32(180, 70, 255, 255), minSpeed = 1.0f, maxSpeed = 2.0f, minFontSize = 6.0f, maxFontSize = 8.0f }
    };

    [Header("厳選オフィス用語リスト（1行特化・視認性重視）")]
    [SerializeField] string[] officeWordsList = new string[]
    {
        "定時退社は都市伝説",
        "アグリーしてください",
        "PDCAが回っていない",
        "エビデンスはあるのか？",
        "持ち帰って検討します",
        "コミットメントが足りない",
        "電話鳴ってるよ！",
        "見積もりよろしく",
        "急な仕様変更です",
        "今日中にやっておいて",
        "進捗どうですか？",
        "至急バグ対応よろしく",
        "お客様クレーム対応！",
        "まだ終わらないの？",
        "21時から緊急会議ね",
    };

    Camera mainCamera;
    Transform playerTransform;
    float lastSpawnY;
    bool isStopSpawning = false;
    List<GameObject> activeObstaclesList = new List<GameObject>();

    void Start()
    {
        mainCamera = Camera.main;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            lastSpawnY = playerTransform.position.y;
        }
    }

    void Update()
    {
        if (playerTransform == null || isStopSpawning) return;

        if (playerTransform.position.y - lastSpawnY >= initialSpawnInterval)
        {
            SpawnBossText();
            lastSpawnY = playerTransform.position.y;
        }
    }

    void SpawnBossText()
    {
        if (bossTextPrefab == null || officeWordsList.Length == 0 || textTypeList.Length == 0 || mainCamera == null) return;

        Vector3 spawnTargetViewport = new Vector3(0.5f, 1.1f, 10f);
        Vector3 spawnWorldPos = mainCamera.ViewportToWorldPoint(spawnTargetViewport);

        GameObject spawnedObstacle = Instantiate(bossTextPrefab, new Vector3(0f, spawnWorldPos.y, 0f), Quaternion.identity);

        activeObstaclesList.Add(spawnedObstacle);

        BossTextObstacle obstacleScript = spawnedObstacle.GetComponent<BossTextObstacle>();
        if (obstacleScript != null)
        {
            string randomText = officeWordsList[Random.Range(0, officeWordsList.Length)];
            
            // 1. まず「どのタイプ（色・性能）」にするかをランダムに決定
            TextTypeSettings selectedType = textTypeList[Random.Range(0, textTypeList.Length)];

            var tmp = spawnedObstacle.GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                tmp.textWrappingMode = TextWrappingModes.NoWrap;
                
                // 選ばれたタイプの「色」を適用
                tmp.color = selectedType.color;

                // 選ばれたタイプの範囲内で「フォントサイズ」をランダム決定
                float randomFontSize = Random.Range(selectedType.minFontSize, selectedType.maxFontSize);
                tmp.fontSize = randomFontSize;
            }

            obstacleScript.SetText(randomText);
            obstacleScript.UpdateObstacleCollider(); 

            float halfWidth = spawnedObstacle.GetComponent<BoxCollider2D>().size.x / 2f;
            bool isLeftToRight = Random.Range(0, 2) == 0;
            
            float initialX = 0f;
            // 選ばれたタイプの範囲内で「スピード」をランダム決定
            float finalSpeed = Random.Range(selectedType.minSpeed, selectedType.maxSpeed);

            if (isLeftToRight)
            {
                initialX = -screenWidthBoundary - halfWidth;
                obstacleScript.SetMovement(finalSpeed); 
            }
            else
            {
                initialX = screenWidthBoundary + halfWidth;
                obstacleScript.SetMovement(-finalSpeed); 
            }

            spawnedObstacle.transform.position = new Vector3(initialX, spawnWorldPos.y, 0f);
        }
    }

    public void StopAndClearObstacles()
    {
        isStopSpawning = true; 

        foreach (GameObject obstacle in activeObstaclesList)
        {
            // すでに画面外判定等でDestroyされている可能性を考慮（Nullチェック）
            if (obstacle != null)
            {
                Destroy(obstacle);
            }
        }

        // リスト自体もきれいに空にする
        activeObstaclesList.Clear();
    }

    public void SetInitialSpawnInterval(float newInterval)
    {
        initialSpawnInterval = newInterval;
    }

    public void ResetSpawnerForNextLevel()
    {
        isStopSpawning = false;
        if (playerTransform != null)
        {
            lastSpawnY = playerTransform.position.y;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 spawnTargetViewport = new Vector3(0.5f, 1.1f, 10f);
        Vector3 centerPos = mainCamera.ViewportToWorldPoint(spawnTargetViewport);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-screenWidthBoundary, centerPos.y - 2f, 0), new Vector3(-screenWidthBoundary, centerPos.y + 2f, 0));
        Gizmos.DrawLine(new Vector3(screenWidthBoundary, centerPos.y - 2f, 0), new Vector3(screenWidthBoundary, centerPos.y + 2f, 0));
    }
}
