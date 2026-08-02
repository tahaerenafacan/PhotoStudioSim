using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.splashart.RePivot
{
    /// <summary>Pivot Adjuster editor panel with Scene View handle support.</summary>
    public sealed class PivotAdjusterWindow : EditorWindow
    {
        private const string UxmlResourcePath = "PivotAdjusterWindow";
        private const string UssResourcePath = "PivotAdjusterWindow";
        private const string UnpackButtonName = "btn-unpack-prefab";
        private const string SceneHandleLabel = "Pivot";
        private const string ModeActiveClass = "rp-preset-mode__btn--active";
        private const string PresetActiveClass = "rp-preset__btn--active";

        private Label _lblSelectedObject;
        private Label _lblSelectionStatus;
        private Vector3Field _vec3ManualPivot;
        private Button _btnApply;

        private Button _btnModeVertices;
        private Button _btnModeEdges;
        private Button _btnModeFaces;
        private Button _btnModeCenter;

        private VisualElement _presetVertices;
        private VisualElement _presetEdges;
        private VisualElement _presetFaces;
        private VisualElement _presetCenter;

        private readonly List<Button> _presetButtons = new(32);
        private readonly List<Button> _modeButtons = new(4);

        [SerializeField] private Vector3 handlePosition;
        [SerializeField] private int targetInstanceId;

        private Anchor3D? _selectedAnchor;

        private GameObject _currentTarget;
        private Bounds _cachedBounds;
        private bool _boundsValid;
        private bool _hasRenderers;

        private static readonly Color BoundsWireColor = new(0.2f, 0.85f, 1f, 0.6f);
        private static readonly Color PivotHandleColor = new(1f, 0.55f, 0.1f, 0.9f);
        private static readonly Color PivotDiscColor = new(1f, 0.55f, 0.1f, 0.35f);
        private static readonly Color PivotCrossColor = new(1f, 0.55f, 0.1f, 0.9f);
        private static readonly Color ConnectLineColor = new(1f, 0.75f, 0.3f, 0.5f);

        [MenuItem("Window/SplashArt/RePivot")]
        public static void ShowWindow()
        {
            var window = GetWindow<PivotAdjusterWindow>();
            window.titleContent = new GUIContent("RePivot");
            window.minSize = new Vector2(300f, 420f);
        }

        /// <summary>Builds the UI from UXML layout and applies stylesheet.</summary>
        public void CreateGUI()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(UxmlResourcePath);
            if (uxmlAsset == null)
            {
                rootVisualElement.Add(
                    new Label("Error: Could not load PivotAdjusterWindow.uxml"));
                return;
            }

            var ussAsset = Resources.Load<StyleSheet>(UssResourcePath);
            if (ussAsset != null)
                rootVisualElement.styleSheets.Add(ussAsset);

            uxmlAsset.CloneTree(rootVisualElement);

            QueryElements();
            BindPresetModeButtons();
            BindPresetButtons();
            BindApplyButton();
            BindManualField();
            SetPresetMode(AnchorKind.Vertex);

            Selection.selectionChanged += OnSelectionChanged;
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            if (targetInstanceId != 0)
            {
                var restored = EditorUtility.InstanceIDToObject(targetInstanceId) as GameObject;
                if (restored != null)
                {
                    _currentTarget = restored;
                    RefreshCachedBounds();
                    UpdateSelectionUI(restored.name, "", true);
                    _vec3ManualPivot?.SetValueWithoutNotify(handlePosition);
                    return;
                }
            }

            OnSelectionChanged();
        }

        private void OnDestroy()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            SceneView.RepaintAll();
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                UpdateSelectionUI("Disabled during Play mode", "", false);
                _currentTarget = null;
                targetInstanceId = 0;
                RemoveUnpackButton();
                SceneView.RepaintAll();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                OnSelectionChanged();
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (EditorApplication.isPlaying)
                return;

            if (_currentTarget == null)
                return;

            if (!PivotLogic.IsSafeToModify(_currentTarget))
                return;

            float size = HandleUtility.GetHandleSize(handlePosition);

            if (_boundsValid)
            {
                Handles.color = BoundsWireColor;
                Handles.DrawWireCube(_cachedBounds.center, _cachedBounds.size);
            }

            Vector3 objectPos = _currentTarget.transform.position;
            if (Vector3.SqrMagnitude(handlePosition - objectPos) > 0.0001f)
            {
                Handles.color = ConnectLineColor;
                Handles.DrawDottedLine(objectPos, handlePosition, 4f);
            }

            Vector3 camForward = sceneView.camera.transform.forward;
            Vector3 camRight = sceneView.camera.transform.right;
            Vector3 camUp = sceneView.camera.transform.up;
            float discRadius = size * 0.18f;
            float crossRadius = size * 0.28f;

            Handles.color = PivotDiscColor;
            Handles.DrawSolidDisc(handlePosition, camForward, discRadius);

            Handles.color = PivotCrossColor;
            Handles.DrawWireDisc(handlePosition, camForward, discRadius);
            Handles.DrawLine(
                handlePosition - camRight * crossRadius,
                handlePosition + camRight * crossRadius);
            Handles.DrawLine(
                handlePosition - camUp * crossRadius,
                handlePosition + camUp * crossRadius);

            Handles.color = PivotHandleColor;

            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(handlePosition, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                handlePosition = newPos;
                _vec3ManualPivot?.SetValueWithoutNotify(handlePosition);
                sceneView.Repaint();
                Repaint();
            }

            Handles.color = Color.white;
            Vector3 labelPos;
            labelPos.x = handlePosition.x;
            labelPos.y = handlePosition.y + size * 0.6f;
            labelPos.z = handlePosition.z;
            Handles.Label(labelPos, "\u2316 New Pivot");

            if (Event.current.type == EventType.MouseMove)
                sceneView.Repaint();
        }

        private void QueryElements()
        {
            _lblSelectedObject = rootVisualElement.Q<Label>("lbl-selected-object");
            _lblSelectionStatus = rootVisualElement.Q<Label>("lbl-selection-status");
            _vec3ManualPivot = rootVisualElement.Q<Vector3Field>("vec3-manual-pivot");
            _btnApply = rootVisualElement.Q<Button>("btn-apply");

            _btnModeVertices = rootVisualElement.Q<Button>("btn-mode-vertices");
            _btnModeEdges = rootVisualElement.Q<Button>("btn-mode-edges");
            _btnModeFaces = rootVisualElement.Q<Button>("btn-mode-faces");
            _btnModeCenter = rootVisualElement.Q<Button>("btn-mode-center");

            _presetVertices = rootVisualElement.Q<VisualElement>("preset-vertices");
            _presetEdges = rootVisualElement.Q<VisualElement>("preset-edges");
            _presetFaces = rootVisualElement.Q<VisualElement>("preset-faces");
            _presetCenter = rootVisualElement.Q<VisualElement>("preset-center");
        }

        private void OnSelectionChanged()
        {
            if (EditorApplication.isPlaying)
                return;

            _currentTarget = Selection.activeGameObject;
            _boundsValid = false;
            _hasRenderers = false;
            targetInstanceId = 0;
            _selectedAnchor = null;
            UpdatePresetSelection(null);

            if (_currentTarget == null)
            {
                UpdateSelectionUI("No object selected", "", false);
                RemoveUnpackButton();
                SceneView.RepaintAll();
                return;
            }

            if (!PivotLogic.IsSceneObject(_currentTarget))
            {
                UpdateSelectionUI(
                    _currentTarget.name,
                    "Project asset \u2014 select a scene object.",
                    false);
                _currentTarget = null;
                RemoveUnpackButton();
                SceneView.RepaintAll();
                return;
            }

            string multiWarning = "";
            if (Selection.gameObjects.Length > 1)
                multiWarning = "Multi-selection: only the active object is shown. ";

            if (PivotLogic.IsUIElement(_currentTarget))
            {
                UpdateSelectionUI(
                    _currentTarget.name,
                    multiWarning + "UI elements (RectTransform) are not supported.",
                    false);
                _currentTarget = null;
                RemoveUnpackButton();
                SceneView.RepaintAll();
                return;
            }

            if (!PivotLogic.IsSafeToModify(_currentTarget))
            {
                UpdateSelectionUI(
                    _currentTarget.name,
                    multiWarning + "\u26a0 Prefab instance \u2014 unpack before adjusting.",
                    false);
                AppendUnpackButton();
                SceneView.RepaintAll();
                return;
            }

            RemoveUnpackButton();
            targetInstanceId = _currentTarget.GetInstanceID();

            RefreshCachedBounds();
            _hasRenderers = PivotLogic.HasVisibleRenderers(_currentTarget);

            string statusMsg = multiWarning;
            if (PivotLogic.HasPivotWrapper(_currentTarget))
                statusMsg += "Already wrapped \u2014 Apply will reposition existing pivot.";
            else if (!_hasRenderers)
                statusMsg += "No renderers found \u2014 presets use object position.";

            UpdateSelectionUI(_currentTarget.name, statusMsg, true);

            handlePosition = _currentTarget.transform.position;
            _vec3ManualPivot?.SetValueWithoutNotify(handlePosition);

            SceneView.RepaintAll();
        }

        private void RefreshCachedBounds()
        {
            if (_currentTarget == null)
            {
                _boundsValid = false;
                return;
            }

            _cachedBounds = PivotLogic.CalculateBounds(_currentTarget);
            _boundsValid = _cachedBounds.size.sqrMagnitude > 0f;
        }

        private void BindPresetButtons()
        {
            BindPreset("btn-vertex-front-top-left", new Anchor3D(AxisSnap.Min, AxisSnap.Max, AxisSnap.Min));
            BindPreset("btn-vertex-front-top-right", new Anchor3D(AxisSnap.Max, AxisSnap.Max, AxisSnap.Min));
            BindPreset("btn-vertex-front-bottom-left", new Anchor3D(AxisSnap.Min, AxisSnap.Min, AxisSnap.Min));
            BindPreset("btn-vertex-front-bottom-right", new Anchor3D(AxisSnap.Max, AxisSnap.Min, AxisSnap.Min));

            BindPreset("btn-vertex-back-top-left", new Anchor3D(AxisSnap.Min, AxisSnap.Max, AxisSnap.Max));
            BindPreset("btn-vertex-back-top-right", new Anchor3D(AxisSnap.Max, AxisSnap.Max, AxisSnap.Max));
            BindPreset("btn-vertex-back-bottom-left", new Anchor3D(AxisSnap.Min, AxisSnap.Min, AxisSnap.Max));
            BindPreset("btn-vertex-back-bottom-right", new Anchor3D(AxisSnap.Max, AxisSnap.Min, AxisSnap.Max));

            BindPreset("btn-edge-top-front", new Anchor3D(AxisSnap.Center, AxisSnap.Max, AxisSnap.Min));
            BindPreset("btn-edge-top-back", new Anchor3D(AxisSnap.Center, AxisSnap.Max, AxisSnap.Max));
            BindPreset("btn-edge-top-left", new Anchor3D(AxisSnap.Min, AxisSnap.Max, AxisSnap.Center));
            BindPreset("btn-edge-top-right", new Anchor3D(AxisSnap.Max, AxisSnap.Max, AxisSnap.Center));

            BindPreset("btn-edge-bottom-front", new Anchor3D(AxisSnap.Center, AxisSnap.Min, AxisSnap.Min));
            BindPreset("btn-edge-bottom-back", new Anchor3D(AxisSnap.Center, AxisSnap.Min, AxisSnap.Max));
            BindPreset("btn-edge-bottom-left", new Anchor3D(AxisSnap.Min, AxisSnap.Min, AxisSnap.Center));
            BindPreset("btn-edge-bottom-right", new Anchor3D(AxisSnap.Max, AxisSnap.Min, AxisSnap.Center));

            BindPreset("btn-edge-vertical-front-left", new Anchor3D(AxisSnap.Min, AxisSnap.Center, AxisSnap.Min));
            BindPreset("btn-edge-vertical-front-right", new Anchor3D(AxisSnap.Max, AxisSnap.Center, AxisSnap.Min));
            BindPreset("btn-edge-vertical-back-left", new Anchor3D(AxisSnap.Min, AxisSnap.Center, AxisSnap.Max));
            BindPreset("btn-edge-vertical-back-right", new Anchor3D(AxisSnap.Max, AxisSnap.Center, AxisSnap.Max));

            BindPreset("btn-face-top", new Anchor3D(AxisSnap.Center, AxisSnap.Max, AxisSnap.Center));
            BindPreset("btn-face-bottom", new Anchor3D(AxisSnap.Center, AxisSnap.Min, AxisSnap.Center));
            BindPreset("btn-face-left", new Anchor3D(AxisSnap.Min, AxisSnap.Center, AxisSnap.Center));
            BindPreset("btn-face-right", new Anchor3D(AxisSnap.Max, AxisSnap.Center, AxisSnap.Center));
            BindPreset("btn-face-front", new Anchor3D(AxisSnap.Center, AxisSnap.Center, AxisSnap.Min));
            BindPreset("btn-face-back", new Anchor3D(AxisSnap.Center, AxisSnap.Center, AxisSnap.Max));

            BindPreset("btn-center-bounds", new Anchor3D(AxisSnap.Center, AxisSnap.Center, AxisSnap.Center));
        }

        private void BindPreset(string buttonName, Anchor3D anchor)
        {
            Button button = rootVisualElement.Q<Button>(buttonName);
            if (button == null)
                return;

            button.userData = anchor;
            _presetButtons.Add(button);
            button.clicked += () => OnPresetClicked(anchor);
        }

        private void OnPresetClicked(Anchor3D anchor)
        {
            if (_currentTarget == null)
                return;

            _selectedAnchor = anchor;
            UpdatePresetSelection(_selectedAnchor);
            RefreshCachedBounds();

            Vector3 pivotPos = PivotLogic.CalculateAnchorPosition(_cachedBounds, anchor);
            handlePosition = pivotPos;
            _vec3ManualPivot?.SetValueWithoutNotify(pivotPos);

            SceneView.RepaintAll();
        }

        private void BindPresetModeButtons()
        {
            BindPresetMode(_btnModeVertices, AnchorKind.Vertex);
            BindPresetMode(_btnModeEdges, AnchorKind.Edge);
            BindPresetMode(_btnModeFaces, AnchorKind.Face);
            BindPresetMode(_btnModeCenter, AnchorKind.Center);
        }

        private void BindPresetMode(Button button, AnchorKind kind)
        {
            if (button == null)
                return;

            _modeButtons.Add(button);
            button.clicked += () => SetPresetMode(kind);
        }

        private void SetPresetMode(AnchorKind kind)
        {
            SetVisible(_presetVertices, kind == AnchorKind.Vertex);
            SetVisible(_presetEdges, kind == AnchorKind.Edge);
            SetVisible(_presetFaces, kind == AnchorKind.Face);
            SetVisible(_presetCenter, kind == AnchorKind.Center);

            _btnModeVertices?.EnableInClassList(ModeActiveClass, kind == AnchorKind.Vertex);
            _btnModeEdges?.EnableInClassList(ModeActiveClass, kind == AnchorKind.Edge);
            _btnModeFaces?.EnableInClassList(ModeActiveClass, kind == AnchorKind.Face);
            _btnModeCenter?.EnableInClassList(ModeActiveClass, kind == AnchorKind.Center);
        }

        private static void SetVisible(VisualElement element, bool visible)
        {
            if (element == null)
                return;

            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdatePresetSelection(Anchor3D? selectedAnchor)
        {
            for (int buttonIndex = 0; buttonIndex < _presetButtons.Count; buttonIndex++)
                _presetButtons[buttonIndex].EnableInClassList(PresetActiveClass, false);

            if (!selectedAnchor.HasValue)
                return;

            for (int buttonIndex = 0; buttonIndex < _presetButtons.Count; buttonIndex++)
            {
                Button button = _presetButtons[buttonIndex];
                if (button.userData is Anchor3D anchor && anchor == selectedAnchor.Value)
                    button.EnableInClassList(PresetActiveClass, true);
            }
        }

        private void BindApplyButton()
        {
            if (_btnApply == null)
                return;

            _btnApply.clicked += OnApplyClicked;
        }

        private void OnApplyClicked()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[PivotAdjuster] Cannot modify objects during Play mode.");
                return;
            }

            if (_currentTarget == null)
                return;

            Vector3 newPivot = _vec3ManualPivot?.value ?? _currentTarget.transform.position;

            if (!PivotLogic.IsFinite(newPivot))
            {
                Debug.LogWarning("[PivotAdjuster] Pivot position contains NaN or Infinity.");
                return;
            }

            GameObject originalTarget = _currentTarget;
            GameObject wrapper = PivotLogic.AdjustPivot(_currentTarget, newPivot);

            if (wrapper != null)
                Selection.activeGameObject = originalTarget;
        }

        private void BindManualField()
        {
            if (_vec3ManualPivot == null)
                return;

            _vec3ManualPivot.RegisterValueChangedCallback(OnManualFieldChanged);
        }

        private void OnManualFieldChanged(ChangeEvent<Vector3> evt)
        {
            handlePosition = evt.newValue;
            SceneView.RepaintAll();
        }

        private void AppendUnpackButton()
        {
            if (rootVisualElement.Q<Button>(UnpackButtonName) != null)
                return;

            VisualElement container = _lblSelectionStatus?.parent;
            if (container == null)
                return;

            var btn = new Button(OnUnpackClicked)
            {
                name = UnpackButtonName,
                text = "Unpack Prefab"
            };

            btn.AddToClassList("rp-btn--warning");
            container.Add(btn);
        }

        private void RemoveUnpackButton()
        {
            rootVisualElement.Q<Button>(UnpackButtonName)?.RemoveFromHierarchy();
        }

        private void OnUnpackClicked()
        {
            if (_currentTarget == null)
                return;

            if (!PrefabUtility.IsPartOfPrefabInstance(_currentTarget))
                return;

            GameObject outerRoot =
                PrefabUtility.GetOutermostPrefabInstanceRoot(_currentTarget);

            if (outerRoot == null)
                return;

            Undo.SetCurrentGroupName("Unpack Prefab for RePivot");
            PrefabUtility.UnpackPrefabInstance(
                outerRoot,
                PrefabUnpackMode.OutermostRoot,
                InteractionMode.UserAction);

            OnSelectionChanged();
        }

        private void UpdateSelectionUI(string objectName, string status, bool controlsEnabled)
        {
            if (_lblSelectedObject != null)
                _lblSelectedObject.text = objectName;

            if (_lblSelectionStatus != null)
            {
                _lblSelectionStatus.text = status;
                _lblSelectionStatus.EnableInClassList(
                    "rp-status--warning",
                    !string.IsNullOrEmpty(status));
            }

            _btnApply?.SetEnabled(controlsEnabled);
            _vec3ManualPivot?.SetEnabled(controlsEnabled);

            for (int buttonIndex = 0; buttonIndex < _modeButtons.Count; buttonIndex++)
                _modeButtons[buttonIndex].SetEnabled(controlsEnabled);

            for (int buttonIndex = 0; buttonIndex < _presetButtons.Count; buttonIndex++)
                _presetButtons[buttonIndex].SetEnabled(controlsEnabled);
        }
    }
}
