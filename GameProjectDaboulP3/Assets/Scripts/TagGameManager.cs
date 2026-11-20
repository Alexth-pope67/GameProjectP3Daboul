using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TagGameManager : MonoBehaviour
{
    public static TagGameManager Instance { get; private set; }

    private static int player1Score;
    private static int player2Score;
    private static bool charactersSwapped;
    private static int characterPairIndex;

    [Header("Match Rules")]
    public float roundDuration = 45f;
    public float restartDelay = 3f;
    public int winsNeeded = 3;

    [Header("Character Switching (Optional Prefabs)")]
    [Tooltip("Add character prefabs here to swap them every round. Leave empty to swap existing scene players.")]
    public List<GameObject> characterPrefabs = new List<GameObject>();
    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;

    public GameObject CurrentTagger { get; private set; }
    public GameObject Runner { get; private set; }
    public bool RoundEnded { get; private set; }

    private float timeRemaining;
    private bool roundInitialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetMatchStateOnPlay()
    {
        player1Score = 0;
        player2Score = 0;
        charactersSwapped = false;
        characterPairIndex = 0;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SetupAndInitializeRound();
    }

    private void SetupAndInitializeRound()
    {
        GameObject playerOne = GameObject.FindGameObjectWithTag("Player1");
        GameObject playerTwo = GameObject.FindGameObjectWithTag("Player2");

        // CASE 1: Character Prefabs are assigned in the Inspector
        if (characterPrefabs != null && characterPrefabs.Count >= 2)
        {
            Vector3 p1Pos = player1SpawnPoint != null ? player1SpawnPoint.position : (playerOne != null ? playerOne.transform.position : new Vector3(-3f, 1f, 0f));
            Quaternion p1Rot = player1SpawnPoint != null ? player1SpawnPoint.rotation : (playerOne != null ? playerOne.transform.rotation : Quaternion.identity);

            Vector3 p2Pos = player2SpawnPoint != null ? player2SpawnPoint.position : (playerTwo != null ? playerTwo.transform.position : new Vector3(3f, 1f, 0f));
            Quaternion p2Rot = player2SpawnPoint != null ? player2SpawnPoint.rotation : (playerTwo != null ? playerTwo.transform.rotation : Quaternion.identity);

            // Disable old players immediately so they can't interfere before Destroy finishes
            if (playerOne != null) { playerOne.SetActive(false); Destroy(playerOne); }
            if (playerTwo != null) { playerTwo.SetActive(false); Destroy(playerTwo); }

            int indexA = characterPairIndex % characterPrefabs.Count;
            int indexB = (characterPairIndex + 1) % characterPrefabs.Count;

            GameObject p1Prefab = charactersSwapped ? characterPrefabs[indexB] : characterPrefabs[indexA];
            GameObject p2Prefab = charactersSwapped ? characterPrefabs[indexA] : characterPrefabs[indexB];

            playerOne = Instantiate(p1Prefab, p1Pos, p1Rot);
            playerOne.tag = "Player1";
            playerOne.name = "Player1_Character";

            playerTwo = Instantiate(p2Prefab, p2Pos, p2Rot);
            playerTwo.tag = "Player2";
            playerTwo.name = "Player2_Character";
        }
        // CASE 2: No prefabs assigned — swap the existing scene characters' positions & tags
        else if (charactersSwapped && playerOne != null && playerTwo != null)
        {
            Vector3 tempPos = playerOne.transform.position;
            Quaternion tempRot = playerOne.transform.rotation;

            CharacterController cc1 = playerOne.GetComponent<CharacterController>();
            CharacterController cc2 = playerTwo.GetComponent<CharacterController>();
            if (cc1 != null) cc1.enabled = false;
            if (cc2 != null) cc2.enabled = false;

            playerOne.transform.SetPositionAndRotation(playerTwo.transform.position, playerTwo.transform.rotation);
            playerTwo.transform.SetPositionAndRotation(tempPos, tempRot);

            playerOne.tag = "Player2";
            playerTwo.tag = "Player1";

            if (cc1 != null) cc1.enabled = true;
            if (cc2 != null) cc2.enabled = true;

            // Swap references so playerOne always points to the current Player1 tag
            GameObject tempObj = playerOne;
            playerOne = playerTwo;
            playerTwo = tempObj;
        }

        if (playerOne != null && playerTwo != null)
        {
            bool p1StartsAsTagger = Random.Range(0, 2) == 0;
            CurrentTagger = p1StartsAsTagger ? playerOne : playerTwo;
            Runner = p1StartsAsTagger ? playerTwo : playerOne;

            timeRemaining = roundDuration;
            RoundEnded = false;
            roundInitialized = true;

            Debug.Log($"Round Started! Tagger is: {CurrentTagger.name} ({CurrentTagger.tag})");
        }
        else
        {
            Debug.LogError("TagGameManager: Could not find both 'Player1' and 'Player2' tags in the scene!");
        }
    }

    void Update()
    {
        if (!roundInitialized || RoundEnded) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndRound(Runner, "Time's up!");
        }
    }

    public void EndRound(GameObject winner, string reason)
    {
        if (RoundEnded || winner == null) return;
        RoundEnded = true;

        if (winner.CompareTag("Player1"))
        {
            player1Score++;
        }
        else if (winner.CompareTag("Player2"))
        {
            player2Score++;
        }

        bool matchWon = player1Score >= winsNeeded || player2Score >= winsNeeded;
        string statusText = matchWon ? "MATCH CHAMPION!" : "ROUND WINNER!";

        Debug.Log($"{reason} {winner.tag} wins! Score: P1 {player1Score} - {player2Score} P2");

        foreach (TaggingScript player in FindObjectsByType<TaggingScript>(FindObjectsSortMode.None))
        {
            player.ShowWinnerTitle(winner, statusText);
        }

        StartCoroutine(RestartRoundRoutine(matchWon));
    }

    private IEnumerator RestartRoundRoutine(bool resetMatchScores)
    {
        yield return new WaitForSeconds(resetMatchScores ? restartDelay + 2f : restartDelay);

        charactersSwapped = !charactersSwapped;

        if (resetMatchScores)
        {
            player1Score = 0;
            player2Score = 0;
            characterPairIndex += 2;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnGUI()
    {
        if (!roundInitialized) return;

        GUIStyle timerStyle = new GUIStyle(GUI.skin.label);
        timerStyle.fontSize = 36;
        timerStyle.alignment = TextAnchor.UpperCenter;
        timerStyle.normal.textColor = Color.white;

        GUIStyle scoreStyle = new GUIStyle(GUI.skin.label);
        scoreStyle.fontSize = 28;
        scoreStyle.alignment = TextAnchor.UpperCenter;
        scoreStyle.normal.textColor = Color.yellow;

        GUI.Label(new Rect(0, 15, Screen.width, 50), $"Time: {Mathf.CeilToInt(timeRemaining)}", timerStyle);
        GUI.Label(new Rect(0, 60, Screen.width, 45), $"Player 1: {player1Score}  |  Player 2: {player2Score}  (First to {winsNeeded})", scoreStyle);
    }
}
