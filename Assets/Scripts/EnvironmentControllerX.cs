using System;
using System.IO;
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
    public const float ApproachingKeyPositionsReward = 0.01f; //0.005f;
    public const float StayingAroundKeyPositionsReward = 0.1f; // 0.01f;
    public const float HittingBallReward = 0.05f;
    public const float SpeedReward = 0.001f;
    private bool simulationCompleted;
    public bool RecordingDemonstrations;

    public bool DebugMode;
    public bool logEnabled;

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

    public void HitBall(Team team, Vector3 force, int hitType)
    {
        ballController.HitBall(team, force, hitType);
    }

    public Vector3 GetBallLocalPosition()
    {
        return ballController.GetBallLocalPosition();
    }

    public float GetMaxspeed_Agent()
    {
        return agentsController.padelAgentsList[0].Maxspeed;
    }
    public bool BallIsLocked()
    {
        return ballController.BallIsLocked();
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

    public Vector3 CalculateForce(Team team, float yMax, float xGrid, float zGrid, int hitType)
    {
        return ballController.CalculateForce(team, yMax, xGrid, zGrid, hitType);
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

    public void SimulateTrajectory(Vector3 pos, Vector3 velocity, Team hitByTeam, BallControllerX.Efecto efecto)
    {
        trajectoryController.SimulateTrajectory(pos, velocity, hitByTeam, efecto);
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

    public Vector3 GetCenterField()
    {
        return ballController.centerField;
    }
    public Logger logger = new();

}


public class Action
{
    public string time;
    public long seconds;
    public int team;
    public int player;
    public float Zpos;
    public float Xpos;
    public float xGrid;
    public float zGrid;
    public int hitType;
    public Vector3[] playerPositions;

    public Action(string _time, long _seconds, int _team, int _player, float _Zpos, float _Xpos, float x, float z, int type, Vector3[] _playerPositions)
    {
        time = _time;
        seconds = _seconds;
        team = _team;
        player = _player;
        Zpos = _Zpos;
        Xpos = _Xpos;
        xGrid = x;
        zGrid = z;
        hitType = type;
        playerPositions = _playerPositions;
    }
}

public class State
{
    public string time;
    public long seconds;

    public Vector3[] playerPositions;
    public Vector3 ballPosition;

    public State(string _time, long _seconds, Vector3[] _playerPositions, Vector3 _ballPosition)
    {
        time = _time;
        seconds = _seconds;
        playerPositions = _playerPositions;
        ballPosition = _ballPosition;
    }
}

public class Logger
{
    private const int STEPS = 1000;  // save to disk every STEPS actions
    public List<Action> actions = new List<Action>();
    public List<State> states = new List<State>();
    public void LogAction(int team, int player, float Xtarget, float Ztarget, float xGrid, float zGrid, int hitType, Vector3[] playerPositions)
    {
        actions.Add(new Action(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fff"), DateTimeOffset.Now.ToUnixTimeMilliseconds(), team, player, Xtarget, Ztarget, xGrid, zGrid, hitType, playerPositions));
        if (actions.Count % STEPS == 0)
        {
            SaveActions();
        }
    }

    public void LogState(Vector3[] playerPositions, Vector3 ballPosition)
    {
        states.Add(new State(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fff"), DateTimeOffset.Now.ToUnixTimeMilliseconds(), playerPositions, ballPosition));
        if (states.Count % STEPS == 0)
        {
            SaveStates();
        }
    }

    const string folder = @"C:\Temp\";
    public void SaveActions()
    {
        string fileName = "actionLog.csv";
        string fullPath = folder + fileName;

        string[] rows = new string[actions.Count];
        for (int i = 0; i < actions.Count; i++)
        {
            rows[i] = $"{actions[i].time};{actions[i].seconds};{actions[i].team};{actions[i].player % 2};{actions[i].Xpos};{actions[i].Zpos};{actions[i].xGrid};{actions[i].zGrid};{actions[i].hitType};";
            for (int j = 0; j < 4; j++)
            {
                Vector3 p = actions[i].playerPositions[j];
                rows[i] += $"{actions[i].playerPositions[j].x};{actions[i].playerPositions[j].z};";
            }
        }
        File.WriteAllLines(fullPath, rows);
    }
    public void SaveStates()
    {
        string fileName = "stateLog.csv";
        string fullPath = folder + fileName;
        string[] rows = new string[states.Count];
        for (int i = 0; i < states.Count; i++)
        {
            rows[i] = $"{states[i].time};{states[i].seconds};";
            for (int j = 0; j < 4; j++)
            {
                Vector3 p = states[i].playerPositions[j];
                rows[i] += $"{states[i].playerPositions[j].x};{states[i].playerPositions[j].z};";
            }
            rows[i] += $"{states[i].ballPosition.x};{states[i].ballPosition.y};{states[i].ballPosition.z}";
        }
        File.WriteAllLines(fullPath, rows);
    }

}
