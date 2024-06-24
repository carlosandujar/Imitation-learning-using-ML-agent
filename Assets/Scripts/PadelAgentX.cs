using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.VisualScripting;
using System;
using System.IO;
using static Unity.VisualScripting.Member;
using UnityEngine.EventSystems;
using System.Globalization;
using System.Diagnostics;

public class PadelAgentX : Agent
{
    CharacterController characterController;
    private bool canMove = false;
    private Quaternion initialRotation;
    public float Maxspeed = 5;

    [HideInInspector]
    public Team team;
    public PlayerId playerId;

    BehaviorParameters behaviorParameters;
    public EnvironmentControllerX environmentController;

    private GameObject mark;
    private Renderer markRenderer;
    private Role role;

    public Rigidbody ballRb;
    public BallControllerX ballController;
    public Transform teammateTransform, opponent1Transform, opponent2Transform;
    private bool ballOnRange;
    public Vector3 previousPos;



    public bool speedControl = false;
    public bool heightControl = false;
    public override void Initialize()
    {
        behaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        ballController = GameObject.Find("Ball").GetComponent<BallControllerX>();
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
        // must match VectorObservationSize in Initialize
        float teamSign = this.team == Team.T1 ? 1 : -1;
        Vector3 p = new Vector3(teamSign * ballRb.transform.localPosition.x, ballRb.transform.localPosition.y, teamSign * ballRb.transform.localPosition.z);
        List<float> observations = new();
        observations.Add(p.x);
        observations.Add(p.y);
        observations.Add(p.z);
        Vector3 vel = new Vector3(teamSign * ballRb.velocity.x, ballRb.velocity.y, teamSign * ballRb.velocity.z);
        observations.Add(vel.x);
        observations.Add(vel.y);
        observations.Add(vel.z);
        observations.Add(teamSign * transform.localPosition.x);
        observations.Add(teamSign * transform.localPosition.z);
        observations.Add(teamSign * teammateTransform.localPosition.x);
        observations.Add(teamSign * teammateTransform.localPosition.z);
        observations.Add(teamSign * opponent1Transform.localPosition.x);
        observations.Add(teamSign * opponent1Transform.localPosition.z);
        observations.Add(teamSign * opponent2Transform.localPosition.x);
        observations.Add(teamSign * opponent2Transform.localPosition.z); // ideally, opponents should be assigned to 1, 2 according to their current lateral position 
        observations.Add((float)role);
        bool hitable = ballOnRange && ballRb.transform.localPosition.y > 0.25 && environmentController.GetLastHitByTeam() != team && !environmentController.BallIsLocked();
        observations.Add(hitable ? 1f : 0f);
        observations.Add((int)ballController.efecto);

        //float teamSign = this.team == Team.T1 ? 1 : -1;
        //sensor.AddObservation(new Vector3 (teamSign * ballRb.transform.localPosition.x, ballRb.transform.localPosition.y, teamSign * ballRb.transform.localPosition.z)); // 3
        //sensor.AddObservation(new Vector3 (teamSign * ballRb.velocity.x, ballRb.velocity.y, teamSign * ballRb.velocity.z)); // 3
        //sensor.AddObservation(teamSign * transform.localPosition.x);
        //sensor.AddObservation(teamSign * transform.localPosition.z);
        //sensor.AddObservation(teamSign * teammateTransform.localPosition.x);
        //sensor.AddObservation(teamSign * teammateTransform.localPosition.z);
        //sensor.AddObservation(teamSign * opponent1Transform.localPosition.x);
        //sensor.AddObservation(teamSign * opponent1Transform.localPosition.z);
        //sensor.AddObservation(teamSign * opponent2Transform.localPosition.x);
        //sensor.AddObservation(teamSign * opponent2Transform.localPosition.z);
        //sensor.AddObservation((float)role);
        //sensor.AddObservation(ballOnRange && ballRb.transform.localPosition.y > 0.25f && environmentController.GetLastHitByTeam() != team && !environmentController.BallIsLocked());

        /*
        * Receiver keypoints
        * */
        bool oneIsValid = false;
        float validMargin = 0.0f;
        Vector3 validPos = new Vector3(0, 0, 0);
        for (int i = 0; i < environmentController.keyPositionsController.receiverKeyPositions.Length; i++)
        {
            if (environmentController.keyPositionsController.receiverKeyPositions[i].timeMargin > 0)
            {
                oneIsValid = true;
                validPos = environmentController.keyPositionsController.receiverKeyPositions[i].position;
                validMargin = environmentController.keyPositionsController.receiverKeyPositions[i].timeMargin;
            }
        }
        if (!oneIsValid)
        {
            // TODO: one default pos
            validPos = transform.localPosition;
        }

        //for (int i = 0; i < environmentController.keyPositionsController.receiverKeyPositions.Length; i++)
        //{
        //    Vector3 pos = environmentController.keyPositionsController.receiverKeyPositions[i].position;
        //    float time = environmentController.keyPositionsController.receiverKeyPositions[i].timeMargin;

        //    if (time < 0)
        //    {
        //        pos = validPos;
        //        time = validMargin;
        //    }
        //    pos.x *= teamSign;
        //    pos.z *= teamSign;
        //    observations.Add(time);
        //    observations.Add(pos.x);
        //    observations.Add(pos.y);
        //    observations.Add(pos.z);

        //}

        // we could add:
        // type of shot (as oneHotObservation)
        // posx, posy of most probable feasible return position
        //Debug.Log("Sensor size:" + sensor.ObservationSize().ToString());

        // Add all collected observations
        for (int i = 0; i < observations.Count; i++)
            sensor.AddObservation(observations[i]);
        // Logger  
        if (environmentController.logEnabled && environmentController.environmentId == 0)
        {
            if (this.team == Team.T1 && this.playerId == PlayerId.T1_1)  // T1_1 saves the states for all players
            {
                environmentController.logger.LogState(
                    new Vector3[] { transform.localPosition, teammateTransform.localPosition, opponent1Transform.localPosition, opponent2Transform.localPosition }, ballRb.transform.localPosition);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Movement
        if (canMove)
        {
            MoveAgent(actionBuffers.DiscreteActions, actionBuffers.ContinuousActions);
        }
        // REWARDS!
        environmentController.CalculateKeyPositionsRelatedRewards(this);
    }

    public void MoveAgent(ActionSegment<int> discreteActions, ActionSegment<float> ContinuousActions)
    {
        var targetX = discreteActions[0];
        var targetZ = discreteActions[1];
        var xGrid = discreteActions[2];
        var zGrid = discreteActions[3];
        var hitType = discreteActions[4];


        float xTarget = -2 * (10f / 6) + (10f / 6) * targetX;
        float zTarget = -((10f / 6) + (10f / 6) * targetZ);

        
        if (team == Team.T2)
        {
            zTarget *= -1;
            xTarget *= -1;
        }

        float ballTargetX = -2 * (10f / 6) + (10f / 6) * xGrid;
        float ballTargetZ = (10f / 6 * 5) - (10f / 6) * zGrid;
        if (team == Team.T2)
        {
            ballTargetX *= -1;
            ballTargetZ *= -1;
        }

        // Log Action
        if (environmentController.logEnabled && environmentController.environmentId == 0)
        {
            environmentController.logger.LogAction((int)this.team, (int)this.playerId,  xTarget, zTarget, ballTargetX, ballTargetZ, hitType, 
                    new Vector3[] { transform.localPosition, teammateTransform.localPosition, opponent1Transform.localPosition, opponent2Transform.localPosition });
        }



        Vector3 targetPos = new Vector3(xTarget,0f, zTarget);
        Vector3 posPlayer = transform.localPosition;
        posPlayer.y = 0f;

        Vector3 direction = targetPos - posPlayer;
        float magnitude = direction.magnitude;
        direction /= magnitude;
        direction.y = 0f;

        float speed = Maxspeed;

        if (speedControl)
        {
            float range = ContinuousActions[0];
            speed = ((range / 2f) + 0.5f) * Maxspeed;
            float reward = EnvironmentControllerX.SpeedReward * (1f - speed / Maxspeed);
            AddReward(reward);
        }
        Vector3 movement = direction * speed;
        characterController.Move(movement * Time.fixedDeltaTime);

        if (characterController.transform.localPosition.y > 0.875f)
        {
            Vector3 newPosition = this.transform.localPosition;
            newPosition.y = 0.875f;
            this.transform.localPosition = newPosition;
        }

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
                if (heightControl)
                {
                    if (speedControl)
                    {
                        hitHeight = Mathf.Clamp(ContinuousActions[1], 1.25f, 4);
                    }
                    else hitHeight = Mathf.Clamp(ContinuousActions[0], 1.25f, 4);
                } else hitHeight = 1.5f;
                break;
            // Slice
            case 2:
                if (heightControl)
                {
                    if (speedControl)
                    {
                        hitHeight = Mathf.Clamp(ContinuousActions[1], 1.25f, 4);
                    }
                    else hitHeight = Mathf.Clamp(ContinuousActions[0], 1.25f, 4);
                } else hitHeight = 2;
                break;
            // top spin
            case 3:
                if (heightControl)
                {
                    if (speedControl)
                    {
                        hitHeight = Mathf.Clamp(ContinuousActions[1], 1.25f, 4);
                    }
                    else hitHeight = Mathf.Clamp(ContinuousActions[0], 1.25f, 4);
                }
                else hitHeight = 2;
                break;
            // Globo
            case 4:
                hitHeight = 4;
                break;
            // Remate
            case 5:
                hitHeight = -1;
                if (ballRb.transform.localPosition.y < 1.5f)
                {
                    hitBall = false;
                }
                break;
        }
        if (hitBall)
        {
            Vector3 hitForce = environmentController.CalculateForce(team, hitHeight, xGrid, zGrid, hitType);
            if (ballOnRange && ballRb.transform.localPosition.y > 0.25 && environmentController.GetLastHitByTeam() != team && hitForce != Vector3.zero && !environmentController.BallIsLocked()  && !environmentController.PointJustGiven())
            {
                environmentController.AddTeamRewards(team, EnvironmentControllerX.HittingBallReward);
                environmentController.HitBall(team, hitForce, hitType);
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
                discreteActionsOut[0] = movement[0]; // xgrid [0..4]
                discreteActionsOut[1] = movement[1]; // zgrid [0..4]
                movementReceived = false;
                pendingMovementRequest = false;
            }

            // MAKE SHOT
            if (shotReceived)
            {
                var discreteActionsOut = actionsOut.DiscreteActions;
                discreteActionsOut[2] = shot[0]; // xGrid [0..4]
                discreteActionsOut[3] = shot[1]; // zGrid [0..4]
                if (shot[2] == 4 && ballRb.transform.localPosition.y < 1.5)
                {
                    shot[2] = 1;
                }
                discreteActionsOut[4] = shot[2]; // hitType: 0 no hit, 1 derecha/reves, 4 globo, 5 remate
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
        return Maxspeed;
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
