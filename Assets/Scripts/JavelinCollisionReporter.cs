using UnityEngine;

public class JavelinCollisionReporter : MonoBehaviour
{
    private Accumulation_test accumulationTest;

    void Start()
    {
        // 自動找父物件的腳本，不用手動拖
        accumulationTest = GetComponentInParent<Accumulation_test>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Accumulation"))
            accumulationTest.EnterZone();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Accumulation"))
            accumulationTest.ExitZone();
    }
}