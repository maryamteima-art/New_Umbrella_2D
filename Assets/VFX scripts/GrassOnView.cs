using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//PERFORMANCE SCRIPT
//Activates external forces of grass particles that are only in view of camera
//Extrenal forces elements are modified (grass slowly gets pulled down)
public class GrassOnView : MonoBehaviour
{
    // Time to apply external forces before gravity takes over
    public float externalForceDuration = 5.0f;
    //Gravity strength for the particles after external force
    public float maxGravityStrength = 2.0f;
    //Duration (after external forces) where gravity gradually increases
    public float gravityIncreaseDuration = 2.0f;

    //Dictionary to start manipulating the time and gravity of particle system activated
    private Dictionary<ParticleSystem, float> particleTimers = new Dictionary<ParticleSystem, float>();

    void Update()
    {
        foreach (Transform child in transform)
        {
            ParticleSystem particleSystem = child.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                if (IsInView(child.position))
                {
                    //Start the timer for external force duration
                    if (!particleTimers.ContainsKey(particleSystem))
                    {
                        particleTimers[particleSystem] = externalForceDuration;
                    }

                    //Enable external forces to make particles fly initially
                    var externalForces = particleSystem.externalForces;
                    externalForces.enabled = true;

                    if (!particleSystem.isPlaying)
                    {
                        particleSystem.Play();
                    }

                    //Countdown the timer for external force application
                    particleTimers[particleSystem] -= Time.deltaTime;
                    /*
                    //When the external forces timer reaches zero, start applying gravity
                    if (particleTimers[particleSystem] <= 0)
                    {
                        //Disable external forces
                        externalForces.enabled = false; 

                        //Gradual increase gravity for smooth falling effect
                        float gravityRatio = Mathf.Clamp01((Mathf.Abs(particleTimers[particleSystem]) - externalForceDuration)/gravityIncreaseDuration);
                        var mainModule = particleSystem.main;
                        mainModule.gravityModifier = Mathf.Lerp(0, maxGravityStrength, gravityRatio);

                        //Remove the particle system from dictionary once gravity is fully applied
                        if (gravityRatio >= 1.0f)
                        {
                            particleTimers.Remove(particleSystem);
                        }
                    }*/
                }
                else
                {
                    //Pause particle system if out of view
                    if (particleSystem.isPlaying)
                    {
                        particleSystem.Pause();
                    }

                    //Reset timers if particle goes out of view
                    if (particleTimers.ContainsKey(particleSystem))
                    {
                        particleTimers.Remove(particleSystem);
                    }
                }
            }
        }
    }
    //-------- HELPER/CUSTOM FUNCTIONS ---------- 

    //Check if the element's position is within the camera's view
    private bool IsInView(Vector3 position)
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(position);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
               viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
               viewportPoint.z > 0;
    }
}