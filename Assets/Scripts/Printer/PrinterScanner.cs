using UnityEngine;

namespace SyntaxSultan.PrinterSystem
{
    public class PrinterScanner
    {
        public bool Scan(PrintedPaper paper)
        {
            if (paper?.PrintedTexture == null)
            {
                Debug.LogWarning("[PrinterScanner] Taranacak geçerli texture bulunamadı.");
                return false;
            }

            if (CameraStorage.Instance == null)
            {
                Debug.LogError("[PrinterScanner] CameraStorage instance bulunamadı.");
                return false;
            }

            Texture2D copy = CopyTextureGPU(paper.PrintedTexture);
            CameraStorage.Instance.Upload(new[] { copy });
            return true;
        }

        private Texture2D CopyTextureGPU(Texture2D source)
        {
            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0);
            Graphics.Blit(source, rt);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            // Bug Fix: Using RGBA32 avoids errors when the source texture is compressed.
            Texture2D copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            copy.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return copy;
        }
    }
}