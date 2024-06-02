using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;
using UnityEditor;
using static UnityEngine.GraphicsBuffer;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Linq;
using System.Globalization;

using ExcelDataReader;
//using NumSharp;
using KdTree;
using System.Runtime.InteropServices.ComTypes;
using NetMQ;
using NetMQ.Sockets;
using AsyncIO;
using UnityEditor.Search;
public class CoachController : MonoBehaviour
{

    private RequestSocket client;
    private bool isConnected = false;
    private byte[] buffer = new byte[1024];

    public EnvironmentControllerX environmentControllerX;

    private Dictionary<string, PlayerId> playerIdMapping;
    private Dictionary<string, int> hitTypeMapping;
    // Start is called before the first frame update

    private Vector2[][] T1SideGrid;
    private Vector2[][] T2SideGrid;
    private Vector2[][] T1MoveGrid;
    private Vector2[][] T2MoveGrid;

    Queue<string> commandsQueue = new Queue<string>();

    Stopwatch stopwatch;
    Stopwatch stopwatch2;

    void Start()
    {
        //LoadData();
        if (environmentControllerX.RecordingDemonstrations)
        {
            try
            {

                // Initialize ZeroMQ client
                AsyncIO.ForceDotNet.Force();
                client = new RequestSocket();
                client.Connect("tcp://127.0.0.1:5555");
                isConnected = true;
                UnityEngine.Debug.Log("Connected to the server via ZeroMQ");
            }
            catch (SocketException ex)
            {
                UnityEngine.Debug.LogError($"SocketException: {ex.Message}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Exception: {ex.Message}");
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("environmentControllerX is null or RecordingDemonstrations is false");
        }


        playerIdMapping = new Dictionary<string, PlayerId>();
        playerIdMapping["T1_1"] = PlayerId.T1_1;
        playerIdMapping["T1_2"] = PlayerId.T1_2;
        playerIdMapping["T2_1"] = PlayerId.T2_1;
        playerIdMapping["T2_2"] = PlayerId.T2_2;
        hitTypeMapping = new Dictionary<string, int>();
        hitTypeMapping["normal"] = 1;
        hitTypeMapping["lob"] = 4;
        hitTypeMapping["smash"] = 5;
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

        T1MoveGrid = new Vector2[5][];
        T2MoveGrid = new Vector2[5][];
        for (int i = 0; i < 5; i++)
        {
            T1MoveGrid[i] = new Vector2[5];
            T2MoveGrid[i] = new Vector2[5];
            for (int j = 0; j < 5; j++)
            {
                T1MoveGrid[i][j] = new Vector2(-2 * (10f / 6) + (10f / 6) * i, -((10f / 6) + (10f / 6) * j));
                T2MoveGrid[i][j] = new Vector2(-T1MoveGrid[i][j][0], -T1MoveGrid[i][j][1]);
            }
        }
        stopwatch = new Stopwatch();
        stopwatch2 = new Stopwatch();
        
    }
    void OnDestroy()
    {
        if (client != null)
        {
            client.Close();
            client.Dispose();
            isConnected = false;
            NetMQConfig.Cleanup();
            UnityEngine.Debug.Log("Disconnected from the server via ZeroMQ");
        }
    }

    private void ProcessShotResponse(string[] response)
    {
        PlayerId playerId = playerIdMapping[response[1]];
        // hitType: 0 no hit, 1 derecha / reves, 2 globo, 3 remate
        int hitType = hitTypeMapping[response[2]];
        float xTarget = float.Parse(response[3]) - 5;
        float zTarget = float.Parse(response[4]) - 10;
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

        Vector3 TLposition = new Vector3(float.Parse(response[2]) - 5, 0f, float.Parse(response[3]) - 10);
        Vector3 TRposition = new Vector3(float.Parse(response[4]) - 5, 0f, float.Parse(response[5]) - 10);
        Vector3 BLposition = new Vector3(float.Parse(response[6]) - 5, 0f, float.Parse(response[7]) - 10);
        Vector3 BRposition = new Vector3(float.Parse(response[8]) - 5, 0f, float.Parse(response[9]) - 10);
        Vector3 TLspeed = new Vector3(float.Parse(response[10]), 0f, float.Parse(response[11]));
        Vector3 TRspeed = new Vector3(float.Parse(response[12]), 0f, float.Parse(response[13]));
        Vector3 BLspeed = new Vector3(float.Parse(response[14]), 0f, float.Parse(response[15]));
        Vector3 BRspeed = new Vector3(float.Parse(response[16]), 0f, float.Parse(response[17]));


        float Maxspeed = environmentControllerX.GetMaxspeed_Agent();
        
        Vector3[] movement = new Vector3[4] { TLspeed , TRspeed, BLspeed, BRspeed};
        Vector3[] position = new Vector3[4] { TLposition, TRposition, BLposition, BRposition };

        for (int i = 0; i < 4; i++)
        {
            movement[i].x = Mathf.Clamp(movement[i].x, -Maxspeed, Maxspeed);
            movement[i].y = Mathf.Clamp(movement[i].y, -Maxspeed, Maxspeed);
            movement[i].z = Mathf.Clamp(movement[i].z, -Maxspeed, Maxspeed);
        }

        for (int i = 0; i < 4; i++)
        {

            float magnitudeMovement = movement[i].magnitude;
            float speed = Maxspeed;
            Vector3 direction = movement[i] / speed;

            Vector3 posPlayer = position[i];
            Vector3 targetPos = posPlayer + direction * magnitudeMovement;

            float xTarget = targetPos.x;
            float zTarget = targetPos.z;

            int xGrid = 0, zGrid = 0;
            Vector2[][] Grid;
            if (i>1)
            {
                Grid = T1MoveGrid;
            }
            else
            {
                Grid = T2MoveGrid;
            }
            
            Vector2 target = new Vector2(xTarget, zTarget);
            for (int x = 0; x < 5; x++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (Vector2.Distance(target, Grid[x][j]) < Vector2.Distance(target, Grid[xGrid][zGrid]))
                    {
                        xGrid = x;
                        zGrid = j;
                    }
                }
            }

            int[] Action = new int[2];
            Action[0] = xGrid;
            Action[1] = zGrid;

            PlayerId nearestToPos = environmentControllerX.GetNearestPlayerToPosition(new Vector2(position[i].x, position[i].z));

            environmentControllerX.SendCoachedMovement(nearestToPos, Action);

        }
        if (environmentControllerX.environmentId == 0)
        {
            stopwatch.Stop();
            float tiempoEjecucion = stopwatch.ElapsedMilliseconds;
            UnityEngine.Debug.Log("Tiempo de Enviar y calcular: " + tiempoEjecucion + " ms");
            stopwatch.Reset();
        }
        someoneAlreadyRequested = false;
    }

