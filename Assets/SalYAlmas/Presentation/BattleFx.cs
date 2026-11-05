using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// Ráfagas de partículas del combate: chispas al golpe, sal al deshacerse y almitas al ganar.
    /// Cada ráfaga es un ParticleSystem de un solo uso que se destruye solo. Densidad baja en Ahorro de batería.
    /// </summary>
    public static class BattleFx
    {
        /// <summary>De 0 a 1. La pone el juego según Ahorro de batería y Reducir destellos.</summary>
        public static float Density = 1f;

        private static Material _spark;
        private static Material _dot;

        private static Material Mat(string tex, ref Material cache)
        {
            if (cache != null) return cache;
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) return null;
            cache = new Material(shader) { mainTexture = Resources.Load<Texture2D>("Ui/fx/" + tex), renderQueue = 3100 };
            return cache;
        }

        /// <summary>Chispas que saltan del punto del golpe, hacia atrás del atacante.</summary>
        public static void Hit(Vector3 at, Color color, bool big)
        {
            var ps = Make("Chispas", at, Mat("sparkle", ref _spark), 0.45f);
            if (ps == null) return;
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, big ? 7f : 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, big ? 0.42f : 0.3f);
            main.startColor = new ParticleSystem.MinMaxGradient(color, Color.Lerp(color, Color.white, 0.7f));
            main.gravityModifier = 0.6f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;
            Fade(ps);
            Emit(ps, big ? 34 : 18);
        }

        /// <summary>La unidad se deshace en sal: granos blancos que caen y se apagan.</summary>
        public static void Salt(Vector3 feet, float height)
        {
            var ps = Make("Sal", feet + Vector3.up * height * 0.5f, Mat("soft_dot", ref _dot), 1.6f);
            if (ps == null) return;
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.16f);
            main.startColor = new ParticleSystem.MinMaxGradient(Palette.SaltWhite, new Color(1f, 0.9f, 0.85f));
            main.gravityModifier = 0.9f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(height * 0.45f, height, 0.3f);
            Fade(ps);
            Emit(ps, 90);
        }

        /// <summary>Almitas doradas que suben desde las brujas al ganar.</summary>
        public static void Souls(Vector3 feet)
        {
            var ps = Make("Almitas", feet + Vector3.up * 0.4f, Mat("soft_dot", ref _dot), 3f);
            if (ps == null) return;
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            main.startColor = new ParticleSystem.MinMaxGradient(Palette.GoldBright, Palette.Spark);
            main.gravityModifier = -0.15f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.8f;
            shape.rotation = new Vector3(90f, 0f, 0f);
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.5f;
            noise.frequency = 0.6f;
            Fade(ps);
            Emit(ps, 40);
        }

        private static ParticleSystem Make(string name, Vector3 at, Material mat, float life)
        {
            if (mat == null || Density <= 0f) return null;
            var go = new GameObject(name);
            go.transform.position = at;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = 0.1f;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.useUnscaledTime = true;
            main.maxParticles = 200;
            var emission = ps.emission;
            emission.enabled = false;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.renderMode = ParticleSystemRenderMode.Billboard;
            Object.Destroy(go, life + 1.5f);
            return ps;
        }

        private static void Fade(ParticleSystem ps)
        {
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.9f, 0.5f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
        }

        private static void Emit(ParticleSystem ps, int count)
        {
            ps.Play();
            ps.Emit(Mathf.Max(1, Mathf.RoundToInt(count * Mathf.Clamp01(Density))));
        }
    }
}
