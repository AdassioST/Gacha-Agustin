using System;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>16 voces y cuatro colas de salida: una ráfaga no puede tapar una firma ni cortar una cola en seco.</summary>
    internal sealed class SfxVoicePool
    {
        internal const int Capacity = 16;
        private const int ReleaseVoices = 4;
        private const float ReleaseSeconds = 0.02f;

        private sealed class Voice
        {
            public AudioSource Source;
            public string Family;
            public int Priority;
            public float Started, Level, ReleaseAt = -1f, ReleaseDuration;
        }

        private readonly Voice[] _voices = new Voice[Capacity + ReleaseVoices];
        public int ActiveCount { get; private set; }

        public SfxVoicePool(Transform parent)
        {
            for (int i = 0; i < _voices.Length; i++)
            {
                var go = new GameObject("Efectos: voz " + (i + 1));
                go.transform.SetParent(parent, false);
                var source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.dopplerLevel = 0f;
                source.reverbZoneMix = 0f;
                _voices[i] = new Voice { Source = source };
            }
        }

        internal static int Priority(string name)
        {
            if (name.StartsWith("mus_", StringComparison.Ordinal) || name.StartsWith("sfx_ascension_", StringComparison.Ordinal)
                || name.StartsWith("sfx_cadena_spark__", StringComparison.Ordinal)
                || name.StartsWith("sfx_partitura_", StringComparison.Ordinal)
                || name == "sfx_combate_victoria" || name == "sfx_combate_derrota"
                || name.StartsWith("ui_reveal", StringComparison.Ordinal) || name == "ui_enter" || name == "ui_riser") return 32;
            if (name == "ui_drop" || name == "ui_boil" || name == "ui_steam") return 80;
            if (name.StartsWith("sfx_ui_", StringComparison.Ordinal) || name.StartsWith("sfx_libro_", StringComparison.Ordinal)
                || name.StartsWith("ui_", StringComparison.Ordinal)) return 192;
            if (name.Contains("estado_tick") || name.Contains("granos__") || name.Contains("dano_recibido")) return 144;
            return 80;
        }

        public bool Play(AudioClip clip, string family, float level, float master, float now, float pitch = 1f)
        {
            int priority = Priority(family);
            int count = 0, active = 0;
            Voice idle = null, victim = null;
            foreach (var v in _voices)
            {
                if (!v.Source.isPlaying) { if (idle == null) idle = v; continue; }
                if (v.ReleaseAt >= 0f) continue;
                active++;
                if (v.Family == family) count++;
            }
            int limit = priority == 192 ? 2 : 3;
            bool replaceFamily = count >= limit;
            if (replaceFamily || active >= Capacity)
            {
                foreach (var v in _voices)
                {
                    if (!v.Source.isPlaying || v.ReleaseAt >= 0f || (replaceFamily && v.Family != family)) continue;
                    if (v.Priority < priority) continue;
                    if (victim == null || v.Priority > victim.Priority || (v.Priority == victim.Priority && v.Started < victim.Started)) victim = v;
                }
                if (victim == null || idle == null) return false;
                victim.ReleaseAt = now;
                victim.ReleaseDuration = ReleaseSeconds;
            }
            if (idle == null) return false;
            idle.Family = family;
            idle.Priority = priority;
            idle.Started = now;
            idle.ReleaseAt = -1f;
            // Dejar espacio a la música y a la suma de capas. La afinación de los metales no se aleatoriza.
            idle.Level = level * (priority == 192 ? 0.48f : priority == 144 ? 0.58f : priority == 32 ? 0.9f : 0.78f);
            idle.Source.clip = clip;
            idle.Source.priority = priority;
            idle.Source.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
            idle.Source.mute = false;
            idle.Source.volume = Mathf.Clamp01(idle.Level * master);
            idle.Source.Play();
            return true;
        }

        public void Tick(float master, float now, bool muted)
        {
            ActiveCount = 0;
            foreach (var v in _voices)
            {
                v.Source.mute = muted;
                if (!v.Source.isPlaying) continue;
                float envelope = v.ReleaseAt < 0f ? 1f : Mathf.Clamp01(1f - (now - v.ReleaseAt) / v.ReleaseDuration);
                v.Source.volume = Mathf.Clamp01(master * v.Level * envelope);
                if (envelope <= 0f) { v.Source.Stop(); v.Source.clip = null; }
                else if (v.ReleaseAt < 0f) ActiveCount++;
            }
        }

        public void ReleaseFamily(string family, float now, float seconds)
        {
            foreach (var v in _voices)
            {
                if (!v.Source.isPlaying || v.Family != family || v.ReleaseAt >= 0f) continue;
                v.ReleaseAt = now;
                v.ReleaseDuration = Mathf.Max(0.001f, seconds);
            }
        }

        public void Stop()
        {
            foreach (var v in _voices) { v.Source.Stop(); v.Source.clip = null; v.ReleaseAt = -1f; }
            ActiveCount = 0;
        }
    }
}
