using UnityEngine;

public class GasBreathingController : MonoBehaviour
{
    [SerializeField] private GasExposureState gasExposure;
    [SerializeField] private GasMaskState gasMask;

    [SerializeField] private AudioSource normalBreathing;
    [SerializeField] private AudioSource maskedBreathing;
    [SerializeField] private AudioSource choking;

    private bool lastMaskState;
    private bool lastGasState;

    private void Start()
    {
        UpdateBreathingAudio();
    }

    private void Update()
    {
        if (gasMask.IsWorn != lastMaskState ||
            gasExposure.HeadInGas != lastGasState)
        {
            UpdateBreathingAudio();
        }
    }

    private void UpdateBreathingAudio()
    {
        bool wearingMask = gasMask.IsWorn;
        bool inGas = gasExposure.HeadInGas;

        normalBreathing.Stop();
        maskedBreathing.Stop();
        choking.Stop();

        if (wearingMask)
        {
            maskedBreathing.Play();
        }
        else if (inGas)
        {
            choking.Play();
        }
        else
        {
            normalBreathing.Play();
        }

        lastMaskState = wearingMask;
        lastGasState = inGas;
    }
}