# Agent Training Configuration

To train agents, a training configuration file is required. The files used during the development of the project are files ending with `.yaml`. 
```
mlagents-learn \config\padel_ppo_config.yaml --run-id=PadelPPO_EXP1 --time-scale=1
```

In addition to the training configuration, it is also possible to modify the reward function by changing the corresponding values of `WinningReward`, `LosingReward`, `ApproachingKeyPositionsReward`, `StayingAroundKeyPositionsReward`, etc. Which are located in the script `Assets/Scripts/EnvironmentControllerX.cs`.






