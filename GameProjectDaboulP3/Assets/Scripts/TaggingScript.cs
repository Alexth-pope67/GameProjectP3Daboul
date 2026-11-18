using UnityEngine;

public class TaggingScript : MonoBehaviour
{
    private TextMesh taggerTitle;
    private GameObject iconObject;
    private SpriteRenderer taggerImageRenderer;
    private float baseHeadHeight;

    [Header("Overhead Sprite & Text Settings")]
    public Sprite taggerImage;
    public float heightAboveHead = 0.8f;
    public float iconScale = 1.0f;
    public bool bobUpAndDown = true;

    [Header("Tagging Settings")]
    public float tagReach = 2.0f;
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
            baseHeadHeight = 2.5f + heightAboveHead;
        }
    }

    void Update()
    {
        if (TagGameManager.Instance == null || TagGameManager.Instance.RoundEnded)
        {
            return;
        }

        CheckManualTag();
    }

    private void CheckManualTag()
    {
        GameObject currentTagger = TagGameManager.Instance.CurrentTagger;
        GameObject runner = TagGameManager.Instance.Runner;

        if (currentTagger != gameObject || runner == null) return;

        float distance = Vector3.Distance(transform.position, runner.transform.position);
        if (distance <= tagReach && Input.GetKeyDown(tagKey))
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
        if (TagGameManager.Instance == null || TagGameManager.Instance.RoundEnded) return;

        GameObject currentTagger = TagGameManager.Instance.CurrentTagger;
        GameObject runner = TagGameManager.Instance.Runner;

        if ((currentTagger == gameObject && hitObject == runner) ||
            (runner == gameObject && hitObject == currentTagger))
        {
            TagGameManager.Instance.EndRound(currentTagger, "Tagged!");
        }
    }

    public void UpdateTaggerVisuals()
    {
        EnsureVisualsExist();

        bool isTagger = TagGameManager.Instance != null && TagGameManager.Instance.CurrentTagger == gameObject;
        taggerTitle.color = Color.red;
        taggerTitle.text = isTagger ? "TAGGER" : string.Empty;
        taggerImageRenderer.sprite = taggerImage;
        taggerImageRenderer.enabled = isTagger && taggerImage != null;
    }

    public void ShowWinnerTitle(GameObject winner, string label)
    {
        EnsureVisualsExist();

        bool isWinner = (winner == gameObject);
        taggerTitle.color = Color.green;
        taggerTitle.text = isWinner ? label : string.Empty;
        taggerImageRenderer.enabled = false;
    }

    private void EnsureVisualsExist()
    {
        if (taggerTitle == null)
        {
            GameObject titleObject = new GameObject("TaggerTitle");
            titleObject.transform.SetParent(transform);
            titleObject.transform.localPosition = new Vector3(0f, baseHeadHeight, 0f);

            taggerTitle = titleObject.AddComponent<TextMesh>();
            taggerTitle.anchor = TextAnchor.MiddleCenter;
            taggerTitle.alignment = TextAlignment.Center;
            taggerTitle.fontSize = 32;
            taggerTitle.characterSize = 0.1f;
        }

        if (iconObject == null)
        {
            iconObject = new GameObject("TaggerImage");
            iconObject.transform.SetParent(transform);
            iconObject.transform.localPosition = new Vector3(0f, baseHeadHeight + 0.7f, 0f);
            iconObject.transform.localScale = Vector3.one * iconScale;

            taggerImageRenderer = iconObject.AddComponent<SpriteRenderer>();
            taggerImageRenderer.sortingOrder = 10;
        }
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
