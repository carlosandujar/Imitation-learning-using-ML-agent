using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerId
{
    T1_1, T1_2, T2_1, T2_2
}

public enum Team
{
    T1 = 1,
    T2 = 2
}

public enum Side
{
    Left, Right
}

public enum Role
{
    Receiver, Teammate, Opponent
}

public class EnvironmentControllerX : MonoBehaviour
{
    /* V0 REWARDS
       public const float WinningReward = 0;
       public const float LosingReward = -5;
       public const float ApproachingKeyPositionsReward = 0.005f;
       public const float StayingAroundKeyPositionsReward = 0.01f;
       public const float HittingBallReward = 1f;
    */
    /* V1 REWARDS
       public const float WinningReward = 10;
       public const float LosingReward = -10;
       public const float ApproachingKeyPositionsReward = 0.005f;
       public const float StayingAroundKeyPositionsReward = 0.01f;
       public const float HittingBallReward = 0f;
    */
    public const float WinningReward = 10;
    public const float LosingReward = -10;
    public const float ApproachingKeyPositionsReward = 0.005f;
    public const float StayingAroundKeyPositionsReward = 0.01f;
    public const float HittingBallReward = 0f;
    private bool simulationCompleted;
    public bool DebugMode;
    public bool RecordingDemonstrations;

    private void Start()
    {
        keyPositionsController.ChangeDebugMode(DebugMode);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && environmentId == 0)
        {
            DebugMode = !DebugMode;
            keyPositionsController.ChangeDebugMode(DebugMode);
            if (!DebugMode)
            {
                trajectoryController.ClearTrajectory();
            }
        }
    }

    public void SetSimulationCompleted(bool simulationCompleted)
    {
        this.simulationCompleted = simulationCompleted;
    }

    private Team OtherTeam(Team team)
    {
        return team == Team.T1 ? Team.T2 : Team.T1;
    }

    public bool GetSimulationCompleted()
    {
        return simulationCompleted;
    }

    public ServeControllerX serveController;
    public ScoreControllerX scoreController;
    public BallControllerX ballController;
    public AgentsControllerX agentsController;
    public KeyPositionsController keyPositionsController;
    public TrajectoryController trajectoryController;
    public CoachController coachController;
    public int environmentId;

    public int GetEnvironmentId()
    {
        return environmentId;
    }

    public void SwitchServerSide()
    {
        serveController.SwitchServerSide();
    }

    public void UpdateServerSide(Side side)
    {
        serveController.UpdateServerSide(side);
    }

    public void SwitchServerTeam()
    {
        serveController.SwitchServerTeam();
    }

    public void SwitchServerPlayer()
    {
        serveController.SwitchServerPlayer();
    }

    public void BounceOnService()
    {
        ballController.BounceOnService();
    }

    public bool BallCanBeServed()
    {
        return ballController.BallCanBeServed();
    }

    public void ServeBall(Team team, Side side, Vector3 force)
    {
        ballController.ServeBall(team, side, force);
    }

    public void HitBall(Team team, Vector3 force)
    {
        ballController.HitBall(team, force);
    }

    public Vector3 GetBallLocalPosition()
    {
        return ballController.GetBallLocalPosition();
    }

    public void GivePoint(Team team, string debugMessage)
    {
        if (environmentId == 0)
        {
            Debug.Log(debugMessage);
        }
        agentsController.AddTeamRewards(team, WinningReward);
        agentsController.AddTeamRewards(OtherTeam(team), LosingReward);
        scoreController.GivePoint(team);
    }

    public void AddTeamRewards(Team team, float reward)
    {
        agentsController.AddTeamRewards(team, reward);

    }

    public bool PointJustGiven()
    {
        return ballController.PointJustGiven();
    }

    public Vector3 CalculateForce(Team team, float yMax, float xGrid, float zGrid)
    {
        return ballController.CalculateForce(team, yMax, xGrid, zGrid);
    }

    public void ResetScene()
    {
        agentsController.EndAgentsEpisodes();

        agentsController.UpdateAgentsPosition(serveController.GetServerSide(), serveController.GetServerPlayerId());
        ballController.UpdateBallPosition(serveController.GetServerTeam(), serveController.GetServerSide());

        serveController.SetHasToServe(true);
        serveController.SetHasToBounce(true);
    }

    public void PauseForDuration(float duration)
    {
        // Pause the game by setting the time scale to 0
        Time.timeScale = 0f;

        // Start a coroutine to resume the game after the specified duration
        StartCoroutine(ResumeAfterDelay(duration));
    }

    IEnumerator ResumeAfterDelay(float duration)
    {
        // Wait for the specified duration
        yield return new WaitForSecondsRealtime(duration);

        // Resume the game by setting the time scale back to 1 (normal speed)
        Time.timeScale = 1f;
    }

    public void AnalyzeKeyPosition(Vector3 ghostBallLocalPosition, float timeMargin, Team hitByTeam)
    {
        keyPositionsController.AnalyzeKeyPosition(ghostBallLocalPosition, timeMargin, hitByTeam);
    }

    public void ClearTrajectory()
    {
        trajectoryController.ClearTrajectory();
    }

    public void SimulateTrajectory(GhostBall ghostBallPrefab, Vector3 pos, Vector3 velocity, Team hitByTeam)
    {
        trajectoryController.SimulateTrajectory(ghostBallPrefab, pos, velocity, hitByTeam);
    }

    public void ClearKeyPositions()
    {
        keyPositionsController.ClearKeyPositions();
    }

    public void UpdatePlayersRoles(Team hitByTeam)
    {
        keyPositionsController.UpdatePlayersRoles(hitByTeam);
    }

    public Team GetLastHitByTeam()
    {
        return ballController.GetLastHitByTeam();
    }

    public void AllowPlayersMovement()
    {
        agentsController.AllowPlayersMovement();
    }

    public void StopPlayersMovement()
    {
        agentsController.StopPlayersMovement();
    }

    public void CalculateKeyPositionsRelatedRewards(PadelAgentX player)
    {
        keyPositionsController.CalculateKeyPositionsRelatedRewards(player);
    }

    public bool BallIsLocked()
    {
        return ballController.BallIsLocked();
    }

    public void RequestCoachedMovement(PlayerId playerId, Vector3 selfPosition, Vector3 teammatePosition, Vector3 opponent1Position, Vector3 opponent2Position, Vector3 ballPosition, Team lastHitBy)
    {
        coachController.RequestCoachedMovement(playerId, selfPosition, teammatePosition, opponent1Position, opponent2Position, ballPosition, lastHitBy);
    }

    public void RequestCoachedShot(PlayerId playerId, Vector3 selfPosition, Vector3 teammatePosition, Vector3 opponent1Position, Vector3 opponent2Position, Vector3 ballPosition)
    {
        coachController.RequestCoachedShot(playerId, selfPosition, teammatePosition, opponent1Position, opponent2Position, ballPosition);
    }

    public void SendCoachedShot(PlayerId playerId, int[] shot)
    {
        agentsController.SendCoachedShot(playerId, shot); 
    }

    public PlayerId GetNearestPlayerToPosition(Vector2 position)
    {
        return agentsController.GetNearestPlayerToPosition(position);
    }

    public void SendCoachedMovement(PlayerId playerId, int[] movement)
    {
        agentsController.SendCoachedMovement(playerId, movement);
    }
}
