# Reinforcement Learning and Imitation Learning using ML-Agents in a Virtual Padel Environment

Project based on [Repository](https://github.com/jialongjq/tfg?tab=readme-ov-file)

## Overview

This project is developed in Unity and allows training agents in a virtual padel environment based on Unity, utilizing either reinforcement learning or imitation learning, with the help of the [Unity ML-Agents](https://github.com/Unity-Technologies/ml-agents) toolkit.

The directories <code>Assets</code>, <code>Packages</code>, <code>Project Settings</code>, and <code>User Settings</code> are necessary to open the project in Unity.

The <code>results and config</code> directory contains the configuration files for training agents in the virtual environment, it also contains already trained models.

The <code>demos</code> directory contains different videos of the training results.

## Requirements
This project has been tested only on Windows 10 and 11. To run the project, Unity version 2021.3.22f1 and ML-Agents version 20 are required. For agent training, Python version 3.7.9 is needed (other compatible versions: 3.9).

## Project Installation in Unity

To run the virtual padel environment from Unity, follow these steps:

1. Install Unity version 2021.3.22f1, preferably via Unity Hub.

2. Download [ML-Agents Version 20](https://github.com/Unity-Technologies/ml-agents/releases/tag/release_20) from the official repository. The downloaded folder `ml-agents-release-20` contains the Unity package necessary for running the environment.

3. Clone this repository and open it from Unity in Safe Mode.

4. To add the Unity package to the project:
    - Navigate to the menu `Window -> Package Manager`.
    - Click the `+` button (located in the top left corner of the menu).
    - Select `Add package from disk...`.
    - Navigate to the `com.unity.ml-agents` folder (inside the `ml-agents-release-20` folder).
    - Select the `package.json` file.

5. At this point, all components from **ML-Agents** (Agent, Behavior Parameters, Decision Requester, etc.) should be detected, and the scene `Scenes\Padel2vs2` should be ready to run.

## Python Package Installation

To train agents, follow these steps:

1. Create and activate a Python virtual environment; in this case, we use [Anaconda](https://www.anaconda.com/download):

    ```bash
    # Commands to create and activate the environment in Anaconda
    conda create -n myenv python=3.8
    conda activate myenv
    ```

2. From the Python virtual environment, first install the `ml-agents` dependencies:

    ```bash
    python -m pip install --upgrade pip
    pip install torch torchvision torchaudio
    pip install protobuf==3.20.3
    pip install six
    ```

3. Install `ml-agents` and verify that it has been installed correctly:

    ```bash
    pip install mlagents
    mlagents-learn --help
    ```

4. To train agents, the basic command is:

    ```bash
    mlagents-learn <trainer-config-file> --run-id=<run-identifier> --time-scale=x
    ```

    - `<trainer-config-file>`: YAML file where training hyperparameters are configured.
    - `<run-identifier>`: Defines the training name.
    - `<time-scale>`: Training speed (1-20).

5. Command to visualize training graphs:

    ```bash
    tensorboard --logdir results/<run-identifier> --port 6006
    ```

A more detailed guide on how to train agents can be found [here](https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Training-ML-Agents.md).