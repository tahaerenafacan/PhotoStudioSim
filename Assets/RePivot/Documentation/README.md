# RePivot

**Version:** 1.1.0
**Publisher:** SplashArt Toolbox
**Namespace:** `io.splashart.RePivot`

---

## Requirements

| Requirement        | Minimum Version           |
|--------------------|---------------------------|
| **Unity**          | 6 (6000.0) or newer       |
| **Render Pipeline**| Any (Built-in, URP, HDRP) |
| **Platform**       | Editor only — no runtime footprint |

**Dependencies:** `com.unity.modules.uielements`, `com.unity.modules.imgui` (both included in Unity by default).

> **Note:** This tool uses UI Toolkit and Unity 6 APIs exclusively.
> It will **not** compile on Unity 2022 or earlier.

---

## Installation

### From the Unity Asset Store

1. Open **Window → Asset Store** (or the Unity Hub Asset Store tab).
2. Search for **"RePivot"** by SplashArt Toolbox.
3. Click **Import** and accept all files.
4. Verify the following folder exists:
   ```
   Assets/RePivot/
   ```

### From a `.unitypackage` File

1. Double-click the downloaded `.unitypackage` file.
2. In the Import dialog, ensure **all files** are selected.
3. Click **Import**.

---

## How to Use

### Opening the Tool

Navigate to:

```
Window → SplashArt → RePivot
```

The RePivot window will open as a dockable panel.

### Basic Workflow

1. **Select a GameObject** in the Scene or Hierarchy.
   - The window displays the object name and current status.
   - If the object is a Prefab instance, a warning and an **Unpack Prefab**
     button appear. Unpack before adjusting.

2. **Choose a pivot position** using one of three methods:

   | Method            | Description                                      |
   |-------------------|--------------------------------------------------|
  | **Quick Presets** | Choose **Vertices**, **Edges**, **Faces**, or **Center**, then click a named bounds anchor such as Front Top Left, Top Front Edge, or Bounds Center. |
   | **Manual Entry**  | Type exact world-space coordinates into the Vector3 field. |
   | **Scene Handle**  | Drag the orange position handle directly in the Scene View. |

3. **Click "Apply Pivot"**.
   - A wrapper GameObject (`ObjectName_Pivot`) is created at the chosen position.
   - The original object is re-parented under the wrapper.
   - The object does **not** visually move — only the pivot (parent origin) changes.

4. **Undo** at any time with **Ctrl+Z** / **Cmd+Z**. The entire operation
   is collapsed into a single undo step.

### Re-adjusting a Pivot

If the target already has a `_Pivot` wrapper, clicking **Apply Pivot** repositions
the existing wrapper instead of nesting a new one.

### Scene View Visualisation

While the window is open and an object is selected:

- A **cyan wireframe cube** shows the combined renderer bounds.
- An **orange disc and position handle** marks the proposed pivot location.
- A **"Pivot" label** floats above the handle for clarity.

---

## Known Limitations

- **UI elements (RectTransform) are not supported.** The tool detects RectTransform
  components and disables controls with a status message.
- **Non-uniform ancestor scale.** When a target's parent hierarchy contains
  non-uniform scale, the child's `lossyScale` restoration after re-parenting is
  approximate. This is a Unity engine limitation — `lossyScale` is read-only and
  cannot always be decomposed exactly. For best results, ensure parent objects use
  uniform scale before adjusting the pivot.
- **Multi-selection.** When multiple GameObjects are selected, only the active
  (primary) object is shown and operated on.
- **Prefab instances must be unpacked** before adjusting. The tool provides a
  one-click Unpack button, or you can open Prefab Mode to edit internally.

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "No object selected" message | Select a GameObject in the Scene Hierarchy. Project assets are not supported — the object must exist in a scene. |
| "UI elements not supported" | The selected object has a RectTransform. RePivot works with standard Transform objects only. |
| "Prefab instance — unpack before adjusting" | Click the **Unpack Prefab** button in the tool window, or right-click the object in the Hierarchy and select **Unpack Prefab**. |
| Controls disabled during Play mode | Exit Play mode. Scene modifications are disabled while playing. |
| Bounds wireframe not visible | The object has no enabled Renderer components. Preset buttons will use the object's transform position instead. |
| Pivot position shows NaN | Ensure the manual coordinates contain valid numbers. Check that the object's transform does not contain NaN values. |

