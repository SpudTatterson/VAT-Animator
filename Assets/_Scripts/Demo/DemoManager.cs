using UnityEngine;

public class DemoManager : MonoBehaviour
{
    [SerializeField] KeyCode changeKey;

    [SerializeField] GameObject regularVikings;
    [SerializeField] GameObject vatVikings;

    bool vatShown = false;

    void Start()
    {
        regularVikings.SetActive(!vatShown);
        vatVikings.SetActive(vatShown);
        vatShown = !vatShown;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(changeKey))
        {
            regularVikings.SetActive(!vatShown);
            vatVikings.SetActive(vatShown);
            vatShown = !vatShown;
        }
    }
}
