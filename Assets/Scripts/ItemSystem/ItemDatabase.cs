using System.Collections.Generic;
using UnityEngine;

namespace ItemSystem
{
    /// <summary>
    /// itemId → ItemDefinition çözümü. Resources/Items klasöründeki tüm
    /// ItemDefinition asset'lerini ilk erişimde tarar ve cache'ler (SSOT).
    /// KURULUM: Tüm ItemDefinition asset'lerini "Resources/Items" altına koy.
    /// </summary>
    public static class ItemDatabase
    {
        private static Dictionary<string, ItemDefinition> lookup;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            lookup = null;
        }


        public static ItemDefinition GetById(string itemId)
        {
            if (lookup == null) BuildLookup();
            return lookup.TryGetValue(itemId, out var def) ? def : null;
        }

        private static void BuildLookup()
        {
            lookup = new Dictionary<string, ItemDefinition>();
            foreach (var def in Resources.LoadAll<ItemDefinition>("Data/Items"))
            {
                lookup[def.ItemId] = def;
            }
        }
    }
}