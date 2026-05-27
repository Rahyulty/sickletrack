using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MedicationManager : MonoBehaviour
{
    public static MedicationManager Instance { get; private set; }

    public MedicationList data = new MedicationList();
    private string savePath;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "meds.json");
        Load();
    }

    public void AddMedication(Medication med)
    {
        med.id = System.Guid.NewGuid().ToString();
        data.medications.Add(med);
        Save();
    }

    public void LogDose(string medId, string scheduledTime, bool taken, string takenAt = null)
    {
        var med = data.medications.Find(m => m.id == medId);
        if (med == null) return;

        string nowTime = System.DateTime.Now.ToString("HH:mm");
        med.history.Add(new DoseLog
        {
            date = System.DateTime.Now.ToString("yyyy-MM-dd"),
            time = scheduledTime,
            takenAt = string.IsNullOrEmpty(takenAt) ? nowTime : takenAt,
            taken = taken
        });
        Save();
    }

    public bool RemoveMedication(string medId)
    {
        int index = data.medications.FindIndex(m => m.id == medId);
        if (index < 0) return false;

        data.medications.RemoveAt(index);
        Save();
        return true;
    }

    public void MarkRemindersNotifiedToday(Medication med)
    {
        if (med == null) return;

        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        med.lastNotifiedDate = today;

        foreach (string reminderTime in med.reminderTimes)
        {
            if (!string.IsNullOrEmpty(reminderTime) && !med.notifiedToday.Contains(reminderTime))
                med.notifiedToday.Add(reminderTime);
        }

        Save();
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Saved to: " + savePath);
    }

    public void Load()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<MedicationList>(json);
        }
        else
        {
            data = new MedicationList();
        }
    }
}