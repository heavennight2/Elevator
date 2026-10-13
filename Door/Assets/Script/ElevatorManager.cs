
using UnityEngine;
using System.Collections;

public class ElevatorManager : MonoBehaviour
{
    [Header("Elevator")]
    public Animator elevatorAnimator;
    public string doorAnimationName = "Animation";

    [Header("Worlds")]
    public GameObject[] floors;
    public Material[] skyboxes;
    public AudioSource[] worldAudioSources;
    public Light[] worldLights;

    [Header("Testing")]
    public bool testMode = true;
    public int testFloorIndex = 3;

    [Header("Door Timing")]
    public float waitTime = 10f;
    public float openingTime = 6f;
    public float openHoldTime = 15f;
    public float animationDuration = 12.5f;

    [Range(0f, 1f)]
    public float fullyOpenPoint = 0.48f;

    [Header("Fade Settings")]
    public float audioFadeDuration = 2f;
    public float lightFadeDuration = 2f;

    [Header("Backrooms Monster")]
    public int backroomsIndex = 3;
    public MonsterRun monster;
    public float monsterWaitTime = 3f;
    public float monsterChaseTime = 5f;

    private float[] originalLightIntensities;
    private float[] originalAudioVolumes;
    private Material originalSkybox;

    void Start()
    {
        if (elevatorAnimator == null || floors.Length == 0)
        {
            Debug.LogError("Assign Elevator Animator and Floors!");
            return;
        }

        originalSkybox = RenderSettings.skybox;

        originalLightIntensities = new float[worldLights.Length];
        originalAudioVolumes = new float[worldAudioSources.Length];

        for (int i = 0; i < worldLights.Length; i++)
        {
            if (worldLights[i] != null)
            {
                originalLightIntensities[i] =
                    worldLights[i].intensity;

                worldLights[i].enabled = false;
            }
        }

        for (int i = 0; i < worldAudioSources.Length; i++)
        {
            if (worldAudioSources[i] != null)
            {
                AudioSource audio = worldAudioSources[i];

                originalAudioVolumes[i] = audio.volume;
                audio.playOnAwake = false;
                audio.loop = true;
                audio.Stop();
            }
        }

        foreach (GameObject floor in floors)
        {
            if (floor != null)
                floor.SetActive(false);
        }

        // Keep doors at the beginning of animation
        elevatorAnimator.speed = 0f;
        elevatorAnimator.Play(doorAnimationName, 0, 0f);
        elevatorAnimator.Update(0f);

        StartCoroutine(ElevatorLoop());
    }

