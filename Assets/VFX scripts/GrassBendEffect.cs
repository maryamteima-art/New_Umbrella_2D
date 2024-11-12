using System.Collections;
using UnityEngine;

public class GrassBendEffect : MonoBehaviour
{
    //Stretch Strength
    public float stretchAmount = 0.2f;
    //Duration of the bend effect
    public float bendDuration = 0.5f;
    //Stores the original scale of the grass sprite
    private Vector3 originalScale;
    private Coroutine bendCoroutine;

    void Start()
    {
        // Store the original scale of the grass particle system
        originalScale = transform.localScale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //If the object entering the trigger is the player, stretch the grass
        if (other.CompareTag("Player"))
        {
            //Calculate the stretch direction based on the player's position
            Vector3 direction = (other.transform.position - transform.position).normalized;

            //Adjust the scale based on the player's direction
            Vector3 targetScale = originalScale + new Vector3(stretchAmount * direction.x, -Mathf.Abs(stretchAmount * direction.y), 0);

            //Apply the bending effect over time
            if (bendCoroutine != null)
                StopCoroutine(bendCoroutine);

            bendCoroutine = StartCoroutine(ApplyBendEffect(targetScale));
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //If the object exiting the trigger is the player, reset the grass's scale
        if (other.CompareTag("Player"))
        {
            // Reset back to its original size
            if (bendCoroutine != null)
                StopCoroutine(bendCoroutine);

            bendCoroutine = StartCoroutine(ApplyBendEffect(originalScale));
        }
    }

    private IEnumerator ApplyBendEffect(Vector3 targetScale)
    {
        //Gradually apply the bending effect
        float elapsed = 0f;
        Vector3 initialScale = transform.localScale;

        while (elapsed < bendDuration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / bendDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        //Ensure the final scale is precisely the target scale
        transform.localScale = targetScale;
    }
}