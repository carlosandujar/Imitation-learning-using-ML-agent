# Configuración del entrenamiento de los agentes

Para el entrenamiento de agentes es necesario un fichero de configuración del entrenamiento. Los ficheros que se han utilizado durante el desarrollo del proyecto son `config/padel_ppo_config.yaml` y `config/padel_ppo+curiosity_config.yaml`.
Específicamente para este entorno, donde las físicas son importantes, se debe definir la opción `--time-scale=1` para evitar un desfase en las físicas.

Un ejemplo de comando para el entrenamiento de agentes es, desde la carpeta del proyecto:
```
mlagents-learn \config\padel_ppo_config.yaml --run-id=PadelPPO_EXP1 --time-scale=1
```

Además de la configuración del entrenamiento, también es posible cambiar la función de recompensa cambiando los valores correspondientes de `WinningReward`, `LosingReward`, `ApproachingKeyPositionsReward`, `StayingAroundKeyPositionsReward` y `HittingBallReward`,
los cuales se encuentran en el script `Assets/Scripts/EnvironmentControllerX.cs`.

## Experimentos

A continuación se detallan los experimentos realizados durante el desarrollo del proyecto. Los modelos que se han obtenido en cada experimento se encuentran en la carpeta `results`.

### Experimento 1. Entrenamiento simple mediante PPO
- Fichero de configuración utilizada: `config/padel_ppo_config.yaml`
- Recompensas utilizadas:
```
public const float WinningReward = 10;
public const float LosingReward = -10;
public const float ApproachingKeyPositionsReward = 0f;
public const float StayingAroundKeyPositionsReward = 0f;
public const float HittingBallReward = 0f;
```
### Experimento 2. Efecto de Curiosity en el entorno virtual de pádel
- Fichero de configuración utilizada: `config/padel_ppo+curiosity_config.yaml`
- Recompensas utilizadas:
```
public const float WinningReward = 10;
public const float LosingReward = -10;
public const float ApproachingKeyPositionsReward = 0f;
public const float StayingAroundKeyPositionsReward = 0f;
public const float HittingBallReward = 0f;
```
### Experimento 3. Influencia de la recompensa por golpeo
- Fichero de configuración utilizada: `config/padel_ppo_config.yaml`
- Recompensas utilizadas:
```
Experimento 3:
Curiosity [NO]
public const float WinningReward = 10;
public const float LosingReward = -10;
public const float ApproachingKeyPositionsReward = 0f;
public const float StayingAroundKeyPositionsReward = 0f;
public const float HittingBallReward = 1f;
```
### Experimento 4. Importancia del sistema de puntuación Elo

- Fichero de configuración utilizada: `config/padel_ppo_config.yaml`
- Recompensas utilizadas:
```
public const float WinningReward = 0;
public const float LosingReward = -10;
public const float ApproachingKeyPositionsReward = 0f;
public const float StayingAroundKeyPositionsReward = 0f;
public const float HittingBallReward = 1f;
```

### Experimento 5. Influencia de las posiciones clave
- Fichero de configuración utilizada: `config/padel_ppo_config.yaml`
- Recompensas utilizadas:
```
public const float WinningReward = 10;
public const float LosingReward = -10;
public const float ApproachingKeyPositionsReward = 0.005f;
public const float StayingAroundKeyPositionsReward = 0.01f;
public const float HittingBallReward = 0f;
```
