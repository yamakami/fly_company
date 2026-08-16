using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    [Header("設定アセット")]
    [SerializeField] GameObject platformPrefab; // 事務机のプレハブ
    [SerializeField] Transform playerTransform;  // 社畜（プレイヤー）のTransform

    [Header("生成バランス調整")]
    [SerializeField] float generateAheadDistance = 15f; // プレイヤーの何マス先まで先行生成するか
    [SerializeField] float platformIntervalY = 2.5f;   // 事務机の縦の間隔（ジャンプ力に合わせる）
    [SerializeField] float minX = -3f;               // 縦画面の左端制限
    [SerializeField] float maxX = 3f;                // 縦画面の右端制限

    float nextGenerateY = 0f; // 次に生成するY座標

    void Start()
    {
        // 初期位置（プレイヤーの少し上）から先行して数個生成
        nextGenerateY = playerTransform.position.y + 2f;
        GeneratePlatforms();
    }

    void Update()
    {
        // プレイヤーが登るにつれて、先方に自動生成を継ぎ足す
        if (playerTransform.position.y + generateAheadDistance > nextGenerateY)
        {
            GeneratePlatforms();
        }

        // 🧹【追加ロジック】画面外（下方向）に消えた机を自動で掃除する
        // プレイヤーの15マス（1.5大四角）より下に落ちた「Platform」タグのオブジェクトを検索して削除
        float destroyBoundaryY = playerTransform.position.y - generateAheadDistance;
        
        // 効率化のため、一定時間（例: 0.5秒）ごとに処理するか、一括タグ検索を行います
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");
        foreach (GameObject platform in platforms)
        {
            if (platform.transform.position.y < destroyBoundaryY)
            {
                Destroy(platform); // メモリと物理計算から完全に消去！
            }
        }
    }

    void GeneratePlatforms()
    {
        // プレイヤーの視界外先方までループ生成
        while (nextGenerateY < playerTransform.position.y + generateAheadDistance)
        {
            // 縦画面に収まる範囲でランダムにX座標を決定
            float randomX = Random.Range(minX, maxX);
            Vector3 spawnPosition = new Vector3(randomX, nextGenerateY, 0f);

            // インスタンス化
            Instantiate(platformPrefab, spawnPosition, Quaternion.identity);

            // 次の床の高さへ進める
            nextGenerateY += platformIntervalY;
        }
    }
}
