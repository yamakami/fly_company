using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    [Header("追従ターゲット（社畜）")]
    [SerializeField] Transform playerTransform;

    [Header("設定")]
    [SerializeField] float smoothTime = 0.2f;

    float yOffset; // Start時に自動取得
    float currentVelocityY;
    float highestCameraY;

    void Start()
    {
        if (playerTransform != null)
        {
            // 開始時のカメラのY座標と、社畜のY座標の「差」を自動でオフセットにする
            yOffset = transform.position.y - playerTransform.position.y;
        }

        // ゲーム開始時のカメラ位置（0）を最初の最高到達点にする
        highestCameraY = transform.position.y;
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // 社畜の現在位置に、自動取得したオフセットを足して理想のカメラY座標を計算
        float targetY = playerTransform.position.y + yOffset;

        // カメラは上方向にしか動かない（最高到達点を更新していく）
        if (targetY > highestCameraY)
        {
            highestCameraY = targetY;
        }

        // SmoothDampで、ジャンプの勢いに合わせて滑らかに追従
        float newY = Mathf.SmoothDamp(transform.position.y, highestCameraY, ref currentVelocityY, smoothTime);
        
        // カメラの位置を更新（XとZは固定）
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