---

## API Reference

All core logic is in the `PivotLogic` static class (`io.splashart.RePivot` namespace).

| Method | Signature | Description |
|--------|-----------|-------------|
| `AdjustPivot` | `GameObject AdjustPivot(GameObject target, Vector3 newPivotWorldPos)` | Creates (or repositions) a `_Pivot` wrapper parent at the given world position, re-parenting the target underneath. Returns the wrapper, or `null` if validation fails. The entire operation is a single undo step. |
| `CalculateBounds` | `Bounds CalculateBounds(GameObject target)` | Returns the combined world-space AABB of all enabled renderers (excluding `ParticleSystemRenderer`). If no renderers exist, returns a zero-size bounds at the object's position. |
| `CalculateAnchorPosition` | `Vector3 CalculateAnchorPosition(Bounds bounds, Anchor3D anchor)` | Maps a three-axis `Anchor3D` value to a world-space point on the given bounds. Supports vertices, edge midpoints, face centers, and bounds center. |
| `IsSafeToModify` | `bool IsSafeToModify(GameObject target)` | Returns `false` if the target is part of an unpacked Prefab instance (modification would break the Prefab link). Returns `true` for plain scene objects and objects inside Prefab Mode. |
| `IsSceneObject` | `bool IsSceneObject(GameObject target)` | Returns `true` if the target exists in a scene (not a Project asset on disk). |
| `HasPivotWrapper` | `bool HasPivotWrapper(GameObject target)` | Returns `true` if the target's immediate parent has a name ending with `_Pivot`. |
| `HasVisibleRenderers` | `bool HasVisibleRenderers(GameObject target)` | Returns `true` if the target or any child has at least one enabled, non-particle renderer. |
| `IsUIElement` | `bool IsUIElement(GameObject target)` | Returns `true` if the target has a `RectTransform` component. |
| `IsFinite` | `bool IsFinite(Vector3 v)` | Returns `true` if all three components are finite (not NaN, not Infinity). |

---

## Folder Structure

```
Assets/RePivot/
├── CHANGELOG.md
├── Third Party Notices.txt
├── RePivot Welcome.asset
├── Documentation/
│   ├── README.md
│   ├── LICENSE.md
│   ├── RePivot-Documentation.html
│   └── RePivot-Documentation.pdf
├── Editor/
│   ├── io.splashart.RePivot.Editor.asmdef
│   ├── PivotAdjusterCore.cs
│   ├── PivotAdjusterWindow.cs
│   ├── RPDocHelper.cs
│   ├── RPReadme.cs
│   ├── RPReadmeEditor.cs
│   └── Resources/
│       ├── PivotAdjusterWindow.uxml
│       └── PivotAdjusterWindow.uss
├── Samples/
│   └── RePivotDemo.unity
└── Tests/
    └── Editor/
        ├── io.splashart.RePivot.Tests.Editor.asmdef
        └── PivotLogicTests.cs
```

---

## Changelog

See [CHANGELOG.md](../CHANGELOG.md) for version history.

---

## Frequently Asked Questions

**Q: Can I use this on Prefab instances?**
A: You must unpack the Prefab first. The tool detects Prefab instances and
offers a one-click Unpack button. Alternatively, open the Prefab in
Prefab Mode to edit its internal hierarchy directly.

**Q: Does this add any runtime components or scripts?**
A: No. The assembly definition is Editor-only. Nothing is included in
player builds.

**Q: What happens to existing child objects?**
A: They remain as children of the original object, which is now a child
of the new wrapper. The entire sub-hierarchy is preserved.

**Q: Can I undo the pivot change?**
A: Yes. Press Ctrl+Z (Cmd+Z on macOS) to undo. The wrapper is destroyed
and the original hierarchy is restored.

**Q: Does the tool work with all render pipelines?**
A: Yes. RePivot manipulates transforms and hierarchy only — it does
not interact with materials, shaders, or rendering features. It works
identically on Built-in, URP, and HDRP.
