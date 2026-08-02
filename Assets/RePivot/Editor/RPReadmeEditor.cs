using UnityEditor;
using UnityEngine;

namespace io.splashart.RePivot
{
    [CustomEditor(typeof(RPReadme))]
    [InitializeOnLoad]
    public class RPReadmeEditor : UnityEditor.Editor
    {
        private const string k_ShowedReadmeKey = "RePivot.ShowedReadme";
        private const string k_RePivotRoot = "Assets/RePivot";
        private const float k_Space = 16f;

        private bool m_Initialized;
        private GUIStyle m_TitleStyle;
        private GUIStyle m_HeadingStyle;
        private GUIStyle m_BodyStyle;
        private GUIStyle m_LinkStyle;

        static RPReadmeEditor()
        {
            EditorApplication.delayCall += SelectReadmeAutomatically;
        }

        private static void SelectReadmeAutomatically()
        {
            if (!SessionState.GetBool(k_ShowedReadmeKey, false))
            {
                var readme = FindReadme();
                if (readme != null)
                {
                    Selection.objects = new Object[] { readme };
                    SessionState.SetBool(k_ShowedReadmeKey, true);
                }
            }
        }

        private static RPReadme FindReadme()
        {
            var guids = AssetDatabase.FindAssets("t:RPReadme", new[] { k_RePivotRoot });
            foreach (var guid in guids)
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(
                    AssetDatabase.GUIDToAssetPath(guid));
                if (asset is RPReadme readme)
                    return readme;
            }
            return null;
        }

        protected override void OnHeaderGUI()
        {
            var readme = (RPReadme)target;
            Init();

            var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

            GUILayout.BeginHorizontal("In BigTitle");
            {
                if (readme.icon != null)
                {
                    GUILayout.Space(k_Space);
                    GUILayout.Label(
                        readme.icon,
                        GUILayout.Width(iconWidth),
                        GUILayout.Height(iconWidth));
                }
                GUILayout.Space(k_Space);
                GUILayout.BeginVertical();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(readme.title, m_TitleStyle);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            var readme = (RPReadme)target;
            Init();

            foreach (var section in readme.sections)
            {
                if (!string.IsNullOrEmpty(section.heading))
                    GUILayout.Label(section.heading, m_HeadingStyle);

                if (!string.IsNullOrEmpty(section.text))
                    GUILayout.Label(section.text, m_BodyStyle);

                if (!string.IsNullOrEmpty(section.linkText))
                {
                    if (LinkLabel(new GUIContent(section.linkText)))
                    {
                        if (!string.IsNullOrEmpty(section.url))
                        {
                            if (section.url.StartsWith("{doc}"))
                            {
                                var docPath = RPDocHelper.GetDocumentationPath();
                                if (!string.IsNullOrEmpty(docPath))
                                    Application.OpenURL(
                                        "file:///" + docPath.Replace('\\', '/'));
                            }
                            else
                            {
                                Application.OpenURL(section.url);
                            }
                        }
                    }
                }

                GUILayout.Space(k_Space);
            }
        }

        private void Init()
        {
            if (m_Initialized)
                return;

            m_BodyStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                fontSize = 14,
                richText = true
            };

            m_TitleStyle = new GUIStyle(m_BodyStyle)
            {
                fontSize = 26
            };

            m_HeadingStyle = new GUIStyle(m_BodyStyle)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 18
            };

            m_LinkStyle = new GUIStyle(m_BodyStyle)
            {
                wordWrap = false,
                stretchWidth = false
            };
            m_LinkStyle.normal.textColor =
                new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);

            m_Initialized = true;
        }

        private bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
        {
            var position = GUILayoutUtility.GetRect(label, m_LinkStyle, options);

            Handles.BeginGUI();
            Handles.color = m_LinkStyle.normal.textColor;
            Handles.DrawLine(
                new Vector3(position.xMin, position.yMax),
                new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();

            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

            return GUI.Button(position, label, m_LinkStyle);
        }
    }
}
