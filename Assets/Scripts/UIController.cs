using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    [Header("Panels")]
    [SerializeField] private GameObject characterContextPanel;
    [SerializeField] private GameObject sherlockDialoguePanel;
    [SerializeField] private GameObject playerInputPanel;
    [SerializeField] private GameObject[] strikes;
    private int currentStrikeIndex = 0;

    [Header("Buttons")]
    [SerializeField] private Button characterButton;
    [SerializeField] private Button closeCharacterButton;
    [SerializeField] private Button answerButton;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button backButton;

    [Header("Texts")]
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI characterDescription;
    public TextMeshProUGUI characterROL;
    public TextMeshProUGUI sherlokDialogueText;
    [SerializeField] private TMP_InputField playerAnswerInput;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        characterButton.onClick.AddListener(OpenCharacter);
        closeCharacterButton.onClick.AddListener(CloseCharacter);
        answerButton.onClick.AddListener(OpenAnswerInput);
        backButton.onClick.AddListener(CloseAnswerInput);
        sendButton.onClick.AddListener(SendAnswer);
    }
    private void Update()
    {

    }
    public void UpdateStrike() 
    {
        if (currentStrikeIndex < strikes.Length)
        {
            GameObject strike = strikes[currentStrikeIndex];

            strike.SetActive(true);
            Debug.Log("activandito");
            currentStrikeIndex++;
        }
        else
        {
            Debug.Log("Ya se encendieron todos los strikes.");
        }
    }
    public void OpenSherlockDialogue()
    {
        sherlockDialoguePanel.SetActive(true);
        closeCharacterButton.gameObject.SetActive(false);
    }
    public void SendAnswer()
    {
        playerInputPanel.SetActive(false);
    }
    private void OpenCharacter() 
    {
        characterContextPanel.SetActive(true);
    }
    private void CloseCharacter()
    {
        characterContextPanel.SetActive(false);
        closeCharacterButton.gameObject.SetActive(true);
    }
    private void OpenAnswerInput() 
    {
        sherlockDialoguePanel.SetActive(false);
        playerInputPanel.SetActive(true);
    }
    private void CloseAnswerInput()
    {
        sherlockDialoguePanel.SetActive(true);
        playerInputPanel.SetActive(false);
    }
}
