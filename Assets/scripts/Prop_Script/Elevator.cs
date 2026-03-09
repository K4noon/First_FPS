using UnityEngine;

public class Elevator : MonoBehaviour
{
    public float speed = 2f;
    public Vector3 pointA;
    public Vector3 pointB;

    public bool dir = false;

    // Update is called once per frame
    void Update()
    {
        if (dir) transform.position += Vector3.up * Time.deltaTime * speed;
        else transform.position -= Vector3.up * Time.deltaTime * speed;

        if(Vector3.Distance(transform.position, pointA) < 0.1f && !dir)
        {
            dir = true;
        }
        else if(Vector3.Distance(transform.position, pointB) < 0.1f && dir)
        {
            dir = false;
        }
    }
}
