using System;
using System.Globalization;

public static class MedicationTimeUtil
{
    static readonly string[] InputFormats =
    {
        "HH:mm",
        "H:mm",
        "hh:mm tt",
        "h:mm tt",
        "hh:mmtt",
        "h:mmtt",
    };

    public static bool TryParse(string input, out TimeSpan time)
    {
        time = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        input = input.Trim();
        if (TimeSpan.TryParse(input, out time))
            return true;

        if (DateTime.TryParseExact(
                input,
                InputFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out DateTime dt))
        {
            time = dt.TimeOfDay;
            return true;
        }

        return false;
    }

    public static string ToStorage(TimeSpan time) =>
        DateTime.Today.Add(time).ToString("HH:mm");

    public static string ToDisplay(string stored)
    {
        if (!TryParse(stored, out TimeSpan time))
            return stored;
        return DateTime.Today.Add(time).ToString("hh:mm tt", CultureInfo.InvariantCulture);
    }

    public static bool IsTakenToday(Medication med)
    {
        if (med?.history == null) return false;
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        return med.history.Exists(h => h.date == today && h.taken);
    }

    public static bool TryGetNextDose(Medication med, out DateTime nextDose)
    {
        nextDose = default;
        if (med == null || med.reminderTimes == null || med.reminderTimes.Count == 0)
            return false;
        if (!TryParse(med.reminderTimes[0], out TimeSpan timeOfDay))
            return false;

        DateTime todayDose = DateTime.Today.Add(timeOfDay);
        if (!IsTakenToday(med) && DateTime.Now < todayDose)
            nextDose = todayDose;
        else
            nextDose = todayDose.AddDays(1);

        return true;
    }

    public static string FormatCountdown(TimeSpan remaining)
    {
        if (remaining.TotalSeconds < 0)
            remaining = TimeSpan.Zero;

        if (remaining.TotalDays >= 1)
            return $"{(int)remaining.TotalDays}d {remaining.Hours}h";

        if (remaining.TotalHours >= 1)
            return $"{remaining.Hours}h {remaining.Minutes}m";

        if (remaining.TotalMinutes >= 1)
            return $"{remaining.Minutes}m";

        return "<1m";
    }

    public const int OnTimeToleranceMinutes = 5;

    public enum DoseTimingStatus
    {
        OnTime,
        Early,
        Late,
        Missed,
        Unknown
    }

    public struct DoseTimingResult
    {
        public DoseTimingStatus Status;
        public TimeSpan OffsetFromScheduled;
    }

    public static DoseTimingResult GetDoseTiming(DoseLog log)
    {
        var result = new DoseTimingResult { Status = DoseTimingStatus.Unknown };
        if (log == null || !log.taken)
        {
            result.Status = DoseTimingStatus.Missed;
            return result;
        }

        if (!TryParse(log.time, out TimeSpan scheduledTime))
            return result;

        string actualRaw = string.IsNullOrEmpty(log.takenAt) ? log.time : log.takenAt;
        if (!TryParse(actualRaw, out TimeSpan actualTime))
            return result;

        if (!DateTime.TryParseExact(log.date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime day))
            return result;

        DateTime scheduled = day.Add(scheduledTime);
        DateTime actual = day.Add(actualTime);
        TimeSpan offset = actual - scheduled;
        result.OffsetFromScheduled = offset;

        double absMinutes = Math.Abs(offset.TotalMinutes);
        if (absMinutes <= OnTimeToleranceMinutes)
            result.Status = DoseTimingStatus.OnTime;
        else if (offset.TotalMinutes < 0)
            result.Status = DoseTimingStatus.Early;
        else
            result.Status = DoseTimingStatus.Late;

        return result;
    }

    public static string FormatTimingLabel(DoseTimingResult timing)
    {
        switch (timing.Status)
        {
            case DoseTimingStatus.OnTime:
                return "On time";
            case DoseTimingStatus.Early:
                return "Early (" + FormatDuration(timing.OffsetFromScheduled.Negate()) + " early)";
            case DoseTimingStatus.Late:
                return "Late (" + FormatDuration(timing.OffsetFromScheduled) + " late)";
            case DoseTimingStatus.Missed:
                return "Missed";
            default:
                return "Unknown";
        }
    }

    static string FormatDuration(TimeSpan span)
    {
        span = span.Duration();
        if (span.TotalDays >= 1)
            return $"{(int)span.TotalDays}d {span.Hours}h";
        if (span.TotalHours >= 1)
            return $"{span.Hours}h {span.Minutes}m";
        if (span.TotalMinutes >= 1)
            return $"{span.Minutes}m";
        return "<1m";
    }

    public static string FormatHistoryDate(string yyyyMmDd)
    {
        if (!DateTime.TryParseExact(yyyyMmDd, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime day))
            return yyyyMmDd;
        return day.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
    }
}
