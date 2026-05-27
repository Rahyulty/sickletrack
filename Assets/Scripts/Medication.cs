using System;
using System.Collections.Generic;

[Serializable]
public class Medication
{
    public string id;
    public string name;
    public string dose;
    public List<string> reminderTimes = new List<string>();
    public List<DoseLog> history = new List<DoseLog>();
    public List<string> notifiedToday = new List<string>(); // reminder slots we already pinged today
    public string lastNotifiedDate = ""; // last day we cleared notifiedToday
}

[Serializable]
public class DoseLog
{
    public string date;     // yyyy-mm-dd
    public string time;     // scheduled dose time HH:mm
    public string takenAt;  // when they actually took it blank uses time on old saves
    public bool taken;
}

[Serializable]
public class MedicationList
{
    public List<Medication> medications = new List<Medication>();
}