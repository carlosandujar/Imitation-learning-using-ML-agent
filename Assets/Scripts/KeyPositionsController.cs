using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net.Security;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class KeyPositionsController : MonoBehaviour
{
    public EnvironmentControllerX environmentController;
    public GameObject pointObject;
    public Transform parentEnvironment;
    public Material redMaterial;
    public Material greenMaterial;
    public Material darkBlueMaterial;

    public TextMeshProUGUI T1_1Text;
    public TextMeshProUGUI T1_2Text;
    public TextMeshProUGUI T2_1Text;
    public TextMeshProUGUI T2_2Text;

    public TextMeshProUGUI Opponent1Text;
    public TextMeshProUGUI Opponent2Text;
    public TextMeshProUGUI ReceiverText;
    public TextMeshProUGUI TeammateText;
    

    private class ReceiverKeyPosition
    {
        public Vector3 position;
        public float timeMargin;

        public ReceiverKeyPosition(Vector3 position, float timeMargin)
        {
            this.position = position;
            this.timeMargin = timeMargin;
        }
    }

    public List<PadelAgentX> padelAgentsList;
    private PadelAgentX[] t1Players = null;
    private PadelAgentX[] t2Players = null;

    private const float GridDistance = 10f / 6;

    // Receiver related variables
    private ReceiverKeyPosition[] receiverKeyPositions; // LOCAL POSITIONS
    private GameObject[] points;
    private Vector3 lastReceiverPosition = Vector3.zero;
    private bool receiverApproachingReward = false;
    private bool receiverStayingReward = false;
    
    // Teammate related variables
    private Vector3 teammateKeyPosition; // LOCAL POSITION
    private GameObject teammatePoint;
    private Vector3 lastTeammatePosition = Vector3.zero;
    private bool teammateApproachingReward = false;
    private bool teammateStayingReward = false;

    // Oponents related variables
    private Vector3[] opponentsKeyPositions; // LOCAL POSITION
    private GameObject[] opponentsPoints;
    private Vector3[] lastOpponentsPositions;
    private bool opponent1ApproachingReward = false;
    private bool opponent2ApproachingReward = false;
    private bool opponent1StayingReward = false;
    private bool opponent2StayingReward = false;

    // Start is called before the first frame update
    void Start()
    {
        t1Players = new PadelAgentX[2];
        t2Players = new PadelAgentX[2];
        foreach (PadelAgentX agent in padelAgentsList)
        {
            if (agent.playerId == PlayerId.T1_1) t1Players[0] = agent;
            else if (agent.playerId == PlayerId.T1_2) t1Players[1] = agent;
            else if (agent.playerId == PlayerId.T2_1) t2Players[0] = agent;
            else if (agent.playerId == PlayerId.T2_2) t2Players[1] = agent;
        }
        opponentsKeyPositions = new Vector3[2];
        opponentsKeyPositions[0] = Vector3.zero;
        opponentsKeyPositions[1] = Vector3.zero;
        opponentsPoints = new GameObject[2];
        opponentsPoints[0] = null;
        opponentsPoints[1] = null;
        receiverKeyPositions = new ReceiverKeyPosition[5];
        points = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            receiverKeyPositions[i] = new ReceiverKeyPosition(Vector3.zero, -1);
            points[i] = null;
        }

        teammatePoint = null;

        lastOpponentsPositions = new Vector3[2];
        lastOpponentsPositions[0] = Vector3.zero;
        lastOpponentsPositions[1] = Vector3.zero;
    }

    public void ChangeDebugMode(bool debugMode)
    {
        if (environmentController.GetEnvironmentId() != 0) return;
        else
        {
            T1_1Text.enabled = debugMode;
            T1_2Text.enabled = debugMode;
            T2_1Text.enabled = debugMode;
            T2_2Text.enabled = debugMode;
            Opponent1Text.enabled = debugMode;
            Opponent2Text.enabled = debugMode;
            ReceiverText.enabled = debugMode;
            TeammateText.enabled = debugMode;
        }
        foreach (GameObject point in points)
        {
            if (point != null)
            {
                point.GetComponent<Renderer>().enabled = debugMode;
            }
        }
        foreach (GameObject opponentsPoint in opponentsPoints)
        {
            if (opponentsPoint != null)
            {
                opponentsPoint.GetComponent<Renderer>().enabled = debugMode;
            }
        }
        if (teammatePoint != null)
        {
            teammatePoint.GetComponent<Renderer>().enabled = debugMode;
        }
        foreach (PadelAgentX agent in padelAgentsList)
        {
            agent.SetMarkRendererEnabled(debugMode);
        }
    }

    private void DisplayPlayersInfo()
    {
        if (environmentController.GetEnvironmentId() != 0) return;
        T1_1Text.text = $"[T1-1 Info]\n" +
            $"Position: {t1Players[0].transform.localPosition}\n" +
            $"Role: {t1Players[0].GetRole()}\n" +
            $"Reward: {t1Players[0].GetCumulativeReward()}";
        T1_2Text.text = $"[T1-2 Info]\n" +
            $"Position: {t1Players[1].transform.localPosition}\n" +
            $"Role: {t1Players[1].GetRole()}\n" +
            $"Reward: {t1Players[1].GetCumulativeReward()}";
        T2_1Text.text = $"[T2-1 Info]\n" +
            $"Position: {t2Players[0].transform.localPosition}\n" +
            $"Role: {t2Players[0].GetRole()}\n" +
            $"Reward: {t2Players[0].GetCumulativeReward()}";
        T2_2Text.text = $"[T2-2 Info]\n" +
            $"Position: {t2Players[1].transform.localPosition}\n" +
            $"Role: {t2Players[1].GetRole()}\n" +
            $"Reward: {t2Players[1].GetCumulativeReward()}";

        ReceiverText.text = $"[Receiver Info]\n" +
            $"LastPos: {lastReceiverPosition}\n" +
            $"StayingReward: {receiverStayingReward}\n" +
            $"ApproachingReward: {receiverApproachingReward}\n";

        TeammateText.text = $"[Teammate Info]\n" +
            $"LastPos: {lastTeammatePosition}\n" +
            $"StayingReward: {teammateStayingReward}\n" +
            $"ApproachingReward: {teammateApproachingReward}\n";

        Opponent1Text.text = $"[Opponent1 Info]\n" +
            $"LastPos: {lastOpponentsPositions[0]}\n" +
            $"StayingReward: {opponent1StayingReward}\n" +
            $"ApproachingReward: {opponent1ApproachingReward}\n";

        Opponent2Text.text = $"[Opponent2 Info]\n" +
            $"LastPos: {lastOpponentsPositions[1]}\n" +
            $"StayingReward: {opponent2StayingReward}\n" +
            $"ApproachingReward: {opponent2ApproachingReward}\n";

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DisplayPlayersInfo();
        // Decrease the time margin of receiver key positions and update whether they are still reachable
        for (int i = 0; i < 5; i++)
        {
            if (receiverKeyPositions[i].timeMargin < 0 || !PositionIsReachable(receiverKeyPositions[i].position, receiverKeyPositions[i].timeMargin, environmentController.GetLastHitByTeam()))
            {
                receiverKeyPositions[i].timeMargin = -1;
                if (points[i] != null)
                {
                    Destroy(points[i]);
                    points[i] = null;
                }
            }
            else
            {
                receiverKeyPositions[i].timeMargin -= Time.fixedDeltaTime;
            }
        }

        // Update the player with most nearest keypoints (update the Receiver role assignment)
        if (environmentController.GetSimulationCompleted())
        {
            UpdatePlayerWithMostNearestKeypoints();
        }

        // Update the key positions of players that are not the Receiver
        UpdateOpponentsKeyPositions();
        UpdateTeammateKeyPosition();
    }

    private void UpdateOpponentsKeyPositions()
    {
        PadelAgentX[] players = environmentController.GetLastHitByTeam() == Team.T1 ? t1Players : t2Players;
        
        for (int i = 0; i < players.Length; i++)
        {
            PadelAgentX playerToCover = i == 0 ? players[1] : players[0];
            Vector3 coveringPosition = CalculateOptimalCoveringPosition(playerToCover);
            opponentsKeyPositions[i] = coveringPosition;
            if (opponentsPoints[i] != null)
            {
                opponentsPoints[i].transform.localPosition = opponentsKeyPositions[i];
            }
            else
            {
                GameObject instantiatedPoint = Instantiate(pointObject, parentEnvironment.TransformPoint(opponentsKeyPositions[i]), parentEnvironment.rotation, parentEnvironment);
                instantiatedPoint.GetComponent<Renderer>().enabled = environmentController.DebugMode;
                opponentsPoints[i] = instantiatedPoint;
            }
        }
    }

    private void UpdateTeammateKeyPosition()
    {
        PadelAgentX[] players = environmentController.GetLastHitByTeam() == Team.T1 ? t2Players : t1Players;
        for (int i = 0; i < players.Length; i++)
        {
            PadelAgentX playerToCover = i == 0 ? players[1] : players[0];
            if (playerToCover.GetRole() == Role.Receiver)
            {
                Vector3 coveringPosition = CalculateOptimalCoveringPosition(playerToCover);
                teammateKeyPosition = coveringPosition;
                if (teammatePoint != null)
                {
                    teammatePoint.transform.localPosition = teammateKeyPosition;
                }
                else
                {
                    GameObject instantiatedPoint = Instantiate(pointObject, parentEnvironment.TransformPoint(teammateKeyPosition), parentEnvironment.rotation, parentEnvironment);
                    instantiatedPoint.GetComponent<Renderer>().enabled = environmentController.DebugMode;
                    teammatePoint = instantiatedPoint;
                }
            }
        }
    }

    private Vector3 CalculateOptimalCoveringPosition(PadelAgentX playerToBeCovered)
    {
        float teamSign = playerToBeCovered.team == Team.T1 ? 1 : -1;
        float zFront = 0;
        float zBack = teamSign * -10;
        float xLeft = teamSign * -5;
        float xRight = teamSign * 5;
        Vector3 playerPosition = playerToBeCovered.transform.localPosition;
        float xAxisDistanceToLeft = Mathf.Abs(xLeft - playerPosition.x);
        float xAxisDistanceToRight = Mathf.Abs(xRight - playerPosition.x);
        float zAxisDistanceToFront = Mathf.Abs(zFront - playerPosition.z);
        float zAxisDistanceToBack = Mathf.Abs(zBack - playerPosition.z);
        float zCoveringPosition = zAxisDistanceToFront > zAxisDistanceToBack ? playerPosition.z + teamSign * (zAxisDistanceToFront / 2) : playerPosition.z + teamSign * (-zAxisDistanceToBack / 2);
        float xCoveringPosition = xAxisDistanceToRight > xAxisDistanceToLeft ? playerPosition.x + teamSign * (xAxisDistanceToRight / 2) : playerPosition.x + teamSign * (-xAxisDistanceToLeft / 2);
        return new Vector3(xCoveringPosition, 0, zCoveringPosition);
    }

    private void UpdatePlayerWithMostNearestKeypoints()
    {
        int player0NearestKeypoints = 0;
        int player1NearestKeypoints = 0;
        PadelAgentX player0 = environmentController.GetLastHitByTeam() == Team.T1 ? t2Players[0] : t1Players[0];
        PadelAgentX player1 = environmentController.GetLastHitByTeam() == Team.T1 ? t2Players[1] : t1Players[1];
        for (int i = 0; i < 5; i++)
        {
            if (receiverKeyPositions[i].timeMargin > 0)
            {
                Vector3 keyPositionXZ = new Vector3(receiverKeyPositions[i].position.x, 0, receiverKeyPositions[i].position.z);
                Vector3 player0PositionXZ = new Vector3(player0.transform.localPosition.x, 0, player0.transform.localPosition.z);
                Vector3 player1PositionXZ = new Vector3(player1.transform.localPosition.x, 0, player1.transform.localPosition.z);
                float distanceToPlayer0 = Vector3.Distance(keyPositionXZ, player0PositionXZ);
                float distanceToPlayer1 = Vector3.Distance(keyPositionXZ, player1PositionXZ);
                if (distanceToPlayer0 < distanceToPlayer1)
                {
                    player0NearestKeypoints++;
                }
                else if (distanceToPlayer0 > distanceToPlayer1)
                {
                    player1NearestKeypoints++;
                }
            }
        }
        if (player0NearestKeypoints > player1NearestKeypoints)
        {
            player0.SetMarkMaterial(greenMaterial);
            player0.AssignRole(Role.Receiver);
            player1.SetMarkMaterial(redMaterial);
            player1.AssignRole(Role.Teammate);
        }
        else if (player0NearestKeypoints < player1NearestKeypoints)
        {
            player0.SetMarkMaterial(redMaterial);
            player0.AssignRole(Role.Teammate);
            player1.SetMarkMaterial(greenMaterial);
            player1.AssignRole(Role.Receiver);
        }
        else
        {
            player0.SetMarkMaterial(redMaterial);
            player0.AssignRole(Role.Teammate);
            player1.SetMarkMaterial(redMaterial);
            player1.AssignRole(Role.Teammate);
            Vector3 ballLocalPosition = environmentController.GetBallLocalPosition();
            Vector3 player0PositionXZ = new Vector3(player0.transform.localPosition.x, 0, player0.transform.localPosition.z);
            Vector3 player1PositionXZ = new Vector3(player1.transform.localPosition.x, 0, player1.transform.localPosition.z);
            Vector3 ballXZ = new Vector3(ballLocalPosition.x, 0, ballLocalPosition.z);
            float distanceToPlayer0 = Vector3.Distance(ballXZ, player0PositionXZ);
            float distanceToPlayer1 = Vector3.Distance(ballXZ, player1PositionXZ);
            if (distanceToPlayer0 <= distanceToPlayer1)
            {
                player0.SetMarkMaterial(greenMaterial);
                player0.AssignRole(Role.Receiver);
            }
            else
            {
                player1.SetMarkMaterial(greenMaterial);
                player1.AssignRole(Role.Receiver);
            }
        }
    }


    // NOTE: ghostBallPosition is in world space since it comes form the physics scene
    public void AnalyzeKeyPosition(Vector3 ghostBallPosition, float timeMargin, Team hitByTeam)
    {
        Vector3 ghostBallLocalPosition = parentEnvironment.InverseTransformPoint(ghostBallPosition);

        if ((hitByTeam == Team.T1 && ghostBallLocalPosition.z < 0) || (hitByTeam == Team.T2 && ghostBallLocalPosition.z > 0))
            return;

        float z = ghostBallLocalPosition.z;
        if (hitByTeam == Team.T2) z = -z;

        if (PositionIsHittable(ghostBallLocalPosition) && PositionIsReachable(ghostBallLocalPosition, timeMargin, hitByTeam))
        {
            int index = -1;
            for (int i = 0; i < 5; i++)
            {
                if (z >= (i + 1) * GridDistance && z < (i + 2) * GridDistance && receiverKeyPositions[i].timeMargin == -1)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                receiverKeyPositions[index].timeMargin = timeMargin;
                receiverKeyPositions[index].position = ghostBallLocalPosition;
                GameObject instantiatedPoint = Instantiate(pointObject, ghostBallPosition, parentEnvironment.rotation, parentEnvironment);
                instantiatedPoint.GetComponent<Renderer>().enabled = environmentController.DebugMode;
                points[index] = instantiatedPoint;
            }
        }
    }

    private bool PositionIsHittable(Vector3 localPosition)
    {
        return localPosition.y >= 0.25f && localPosition.y <= 2; 
    }

    private bool PositionIsReachable(Vector3 localPosition, float timeMargin, Team hitByTeam)
    {
        Vector3 ballPositionXZ = new Vector3(localPosition.x, 0, localPosition.z);
        PadelAgentX[] players = hitByTeam == Team.T1 ? t2Players : t1Players;
        for (int i = 0; i < 2; i++)
        {
            Vector3 playerPositionXZ = new Vector3(players[i].transform.localPosition.x, 0, players[i].transform.localPosition.z);
            float distance = Vector3.Distance(ballPositionXZ, playerPositionXZ);
            if (distance - 1.5f <= players[i].GetSpeed() * timeMargin)
                return true;
        }
        return false;
    }

    public void ClearKeyPositions()
    {
        // Clear receiver key positions
        for (int i = 0; i < 5; i++)
        {
            receiverKeyPositions[i].position = Vector3.zero;
            receiverKeyPositions[i].timeMargin = -1;
            if (points[i] != null)
            {
                Destroy(points[i]);
                points[i] = null;
            }
        }
        // Clear teammate key position
        if (teammatePoint != null)
        {
            teammateKeyPosition = Vector3.zero;
            Destroy(teammatePoint);
            teammatePoint = null;
        }
        // Clear opponents key positions
        for (int i = 0; i < 2; i++)
        {
            opponentsKeyPositions[i] = Vector3.zero;
            if (opponentsPoints[i] != null)
            {
                Destroy(opponentsPoints[i]);
                opponentsPoints[i] = null;
            }
        }
    }

    public void UpdatePlayersRoles(Team hitByTeam)
    {
        opponentsKeyPositions[0] = Vector3.zero;
        opponentsKeyPositions[1] = Vector3.zero;
        lastOpponentsPositions[0] = Vector3.zero;
        lastOpponentsPositions[1] = Vector3.zero;
        
        foreach (var keyPosition in receiverKeyPositions)
        {
            keyPosition.position = Vector3.zero;
            keyPosition.timeMargin = -1;
        }
        lastReceiverPosition = Vector3.zero;
        teammateKeyPosition = Vector3.zero;
        lastTeammatePosition = Vector3.zero;
        PadelAgentX[] hitByTeamPlayers = hitByTeam == Team.T1 ? t1Players : t2Players;
        PadelAgentX[] otherTeamPlayers = hitByTeam == Team.T1 ? t2Players : t1Players;

        for (int i = 0; i < 2; i++)
        {
            // OTHER TEAM PLAYERS ARE RECEIVER + TEAMMATE, BUT BOTH ARE TEAMMATES INITIALLY
            otherTeamPlayers[i].AssignRole(Role.Teammate);
            otherTeamPlayers[i].SetMarkMaterial(redMaterial);
            // HIT BY TEAM PLAYERS ARE OPPONENTS
            hitByTeamPlayers[i].AssignRole(Role.Opponent);
            hitByTeamPlayers[i].SetMarkMaterial(darkBlueMaterial);
        }
    }

    public void CalculateKeyPositionsRelatedRewards(PadelAgentX player)
    {
        Vector3 currentPosition = new Vector3(player.transform.localPosition.x, 0, player.transform.localPosition.z);
        Vector3 lastPosition = Vector3.zero;
        Vector3 targetPosition = Vector3.zero;
        switch (player.GetRole())
        {
            case Role.Opponent:
                int opponentIndex = player.playerId == PlayerId.T1_1 || player.playerId == PlayerId.T2_1 ? 0 : 1;
                if (opponentsKeyPositions[opponentIndex] == Vector3.zero) return;
                lastPosition = lastOpponentsPositions[opponentIndex];
                if (lastPosition == Vector3.zero)
                {
                    lastOpponentsPositions[opponentIndex] = currentPosition;
                    return;
                }
                targetPosition = opponentsKeyPositions[opponentIndex];
                break;
            case Role.Teammate:
                if (teammateKeyPosition == Vector3.zero) return;
                lastPosition = lastTeammatePosition;
                if (lastPosition == Vector3.zero)
                {
                    lastTeammatePosition = currentPosition;
                    return;
                }
                targetPosition = teammateKeyPosition;
                break;
            case Role.Receiver:
                lastPosition = lastReceiverPosition;
                if (lastPosition == Vector3.zero)
                {
                    lastReceiverPosition = currentPosition;
                    return;
                }
                foreach (var keyPosition in receiverKeyPositions)
                {
                    if (keyPosition.timeMargin > 0)
                    {
                        if (targetPosition == Vector3.zero)
                        {
                            if (keyPosition.timeMargin > 0) targetPosition = new Vector3(keyPosition.position.x, 0, keyPosition.position.z);
                        }
                        else
                        {
                            Vector3 otherTargetPosition = new Vector3(keyPosition.position.x, 0, keyPosition.position.z);
                            if (Vector3.Distance(currentPosition, otherTargetPosition) < Vector3.Distance(currentPosition, targetPosition))
                            {
                                targetPosition = otherTargetPosition;
                            }
                        }
                    }
                }
                break;
        }
        if (targetPosition == Vector3.zero) return;
        bool approachingRewardAdded = false;
        bool stayingRewardAdded = false;
        if (Vector3.Distance(targetPosition, currentPosition) < 1.5f)
        {
            player.AddReward(EnvironmentControllerX.StayingAroundKeyPositionsReward);
            stayingRewardAdded = true;
        }
        if (Vector3.Distance(targetPosition, currentPosition) < Vector3.Distance(targetPosition, lastPosition))
        {
            player.AddReward(EnvironmentControllerX.ApproachingKeyPositionsReward);
            approachingRewardAdded = true;
        }
        /* It's good to stay near key positions, but not necessarily bad if the agent stays far from them.
        else
        {
            if (Vector3.Distance(targetPosition, currentPosition) > 1.5f)
                player.AddReward(-EnvironmentControllerX.ApproachingKeyPositionsReward);
        }
        */
        switch (player.GetRole())
        {
            case Role.Opponent:
                int opponentIndex = player.playerId == PlayerId.T1_1 || player.playerId == PlayerId.T2_1 ? 0 : 1;
                lastOpponentsPositions[opponentIndex] = currentPosition;
                if (player.playerId == PlayerId.T1_1 || player.playerId == PlayerId.T2_1)
                {
                    opponent1ApproachingReward = approachingRewardAdded;
                    opponent1StayingReward = stayingRewardAdded;
                }
                else
                {
                    opponent2ApproachingReward = approachingRewardAdded;
                    opponent2StayingReward = stayingRewardAdded;
                }
                break;
            case Role.Teammate:
                lastTeammatePosition = currentPosition;
                teammateApproachingReward = approachingRewardAdded;
                teammateStayingReward = stayingRewardAdded;
                break;
            case Role.Receiver:
                lastReceiverPosition = currentPosition;
                receiverApproachingReward = approachingRewardAdded;
                receiverStayingReward = stayingRewardAdded;
                break;
        }
    }

}
