using UnityEngine;

public class MeteoriteHitBox : MonoBehaviour
{
    public Meteorite meteorite;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<JavelinFlight>() != null)
        {
            meteorite.OnRockHit();
            if (GameManager.Instance != null)
                GameManager.Instance.OnJavelinLanded();
        }
    }
}
