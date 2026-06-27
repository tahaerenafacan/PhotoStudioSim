using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

namespace SyntaxSultan.PrinterSystem
{
    /// <summary>
    /// Fiziksel yazıcı cihazı. Baskı, tarama ve güç yönetimini orkestra eder.
    ///
    /// Interact() context-aware çalışır:
    ///   - Elimizde PrintedPaper varsa → tarama başlatılır
    ///   - Aksi hâlde → güç toggle
    ///
    /// PrintDocument() dışarıdan (GalleryApp vb.) çağrılır; validasyon burada yapılır.
    /// </summary>
    public class ItemPaperPrinter : MonoBehaviour, IInteractable, INetworkDevice
    {
        [SerializeField] private NetworkDeviceSO networkData;
        [SerializeField] private LocalizedString interactHint;
        [SerializeField] private LocalizedString interactName;
        [SerializeField] private Material displayMat;

        [Header("Printer Specs")]
        [SerializeField] private PrintQuality maxSupportedQuality = PrintQuality.Average;
        [SerializeField] private bool isColoredSupported          = true;
        [SerializeField] private int  blackPagePerMinute          = 10;
        [SerializeField] private int  colorPagePerMinute          = 5;

        [Header("Ink System")]
        [SerializeField] private PrinterInkSystem inkSystem = new();

        [Header("Paper Tray")]
        [SerializeField] private PrinterPaperTray paperTray = new();

        [Header("Paper Prefabs & Spawn Points")]
        [SerializeField] private Transform    paperSpawnPoint;
        [SerializeField] private Transform    paperSpawnEndPoint;
        [SerializeField] private PrintedPaper prefabA3;
        [SerializeField] private PrintedPaper prefabA4;
        [SerializeField] private PrintedPaper prefabA5;

        public LocalizedString InteractHint => interactHint;
        public LocalizedString InteractName => interactName;
        public bool CanInteract => true;

        public PrinterInkSystem InkSystem  => inkSystem;
        public PrinterPaperTray PaperTray  => paperTray;
        public PrintQuality MaxSupportedQuality => maxSupportedQuality;
        public bool IsColoredSupported => isColoredSupported;
        public bool IsPowered          => isPowered;
        public bool IsPrinting         => isPrinting;

        public event Action<PrinterError> OnPrintError;
        public event Action<PrintedPaper> OnPrintCompleted;
        public event Action<bool>         OnScanCompleted;  // true → başarılı
        public event Action<bool>         OnPowerChanged;

        public NetworkDeviceSO GetNetworkDeviceData() => networkData;

        private bool isPowered;
        private bool isPrinting;
        private readonly PrinterScanner scanner = new();
        
        public void Interact()
        {
            if (PlayerItemHolder.Instance?.CurrentItem is PrintedPaper paper)
            {
                bool success = scanner.Scan(paper);
                OnScanCompleted?.Invoke(success);
                return;
            }

            TogglePower();
        }

        // ── Print API ──────────────────────────────────────────────

        public void PrintDocument(PrintSettings settings, Texture2D imageToPrint)
        {
            Debug.Log(settings);
            PrinterError error = ValidatePrint(settings);
            if (error != PrinterError.None)
            {
                OnPrintError?.Invoke(error);
                Debug.LogWarning($"[ItemPaperPrinter] Baskı hatası: {error}");
                return;
            }

            PrintedPaper prefab = ResolvePaperPrefab(settings.paperSize);
            if (prefab == null || paperSpawnPoint == null) return;

            paperTray.TryConsume();
            inkSystem.ConsumeInk(settings.isColored);

            PrintedPaper spawnedPaper = Instantiate(prefab, paperSpawnPoint.position, paperSpawnPoint.rotation);
            spawnedPaper.Setup(imageToPrint, settings);
            spawnedPaper.TogglePhysics(false);

            float ppm      = settings.isColored ? colorPagePerMinute : blackPagePerMinute;
            float duration = 60f / ppm;
            StartCoroutine(AnimatePaperEject(spawnedPaper, duration));
        }

        // ── Refill API (dışarıdan çağrılır) ───────────────────────

        public void RefillCyan(float amount)    => inkSystem.RefillCyan(amount);
        public void RefillMagenta(float amount) => inkSystem.RefillMagenta(amount);
        public void RefillYellow(float amount)  => inkSystem.RefillYellow(amount);
        public void RefillBlack(float amount)   => inkSystem.RefillBlack(amount);
        public void RefillAllInk(float amount)  => inkSystem.RefillAll(amount);
        public int  RefillPaper(int amount)     => paperTray.Refill(amount);

        // ── Private ────────────────────────────────────────────────

        /// <summary>
        /// Baskı öncesi tüm koşulları öncelik sırasıyla kontrol eder.
        /// Sıralama kasıtlı: offline → zaten basılıyor → kağıt → mürekkep.
        /// </summary>
        private PrinterError ValidatePrint(PrintSettings settings)
        {
            if (!isPowered)                              return PrinterError.PrinterOffline;
            if (isPrinting)                              return PrinterError.AlreadyPrinting;
            if (!paperTray.HasPaper)                     return PrinterError.NoPaper;
            if (!inkSystem.CanPrint(settings.isColored)) return PrinterError.InkEmpty;
            return PrinterError.None;
        }

        private PrintedPaper ResolvePaperPrefab(PrintPaperSize size) => size switch
        {
            PrintPaperSize.A3 => prefabA3,
            PrintPaperSize.A4 => prefabA4,
            PrintPaperSize.A5 => prefabA5,
            _                 => prefabA4
        };

        private void TogglePower()
        {
            isPowered = !isPowered;

            if (isPowered)
            {
                Router.Instance?.Connect(this);
                displayMat?.EnableKeyword("_EMISSION");
            }
            else
            {
                Router.Instance?.Disconnect(this);
                displayMat?.DisableKeyword("_EMISSION");
            }

            OnPowerChanged?.Invoke(isPowered);
        }

        /// <summary>
        /// Kağıdı spawn noktasından çıkış noktasına kaydırır.
        /// Yan etki: isPrinting flag'ini yönetir; doğrudan çağrılmamalı.
        /// </summary>
        private IEnumerator AnimatePaperEject(PrintedPaper paper, float duration)
        {
            isPrinting = true;

            Vector3 startPos = paperSpawnPoint.position;
            Vector3 endPos   = paperSpawnEndPoint.position;
            float elapsed    = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                paper.transform.position = Vector3.Lerp(startPos, endPos, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            paper.transform.position = endPos;
            paper.TogglePhysics(true);
            isPrinting = false;

            OnPrintCompleted?.Invoke(paper);
        }
    }
}