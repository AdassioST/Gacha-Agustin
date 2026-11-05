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
        /// <summary>La versión guardada justo antes de la actual, o null. Sirve para recuperar un perfil dañado.</summary>
        string LoadBackup(string slot);
    }

    /// <summary>
    /// Guardado en archivos JSON de una carpeta (en Unity, Application.persistentDataPath). Cada guardado escribe un
    /// temporal completo y lo pone en su lugar con renombres; la versión anterior queda como respaldo (.bak).
    /// Un corte en cualquier punto deja al menos una copia completa, y Load la encuentra.
    /// </summary>
    public sealed class FileSaveStore : ISaveStore
    {
        private readonly string _directory;

        public FileSaveStore(string directory)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            Directory.CreateDirectory(_directory);
        }

        public string PathFor(string slot) => Path.Combine(_directory, slot + ".json");
        private string TempFor(string slot) => PathFor(slot) + ".tmp";
        private string BackupFor(string slot) => PathFor(slot) + ".bak";

        /// <summary>Si el corte cayó entre los dos renombres, el perfil vigente es el temporal ya completo.</summary>
        private string Current(string slot)
        {
            if (File.Exists(PathFor(slot))) return PathFor(slot);
            if (File.Exists(TempFor(slot)) && File.Exists(BackupFor(slot))) return TempFor(slot);
            return null;
        }

        public bool Exists(string slot) => Current(slot) != null || File.Exists(BackupFor(slot));

        public string Load(string slot)
        {
            var path = Current(slot);
            if (path != null) return File.ReadAllText(path);
            return LoadBackup(slot);
        }

        public string LoadBackup(string slot) => File.Exists(BackupFor(slot)) ? File.ReadAllText(BackupFor(slot)) : null;

        public void Save(string slot, string json)
        {
            string path = PathFor(slot), tmp = TempFor(slot), bak = BackupFor(slot);
            // Si quedó un temporal completo de un corte anterior y no hay perfil, ese temporal es el perfil vigente.
            if (!File.Exists(path) && File.Exists(tmp) && File.Exists(bak)) File.Move(tmp, path);
            using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(fs, new System.Text.UTF8Encoding(false)))
            {
                writer.Write(json);
                writer.Flush();
                fs.Flush(true);
            }
            if (File.Exists(path))
            {
                if (File.Exists(bak)) File.Delete(bak);
                File.Move(path, bak);
            }
            File.Move(tmp, path);
        }

        public void Delete(string slot)
        {
            foreach (var p in new[] { PathFor(slot), TempFor(slot), BackupFor(slot) })
                if (File.Exists(p)) File.Delete(p);
        }
    }

    /// <summary>Guardado en memoria (pruebas y simulador).</summary>
    public sealed class MemorySaveStore : ISaveStore
    {
        private readonly Dictionary<string, string> _slots = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _backups = new Dictionary<string, string>();

        public bool Exists(string slot) => _slots.ContainsKey(slot);
        public string Load(string slot) => _slots.TryGetValue(slot, out var s) ? s : null;
        public string LoadBackup(string slot) => _backups.TryGetValue(slot, out var s) ? s : null;

        public void Save(string slot, string json)
        {
            if (_slots.TryGetValue(slot, out var old)) _backups[slot] = old;
            _slots[slot] = json;
        }

        public void Delete(string slot)
        {
            _slots.Remove(slot);
            _backups.Remove(slot);
        }
    }

    /// <summary>El guardado lo escribió una versión más nueva del juego: nunca se sustituye por una copia vieja.</summary>
    public sealed class NewerSaveException : InvalidOperationException
    {
        public NewerSaveException(string message) : base(message) { }
    }

    public static class ProfileSerializer
    {
        public static string ToJson(PlayerProfile profile) => JsonSerializer.ToJson(profile);

        public static PlayerProfile FromJson(string json)
        {
            var p = JsonSerializer.FromJson<PlayerProfile>(json);
            if (p == null) throw new InvalidOperationException("El guardado está vacío.");
            if (p.Version > PlayerProfile.CurrentVersion)
                throw new NewerSaveException($"El guardado es de una versión más nueva ({p.Version}).");
            // Un bloque en null (guardado editado a mano o de una versión intermedia) vuelve a sus valores por defecto.
            p.Calculista = p.Calculista ?? new CalculistaState();
            p.TermasBase = p.TermasBase ?? new TermasState();
            p.Store = p.Store ?? new StoreState();
            p.Quests = p.Quests ?? new QuestsState();
            p.Settings = p.Settings ?? new SettingsState();
            p.Ledger = p.Ledger ?? new Economy.Ledger();
            p.Gacha = p.Gacha ?? new GachaState();
            p.Fresno = p.Fresno ?? new FresnoProfile();
            p.Fresno.Eco = p.Fresno.Eco ?? new List<EcoSave>();
            while (p.Fresno.Eco.Count < 3) p.Fresno.Eco.Add(null);
            // Versión 1 a 2: la Fase 3 agrega Calculista, Termas, tienda y ajustes. Lo que falta toma sus valores por defecto.
            p.Version = PlayerProfile.CurrentVersion;
            return p;
        }
    }
}
