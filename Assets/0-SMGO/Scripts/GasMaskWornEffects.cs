using UnityEngine;
using Oculus.Interaction.HandGrab;

public class GasMaskWornEffects : MonoBehaviour
{
    [SerializeField] private GasMaskState gasMaskState;
    [SerializeField] private Transform gasMask;
    [SerializeField] private Renderer lensRenderer;
    [SerializeField] private Material transparentLensMaterial;
    [SerializeField] private float wornScale = 155f;
    [SerializeField] private Renderer maskRenderer;
    [SerializeField] private Material translucentMaskMaterial;
    [SerializeField] private HandGrabInteractable handGrabInteractable;

    private Vector3 originalScale;
    private Material originalMaskMaterial;
    private Material originalLensMaterial;


    private void Awake()
    {
        originalScale = gasMask.localScale;
        originalMaskMaterial = maskRenderer.material;
        originalLensMaterial = lensRenderer.material;
    }


    private void Update()
    {
        if (gasMaskState.IsWorn)
        {
            gasMask.localScale = Vector3.one * wornScale;
            lensRenderer.material = transparentLensMaterial;
            maskRenderer.material = translucentMaskMaterial;
            handGrabInteractable.enabled = false;
        }

        else
        {
            gasMask.localScale = originalScale;
            maskRenderer.material = originalMaskMaterial;
            lensRenderer.material = originalLensMaterial;
        }
    }
}