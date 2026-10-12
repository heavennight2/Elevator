using UnityEngine;

public class SimpleAI : MonoBehaviour
{

    
    public enum Emotion { Happy, Sad, Angry, Aloof }

    public Emotion currentState;
    private float timer;

    void Start()
    {
        currentState = Emotion.Happy;
        timer = 0;
    }

    void Update()
    {
        timer += Time.deltaTime;

        switch (currentState)
        {
            case Emotion.Happy:
                OnHappy();
                break;

            case Emotion.Sad:
                OnSad();
                break;

            case Emotion.Angry:
                OnAngry();
                break;

            case Emotion.Aloof:
                OnAloof();
                break;
        }

        if (timer > 1)
        {
            timer = 0;

            int randomNumber = Random.Range(
                0,
                System.Enum.GetNames(typeof(Emotion)).Length
            );

            currentState = (Emotion)randomNumber;
        }
    }

    private void OnHappy()
    {
        Debug.Log("I am Happy");
    }

    private void OnSad()
    {
        Debug.Log("I am Sad");
    }

    private void OnAngry()
    {
        Debug.Log("I am Angry");
    }

    private void OnAloof()
    {
        Debug.Log("I am Aloof");
    }
}