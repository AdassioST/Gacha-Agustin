"""Sintetiza los sonidos de interfaz de Sal y Almas: vidrio, sal, agua, papel y campanas. Sin muestras de terceros.

Uso: python scripts/ui_sfx.py
Salida: Assets/SalYAlmas/Art/Ui/Resources/Ui/sfx/*.wav (mono, 44.1 kHz, 16 bits)
Son temporales hasta que llegue el diseño de sonido de docs/audio/INTEGRACION_AUDIO.md.
"""
import os
import wave

import numpy as np
from scipy import signal

SR = 44100
OUT = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "Assets", "SalYAlmas", "Art", "Ui", "Resources", "Ui", "sfx")


def t(d):
    return np.arange(int(d * SR)) / SR


def env(d, a=0.003, tau=0.1):
    x = t(d)
    return np.minimum(x / a, 1) * np.exp(-x / tau)


def bell(f, d=1.2, tau=0.35, partials=((1, 1), (2.01, 0.5), (2.76, 0.34), (5.4, 0.16), (8.93, 0.06))):
    x = t(d)
    out = np.zeros_like(x)
    for r, a in partials:
        out += a * np.sin(2 * np.pi * f * r * x) * np.exp(-x / (tau / (r ** 0.55)))
    return out * np.minimum(x / 0.002, 1)


def noise(d, seed=0):
    return np.random.default_rng(seed).standard_normal(int(d * SR))


def band(x, lo, hi):
    sos = signal.butter(3, [lo, hi], "band", fs=SR, output="sos")
    return signal.sosfilt(sos, x)


def low(x, f):
    return signal.sosfilt(signal.butter(3, f, "low", fs=SR, output="sos"), x)


def high(x, f):
    return signal.sosfilt(signal.butter(3, f, "high", fs=SR, output="sos"), x)


def pad(x, n):
    return np.pad(x, (0, max(0, n - len(x))))


def mix(*items):
    """items: (señal, inicio en s, ganancia)"""
    n = max(int(s * SR) + len(x) for x, s, _ in items)
    out = np.zeros(n)
    for x, s, g in items:
        i = int(s * SR)
        out[i:i + len(x)] += x * g
    return out


def reverb(x, d=1.4, wet=0.22, seed=7):
    ir = noise(d, seed) * np.exp(-t(d) / (d / 5))
    ir = low(ir, 5200)
    wetsig = signal.fftconvolve(x, ir)
    wetsig /= (np.max(np.abs(wetsig)) + 1e-9)
    out = pad(x, len(wetsig)) * (1 - wet) + wetsig * wet * np.max(np.abs(x))
    return out


