using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallTest : MonoBehaviour
{
    private Vector3 initialPosition;
    public float yMax = 5;
    public float xMax = 3;
    private Transform ballpos;
    public float xAc;
    public float xStart;
    public float yStart;
    public float zStart;
    public float xGrid, zGrid;
    public float CoefficientRestitution;
    private Vector3 previousPos;
    [SerializeField] private float xTarget, zTarget;
    [SerializeField] private Vector3 velocityStart;
    [SerializeField] private Vector3 velocityNow;
    private float tiempoAcumulado = 0f;
    // Start is called before the first frame update
    void Start()
    {
        ballpos = GetComponent<Transform>();
        initialPosition = new Vector3(2.5f, 1, -7);
        zStart = ballpos.localPosition.z;
        xStart = ballpos.localPosition.x;
        yStart = ballpos.localPosition.y;
        velocityStart = CalculateForce(yMax, xGrid, zGrid);
    }

    // Update is called once per frame
    void Update()
    {
        previousPos = ballpos.localPosition;
        tiempoAcumulado += Time.deltaTime;
        velocityNow.x = velocityStart.x + xAc * tiempoAcumulado;
        velocityNow.y = velocityStart.y + (-9.8f) * tiempoAcumulado;
        velocityNow.z = velocityStart.z;
        float X = xStart + velocityStart.x * tiempoAcumulado + 0.5f * xAc * Mathf.Pow(tiempoAcumulado, 2);
        float Y = yStart + velocityStart.y * tiempoAcumulado + 0.5f * (-9.8f) * Mathf.Pow(tiempoAcumulado, 2);
        float Z = zStart + velocityStart.z * tiempoAcumulado;
        ballpos.localPosition = new Vector3(X, Y, Z);
    }
    public Vector3 CalculateForce(float yMax, float xGrid, float zGrid)
    {
        xTarget = -2 * (10f / 6) + (10f / 6) * xGrid;
        zTarget = (10f / 6 * 5) - (10f / 6) * zGrid;
        if (yMax < yStart) yMax = yStart;
        float yVelocity = Mathf.Sqrt((yStart - yMax) * 2 * (Physics.gravity.y));
        float t = (-yVelocity - Mathf.Sqrt(yVelocity * yVelocity - 2 * Physics.gravity.y * yStart)) / Physics.gravity.y;
        if (t < 0)
            t = (-yVelocity + Mathf.Sqrt(yVelocity * yVelocity - 2 * Physics.gravity.y * yStart)) / Physics.gravity.y;
        Debug.Log(t);
        float zVelocity = (zTarget - zStart) / t;
        float xVelocity =  (xTarget - xStart - 0.5f*xAc*Mathf.Pow(t,2))/t;
        return new Vector3(xVelocity, yVelocity, zVelocity);
    }
    private void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;
        if (tag == "PlayerT1")
        {

        }
        else if (tag == "PlayerT2")
        {
           
        }
        else if (tag == "Net")
        {
            Vector3 incomingVector = ballpos.localPosition - previousPos;
            Vector3 collisionNormal = collision.contacts[0].normal;
            Vector3 reflectedVelocity = Vector3.Reflect(velocityNow, collisionNormal);
            Debug.Log(reflectedVelocity);
            velocityStart = CoefficientRestitution * reflectedVelocity;
            velocityStart.y = Mathf.Abs(velocityStart.y);
            xStart = ballpos.localPosition.x;
            yStart = ballpos.localPosition.y;
            zStart = ballpos.localPosition.z;
            tiempoAcumulado = 0;

        }
        else if (tag == "Floor")
        {
            xAc = CoefficientRestitution * xAc;
            Vector3 incomingVector = ballpos.localPosition - previousPos;
            Vector3 collisionNormal = collision.contacts[0].normal;
            Vector3 reflectedVelocity = Vector3.Reflect(velocityNow, collisionNormal);
            Debug.Log(reflectedVelocity);
            velocityStart = CoefficientRestitution * reflectedVelocity;
            velocityStart.y = Mathf.Abs(velocityStart.y);
            xStart = ballpos.localPosition.x;
            yStart = ballpos.localPosition.y;
            zStart = ballpos.localPosition.z;
            tiempoAcumulado = 0;
        }
        else if (tag == "Wall")
        {
            xAc = 0;
            Vector3 incomingVector = ballpos.localPosition - previousPos;
            Vector3 collisionNormal = collision.contacts[0].normal;
            Vector3 reflectedVelocity = Vector3.Reflect(velocityNow, collisionNormal);
            Debug.Log(reflectedVelocity);
            velocityStart = CoefficientRestitution * reflectedVelocity;
            velocityStart.y = Mathf.Abs(velocityStart.y);
            xStart = ballpos.localPosition.x;
            yStart = ballpos.localPosition.y;
            zStart = ballpos.localPosition.z;
            tiempoAcumulado = 0;

        }
    }
}
