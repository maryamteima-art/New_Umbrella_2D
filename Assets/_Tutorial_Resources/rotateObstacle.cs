using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateObstacle : MonoBehaviour
{
    public float rotateSpeed;
    private void Update()
    {
        //transform.Rotate(new Vector3(0, rotateSpeed, 0) * Time.deltaTime);
        transform.Rotate(new Vector3(0, 0, rotateSpeed) * Time.deltaTime);

    }
}
