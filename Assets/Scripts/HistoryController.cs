using System;
using System.Collections.Generic;
using UnityEngine;

public class HistoryController : MonoBehaviour
{
    public GameObject historyItemPrefab;
    public Transform contentParent;

    struct HistoryRow
    {
        public string medName;
        public string dose;
        public DoseLog log;
        public DateTime sortKey;
    }

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (MedicationManager.Instance == null)
        {
            Debug.LogWarning("MedicationManager not found. Open Home first.");
            return;
        }

        var rows = new List<HistoryRow>();
        foreach (var med in MedicationManager.Instance.data.medications)
        {
            if (med.history == null) continue;

            foreach (var log in med.history)
            {
                if (log == null || !log.taken) continue;
                rows.Add(new HistoryRow
                {
                    medName = med.name,
                    dose = med.dose,
                    log = log,
                    sortKey = GetSortDateTime(log)
                });
            }
        }

        rows.Sort((a, b) => b.sortKey.CompareTo(a.sortKey));

        foreach (var row in rows)
        {
            GameObject item = Instantiate(historyItemPrefab, contentParent);
            item.GetComponent<HistoryItemView>().Setup(row.medName, row.dose, row.log);
        }

        Debug.Log("History loaded with " + rows.Count + " dose(s)");
    }

    static DateTime GetSortDateTime(DoseLog log)
    {
        if (!DateTime.TryParseExact(log.date, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime day))
            return DateTime.MinValue;

        string timeRaw = string.IsNullOrEmpty(log.takenAt) ? log.time : log.takenAt;
        if (MedicationTimeUtil.TryParse(timeRaw, out TimeSpan time))
            return day.Add(time);

        return day;
    }
}
