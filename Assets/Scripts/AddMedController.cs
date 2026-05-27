using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class AddMedController : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField doseInput;
    public TMP_InputField timeInput;
    public Button saveButton;
    public Button cancelButton;

    void Start()
    {
        saveButton.onClick.AddListener(OnSaveClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    void OnSaveClicked()
    {
        string medName = nameInput.text.Trim();
        string dose = doseInput.text.Trim();
        string timeRaw = timeInput.text.Trim();

        if (string.IsNullOrEmpty(medName))
        {
            Debug.Log("Medication name required");
            return;
        }

        if (!MedicationTimeUtil.TryParse(timeRaw, out System.TimeSpan parsedTime))
        {
            Debug.Log("Invalid time. Use 08:00 AM or 14:30 (24-hour).");
            return;
        }

        Medication med = new Medication();
        med.name = medName;
        med.dose = dose;
        med.reminderTimes.Add(MedicationTimeUtil.ToStorage(parsedTime));

        MedicationManager.Instance.AddMedication(med);
        Debug.Log("Saved: " + medName);

        SceneManager.LoadScene("Home");
    }

    void OnCancelClicked()
    {
        SceneManager.LoadScene("Home");
    }
}