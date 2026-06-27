using System;
using UnityEngine;

namespace SyntaxSultan.PrinterSystem
{
    /// <summary>
    /// Kağıt haznesi kapasitesini takip eder.
    /// Refill dışarıdan çağrılır; hazneye sığandan fazlası sessizce yok sayılır.
    /// </summary>
    [Serializable]
    public class PrinterPaperTray
    {
        [SerializeField] private int maxCapacity  = 50;
        [SerializeField] private int currentCount = 50;

        public int  MaxCapacity  => maxCapacity;
        public int  CurrentCount => currentCount;
        public bool HasPaper     => currentCount > 0;

        /// <summary>UI progress bar için normalize edilmiş doluluk oranı (0–1).</summary>
        public float NormalizedFillRatio => maxCapacity > 0 ? (float)currentCount / maxCapacity : 0f;

        public event Action OnPaperCountChanged;

        public bool TryConsume()
        {
            if (!HasPaper) return false;
            currentCount--;
            OnPaperCountChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Hazneye kağıt ekler. Sığabilecek maksimum kadar ekler.
        /// Dönüş: gerçekte eklenen kağıt adedi (UI feedback için).
        /// </summary>
        public int Refill(int amount)
        {
            int space = maxCapacity - currentCount;
            int added = Mathf.Min(amount, space);
            currentCount += added;
            if (added > 0) OnPaperCountChanged?.Invoke();
            return added;
        }
    }
}