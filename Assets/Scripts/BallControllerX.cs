using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BallControllerX : MonoBehaviour
{
    private Rigidbody ballRb;
    public EnvironmentControllerX environmentController;
    private Vector3 initialPosition;

    private bool hasToBounceOnService;
    private Team lastHitByTeam;
    private Side serverSide;
    private bool bouncedFloor, bouncedWall;
    private Vector3 bouncingPosition;

    private bool ballIsLocked = false;
    private bool pointJustGiven = false;

    private int timesBouncedOnFloor;
    private bool isServeBall;

    private TrailRenderer trailRenderer;
    Color _orange = new Color(1.0f, 0.64f, 0.0f);
    Color _purple = new Color(0.5f, 0.0f, 0.5f);

    public TextMeshProUGUI BallInfoText;

    private float timesBallHit = 0;

    public float GetTimesBallHit()
    {
        return timesBallHit;
    }

    void DisplayBallInfo()
    {
        if (environmentController.environmentId != 0) return;
        if (environmentController.DebugMode)
        {
            BallInfoText.text = $"[Ball Info]\n" +
            $"LastHitBy: {lastHitByTeam}\n" +
            $"TimesBounced: {timesBouncedOnFloor}\n" +
            $"LastBouncingPos: {bouncingPosition}\n" +
            $"TimesBallHit: {timesBallHit}";
        }
        else
        {
            BallInfoText.text = "";
        }
    }

    void Start()
    {
        ballRb = GetComponent<Rigidbody>();
        ballRb.useGravity = false;
        hasToBounceOnService = false;
        bouncedFloor = false;
        bouncedWall = false;
        timesBouncedOnFloor = 0;
        trailRenderer = GetComponent<TrailRenderer>();
        initialPosition = new Vector3(2.5f, 1, -7);
    }

    private float hitTimeMargin = 0;
    private float pointTimeMargin = 0;

    private void FixedUpdate()
    {
        /*
         * Necessary for sync. Without locking the ball, a player might hit the ball while the trajectory
         * is being calculated
         */
        if (ballIsLocked)
        {
            hitTimeMargin += Time.fixedDeltaTime;
            if (hitTimeMargin > 0.1)
            {
                ballIsLocked = false;
                hitTimeMargin = 0;
            }
        }
        /*
         * Points are given with delay: if all positions are updated within the same Update(), some weird collisions can occur
         * when an agent is exactly at the position where the ball is going to spawn. This caused problems like instantly giving a point after reset
         * The solution here was ResetBall() first, then with a delay of 0.1s giving the point. During the reset, the ball is moved to y = 20 so
         * no collisions can be caused. Then, after the 0.1s delay, players are placed to their corresponding positions and then the ball is placed back
         */
        if (pointJustGiven)
        {
            pointTimeMargin += Time.fixedDeltaTime;
            if (pointTimeMargin > 0.1)
            {
                environmentController.GivePoint(pendingRewardTeam, pendingDebugMessage);
                pointJustGiven = false;
                pointTimeMargin = 0;
            }
        }
    }

    void Update()
    {
        if (transform.position.y < -1)
        {
            GivePointToOpponent($"Giving point to {OtherTeam(lastHitByTeam)} because the ball went outside");
            return;
        }
        /* Cuando la pelota rebota en el suelo:
         *  - En el saque, si el primer rebote no aterriza donde corresponde, punto para el equipo rival
         *  - En los remates, si el primer rebote aterriza en el propio campo, punto para el equipo rival
         *  - Si ha rebotado 2 veces en el suelo, y el punto no se ha resuelto con el primer rebote en el suelo, el punto será para el equipo que haya golpeado 100%
         */
        if (bouncedFloor)
        {
            bouncedFloor = false;
            timesBouncedOnFloor += 1;
            if (timesBouncedOnFloor > 1)
            {
                GivePointToSelf($"Giving point to {lastHitByTeam} because the ball bounced twice");
                return;
            }
            if (isServeBall && timesBouncedOnFloor == 1)
            {
                if (lastHitByTeam == Team.T1)
                {
                    if (serverSide == Side.Right && !IsOnT2RightSide(bouncingPosition))
                    {
                        GivePointToOpponent("Giving point to T2 because T1 is serving from right and ball didn't bounce on T2 right");
                        return;
                    }
                    else if (serverSide == Side.Left && !IsOnT2LeftSide(bouncingPosition))
                    {
                        GivePointToOpponent("Giving point to T2 because T1 is serving from left and ball didn't bounce on T2 left");
                        return;
                    }
                }
                else if (lastHitByTeam == Team.T2)
                {
                    if (serverSide == Side.Right && !IsOnT1RightSide(bouncingPosition))
                    {
                        GivePointToOpponent("Giving point to T1 because T2 is serving from right and ball didn't bounce on T1 right");
                        return;
                    }
                    else if (serverSide == Side.Left && !IsOnT1LeftSide(bouncingPosition))
                    {
                        GivePointToOpponent("Giving point to T1 because T2 is serving from left and ball didn't bounce on T1 left");
                        return;
                    }
                }
            }
            else if (timesBouncedOnFloor == 1)
            {
                if (lastHitByTeam == Team.T1 && IsOnT1Side(bouncingPosition))
                {
                    GivePointToOpponent($"Giving point to {OtherTeam(lastHitByTeam)} because first bounce was in own side");
                    return;
                }
                else if (lastHitByTeam == Team.T2 && IsOnT2Side(bouncingPosition))
                {
                    GivePointToOpponent($"Giving point to {OtherTeam(lastHitByTeam)} because first bounce was in own side");
                    return;
                }
            }
        }
        if (bouncedWall)
        {
            // Falta si: la pelota rebota en la pared del equipo rival sin antes haber rebotado en el suelo
            // Falta si: la pelota rebota en la malla tras haber rebotado en el suelo durante el servicio
            bouncedWall = false;
            if (timesBouncedOnFloor == 0)
            {
                if ((lastHitByTeam == Team.T1 && !IsOnT1Side(bouncingPosition)) || (lastHitByTeam == Team.T2 && !IsOnT2Side(bouncingPosition)))
                {
                    GivePointToOpponent($"Giving point to {OtherTeam(lastHitByTeam)} because ball hit the wall before bouncing on the ground");
                    return;
                }
            }
            else if (timesBouncedOnFloor == 1 && isServeBall && BouncedOnMesh(bouncingPosition))
            {
                GivePointToOpponent($"Giving point to {OtherTeam(lastHitByTeam)} because ball hit the mesh on serve ball");
                return;
            }
        }

        DisplayBallInfo();
    }

    // NOTE: T1 side is -Z, T2 side is +Z
    /* T1 Side: -Z (< 0, >= -10)
     * T2 Side: +Z (> 0, <= 10)
     * T1 Left Side -X (< 0, >= -5), T1 Right Side +X (> 0, <= 5)
     * T2 Left Side +X (> 0, <= 5), T2 Right Side -X (< 0, >= -5)
     */

    private bool IsOnT1Side(Vector3 position)
    {
        float z = position.z;
        return (z < 0 && z >= -10);
    }

    private bool IsOnT1LeftSide(Vector3 position)
    {
        float x = position.x;
        float z = position.z;
        return (z < 0 && z >= -7) && (x < 0 && x >= -5);
    }

    private bool IsOnT1RightSide(Vector3 position)
    {
        float x = position.x;
        float z = position.z;
        return (z < 0 && z >= -7) && (x > 0 && x <= 5);
    }

    private bool IsOnT2Side(Vector3 position)
    {
        float z = position.z;
        return (z > 0 && z <= 10);

    }

    private bool IsOnT2LeftSide(Vector3 position)
    {
        float x = position.x;
        float z = position.z;
        return (z > 0 && z <= 7) && (x > 0 && x <= 5);
    }

    private bool IsOnT2RightSide(Vector3 position)
    {
        float x = position.x;
        float z = position.z;
        return (z > 0 && z <= 7) && (x < 0 && x >= -5);
    }

    private bool BouncedOnMesh(Vector3 position)
    {
        float y = position.y;
        float z = position.z;
        return y > 3 || (z >= -6 && z <= 6);
    }

    private void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;
        if (tag == "PlayerT1")
        {
            if (lastHitByTeam == Team.T1)
            {
                GivePointToOpponent($"Giving point to {OtherTeam(lastHitByTeam)} because {lastHitByTeam} hit {lastHitByTeam}");
                return;
            }
            else
            {
                GivePointToSelf($"Giving point to {lastHitByTeam} because {lastHitByTeam} hit {OtherTeam(lastHitByTeam)}");
                return;
            }
        }
        else if (tag == "PlayerT2")
        {
            if (lastHitByTeam == Team.T2)
            {
                GivePointToOpponent($"Giving point to {OtherTeam(lastHitByTeam)} because {lastHitByTeam} hit {lastHitByTeam}");
                return;
            }
            else
            {
                GivePointToSelf($"Giving point to {lastHitByTeam} because {lastHitByTeam} hit {OtherTeam(lastHitByTeam)}");
                return;
            }
        }
        else if (tag == "Net")
        {
            if (timesBouncedOnFloor == 0)
            {
                GivePointToOpponent($"Giving point to {OtherTeam(lastHitByTeam)} because {lastHitByTeam} has hit the net");
                return;
            }
            else if (timesBouncedOnFloor >= 1)
            {
                GivePointToSelf($"Giving point to {lastHitByTeam} because ball bounced once and hit the net");
                return;
            }
        }
        else if (tag == "Floor")
        {
            if (hasToBounceOnService)
            {
                hasToBounceOnService = false;
            }
            else
            {
                bouncedFloor = true;
                bouncingPosition = transform.localPosition;
            }
        }
        else if (tag == "Wall")
        {
            bouncedWall = true;
            bouncingPosition = transform.localPosition;
        }
    }

    private Team OtherTeam(Team team)
    {
        if (lastHitByTeam == Team.T1)
        {
            return Team.T2;
        }
        else return Team.T1;
    }

    Team pendingRewardTeam = 0;
    string pendingDebugMessage = "";

    private void GivePointToOpponent(string debugMessage)
    {
        if (!pointJustGiven)
        {
            environmentController.StopPlayersMovement();
            ResetBall();
            pendingRewardTeam = OtherTeam(lastHitByTeam);
            pendingDebugMessage = debugMessage;
            pointJustGiven = true;
        }
    }

    private void GivePointToSelf(string debugMessage)
    {
        if (!pointJustGiven)
        {
            environmentController.StopPlayersMovement();
            ResetBall();
            pendingRewardTeam = lastHitByTeam;
            pendingDebugMessage = debugMessage;
            pointJustGiven = true;
        }
    }

    public void BounceOnService()
    {
        hasToBounceOnService = true;
        this.ballRb.useGravity = true;
        ballRb.AddForce(new Vector3(0, -10, 0));
    }

    public bool BallCanBeServed()
    {
        return hasToBounceOnService == false && ballRb.transform.localPosition.y >= 1;
    }

    public bool BallIsLocked()
    {
        return ballIsLocked;
    }

    public bool PointJustGiven()
    {
        return pointJustGiven;
    }

    public void ServeBall(Team team, Side side, Vector3 force)
    {
        ballIsLocked = true;
        environmentController.UpdatePlayersRoles(team);
        environmentController.ClearTrajectory();
        environmentController.ClearKeyPositions();

        environmentController.AllowPlayersMovement();
        
        ChangeTrailRendererColor(team);
        lastHitByTeam = team;
        ballRb.velocity = Vector3.zero;
        ballRb.AddForce(force, ForceMode.VelocityChange);
        timesBallHit += 1;
        environmentController.SimulateTrajectory(this.GetComponent<GhostBall>(), ballRb.position, force, team);
        isServeBall = true;
        serverSide = side;
        timesBouncedOnFloor = 0;
        trailRenderer.emitting = true;
    }

    public void HitBall(Team team, Vector3 force)
    {
        ballIsLocked = true;
        environmentController.UpdatePlayersRoles(team);
        environmentController.ClearTrajectory();
        environmentController.ClearKeyPositions();

        ChangeTrailRendererColor(team);
        lastHitByTeam = team;
        ballRb.velocity = Vector3.zero;
        ballRb.AddForce(force, ForceMode.VelocityChange);
        timesBallHit += 1;
        environmentController.SimulateTrajectory(this.GetComponent<GhostBall>(), ballRb.position, force, team);
        isServeBall = false;
        timesBouncedOnFloor = 0;
    }

    private void ChangeTrailRendererColor(Team team)
    {
        if (team == Team.T1)
        {
            trailRenderer.startColor = _orange;
            trailRenderer.endColor = _orange;
        }
        else
        {
            trailRenderer.startColor = _purple;
            trailRenderer.endColor = _purple;
        }
    }

    public void ResetBall()
    {
        trailRenderer.emitting = false;
        ballRb.transform.localPosition = new Vector3(0, 20, 0);
        ballRb.useGravity = false;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        this.transform.rotation = Quaternion.identity;
        timesBouncedOnFloor = 0;
        bouncedFloor = false;
        bouncedWall = false;
        timesBallHit = 0;
    }

    public void UpdateBallPosition(Team team, Side side)
    {
        switch (team)
        {
            case Team.T1:
                switch (side)
                {
                    case Side.Left:
                        ballRb.transform.localPosition = new Vector3(-initialPosition.x, initialPosition.y, initialPosition.z);
                        break;
                    case Side.Right:
                        ballRb.transform.localPosition = initialPosition;
                        break;
                }
                break;
            case Team.T2: 
                switch (side)
                {
                    case Side.Left:
                        ballRb.transform.localPosition = new Vector3(initialPosition.x, initialPosition.y, -initialPosition.z);
                        break;
                    case Side.Right:
                        ballRb.transform.localPosition = new Vector3(-initialPosition.x, initialPosition.y, -initialPosition.z);
                        break;
                }
                break;
        }
    }

    public Vector3 CalculateForce(Team team, float yMax, float xGrid, float zGrid)
    {
        float xTarget = -2 * (10f / 6) + (10f / 6) * xGrid;
        float zTarget = (10f / 6 * 5) - (10f / 6) * zGrid;
        if (team == Team.T2)
        {
            zTarget *= -1;
            xTarget *= -1;
        }

        float zStart = ballRb.transform.localPosition.z;
        float xStart = ballRb.transform.localPosition.x;
        float yStart = ballRb.transform.localPosition.y;
        if (yMax < yStart) yMax = yStart;
        float yVelocity = Mathf.Sqrt((yStart - yMax) * 2 * (Physics.gravity.y));
        float t = (-yVelocity - Mathf.Sqrt(yVelocity * yVelocity - 2 * Physics.gravity.y * yStart)) / Physics.gravity.y;
        if (t < 0)
            t = (-yVelocity + Mathf.Sqrt(yVelocity * yVelocity - 2 * Physics.gravity.y * yStart)) / Physics.gravity.y;
        float zVelocity = (zTarget - zStart) / t;
        float xVelocity = (xTarget - xStart) / t;
        return new Vector3(xVelocity, yVelocity, zVelocity);
    }

    public Team GetLastHitByTeam()
    {
        return lastHitByTeam;
    }

    public Vector3 GetBallLocalPosition()
    {
        return ballRb.transform.localPosition;
    }
}
