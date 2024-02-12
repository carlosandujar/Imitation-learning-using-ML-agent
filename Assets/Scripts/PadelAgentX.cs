using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.VisualScripting;
using System;
using static Unity.VisualScripting.Member;
using UnityEngine.EventSystems;

public class PadelAgentX : Agent
{
    CharacterController characterController;
    private bool canMove = false;
    private Quaternion initialRotation;
    public float speed = 5;

    [HideInInspector]
    public Team team;
    public PlayerId playerId;

    BehaviorParameters behaviorParameters;
    public EnvironmentControllerX environmentController;

    private GameObject mark;
    private Renderer markRenderer;
    private Role role;

    public Rigidbody ballRb;
    public Transform teammateTransform, opponent1Transform, opponent2Transform;
    private bool ballOnRange;

    public override void Initialize()
    {
        behaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        if (behaviorParameters.TeamId == (int)Team.T1)
        {
            team = Team.T1;
        }
        else
        {
            team = Team.T2;
        }
        characterController = gameObject.GetComponent<CharacterController>();
        initialRotation = transform.rotation;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.transform.rotation = initialRotation;
        mark = transform.Find("Mark").gameObject;
        markRenderer = mark.GetComponent<Renderer>();
        markRenderer.enabled = false;
        role = Role.Opponent;
        ballOnRange = false;
        canMove = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float teamSign = this.team == Team.T1 ? 1 : -1;
        sensor.AddObservation(new Vector3 (teamSign * ballRb.transform.localPosition.x, ballRb.transform.localPosition.y, teamSign * ballRb.transform.localPosition.z)); // 3
        sensor.AddObservation(new Vector3 (teamSign * ballRb.velocity.x, ballRb.velocity.y, teamSign * ballRb.velocity.z)); // 3
        sensor.AddObservation(teamSign * transform.localPosition.x);
        sensor.AddObservation(teamSign * transform.localPosition.z);
        sensor.AddObservation(teamSign * teammateTransform.localPosition.x);
        sensor.AddObservation(teamSign * teammateTransform.localPosition.z);
        sensor.AddObservation(teamSign * opponent1Transform.localPosition.x);
        sensor.AddObservation(teamSign * opponent1Transform.localPosition.z);
        sensor.AddObservation(teamSign * opponent2Transform.localPosition.x);
        sensor.AddObservation(teamSign * opponent2Transform.localPosition.z);
        sensor.AddObservation((float)role);
        sensor.AddObservation(ballOnRange && ballRb.transform.localPosition.y > 0.25f && environmentController.GetLastHitByTeam() != team && !environmentController.BallIsLocked());
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Movement
        if (canMove)
        {
            MoveAgent(actionBuffers.DiscreteActions);
        }

        // REWARDS!
        environmentController.CalculateKeyPositionsRelatedRewards(this);
    }

    public void MoveAgent(ActionSegment<int> discreteActions)
    {

        var forwardAxis = discreteActions[0];
        var rightAxis = discreteActions[1];
        var xGrid = discreteActions[2];
        var zGrid = discreteActions[3];
        var hitType = discreteActions[4];

        Vector3 xDirection = Vector3.zero;
        Vector3 zDirection = Vector3.zero;

        switch (forwardAxis)
        {
            case 1:
                zDirection = transform.forward * speed;
                break;
            case 2:
                zDirection = -transform.forward * speed;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                xDirection = transform.right * speed;
                break;
            case 2:
                xDirection = -transform.right * speed;
                break;
        }

        characterController.Move((xDirection + zDirection) * Time.deltaTime);

        float hitHeight = 0;

        bool hitBall = true;
        switch (hitType)
        {
            // No-hit
            case 0:
                hitBall = false;
                break;
            // Derecha
            case 1:
                hitHeight = 2;
                break;
            // Globo
            case 2:
                hitHeight = 4;
                break;
            // Remate
            case 3:
                hitHeight = -1;
                if (ballRb.transform.localPosition.y < 1.5)
                {
                    hitBall = false;
                }
                break;
        }
        if (hitBall)
        {
            Vector3 hitForce = environmentController.CalculateForce(team, hitHeight, xGrid, zGrid);

            if (ballOnRange && ballRb.transform.localPosition.y > 0.25 && environmentController.GetLastHitByTeam() != team && hitForce != Vector3.zero && !environmentController.BallIsLocked() && !environmentController.PointJustGiven())
            {
                environmentController.AddTeamRewards(team, EnvironmentControllerX.HittingBallReward);
                environmentController.HitBall(team, hitForce);
            }
        }
    }

