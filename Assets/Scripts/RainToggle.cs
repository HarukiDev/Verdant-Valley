using UnityEngine;
using UnityEngine.Rendering.Universal; // Untuk Global Light 2D (URP)

public class RainToggle : MonoBehaviour
{
    public GameObject[] rainEffects;
    public float initialDelay = 30f; // Delay awal sebelum hujan pertama (detik)
    public float rainDuration = 300f; // Durasi hujan 5 menit (300 detik)
    public float minDryDelay = 20f; // Minimal waktu kering
    public float maxDryDelay = 100f; // Maksimal waktu kering

    // Untuk pengaturan pencahayaan
    public Light2D globalLight; // Referensi ke Global Light 2D
    public float normalLightIntensity = 1f; // Intensitas cahaya normal (cerah)
    public float rainLightIntensity = 0.5f; // Intensitas cahaya saat hujan (gelap)
    public float transitionDuration = 2f; // Durasi transisi (dalam detik)

    private ParticleSystem rainParticles;

    void Awake()
    {
        rainParticles = GetComponentInChildren<ParticleSystem>();
        if (rainParticles != null)
        {
            rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void Start()
    {
        // Pastikan Global Light 2D diatur ke intensitas normal saat start
        if (globalLight != null)
        {
            globalLight.intensity = normalLightIntensity;
        }
        StartCoroutine(ToggleRain());
    }

    System.Collections.IEnumerator ToggleRain()
    {
        // Delay awal sebelum hujan pertama
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            // Mulai hujan
            rainParticles.Play();
            // Transisi ke gelap
            yield return StartCoroutine(ChangeLightIntensity(rainLightIntensity));

            // Tunggu selama durasi hujan
            yield return new WaitForSeconds(rainDuration);

            // Stop hujan
            rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            // Transisi kembali ke cerah
            yield return StartCoroutine(ChangeLightIntensity(normalLightIntensity));

            // Tunggu waktu kering random
            float dryTime = Random.Range(minDryDelay, maxDryDelay);
            yield return new WaitForSeconds(dryTime);
        }
    }

    // Coroutine untuk transisi intensitas cahaya
    System.Collections.IEnumerator ChangeLightIntensity(float targetIntensity)
    {
        if (globalLight == null) yield break;

        float startIntensity = globalLight.intensity;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            yield return null;
        }

        // Pastikan intensitas tepat di nilai target
        globalLight.intensity = targetIntensity;
    }
}