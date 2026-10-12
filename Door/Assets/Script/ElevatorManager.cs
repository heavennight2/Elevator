
using UnityEngine;
using System.Collections;

public class ElevatorManager : MonoBehaviour
{
    public Animator elevatorAnimator;
    public float waitTime = 10f;

    void Start()
    {
        StartCoroutine(ElevatorLoop());
    }

    IEnumerator ElevatorLoop()
    {
        while (true)
        {
            // Wait before reaching the next floor
            yield return new WaitForSeconds(waitTime);

            // Play the door opening and closing animation
            elevatorAnimator.Play("Animation", 0, 0f);

            // Wait until the whole door animation finishes
            yield return new WaitForSeconds(12.5f);
        }
    }
}
