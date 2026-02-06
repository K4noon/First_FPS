using UnityEngine;
using UnityEngine.UI;

public class CurrentSpeed : MonoBehaviour
{
    [SerializeField] PlayerMovement_Bis clc;
    [SerializeField] Image BarBackground;
    [SerializeField] Image Staminabar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (clc == null)
        {
            clc = GetComponent<PlayerMovement_Bis>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        float currentSpeed = clc.curSpeed;
        Staminabar.fillAmount = currentSpeed / clc.speedMaxSprint;
    }
}
