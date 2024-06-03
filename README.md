# Apply imitation learning using ML-agend in virtual environment of padel

Project based on https://github.com/jialongjq/tfg?tab=readme-ov-file

## Descripción general
Un proyecto desarrollado en Unity que permite entrenar agentes en un entorno de virtual de pádel basado en Unity, se puede entrena mediante aprendizaje por refuerzo o aprendizaje por imitación, utilizando el toolkit de
[Unity ML-Agents](https://github.com/Unity-Technologies/ml-agents). 

Los directorios <code>Assets</code>, <code>Packages</code>, <code>Project Settings</code> y <code>User Settings</code> son los necesarios para abrir el proyecto en Unity.

El directorio <code>config</code> contiene los archivos de configuración del entrenamiento de agentes en el entorno virtual de pádel.

## Requerimientos
Este proyecto se ha probado únicamente en Windows 10 y 11. Para la ejecución del proyecto se requiere la versión 2021.3.22f1 de Unity y la versión 20 de ML-Agents. Para el entrenamiento de agentes se requiere la versión 3.7.9 de Python (otras versiones compatibles: 3.9).

## Instalación del proyecto en Unity
Los pasos a seguir para la ejecución del entorno virtual de pádel desde Unity son los siguientes:
<ol>
<li>Instalar la versión 2021.3.22f1 de Unity, preferiblemente a través de Unity Hub.</li>

<li>Descargar la [Versión 20] (https://github.com/Unity-Technologies/ml-agents/releases/tag/release_20) de ML-Agents desde el repositorio oficial. La carpeta descargada ml-agents-release-20 contiene el paquete de Unity necesario para la ejecución del entorno.</li>
<li>Clonar este repositorio y abrirlo desde Unity, en Modo Seguro.</li>
<li>Para añadir el paquete de Unity al proyecto:</li>
    <ul>
      <li>Navegar hasta el menú Window -> Package Manager.</li>
      <li>Hacer click al botón + (situado en esquina superior izquierda del menú)</li>
      <li>Seleccionar Add package from disk...</li>
      <li>Navegar hasta la carpeta de com.unity.ml-agents (dentro de la carpeta ml-agents-release-20)</li>
      <li>Seleccionar el archivo package.json.</li>
    </ul>
<li>En este punto, se deberían haber detectado todos los componentes procedentes de **ML-Agents** (Agent, Behavior Parameters, Decision Requester...) y se debería poder ejecutar la escena <code>Scenes\Padel2vs2</code> .</li>
</ol>

<li></li>
## Instalación del paquete de Python
Los pasos a seguir para entrenar agentes son los siguientes:
<ol>
<li>Crear y activar un entorno virtual de Python en este caso usamos [Anaconda](https://www.anaconda.com/download):</li>
<li>Desde el entorno virtual de Python, lo primero es instalar las dependencias de <code>ml-agents</code>:
</li>

```
python -m pip install --upgrade pip
pip install torch torchvision torchaudio
pip install protobuf==3.20.3
pip install six
```

<li>Instalar <code>ml-agents</code> y comprobar que se haya instalado correctamente:</li>

```
pip install mlagents
mlagents-learn --help
```
<li>Para entrenar agentes, el comando básico es:</li>

```
mlagents-learn <trainer-config-file> --run-id=<run-identifier> --time-scale=x
```
<ul>
    <li><code>trainer-config-file</code>: Fichero .yaml donde configura el hiperparametro de entrenamiento.</li>
    <li><code>run-identifier</code>:Define el nombre de entrenamiento.</li>
    <li><code>time-scale</code>: Velocidad de entrenamiento 1-20. </li>
</ul>

<li>Comando para visualizar las gráficas de entrenamiento: </li>

```
tensorboard --logdir results/<run-identifier> --port 6006
```

Una guía más detallada sobre cómo entrenar agentes se puede consultar [aquí] (https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Training-ML-Agents.md).
</ol>