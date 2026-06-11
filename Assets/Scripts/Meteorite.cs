using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Meteorite : MonoBehaviour
{
    [Header("References")]
    public BossHealth bossHealth;
    public GameObject mainRock;
    public GameObject electricityObject;
    public GameObject explosionObject;
    public GameObject[] smokeObject;

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

    [Header("Audio")]
    public SfxCue bossHurtSfx;              // played once when the meteorite rock is hit
    public SfxCue electricitySfx;           // when the electricity turns on
    public SfxCue explosionSfx;             // synced with the explosion
    public SfxCue whooshSfx;                // played on each smoke ramp (whoosh)

    [Header("Events")]
    public UnityEvent onSequenceComplete;   // fires when the meteorite sequence fully finishes

    public Material rockMat;

    [Header("Flashing Material")]
    public Material flashMat;               // material whose emission pulses to look like it's flashing
    public float flashMinIntensity = 1f;    // emission multiplier low point
    public float flashMaxIntensity = 3f;    // emission multiplier high point
    public float flashSpeed = 2f;           // how fast it ping-pongs

    private Collider rockCollider;
    private Color originalEmissionColor;
    private Color flashBaseEmission;        // flashMat's authored emission color (restored on exit)
    private ParticleSystem[] smokeParticles;
    private bool activated = false;

    void Start()
    {
        rockCollider = mainRock.GetComponent<Collider>();
        rockCollider.enabled = false;

        originalEmissionColor = rockMat.GetColor("_EmissionColor");

        if (flashMat != null)
        {
            flashBaseEmission = flashMat.GetColor("_EmissionColor");
            flashMat.EnableKeyword("_EMISSION");
        }

        smokeParticles = new ParticleSystem[smokeObject.Length];
        for (int i = 0; i < smokeObject.Length; i++)
        {
            smokeParticles[i] = smokeObject[i].GetComponent<ParticleSystem>();
            var em = smokeParticles[i].emission;
            em.rateOverTime = 0f;
        }
    }

    void OnDisable()
    {
        rockMat.SetColor("_EmissionColor", originalEmissionColor);
        if (flashMat != null) flashMat.SetColor("_EmissionColor", flashBaseEmission);
    }

    void OnApplicationQuit()
    {
        rockMat.SetColor("_EmissionColor", originalEmissionColor);
        if (flashMat != null) flashMat.SetColor("_EmissionColor", flashBaseEmission);
    }

    void Update()
    {
        if (!activated && !rockCollider.enabled && bossHealth != null && bossHealth.IsDead())
            rockCollider.enabled = true;

        // Pulse the flash material's emission, ping-ponging the intensity for a flicker.
        if (flashMat != null)
        {
            float intensity = flashMinIntensity
                + Mathf.PingPong(Time.time * flashSpeed, flashMaxIntensity - flashMinIntensity);
            flashMat.SetColor("_EmissionColor", flashBaseEmission * intensity);
        }
    }

    public void OnRockHit()
    {
        if (activated) return;
        activated = true;
        rockCollider.enabled = false;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(bossHurtSfx);
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
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(electricitySfx);

        yield return new WaitForSeconds(electricityToExplosionDelay);

        explosionObject.SetActive(true);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(explosionSfx);
        mainRock.SetActive(false);
        electricityObject.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(whooshSfx);
        yield return StartCoroutine(AnimateSmokeRate(0f, smokePeakRate, smokeRampUpDuration));

        if (bossDefeatTransition != null)
            bossDefeatTransition.TriggerTransition();

        yield return new WaitForSeconds(smokePeakDuration);
        yield return StartCoroutine(AnimateSmokeRate(smokePeakRate, 0f, smokeRampDownDuration));

        onSequenceComplete?.Invoke();
    }

    IEnumerator AnimateSmokeRate(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float rate = Mathf.Lerp(from, to, elapsed / duration);
            foreach (var ps in smokeParticles)
            {
                var em = ps.emission;
                em.rateOverTime = rate;
            }
            yield return null;
        }
        foreach (var ps in smokeParticles)
        {
            var em = ps.emission;
            em.rateOverTime = to;
        }
    }
}
