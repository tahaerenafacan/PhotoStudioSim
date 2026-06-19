using UnityEngine;

namespace SyntaxSultan.PrinterSystem
{
    /// <summary>
    /// Fiziksel PrintedPaper'ı okur ve CameraStorage'a dijital kopya olarak aktarır.
    /// Non-destructive: orijinal kağıt yok edilmez, sadece texture okunur.
    /// GPU-safe kopyalama için RenderTexture pipeline kullanılır.
    /// </summary>
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

        /// <summary>
        /// Orijinal texture'ı kirletmemek için GPU blit ile kopyalar.
        /// CPU GetPixels/SetPixels yerine tercih edilir: büyük texture'larda önemli ölçüde daha hızlı.
        /// </summary>
        private Texture2D CopyTextureGPU(Texture2D source)
        {
            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0);
            Graphics.Blit(source, rt);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D copy = new Texture2D(source.width, source.height, source.format, false);
            copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            copy.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return copy;
        }
    }
}