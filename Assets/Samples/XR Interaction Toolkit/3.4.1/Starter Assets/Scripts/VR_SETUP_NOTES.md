# VR Setup Notes

Stand: HTC Vive Pro / Vive Wands sollen an das vorhandene Spiel angebunden werden.

## Ziel-Mapping

- Linkes Touchpad: Bewegung
- Rechtes Touchpad: 4 Faehigkeiten per Richtung
- Linker Trigger: Springen
- Rechter Trigger: Zunge schiessen

## Bereits angelegte Scripts

- `VRJump.cs`
  - Nur als Fallback angelegt.
  - In XRI 3.4.1 hat das Starter Rig bereits `Locomotion/Jump`; diesen vorhandenen Jump Provider bevorzugen.
  - `VRJump` nicht zusaetzlich auf den XR Origin setzen, wenn `Locomotion/Jump` aktiv ist.

- `ViveProAbilityInput.cs`
  - Auf den `XR Origin (XR Rig)` setzen.
  - Liest rechtes Touchpad und Trigger-Actions.
  - Ruft Faehigkeiten ueber Message Targets auf.
  - Rechter Trigger kann fuer Zunge verwendet werden.

## Bereits angepasste Gameplay-Scripts

Die folgenden Methoden sind jetzt public und koennen per VR-Input aufgerufen werden:

- `NightVision.TryUseNightVision()`
- `SonarSense.TryUseSonar()`
- `ScentSense.TryUseScentSense()`
- `HearingSense.TryUseHearing()`
- `PlayerTongue.TryShoot()`

Desktop-Tastatursteuerung bleibt als Fallback erhalten.

## XR Origin Locomotion

Im `XR Origin (XR Rig)` die Hierarchy aufklappen:

```text
XR Origin (XR Rig)
└── Locomotion
    ├── Move
    ├── Gravity
    ├── Jump
    ├── Turn
    ├── Teleportation
    ├── Climb
    └── Grab Move
```

Aktiv lassen:

```text
Locomotion/Move
Locomotion/Gravity
Locomotion/Jump
```

Deaktivieren:

```text
Locomotion/Turn
Locomotion/Teleportation
Locomotion/Climb
Locomotion/Grab Move
```

Auf `Locomotion/Move`:

- `Left Hand Move Input` gesetzt lassen.
- `Right Hand Move Input` leer setzen oder auf `Unused`, damit das rechte Touchpad nur fuer Faehigkeiten genutzt wird.

## Input Actions

Im Input Actions Asset eine Action Map `VR Abilities` anlegen:

```text
RightAbilityAxis
Type: Value
Control Type: Vector2
Binding: <XRController>{RightHand}/{Primary2DAxis}

RightAbilityClick
Type: Button
Binding: <XRController>{RightHand}/{Primary2DAxisClick}

Tongue
Type: Button
Binding: <XRController>{RightHand}/{TriggerButton}
```

Den bereits vorhandenen `Jump` Input des Starter Rig auf den linken Trigger legen:

```text
Jump
Binding: <XRController>{LeftHand}/{TriggerButton}
```

## ViveProAbilityInput Inspector

Auf `XR Origin (XR Rig)` die Komponente `ViveProAbilityInput` befuellen:

```text
Right Touchpad Axis  -> VR Abilities / RightAbilityAxis
Right Touchpad Click -> VR Abilities / RightAbilityClick
Right Trigger        -> VR Abilities / Tongue
```

Trigger Event fuer Zunge:

```text
Tongue:
Objekt mit PlayerTongue -> PlayerTongue.TryShoot()
```

Touchpad Direction Messages:

```text
Ability Up:
Target = Objekt mit NightVision
Method Name = TryUseNightVision

Ability Right:
Target = Objekt mit SonarSense
Method Name = TryUseSonar

Ability Down:
Target = Objekt mit ScentSense
Method Name = TryUseScentSense

Ability Left:
Target = Objekt mit HearingSense
Method Name = TryUseHearing
```

## Nach dem Neustart

In Codex/Chat einfach schreiben:

```text
Bitte mach weiter mit dem Vive Pro VR Setup. Lies VR_SETUP_NOTES.md.
```
