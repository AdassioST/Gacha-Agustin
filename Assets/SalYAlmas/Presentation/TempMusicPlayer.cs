using System;
using System.Collections.Generic;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>Qué pantalla está sonando. La decide la presentación, no el núcleo.</summary>
    public enum MusicContext { None, Formation, Battle, Boss, Victory, Defeat, Termas }

    [Serializable]
    internal sealed class TempMusicEntry
    {
        public string context = "";
        public string resource = "";
        public float gainDb;
    }

    [Serializable]
    internal sealed class TempMusicCatalog
    {
        public TempMusicEntry[] entries = new TempMusicEntry[0];
    }

    /// <summary>
    /// Música temporal del prototipo con las tomas de Suno del lote 1 (docs/audio/TOMAS_LOTE_1.md).
    /// Lee Resources/MusicaTemporal/musica_temporal.json, que asigna una pista y su ganancia a cada contexto,
    /// y cambia de pista con un fundido cruzado. La Fase 3C la reemplaza por FMOD (docs/audio/INTEGRACION_AUDIO.md).
    /// </summary>
    public sealed class TempMusicPlayer : MonoBehaviour
    {
        private const string Folder = "MusicaTemporal/";
        private const string CatalogName = "musica_temporal";
        private const string MutedKey = "SalYAlmas.MusicaSilenciada";

        [Range(0f, 1f)] public float MasterVolume = 0.7f; // TUNING
        public float FadeSeconds = 1.5f; // TUNING
        [Tooltip("Volumen relativo mientras la pantalla de Cadena está abierta.")]
        [Range(0f, 1f)] public float DuckFactor = 0.45f; // TUNING

        private readonly Dictionary<MusicContext, TempMusicEntry> _entries = new Dictionary<MusicContext, TempMusicEntry>();
        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        private AudioSource _current;
        private AudioSource _previous;
        private float _currentGain = 1f;
        private bool _ducked;
        private MusicContext _context = MusicContext.None;

        public bool Muted { get; private set; }
        public MusicContext Context => _context;

        private void Awake()
        {
            _current = CreateSource("Música A");
            _previous = CreateSource("Música B");
            Muted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
            LoadCatalog();
        }

        private AudioSource CreateSource(string sourceName)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
            return source;
        }

        private void LoadCatalog()
        {
            var asset = Resources.Load<TextAsset>(Folder + CatalogName);
            if (asset == null)
            {
                Debug.LogWarning("Música temporal: no encontré Resources/" + Folder + CatalogName + ".json. El prototipo sigue sin música.");
                return;
            }
            var catalog = JsonUtility.FromJson<TempMusicCatalog>(asset.text);
            if (catalog == null || catalog.entries == null) return;
            foreach (var entry in catalog.entries)
            {
                if (entry != null && Enum.TryParse(entry.context, true, out MusicContext context))
                    _entries[context] = entry;
                else
                    Debug.LogWarning("Música temporal: contexto desconocido en el catálogo: " + (entry != null ? entry.context : "(vacío)"));
            }
        }

        /// <summary>Se llama en cada cuadro con la pantalla actual. Solo cambia de pista cuando cambia el contexto.</summary>
        public void SetState(MusicContext context, bool ducked)
        {
            _ducked = ducked;
            if (context == _context) return;
            _context = context;

            var old = _current;
            _current = _previous;
            _previous = old;

            _current.Stop();
            _current.clip = null;
            _current.volume = 0f;
            _currentGain = 1f;

            if (_entries.TryGetValue(context, out var entry))
            {
                var clip = Clip(entry.resource);
                if (clip != null)
                {
                    _current.clip = clip;
                    _currentGain = Mathf.Pow(10f, entry.gainDb / 20f);
                    _current.Play();
                }
            }
        }

        public void ToggleMute()
        {
            Muted = !Muted;
            PlayerPrefs.SetInt(MutedKey, Muted ? 1 : 0);
            PlayerPrefs.Save();
        }

        private AudioClip Clip(string resource)
        {
            if (string.IsNullOrEmpty(resource)) return null;
            if (_clips.TryGetValue(resource, out var cached)) return cached;
            var clip = Resources.Load<AudioClip>(Folder + resource);
            if (clip == null) Debug.LogWarning("Música temporal: no encontré la pista Resources/" + Folder + resource);
            _clips[resource] = clip;
            return clip;
        }

        private void Update()
        {
            float step = Time.unscaledDeltaTime / Mathf.Max(0.01f, FadeSeconds);
            float target = Muted || _current.clip == null ? 0f : MasterVolume * _currentGain * (_ducked ? DuckFactor : 1f);
            _current.volume = Mathf.MoveTowards(_current.volume, target, step);
            _previous.volume = Mathf.MoveTowards(_previous.volume, 0f, step);
            if (_previous.isPlaying && _previous.volume <= 0f)
            {
                _previous.Stop();
                _previous.clip = null;
            }
        }
    }
}