    private int[] movement = null;
    private int[] shot = null;

    public void SetMovement(int[] movement)
    {
        this.movement = movement;
        movementReceived = true;
    }

    public void SetShot(int[] shot)
    {
        this.shot = shot;
        shotReceived = true;
    }

    bool pendingMovementRequest = false;
    bool pendingShotRequest = false;
    bool movementReceived = false;
    bool shotReceived = false;
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (environmentController.RecordingDemonstrations && canMove)
        {
            // MOVEMENT REQUEST
            if (!pendingMovementRequest)
            {
                environmentController.RequestCoachedMovement(
                    playerId,
                    transform.localPosition,
                    teammateTransform.localPosition,
                    opponent1Transform.localPosition,
                    opponent2Transform.localPosition,
                    ballRb.transform.localPosition,
                    environmentController.GetLastHitByTeam());
                pendingMovementRequest = true;
            }
            bool ballIsHittable = ballOnRange && ballRb.transform.localPosition.y > 0.25
                    && environmentController.GetLastHitByTeam() != team && !environmentController.BallIsLocked();
            
            // SHOT REQUEST
            if (!pendingShotRequest && ballIsHittable)
            {
                environmentController.RequestCoachedShot(
                    playerId,
                    transform.localPosition,
                    teammateTransform.localPosition,
                    opponent1Transform.localPosition,
                    opponent2Transform.localPosition,
                    ballRb.transform.localPosition);
                pendingShotRequest = true;
            }

            // MAKE MOVEMENT
            if (movementReceived)
            {
                var discreteActionsOut = actionsOut.DiscreteActions;
                discreteActionsOut[0] = movement[0]; // Forward axis: 0 stay, 1 forward, 2 backwards
                discreteActionsOut[1] = movement[1]; // Right axis: 0 stay, 1 right, 2 left
                movementReceived = false;
                pendingMovementRequest = false;
            }

            // MAKE SHOT
            if (shotReceived)
            {
                var discreteActionsOut = actionsOut.DiscreteActions;
                discreteActionsOut[2] = shot[0]; // xGrid [0..4]
                discreteActionsOut[3] = shot[1]; // zGrid [0..4]
                if (shot[2] == 3 && ballRb.transform.localPosition.y < 1.5)
                {
                    shot[2] = 1;
                }
                discreteActionsOut[4] = shot[2]; // hitType: 0 no hit, 1 derecha/reves, 2 globo, 3 remate
                shotReceived = false;
                pendingShotRequest = false;
                shot[2] = 0;
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        canMove = false;
        pendingMovementRequest = false;
        pendingShotRequest = false;
        movementReceived = false;
        shotReceived = false;
        this.transform.rotation = initialRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && canMove)
        {
            ballOnRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            ballOnRange = false;
        }
    }

    public void StartMoving()
    {
        canMove = true;
    }

    public void StopMoving()
    {
        canMove = false;
    }

    public void SetMarkMaterial(Material material)
    {
        markRenderer.material = material;
    }

    public void SetMarkRendererEnabled(bool enabled)
    {
        markRenderer.enabled = enabled;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public void AssignRole(Role role)
    {
        this.role = role;
    }

    public Role GetRole()
    {
        return role;
    }
}
