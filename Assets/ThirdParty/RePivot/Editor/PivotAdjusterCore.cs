using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace io.splashart.RePivot
{
    public enum AxisSnap
    {
        Min,
        Center,
        Max,
    }

    public enum AnchorKind
    {
        Vertex,
        Edge,
        Face,
        Center,
    }

    public readonly struct Anchor3D : System.IEquatable<Anchor3D>
    {
        public Anchor3D(AxisSnap xAxis, AxisSnap yAxis, AxisSnap zAxis)
        {
            XAxis = xAxis;
            YAxis = yAxis;
            ZAxis = zAxis;
        }

        public AxisSnap XAxis { get; }
        public AxisSnap YAxis { get; }
        public AxisSnap ZAxis { get; }

        public AnchorKind Kind
        {
            get
            {
                int centeredAxes = 0;
                if (XAxis == AxisSnap.Center)
                    centeredAxes++;
                if (YAxis == AxisSnap.Center)
                    centeredAxes++;
                if (ZAxis == AxisSnap.Center)
                    centeredAxes++;

                return centeredAxes switch
                {
                    0 => AnchorKind.Vertex,
                    1 => AnchorKind.Edge,
                    2 => AnchorKind.Face,
                    _ => AnchorKind.Center,
                };
            }
        }

        /// <inheritdoc />
        public bool Equals(Anchor3D other)
        {
            return XAxis == other.XAxis &&
                   YAxis == other.YAxis &&
                   ZAxis == other.ZAxis;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Anchor3D other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + XAxis.GetHashCode();
                hash = hash * 31 + YAxis.GetHashCode();
                hash = hash * 31 + ZAxis.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Anchor3D left, Anchor3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Anchor3D left, Anchor3D right)
        {
            return !left.Equals(right);
        }
    }

    public static class PivotLogic
    {
        private static readonly List<Renderer> RendererBuffer = new(32);
        private const string PivotSuffix = "_Pivot";

        /// <summary>
        /// Creates a wrapper parent at <paramref name="newPivotWorldPos"/> and re-parents
        /// <paramref name="target"/> under it, compensating the child transform.
        /// </summary>
        /// <returns>The wrapper, or <c>null</c> if aborted.</returns>
        public static GameObject AdjustPivot(GameObject target, Vector3 newPivotWorldPos)
        {
            if (target == null)
            {
                Debug.LogWarning("[PivotAdjuster] Target is null. Operation aborted.");
                return null;
            }

            if (!ValidateTarget(target, newPivotWorldPos))
                return null;

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Adjust Pivot");

            Transform targetTransform = target.transform;
            Vector3 originalWorldPosition = targetTransform.position;
            Quaternion originalWorldRotation = targetTransform.rotation;
            Vector3 originalLossyScale = targetTransform.lossyScale;
            Transform originalParent = targetTransform.parent;
            int siblingIndex = targetTransform.GetSiblingIndex();

            // If target already has a pivot wrapper, re-use it instead of nesting.
            if (HasPivotWrapper(target))
            {
                return RepositionExistingWrapper(target, newPivotWorldPos, undoGroup);
            }

            var wrapper = new GameObject(target.name + PivotSuffix);
            Undo.RegisterCreatedObjectUndo(wrapper, "Create Pivot Wrapper");

            wrapper.layer = target.layer;
            wrapper.tag = target.tag;

            // Copy static flags so batching/navigation/lightmap settings carry over.
            GameObjectUtility.SetStaticEditorFlags(
                wrapper,
                GameObjectUtility.GetStaticEditorFlags(target));

            Transform wrapperTransform = wrapper.transform;

            if (originalParent != null)
            {
                Undo.SetTransformParent(
                    wrapperTransform,
                    originalParent,
                    "Set Pivot Wrapper Parent");

                // Preserve original sibling order.
                Undo.RecordObject(wrapperTransform, "Restore Sibling Index");
                wrapperTransform.SetSiblingIndex(siblingIndex);
            }

            Undo.RecordObject(wrapperTransform, "Position Pivot Wrapper");
            wrapperTransform.position = newPivotWorldPos;
            wrapperTransform.rotation = originalWorldRotation;
            wrapperTransform.localScale = Vector3.one;

            Undo.SetTransformParent(
                targetTransform,
                wrapperTransform,
                "Re-parent Target Under Pivot Wrapper");

            Undo.RecordObject(targetTransform, "Compensate Child Transform");
            targetTransform.position = originalWorldPosition;
            targetTransform.rotation = originalWorldRotation;
            // NOTE: lossyScale is read-only and cannot be decomposed exactly when
            // ancestors have non-uniform scale. The restored localScale is approximate
            // in that case. See Known Limitations in ReadMe.md.
            targetTransform.localScale = originalLossyScale;

            Undo.CollapseUndoOperations(undoGroup);

            return wrapper;
        }

        /// <summary>Returns combined world-space bounds of enabled renderers (excluding particles).</summary>
        public static Bounds CalculateBounds(GameObject target)
        {
            if (target == null)
                return default;

            RendererBuffer.Clear();
            target.GetComponentsInChildren(RendererBuffer);

            // Filter: only enabled renderers, skip ParticleSystemRenderer (unreliable bounds).
            Bounds combined = default;
            bool first = true;

            for (int i = 0; i < RendererBuffer.Count; i++)
            {
                Renderer r = RendererBuffer[i];
                if (!r.enabled)
                    continue;
                if (r is ParticleSystemRenderer)
                    continue;

                if (first)
                {
                    combined = r.bounds;
                    first = false;
                }
                else
                {
                    combined.Encapsulate(r.bounds);
                }
            }

            if (first)
                return new Bounds(target.transform.position, Vector3.zero);

            return combined;
        }

        /// <summary>Returns <c>false</c> if <paramref name="target"/> is part of an unpacked Prefab instance.</summary>
        public static bool IsSafeToModify(GameObject target)
        {
            if (target == null)
                return false;

            if (!PrefabUtility.IsPartOfAnyPrefab(target))
                return true;

            var prefabStage =
                UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage != null && prefabStage.IsPartOfPrefabContents(target))
                return true;

            if (PrefabUtility.IsPartOfPrefabInstance(target))
                return false;

            return true;
        }

        /// <summary>Returns <c>true</c> if the target is a scene object (not a Project asset).</summary>
        public static bool IsSceneObject(GameObject target)
        {
            if (target == null)
                return false;

            return string.IsNullOrEmpty(AssetDatabase.GetAssetPath(target));
        }

        /// <summary>Returns <c>true</c> if the target already sits under a <c>_Pivot</c> wrapper.</summary>
        public static bool HasPivotWrapper(GameObject target)
        {
            if (target == null)
                return false;

            Transform parent = target.transform.parent;
            return parent != null && parent.name.EndsWith(PivotSuffix);
        }

        /// <summary>Returns <c>true</c> if the target has any enabled renderers (excluding particles).</summary>
        public static bool HasVisibleRenderers(GameObject target)
        {
            if (target == null)
                return false;

            RendererBuffer.Clear();
            target.GetComponentsInChildren(RendererBuffer);

            for (int i = 0; i < RendererBuffer.Count; i++)
            {
                Renderer r = RendererBuffer[i];
                if (r.enabled && r is not ParticleSystemRenderer)
                    return true;
            }

            return false;
        }

        /// <summary>Returns <c>true</c> if the target uses RectTransform (UI element).</summary>
        public static bool IsUIElement(GameObject target)
        {
            return target != null && target.TryGetComponent<RectTransform>(out _);
        }

        /// <summary>Resolves <paramref name="anchor"/> to a world-space point on <paramref name="bounds"/>.</summary>
        public static Vector3 CalculateAnchorPosition(Bounds bounds, Anchor3D anchor)
        {
            return new Vector3(
                ResolveAxis(bounds.min.x, bounds.center.x, bounds.max.x, anchor.XAxis),
                ResolveAxis(bounds.min.y, bounds.center.y, bounds.max.y, anchor.YAxis),
                ResolveAxis(bounds.min.z, bounds.center.z, bounds.max.z, anchor.ZAxis));
        }

        private static float ResolveAxis(float min, float center, float max, AxisSnap snap)
        {
            return snap switch
            {
                AxisSnap.Min => min,
                AxisSnap.Max => max,
                _ => center,
            };
        }

        /// <summary>Returns <c>true</c> if all components are finite (not NaN, not Infinity).</summary>
        public static bool IsFinite(Vector3 v)
        {
            return float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
        }

        private static bool ValidateTarget(GameObject target, Vector3 pivotPos)
        {
            if (!IsSceneObject(target))
            {
                Debug.LogWarning("[PivotAdjuster] Target is a Project asset, not a scene object.");
                return false;
            }

            if (!IsSafeToModify(target))
            {
                Debug.LogWarning(
                    $"[PivotAdjuster] \"{target.name}\" is part of a Prefab instance. " +
                    "Unpack the Prefab first or edit the Prefab asset directly.");
                return false;
            }

            if (IsUIElement(target))
            {
                Debug.LogWarning(
                    $"[PivotAdjuster] \"{target.name}\" uses RectTransform. " +
                    "UI elements are not supported.");
                return false;
            }

            if (!IsFinite(pivotPos))
            {
                Debug.LogWarning("[PivotAdjuster] Pivot position contains NaN or Infinity.");
                return false;
            }

            return true;
        }

        private static GameObject RepositionExistingWrapper(
            GameObject target, Vector3 newPivotWorldPos, int undoGroup)
        {
            Transform targetTransform = target.transform;
            Transform wrapperTransform = targetTransform.parent;
            GameObject wrapper = wrapperTransform.gameObject;

            Vector3 originalWorldPosition = targetTransform.position;
            Quaternion originalWorldRotation = targetTransform.rotation;
            Vector3 originalLossyScale = targetTransform.lossyScale;

            Undo.RecordObject(wrapperTransform, "Reposition Pivot Wrapper");
            wrapperTransform.position = newPivotWorldPos;

            Undo.RecordObject(targetTransform, "Compensate Child Transform");
            targetTransform.position = originalWorldPosition;
            targetTransform.rotation = originalWorldRotation;
            targetTransform.localScale = originalLossyScale;

            Undo.CollapseUndoOperations(undoGroup);

            return wrapper;
        }
    }
}