    IEnumerator ElevatorLoop()
    {
        while (true)
        {
            // Elevator waits before next world
            yield return new WaitForSeconds(waitTime);

            int index = testMode
                ? Mathf.Clamp(testFloorIndex, 0, floors.Length - 1)
                : Random.Range(0, floors.Length);

            GameObject selectedFloor = floors[index];

            if (selectedFloor == null)
                continue;

            // Prepare light
            Light selectedLight = null;
            float targetIntensity = 0f;

            if (index < worldLights.Length)
            {
                selectedLight = worldLights[index];

                if (selectedLight != null)
                {
                    targetIntensity =
                        originalLightIntensities[index];

                    selectedLight.intensity = 0f;
                    selectedLight.enabled = false;
                }
            }

            // Prepare audio
            AudioSource selectedAudio = null;
            float targetVolume = 1f;

            if (index < worldAudioSources.Length)
            {
                selectedAudio = worldAudioSources[index];

                if (selectedAudio != null)
                    targetVolume = originalAudioVolumes[index];
            }

            // Show selected world
            selectedFloor.SetActive(true);

            if (index == backroomsIndex && monster != null)
                monster.ResetMonster();

            // Change skybox
            if (index < skyboxes.Length &&
                skyboxes[index] != null)
            {
                RenderSettings.skybox = skyboxes[index];
                DynamicGI.UpdateEnvironment();
            }

            // START OPENING DOORS
            elevatorAnimator.speed = 1f;
            elevatorAnimator.Play(doorAnimationName, 0, 0f);
            elevatorAnimator.Update(0f);

            // Start audio
            if (selectedAudio != null)
            {
                selectedAudio.Stop();
                selectedAudio.volume = targetVolume;
                selectedAudio.Play();
            }

            // Fade in light
            if (selectedLight != null)
            {
                selectedLight.intensity = 0f;
                selectedLight.enabled = true;

                StartCoroutine(
                    FadeLight(
                        selectedLight,
                        targetIntensity,
                        lightFadeDuration
                    )
                );
            }

            // Wait for opening animation
            yield return new WaitForSeconds(openingTime);

            // FORCE doors to fully open position
            elevatorAnimator.speed = 0f;
            elevatorAnimator.Play(
                doorAnimationName,
                0,
                fullyOpenPoint
            );
            elevatorAnimator.Update(0f);

            Debug.Log("DOORS OPEN - HOLDING");

            // Backrooms event
            if (index == backroomsIndex && monster != null)
            {
                float delay = Mathf.Min(
                    monsterWaitTime,
                    openHoldTime
                );

                yield return new WaitForSeconds(delay);

                if (delay < openHoldTime)
                    monster.StartRunning();

                float chase = Mathf.Min(
                    monsterChaseTime,
                    Mathf.Max(0f, openHoldTime - delay)
                );

                yield return new WaitForSeconds(chase);

                monster.StopRunning();

                float remaining = Mathf.Max(
                    0f,
                    openHoldTime - delay - chase
                );

                yield return new WaitForSeconds(remaining);
            }
            else
            {
                yield return new WaitForSeconds(openHoldTime);
            }

            Debug.Log("DOORS CLOSING");

            // START CLOSING DOORS
            elevatorAnimator.speed = 1f;

            // Fade out sound
            Coroutine audioFade = null;
            Coroutine lightFade = null;

            if (selectedAudio != null)
            {
                audioFade = StartCoroutine(
                    FadeAudio(
                        selectedAudio,
                        audioFadeDuration
                    )
                );
            }

            if (selectedLight != null)
            {
                lightFade = StartCoroutine(
                    FadeLight(
                        selectedLight,
                        0f,
                        lightFadeDuration
                    )
                );
            }

            // Resume animation from fully open point
            float closingTime = Mathf.Max(
                0f,
                animationDuration * (1f - fullyOpenPoint)
            );

            yield return new WaitForSeconds(closingTime);

            if (audioFade != null)
                yield return audioFade;

            if (lightFade != null)
                yield return lightFade;

            // Stop sound
            if (selectedAudio != null)
            {
                selectedAudio.Stop();
                selectedAudio.volume = targetVolume;
            }

            // Turn off light
            if (selectedLight != null)
            {
                selectedLight.enabled = false;
                selectedLight.intensity = targetIntensity;
            }

            // Hide world
            selectedFloor.SetActive(false);

            // Restore skybox
            RenderSettings.skybox = originalSkybox;

            // Reset doors to closed position
            elevatorAnimator.speed = 0f;
            elevatorAnimator.Play(doorAnimationName, 0, 0f);
            elevatorAnimator.Update(0f);
        }
    }

    IEnumerator FadeAudio(AudioSource audio, float duration)
    {
        float startVolume = audio.volume;

        if (duration <= 0f)
        {
            audio.Stop();
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            audio.volume = Mathf.Lerp(
                startVolume,
                0f,
                Mathf.Clamp01(timer / duration)
            );

            yield return null;
        }

        audio.Stop();
        audio.volume = startVolume;
    }

    IEnumerator FadeLight(
        Light lightSource,
        float targetIntensity,
        float duration
    )
    {
        float startIntensity = lightSource.intensity;

        if (duration <= 0f)
        {
            lightSource.intensity = targetIntensity;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            lightSource.intensity = Mathf.Lerp(
                startIntensity,
                targetIntensity,
                Mathf.Clamp01(timer / duration)
            );

            yield return null;
        }

        lightSource.intensity = targetIntensity;
    }
}
