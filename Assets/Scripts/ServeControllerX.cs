using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServeControllerX : MonoBehaviour
{
    private Side serverSide;
    private Team serverTeam;
    private PlayerId serverPlayerId;

    private bool hasToServe;
    private bool hasToBounce;

    public EnvironmentControllerX environmentController;

    // Start is called before the first frame update
    void Start()
    {
        hasToServe = true;
        hasToBounce = true;
        serverSide = Side.Right;
        serverTeam = Team.T1;
        serverPlayerId = PlayerId.T1_1;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasToServe)
        {
            if (hasToBounce)
            {
                environmentController.BounceOnService();
                hasToBounce = false;
            }
            else
            {
                if (environmentController.BallCanBeServed())
                {
                    Vector3 force = CalculateServingForce();
                    environmentController.ServeBall(serverTeam, serverSide, force);
                    hasToServe = false;
                }
            }
        }
    }

    private Vector3 CalculateServingForce()
    {
        int zGrid = 2;
        int xGrid = -1;
        switch (serverSide)
        {
            case Side.Left:
                xGrid = 3;
                break;
            case Side.Right:
                xGrid = 1;
                break;
        }
        return environmentController.CalculateForce(serverTeam, 2, xGrid, zGrid);
    }

    public void SwitchServerSide()
    {
        switch (serverSide)
        {
            case Side.Left:
                serverSide = Side.Right;
                break;
            case Side.Right:
                serverSide = Side.Left;
                break;
        }
    }

    public void UpdateServerSide(Side side)
    {
        serverSide = side;
    }

    public void SwitchServerTeam()
    {
        switch (serverTeam)
        {
            case Team.T1:
                serverTeam = Team.T2;
                break;
            case Team.T2:
                serverTeam = Team.T1;
                break;
        }
    }

    public void SwitchServerPlayer()
    {
        switch (serverPlayerId)
        {
            case PlayerId.T1_1:
                serverPlayerId = PlayerId.T2_1;
                break;
            case PlayerId.T1_2:
                serverPlayerId = PlayerId.T2_2;
                break;
            case PlayerId.T2_1:
                serverPlayerId = PlayerId.T1_2;
                break;
            case PlayerId.T2_2:
                serverPlayerId = PlayerId.T1_1;
                break;
        }
    }

    public void SetHasToServe(bool hasToServe)
    {
        this.hasToServe = hasToServe;
    }

    public void SetHasToBounce(bool hasToBounce)
    {
        this.hasToBounce = hasToBounce;
    }

    public Team GetServerTeam() { return serverTeam; }

    public Side GetServerSide() { return serverSide; }

    public PlayerId GetServerPlayerId() { return serverPlayerId; }
}
