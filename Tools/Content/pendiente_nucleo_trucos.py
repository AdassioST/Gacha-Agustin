base='Assets/SalYAlmas/Core/'
def edit(path, pairs):
    p=base+path
    s=open(p,encoding='utf-8',newline='').read()
    crlf='\r\n' in s
    s=s.replace('\r\n','\n')
    for a,b in pairs:
        if a not in s: print('NO MATCH',path,a[:80])
        s=s.replace(a,b,1)
    if crlf: s=s.replace('\n','\r\n')
    open(p,'w',encoding='utf-8',newline='').write(s)

edit('Data/Definitions.cs',[
('''        Paralaje         // Fuera de combate (Agua de Olvido)
''','''        Paralaje,        // Fuera de combate (Agua de Olvido)
        Taunt,           // Turns: los enemigos la atacan a ella (Guardianas)
        MaxHpBurst,      // Value: % de la vida máxima del objetivo. Consume la intoxicación propia (Salitre)
        StealGrains,     // Value: Granos que se roba (enemigos)
'''),
('''        public bool DealsNoDamage;
''','''        public bool DealsNoDamage;
        /// <summary>Axa: al caer revive una vez con este % de vida y la Salinidad en cero.</summary>
        public double ReviveOnceHp;
        /// <summary>Salitre: se intoxica al empezar el combate. Coeficiente del veneno sobre su ATQ.</summary>
        public double StartPoison;
'''),
('''    public sealed class EnemySkillDef
''','''    /// <summary>
    /// Mecánicas que vuelven distinto a un enemigo. Se combinan; las declara enemies.json o fresno_enemies.json.
    /// Con todo en cero, el enemigo se comporta como en la Fase 1.
    /// </summary>
    public sealed class EnemyTraits
    {
        /// <summary>Devuelve este % del daño directo a quien lo golpea (Espejos).</summary>
        public double ReflectPct;
        /// <summary>Los Sin Carta: los Aspectos no le afectan (GDD 6.5).</summary>
        public bool AspectImmune;
        /// <summary>Mientras viva, sus compañeros reciben este % menos de daño (hay que matarlo primero).</summary>
        public double ProtectAllies;
        /// <summary>Cuando alguien le pega con un Hechizo, gana este % de ataque por 2 turnos.</summary>
        public double SpellEnrage;
        /// <summary>Efectos que suelta al caer. Golpean a las brujas aunque ya haya muerto.</summary>
        public readonly List<EffectDef> OnDeath = new List<EffectDef>();
        /// <summary>Forma de la silueta de prueba: cristal, bestia, ave, masa, torre, cangrejo, ojo, espejo, sombra, flor.</summary>
        public string Shape = "";
        /// <summary>Texto para la placa: qué hace y cómo se le gana.</summary>
        public string Hint = "";
    }

    public sealed class EnemySkillDef
'''),
])
s=open(base+'Data/Definitions.cs',encoding='utf-8',newline='').read()
import re
s=s.replace('public bool IsBoss;','public bool IsBoss;\n        public EnemyTraits Traits = new EnemyTraits();',1)
open(base+'Data/Definitions.cs','w',encoding='utf-8',newline='').write(s)

