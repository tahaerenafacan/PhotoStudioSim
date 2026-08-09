using System;
using System.Collections.Generic;

namespace SyntaxSultan.TutorialSystem
{
    /// <summary>
    /// Oyun içi herhangi bir sistemin (temizlik, mağaza açma, kamera kullanma vb.)
    /// Tutorial sistemiyle DOĞRUDAN referans almadan konuşmasını sağlar.
    /// Neden: TutorialManager'ı her yeni mekanikte güncellemek yerine,
    /// mekanikler sadece bir "key" yayınlar, kim dinliyorsa tepki verir.
    /// </summary>
    public static class TutorialEventBus
    {
        private static readonly Dictionary<string, Action<int>> events = new();

        public static void Subscribe(TutorialObjectiveKeys eventKey, Action<int> listener)
        {
            if (!events.ContainsKey(eventKey.ToString()))
                events[eventKey.ToString()] = delegate { };
            events[eventKey.ToString()] += listener;
        }

        public static void Unsubscribe(TutorialObjectiveKeys eventKey, Action<int> listener)
        {
            if (events.ContainsKey(eventKey.ToString()))
                events[eventKey.ToString()] -= listener;
        }

        // amount: bazı objective'ler tek seferde birden fazla ilerleme kaydedebilir (örn: 3 kir aynı anda silindi)
        public static void Raise(TutorialObjectiveKeys eventKey, int amount = 1)
        {
            if (events.TryGetValue(eventKey.ToString(), out var action))
                action.Invoke(amount);
        }
    }
}