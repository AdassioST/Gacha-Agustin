using System;

namespace SalYAlmas.Core.Combat
{
    /// <summary>Fuente de azar inyectable. El combate solo usa esta interfaz, nunca el Random de .NET (su algoritmo puede cambiar entre runtimes).</summary>
    public interface IRandomSource
    {
        /// <summary>Número en [0, 1).</summary>
        double NextDouble();

        /// <summary>Entero en [0, maxExclusive).</summary>
        int NextInt(int maxExclusive);
    }

    /// <summary>
    /// xoshiro256** sembrado con SplitMix64. Mismo algoritmo en .NET y en Unity (Mono o IL2CPP),
    /// así que la misma semilla da la misma secuencia en todas partes.
    /// </summary>
    public sealed class SeededRandom : IRandomSource
    {
        private ulong _s0, _s1, _s2, _s3;

        public ulong Seed { get; }

        public SeededRandom(ulong seed)
        {
            Seed = seed;
            ulong x = seed;
            _s0 = SplitMix(ref x);
            _s1 = SplitMix(ref x);
            _s2 = SplitMix(ref x);
            _s3 = SplitMix(ref x);
        }

        private static ulong SplitMix(ref ulong x)
        {
            x += 0x9E3779B97F4A7C15UL;
            ulong z = x;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        private static ulong Rotl(ulong x, int k) => (x << k) | (x >> (64 - k));

        public ulong NextULong()
        {
            ulong result = Rotl(_s1 * 5, 7) * 9;
            ulong t = _s1 << 17;
            _s2 ^= _s0;
            _s3 ^= _s1;
            _s1 ^= _s2;
            _s0 ^= _s3;
            _s2 ^= t;
            _s3 = Rotl(_s3, 45);
            return result;
        }

        public double NextDouble() => (NextULong() >> 11) * (1.0 / 9007199254740992.0);

        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0) throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            // Rechazo para evitar sesgo.
            ulong bound = (ulong)maxExclusive;
            ulong limit = ulong.MaxValue - (ulong.MaxValue % bound);
            ulong v;
            do { v = NextULong(); } while (v >= limit);
            return (int)(v % bound);
        }
    }

    /// <summary>Azar fijo para pruebas: devuelve siempre el mismo valor.</summary>
    public sealed class FixedRandom : IRandomSource
    {
        private readonly double _value;
        public FixedRandom(double value) { _value = value; }
        public double NextDouble() => _value;
        public int NextInt(int maxExclusive) => 0;
    }
}
