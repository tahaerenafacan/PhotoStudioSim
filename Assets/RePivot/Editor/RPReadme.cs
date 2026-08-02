using System;
using UnityEngine;

namespace io.splashart.RePivot
{
    public class RPReadme : ScriptableObject
    {
        public Texture2D icon;
        public string title;
        public Section[] sections;
        public bool loadedLayout;

        [Serializable]
        public class Section
        {
            public string heading;
            [TextArea(2, 10)]
            public string text;
            public string linkText;
            public string url;
        }
    }
}