def save(name, x, gain=0.85):
    x = x / (np.max(np.abs(x)) + 1e-9) * gain
    fade = int(0.01 * SR)
    x[-fade:] *= np.linspace(1, 0, fade)
    os.makedirs(OUT, exist_ok=True)
    with wave.open(os.path.join(OUT, name + ".wav"), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes((np.clip(x, -1, 1) * 32767).astype(np.int16).tobytes())
    print(f"  {name} {len(x) / SR:.2f}s")


def grit(d, seed, rate=120, f0=3500, f1=9000):
    """Granos de sal: crujidos muy cortos a tasa aleatoria."""
    rng = np.random.default_rng(seed)
    out = np.zeros(int(d * SR))
    n = int(d * rate)
    for _ in range(n):
        i = rng.integers(0, len(out) - 400)
        ln = rng.integers(40, 300)
        g = rng.standard_normal(ln) * np.exp(-np.arange(ln) / (ln / 4)) * rng.random()
        out[i:i + ln] += g
    return band(out, f0, f1)


def main():
    # Toque: un grano de sal sobre vidrio. Corto y seco; se oye cientos de veces.
    save("tick", mix((band(noise(0.03, 1), 2500, 9000) * env(0.03, 0.0005, 0.005), 0, 0.5), (bell(2350, 0.09, 0.016, ((1, 1), (2.7, 0.3))), 0, 0.35)), 0.5)
    # Confirmar: campanilla de vidrio
    save("confirm", reverb(mix((bell(1175, 0.7, 0.16), 0, 1), (bell(1760, 0.7, 0.12), 0.05, 0.6)), 0.8, 0.2), 0.62)
    # Volver: tono que baja, sordo
    save("back", reverb(mix((bell(740, 0.4, 0.08), 0, 0.8), (bell(554, 0.4, 0.09), 0.05, 0.7)), 0.5, 0.14), 0.55)
    # Negación: madera apagada
    x = t(0.13)
    save("deny", low(np.sin(2 * np.pi * 150 * x) * np.exp(-x / 0.04) + band(noise(0.13, 2), 300, 1300) * np.exp(-x / 0.02) * 0.6, 1500), 0.55)
    # Pasar la página: ráfaga de papel
    d = 0.42
    x = t(d)
    e = np.exp(-((x - 0.16) / 0.1) ** 2)
    save("page", band(noise(d, 3), 900, 7500) * e + grit(d, 4, 60, 2000, 6000) * e * 0.5, 0.55)
    # Moneda: tintineo de cristal
    save("coin", reverb(mix((bell(2637, 0.5, 0.09), 0, 1), (bell(3136, 0.5, 0.08), 0.06, 0.8), (bell(3951, 0.4, 0.06), 0.12, 0.5)), 0.6, 0.18), 0.55)
    # Sal que cae al agua: plop grave más crujido
    d = 0.8
    x = t(d)
    f = 240 + 520 * np.exp(-x / 0.05)
    plop = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-x / 0.13)
    splash = band(noise(d, 5), 900, 5200) * np.exp(-x / 0.12)
    save("drop", reverb(mix((plop, 0.0, 1), (splash, 0.01, 0.35), (grit(0.2, 6, 240, 3000, 9000), 0, 0.3)), 1.0, 0.22), 0.75)
    # Hervor: burbujas que crecen. Se dispara junto con el vapor.
    d = 2.6
    x = t(d)
    rng = np.random.default_rng(8)
    bub = np.zeros(int(d * SR))
    for _ in range(170):
        s = rng.random() ** 0.6 * (d - 0.2)
        fr = rng.uniform(280, 900)
        tt = t(rng.uniform(0.03, 0.11))
        b = np.sin(2 * np.pi * (fr + 500 * tt) * tt) * np.exp(-tt / 0.03) * rng.random()
        i = int(s * SR)
        bub[i:i + len(b)] += b
    hiss = band(noise(d, 9), 1800, 9000) * (x / d) ** 1.6 * 0.5
    save("boil", (bub + hiss) * np.minimum((d - x) / 0.2, 1), 0.6)
    # Vapor que sube: siseo filtrado que abre
    d = 1.6
    x = t(d)
    s = noise(d, 10)
    s = high(low(s, 7000), 700) * np.sin(np.pi * x / d) ** 1.4
    save("steam", s, 0.45)
    # Revelados: 3★ sobrio, 4★ acorde con cuerpo, 5★ acorde grande con coro de campanas
    save("reveal3", reverb(mix((bell(659, 0.9, 0.2), 0, 1), (bell(988, 0.9, 0.16), 0.04, 0.5)), 1.0, 0.22), 0.55)
    save("reveal4", reverb(mix((bell(523, 1.6, 0.42), 0, 1), (bell(659, 1.6, 0.38), 0.05, 0.8), (bell(784, 1.6, 0.34), 0.1, 0.7), (bell(1047, 1.6, 0.26), 0.15, 0.5)), 1.4, 0.3), 0.72)
    x = t(3.4)
    drone = (np.sin(2 * np.pi * 110 * x) + 0.5 * np.sin(2 * np.pi * 165 * x) + 0.25 * np.sin(2 * np.pi * 220.5 * x)) * np.minimum(x / 0.3, 1) * np.exp(-x / 1.3)
    up = [(bell(f, 3.0, 0.75), 0.16 * i, 0.82 - i * 0.08) for i, f in enumerate((523.3, 659.3, 784, 987.8, 1174.7, 1568))]
    sh = high(noise(3.4, 11), 5500) * np.minimum(x / 0.8, 1) * np.exp(-x / 0.95) * 0.1
    save("reveal5", reverb(mix((drone, 0, 0.42), (sh, 0, 1), *up), 2.2, 0.36), 0.88)
    # Anticipación dorada: sube el tono antes del 5★
    d = 1.3
    x = t(d)
    f = 180 * 2 ** (x / d * 2.6)
    riser = np.sin(2 * np.pi * np.cumsum(f) / SR) * (x / d) ** 2.2 + 0.4 * band(noise(d, 12), 2000, 9000) * (x / d) ** 3
    save("riser", riser, 0.5)
    # Sello: golpe de lacre, para estrellas y hitos
    x = t(0.26)
    save("stamp", mix((low(band(noise(0.26, 13), 80, 1800) * np.exp(-x / 0.03), 1800), 0, 1), (np.sin(2 * np.pi * 96 * x) * np.exp(-x / 0.06), 0, 0.9)), 0.65)
    # Estrella: chispa corta y ascendente
    save("star", reverb(mix((bell(1568, 0.5, 0.11), 0, 1), (bell(2093, 0.5, 0.09), 0.03, 0.7)), 0.5, 0.2), 0.48)
    # Entrada al juego: toque grave con campana lejana
    save("enter", reverb(mix((bell(196, 2.6, 0.8, ((1, 1), (2, 0.4), (3, 0.2))), 0, 1), (bell(784, 2.2, 0.4), 0.12, 0.5), (bell(1175, 2.2, 0.32), 0.22, 0.38)), 2.0, 0.36), 0.75)


if __name__ == "__main__":
    main()
