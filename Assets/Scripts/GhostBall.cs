using System.Threading;
using UnityEngine;

public class GhostBall : MonoBehaviour
{
    private Rigidbody rb;
    private int timesCollidedWithFloor = 0;
    private bool initialized = false;
    private Vector3 initialPosition;

    public void Init(Vector3 velocity)
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = rb.transform.localPosition;
        rb.AddForce(velocity, ForceMode.VelocityChange);
        initialized = true;
    }

    public void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;
        if (tag == "Floor" && initialized && Vector3.Distance(initialPosition, rb.transform.localPosition) > 0.1)
        {
            timesCollidedWithFloor++;
        }
    }
    public void SetTimesCollidedWithFloor(int times)
    {
        timesCollidedWithFloor = times;
    }
    public int GetTimesCollidedWithFloor()
    {
        return timesCollidedWithFloor;
    }
}