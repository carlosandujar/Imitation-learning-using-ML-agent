using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrajectoryController : MonoBehaviour
{

    public EnvironmentControllerX environmentController;

    void Start()
    {
        CreatePhysicsScene();
    }

    private Scene simulationScene;
    private PhysicsScene physicsScene;
    [SerializeField] private Transform obstaclesParent;
    
    void CreatePhysicsScene()
    {
        simulationScene = SceneManager.CreateScene("Simulation" + environmentController.GetEnvironmentId(), new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        physicsScene = simulationScene.GetPhysicsScene();

        foreach (Transform obj in obstaclesParent)
        {
            var ghostObj = Instantiate(obj.gameObject, obj.position, obj.rotation);
            SceneManager.MoveGameObjectToScene(ghostObj, simulationScene);
        }
    }

    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int maxPhysicsFrameIterations;

    // NOTE: Whatever is instantiated in the physics scene is in world space: there are no parent transforms.
    public void SimulateTrajectory(GhostBall ghostBallPrefab, Vector3 pos, Vector3 velocity, Team hitByTeam)
    {
        GhostBall ghostBall = Instantiate(ghostBallPrefab, pos, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(ghostBall.gameObject, simulationScene);

        ghostBall.Init(velocity);
        lineRenderer.positionCount = 0;
        if (environmentController.DebugMode)
        {
            lineRenderer.positionCount = maxPhysicsFrameIterations;
        }
        ghostBall.SetTimesCollidedWithFloor(0);
        float accumulatedTime = 0;
        environmentController.SetSimulationCompleted(false);
        for (var i = 0; i < maxPhysicsFrameIterations; i++)
        {
            physicsScene.Simulate(Time.fixedDeltaTime);
            accumulatedTime += Time.fixedDeltaTime;
            environmentController.AnalyzeKeyPosition(ghostBall.transform.position, accumulatedTime, hitByTeam);
            if (environmentController.DebugMode && lineRenderer.positionCount > 0)
            {
                lineRenderer.SetPosition(i, ghostBall.transform.position);
            }
            if (ghostBall.GetTimesCollidedWithFloor() >= 2)
            {
                lineRenderer.positionCount = i;
                break;
            }
        }
        environmentController.SetSimulationCompleted(true);
        Destroy(ghostBall.gameObject);
    }

    public void ClearTrajectory()
    {
        lineRenderer.positionCount = 0;
    }
}
