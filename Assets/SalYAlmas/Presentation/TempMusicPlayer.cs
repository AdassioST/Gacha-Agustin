using System;
using System.Collections.Generic;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// Qué pantalla está sonando. La decide la presentación, no el núcleo.
    /// Title es la pantalla de inicio; Menu, el menú general (en el prototipo, la formación).
    /// </summary>
    public enum MusicContext { None, Title, Menu, Battle, Boss, Victory, Defeat, Termas, FresnoSantuario }

    [Serializable]
    internal sealed class TempMusicTrack
    {
        public string resource = "";
        public float gainDb;
    }

    [Serializable]
    internal sealed class TempMusicEntry
    {
        public string context = "";
        /// <summary>loop: una pista al azar en bucle. shuffle: pistas al azar, una tras otra. once: una al azar y silencio.</summary>
        public string mode = "loop";
        /// <summary>Pista que suena la primera vez que se entra al contexto en la sesión (por ejemplo, el inicio).</summary>
        public string first = "";
        public TempMusicTrack[] tracks = new TempMusicTrack[0];
    }

    [Serializable]
    internal sealed class TempMusicCatalog
    {
        public TempMusicEntry[] entries = new TempMusicEntry[0];
    }

    /// <summary>
    /// Música temporal del prototipo con las tomas de Suno (docs/audio/TOMAS_LOTE_1.md y TOMAS_LOTE_2.md).
    /// Lee Resources/MusicaTemporal/musica_temporal.json, que da a cada contexto una lista de pistas con su ganancia
    /// y un modo de reproducción. Cambia de pista con un fundido cruzado. La Fase 3C la reemplaza por FMOD
    /// (docs/audio/INTEGRACION_AUDIO.md). El azar de aquí es solo de presentación: no toca el núcleo determinista.
    /// </summary>
    public sealed class TempMusicPlayer : MonoBehaviour
    {
        private const string Folder = "MusicaTemporal/";
        private const string CatalogName = "musica_temporal";
        private const string MutedKey = "SalYAlmas.MusicaSilenciada";

        [Range(0f, 1f)] public float MasterVolume = 0.55f; // TUNING
        public float FadeSeconds = 1.5f; // TUNING
        [Tooltip("Volumen relativo mientras la pantalla de Cadena está abierta.")]
        [Range(0f, 1f)] public float DuckFactor = 0.45f; // TUNING

        private readonly Dictionary<MusicContext, TempMusicEntry> _entries = new Dictionary<MusicContext, TempMusicEntry>();
        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        private readonly HashSet<MusicContext> _firstPlayed = new HashSet<MusicContext>();
        private readonly Dictionary<MusicContext, string> _lastTrack = new Dictionary<MusicContext, string>();
        private AudioSource _current;
        private AudioSource _previous;
        private float _currentGain = 1f;
        private float _previousGain = 1f, _previousEnvelope;
        private float _fadeElapsed, _envelope, _duck = 1f;
        private float _duckUntil, _transientDuck = 1f;
        private bool _ducked;
        private bool _advancing;
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
            source.loop = false;
            source.spatialBlend = 0f;
            source.priority = 0;
            source.dopplerLevel = 0f;
            source.reverbZoneMix = 0f;
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
            PlayNext();
        }

        public void ToggleMute()
        {
            Muted = !Muted;
            PlayerPrefs.SetInt(MutedKey, Muted ? 1 : 0);
            PlayerPrefs.Save();
            _current.mute = _previous.mute = Muted;
        }

        /// <summary>Espacio para firmas, sin reiniciar la canción ni el fundido entre contextos.</summary>
        public void DuckFor(float seconds, float factor)
        {
            _transientDuck = Time.unscaledTime >= _duckUntil ? Mathf.Clamp01(factor) : Mathf.Min(_transientDuck, Mathf.Clamp01(factor));
            _duckUntil = Mathf.Max(_duckUntil, Time.unscaledTime + Mathf.Max(0f, seconds));
        }

        /// <summary>Elige la siguiente pista del contexto actual y hace el fundido cruzado hacia ella.</summary>
        private void PlayNext()
        {
            _previousGain = _currentGain;
            _previousEnvelope = _envelope;
            _fadeElapsed = 0f;
            _envelope = 0f;
            var old = _current;
            _current = _previous;
            _previous = old;

            _current.Stop();
            _current.clip = null;
            _current.volume = 0f;
            _currentGain = 1f;

            if (!_entries.TryGetValue(_context, out var entry)) return;
            var track = PickTrack(_context, entry);
            if (track == null) return;
            var clip = Clip(track.resource);
            if (clip == null) return;

            _current.clip = clip;
            _current.loop = string.Equals(entry.mode, "loop", StringComparison.OrdinalIgnoreCase);
            _currentGain = Mathf.Pow(10f, track.gainDb / 20f);
            _current.Play();
            _lastTrack[_context] = track.resource;
        }

        private TempMusicTrack PickTrack(MusicContext context, TempMusicEntry entry)
        {
            if (entry.tracks == null || entry.tracks.Length == 0) return null;

            if (!string.IsNullOrEmpty(entry.first) && _firstPlayed.Add(context))
                foreach (var t in entry.tracks)
                    if (t != null && t.resource == entry.first) return t;

            var candidates = new List<TempMusicTrack>();
            _lastTrack.TryGetValue(context, out var last);
            foreach (var t in entry.tracks)
                if (t != null && (entry.tracks.Length == 1 || t.resource != last)) candidates.Add(t);
            if (candidates.Count == 0) return entry.tracks[0];
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
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
            // En modo shuffle, la siguiente pista entra con fundido cruzado cuando a la actual le queda lo que dura el fundido.
            if (_current.clip != null && !_current.loop && _entries.TryGetValue(_context, out var entry)
                && string.Equals(entry.mode, "shuffle", StringComparison.OrdinalIgnoreCase))
            {
                bool ending = !_current.isPlaying || _current.clip.length - _current.time <= FadeSeconds;
                if (ending && !_advancing)
                {
                    _advancing = true;
                    PlayNext();
                }
                else if (!ending)
                {
                    _advancing = false;
                }
            }

            float dt = Time.unscaledDeltaTime;
            float targetDuck = _ducked ? DuckFactor : 1f;
            if (Time.unscaledTime < _duckUntil) targetDuck = Mathf.Min(targetDuck, _transientDuck);
            _duck = Mathf.Lerp(_duck, targetDuck, 1f - Mathf.Exp(-dt / (targetDuck < _duck ? 0.06f : 0.6f)));
            _fadeElapsed += dt;
            float progress = Mathf.Clamp01(_fadeElapsed / Mathf.Max(0.01f, FadeSeconds));
            _envelope = Mathf.Sin(progress * Mathf.PI * 0.5f);
            _current.mute = _previous.mute = Muted;
            _current.volume = MasterVolume * _currentGain * _duck * _envelope;
            _previous.volume = MasterVolume * _previousGain * _duck * _previousEnvelope * Mathf.Cos(progress * Mathf.PI * 0.5f);
            if (_previous.isPlaying && progress >= 1f)
            {
                _previous.Stop();
                _previous.clip = null;
            }
        }
    }
}
