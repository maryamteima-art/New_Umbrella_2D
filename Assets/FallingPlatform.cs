using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//https://www.youtube.com/watch?v=fAuZl8K85qs
public class FallingPlatform : MonoBehaviour
{
    Rigidbody2D platformBody;
    Vector2 initalPos;

    [SerializeField] float fallDelay, respawnTime;


    // Start is called before the first frame update
    void Start()
    {
        initalPos = transform.position;
        platformBody = GetComponent <Rigidbody2D> ();

    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            StartCoroutine(PlatformDrop());
        }

    }

    IEnumerator PlatformDrop()
    {
        yield return new WaitForSeconds(fallDelay);
        platformBody.bodyType = RigidbodyType2D.Dynamic;
        yield return new WaitForSeconds(respawnTime);
        ResetPos();

    }

    private void ResetPos()
    {
        platformBody.bodyType = RigidbodyType2D.Static;
        transform.position = initalPos;

    }

}
