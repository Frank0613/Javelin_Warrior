using UnityEngine;

public class JavelinEffects : MonoBehaviour
{
    public GameObject chargeEffect;
    public GameObject[] finishEffects;
    public ParticleSystem showupEffect;

    public void ShowCharge(bool show)
    {
        if (chargeEffect != null)
            chargeEffect.SetActive(show);
    }

    public void ShowFinish(bool show)
    {
        foreach (var fx in finishEffects)
        {
            if (fx != null) fx.SetActive(show);
        }
    }

    public void Showup(bool show)
    {
        if (showupEffect != null)
        {
            if (show) showupEffect.Play();
            else showupEffect.Stop();
        }
    }

    public void HideAll()
    {
        ShowCharge(false);
        ShowFinish(false);
    }
}

