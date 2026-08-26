using UnityEngine;
using UnityEngine.InputSystem; // 新Input Systemを使用
using UnityEngine.InputSystem.EnhancedTouch; // スマホタッチ用
using UnityEngine.SceneManagement; // ゲームオーバー時のリトライ用

public class PlayerController : MonoBehaviour
{
    // 左ジャンプ（横-4 / 縦8）: 「低く・広く」跳ぶ。
    // 👉 使い道: 横に離れた次の机に飛び移りたい時に便利。
    // 右ジャンプ（横2 / 縦11）: 「高く・狭く」跳ぶ。
    // 👉 使い道: すぐ真上（高め）にある机に一気に登りたい時に便利。
    [Header("ジャンプ力の設定")]
    [SerializeField] float leftJumpForceX = -4f;  // 左ジャンプの横移動
    [SerializeField] float leftJumpForceY = 8f;   // 左ジャンプの縦移動
    [SerializeField] float rightJumpForceX = 2f;  // 右ジャンプの横移動（少し控えめ）
    [SerializeField] float rightJumpForceY = 11f; // 右ジャンプの縦移動（高く飛ぶ）

    [Header("被弾パニック時の設定")]
    [SerializeField] float binnedKnockbackForceY = 10f; // 被弾時に上に弾き飛ばされる力（これで落下までの時間を稼ぐ）
    [SerializeField] float binnedTorque = 30f;          // 回転にかかるトルク（回転速度）
    [SerializeField] float recoverCountRequired = 5f;   // 復帰に必要な連打回数

    [Header("ゴール（社長室）の設定")]
    [SerializeField] float goalYPosition = 100f; // 何メートル（Y座標）で社長室に到達するか
     [SerializeField] GameObject clearUIPanel;  
    [SerializeField] BossTextSpawner bossTextSpawner;
    bool isCleared = false; // クリア済みフラグ

    Rigidbody2D rb;
    float screenWidth;
    Camera mainCamera;
    // プレイヤーの状態管理フラグ
    bool isStunned = false;
    float currentRecoverProgress = 0f;

    void OnEnable()
    {
        // 新Input Systemで高精度なタッチを有効化
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        // 無効化処理
        EnhancedTouchSupport.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        screenWidth = Screen.width;
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (isCleared) return;

        // ゴール判定（プレイヤーのY座標がゴールを超えたか？）
        if (transform.position.y >= goalYPosition)
        {
            GameClear();
            return;
        }

        if (isStunned)
        {
            // 【気絶中】連打入力を監視して復帰ゲージを溜める
            HandleRecoverInput();
            
            // 【画面外ゲームオーバー判定】カメラの画面下端より下に落ちたらリトライ
            CheckGameOver();
            return; // 気絶中は通常のジャンプ移動を受け付けない
        }

        // 1. PCテスト用（新Input System対応のキー入力判定）
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                Jump(leftJumpForceX, leftJumpForceY);
            }
            else if (Keyboard.current.dKey.wasPressedThisFrame)
            {
                Jump(rightJumpForceX, rightJumpForceY);
            }
        }

        // 2. スマホ・ブラウザのタッチ用（新Input System対応の画面タップ判定）
        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
        {
            var touch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0];

            // タップした瞬間のみ判定
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                if (touch.screenPosition.x < screenWidth / 2f)
                {
                    Jump(leftJumpForceX, leftJumpForceY); // 画面の左半分
                }
                else
                {
                    Jump(rightJumpForceX, rightJumpForceY); // 画面の右半分
                }
            }
        }
    }

    void Jump(float forceX, float forceY)
    {
        // 現在の速度をリセット（連打しても同じ高さに飛ぶため）
        rb.linearVelocity = Vector2.zero; // Unity 6仕様

        // 斜め方向に力を加える
        rb.AddForce(new Vector2(forceX, forceY), ForceMode2D.Impulse);
    }
    // 【最重要】文字障害物にぶつかった時に、障害物側のスクリプトから呼ばれる関数
    public void HitByObstacle()
    {
        // すでに気絶中（isStunned が true）なら、この関数の中身を即座に無視する！
        if (isStunned) return; 

        isStunned = true;
        currentRecoverProgress = 0f;

        // Z軸の回転固定（Constraints）をオフにする
        rb.constraints = RigidbodyConstraints2D.None;

        // 現在の速度を完全にリセット（これまでのジャンプの勢いを消す）
        rb.linearVelocity = Vector2.zero;

        // 真上にドカンと弾き飛ばして画面内の滞空時間を稼ぐ（1回だけ綺麗に発動）
        rb.AddForce(Vector2.up * binnedKnockbackForceY, ForceMode2D.Impulse);

        // トルクを加えて強制的にグルグル回転させる
        float spinDirection = Random.Range(0, 2) == 0 ? 1f : -1f;
        rb.AddTorque(spinDirection * binnedTorque, ForceMode2D.Impulse);
    }

    void HandleRecoverInput()
    {
        bool pressedThisFrame = false;

        // PC：AキーかDキーが押されたら連打としてカウント
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
            {
                pressedThisFrame = true;
            }
        }

        // スマホ：画面のどこでもタップされたら連打としてカウント
        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
        {
            var touch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0];
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                pressedThisFrame = true;
            }
        }

        if (pressedThisFrame)
        {
            currentRecoverProgress += 1f;

            if (currentRecoverProgress >= recoverCountRequired)
            {
                Recover();
            }
        }
    }

    void Recover()
    {
        isStunned = false;

        // Z軸の回転固定を元に戻す（直立状態に戻す）
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // プレイヤーの傾きをパッと0度（真っ直ぐ）にリセット
        transform.rotation = Quaternion.identity;
        rb.angularVelocity = 0f; // 回転の勢いを止める

        // 復帰した瞬間に少しだけ上にフワッと浮かせる（立て直しの猶予用、お好みで）
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(Vector2.up * 4f, ForceMode2D.Impulse);
    }

    void CheckGameOver()
    {
        if (mainCamera == null) return;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        // 画面の下端（y < 0）より完全に落ちたらゲームオーバー（現在のシーンをリロード）
        if (viewportPos.y < -0.05f)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void GameClear()
    {
        isCleared = true;
        
        if (bossTextSpawner != null)
        {
            bossTextSpawner.StopAndClearObstacles();
        }

        // プレイヤーの動きを完全に止める
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll; // 物理挙動を完全に固める
        
        if (clearUIPanel != null)
        {
            clearUIPanel.SetActive(true);
        }
    }

    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