edit('Data/GameDatabase.cs',[
('''        public void Add(WitchDef w) {''','''        /// <summary>Copia con las mismas reglas y contenido, para sumar datos sin tocar la base de la Fase 1.</summary>
        public GameDatabase Clone()
        {
            var c = new GameDatabase(Rules);
            foreach (var id in _witchOrder) c.Add(_witches[id]);
            foreach (var e in _enemies.Values) c.Add(e);
            foreach (var id in _encounterOrder) c.Add(_encounters[id]);
            foreach (var id in _potionOrder) c.Add(_potions[id]);
            return c;
        }

        public void Add(WitchDef w) {'''),
])
edit('Data/DataLoader.cs',[
('''                    w.DealsNoDamage = r.Bool(passive, "dealsNoDamage", false);
''','''                    w.DealsNoDamage = r.Bool(passive, "dealsNoDamage", false);
                    w.ReviveOnceHp = r.Num(passive, "reviveOnce", 0, 1, required: false);
                    w.StartPoison = r.Num(passive, "startPoison", 0, 5, required: false);
'''),
('''        private static void LoadWitches(JsonNode root, GameDatabase db, Reader r)
''','''        /// <summary>Suma brujas de otro archivo a una base ya cargada (el resto del elenco, GDD 18).</summary>
        internal static void AddWitches(JsonNode root, GameDatabase db, Reader r) => LoadWitches(root, db, r);

        private static void LoadWitches(JsonNode root, GameDatabase db, Reader r)
'''),
('''{ "paralaje", EffectType.Paralaje },
''','''{ "paralaje", EffectType.Paralaje }, { "taunt", EffectType.Taunt },
            { "maxHpBurst", EffectType.MaxHpBurst }, { "stealGrains", EffectType.StealGrains },
'''),
])
# rasgos de enemigos: justo después de leer IsBoss
s=open(base+'Data/DataLoader.cs',encoding='utf-8',newline='').read()
m=re.search(r'\n([ \t]*)var (\w+) = new EnemyDef\b',s)
print('enemy var',m.group(2) if m else None)
open(base+'Data/DataLoader.cs','w',encoding='utf-8',newline='').write(s)

