using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MedItemView : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text infoText;
    public Button takenButton;
    public Button deleteButton;

    private Medication med;
    private bool listenersBound;
    private TMP_Text takenButtonLabel;
    private Color takenButtonGraphicColor;
    private bool takenToday;
    private float countdownRefreshTimer;

    const string TakenLabel = "Taken";

    void Awake()
    {
        if (takenButton != null)
        {
            takenButtonLabel = takenButton.GetComponentInChildren<TMP_Text>();
            if (takenButton.targetGraphic != null)
                takenButtonGraphicColor = takenButton.targetGraphic.color;
        }
    }

    public void Setup(Medication medication)
    {
        med = medication;
        nameText.text = med.name;

        if (!listenersBound)
        {
            takenButton.onClick.AddListener(OnTakenClicked);
            deleteButton.onClick.AddListener(OnDeleteClicked);
            listenersBound = true;
        }

        RefreshTakenState();
    }

    void Update()
    {
        if (!takenToday || med == null)
            return;

        countdownRefreshTimer += Time.deltaTime;
        if (countdownRefreshTimer >= 1f)
        {
            countdownRefreshTimer = 0f;
            UpdateCountdownDisplay();
        }
    }

    void RefreshTakenState()
    {
        takenToday = MedicationTimeUtil.IsTakenToday(med);
        countdownRefreshTimer = 0f;

        string timeStr = med.reminderTimes.Count > 0
            ? MedicationTimeUtil.ToDisplay(med.reminderTimes[0])
            : "no time set";
        infoText.text = med.dose + "  •  " + timeStr;

        if (takenToday)
        {
            takenButton.interactable = false;
            if (takenButton.targetGraphic != null)
                takenButton.targetGraphic.color = new Color(0.65f, 0.65f, 0.65f, 1f);
            UpdateCountdownDisplay();
        }
        else
        {
            takenButton.interactable = true;
            if (takenButton.targetGraphic != null)
                takenButton.targetGraphic.color = takenButtonGraphicColor;
            if (takenButtonLabel != null)
                takenButtonLabel.text = TakenLabel;
        }
    }

    void UpdateCountdownDisplay()
    {
        if (takenButtonLabel == null)
            return;

        if (!MedicationTimeUtil.TryGetNextDose(med, out DateTime nextDose))
        {
            takenButtonLabel.text = "Done today";
            return;
        }

        TimeSpan remaining = nextDose - DateTime.Now;
        if (remaining.TotalSeconds <= 0)
        {
            RefreshTakenState();
            return;
        }

        string nextAt = MedicationTimeUtil.ToDisplay(
            MedicationTimeUtil.ToStorage(nextDose.TimeOfDay));
        takenButtonLabel.text = "Next in " + MedicationTimeUtil.FormatCountdown(remaining);
        infoText.text = med.dose + "  •  Next: " + nextAt;
    }

    void OnTakenClicked()
    {
        string scheduledTime = med.reminderTimes.Count > 0 ? med.reminderTimes[0] : "";
        string logTime = string.IsNullOrEmpty(scheduledTime)
            ? DateTime.Now.ToString("HH:mm")
            : scheduledTime;

        MedicationManager.Instance.LogDose(med.id, logTime, true, DateTime.Now.ToString("HH:mm"));
        MedicationManager.Instance.MarkRemindersNotifiedToday(med);
        RefreshTakenState();

        Debug.Log("Marked taken early: " + med.name);
    }

    void OnDeleteClicked()
    {
        if (!MedicationManager.Instance.RemoveMedication(med.id))
            return;

        var home = FindFirstObjectByType<HomeController>();
        if (home != null)
            home.Refresh();

        Debug.Log("Deleted medication: " + med.name);
    }
}
