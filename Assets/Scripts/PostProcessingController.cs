using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PostProcessingController : MonoBehaviour
{
    public Volume volume;

    private ColorAdjustments colorAdjustments;

    void Awake()
    {
        // Clone to avoid permanently modifying the shared profile asset
        volume.profile = Instantiate(volume.profile);
        volume.profile.TryGet(out colorAdjustments);
        if (colorAdjustments != null)
            colorAdjustments.saturation.overrideState = true;
    }

    public IEnumerator GameOverEffect(float duration)
    {
        if (colorAdjustments == null) yield break;

        colorAdjustments.saturation.value = 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            colorAdjustments.saturation.value = Mathf.Lerp(0f, -100f, elapsed / duration);
            yield return null;
        }

        colorAdjustments.saturation.value = -100f;
    }
}
