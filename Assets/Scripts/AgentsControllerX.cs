using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentsControllerX : MonoBehaviour
{
    public List<PadelAgentX> padelAgentsList;
    private Dictionary<PlayerId, PadelAgentX> padelAgents = new Dictionary<PlayerId, PadelAgentX>();

    private static Vector3 T1RightLocalPosition = new Vector3(2.5f, 0.875f, -8);
    private static Vector3 T1LeftLocalPosition = new Vector3(-2.5f, 0.875f, -8);
    private static Vector3 T2RightLocalPosition = new Vector3(-2.5f, 0.875f, 8);
    private static Vector3 T2LeftLocalPosition = new Vector3(2.5f, 0.875f, 8);

    // Start is called before the first frame update
    void Start()
    {
        foreach (PadelAgentX agent in padelAgentsList)
        {
            PlayerId id = agent.playerId;
            padelAgents[id] = agent;
        }
    }

    private PlayerId TeammateOf(PlayerId playerId)
    {
        switch (playerId)
        {
            case PlayerId.T1_1:
                return PlayerId.T1_2;
            case PlayerId.T1_2:
                return PlayerId.T1_1;
            case PlayerId.T2_1:
                return PlayerId.T2_2;
        }
        return PlayerId.T2_1;
    }

    public void UpdateAgentsPosition(Side serverSide, PlayerId serverPlayerId)
    {
        if (serverPlayerId == PlayerId.T1_1 || serverPlayerId == PlayerId.T1_2)
        {
            if (serverSide == Side.Right)
            {
                padelAgents[serverPlayerId].transform.localPosition = T1RightLocalPosition;
                padelAgents[TeammateOf(serverPlayerId)].transform.localPosition = T1LeftLocalPosition;
            }
            else if (serverSide == Side.Left)
            {
                padelAgents[serverPlayerId].transform.localPosition = T1LeftLocalPosition;
                padelAgents[TeammateOf(serverPlayerId)].transform.localPosition = T1RightLocalPosition;
            }
            padelAgents[PlayerId.T2_1].transform.localPosition = T2RightLocalPosition;
            padelAgents[PlayerId.T2_2].transform.localPosition = T2LeftLocalPosition;
        }
        else
        {
            if (serverSide == Side.Right)
            {
                padelAgents[serverPlayerId].transform.localPosition = T2RightLocalPosition;
                padelAgents[TeammateOf(serverPlayerId)].transform.localPosition = T2LeftLocalPosition;
            }
            else if (serverSide == Side.Left)
            {
                padelAgents[serverPlayerId].transform.localPosition = T2LeftLocalPosition;
                padelAgents[TeammateOf(serverPlayerId)].transform.localPosition = T2RightLocalPosition;
            }
            padelAgents[PlayerId.T1_1].transform.localPosition = T1RightLocalPosition;
            padelAgents[PlayerId.T1_2].transform.localPosition = T1LeftLocalPosition;
        }
    }

    public void AllowPlayersMovement()
    {
        foreach (PadelAgentX player in padelAgents.Values)
        {
            player.StartMoving();
        }
    }

    public void StopPlayersMovement()
    {
        foreach (PadelAgentX player in padelAgents.Values)
        {
            player.StopMoving();
        }
    }

    public void EndAgentsEpisodes()
    {
        foreach (PadelAgentX player in padelAgents.Values)
        {
            player.EndEpisode();
        }
    }

    public void AddTeamRewards(Team team, float reward)
    {
        foreach (PadelAgentX player in padelAgents.Values)
        {
            if (player.team == team)
            {
                player.AddReward(reward);
            }
        }
    }

    public void SendCoachedShot(PlayerId playerId, int[] shot)
    {
        padelAgents[playerId].SetShot(shot);
    }

    public void SendCoachedMovement(PlayerId playerId, int[] movement)
    {
        padelAgents[playerId].SetMovement(movement);
    }

    public PlayerId GetNearestPlayerToPosition(Vector2 position)
    {
        Vector2 nearestPlayerLocalPosition = new Vector2(padelAgents[PlayerId.T1_1].gameObject.transform.localPosition.x, padelAgents[PlayerId.T1_1].gameObject.transform.localPosition.z);
        PlayerId nearestPlayerId = PlayerId.T1_1;
        foreach (PadelAgentX player in padelAgents.Values)
        {
            Vector2 currentPlayerLocalPosition = new Vector2(player.gameObject.transform.localPosition.x, player.gameObject.transform.localPosition.z);
            if (Vector2.Distance(position, currentPlayerLocalPosition) < Vector2.Distance(position, nearestPlayerLocalPosition))
            {
                nearestPlayerLocalPosition = currentPlayerLocalPosition;
                nearestPlayerId = player.playerId;
            }
        }
        return nearestPlayerId;
    }
}