edit('Combat/Status.cs',[('GrainBoost, Desalada, Ecstasy, Vislumbre,','GrainBoost, Desalada, Ecstasy, Vislumbre, Taunt,')])
edit('Combat/Units.cs',[('''        public bool VislumbreUsed { get; internal set; }
''','''        public bool VislumbreUsed { get; internal set; }
        public bool ReviveUsed { get; internal set; }
''')])
edit('Combat/Battle.Effects.cs',[
('''                    case EffectType.NextToughness:
                        AddStatus(u, StatusKind.NextToughness, fx.Value, -1, name, isBuff: true, source: actor);
                        break;
''','''                    case EffectType.NextToughness:
                        AddStatus(u, StatusKind.NextToughness, fx.Value, -1, name, isBuff: true, source: actor);
                        break;
                    case EffectType.Taunt:
                        AddStatus(u, StatusKind.Taunt, 0, Math.Max(1, fx.Turns), name, isBuff: true, source: actor);
                        break;
                    case EffectType.MaxHpBurst:
                        MaxHpBurst(u, actor, fx.Value, name);
                        break;
'''),
('''                case EffectType.Paralaje:
                    return; // Solo fuera de combate (GDD 6.5).
''','''                case EffectType.Paralaje:
                    return; // Solo fuera de combate (GDD 6.5).
                case EffectType.StealGrains:
                    AddGrains(-(int)Math.Round(fx.Value), name, allowBoost: false);
                    return;
'''),
('''        /// <summary>Quita hasta count estados que cumplan la condición, empezando por el más reciente.</summary>
''','''        /// <summary>
        /// El truco de Salitre (GDD 17): convierte su intoxicación en una explosión que escala con la vida máxima del objetivo.
        /// Sin intoxicación vale la mitad. Queda topada por su ATQ para no borrar jefes de un golpe.
        /// </summary>
        private void MaxHpBurst(Unit target, Unit actor, double fraction, string name)
        {
            if (target == null || !target.IsAlive) return;
            bool intoxicated = false;
            if (actor != null)
                intoxicated = RemoveMatching(actor, s => s.Kind == StatusKind.Poison, 99, "se vuelve explosión") > 0;
            double raw = target.MaxHp * fraction * (intoxicated ? 1.0 : 0.5);
            double cap = actor != null ? Atk(actor) * 12.0 : raw; // TUNING
            int dmg = (int)Math.Round(Math.Min(raw, cap), MidpointRounding.AwayFromZero);
            int dealt = ApplyDamage(target, dmg, out _);
            Emit(new DotTicked { UnitId = target.Id, UnitName = target.Name, StatusName = name, Planet = Planet.Venus, Damage = dealt, Hp = target.Hp });
        }

        /// <summary>Quita hasta count estados que cumplan la condición, empezando por el más reciente.</summary>
'''),
])
edit('Combat/Battle.Damage.cs',[
('''            int dealt = Math.Min(remaining, target.Hp);
            target.Hp -= dealt;
            return dealt;
''','''            int dealt = Math.Min(remaining, target.Hp);
            target.Hp -= dealt;
            if (target.Hp <= 0 && target is WitchUnit w && w.Def.ReviveOnceHp > 0 && !w.ReviveUsed)
            {
                // Axa (GDD 17): revive una vez, con la Salinidad en cero.
                w.ReviveUsed = true;
                w.Hp = Math.Max(1, (int)Math.Round(w.MaxHp * w.Def.ReviveOnceHp, MidpointRounding.AwayFromZero));
                ChangeSalinity(w, -w.Salinity, "Revive");
                Emit(new UnitRevived { UnitId = w.Id, UnitName = w.Name, Hp = w.Hp });
            }
            return dealt;
'''),
])
edit('Events/BattleEvents.cs',[
('''    public sealed class UnitDefeated : BattleEvent
''','''    /// <summary>Una bruja vuelve de la caída (el truco de Axa).</summary>
    public sealed class UnitRevived : BattleEvent
    {
        public int UnitId { get; internal set; }
        public string UnitName { get; internal set; }
        public int Hp { get; internal set; }
    }

    public sealed class UnitDefeated : BattleEvent
'''),
])
edit('Events/CombatLogFormatter.cs',[
('''                case UnitDefeated e:
''','''                case UnitRevived e:
                    return $"[Regreso] {e.UnitName} vuelve del agua con {e.Hp} de vida.";
                case UnitDefeated e:
'''),
])
edit('Combat/Battle.Enemies.cs',[
('''            if (alive.Count == 0) return result;
            switch (skill.TargetRule)
            {''','''            if (alive.Count == 0) return result;
            if (skill.TargetRule != EnemyTargetRule.All && skill.TargetRule != EnemyTargetRule.Self)
            {
                // Provocación: quien la lleva atrae los golpes dirigidos.
                var taunter = alive.Find(w => w.Has(StatusKind.Taunt));
                if (taunter != null) { result.Add(taunter); return result; }
            }
            switch (skill.TargetRule)
            {'''),
])
edit('Combat/Battle.cs',[
('''        private void ApplyStartModifiers()
        {
''','''        private void ApplyStartModifiers()
        {
            foreach (var w in _witches)
                if (w.Def.StartPoison > 0) AddDot(w, DotKind.Poison, w.Def.StartPoison, 3, w, "Intoxicada"); // TUNING: 3 turnos
'''),
('''        private void AnnounceDeath(Unit u)
        {
            if (u.IsAlive || _deathAnnounced.Contains(u.Id)) return;
            _deathAnnounced.Add(u.Id);
            if (u is EnemyUnit e) e.ChargedSkill = null;
''','''        private void AnnounceDeath(Unit u)
        {
            if (u.IsAlive || _deathAnnounced.Contains(u.Id)) return;
            _deathAnnounced.Add(u.Id);
            if (u is EnemyUnit e)
            {
                e.ChargedSkill = null;
                // Lo que suelta al caer: la trampa de los que explotan, siembran o maldicen.
                foreach (var fx in e.Def.Traits.OnDeath)
                {
                    if (fx.Target == EffectTarget.Target) foreach (var w in AliveWitches()) ApplyEffect(fx, e, w, e.Name);
                    else ApplyEffect(fx, e, e, e.Name);
                }
            }
'''),
])
