using UnityEngine;

public class GasExposureState : MonoBehaviour
{
    public bool HeadInGas { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gas"))
            Debug.Log("IN GAS CLOUD");
        HeadInGas = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Gas"))
            Debug.Log("IN FRESH AIR");
        HeadInGas = false;
    }
}