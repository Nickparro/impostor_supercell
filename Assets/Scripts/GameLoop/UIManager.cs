using System.Collections;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject rolePanel;
    [SerializeField] private GameObject iaPanel;

    [SerializeField] private TextMeshProUGUI roleNameText;
    [SerializeField] private TextMeshProUGUI roleDescriptionText;

    [SerializeField] private TextMeshProUGUI iaDescriptionText;

    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TMP_InputField answerInputField;

    public static UIManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void ShowRolePanel(string roleName, string roleDescription)
    {
        rolePanel.gameObject.SetActive(true);
        roleNameText.text = roleName;
        roleDescriptionText.text = roleDescription;
        StartCoroutine(HideRolePanelAfterDelay(12f));
    }

    private IEnumerator HideRolePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        rolePanel.gameObject.SetActive(false);
    }

    public void ShowIAPanel(string iaDescription)
    {
        iaPanel.gameObject.SetActive(true);
        iaDescriptionText.text = iaDescription;
        StartCoroutine(HideIaPanelAfterDelay(12f));
    }

    private IEnumerator HideIaPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        iaPanel.gameObject.SetActive(false);
    }

    public void ShowQuestion(string question)
    {
        iaPanel.gameObject.SetActive(true);
        questionText.text = question;
    }

    public void SendAnswer()
    {
        string answer = answerInputField.text;
        GameManager.Instance.SendAnswerAsync(answer);
    }
}
