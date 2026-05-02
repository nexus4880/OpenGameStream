# OGS.Client.Desktop (Raylib)

Native desktop client intended to replace browser-driven input capture.

## Implemented web-client parity (setup flow)

- Dedicated configurable exit key selected before connect (F1-F4).
- Invite ingestion flow mirrored from web client:
  - paste invite code from clipboard (`CTRL+V`),
  - decode Base64 JSON invite payload,
  - show invite info screen,
  - confirm to enter session state.
- Session capture behavior with a single reserved key (the configured exit key), while all other keyboard and mouse input is forwarded to the host transport.

## Remaining work

- Hook `InviteData` flows to actual RTC signaling/media client implementation (ManualRtc + MQTT paths).
- Implement gamepad forwarding parity.
- Render remote video/audio stream.
