using UnityEngine;

public class Lamp : MonoBehaviour
{
    [SerializeField]
    private Grabbable grabbable;

    [SerializeField]
    private Transform pivot;

    [SerializeField]
    private Transform reversePivot;

    public void SwitchPivot()
    {
        if (grabbable.CustomPivot == pivot) 
            grabbable.CustomPivot = reversePivot;
         else
            grabbable.CustomPivot = pivot;
    }
}
