using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReminderManager : MonoBehaviour
{
    public GameObject popupRoot;
    public TMP_Text popupMedName;
    public Button takeButton;
    public Button dismissButton;

    private Medication currentMed;
    private string currentTime;
    private float checkInterval = 5f;
    private float timer = 0f;

    void Start()
    {
        popupRoot.SetActive(false);
        takeButton.onClick.AddListener(OnTakeClicked);
        dismissButton.onClick.AddListener(OnDismissClicked);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            CheckReminders();
        }
    }

    void CheckReminders()
    {
        if (popupRoot.activeSelf) return; // skip if popup is already up

        string today = DateTime.Now.ToString("yyyy-MM-dd");
        string nowTime = DateTime.Now.ToString("HH:mm");

        var meds = MedicationManager.Instance.data.medications;
        foreach (var med in meds)
        {
            // new day wipe the notified list
            if (med.lastNotifiedDate != today)
            {
                med.notifiedToday.Clear();
                med.lastNotifiedDate = today;
            }

            foreach (var reminderTime in med.reminderTimes)
            {
                if (string.IsNullOrEmpty(reminderTime)) continue;

                // dose time passed and we have not nudged this slot yet
                if (IsTimeReached(reminderTime, nowTime) && !med.notifiedToday.Contains(reminderTime))
                {
                    ShowPopup(med, reminderTime);
                    return; // only one popup at a time
                }
            }
        }
    }

    bool IsTimeReached(string scheduled, string now)
    {
        if (!MedicationTimeUtil.TryParse(scheduled, out TimeSpan s)) return false;
        if (!MedicationTimeUtil.TryParse(now, out TimeSpan n)) return false;
        return n >= s;
    }

    void ShowPopup(Medication med, string time)
    {
        currentMed = med;
        currentTime = time;
        popupMedName.text = med.name;
        popupRoot.SetActive(true);
    }

    void OnTakeClicked()
    {
        MedicationManager.Instance.LogDose(
            currentMed.id, currentTime, true, DateTime.Now.ToString("HH:mm"));
        currentMed.notifiedToday.Add(currentTime);
        MedicationManager.Instance.Save();
        popupRoot.SetActive(false);

        var home = FindFirstObjectByType<HomeController>();
        if (home != null)
            home.Refresh();
    }

    void OnDismissClicked()
    {
        currentMed.notifiedToday.Add(currentTime);
        MedicationManager.Instance.Save();
        popupRoot.SetActive(false);
    }
}