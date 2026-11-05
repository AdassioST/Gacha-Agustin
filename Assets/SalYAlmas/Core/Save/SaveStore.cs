using System;
using System.Collections.Generic;
using System.IO;

namespace SalYAlmas.Core.Save
{
    /// <summary>
    /// Dónde vive el guardado. Hoy es un archivo local; en la Fase 3 la misma interfaz puede apuntar al servidor
    /// (Cloud Save, PlayFab o Nakama) sin tocar el núcleo.
    /// </summary>
    public interface ISaveStore
    {
        bool Exists(string slot);
        string Load(string slot);
        void Save(string slot, string json);
        void Delete(string slot);
    }

    /// <summary>Guardado en archivos JSON de una carpeta (en Unity, Application.persistentDataPath).</summary>
    public sealed class FileSaveStore : ISaveStore
    {
        private readonly string _directory;

        public FileSaveStore(string directory)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            Directory.CreateDirectory(_directory);
        }

        public string PathFor(string slot) => Path.Combine(_directory, slot + ".json");

        public bool Exists(string slot) => File.Exists(PathFor(slot));

        public string Load(string slot) => File.Exists(PathFor(slot)) ? File.ReadAllText(PathFor(slot)) : null;

        public void Save(string slot, string json)
        {
            // Escribe primero a un temporal: un corte a medio guardar no deja el perfil roto.
            string path = PathFor(slot);
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }

        public void Delete(string slot)
        {
            if (File.Exists(PathFor(slot))) File.Delete(PathFor(slot));
        }
    }

    /// <summary>Guardado en memoria (pruebas y simulador).</summary>
    public sealed class MemorySaveStore : ISaveStore
    {
        private readonly Dictionary<string, string> _slots = new Dictionary<string, string>();

        public bool Exists(string slot) => _slots.ContainsKey(slot);
        public string Load(string slot) => _slots.TryGetValue(slot, out var s) ? s : null;
        public void Save(string slot, string json) => _slots[slot] = json;
        public void Delete(string slot) => _slots.Remove(slot);
    }

    public static class ProfileSerializer
    {
        public static string ToJson(PlayerProfile profile) => JsonSerializer.ToJson(profile);

        public static PlayerProfile FromJson(string json)
        {
            var p = JsonSerializer.FromJson<PlayerProfile>(json);
            if (p.Version > PlayerProfile.CurrentVersion)
                throw new InvalidOperationException($"El guardado es de una versión más nueva ({p.Version}).");
            while (p.Fresno.Eco.Count < 3) p.Fresno.Eco.Add(null);
            return p;
        }
    }
}
