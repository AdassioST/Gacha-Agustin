using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    [Serializable]
    internal sealed class TempSfxIndex
    {
        public string[] files = new string[0];
    }

    /// <summary>
    /// Efectos de sonido temporales del prototipo. Escucha los eventos que el BattleDirector muestra en pantalla
    /// (así el sonido cae cuando se ve el golpe, no cuando lo calcula el núcleo) y los traduce a los efectos de
    /// Resources/SfxTemporal, generados con Tools/Audio/sfx. Los nombres siguen docs/audio/INTEGRACION_AUDIO.md:
    /// sfx_id, variantes con "__valor" y variaciones al azar con "__vN". La Fase 3C lo reemplaza por FMOD.
    /// El azar de aquí es solo de presentación: no toca el núcleo determinista.
    /// </summary>
    public sealed class TempSfxPlayer : MonoBehaviour
    {
        private const string Folder = "SfxTemporal/";
        private const string IndexName = "sfx_indice";
        private const string MutedKey = "SalYAlmas.EfectosSilenciados";

        [Range(0f, 1f)] public float MasterVolume = 0.85f; // TUNING
        [Tooltip("Tiempo mínimo entre dos disparos del mismo efecto, para no saturar las Cadenas.")]
        public float MinRepeatSeconds = 0.03f; // TUNING

        private struct Scheduled
        {
            public float Time;
            public string Name;
            public float Volume;
        }

        private readonly List<string> _files = new List<string>();
        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, float> _lastPlayed = new Dictionary<string, float>();
        private readonly List<Scheduled> _scheduled = new List<Scheduled>();

        // Estado del combate que solo importa al sonido.
        private Battle _battle;
        private readonly HashSet<int> _ritualDone = new HashSet<int>();
        private readonly Dictionary<int, ActionKind> _lastAction = new Dictionary<int, ActionKind>();
        private readonly Dictionary<int, ChainRole> _chainRole = new Dictionary<int, ChainRole>();
        private readonly Dictionary<int, SalinityBand> _band = new Dictionary<int, SalinityBand>();
        private int _sparkIndex;
        private bool _chainWithGuest;
        private float _pendingCloseTime = -1f;

        private AudioSource _oneShots;
        private AudioSource _crystalLoop;

        public bool Muted { get; private set; }

        private void Awake()
        {
            _oneShots = CreateSource("Efectos");
            _crystalLoop = CreateSource("Efectos: cristalización");
            _crystalLoop.loop = true;
            Muted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
            LoadIndex();
        }

        private AudioSource CreateSource(string sourceName)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }

        private void LoadIndex()
        {
            var asset = Resources.Load<TextAsset>(Folder + IndexName);
            if (asset == null)
            {
                Debug.LogWarning("Efectos temporales: no encontré Resources/" + Folder + IndexName + ".json. El prototipo sigue sin efectos.");
                return;
            }
            var index = JsonUtility.FromJson<TempSfxIndex>(asset.text);
            if (index != null && index.files != null) _files.AddRange(index.files);
        }

        public void ToggleMute()
        {
            Muted = !Muted;
            PlayerPrefs.SetInt(MutedKey, Muted ? 1 : 0);
            PlayerPrefs.Save();
            if (Muted) _crystalLoop.Stop();
        }

        // ------------------------------------------------------------------ Conexión con la escena

        /// <summary>Se llama al empezar cada combate (también en las repeticiones).</summary>
        public void Bind(Battle battle)
        {
            _battle = battle;
            _ritualDone.Clear();
            _lastAction.Clear();
            _chainRole.Clear();
            _band.Clear();
            _scheduled.Clear();
            _sparkIndex = 0;
            _crystalLoop.Stop();
        }

        /// <summary>La pantalla de Cadena se abrió o se cerró.</summary>
        public void NotifyChainScreen(bool open)
        {
            if (open)
            {
                _pendingCloseTime = -1f;
                Play("sfx_cadena_abrir");
            }
            else
            {
                // Si en un momento llega ChainStarted, fue una confirmación; si no, se cerró sin ejecutar.
                _pendingCloseTime = Time.unscaledTime + 0.3f;
            }
        }

        public void OnButtonPressed() => Play("sfx_ui_tocar", 0.8f);

        /// <summary>Efecto suelto pedido por una pantalla (la tirada, las Termas).</summary>
        public void PlayCue(string id) => Play(id);

        /// <summary>Traduce un evento del núcleo a sus efectos. Lo invoca BattleDirector.EventPresented.</summary>
        public void OnEvent(BattleEvent e)
        {
            switch (e)
            {
                case BattleStarted _:
                    Play("sfx_combate_entrada");
                    break;

                case TurnStarted t:
                    Play("sfx_combate_turno", 0.7f, 0f, t.Side == Side.Ally ? "aliada" : "enemiga");
                    if (_band.TryGetValue(t.UnitId, out var turnBand) && turnBand == SalinityBand.CristalizacionTurbia)
                        Play("sfx_salinidad_perdida_vida");
                    break;

                case ActionUsed a:
                    _lastAction[a.ActorId] = a.Action;
                    OnAction(a);
                    break;

                case HitLanded h:
                    OnHit(h);
                    break;

                case ShieldGained _:
                    Play("sfx_combate_escudo", 1f, 0f, "poner");
                    break;

                case DotTicked d:
                    Play("sfx_combate_estado_tick", 0.8f, 0f, DotName(d.Planet));
                    break;

                case Healed heal:
                    if (heal.Hp > 0 && heal.Hp - heal.Amount <= 0) Play("sfx_combate_revivir");
                    break;

                case AspectFormed asp:
                    Play("sfx_aspecto_" + AspectName(asp.Aspect));
                    break;

                case ReactionTriggered r:
                    if (r.Reaction != HybridReaction.None) Play("sfx_reaccion_" + ReactionName(r.Reaction));
                    break;

                case BreakTriggered b:
                    Play("sfx_quiebre_firmeza");
                    if (b.Planet == Planet.Jupiter) Play("sfx_quiebre_jupiter", 1f, 0.15f, "quiebre");
                    else Play("sfx_quiebre_" + PlanetName(b.Planet), 1f, 0.15f);
                    break;

                case SalinityBandChanged s:
                    OnSalinityBand(s);
                    break;

                case GrainsChanged g:
                    if (g.New != g.Old) Play("sfx_combate_granos", 0.8f, 0f, g.New > g.Old ? "sube" : "baja");
                    break;

                case ResonanceChanged r:
                    if (r.Old < 100 && r.New >= 100) Play("sfx_cadena_resonancia_llena");
                    break;

                case AnimaChanged an:
                    for (int level = 3; level >= 1; level--)
                        if (an.Old < level * 100 && an.New >= level * 100)
                        {
                            Play("sfx_ascension_carga", 1f, 0f, level.ToString(CultureInfo.InvariantCulture));
                            break;
                        }
                    break;

                case ChainStarted c:
                    if (_pendingCloseTime >= 0f) Play("sfx_cadena_confirmar");
                    _pendingCloseTime = -1f;
                    _sparkIndex = 0;
                    _chainRole.Clear();
                    _chainWithGuest = c.WithGuest;
                    Play("sfx_cadena_calculo", 1f, 0.1f);
                    break;

                case ChainLinkStarted l:
                    _chainRole[l.WitchId] = l.Role;
                    Play("sfx_cadena_eslabon", 1f, 0f, RoleName(l.Role));
                    if (_chainWithGuest && l.Role == ChainRole.Cierre) Play("sfx_cadena_invitada");
                    break;

                case ChainEnded c:
                    Play("sfx_cadena_fin", 1f, 0f, c.Sparks == 0 ? "0" : c.Sparks <= 2 ? "1" : "3");
                    _chainRole.Clear();
                    break;

                case AscensionCast asc:
                    OnAscension(asc);
                    break;

                case PhaseChanged _:
                    Play("sfx_combate_jefe_fase");
                    break;

                case EnemyCharging _:
                    Play("sfx_combate_jefe_aviso");
                    break;

                case UnitDefeated u:
                    Play("sfx_combate_caida");
                    if (u.Side == Side.Enemy) Play("sfx_almita_soltar", 0.9f, 0.25f);
                    break;

                case BattleEnded end:
                    _crystalLoop.Stop();
                    Play(end.Outcome == BattleOutcome.Victory ? "sfx_combate_victoria" : "sfx_combate_derrota");
                    break;
            }
        }

        private void OnAction(ActionUsed a)
        {
            var witch = _battle != null ? _battle.FindWitch(a.ActorId) : null;
            switch (a.Action)
            {
                case ActionKind.Basic:
                    if (witch != null) Play("sfx_combate_ataque", 1f, 0f, "natural");
                    break;

                case ActionKind.Spell:
                    if (witch == null) break;
                    bool first = _ritualDone.Add(a.ActorId);
                    float gesture = first ? 0.40f : 0.20f;
                    Play("sfx_ritual_" + WitchSoundId(witch), 1f, 0f, first ? "largo" : "corto");
                    Play("sfx_sal_crujido", 1f, gesture, PlanetName(witch.Dominant));
                    Play("sfx_magia_ascendente_aparece", 0.8f, gesture, SignName(witch.Def.AscendantSign));
                    Play("sfx_magia_canalizar", 1f, gesture + 0.05f, PlanetName(witch.Dominant));
                    break;

                case ActionKind.SelfCast:
                    if (witch == null) break;
                    Play("sfx_magia_autohechizo", 1f, 0f, SelfCastName(witch.Def.SelfCast));
                    if (witch.Def.SelfCast == SelfCastForm.FrenesiSalNegra) Play("sfx_magia_sal_negra", 1f, 0.2f);
                    break;

                case ActionKind.Vislumbre:
                    Play("sfx_magia_vislumbre", 1f, 0f, "destilada");
                    break;

                case ActionKind.Potion:
                    Play("sfx_pocion_cinturon", 0.8f);
                    PotionSounds(a.MoveName, out var bottle, out var effect);
                    Play("sfx_pocion_destapar", 1f, 0.1f, bottle);
                    Play("sfx_pocion_beber", 1f, 0.35f);
                    Play("sfx_pocion_efecto", 1f, 0.75f, effect);
                    break;
            }
        }

        private void OnHit(HitLanded h)
        {
            var target = _battle != null ? _battle.FindUnit(h.TargetId) : null;
            var attacker = _battle != null ? _battle.FindWitch(h.AttackerId) : null;

            if (h.Source == HitSource.Reaction)
            {
                Play("sfx_quiebre_jupiter", 0.9f, 0f, "salto");
            }
            else if (h.Source == HitSource.Chain)
            {
                if (h.IsSpark)
                {
                    _sparkIndex = Mathf.Min(_sparkIndex + 1, 5);
                    Play("sfx_cadena_spark", 1f, 0f, _sparkIndex.ToString(CultureInfo.InvariantCulture));
                    Play("sfx_almita_spark", 0.7f, 0.05f);
                }
                else
                {
                    var role = _chainRole.TryGetValue(h.AttackerId, out var r) ? r : ChainRole.Enlace;
                    Play("sfx_partitura_nota", 0.9f, 0f, PlanetName(h.Planet), RoleName(role));
                }
            }
            else if (h.Source == HitSource.Ascension || h.Source == HitSource.Ecstasy || h.Source == HitSource.Graft
                     || (h.Source == HitSource.Action && attacker != null && LastAction(h.AttackerId) != ActionKind.Basic))
            {
                Play("sfx_magia_golpe_astral", 1f, 0f, PlanetName(h.Planet), "destilada");
                if (h.IsSpark) Play("sfx_ascension_duo");
            }
            else
            {
                Play("sfx_combate_golpe", 1f, 0f, Weight(h.Damage, target));
            }

            if (h.IsCrit) Play("sfx_combate_critico", 0.9f);
            if (h.ShieldAbsorbed > 0 && h.Damage > 0) Play("sfx_combate_escudo", 0.9f, 0f, "romper");
            if (target is WitchUnit hurt && h.Damage > 0) Play("sfx_combate_dano_recibido", 0.8f, 0.05f, SpeciesGroup(hurt));
        }

        private void OnSalinityBand(SalinityBandChanged s)
        {
            _band[s.WitchId] = s.NewBand;
            if (s.NewBand < s.OldBand)
            {
                Play(s.OldBand == SalinityBand.ExtasisAbisal ? "sfx_salinidad_desalada" : "sfx_salinidad_baja");
            }
            else
            {
                switch (s.NewBand)
                {
                    case SalinityBand.Salobre: Play("sfx_salinidad_salobre"); break;
                    case SalinityBand.Sobresalada: Play("sfx_salinidad_sobresalada"); break;
                    case SalinityBand.CristalizacionTurbia: Play("sfx_salinidad_cristalizacion"); break;
                    case SalinityBand.ExtasisAbisal: Play("sfx_salinidad_extasis"); break;
                }
            }
            UpdateCrystalLoop();
        }

        private void OnAscension(AscensionCast asc)
        {
            var witch = _battle != null ? _battle.FindWitch(asc.WitchId) : null;
            string planet = witch != null ? PlanetName(witch.Dominant) : "sol";
            Play("sfx_ascension_lanzar", 1f, 0f, asc.Level.ToString(CultureInfo.InvariantCulture));
            if (asc.Level >= 3 && witch != null && Exists("sfx_ascension_iii_" + WitchSoundId(witch)))
                Play("sfx_ascension_iii_" + WitchSoundId(witch), 1f, 0.1f, "corto");
            else
                Play(asc.Level == 1 ? "sfx_ascension_i" : "sfx_ascension_ii", 1f, 0.1f, planet);
            Play("sfx_ascension_impacto", 1f, asc.Level == 1 ? 0.8f : asc.Level == 2 ? 1.4f : 2.2f, planet);
        }

        private void UpdateCrystalLoop()
        {
            bool any = false;
            foreach (var kv in _band) if (kv.Value == SalinityBand.CristalizacionTurbia) any = true;
            if (any && !Muted && !_crystalLoop.isPlaying)
            {
                var name = Resolve("sfx_salinidad_cristal_bucle");
                var clip = name != null ? Clip(name) : null;
                if (clip != null)
                {
                    _crystalLoop.clip = clip;
                    _crystalLoop.volume = MasterVolume * 0.5f;
                    _crystalLoop.Play();
                }
            }
            else if (!any && _crystalLoop.isPlaying)
            {
                _crystalLoop.Stop();
            }
        }

        // ------------------------------------------------------------------ Reproducción

        private void Update()
        {
            if (_pendingCloseTime >= 0f && Time.unscaledTime >= _pendingCloseTime)
            {
                _pendingCloseTime = -1f;
                Play("sfx_cadena_cerrar");
            }

            float now = Time.unscaledTime;
            for (int i = _scheduled.Count - 1; i >= 0; i--)
            {
                if (_scheduled[i].Time > now) continue;
                var s = _scheduled[i];
                _scheduled.RemoveAt(i);
                PlayNow(s.Name, s.Volume);
            }
        }

        /// <summary>Busca la variante más específica que exista y la reproduce, ahora o con retraso.</summary>
        private void Play(string id, float volume = 1f, float delay = 0f, params string[] parts)
        {
            if (Muted) return;
            var name = Resolve(id, parts);
            if (name == null) return;
            if (delay > 0f)
                _scheduled.Add(new Scheduled { Time = Time.unscaledTime + delay, Name = name, Volume = volume });
            else
                PlayNow(name, volume);
        }

        private void PlayNow(string name, float volume)
        {
            if (Muted) return;
            float now = Time.unscaledTime;
            if (_lastPlayed.TryGetValue(name, out var last) && now - last < MinRepeatSeconds) return;
            _lastPlayed[name] = now;
            var clip = Clip(name);
            if (clip != null) _oneShots.PlayOneShot(clip, MasterVolume * volume);
        }

        private bool Exists(string id) => Resolve(id) != null;

        /// <summary>
        /// Prueba id__p1__p2, luego id__p1 y luego id, cada uno con o sin "__vN". Si nada coincide, cualquier variante del id.
        /// </summary>
        private string Resolve(string id, params string[] parts)
        {
            if (_files.Count == 0 || string.IsNullOrEmpty(id)) return null;
            var candidates = new List<string>();
            for (int k = parts != null ? parts.Length : 0; k >= 0; k--)
            {
                var sb = new StringBuilder(id);
                for (int i = 0; i < k; i++)
                    if (!string.IsNullOrEmpty(parts[i])) sb.Append("__").Append(parts[i]);
                string prefix = sb.ToString();
                candidates.Clear();
                foreach (var f in _files)
                    if (f == prefix || IsVariation(f, prefix)) candidates.Add(f);
                if (candidates.Count > 0) return candidates[UnityEngine.Random.Range(0, candidates.Count)];
            }
            candidates.Clear();
            foreach (var f in _files)
                if (f.StartsWith(id + "__", StringComparison.Ordinal)) candidates.Add(f);
            return candidates.Count > 0 ? candidates[UnityEngine.Random.Range(0, candidates.Count)] : null;
        }

        private static bool IsVariation(string file, string prefix)
        {
            if (!file.StartsWith(prefix + "__v", StringComparison.Ordinal)) return false;
            for (int i = prefix.Length + 3; i < file.Length; i++)
                if (!char.IsDigit(file[i])) return false;
            return file.Length > prefix.Length + 3;
        }

        private AudioClip Clip(string name)
        {
            if (_clips.TryGetValue(name, out var cached)) return cached;
            var clip = Resources.Load<AudioClip>(Folder + name);
            if (clip == null) Debug.LogWarning("Efectos temporales: no encontré Resources/" + Folder + name);
            _clips[name] = clip;
            return clip;
        }

        // ------------------------------------------------------------------ Traducciones de datos a nombres de archivo

        private ActionKind LastAction(int actorId) => _lastAction.TryGetValue(actorId, out var k) ? k : ActionKind.Basic;

        private static string Weight(int damage, Unit target)
        {
            if (target == null || target.MaxHp <= 0) return "medio";
            double ratio = (double)damage / target.MaxHp;
            return ratio < 0.04 ? "ligero" : ratio < 0.12 ? "medio" : "pesado"; // TUNING
        }

        /// <summary>IDs de sonido de cada bruja. Los datos de la Fase 1 todavía usan zarigueya y topo (HUECOS C-26).</summary>
        private static string WitchSoundId(WitchUnit w)
        {
            switch (w.Def.Id)
            {
                case "zarigueya": return "muertita";
                case "topo": return "topacia";
                default: return w.Def.Id;
            }
        }

        private static string SpeciesGroup(WitchUnit w)
        {
            switch (WitchSoundId(w))
            {
                case "marisal":
                case "solenne":
                case "cempa": return "canida";
                case "selenia": return "alada";
                case "brea": return "felina";
                case "cervanda":
                case "salitre": return "ungulada";
                case "cupria":
                case "axa": return "escamosa";
                default: return "pequena";
            }
        }

        private static string PlanetName(Planet p)
        {
            switch (p)
            {
                case Planet.Sol: return "sol";
                case Planet.Luna: return "luna";
                case Planet.Marte: return "marte";
                case Planet.Mercurio: return "mercurio";
                case Planet.Venus: return "venus";
                case Planet.Jupiter: return "jupiter";
                default: return "saturno";
            }
        }

        private static string DotName(Planet p)
        {
            switch (p)
            {
                case Planet.Sol: return "quemadura";
                case Planet.Mercurio: return "veneno";
                case Planet.Jupiter: return "electrocucion";
                default: return "sangrado";
            }
        }

        private static string AspectName(Aspect a)
        {
            switch (a)
            {
                case Aspect.Conjuncion: return "conjuncion";
                case Aspect.Oposicion: return "oposicion";
                case Aspect.Sextil: return "sextil";
                case Aspect.Trigono: return "trigono";
                default: return "cuadratura";
            }
        }

        private static string ReactionName(HybridReaction r)
        {
            switch (r)
            {
                case HybridReaction.Eclipse: return "eclipse";
                case HybridReaction.HeladaNegra: return "helada_negra";
                case HybridReaction.EspinasRojas: return "espinas_rojas";
                default: return "tempestad";
            }
        }

        private static string RoleName(ChainRole r)
        {
            switch (r)
            {
                case ChainRole.Apertura: return "apertura";
                case ChainRole.Cierre: return "cierre";
                default: return "enlace";
            }
        }

        private static string SelfCastName(SelfCastForm f)
        {
            switch (f)
            {
                case SelfCastForm.Hilos: return "hilos";
                case SelfCastForm.FrenesiSalNegra: return "frenesi";
                default: return "aguja_sello";
            }
        }

        /// <summary>Signo sin acentos y en minúsculas: "Géminis" → "geminis".</summary>
        private static string SignName(string sign)
        {
            if (string.IsNullOrEmpty(sign)) return "";
            var normalized = sign.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark) sb.Append(char.ToLowerInvariant(ch));
            return sb.ToString();
        }

        /// <summary>Frasco y efecto de cada Salmuera del GDD 14.2, por su nombre.</summary>
        private static void PotionSounds(string potionName, out string bottle, out string effect)
        {
            string n = potionName ?? "";
            bottle = n.Contains("Leche") ? "leche_lunar" : n.Contains("Vinagre") || n.Contains("Elixir") || n.Contains("Plomo") ? "vinagre" : "agua_termal";
            if (n.Contains("Bálsamo")) effect = "cura";
            else if (n.Contains("Leche")) effect = "escudo";
            else if (n.Contains("Tónico")) effect = "velocidad";
            else if (n.Contains("Plomo") || n.Contains("Vinagre")) effect = "retraso";
            else if (n.Contains("Destilado") || n.Contains("Filtro") || n.Contains("Olvido")) effect = "salinidad";
            else effect = "anima";
        }
    }
}
