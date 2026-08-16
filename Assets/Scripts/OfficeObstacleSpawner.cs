using UnityEngine;

public class OfficeObstacleSpawner : MonoBehaviour
{
    [Header("設定アセット")]
    [SerializeField] GameObject officeObstaclePrefab; // 事務机のプレハブ

    // [SerializeField] を削除し、内部で自動取得するよう変更
    private Transform playerTransform;                // 社畜（プレイヤー）のTransform

    [Header("生成バランス調整")]
    [SerializeField] float generateAheadDistance = 15f; // プレイヤーの何マス先まで先行生成するか
    [SerializeField] float minX = -3f;               // 縦画面の左端制限
    [SerializeField] float maxX = 3f;                // 縦画面の右端制限

    [Header("Layout Optimization (Phase 17)")]
    [SerializeField] float minVerticalSpacing = 2.5f; // 操作しやすい最小縦間隔
    [SerializeField] float maxVerticalSpacing = 4.0f; // 操作しやすい最大縦間隔
    [SerializeField] float scaleMultiplierX = 1.5f;   // 机の横幅の拡大倍率
    [SerializeField] float scaleMultiplierY = 1.2f;   // 机の縦幅の拡大倍率

    float nextGenerateY = 0f; // 次に生成するY座標

    void Start()
    {
        // 🔍【自動取得】"Player" タグが付いたゲームオブジェクトを検索
        GameObject playerObj = GameObject.FindWithTag("Player");
        
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("OfficeObstacleSpawner: 'Player' タグが付いたオブジェクトが見つかりません！プレイヤーのTagをPlayerに設定してください。");
            return;
        }

        // 初期位置（プレイヤーの少し上）から先行して数個生成
        nextGenerateY = playerTransform.position.y + 2f;
        GeneratePlatforms();
    }

    void Update()
    {
        if (playerTransform == null) return;

        // プレイヤーが登るにつれて、先方に自動生成を継ぎ足す
        if (playerTransform.position.y + generateAheadDistance > nextGenerateY)
        {
            GeneratePlatforms();
        }

        // 🧹【メモリ最適化】画面外（下方向）に消えた机を自動で掃除する
        float destroyBoundaryY = playerTransform.position.y - generateAheadDistance;
        
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Desk");
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.transform.position.y < destroyBoundaryY)
            {
                Destroy(obstacle);
            }
        }
    }

    void GeneratePlatforms()
    {
        while (nextGenerateY < playerTransform.position.y + generateAheadDistance)
        {
            float randomX = Random.Range(minX, maxX);
            Vector3 spawnPosition = new Vector3(randomX, nextGenerateY, 0f);

            GameObject spawnedOffice = Instantiate(officeObstaclePrefab, spawnPosition, Quaternion.identity);

            // 📐机（OfficeObstacle）のサイズ拡大・調整
            Vector3 currentScale = spawnedOffice.transform.localScale;
            spawnedOffice.transform.localScale = new Vector3(
                currentScale.x * scaleMultiplierX, 
                currentScale.y * scaleMultiplierY, 
                currentScale.z
            );

            // 📐縦間隔の再設計
            nextGenerateY += Random.Range(minVerticalSpacing, maxVerticalSpacing);
        }
    }
}
