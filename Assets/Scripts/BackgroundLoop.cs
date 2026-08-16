using UnityEngine;

/*
    現在背景はParallax適応しているのでこのscriptは不要
    参考の為キープしている
*/
public class BackgroundLoop : MonoBehaviour
{
    Camera mainCamera; // privateを省略
    float backgroundHeight;

    void Start()
    {
        mainCamera = Camera.main;
        
        // スプライトの縦幅（サイズ）を自動取得
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            backgroundHeight = spriteRenderer.bounds.size.y;
        }
        else
        {
            Debug.LogError("SpriteRendererが見つかりません。");
        }
    }

    void Update()
    {
        // カメラが背景の真ん中をどれだけ追い抜いたかチェック
        // カメラのYが背景のY + 縦幅を超えたら、背景を上にワープさせる
        if (mainCamera.transform.position.y > transform.position.y + backgroundHeight)
        {
            // 背景2枚分の長さを上にシフトして再利用
            Vector3 offset = new Vector3(0, backgroundHeight * 2f, 0);
            transform.position += offset;
        }
        
        // 逆に落下したとき用の処理
        else if (mainCamera.transform.position.y < transform.position.y - backgroundHeight)
        {
            Vector3 offset = new Vector3(0, backgroundHeight * 2f, 0);
            transform.position -= offset;
        }
    }
}
