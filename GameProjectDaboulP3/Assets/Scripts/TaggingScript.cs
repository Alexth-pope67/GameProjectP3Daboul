using UnityEngine;

public class TaggingScript : MonoBehaviour
{
    private static GameObject currentTagger;
    private static bool gameInitialized;
    private TextMesh taggerTitle;
    private SpriteRenderer taggerImageRenderer;

    public Sprite taggerImage;

    void Start()
    {
        if (!gameInitialized)
        {
            GameObject playerOne = GameObject.FindGameObjectWithTag("Player1");
            GameObject playerTwo = GameObject.FindGameObjectWithTag("Player2");

            if (playerOne != null && playerTwo != null)
            {
                currentTagger = Random.Range(0, 2) == 0 ? playerOne : playerTwo;
                gameInitialized = true;
            }
        }

        UpdateTaggerTitle();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (currentTagger != gameObject || !hit.collider.CompareTag("Player1") && !hit.collider.CompareTag("Player2"))
        {
            return;
        }

        currentTagger = hit.collider.gameObject;
        UpdateTaggerTitle();

        TaggingScript otherTaggingScript = hit.collider.GetComponent<TaggingScript>();
        if (otherTaggingScript != null)
        {
            otherTaggingScript.UpdateTaggerTitle();
        }
    }

    private void UpdateTaggerTitle()
    {
        if (taggerTitle == null)
        {
            GameObject titleObject = new GameObject("TaggerTitle");
            titleObject.transform.SetParent(transform);
            titleObject.transform.localPosition = Vector3.up * 2.5f;

            taggerTitle = titleObject.AddComponent<TextMesh>();
            taggerTitle.anchor = TextAnchor.MiddleCenter;
            taggerTitle.alignment = TextAlignment.Center;
            taggerTitle.fontSize = 32;
            taggerTitle.characterSize = 0.1f;
            taggerTitle.color = Color.red;
        }

        if (taggerImageRenderer == null)
        {
            GameObject imageObject = new GameObject("TaggerImage");
            imageObject.transform.SetParent(transform);
            imageObject.transform.localPosition = Vector3.up * 3.2f;
            imageObject.transform.localScale = Vector3.one * 0.5f;

            taggerImageRenderer = imageObject.AddComponent<SpriteRenderer>();
            taggerImageRenderer.sortingOrder = 1;
        }

        taggerTitle.text = currentTagger == gameObject ? "TAGGER" : string.Empty;
        taggerImageRenderer.sprite = taggerImage;
        taggerImageRenderer.enabled = currentTagger == gameObject && taggerImage != null;
    }

    void LateUpdate()
    {
        if (taggerTitle != null && Camera.main != null)
        {
            taggerTitle.transform.rotation = Camera.main.transform.rotation;
        }

        if (taggerImageRenderer != null && Camera.main != null)
        {
            taggerImageRenderer.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
