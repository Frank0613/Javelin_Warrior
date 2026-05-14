using UnityEngine;
using System.Collections;

public class Meteorite : MonoBehaviour
{
    [Header("References")]
    public BossHealth bossHealth;
    public GameObject mainRock;
    public GameObject electricityObject;
    public GameObject explosionObject;
    public GameObject smokeObject;

    [Header("Settings")]
    public float emissionFadeDuration = 1f;
    public float electricityToExplosionDelay = 2f;

    [Header("Smoke")]
    public float smokeRampUpDuration = 2f;
    public float smokePeakRate = 150f;
    public float smokePeakDuration = 3f;
    public float smokeRampDownDuration = 2f;

    [Header("Boss Defeat")]
    public BossDefeatTransition bossDefeatTransition;

    public Material rockMat;

    private Collider rockCollider;
    private Color originalEmissionColor;
    private ParticleSystem smokeParticle;
    private bool activated = false;

    void Start()
    {
        rockCollider = mainRock.GetComponent<Collider>();
        rockCollider.enabled = false;

        originalEmissionColor = rockMat.GetColor("_EmissionColor");

        smokeParticle = smokeObject.GetComponent<ParticleSystem>();
        var em = smokeParticle.emission;
        em.rateOverTime = 0f;
    }

    void OnDisable()
    {
        rockMat.SetColor("_EmissionColor", originalEmissionColor);
    }

    void OnApplicationQuit()
    {
        rockMat.SetColor("_EmissionColor", originalEmissionColor);
    }

    void Update()
    {
        if (!activated && !rockCollider.enabled && bossHealth != null && bossHealth.IsDead())
            rockCollider.enabled = true;
    }

    public void OnRockHit()
    {
        if (activated) return;
        activated = true;
        rockCollider.enabled = false;
        StartCoroutine(MeteoriteSequence());
    }

    IEnumerator MeteoriteSequence()
    {
        // Fade HDR emission: blue → black
        float elapsed = 0f;
        while (elapsed < emissionFadeDuration)
        {
            elapsed += Time.deltaTime;
            rockMat.SetColor("_EmissionColor",
                Color.Lerp(originalEmissionColor, Color.black, elapsed / emissionFadeDuration));
            yield return null;
        }
        rockMat.SetColor("_EmissionColor", Color.black);

        electricityObject.SetActive(true);

        yield return new WaitForSeconds(electricityToExplosionDelay);

        explosionObject.SetActive(true);
        mainRock.SetActive(false);
        electricityObject.SetActive(false);

        yield return StartCoroutine(AnimateSmokeRate(0f, smokePeakRate, smokeRampUpDuration));

        if (bossDefeatTransition != null)
            bossDefeatTransition.TriggerTransition();

        yield return new WaitForSeconds(smokePeakDuration);
        yield return StartCoroutine(AnimateSmokeRate(smokePeakRate, 0f, smokeRampDownDuration));
    }

    IEnumerator AnimateSmokeRate(float from, float to, float duration)
    {
        var em = smokeParticle.emission;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            em.rateOverTime = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        em.rateOverTime = to;
    }
}
