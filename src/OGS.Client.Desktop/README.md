# OGS.Client.Desktop (Raylib)

This project is a desktop-first client shell intended to replace browser-based input capture.

## Current scope

- Fullscreen-native desktop window via Raylib.
- Setup screen requiring a dedicated configurable **exit key** (F1-F4).
- Runtime input capture toggle using only the configured exit key.
- Keyboard, mouse movement, mouse buttons, and mouse wheel are encoded according to the same command schema as the existing web client (`RtcCommandType`).
- A transport abstraction (`IRemoteCommandTransport`) is provided for wiring into WebRTC/data-channel signaling.

## Remaining integration work

- Implement an `IRemoteCommandTransport` backed by the OGS RTC command channel.
- Render host video frames in-window.
- Add gamepad forwarding parity with the web client.
- Add host connection UI (manual invite + MQTT invite flow).

This staged approach removes browser shortcut limitations immediately at the architecture level while keeping protocol compatibility.
