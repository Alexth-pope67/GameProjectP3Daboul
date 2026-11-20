using UnityEngine;

public class TaggingScript : MonoBehaviour
{
    private TextMesh taggerTitle;
    private GameObject iconObject;
    private SpriteRenderer taggerImageRenderer;
    private float baseHeadHeight;
    private bool visualsCreated;

    [Header("Overhead Sprite & Text Settings")]
    public Sprite taggerImage;
    public float heightAboveHead = 0.8f;
    public float iconScale = 1.0f;
    public bool bobUpAndDown = true;

    [Header("Tagging Settings")]
    public float autoTouchRadius = 1.3f;
    public float manualTagReach = 2.2f;
    public KeyCode tagKey = KeyCode.E;

    void Start()
    {
        CalculateHeadHeight();
        EnsureVisualsExist();
        UpdateTaggerVisuals();
    }

    private void CalculateHeadHeight()
    {
        Collider col = GetComponentInChildren<Collider>();
        if (col != null)
        {
            baseHeadHeight = (col.bounds.max.y - transform.position.y) + heightAboveHead;
        }
        else
        {
            baseHeadHeight = 2.2f + heightAboveHead;
        }
    }

    void Update()
    {
        if (TagGameManager.Instance == null || TagGameManager.Instance.RoundEnded)
        {
            return;
        }

        // Keep overhead sign synced in case GameManager initialized after Start
        if (visualsCreated && taggerImageRenderer != null)
        {
            bool shouldShow = (TagGameManager.Instance.CurrentTagger == gameObject);
            if (taggerImageRenderer.enabled != (shouldShow && taggerImage != null))
            {
                UpdateTaggerVisuals();
            }
        }

        CheckProximityAndManualTag();
    }

    private void CheckProximityAndManualTag()
    {
        GameObject currentTagger = TagGameManager.Instance.CurrentTagger;
        GameObject runner = TagGameManager.Instance.Runner;

        if (currentTagger != gameObject || runner == null) return;

        float distance = Vector3.Distance(transform.position, runner.transform.position);

        // 1. Auto-tag if players get right up against each other (works with any movement script)
        if (distance <= autoTouchRadius)
        {
            TagGameManager.Instance.EndRound(currentTagger, "Tagged!");
            return;
        }

        // 2. Manual tag with Tag Key within reach
        if (tagKey != KeyCode.None && distance <= manualTagReach && Input.GetKeyDown(tagKey))
        {
            TagGameManager.Instance.EndRound(currentTagger, "Tagged!");
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        TryTagObject(hit.collider.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        TryTagObject(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        TryTagObject(other.gameObject);
    }

    private void TryTagObject(GameObject hitObject)
    {
        if (TagGameManager.Instance == null || TagGameManager.Instance.RoundEnded || hitObject == null) return;

        // Check parent/root in case a child collider (like an arm or body mesh) was hit
        TaggingScript hitPlayer = hitObject.GetComponentInParent<TaggingScript>();
        GameObject hitPlayerObj = hitPlayer != null ? hitPlayer.gameObject : hitObject;

        GameObject currentTagger = TagGameManager.Instance.CurrentTagger;
        GameObject runner = TagGameManager.Instance.Runner;

        if ((currentTagger == gameObject && hitPlayerObj == runner) ||
            (runner == gameObject && hitPlayerObj == currentTagger))
        {
            TagGameManager.Instance.EndRound(currentTagger, "Tagged!");
        }
    }

    public void UpdateTaggerVisuals()
    {
        if (!visualsCreated)
        {
            CalculateHeadHeight();
            EnsureVisualsExist();
        }

        bool isTagger = TagGameManager.Instance != null && TagGameManager.Instance.CurrentTagger == gameObject;
        taggerTitle.color = Color.red;
        taggerTitle.text = isTagger ? "TAGGER" : string.Empty;
        taggerImageRenderer.sprite = taggerImage;
        taggerImageRenderer.enabled = isTagger && taggerImage != null;
    }

    public void ShowWinnerTitle(GameObject winner, string label)
    {
        if (!visualsCreated)
        {
            CalculateHeadHeight();
            EnsureVisualsExist();
        }

        bool isWinner = (winner == gameObject);
        taggerTitle.color = Color.green;
        taggerTitle.text = isWinner ? label : string.Empty;
        taggerImageRenderer.enabled = false;
    }

    private void EnsureVisualsExist()
    {
        if (visualsCreated) return;

        GameObject titleObject = new GameObject("TaggerTitle");
        titleObject.transform.SetParent(transform);
        titleObject.transform.localPosition = new Vector3(0f, baseHeadHeight, 0f);

        taggerTitle = titleObject.AddComponent<TextMesh>();
        taggerTitle.anchor = TextAnchor.MiddleCenter;
        taggerTitle.alignment = TextAlignment.Center;
        taggerTitle.fontSize = 32;
        taggerTitle.characterSize = 0.1f;

        iconObject = new GameObject("TaggerImage");
        iconObject.transform.SetParent(transform);
        iconObject.transform.localPosition = new Vector3(0f, baseHeadHeight + 0.7f, 0f);
        iconObject.transform.localScale = Vector3.one * iconScale;

        taggerImageRenderer = iconObject.AddComponent<SpriteRenderer>();
        taggerImageRenderer.sortingOrder = 100;

        visualsCreated = true;
    }

    void LateUpdate()
    {
        if (iconObject != null && taggerImageRenderer != null && taggerImageRenderer.enabled)
        {
            iconObject.transform.localScale = Vector3.one * iconScale;
            float bobOffset = bobUpAndDown ? Mathf.Sin(Time.time * 4f) * 0.1f : 0f;
            iconObject.transform.localPosition = new Vector3(0f, baseHeadHeight + 0.7f + bobOffset, 0f);
        }

        if (Camera.main == null) return;

        if (taggerTitle != null)
        {
            taggerTitle.transform.rotation = Camera.main.transform.rotation;
        }

        if (iconObject != null)
        {
            iconObject.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
