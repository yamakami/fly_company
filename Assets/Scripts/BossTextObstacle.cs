using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
[RequireComponent(typeof(BoxCollider2D))]
public class BossTextObstacle : MonoBehaviour
{
    TextMeshPro tmpText;
    BoxCollider2D boxCollider;
    RectTransform rectTransform;
    
    Transform playerTransform;
    Camera mainCamera;

    [Header("コライダー調整オプション")]
    [SerializeField] Vector2 padding = new Vector2(0.2f, 0.2f);

    float moveSpeed = 0f;

    void Awake()
    {
        CacheComponents();
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
        mainCamera = Camera.main;

        SetText(tmpText.text);
    }

    void Update()
    {
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        CheckOutOfBounds();
    }

    public void SetMovement(float speed)
    {
        moveSpeed = speed;
    }

    void CheckOutOfBounds()
    {
        if (mainCamera == null) return;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        if (viewportPos.y < -0.1f || viewportPos.x < -0.5f || viewportPos.x > 1.5f)
        {
            Destroy(gameObject);
        }
    }

    // 追加：プレイヤーとの接触検知（文字のコライダーのIs TriggerがONの場合）
    void OnTriggerEnter2D(Collider2D collision)
    {
        // ぶつかった相手のタグが "Player" だったら
        if (collision.CompareTag("Player"))
        {
            PlayerController playerCtrl = collision.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                // プレイヤーの気絶・回転・吹き飛ばしメソッドを実行！
                playerCtrl.HitByObstacle();
            }
        }
    }

    void OnValidate()
    {
        CacheComponents();
        if (!Application.isPlaying && tmpText != null)
        {
            UpdateObstacleCollider();
        }
    }

    void CacheComponents()
    {
        if (tmpText == null) tmpText = GetComponent<TextMeshPro>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        if (tmpText != null)
        {
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.textWrappingMode = TextWrappingModes.NoWrap;
            tmpText.enableAutoSizing = false;
        }
    }

    public void SetText(string newText)
    {
        CacheComponents();
        tmpText.text = newText;
        UpdateObstacleCollider();
    }

    [ContextMenu("Update Collider")]
    public void UpdateObstacleCollider()
    {
        if (tmpText == null || boxCollider == null || rectTransform == null) return;

        rectTransform.sizeDelta = new Vector2(50f, 20f); 
        tmpText.ForceMeshUpdate();

        Vector2 renderedSize = tmpText.textBounds.size;

        boxCollider.size = new Vector2(renderedSize.x + padding.x, renderedSize.y + padding.y);
        boxCollider.offset = tmpText.textBounds.center;
    }
}
