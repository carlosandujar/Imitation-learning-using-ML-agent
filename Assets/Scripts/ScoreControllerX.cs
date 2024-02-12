using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreControllerX : MonoBehaviour
{

    private int T1Points, T2Points, T1ConsecutivePoints, T2ConsecutivePoints;

    public EnvironmentControllerX environmentController;

    public TextMeshProUGUI ScoreText;

    // Start is called before the first frame update
    void Start()
    {
        T1Points = 0;
        T2Points = 0;
        T1ConsecutivePoints = 0;
        T2ConsecutivePoints = 0;
    }

    void DisplayScore()
    {
        if (environmentController.environmentId != 0) return;
        if (!(T1Points == 40 && T2Points == 40))
        {
            ScoreText.text = $"[T1] {T1Points} - {T2Points} [T2]";
        }
        else
        {
            ScoreText.text = $"[T1] {T1ConsecutivePoints} ({T1Points}) - ({T2Points}) {T2ConsecutivePoints} [T2]";
        }
    }

    public void Update()
    {
        DisplayScore();
    }

    public void GivePoint(Team team)
    {
        switch (team)
        {
            case Team.T1:
                if (T1Points == 0) T1Points = 15;
                else if (T1Points == 15) T1Points = 30;
                else if (T1Points == 30) T1Points = 40;
                else
                {
                    if (T2Points != 40)
                    {
                        GiveGame(Team.T1);
                        return;
                    }
                    else
                    {
                        if (T2ConsecutivePoints == 0)
                        {
                            T1ConsecutivePoints += 1;
                        }
                        else
                        {
                            T2ConsecutivePoints = 0;
                        }
                        if (T1ConsecutivePoints == 2)
                        {
                            GiveGame(Team.T1);
                            return;
                        }
                    }
                }
                break;

            case Team.T2:
                if (T2Points == 0) T2Points = 15;
                else if (T2Points == 15) T2Points = 30;
                else if (T2Points == 30) T2Points = 40;
                else
                {
                    if (T1Points != 40)
                    {
                        GiveGame(Team.T2);
                        return;
                    }
                    else
                    {
                        if (T1ConsecutivePoints == 0)
                        {
                            T2ConsecutivePoints += 1;
                        }
                        else
                        {
                            T1ConsecutivePoints = 0;
                        }
                        if (T2ConsecutivePoints == 2)
                        {
                            GiveGame(Team.T2);
                            return;
                        }
                    }
                }
                break;
        }
        environmentController.SwitchServerSide();
        environmentController.ResetScene();
    }

    public void GiveGame(Team team)
    {
        environmentController.UpdateServerSide(Side.Right);
        environmentController.SwitchServerTeam();
        environmentController.SwitchServerPlayer();
        T1Points = 0;
        T2Points = 0;
        T1ConsecutivePoints = 0;
        T2ConsecutivePoints = 0;
        environmentController.ResetScene();
    }
}
