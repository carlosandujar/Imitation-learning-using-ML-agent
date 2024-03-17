using System.Threading;
using UnityEngine;

public class GhostBall : MonoBehaviour
{
    private Rigidbody rb;
    private int timesCollidedWithFloor = 0;
    private bool initialized = false;
    private Vector3 initialPosition;

    //Speed and position 
    public float CoefficientRestitution = 0.76f;
    private float tiempoAcumulado = 0f;
    public float xAcceleration; // aceleration on x axis
    private Vector3 recive_force_Position;
    [SerializeField] private float xTarget, zTarget;
    [SerializeField] private Vector3 velocityTraject;
    [SerializeField] private Vector3 velocityNow;
    private bool checkLocked = false;

    //Padel Field
    public Vector3 centerField;
    public float radiusBall = 0.25f;
    private float netHeight = 1f;
    public Transform T1_1, T1_2, T2_1, T2_2;
    private float limX_neg;
    private float limX_pos;
    private float limZ_neg;
    private float limZ_pos;
    private float heightPlayer = 1.75f;
    public void Init(Vector3 velocity)
    {
        Debug.Log("init");
        rb = GetComponent<Rigidbody>();
        AddForce(velocity);
        initialPosition = rb.transform.localPosition;
        initialized = true;

    }
    private void FixedUpdate()
    {
        if (initialized)
        {
            CheckCollision();
            tiempoAcumulado += Time.fixedDeltaTime;
            velocityNow.x = velocityTraject.x + xAcceleration * tiempoAcumulado;
            velocityNow.y = velocityTraject.y + (-9.8f) * tiempoAcumulado;
            velocityNow.z = velocityTraject.z;
            float X = recive_force_Position.x + velocityTraject.x * tiempoAcumulado + 0.5f * xAcceleration * Mathf.Pow(tiempoAcumulado, 2);
            float Y = recive_force_Position.y + velocityTraject.y * tiempoAcumulado + 0.5f * (-9.8f) * Mathf.Pow(tiempoAcumulado, 2);
            float Z = recive_force_Position.z + velocityTraject.z * tiempoAcumulado;
            Debug.Log(new Vector3(X, Y, Z));
            rb.transform.localPosition = new Vector3(X, Y, Z);
        }
    }

    private void CheckCollision()
    {
        Vector3 posBall = rb.transform.localPosition;
        Vector3 collisionNormal = new Vector3();
        string tag = "";
        float distanceNet = Vector3.Distance(new Vector3(0, 0, posBall.z), new Vector3(0, 0, centerField.z));
        // Floor

        if (posBall.y <= radiusBall)
        {
            tag = "Floor";
            collisionNormal = new Vector3(0, 1, 0);
        }
        else if (posBall.x - radiusBall <= limX_neg)
        {
            tag = "Wall";
            collisionNormal = new Vector3(1, 0, 0);
        }
        else if (posBall.x + radiusBall >= limX_pos)
        {
            tag = "Wall";
            collisionNormal = new Vector3(-1, 0, 0);
        }
        else if (posBall.z - radiusBall <= limZ_neg)
        {
            tag = "Wall";
            collisionNormal = new Vector3(0, 0, 1);
        }
        else if (posBall.z + radiusBall >= limZ_pos)
        {
            tag = "Wall";
            collisionNormal = new Vector3(0, 0, -1);
        }
        else
        {
            checkLocked = false;
        }

        if (!checkLocked)
        {
            // Calculation
            if (tag == "Floor")
            {
                xAcceleration = CoefficientRestitution * xAcceleration;
                Vector3 reflectedVelocity = Vector3.Reflect(velocityNow, collisionNormal);
                ++timesCollidedWithFloor;
                Vector3 velocity = CoefficientRestitution * reflectedVelocity;
                velocity.y = Mathf.Abs(velocity.y);
                AddForce(velocity);
            }
            else if (tag == "Wall")
            {
                checkLocked = true;
                xAcceleration = 0;
                Vector3 reflectedVelocity = Vector3.Reflect(velocityNow, collisionNormal);
                Vector3 velocity = CoefficientRestitution * reflectedVelocity;
                velocity.y = Mathf.Abs(velocity.y);
                AddForce(velocity);
            }
        }
    }
    public void AddForce(Vector3 force)
    {
        velocityTraject = force;
        recive_force_Position = rb.transform.localPosition;
        tiempoAcumulado = 0;
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