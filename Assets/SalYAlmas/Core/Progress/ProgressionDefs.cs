using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Progress
{
    /// <summary>Una bruja del elenco de lanzamiento (GDD 18). Solo las que tienen kit pueden pelear en esta fase.</summary>
    public sealed class RosterEntry
    {
        public string Id;
        public string Name;
        public int Rarity;
        public string Species = "";
        public Planet Primary;
        public Planet Secondary;
        public string Role = "";
        public bool Limited;
        public bool HasKit;
    }

    public sealed class LevelRules
    {
        public int Max = 80;
        public double CostBase = 100;
        public double CostExponent = 1.5;
        public double StatGrowth = 0.05;
    }

    /// <summary>Un grado de Destilación por encima de D (GDD 12.3).</summary>
    public sealed class GradeStep
    {
        public Grade Grade;
        /// <summary>k en 20 000 · 1.8^k (0 para C).</summary>
        public int Index;
        public long Almitas;
        public int MinLevel;
        public int TrialLevel;
        /// <summary>1 Sal Destilada, 2 Quintaesencia.</summary>
        public int SaltTier;
        public int SaltAmount;
        public string Unlock = "";
        public StatBlock Mods = new StatBlock();
    }

    public enum TrialConditionType { SalinityAbove, NoAscensions, AlliesRarity, MaxCycles, NoFalls, MinChains, NoPotions, MaxTeam, MinSparks }

    /// <summary>Condición LitRPG de un Juicio (GDD 12.4).</summary>
    public sealed class TrialConditionDef
    {
        public string Id;
        public TrialConditionType Type;
        public int Value;
        public string Text = "";

        public string TextFor(string witchName) => Text.Replace("{bruja}", witchName);
    }

    public sealed class DignityRules
    {
        public readonly List<string> Names = new List<string>();
        public readonly List<long> Costs = new List<long>();
        public readonly List<double> AscendantDamage = new List<double>();
        public int Max => Names.Count - 1;
    }

    public sealed class ChainTalentRules
    {
        public int Max = 10;
        public double BonusPerLevel = 0.04;
        public double AlmitasBase = 2000;
        public double AlmitasExponent = 1.5;
        public int MaterialPerLevel = 1;
        public readonly Dictionary<Planet, string> PlanetMaterials = new Dictionary<Planet, string>();
    }

    public sealed class TrialRules
    {
        public double HpAtReference = 60000;
        public double AtkFactor = 0.85;
        public double Toughness = 160;
        public int ActionsPerTurn = 2;
        public double OwnPlanetRes = 0.2;
        public int MaxAllies = 3;
        /// <summary>Nivel al que se fija la vida de referencia (el del Guardián de la Fase 1).</summary>
        public int ReferenceLevel = 52;
    }

    public sealed class ProgressionRules
    {
        public LevelRules Level = new LevelRules();
        public double GradeCostBase = 20000;
        public double GradeCostFactor = 1.8;
        /// <summary>Pasos C, B, A, S y SS (índices 0 a 4).</summary>
        public readonly List<GradeStep> Grades = new List<GradeStep>();
        public int ViaFreeChanges = 1;
        public long ViaChangeCost = 30000;
        public DignityRules Dignity = new DignityRules();
        public ChainTalentRules ChainTalents = new ChainTalentRules();
        public TrialRules Trial = new TrialRules();
        public readonly Dictionary<string, TrialConditionDef> Conditions = new Dictionary<string, TrialConditionDef>();
        /// <summary>Condiciones por bruja y grado; "default" para las demás.</summary>
        public readonly Dictionary<string, Dictionary<Grade, List<string>>> TrialConditions = new Dictionary<string, Dictionary<Grade, List<string>>>();
        public readonly Dictionary<string, Dictionary<Grade, string>> Memories = new Dictionary<string, Dictionary<Grade, string>>();

        public GradeStep StepTo(Grade g)
        {
            foreach (var s in Grades) if (s.Grade == g) return s;
            return null;
        }

        /// <summary>Costo(n) = 100 · n^1.5: almitas para subir del nivel n − 1 al n (GDD 11.3).</summary>
        public long LevelCost(int n) => n <= 1 ? 0 : (long)Math.Round(Level.CostBase * Math.Pow(n, Level.CostExponent), MidpointRounding.AwayFromZero);

        /// <summary>Almitas totales para ir de un nivel a otro.</summary>
        public long LevelCostRange(int from, int to)
        {
            long total = 0;
            for (int n = from + 1; n <= to; n++) total += LevelCost(n);
            return total;
        }

        /// <summary>Factor de ATQ y VIDA de un nivel, relativo al nivel 1.</summary>
        public double LevelFactor(int level) => 1.0 + Level.StatGrowth * (Math.Max(1, level) - 1);

        /// <summary>20 000 · 1.8^k almitas (GDD 12.3).</summary>
        public long GradeCost(int k) => (long)Math.Round(GradeCostBase * Math.Pow(GradeCostFactor, k), MidpointRounding.AwayFromZero);

        public List<TrialConditionDef> ConditionsFor(string witchId, Grade grade)
        {
            var ids = Lookup(witchId, grade) ?? Lookup("default", grade) ?? new List<string>();
            var list = new List<TrialConditionDef>();
            foreach (var id in ids) if (Conditions.TryGetValue(id, out var c)) list.Add(c);
            return list;
        }

        private List<string> Lookup(string key, Grade grade) =>
            TrialConditions.TryGetValue(key, out var byGrade) && byGrade.TryGetValue(grade, out var ids) ? ids : null;

        public string MemoryFor(string witchId, Grade grade) =>
            Memories.TryGetValue(witchId, out var m) && m.TryGetValue(grade, out var text) ? text : "Un recuerdo sin escribir (Fase 3).";

        public long ChainTalentCost(int fromLevel) => (long)Math.Round(ChainTalents.AlmitasBase * Math.Pow(fromLevel, ChainTalents.AlmitasExponent), MidpointRounding.AwayFromZero);
    }
}
