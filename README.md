# Hands-Free VR Unity Client

This repository contains the Unity VR client used for **Hands-Free VR**, a voice-based natural-language interface for virtual reality. The project accompanies the paper:

> Jorge Askur Vazquez Fernandez, Jae Joong Lee, Santiago Andres Serrano Vacca, Alejandra Magana, Radim Pesa, Bedrich Benes, and Voicu Popescu. "Hands-Free VR." HUCAPP 2025.

Hands-Free VR lets a user issue spoken commands in a VR headset. The headset records audio, sends it to an edge server, receives an executable command, and applies that command in the Unity scene. In the paper, the server pipeline uses a fine-tuned Whisper speech-to-text model and an LLM/RAG text-to-command mapper. This repository is the Unity-side client and study environment.

## Project Overview

The VR task environment compares two interaction conditions:

- **Conventional VR interface**: users grab, carry, and place objects with controller-based manipulation.
- **Experimental Hands-Free VR interface**: users press a trigger to record a spoken command, send the audio to the external server, and receive an executable command such as object selection, box placement, reset, or object arrangement.

The study scenes focus on object selection and arrangement tasks using cubes, cylinders, spheres, hemispheres, and pyramids with different colors. The client logs task completion time, command execution results, headset movement, view rotation, and hand movement.

## Repository Contents

- `Assets/Scenes/Conventional.unity`: conventional controller-based condition.
- `Assets/Scenes/Experimental.unity`: Hands-Free VR voice-command condition.
- `Assets/Scenes/Tutorial.unity`: tutorial/practice scene.
- `Assets/Scripts/MasterManager.cs`: parses executable commands and applies object actions.
- `Assets/Scripts/TCPClientDemo.cs`: TCP client, microphone capture, WAV export, server communication, and logging.
- `Assets/Scripts/TaskManager.cs`: study task generation, progression, metrics, and questionnaire logging.
- `Assets/Scripts/MenuManager.cs`: VR menu state, language config switching, and task controls.
- `Assets/Scripts/GroupManager.cs`: selected object grouping and duplication behavior.
- `Assets/Scripts/MarkerManager.cs` and `Assets/Scripts/AssemblyManager.cs`: task completion checks.
- `Packages/manifest.json`: Unity package dependencies.
- `Conventional.apk` and `Experimental.apk`: prebuilt Android APKs included in the repository.
- `APX/VoiceUI.unitypackage`: exported Unity package.

## Requirements

### Development

- Unity **2021.3.8f1**
- Android Build Support for Unity
- Meta/Oculus XR runtime and Quest-compatible tooling
- A Meta Quest headset. The paper reports Meta Quest 3; the current Android manifest lists Quest 2 and Quest Pro support.

### Runtime

- VR headset and edge server on the same reachable network
- Microphone permission on the headset
- TCP server listening on the same port configured in Unity
- External speech-to-command server implementation

The paper's server was run on a workstation with an Intel i9-12900K CPU, 128 GB RAM, NVIDIA RTX 3090 GPU, Python 3.9, PyTorch 2.0.1, and Wi-Fi 6E. That server code, model weights, and generated fine-tuning data are not included in this Unity repository.

## Setup

1. Install Unity 2021.3.8f1 with Android Build Support.
2. Open this repository as a Unity project.
3. Let Unity restore packages from `Packages/manifest.json`.
4. Open one of the main scenes:
   - `Assets/Scenes/Experimental.unity` for Hands-Free VR.
   - `Assets/Scenes/Conventional.unity` for the control condition.
   - `Assets/Scenes/Tutorial.unity` for practice.
5. In the scene, find the GameObject with `TCPClientDemo`.
6. Set `serverIPAddress` to the IP address of the edge server.
7. Confirm that the server port matches the serialized `port` value in `TCPClientDemo` (`65432` by default).
8. Add the desired scene to Unity's Build Settings if it is not already listed.
9. Build and deploy to the headset as an Android APK.

## Network Protocol

The Unity client sends recorded audio to the server over TCP and then sends an `EOF_MARKER` string. The server should respond with either:

- an executable command string containing parentheses, such as `select(cube, red)`, `align(circle)`, `box(add)`, `reset()`, or `finish()`;
- a status/transcription string such as `STT:<text>`;
- `idonotunderstand` when the command cannot be mapped.

Executable commands are handled by `MasterManager.userIntention`.

## Supported Runtime Actions

The current client code supports these high-level actions:

- Select objects by type and/or color.
- Move selected objects to or from the box.
- Arrange selected objects in a row, circle, or matrix.
- Reset the active task.
- Finish the current task when completion criteria are met.
- Switch dictation language configuration between English and Spanish assets.

The exact natural-language phrasing is handled by the external server. Unity receives only the mapped executable command.

## Logs and Study Data

Runtime logs are written under Unity's `Application.persistentDataPath` on the target device. `TCPClientDemo` creates timestamped log files and records:

- experiment start/end markers;
- speech-to-text output;
- executable command output;
- command execution success/failure;
- TCP send/response timing;
- task completion timing;
- headset position and rotation samples;
- left and right hand position samples;
- questionnaire answers.

## Notes for Reproduction

- This repository is the Unity client and VR task environment, not the full server pipeline.
- Reproducing the paper's Hands-Free VR condition requires an external server that performs speech-to-text and text-to-command mapping.
- `ProjectSettings/EditorBuildSettings.asset` currently has no scenes listed, so add the active scene before building if Unity does not do this automatically.
- The Android manifest currently requests external storage permissions but does not explicitly request microphone or internet permissions. Verify generated Android permissions during deployment.
- The included APKs may not reflect the latest local scene or script changes.

## Citation

If you use this project in academic work, cite the Hands-Free VR paper:

```bibtex
@inproceedings{vazquezfernandez2025handsfreevr,
  title = {Hands-Free VR},
  author = {Vazquez Fernandez, Jorge Askur and Lee, Jae Joong and Serrano Vacca, Santiago Andres and Magana, Alejandra and Pesa, Radim and Benes, Bedrich and Popescu, Voicu},
  booktitle = {Proceedings of HUCAPP 2025},
  year = {2025}
}
```
