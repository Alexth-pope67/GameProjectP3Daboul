using UnityEngine;

public class TaggingScript : MonoBehaviour
{
    private TextMesh taggerTitle;
    private GameObject iconObject;
    private SpriteRenderer taggerImageRenderer;
    private bool visualsCreated;

    [Header("Overhead Symbol & Text Settings")]
    public Sprite taggerImage;
    [Tooltip("Exact world units above the character's head")]
    public float heightAboveHead = 0.6f;
    [Tooltip("Exact world units for the text above the head")]
    public float textOffset = 0.2f;
    [Tooltip("True world-space size of the danger symbol")]
    public float iconScale = 2.5f;
    public bool bobUpAndDown = true;

    [Header("Tagging Settings")]
    public float autoTouchRadius = 1.3f;
    public float manualTagReach = 2.2f;
    public KeyCode tagKey = KeyCode.E;

    void Start()
    {
        EnsureVisualsExist();
        UpdateTaggerVisuals();
    }

    private Vector3 GetTrueHeadPosition()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            return new Vector3(rend.bounds.center.x, rend.bounds.max.y, rend.bounds.center.z);
        }

        Collider col = GetComponentInChildren<Collider>();
        if (col != null)
        {
            return new Vector3(col.bounds.center.x, col.bounds.max.y, col.bounds.center.z);
        }

        return transform.position + Vector3.up * 2f;
    }

    private Vector3 GetTrueBodyCenter()
    {
        Collider col = GetComponentInChildren<Collider>();
        if (col != null) return col.bounds.center;

        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null) return rend.bounds.center;

        return transform.position;
    }

    void Update()
    {
        if (TagGameManager.Instance == null || TagGameManager.Instance.RoundEnded)
        {
            return;
        }

        CheckProximityAndManualTag();
    }

    private void CheckProximityAndManualTag()
    {
        GameObject currentTagger = TagGameManager.Instance.CurrentTagger;
        GameObject runner = TagGameManager.Instance.Runner;

        if (currentTagger != gameObject || runner == null) return;

        TaggingScript runnerScript = runner.GetComponent<TaggingScript>();
        Vector3 myPos = GetTrueBodyCenter();
        Vector3 runnerPos = runnerScript != null ? runnerScript.GetTrueBodyCenter() : runner.transform.position;

        float distance = Vector3.Distance(myPos, runnerPos);

        if (distance <= autoTouchRadius)
        {
            TagGameManager.Instance.EndRound(currentTagger, "Tagged!");
            return;
        }

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
            EnsureVisualsExist();
        }

        if (TagGameManager.Instance != null && TagGameManager.Instance.RoundEnded) return;

        bool isTagger = TagGameManager.Instance != null && TagGameManager.Instance.CurrentTagger == gameObject;

        if (taggerTitle != null)
        {
            taggerTitle.color = Color.red;
            taggerTitle.text = isTagger ? "TAGGER" : string.Empty;
            taggerTitle.gameObject.SetActive(isTagger);
        }

        if (iconObject != null && taggerImageRenderer != null)
        {
            taggerImageRenderer.sprite = taggerImage;
            bool showIcon = isTagger && taggerImage != null;
            taggerImageRenderer.enabled = showIcon;
            iconObject.SetActive(showIcon);
        }
    }

    public void ShowWinnerTitle(GameObject winner, string label)
    {
        if (!visualsCreated)
        {
            EnsureVisualsExist();
        }

        bool isWinner = (winner == gameObject);

        if (taggerTitle != null)
        {
            taggerTitle.gameObject.SetActive(isWinner);
            taggerTitle.color = Color.green;
            taggerTitle.text = isWinner ? label : string.Empty;
        }

        if (iconObject != null)
        {
            iconObject.SetActive(false);
        }
    }

    private void EnsureVisualsExist()
    {
        if (visualsCreated) return;

        GameObject titleObject = new GameObject($"{gameObject.name}_TaggerTitle");
        taggerTitle = titleObject.AddComponent<TextMesh>();
        taggerTitle.anchor = TextAnchor.MiddleCenter;
        taggerTitle.alignment = TextAlignment.Center;
        taggerTitle.fontSize = 32;
        taggerTitle.characterSize = 0.05f;

        iconObject = new GameObject($"{gameObject.name}_TaggerImage");
        taggerImageRenderer = iconObject.AddComponent<SpriteRenderer>();
        taggerImageRenderer.sortingOrder = 100;

        visualsCreated = true;
    }

    void LateUpdate()
    {
        if (!visualsCreated) return;

        // Continuously enforce that ONLY the true CurrentTagger displays the symbol and title
        if (TagGameManager.Instance != null && !TagGameManager.Instance.RoundEnded)
        {
            UpdateTaggerVisuals();
        }

        Vector3 headTop = GetTrueHeadPosition();

        if (taggerTitle != null && taggerTitle.gameObject.activeSelf)
        {
            taggerTitle.transform.position = headTop + Vector3.up * textOffset;
            if (Camera.main != null)
            {
                taggerTitle.transform.rotation = Camera.main.transform.rotation;
            }
        }

        if (iconObject != null && iconObject.activeSelf)
        {
            iconObject.transform.localScale = Vector3.one * iconScale;
            float bobOffset = bobUpAndDown ? Mathf.Sin(Time.time * 4f) * 0.08f : 0f;
            iconObject.transform.position = headTop + Vector3.up * (heightAboveHead + bobOffset);

            if (Camera.main != null)
            {
                iconObject.transform.rotation = Camera.main.transform.rotation;
            }
        }
    }

    void OnDestroy()
    {
        if (taggerTitle != null) Destroy(taggerTitle.gameObject);
        if (iconObject != null) Destroy(iconObject);
    }
}
