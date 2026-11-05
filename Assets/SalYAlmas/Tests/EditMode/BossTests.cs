using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Events;

namespace SalYAlmas.Tests
{
    /// <summary>Comprobaciones de balance para la prueba con jugadores: el jefe se puede vencer y el orden importa.</summary>
    public class BossTests
    {
        private static Battle Play(ulong seed, System.Action<AutoPilot> configure = null)
        {
            var setup = new BattleSetup
            {
                Team = { "marisal", "selenia", "solenne", "tormentina" },
                GuestId = "zarigueya",
                EncounterId = "guardian",
                Seed = seed,
                Belt = { new BeltSlot("balsamo_cuprico", 3), new BeltSlot("leche_luna", 2), new BeltSlot("destilado_rocio", 2) },
            };
            var b = new Battle(TestKit.RealDb(), setup);
            b.Start();
            var pilot = new AutoPilot();
            configure?.Invoke(pilot);
            pilot.RunToEnd(b);
            return b;
        }

        private static int DamageTaken(Battle b) =>
            b.Events.OfType<HitLanded>().Where(h => h.TargetId < Battle.FirstEnemyId).Sum(h => h.Damage);

        [Test]
        public void ElGuardianSeVenceCompleto()
        {
            for (ulong seed = 1; seed <= 10; seed++)
            {
                var b = Play(seed);
                Assert.AreEqual(BattleOutcome.Victory, b.Outcome, "semilla " + seed);
                int allyTurns = b.Events.OfType<TurnStarted>().Count(t => t.Side == Side.Ally);
                Assert.Less(allyTurns, 75, "el combate no se alarga de más");
                Assert.IsTrue(b.Events.OfType<EnemyCharging>().Any(), "el jugador ve el ataque definitivo telegrafiado");
                Assert.IsTrue(b.Events.OfType<PhaseChanged>().Any(), "llega a la segunda fase");
            }
        }

        [Test]
        public void ElOrdenDeLaCadenaImporta()
        {
            var best = new List<int>();
            var worst = new List<int>();
            for (ulong seed = 1; seed <= 10; seed++)
            {
                best.Add(DamageTaken(Play(seed)));
                worst.Add(DamageTaken(Play(seed, p => p.WorstChainOrder = true)));
            }
            Assert.Greater(worst.Average(), best.Average() * 1.5, "con el peor orden el aquelarre recibe mucho más daño");
        }

        [Test]
        public void SinCadenasNiAscensionesElJefeEsUnaAmenaza()
        {
            var b = Play(1, p => { p.UseChains = false; p.UseAscensions = false; });
            Assert.AreEqual(BattleOutcome.Defeat, b.Outcome, "el jefe castiga ignorar la Cadena y las Ascensiones");
        }
    }
}
