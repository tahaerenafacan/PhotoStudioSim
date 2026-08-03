using System.IO;

namespace io.splashart.RePivot
{
    internal static class RPDocHelper
    {
        private const string k_RePivotRoot = "Assets/RePivot";

        internal static string GetDocumentationPath()
        {
            var path = Path.GetFullPath(
                Path.Combine(k_RePivotRoot, "Documentation", "RePivot-Documentation.pdf"));
            return File.Exists(path) ? path : null;
        }
    }
}
