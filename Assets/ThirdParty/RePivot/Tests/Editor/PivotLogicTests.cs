using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace io.splashart.RePivot.Tests
{
    public sealed class PivotLogicTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("TestRoot");
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
                Object.DestroyImmediate(_root);

            // Clean up any leftover wrappers.
            var wrapper = GameObject.Find("TestRoot_Pivot");
            if (wrapper != null)
                Object.DestroyImmediate(wrapper);
        }


        [Test]
        public void CalculateBounds_NullTarget_ReturnsDefault()
        {
            Bounds b = PivotLogic.CalculateBounds(null);
            Assert.AreEqual(Vector3.zero, b.center);
            Assert.AreEqual(Vector3.zero, b.size);
        }

        [Test]
        public void CalculateBounds_NoRenderers_ReturnsBoundsAtPosition()
        {
            _root.transform.position = new Vector3(1f, 2f, 3f);
            Bounds b = PivotLogic.CalculateBounds(_root);

            Assert.AreEqual(_root.transform.position, b.center);
            Assert.AreEqual(Vector3.zero, b.size);
        }

        [Test]
        public void CalculateBounds_WithMeshRenderer_ReturnsEnclosingBounds()
        {
            var child = GameObject.CreatePrimitive(PrimitiveType.Cube);
            child.transform.SetParent(_root.transform);
            child.transform.localPosition = Vector3.zero;

            Bounds b = PivotLogic.CalculateBounds(_root);

            Assert.Greater(b.size.sqrMagnitude, 0f, "Bounds should be non-zero.");
        }


        [Test]
        public void CalculateAnchorPosition_VertexAnchors_ReturnAllEightCorners()
        {
            var bounds = new Bounds(new Vector3(1f, 2f, 3f), new Vector3(8f, 10f, 12f));

            AssertAnchorPosition(bounds, new Anchor3D(AxisSnap.Min, AxisSnap.Min, AxisSnap.Min), bounds.min.x, bounds.min.y, bounds.min.z);
            AssertAnchorPosition(bounds, new Anchor3D(AxisSnap.Min, AxisSnap.Min, AxisSnap.Max), bounds.min.x, bounds.min.y, bounds.max.z);
            AssertAnchorPosition(bounds, new Anchor3D(AxisSnap.Min, AxisSnap.Max, AxisSnap.Min), bounds.min.x, bounds.max.y, bounds.min.z);
            AssertAnchorPosition(bounds, new Anchor3D(AxisSnap.Min, AxisSnap.Max, AxisSnap.Max), bounds.min.x, bounds.max.y, bounds.max.z);
            AssertAnchorPosition(bounds, new Anchor3D(AxisSnap.Max, AxisSnap.Min, AxisSnap.Min), bounds.max.x, bounds.min.y, bounds.min.z);
            AssertAnchorPosition(bounds, new Anchor3D(AxisSnap.Max, AxisSnap.Min, AxisSnap.Max), bounds.max.x, bounds.min.y, bounds.max.z);
            AssertAnchorPosition(bounds, new Anchor3D(AxisSnap.Max, AxisSnap.Max, AxisSnap.Min), bounds.max.x, bounds.max.y, bounds.min.z);
            AssertAnchorPosition(bounds, new Anchor3D(AxisSnap.Max, AxisSnap.Max, AxisSnap.Max), bounds.max.x, bounds.max.y, bounds.max.z);
        }

        [Test]
        public void CalculateAnchorPosition_EdgeMidpoint_ReturnsCenteredAxis()
        {
            var bounds = new Bounds(new Vector3(1f, 2f, 3f), new Vector3(8f, 10f, 12f));
            var anchor = new Anchor3D(AxisSnap.Center, AxisSnap.Max, AxisSnap.Min);

            AssertAnchorPosition(bounds, anchor, bounds.center.x, bounds.max.y, bounds.min.z);
        }

        [Test]
        public void CalculateAnchorPosition_FaceCenter_ReturnsTwoCenteredAxes()
        {
            var bounds = new Bounds(new Vector3(1f, 2f, 3f), new Vector3(8f, 10f, 12f));
            var anchor = new Anchor3D(AxisSnap.Center, AxisSnap.Center, AxisSnap.Max);

            AssertAnchorPosition(bounds, anchor, bounds.center.x, bounds.center.y, bounds.max.z);
        }

        [Test]
        public void CalculateAnchorPosition_BoundsCenter_ReturnsBoundsCenter()
        {
            var bounds = new Bounds(new Vector3(1f, 2f, 3f), new Vector3(8f, 10f, 12f));
            var anchor = new Anchor3D(AxisSnap.Center, AxisSnap.Center, AxisSnap.Center);

            AssertAnchorPosition(bounds, anchor, bounds.center.x, bounds.center.y, bounds.center.z);
        }

        [Test]
        public void Anchor3D_Kind_ClassifiesAnchorTypes()
        {
            Assert.AreEqual(
                AnchorKind.Vertex,
                new Anchor3D(AxisSnap.Min, AxisSnap.Max, AxisSnap.Min).Kind);

            Assert.AreEqual(
                AnchorKind.Edge,
                new Anchor3D(AxisSnap.Center, AxisSnap.Max, AxisSnap.Min).Kind);

            Assert.AreEqual(
                AnchorKind.Face,
                new Anchor3D(AxisSnap.Center, AxisSnap.Center, AxisSnap.Min).Kind);

            Assert.AreEqual(
                AnchorKind.Center,
                new Anchor3D(AxisSnap.Center, AxisSnap.Center, AxisSnap.Center).Kind);
        }

        private static void AssertAnchorPosition(
            Bounds bounds,
            Anchor3D anchor,
            float expectedX,
            float expectedY,
            float expectedZ)
        {
            Vector3 pos = PivotLogic.CalculateAnchorPosition(bounds, anchor);

            Assert.AreEqual(expectedX, pos.x);
            Assert.AreEqual(expectedY, pos.y);
            Assert.AreEqual(expectedZ, pos.z);
        }


        [Test]
        public void IsFinite_FiniteVector_ReturnsTrue()
        {
            Assert.IsTrue(PivotLogic.IsFinite(new Vector3(1f, 2f, 3f)));
        }

        [Test]
        public void IsFinite_NaN_ReturnsFalse()
        {
            Assert.IsFalse(PivotLogic.IsFinite(new Vector3(float.NaN, 0f, 0f)));
        }

        [Test]
        public void IsFinite_Infinity_ReturnsFalse()
        {
            Assert.IsFalse(PivotLogic.IsFinite(new Vector3(0f, float.PositiveInfinity, 0f)));
        }


        [Test]
        public void IsSceneObject_NullTarget_ReturnsFalse()
        {
            Assert.IsFalse(PivotLogic.IsSceneObject(null));
        }

        [Test]
        public void IsSceneObject_SceneGameObject_ReturnsTrue()
        {
            Assert.IsTrue(PivotLogic.IsSceneObject(_root));
        }


        [Test]
        public void IsUIElement_NullTarget_ReturnsFalse()
        {
            Assert.IsFalse(PivotLogic.IsUIElement(null));
        }

        [Test]
        public void IsUIElement_WithRectTransform_ReturnsTrue()
        {
            var uiObj = new GameObject("UIObj", typeof(RectTransform));
            uiObj.transform.SetParent(_root.transform);

            Assert.IsTrue(PivotLogic.IsUIElement(uiObj));
        }

        [Test]
        public void IsUIElement_StandardTransform_ReturnsFalse()
        {
            Assert.IsFalse(PivotLogic.IsUIElement(_root));
        }


        [Test]
        public void IsSafeToModify_NullTarget_ReturnsFalse()
        {
            Assert.IsFalse(PivotLogic.IsSafeToModify(null));
        }

        [Test]
        public void IsSafeToModify_SceneObject_ReturnsTrue()
        {
            Assert.IsTrue(PivotLogic.IsSafeToModify(_root));
        }


        [Test]
        public void HasPivotWrapper_NullTarget_ReturnsFalse()
        {
            Assert.IsFalse(PivotLogic.HasPivotWrapper(null));
        }

        [Test]
        public void HasPivotWrapper_NoWrapper_ReturnsFalse()
        {
            Assert.IsFalse(PivotLogic.HasPivotWrapper(_root));
        }

        [Test]
        public void HasPivotWrapper_WithWrapper_ReturnsTrue()
        {
            var wrapper = new GameObject("TestRoot_Pivot");
            _root.transform.SetParent(wrapper.transform);

            Assert.IsTrue(PivotLogic.HasPivotWrapper(_root));

            Object.DestroyImmediate(wrapper);
        }


        [Test]
        public void HasVisibleRenderers_NoRenderers_ReturnsFalse()
        {
            Assert.IsFalse(PivotLogic.HasVisibleRenderers(_root));
        }

        [Test]
        public void HasVisibleRenderers_WithRenderer_ReturnsTrue()
        {
            var child = GameObject.CreatePrimitive(PrimitiveType.Cube);
            child.transform.SetParent(_root.transform);

            Assert.IsTrue(PivotLogic.HasVisibleRenderers(_root));
        }


        [Test]
        public void AdjustPivot_NullTarget_ReturnsNull()
        {
            Assert.IsNull(PivotLogic.AdjustPivot(null, Vector3.zero));
        }

        [Test]
        public void AdjustPivot_ValidTarget_CreatesWrapper()
        {
            _root.transform.position = new Vector3(1f, 0f, 0f);
            Vector3 pivotPos = new Vector3(0f, -1f, 0f);

            GameObject wrapper = PivotLogic.AdjustPivot(_root, pivotPos);

            Assert.IsNotNull(wrapper, "Wrapper should be created.");
            Assert.AreEqual("TestRoot_Pivot", wrapper.name);
            Assert.AreEqual(pivotPos, wrapper.transform.position);
            Assert.AreEqual(wrapper.transform, _root.transform.parent);

            // Original world position should be preserved.
            Assert.That(_root.transform.position.x, Is.EqualTo(1f).Within(0.001f));

            Object.DestroyImmediate(wrapper);
        }

        [Test]
        public void AdjustPivot_NaNPivot_ReturnsNull()
        {
            var pivot = new Vector3(float.NaN, 0f, 0f);
            Assert.IsNull(PivotLogic.AdjustPivot(_root, pivot));
        }

        [Test]
        public void AdjustPivot_ExistingWrapper_RepositionsWrapper()
        {
            _root.transform.position = Vector3.zero;
            GameObject wrapper = PivotLogic.AdjustPivot(_root, new Vector3(1f, 0f, 0f));
            Assert.IsNotNull(wrapper);

            // Adjust again — should reposition, not nest.
            GameObject wrapper2 = PivotLogic.AdjustPivot(_root, new Vector3(2f, 0f, 0f));
            Assert.AreEqual(wrapper, wrapper2, "Should reuse existing wrapper.");
            Assert.That(wrapper2.transform.position.x, Is.EqualTo(2f).Within(0.001f));

            Object.DestroyImmediate(wrapper);
        }
    }
}
