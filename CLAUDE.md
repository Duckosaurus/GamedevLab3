# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Lab01** is a 3D platformer/jump-and-run game built with Unity (URP 17.4.0). It is an educational project featuring a mage character navigating grass and snow platforms with moving platform mechanics and respawn triggers.

Assets credited in `Lab01/Assets/Readme.txt`:
- Kenney's Platformer Kit (https://kenney-assets.itch.io/platformer-kit)
- Painterly Normals Shader (Unity Asset Store Package 325417)

## Development Commands

This is a Unity project — there are no shell build scripts. All development happens through the Unity Editor.

- **Open project:** Open Unity Hub and add `Lab01/` as a project, or open it directly with Unity 6 (URP 17.4.0 compatible).
- **Run game:** Open `Assets/Scenes/JumpAndRun.unity` in the Unity Editor and press Play.
- **Build:** File → Build Settings → Build (note: `EditorBuildSettings.asset` currently only has `SampleScene` enabled; add `JumpAndRun.unity` if needed).
- **Tests:** The `com.unity.test-framework` package is installed but no tests are authored yet. Use Window → General → Test Runner.

## Code Architecture

All gameplay scripts are in `Lab01/Assets/`:

### `PlayerMovement.cs`
Attached to the `Mage` prefab (`Prefabs/Character/Mage.prefab`). Uses Unity's `CharacterController`. Movement is split into two separate `controller.Move()` calls — one for horizontal XZ movement, one for vertical Y (gravity/jump). Platform surface velocity is sampled via a downward `Physics.Raycast` to the custom **"Platforms"** layer, which adds the platform's velocity to the player each frame. Blocks all input when `GameManager.IsDead` is true.

### `MovingPlatform.cs`
Attached to moving platform GameObjects. Uses `Mathf.PingPong()` in `FixedUpdate()` to oscillate between `start` and `end` Vector3 positions. Exposes `GetVelocity()` so `PlayerMovement` can query the platform's current frame delta for correct player riding behavior.

### `RespawnTrigger.cs`
A trigger volume placed below the level. On `OnTriggerEnter` with a `CharacterController`, calls `GameManager.Die()` to trigger the Game Over screen instead of auto-respawning.

### `GameManager.cs` (Quest 3)
Singleton managing health, coins, Game Over and Victory UI. Uses `CanvasGroup` fade for HUD, GameOver, and Victory canvases. Provides `TakeDamage()`, `Die()`, `Respawn()`, `QuitGame()`, `AddCoin()`, and `ShowVictory()`. The Respawn button resets health to 100% and coins to 0.

### `PlayerHealth.cs` (Quest 3)
Attached to the player. Provides `TakeDamage(float)` with a cooldown to prevent rapid repeat damage. Forwards damage to `GameManager`.

### `JewelPickup.cs` (Quest 3)
Attached to the jewel prefab. Rotates the jewel and triggers `GameManager.ShowVictory()` on player contact.

### `SawTrap.cs` (Quest 3)
Attached to saw prefab instances. Rotates the saw and deals damage via `PlayerHealth.TakeDamage()` on trigger contact.

### `EnemyDamage.cs` (Quest 3)
Attached to enemy creatures alongside `EnemyPatrol`. Deals damage to the player on contact, but skips damage if the player is stomping from above (letting `EnemyPatrol` handle the stomp kill).

### `SignDisplay.cs` (Quest 3)
Attached to sign prefabs. Shows/hides a world-space or screen-space UI panel with localizable text when the player enters/exits the trigger zone.

## Key Architectural Notes

- **Platform layer:** A custom Unity layer named `"Platforms"` is required for the player's ground raycast to detect moving platforms. Ensure platform GameObjects use this layer.
- **Input System:** Uses the **new** Unity Input System (not legacy). Action map is `"Player"` in `Assets/InputSystem_Actions.inputactions`. Actions include `Move`, `Jump`, `Look`, `Sprint`, `Interact`, `Attack`, `Crouch`.
- **Render Pipeline:** URP with two renderer assets — `PC_Renderer.asset` and `Mobile_Renderer.asset`. Custom Painterly shader graph shaders live in `Assets/Shaders/`.
- **Prefab library:** 180+ prefabs in `Assets/Prefabs/World/` cover grass/snow block variants, moving platforms, coins, crates, doors, hazards, and decorative elements — use these for level building rather than raw meshes.
- **Scene to work in:** `Assets/Scenes/JumpAndRun.unity` is the main gameplay scene. `SampleScene.unity` is a scratch/default scene.
