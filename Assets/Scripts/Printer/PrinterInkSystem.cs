using System;
using UnityEngine;

namespace SyntaxSultan.PrinterSystem
{
    /// <summary>
    /// CMYK mürekkep düzeylerini yönetir.
    /// Renkli baskı Cyan+Magenta+Yellow tüketir; siyah-beyaz yalnızca Black (Key) tüketir.
    /// Refill metodları dışarıdan (item, UI) çağrılmak üzere tasarlandı.
    /// </summary>
    [Serializable]
    public class PrinterInkSystem
    {
        [SerializeField] private float maxInkLevel     = 100f;
        [SerializeField] private float colorInkPerPage = 2f;   // Her CMY kanalı için sayfa başı tüketim
        [SerializeField] private float blackInkPerPage = 3f;   // K kanalı sayfa başı tüketim

        [Header("Ink Levels (ml)")]
        [SerializeField] private float cyan    = 100f;
        [SerializeField] private float magenta = 100f;
        [SerializeField] private float yellow  = 100f;
        [SerializeField] private float black   = 100f;

        public float MaxInkLevel => maxInkLevel;
        public float Cyan    => cyan;
        public float Magenta => magenta;
        public float Yellow  => yellow;
        public float Black   => black;

        public event Action OnInkChanged;

        /// <summary>Baskı başlamadan önce yeterli mürekkep olup olmadığını doğrular.</summary>
        public bool CanPrint(bool isColored)
        {
            if (isColored)
                return cyan >= colorInkPerPage && magenta >= colorInkPerPage && yellow >= colorInkPerPage;
            return black >= blackInkPerPage;
        }

        public void ConsumeInk(bool isColored)
        {
            if (isColored)
            {
                cyan    = Mathf.Max(0f, cyan    - colorInkPerPage);
                magenta = Mathf.Max(0f, magenta - colorInkPerPage);
                yellow  = Mathf.Max(0f, yellow  - colorInkPerPage);
            }
            else
            {
                black = Mathf.Max(0f, black - blackInkPerPage);
            }
            OnInkChanged?.Invoke();
        }

        public void RefillCyan(float amount)
        {
            cyan = Mathf.Clamp(cyan + amount, 0f, maxInkLevel);
            OnInkChanged?.Invoke();
        }

        public void RefillMagenta(float amount)
        {
            magenta = Mathf.Clamp(magenta + amount, 0f, maxInkLevel);
            OnInkChanged?.Invoke();
        }

        public void RefillYellow(float amount)
        {
            yellow = Mathf.Clamp(yellow + amount, 0f, maxInkLevel);
            OnInkChanged?.Invoke();
        }

        public void RefillBlack(float amount)
        {
            black = Mathf.Clamp(black + amount, 0f, maxInkLevel);
            OnInkChanged?.Invoke();
        }

        /// <summary>Tüm kanalları aynı anda doldurur. Evrensel mürekkep kartuşu için kullanılır.</summary>
        public void RefillAll(float amount)
        {
            cyan    = Mathf.Clamp(cyan    + amount, 0f, maxInkLevel);
            magenta = Mathf.Clamp(magenta + amount, 0f, maxInkLevel);
            yellow  = Mathf.Clamp(yellow  + amount, 0f, maxInkLevel);
            black   = Mathf.Clamp(black   + amount, 0f, maxInkLevel);
            OnInkChanged?.Invoke();
        }
    }
}