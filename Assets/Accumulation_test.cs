using UnityEngine;

public class Accumulation_test : MonoBehaviour
{
    private bool isAccumulating = false;
    private float accumulateTimer = 0f;
    private int lastPrinted = 0;

    void Update()
    {
        if (isAccumulating)
        {
            accumulateTimer += Time.deltaTime;

            int currentSecond = Mathf.FloorToInt(accumulateTimer);

            if (currentSecond > lastPrinted && currentSecond <= 3)
            {
                lastPrinted = currentSecond;
                Debug.Log(currentSecond);
            }

            if (accumulateTimer >= 3f)
            {
                Debug.Log("success");
                isAccumulating = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Accumulation"))
        {
            isAccumulating = true;
            accumulateTimer = 0f;
            lastPrinted = 0;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Accumulation"))
        {
            isAccumulating = false;
            accumulateTimer = 0f;
            lastPrinted = 0;
        }
    }
}