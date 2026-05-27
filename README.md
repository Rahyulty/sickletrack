# SickleTrack

A medication reminder app for students with sickle cell disease


**Author:** Raehan Ramlal
**Course:** CIS 272 Spring 2026
**Repo:** https://github.com/Rahyulty/sickletrack
**video:** https://youtu.be/qka3nfHugT0

## What it does

Users with sickle cell disease may miss their prescribed drugs due to changing school schedules. The goal of SickleTrack is to provide students with a way to track their medication regimen.

SickleTrack allows users to:
- Enter their prescribed medication(s) including name, dose, and preferred dosage time
- Display today’s schedule of medications showing how many minutes until the next one
- Send an in app notification at the time the next dose should be administered 
- Document a drug was given (date/time it was actually taken)
- Record all medication logs in a reverse chronological order.

## How to run

Requires Unity 6 (6000.3.9f1 or compatible) with the URP 2D template.

1. Clone this repo: `git clone https://github.com/Rahyulty/sickletrack.git`
2. Open Unity Hub go to  Open  then select the cloned folder
3. Wait for Unity to import
4. Open `Assets/Scenes/Home.unity`
5. Press Play

data saves to `Application.persistentDataPath/meds.json`.

## Architecture

Three scenes (Home, AddMed, History) connected by a small `SceneLoader` script. Data lives in a persistent `MedicationManager` singleton that survives scene loads via `DontDestroyOnLoad`. The manager reads and writes a single JSON file using `JsonUtility`.

Each scene's Canvas has a controller MonoBehaviour that pulls data from `MedicationManager.Instance.data` on `Start()`. The home and history scenes instantiate prefab rows under a Vertical Layout Group, so the list grows or shrinks based on the data.

Reminders are in-app rather than OS push notifications. A `ReminderManager` runs on the home Canvas and polls the system clock every 5 seconds. When a scheduled time has passed and hasn't fired yet today, it activates the popup GameObject. Each Medication tracks a `notifiedToday` list and a `lastNotifiedDate` so reminders don't repeat the same day and reset cleanly at midnight.

## Screenshots
| Screen | Image |
|---|---|
| Home | `images/home.png` |
| Add Medication | `images/add-medication.png` |
| Reminder Popup | `images/popup.png` |
| History | `images/history.png` |

## Test cases

All tests run in Play mode in the Unity editor at 1080x1920 (phone portrait).

| #  | Feature                          | Steps                                                             | Expected                                                                     | Result |
| -- | -------------------------------- | ----------------------------------------------------------------- | ---------------------------------------------------------------------------- | ------ |
| 1  | App launch with no data          | Delete meds.json and open the app                                 | Home shows empty list and no console errors                                  | Pass   |
| 2  | Add medication with valid input  | Tap Add Medication. Enter Hydroxyurea, 500mg, 08:00 then tap Save | Goes back to Home. New row shows with name, dose, and scheduled time         | Pass   |
| 3  | Add with empty name              | Leave name blank, fill dose and time, then tap Save               | Stays on AddMed screen. Console logs "Medication name required"              | Pass   |
| 4  | Add with invalid time            | Enter time abc and tap Save                                       | Stays on AddMed. Console logs invalid time message                           | Pass   |
| 5  | Time format flexibility          | Enter 8:00 AM, 8:00PM, and 14:30 on separate meds                 | All three parse and save. Home displays time in 12-hour format               | Pass   |
| 6  | Mark taken from home             | Tap Mark Taken on a med                                           | Button disables and shows "Next in …" countdown. History gets a new entry    | Pass   |
| 7  | Delete medication                | Tap Delete on a med                                               | Row disappears right away. JSON file no longer has that med                  | Pass   |
| 8  | Reminder popup fires             | Add a med with time 1 minute in the future and wait               | Popup appears within 5 seconds of the scheduled time                         | Pass   |
| 9  | Popup Mark Taken                 | Click Mark Taken on the popup                                     | Popup closes. Dose logs with current time in history. Home countdown updates | Pass   |
| 10 | Popup Dismiss                    | Click Dismiss                                                     | Popup closes. No history entry added. Reminder does not fire again today     | Pass   |
| 11 | Reminder doesn't repeat same day | After dismissing, wait through a few check cycles                 | No popup appears                                                             | Pass   |
| 12 | Data persists across restart     | Add meds. Close project, reopen, and Play                         | Same meds load back on Home                                                  | Pass   |
| 13 | History sorted newest first      | Mark several doses across two simulated days. Open History        | Most recent dose is at the top                                               | Pass   |
| 14 | History status colors            | Mark a dose right on time, then one 10 minutes late               | First entry shows "On time" in green. Second shows "Late" in red             | Pass   |
| 15 | Navigation between scenes        | Tap Add Medication then Cancel. Tap View History then Back        | Returns to Home each time with no data loss                                  | Pass   |


## Known limitations

- One reminder time per medication no multi dose yet
- In app reminders only. Popups appear when the app is open the app does not  phone based notifcations yet
- No edit feature. To change a medication just delete and re-add.
- History shows taken doses only. Missed/dismissed doses are not logged.
- later on we should add a notication if any input is missed

## Built with

- Unity 6 
- TextMeshPro for all UI text
- C# 
- JSON

## AI Use
I used Claude (Anthropic) throughout this project as a coding assistant

AI Was not heavily used in this project but it was used to help format the input time into time that the code can read so that they know when to notify users / determine when its late / on time/ early. Ai was also used to help me make the tables for this readme and the documentation for the MD

Thank you again Mr Tang for allowing me to still submit in this project and my work due to my hospitlaziton 
