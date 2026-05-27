using UnityEngine;

public class HomeController : MonoBehaviour
{
    public GameObject medItemPrefab;
    public Transform contentParent;

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (MedicationManager.Instance == null)
            return;

        var meds = MedicationManager.Instance.data.medications;
        foreach (var med in meds)
        {
            GameObject item = Instantiate(medItemPrefab, contentParent);
            item.GetComponent<MedItemView>().Setup(med);
        }

        Debug.Log("Home loaded with " + meds.Count + " medication(s)");
    }
}
