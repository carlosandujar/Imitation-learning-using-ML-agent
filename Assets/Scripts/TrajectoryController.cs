using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrajectoryController : MonoBehaviour
{

    public EnvironmentControllerX environmentController;

    class ghostBall
    {
        public int timesCollidedWithFloor = 0;
        public Vector3 pos;
        public float CoefficientRestitution = 0.76f;
        public float tiempoAcumulado = 0f;
        public float xAcceleration; // aceleration on x axis
        public Vector3 recive_force_Position;
        public float xTarget, zTarget;
        public Vector3 velocityTraject;
        public Vector3 velocityNow;
        public bool checkLocked = false;

        // field
        public Vector3 centerField;
        public float radiusBall = 0.25f;
        private float netHeight = 1f;
        private float limX_neg;
        private float limX_pos;
        private float limZ_neg;
        private float limZ_pos;

        public ghostBall()
        {

        }

        public void CreatePhysicsScene(Vector3 center)
        {
            centerField = center;
            limX_neg = centerField.x - 5f;
            limX_pos = centerField.x + 5f;
            limZ_neg = centerField.z - 10f;
            limZ_pos = centerField.z + 10f;
        }
        public void updatePos(float time)
        {
            CheckCollision();
            tiempoAcumulado += time;
            velocityNow.x = velocityTraject.x + xAcceleration * tiempoAcumulado;
            velocityNow.y = velocityTraject.y + (-9.8f) * tiempoAcumulado;
            velocityNow.z = velocityTraject.z;
            float X = recive_force_Position.x + velocityTraject.x * tiempoAcumulado + 0.5f * xAcceleration * Mathf.Pow(tiempoAcumulado, 2);
            float Y = recive_force_Position.y + velocityTraject.y * tiempoAcumulado + 0.5f * (-9.8f) * Mathf.Pow(tiempoAcumulado, 2);
            float Z = recive_force_Position.z + velocityTraject.z * tiempoAcumulado;
            pos = new Vector3(X, Y, Z);
        }
        private void CheckCollision()
        {
            Vector3 posBall = pos;
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
            recive_force_Position = pos;
            tiempoAcumulado = 0;
        }
        public void AddForce(Vector3 force, Vector3 position)
        {
            velocityTraject = force;
            pos = position;
            recive_force_Position = position;
            tiempoAcumulado = 0;
        }
    }

    void Start()
    {
    }
   
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int maxPhysicsFrameIterations;

    // NOTE: Whatever is instantiated in the physics scene is in world space: there are no parent transforms.
    public void SimulateTrajectory(Vector3 pos, Vector3 velocity, Team hitByTeam)
    {
        ghostBall GhostBall = new ghostBall();
        GhostBall.CreatePhysicsScene(environmentController.GetCenterField());
        GhostBall.AddForce(velocity, pos);
        lineRenderer.positionCount = 0;
        if (environmentController.DebugMode)
        {
            lineRenderer.positionCount = maxPhysicsFrameIterations;
        }
        GhostBall.timesCollidedWithFloor = 0;
        float accumulatedTime = 0;
        for (var i = 0; i < maxPhysicsFrameIterations; i++)
        {
            GhostBall.updatePos(Time.fixedDeltaTime);
            environmentController.AnalyzeKeyPosition(GhostBall.pos, accumulatedTime, hitByTeam);
            if (environmentController.DebugMode && lineRenderer.positionCount > 0)
            {
                lineRenderer.SetPosition(i, GhostBall.pos);
            }
            if (GhostBall.timesCollidedWithFloor >= 2)
            {
                lineRenderer.positionCount = i;
                break;
            }
        }
        environmentController.SetSimulationCompleted(true);
    }

    public void ClearTrajectory()
    {
        lineRenderer.positionCount = 0;
    }
}
