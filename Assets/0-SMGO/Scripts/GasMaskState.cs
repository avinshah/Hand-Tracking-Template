using UnityEngine;

public class GasMaskState : MonoBehaviour
{

    public bool IsWorn { get; private set; }

    public void MaskWorn()
    {
        IsWorn = true;
        Debug.Log("MASK WORN");
    }

    public void MaskRemoved()
    {
        IsWorn = false;
        Debug.Log("MASK REMOVED");
    }
}