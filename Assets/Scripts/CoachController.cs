using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;
using UnityEditor;

public class CoachController : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];

    public EnvironmentControllerX environmentControllerX;

    private Dictionary<string, PlayerId> playerIdMapping;
    private Dictionary<string, int> hitTypeMapping;
    // Start is called before the first frame update

    private Vector2[][] T1SideGrid;
    private Vector2[][] T2SideGrid;

    Queue<string> commandsQueue = new Queue<string>();

    void Start()
    {
        if (environmentControllerX.RecordingDemonstrations)
        {
            client = new TcpClient("127.0.0.1", 5555);
            stream = client.GetStream();
        }
        playerIdMapping = new Dictionary<string, PlayerId>();
        playerIdMapping["T1_1"] = PlayerId.T1_1;
        playerIdMapping["T1_2"] = PlayerId.T1_2;
        playerIdMapping["T2_1"] = PlayerId.T2_1;
        playerIdMapping["T2_2"] = PlayerId.T2_2;
        hitTypeMapping = new Dictionary<string, int>();
        hitTypeMapping["normal"] = 1;
        hitTypeMapping["lob"] = 2;
        hitTypeMapping["smash"] = 3;
        T1SideGrid = new Vector2[5][];
        T2SideGrid = new Vector2[5][];
        for (int i = 0; i < 5; i++)
        {
            T1SideGrid[i] = new Vector2[5];
            T2SideGrid[i] = new Vector2[5];
            for (int j = 0; j < 5; j++)
            {
                T2SideGrid[i][j] = new Vector2(-2 * (10f / 6) + (10f / 6) * i, (10f / 6 * 5) - (10f / 6) * j);
                T1SideGrid[i][j] = new Vector2(-T2SideGrid[i][j][0], -T2SideGrid[i][j][1]);
            }
        }
    }

    private void ProcessShotResponse(string[] response)
    {
        PlayerId playerId = playerIdMapping[response[1]];
        // hitType: 0 no hit, 1 derecha / reves, 2 globo, 3 remate
        int hitType = hitTypeMapping[response[2]];
        float xTarget = float.Parse(response[3].Replace(".", ",")) - 5;
        float zTarget = float.Parse(response[4].Replace(".", ",")) - 10;
        Vector2 target = new Vector2(xTarget, zTarget);
        int xGrid = 0, zGrid = 0;
        Vector2[][] Grid;
        if (playerId == PlayerId.T1_1 || playerId == PlayerId.T1_2)
        {
            Grid = T2SideGrid;
        }
        else
        {
            Grid = T1SideGrid;
        }
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (Vector2.Distance(target, Grid[i][j]) < Vector2.Distance(target, Grid[xGrid][zGrid]))
                {
                    xGrid = i;
                    zGrid = j;
                }
            }
        }
        int[] shot = new int[3];
        shot[0] = xGrid;
        shot[1] = zGrid;
        shot[2] = hitType;
        environmentControllerX.SendCoachedShot(playerId, shot);
    }

    private void ProcessMovementResponse(string[] response)
    {
        Vector2 TLposition = new Vector2(float.Parse(response[2].Replace(".", ",")) - 5, float.Parse(response[3].Replace(".", ",")) - 10);
        Vector2 TRposition = new Vector2(float.Parse(response[4].Replace(".", ",")) - 5, float.Parse(response[5].Replace(".", ",")) - 10);
        Vector2 BLposition = new Vector2(float.Parse(response[6].Replace(".", ",")) - 5, float.Parse(response[7].Replace(".", ",")) - 10);
        Vector2 BRposition = new Vector2(float.Parse(response[8].Replace(".", ",")) - 5, float.Parse(response[9].Replace(".", ",")) - 10);
        Vector2 TLspeed = new Vector2(float.Parse(response[10].Replace(".", ",")), float.Parse(response[11].Replace(".", ",")));
        Vector2 TRspeed = new Vector2(float.Parse(response[12].Replace(".", ",")), float.Parse(response[13].Replace(".", ",")));
        Vector2 BLspeed = new Vector2(float.Parse(response[14].Replace(".", ",")), float.Parse(response[15].Replace(".", ",")));
        Vector2 BRspeed = new Vector2(float.Parse(response[16].Replace(".", ",")), float.Parse(response[17].Replace(".", ",")));



        int[] TLAction = new int[2];
        if (TLspeed[1] < 0) // Z AXIS. FORWARD
        {
            TLAction[0] = 1; // FORWARD AXIS
        }
        else if (TLspeed[1] > 0)
        {
            TLAction[0] = 2;
        }
        else
        {
            TLAction[0] = 0;
        }
        if (TLspeed[0] < 0)// X AXIS. RIGHT 
        {
            TLAction[0] = 1;
        }
        else if (TLspeed[0] > 0)
        {
            TLAction[0] = 2;
        }
        else
        {
            TLAction[0] = 0;
        }

        PlayerId nearestToTL = environmentControllerX.GetNearestPlayerToPosition(TLposition);
        environmentControllerX.SendCoachedMovement(nearestToTL, TLAction);




        int[] TRAction = new int[2];
        if (TRspeed[1] < 0) // Z AXIS. FORWARD
        {
            TRAction[0] = 1; // FORWARD AXIS
        }
        else if (TRspeed[1] > 0)
        {
            TRAction[0] = 2;
        }
        else
        {
            TRAction[0] = 0;
        }
        if (TRspeed[0] < 0)// X AXIS. RIGHT 
        {
            TRAction[0] = 1;
        }
        else if (TRspeed[0] > 0)
        {
            TRAction[0] = 2;
        }
        else
        {
            TRAction[0] = 0;
        }

        PlayerId nearestToTR = environmentControllerX.GetNearestPlayerToPosition(TRposition);
        environmentControllerX.SendCoachedMovement(nearestToTR, TRAction);



        int[] BLAction = new int[2];
        if (BLspeed[1] > 0)
        {
            BLAction[0] = 1;
        }
        else if (BLspeed[1] < 0)
        {
            BLAction[0] = 2;
        }
        else
        {
            BLAction[0] = 0;
        }
        if (BLspeed[0] > 0)
        {
            BLAction[1] = 1;
        }
        else if (BLspeed[0] < 0)
        {
            BLAction[1] = 2;
        }
        else
        {
            BLAction[1] = 0;
        }

        PlayerId nearestToBL = environmentControllerX.GetNearestPlayerToPosition(BLposition);
        environmentControllerX.SendCoachedMovement(nearestToBL, BLAction);




        int[] BRAction = new int[2];
        if (BRspeed[1] > 0)
        {
            BRAction[0] = 1;
        }
        else if (BRspeed[1] < 0)
        {
            BRAction[0] = 2;
        }
        else
        {
            BRAction[0] = 0;
        }
        if (BRspeed[0] > 0)
        {
            BRAction[1] = 1;
        }
        else if (BRspeed[0] < 0)
        {
            BRAction[1] = 2;
        }
        else
        {
            BRAction[1] = 0;
        }
        PlayerId nearestToBR = environmentControllerX.GetNearestPlayerToPosition(BRposition);
        environmentControllerX.SendCoachedMovement(nearestToBR, BRAction);

        someoneAlreadyRequested = false;
    } 

    void Update()
    {
        if (environmentControllerX.RecordingDemonstrations)
        {
            if (client == null || !client.Connected)
            {
                // Handle disconnection or try to reconnect
                Debug.Log("Not connected to the server");
                return;
            }

            if (stream.DataAvailable)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string[] responses = receivedData.Split('\n');
                foreach (string unsplitted_response in responses)
                {
                    string[] response = unsplitted_response.Split(" ");
                    if (response[0] == "SHOT_RESPONSE")
                    {
                        ProcessShotResponse(response);
                    }
                    else if (response[0] == "MOVEMENT_RESPONSE")
                    {
                        ProcessMovementResponse(response);
                    }
                }
            }
        }
    }


    void SendCommandToPython(string command)
    {
        // Send command to Python script
        byte[] data = Encoding.ASCII.GetBytes(command);
        stream.Write(data, 0, data.Length);
    }


    bool someoneAlreadyRequested = false;
    public void RequestCoachedMovement(PlayerId playerId, Vector3 selfPosition, Vector3 teammatePosition, Vector3 opponent1Position, Vector3 opponent2Position, Vector3 ballPosition, Team lastHitBy)
    {
        if (!someoneAlreadyRequested)
        {
            Vector2 selfSideLeft, selfSideRight;
            Vector2 opponentSideLeft, opponentSideRight;

            if (selfPosition.x < teammatePosition.x)
            {
                selfSideLeft = new Vector2(selfPosition.x, selfPosition.z);
                selfSideRight = new Vector2(teammatePosition.x, teammatePosition.z);
            }
            else
            {
                selfSideLeft = new Vector2(teammatePosition.x, teammatePosition.z);
                selfSideRight = new Vector2(selfPosition.x, selfPosition.z);
            }

            if (opponent1Position.x < opponent2Position.x)
            {
                opponentSideLeft = new Vector2(opponent1Position.x, opponent1Position.z);
                opponentSideRight = new Vector2(opponent2Position.x, opponent2Position.z);
            }
            else
            {
                opponentSideLeft = new Vector2(opponent2Position.x, opponent2Position.z);
                opponentSideRight = new Vector2(opponent1Position.x, opponent1Position.z);
            }
            Vector2 TL, TR;
            Vector2 BL, BR;
            if (playerId == PlayerId.T1_1 || playerId == PlayerId.T1_2)
            {
                TL = opponentSideLeft; TR = opponentSideRight;
                BL = selfSideLeft; BR = selfSideRight;
            }
            else
            {
                TL = selfSideLeft; TR = selfSideRight;
                BL = opponentSideLeft; BR = opponentSideRight;
            }

            string command = $"MOVEMENT_REQUEST {playerId} {TL[0] + 5} {TL[1] + 10} {TR[0] + 5} {TR[1] + 10} {BL[0] + 5} {BL[1] + 10} {BR[0] + 5} {BR[1] + 10} {ballPosition.x + 5} {ballPosition.z + 10} {lastHitBy}\n";
            SendCommandToPython(command);
            someoneAlreadyRequested = true;
        }
    }

    public void RequestCoachedShot(PlayerId playerId, Vector3 selfPosition, Vector3 teammatePosition, Vector3 opponent1Position, Vector3 opponent2Position, Vector3 ballPosition)
    {
        Vector2 selfSideLeft, selfSideRight;
        Vector2 opponentSideLeft, opponentSideRight;

        if (selfPosition.x < teammatePosition.x)
        {
            selfSideLeft = new Vector2(selfPosition.x, selfPosition.z);
            selfSideRight = new Vector2(teammatePosition.x, teammatePosition.z);
        }
        else
        {
            selfSideLeft = new Vector2(teammatePosition.x, teammatePosition.z);
            selfSideRight = new Vector2(selfPosition.x, selfPosition.z);
        }

        if (opponent1Position.x < opponent2Position.x)
        {
            opponentSideLeft = new Vector2(opponent1Position.x, opponent1Position.z);
            opponentSideRight = new Vector2(opponent2Position.x, opponent2Position.z);
        }
        else
        {
            opponentSideLeft = new Vector2(opponent2Position.x, opponent2Position.z);
            opponentSideRight = new Vector2(opponent1Position.x, opponent1Position.z);
        }
        Vector2 TL, TR;
        Vector2 BL, BR;
        if (playerId == PlayerId.T1_1 || playerId == PlayerId.T1_2)
        {
            TL = opponentSideLeft; TR = opponentSideRight;
            BL = selfSideLeft;     BR = selfSideRight;
        }
        else
        {
            TL = selfSideLeft;     TR = selfSideRight;
            BL = opponentSideLeft; BR = opponentSideRight;
        }

        string command = $"SHOT_REQUEST {playerId} {TL[0] + 5} {TL[1] + 10} {TR[0]+5} {TR[1]+10} {BL[0]+5} {BL[1]+10} {BR[0]+5} {BR[1]+10} {ballPosition.x+5} {ballPosition.z+10}\n";
        SendCommandToPython(command);
    }
}