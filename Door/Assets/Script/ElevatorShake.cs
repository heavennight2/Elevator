
using UnityEngine;
using System.Collections;

public class ElevatorShake : MonoBehaviour
{
    public float intensity = 0.008f;
    public float speed = 18f;

    private Vector3 originalPosition;
    private Coroutine shakeRoutine;

    void Start()
    {
        originalPosition = transform.localPosition;
        StartShaking();
    }

    public void StartShaking()
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(Shake());
    }

    public void StopShaking()
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        transform.localPosition = originalPosition;
    }

    IEnumerator Shake()
    {
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime;

            float x = Mathf.Sin(time * speed) * intensity;
            float y = Mathf.Sin(time * speed * 1.3f)
                * intensity * 0.5f;

            transform.localPosition =
                originalPosition + new Vector3(x, y, 0f);

            yield return null;
        }
    }

    void OnDisable()
    {
        StopShaking();
    }
}
