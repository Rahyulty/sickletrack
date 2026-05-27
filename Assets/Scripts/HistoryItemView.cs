using UnityEngine;
using TMPro;

public class HistoryItemView : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text infoText;

    public void Setup(string medName, string dose, DoseLog log)
    {
        nameText.text = medName;

        string takenDisplay = MedicationTimeUtil.ToDisplay(
            string.IsNullOrEmpty(log.takenAt) ? log.time : log.takenAt);
        string scheduledDisplay = MedicationTimeUtil.ToDisplay(log.time);
        var timing = MedicationTimeUtil.GetDoseTiming(log);
        string statusLabel = MedicationTimeUtil.FormatTimingLabel(timing);

        string details = MedicationTimeUtil.FormatHistoryDate(log.date)
            + "  •  Taken " + takenDisplay
            + "  •  Scheduled " + scheduledDisplay
            + "\n" + statusLabel;

        infoText.text = string.IsNullOrEmpty(dose) ? details : dose + "  •  " + details;

        infoText.color = GetStatusColor(timing.Status);
    }

    static Color GetStatusColor(MedicationTimeUtil.DoseTimingStatus status)
    {
        switch (status)
        {
            case MedicationTimeUtil.DoseTimingStatus.OnTime:
                return new Color(0.1f, 0.55f, 0.2f);
            case MedicationTimeUtil.DoseTimingStatus.Early:
                return new Color(0.85f, 0.55f, 0.05f);
            case MedicationTimeUtil.DoseTimingStatus.Late:
                return new Color(0.85f, 0.2f, 0.15f);
            default:
                return Color.black;
        }
    }
}
