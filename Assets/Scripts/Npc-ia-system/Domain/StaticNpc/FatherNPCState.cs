using UnityEngine;

public class FatherNPCState : MonoBehaviour
{
    public bool isDistracted = false;

    public void SetDistracted(bool value)
    {
        isDistracted = value;
    }
}
