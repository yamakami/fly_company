using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("ターゲット")]
    [SerializeField] private Transform cameraTransform; // メインカメラのTransform

    [Header("スクロール速度の比率（0 = カメラと同期, 1 = 完全固定）")]
    [SerializeField] private float parallaxEffectY = 0.5f; 

    private float lengthY;
    private float startY;

    void Start()
    {
        // ターゲットカメラが未設定ならメインカメラを自動取得
        if (cameraTransform == null) cameraTransform = Camera.main.transform;

        startY = transform.position.y;
        
        // スプライトの縦幅を取得してループ処理の基準にする
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            lengthY = spriteRenderer.bounds.size.y;
        }
    }

    void LateUpdate()
    {
        // カメラの移動量に応じた背景の移動位置を計算
        float distanceY = cameraTransform.position.y * parallaxEffectY;
        transform.position = new Vector3(transform.position.x, startY + distanceY, transform.position.z);

        // 背景が画面外に出たら位置をループさせて無限スクロール
        float tempY = cameraTransform.position.y * (1 - parallaxEffectY);
        if (tempY > startY + lengthY) 
        {
            startY += lengthY;
        }
        else if (tempY < startY - lengthY) 
        {
            startY -= lengthY;
        }
    }
}
