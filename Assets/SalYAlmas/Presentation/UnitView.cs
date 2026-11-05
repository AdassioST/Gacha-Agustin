using System.Collections.Generic;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    public struct FloatingText
    {
        public string Text;
        public Color Color;
        public int Size;
        public float Age;
        public Vector3 World;
    }

    /// <summary>
    /// Figura de una unidad: un recorte con arte y sombra de contacto o, si falta el arte, la cápsula de prueba de
    /// la Fase 1 con la esfera de su Ascendente. Solo anima; el estado real vive en el núcleo.
    /// </summary>
    public sealed class UnitView : MonoBehaviour
    {
        public int UnitId;
        public bool IsEnemy;
        public Renderer Body;
        public Transform Ascendant;
        public float Height = 2f;
        /// <summary>Sombra de contacto en el suelo, si la figura es un recorte.</summary>
        public Renderer Shadow;
        /// <summary>Recorte con arte: el destello aclara en vez de teñir.</summary>
        public bool IsArt;

        public readonly List<FloatingText> Floats = new List<FloatingText>();

        private Color _baseColor;
        private Color _flashColor;
        private float _flash;
        private float _shake;
        private float _lunge;
        private bool _dead;
        private float _deadTime;
        private bool _active;
        private bool _broken;
        private bool _charging;
        private bool _ecstasy;
        private float _glow;
        private Vector3 _home;
        private Vector3 _baseScale;
        private MaterialPropertyBlock _block;

        public bool IsDead => _dead;
        public Vector3 Head => transform.position + Vector3.up * Height;

        public void Setup(int unitId, bool enemy, Color color, Renderer body, Transform ascendant, float height)
        {
            UnitId = unitId;
            IsEnemy = enemy;
            Body = body;
            Ascendant = ascendant;
            Height = height;
            _baseColor = color;
            _home = transform.position;
            _baseScale = transform.localScale;
            _block = new MaterialPropertyBlock();
            ApplyColor(color);
        }

        private void ApplyColor(Color c)
        {
            if (Body == null) return;
            Body.GetPropertyBlock(_block);
            _block.SetColor("_BaseColor", c); // URP Lit
            _block.SetColor("_Color", c);     // Built-in y otros
            Body.SetPropertyBlock(_block);
        }

        public void Flash(Color c, float seconds = 0.25f)
        {
            _flashColor = IsArt ? new Color(c.r * 1.9f, c.g * 1.9f, c.b * 1.9f, 1f) : c;
            _flash = seconds;
            if (IsArt) BattleFx.Hit(transform.position + Vector3.up * Height * 0.55f, c == Color.white ? Palette.GoldBright : c, seconds > 0.2f);
        }

        public void Shake(float seconds = 0.2f) => _shake = Mathf.Max(_shake, seconds);
        public void Lunge() => _lunge = 0.35f;
        public void Glow(float seconds = 1f) => _glow = seconds;
        public void SetActive(bool active) => _active = active;
        public void SetBroken(bool broken) => _broken = broken;
        public void SetCharging(bool charging) => _charging = charging;
        public void SetEcstasy(bool ecstasy) => _ecstasy = ecstasy;

        public void Die()
        {
            if (_dead) return;
            _dead = true;
            _deadTime = 0f;
            if (IsArt) BattleFx.Salt(_home, Height);
        }

        public void AddFloat(string text, Color color, int size = 30)
        {
            // Escalona los textos que salen juntos para que no se encimen.
            float offset = 0.35f * (Floats.Count % 4);
            Floats.Add(new FloatingText { Text = text, Color = color, Size = size, Age = 0, World = Head + Vector3.up * offset });
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            for (int i = Floats.Count - 1; i >= 0; i--)
            {
                var f = Floats[i];
                f.Age += dt;
                f.World += Vector3.up * dt * 0.8f;
                if (f.Age > 1.4f) Floats.RemoveAt(i);
                else Floats[i] = f;
            }

            Vector3 pos = _home;
            if (_shake > 0)
            {
                _shake -= dt;
                pos += new Vector3(Mathf.Sin(Time.unscaledTime * 90f), 0, Mathf.Cos(Time.unscaledTime * 70f)) * 0.08f;
            }
            if (_lunge > 0)
            {
                _lunge -= dt;
                float t = Mathf.Sin(Mathf.Clamp01(_lunge / 0.35f) * Mathf.PI);
                // Vista lateral: las brujas embisten hacia la derecha y los enemigos hacia la izquierda.
                pos += (IsEnemy ? Vector3.left : Vector3.right) * t * 0.9f;
            }
            if (_active) pos += Vector3.up * (0.12f + Mathf.Sin(Time.unscaledTime * 4f) * 0.05f);
            transform.position = pos;

            if (_dead)
            {
                _deadTime += dt;
                transform.localScale = Vector3.Lerp(_baseScale, new Vector3(_baseScale.x * 1.3f, 0.05f, _baseScale.z * 1.3f), Mathf.Clamp01(_deadTime / 0.6f));
                if (Shadow != null) Shadow.enabled = _deadTime < 0.6f;
            }
            else if (_charging)
            {
                float s = 1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.06f;
                transform.localScale = _baseScale * s;
            }
            else transform.localScale = _baseScale;

            Color c = _baseColor;
            if (_dead) c = Color.Lerp(_baseColor, Palette.SaltWhite, 0.8f) * new Color(1, 1, 1, IsArt ? 1f - Mathf.Clamp01((_deadTime - 0.3f) / 0.5f) : 1f);
            else if (_broken) c = Color.Lerp(_baseColor, Palette.SaltWhite, 0.35f + 0.15f * Mathf.Sin(Time.unscaledTime * 10f));
            else if (_ecstasy) c = Color.Lerp(_baseColor, Palette.Band(Core.Magic.SalinityBand.ExtasisAbisal), 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f));
            if (_flash > 0)
            {
                _flash -= dt;
                c = Color.Lerp(c, _flashColor, Mathf.Clamp01(_flash * 5f));
            }
            ApplyColor(c);

            if (Ascendant != null)
            {
                if (_glow > 0) _glow -= dt;
                float bob = Mathf.Sin(Time.unscaledTime * 2f + UnitId) * 0.15f;
                Ascendant.localPosition = new Vector3(-0.6f, 1.6f + bob + (_glow > 0 ? 0.6f : 0f), -0.7f);
                float scale = _glow > 0 ? 0.9f : 0.45f;
                Ascendant.localScale = Vector3.one * scale;
                Ascendant.gameObject.SetActive(!_dead);
            }
        }
    }
}
