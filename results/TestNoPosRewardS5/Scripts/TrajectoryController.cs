using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BallControllerX;

public class TrajectoryController : MonoBehaviour
{

    public EnvironmentControllerX environmentController;

    class ghostBall
    {
        public int timesCollidedWithFloor = 0;
        public Vector3 pos;
        public Efecto efecto;
        public float CoefficientRestitution_Floor = 0.76f;
        public float CoefficientRestitution_Wall = 0.55f;
        public float CoefficientRestitution_Slice = 0.65f;
        public float CoefficientRestitution_Topspin = 0.85f;
        public float gravity_slice = -6.5f;
        public float tiempoAcumulado = 0f;
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
            velocityNow.x = velocityTraject.x;
            velocityNow.z = velocityTraject.z;
            float X = recive_force_Position.x + velocityTraject.x * tiempoAcumulado;
            float Y;
            if (efecto == Efecto.Slice)
            {
                velocityNow.y = velocityTraject.y + gravity_slice * tiempoAcumulado;
                Y = recive_force_Position.y + velocityTraject.y * tiempoAcumulado + 0.5f * gravity_slice * Mathf.Pow(tiempoAcumulado, 2);
            }
            else
            {
                velocityNow.y = velocityTraject.y + Physics.gravity.y * tiempoAcumulado;
                Y = recive_force_Position.y + velocityTraject.y * tiempoAcumulado + 0.5f * Physics.gravity.y * Mathf.Pow(tiempoAcumulado, 2);
            }
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
                    checkLocked = true;
                    Vector3 reflectedVelocity = Vector3.Reflect(velocityNow, collisionNormal);
                    ++timesCollidedWithFloor;
                    switch (efecto)
                    {
                        case Efecto.Plano:
                            Vector3 velocity = CoefficientRestitution_Floor * reflectedVelocity;
                            velocity.y = Mathf.Abs(velocity.y);
                            AddForce(velocity);
                            break;
                        case Efecto.Slice:
                            Vector3 velocity_slice = CoefficientRestitution_Slice * reflectedVelocity;
                            velocity_slice.y = Mathf.Abs(velocity_slice.y);
                            AddForce(velocity_slice);
                            break;
                        case Efecto.Topspin:
                            Vector3 velocity_topspin = CoefficientRestitution_Topspin * reflectedVelocity;
                            velocity_topspin.y = Mathf.Abs(velocity_topspin.y);
                            AddForce(velocity_topspin);
                            break;
                    }
                    efecto = Efecto.Plano;
                }
                else if (tag == "Wall")
                {
                    checkLocked = true;
                    Vector3 reflectedVelocity = Vector3.Reflect(velocityNow, collisionNormal);
                    Vector3 velocity = CoefficientRestitution_Wall * reflectedVelocity;
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
        public void AddForce(Vector3 force, Vector3 position, Efecto efecto)
        {
            this.efecto = efecto;
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
    public void SimulateTrajectory(Vector3 pos, Vector3 velocity, Team hitByTeam, Efecto efecto)
    {
        ghostBall GhostBall = new ghostBall();
        GhostBall.CreatePhysicsScene(environmentController.GetCenterField());
        GhostBall.AddForce(velocity, pos, efecto);
        lineRenderer.positionCount = 0;
        if (environmentController.DebugMode)
        {
            lineRenderer.positionCount = maxPhysicsFrameIterations;
        }
        GhostBall.timesCollidedWithFloor = 0;
        float accumulatedTime = 0;
        environmentController.SetSimulationCompleted(false);
        for (var i = 0; i < maxPhysicsFrameIterations; i++)
        {
            GhostBall.updatePos(Time.fixedDeltaTime);
            accumulatedTime += Time.fixedDeltaTime;
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
