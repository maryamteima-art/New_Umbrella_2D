using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//As object collide with the clouds, they will disperse outwards
public class CloudDisperse : MonoBehaviour
{
    public GameObject cloudParticleSystem; 
    public float disperseForce = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            //Calculate the player's velocity
            Rigidbody playerRigidbody = other.GetComponent<Rigidbody>();
            float playerVelocityMagnitude = playerRigidbody != null ? playerRigidbody.velocity.magnitude : 0;

            //Calculate the direction from the cloud to the player
            Vector3 direction = other.transform.position - transform.position;
            direction.Normalize();

            //Instantiate dispersed cloud effect
            GameObject dispersedCloud = Instantiate(cloudParticleSystem, transform.position, Quaternion.identity);

            //Calculate disperse force based on player velocity
            float newDisperseForce = disperseForce * playerVelocityMagnitude;
            dispersedCloud.GetComponent<Rigidbody>().AddForce(direction * newDisperseForce, ForceMode.Impulse);
        }
    }
}
