# Hand Pack Integration

The Street Dice demo can use the purchased RRFreelance first-person hand pack for the local bottom shooter throw.

Local package path:

```text
C:\Users\uncle\AppData\Roaming\Unity\Asset Store-5.x\RRFreelance\3D ModelsCharactersHumanoidsHumans\First Person Hand.unitypackage
```

Imported runtime prefabs:

```text
Assets/RRFreelance/FirstPersonHand/Resources/FirstPersonHands/FirstPersonHand_L.prefab
Assets/RRFreelance/FirstPersonHand/Resources/FirstPersonHands/FirstPersonHand_R.prefab
```

The entire `Assets/RRFreelance` folder is ignored because it contains paid Asset Store content. The committed controller loads those prefabs with `Resources.Load`; if they are missing, it falls back to simple generated hands so the project still compiles and the demo remains runnable.

Current behavior:

- Only the local first-person shooter gets visible hands.
- Hands must enter from the first-person bottom edge; wrists should stay off-screen and the palm side should be the primary visible surface.
- Imported Animator components are driven on throw with `bShoot` when available, or `PoseShoot` when the static controller is loaded.
- Human opponents stay represented by mic/profile overlays.
- AI opponent bodies and Kling throw clips are later visual layers.
- Dice results remain server/local-authoritative; the hand animation never decides the roll.