    private bool isWaitingForResponse = false;

    void Update()
    {
        if (environmentControllerX.RecordingDemonstrations)
        {
            if (client == null && !isConnected)
            {
                // Handle disconnection or try to reconnect
                UnityEngine.Debug.Log("Not connected to the server");
                return;
            }

            //if (stream.DataAvailable)
            //{
            //    if (environmentControllerX.environmentId == 0)
            //    {
            //        UnityEngine.Debug.Log("Recibido ");
            //        stopwatch2.Stop();
            //        float tiempoEjecucion = stopwatch2.ElapsedMilliseconds;
            //        UnityEngine.Debug.Log("Tiempo de Enviar y recibir: " + tiempoEjecucion + " ms");
            //        stopwatch2.Reset();
            //    }
            //    int bytesRead = stream.Read(buffer, 0, buffer.Length);
            //    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            //    string[] responses = receivedData.Split('\n');
            //    foreach (string unsplitted_response in responses)
            //    {
            //        string[] response = unsplitted_response.Split(" ");
            //        if (response[0] == "SHOT_RESPONSE")
            //        {
            //            ProcessShotResponse(response);
            //        }
            //        else if (response[0] == "MOVEMENT_RESPONSE")
            //        {
            //            ProcessMovementResponse(response);
            //        }
            //    }
            //}

            if (isWaitingForResponse)
            {
                string receivedData = string.Empty;
                if (client.TryReceiveFrameString(out receivedData))
                {
                    UnityEngine.Debug.Log("Received: " + receivedData);
                    isWaitingForResponse = false; // Mark that we've received the response
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
    }


    void SendCommandToPython(string command)
    {
        if (environmentControllerX.environmentId == 0)
        {
           
            stopwatch.Start();
            stopwatch2.Start();
        }

        if (isConnected && !isWaitingForResponse)
        {
            isWaitingForResponse = true;
            // Send command to Python script
            client.SendFrame(command);
        }
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

    //public static DataSet dataSet;

    //public static DataTable topPositionData;
    //public static DataTable bottomPositionData;
    //public static DataTable topShotData;
    //public static DataTable bottomShotData;


    //public void LoadData()
    //{
    //    string folderPath = Directory.GetCurrentDirectory()+ "\\Assets\\Scripts";
    //    string excelFileName = "data.csv";

    //    string excelFilePath = Path.Combine(folderPath, excelFileName);

    //    // Create a FileStream to read the Excel file
    //    using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read))
    //    {
    //        // Create an ExcelDataReader to read the data from the Excel file
    //        using (var reader = ExcelReaderFactory.CreateCsvReader(stream, new ExcelReaderConfiguration()
    //        {
    //            FallbackEncoding = Encoding.GetEncoding(1252), // Encoding for Western European (Windows)
    //            AutodetectSeparators = new char[] { ',', ';', '\t' }, // Autodetect separators
    //            LeaveOpen = false // Close the reader after data is read
    //        }))
    //        {
    //            // Read the data into a DataSet
    //            DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
    //            {
    //                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
    //                {
    //                    UseHeaderRow = true // Treat the first row as header (containing column names)
    //                }
    //            });

    //            // Extract the DataTable from the DataSet
    //            DataTable data = result.Tables[0];

    //            // Remove unnecessary columns
    //            string[] columnsToDrop = { "Column0", "frame", "time", "role1", "role2", "role3", "role4", "fromx", "fromy", "duration", "shot_full", "rally" };
    //            foreach (string column in columnsToDrop)
    //            {
    //                if (data.Columns.Contains(column))
    //                {
    //                    data.Columns.Remove(column);
    //                }
    //            }

    //            // Filter data by column
    //            topPositionData = data.AsEnumerable().Where(row => row.Field<string>("lastHit") == "T").CopyToDataTable();
    //            bottomPositionData = data.AsEnumerable().Where(row => row.Field<string>("lastHit") == "B").CopyToDataTable();
    //            topShotData = topPositionData.AsEnumerable().Where(row => row.Field<string>("shot") != "undef").CopyToDataTable();
    //            bottomShotData = bottomPositionData.AsEnumerable().Where(row => row.Field<string>("shot") != "undef").CopyToDataTable();
    //        }
    //    }

    //}

}