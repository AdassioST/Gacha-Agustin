using System;
using System.Collections.Generic;
using SalYAlmas.Core.Chain;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>Partituras guardadas por el jugador (GDD 9.5). Se guardan en PlayerPrefs y las usa el modo automático.</summary>
    public sealed class SavedPartituras
    {
        private const string Key = "SalYAlmas.Fase1.Partituras";
        public const int MaxSaved = 6;

        [Serializable]
        private class Entry
        {
            public string name;
            public List<string> witches = new List<string>();
            public bool guest;
        }

        [Serializable]
        private class Wrapper
        {
            public List<Entry> items = new List<Entry>();
        }

        public readonly List<SavedPartitura> Items = new List<SavedPartitura>();

        public void Load()
        {
            Items.Clear();
            string json = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var w = JsonUtility.FromJson<Wrapper>(json);
                if (w?.items == null) return;
                foreach (var e in w.items)
                    Items.Add(new SavedPartitura { Name = e.name ?? "", WitchIds = new List<string>(e.witches ?? new List<string>()), WithGuest = e.guest });
            }
            catch (Exception ex)
            {
                Debug.LogWarning("No pude leer las Partituras guardadas: " + ex.Message);
            }
        }

        public void Save()
        {
            var w = new Wrapper();
            foreach (var p in Items) w.items.Add(new Entry { name = p.Name, witches = new List<string>(p.WitchIds), guest = p.WithGuest });
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(w));
            PlayerPrefs.Save();
        }

        public void Add(SavedPartitura p)
        {
            Items.RemoveAll(x => x.Name == p.Name);
            Items.Insert(0, p);
            if (Items.Count > MaxSaved) Items.RemoveRange(MaxSaved, Items.Count - MaxSaved);
            Save();
        }

        public void Remove(int index)
        {
            if (index < 0 || index >= Items.Count) return;
            Items.RemoveAt(index);
            Save();
        }
    }
}
