# -*- coding: utf-8 -*-
"""Bloques de síntesis de Sal y Almas.

Todo lo que suena en el juego sale de estas piezas: metales por planeta, sal,
papel, pluma, cuerdas, cuencos, agua y vapor, cuerpos, viento, susurros sin
palabras y drones. Cada función recibe un `Ctx` con su semilla, así que el
mismo archivo siempre sale idéntico.

Convenciones:
- Frecuencia de muestreo fija de 48 kHz. Las señales son `numpy` en float64.
- Mono es un arreglo 1D; estéreo es (n, 2).
- Los tiempos van en segundos y las ganancias en dB.
"""
import functools
import math
import os
import zlib
from fractions import Fraction

import numpy as np
from scipy import signal

SR = 48000
DIR = os.path.dirname(os.path.abspath(__file__))
FUENTES_DIR = os.path.join(DIR, "fuentes")

# ---------------------------------------------------------------------------
# Utilidades básicas
# ---------------------------------------------------------------------------


def n_(seg):
    return max(0, int(round(seg * SR)))


def tiempo(dur):
    return np.arange(n_(dur)) / SR


def db(x):
    return 10.0 ** (x / 20.0)


def silencio(dur):
    return np.zeros(n_(dur))


class Ctx:
    """Azar con semilla fija. Un Ctx por archivo; los hijos se derivan por etiqueta."""

    def __init__(self, semilla):
        self.semilla = int(semilla) & 0xFFFFFFFF
        self.rng = np.random.default_rng(self.semilla)

    def u(self, a=0.0, b=1.0):
        return float(self.rng.uniform(a, b))

    def i(self, a, b):
        """Entero en [a, b]."""
        return int(self.rng.integers(a, b + 1))

    def elegir(self, lista):
        return lista[int(self.rng.integers(0, len(lista)))]

    def normal(self, size):
        return self.rng.standard_normal(size)

    def hijo(self, etiqueta):
        return Ctx(zlib.crc32(f"{self.semilla}:{etiqueta}".encode("utf-8")))


def ajustar(x, n):
    """Corta o rellena con ceros hasta n muestras."""
    if len(x) >= n:
        return x[:n]
    pad = np.zeros((n - len(x),) + x.shape[1:])
    return np.concatenate([x, pad])


def a_mono(x):
    return x.mean(axis=1) if x.ndim == 2 else x


def paneo(pan):
    """Paneo de potencia constante, pan en [-1, 1]."""
    ang = (pan + 1.0) * math.pi / 4.0
    return math.cos(ang), math.sin(ang)


class Lienzo:
    """Mezcla capas en el tiempo. Crece solo si una capa se sale del final."""

    def __init__(self, dur=0.0, estereo=False):
        self.estereo = estereo
        self.x = np.zeros((n_(dur), 2)) if estereo else np.zeros(n_(dur))

    def add(self, sig, t=0.0, gdb=0.0, pan=0.0):
        sig = np.asarray(sig, dtype=float)
        if self.estereo and sig.ndim == 1:
            l, r = paneo(pan)
            sig = np.stack([sig * l, sig * r], axis=1)
        elif not self.estereo and sig.ndim == 2:
            sig = sig.mean(axis=1)
        i = n_(t)
        fin = i + len(sig)
        if fin > len(self.x):
            self.x = ajustar(self.x, fin)
        self.x[i:fin] += db(gdb) * sig
        return self

    def senal(self):
        return self.x


def normalizar(x, pico=1.0):
    m = np.max(np.abs(x)) if len(x) else 0.0
    return x * (pico / m) if m > 0 else x


def rms(x):
    return float(np.sqrt(np.mean(np.square(x)))) if len(x) else 0.0


# ---------------------------------------------------------------------------
# Envolventes
# ---------------------------------------------------------------------------


def env_puntos(dur, puntos):
    """Envolvente lineal por puntos [(t, v), ...]."""
    t = tiempo(dur)
    ts = [p[0] for p in puntos]
    vs = [p[1] for p in puntos]
    return np.interp(t, ts, vs)


def env_exp(dur, t60, ataque=0.001):
    """Ataque lineal y caída exponencial (t60 = tiempo en caer 60 dB)."""
    t = tiempo(dur)
    e = np.exp(-6.9078 * t / max(t60, 1e-4))
    na = n_(ataque)
    if na > 1:
        e[:na] *= np.linspace(0.0, 1.0, na)
    return e


def env_campana(dur, pico=0.5, forma=2.0):
    """Envolvente que sube y baja (un 'swell'): pico en fracción `pico` de la duración."""
    t = np.linspace(0.0, 1.0, n_(dur))
    e = np.where(t < pico, (t / pico) ** forma, ((1 - t) / (1 - pico)) ** forma)
    return np.clip(e, 0.0, 1.0)


def fundido(x, ini=0.002, fin=0.02):
    """Fundidos de coseno al inicio y al final."""
    x = np.array(x, dtype=float, copy=True)
    ni, nf = min(n_(ini), len(x)), min(n_(fin), len(x))
    if ni > 1:
        w = 0.5 - 0.5 * np.cos(np.linspace(0, math.pi, ni))
        x[:ni] = (x[:ni].T * w).T
    if nf > 1:
        w = 0.5 + 0.5 * np.cos(np.linspace(0, math.pi, nf))
        x[-nf:] = (x[-nf:].T * w).T
    return x


# ---------------------------------------------------------------------------
# Filtros
# ---------------------------------------------------------------------------


@functools.lru_cache(maxsize=1024)
def _sos(tipo, f1, f2, orden):
    nyq = SR / 2.0
    if tipo in ("lowpass", "highpass"):
        w = min(max(f1, 5.0), nyq * 0.98)
        return signal.butter(orden, w, btype=tipo, fs=SR, output="sos")
    lo = min(max(f1, 5.0), nyq * 0.97)
    hi = min(max(f2, lo * 1.01), nyq * 0.98)
    return signal.butter(orden, [lo, hi], btype=tipo, fs=SR, output="sos")


def lp(x, fc, orden=2):
    return signal.sosfilt(_sos("lowpass", float(fc), None, orden), x, axis=0)


def hp(x, fc, orden=2):
    return signal.sosfilt(_sos("highpass", float(fc), None, orden), x, axis=0)


def bp(x, f1, f2, orden=2):
    return signal.sosfilt(_sos("bandpass", float(f1), float(f2), orden), x, axis=0)


def resonador(x, f, q):
    f = min(f, SR * 0.45)
    b, a = signal.iirpeak(f, q, fs=SR)
    return signal.lfilter(b, a, x, axis=0)


def peq(x, f, q, gdb):
    """Ecualizador de pico (RBJ)."""
    A = 10 ** (gdb / 40.0)
    w0 = 2 * math.pi * min(f, SR * 0.45) / SR
    alpha = math.sin(w0) / (2 * q)
    b = [1 + alpha * A, -2 * math.cos(w0), 1 - alpha * A]
    a = [1 + alpha / A, -2 * math.cos(w0), 1 - alpha / A]
    return signal.lfilter(np.array(b) / a[0], np.array(a) / a[0], x, axis=0)


def filtro_variable(x, ganancia, nfft=1024, hop=256):
    """Filtro que cambia en el tiempo: ganancia(f[:,None], t[None,:]) -> matriz de ganancias."""
    if len(x) < nfft:
        x2 = ajustar(x, nfft)
    else:
        x2 = x
    f, t, Z = signal.stft(x2, SR, window="hann", nperseg=nfft, noverlap=nfft - hop)
    G = ganancia(f[:, None], t[None, :])
    _, y = signal.istft(Z * G, SR, window="hann", nperseg=nfft, noverlap=nfft - hop)
    return ajustar(y, len(x))


def banda_movil(x, fc, q=2.0, piso=0.0):
    """Paso banda gaussiano (en octavas) cuyo centro sigue la función fc(t)."""
    def g(f, t):
        c = np.maximum(fc(t), 20.0)
        oct_ = np.log2(np.maximum(f, 1.0) / c)
        return piso + (1 - piso) * np.exp(-0.5 * (oct_ * q) ** 2)
    return filtro_variable(x, g)


def formantes(x, f_t, anchos, ganancias):
    """Banco de formantes que se mueven: f_t(t) -> lista de centros por formante."""
    def g(f, t):
        centros = f_t(t)
        acc = np.zeros(np.broadcast(f, t).shape)
        for c, bw, gg in zip(centros, anchos, ganancias):
            acc += gg * np.exp(-0.5 * ((f - c) / bw) ** 2)
        return acc
    return filtro_variable(x, g)


# ---------------------------------------------------------------------------
# Ruido
# ---------------------------------------------------------------------------


def blanco(dur, ctx):
    return ctx.normal(n_(dur))


def ruido_color(dur, ctx, pendiente=-3.0):
    """Ruido con pendiente en dB por octava (-3 rosa, -6 marrón)."""
    n = n_(dur)
    if n == 0:
        return np.zeros(0)
    X = np.fft.rfft(ctx.normal(n))
    f = np.fft.rfftfreq(n, 1 / SR)
    f[0] = f[1] if n > 1 else 1.0
    X *= (f / 1000.0) ** (pendiente / 6.0206)
    y = np.fft.irfft(X, n)
    return y / (np.std(y) + 1e-12)


def rosa(dur, ctx):
    return ruido_color(dur, ctx, -3.0)


def marron(dur, ctx):
    return ruido_color(dur, ctx, -6.0)


def am_aleatoria(dur, ctx, f_lo, f_hi, profundidad=1.0):
    """Modulación de amplitud lenta y suave (ruido filtrado), en [1-prof, 1]."""
    n = n_(dur)
    r = lp(ctx.normal(n + 4800), f_hi, 2)[4800:]
    if f_lo > 0:
        r = hp(r, f_lo, 1)
    r = r / (np.max(np.abs(r)) + 1e-12)
    return 1.0 - profundidad * 0.5 * (1 - r)


# ---------------------------------------------------------------------------
# Síntesis modal (campanas, metales, cuencos, madera)
# ---------------------------------------------------------------------------


def modal(dur, freqs, amps, t60s, ctx, ataque=0.0005, glide=None, fases=None):
    """Suma de senoidales con caída exponencial.

    glide: arreglo de multiplicadores de frecuencia en el tiempo (mismo largo) o None.
    """
    t = tiempo(dur)
    out = np.zeros_like(t)
    if glide is not None:
        fase_base = 2 * np.pi * np.cumsum(glide) / SR
    for k, (f, a, T) in enumerate(zip(freqs, amps, t60s)):
        if f <= 0 or f >= SR * 0.46 or a == 0:
            continue
        ph = ctx.u(0, 2 * np.pi) if fases is None else fases[k]
        dec = np.exp(-6.9078 * t / max(T, 1e-4))
        if glide is None:
            out += a * dec * np.sin(2 * np.pi * f * t + ph)
        else:
            out += a * dec * np.sin(f * fase_base + ph)
    na = n_(ataque)
    if na > 1:
        out[:na] *= np.linspace(0, 1, na) ** 1.5
    return out


def chasquido(ctx, dur=0.004, f_lo=1500, f_hi=12000):
    """Transitorio de golpe: ráfaga corta de ruido filtrado."""
    x = blanco(dur + 0.002, ctx) * env_exp(dur + 0.002, dur, 0.0002)
    return bp(x, f_lo, f_hi, 2)


# Perfiles de metal: proporciones de parciales, amplitudes y t60 relativos.
PERFILES = {
    # Oro: campanilla de latón brillante y crótalos.
    "oro": dict(r=[1.0, 2.0, 2.76, 3.01, 4.07, 5.2, 6.43], a=[1.0, 0.5, 0.33, 0.2, 0.17, 0.09, 0.05],
                t=[1.0, 0.68, 0.48, 0.42, 0.3, 0.22, 0.15], golpe=(2500, 14000), golpe_db=-14),
    # Plata: cristal y crótalo suave, casi senoidal, cola larga.
    "plata": dict(r=[1.0, 2.0, 3.0, 4.16], a=[1.0, 0.16, 0.05, 0.03],
                  t=[1.0, 0.5, 0.3, 0.18], golpe=(3000, 12000), golpe_db=-26),
    # Hierro: yunque seco, parciales de barra libre, caída corta.
    "hierro": dict(r=[1.0, 2.31, 2.76, 3.91, 5.40, 6.92, 8.93], a=[1.0, 0.3, 0.55, 0.28, 0.32, 0.14, 0.12],
                   t=[0.45, 0.3, 0.28, 0.2, 0.15, 0.1, 0.07], golpe=(1200, 9000), golpe_db=-6),
    # Azogue: vidrio y gota, puro e inestable.
    "azogue": dict(r=[1.0, 2.0, 3.0], a=[1.0, 0.12, 0.04], t=[0.6, 0.28, 0.15],
                   golpe=(4000, 14000), golpe_db=-28),
    # Cobre: cuenco cantor, parciales en dobletes que laten.
    "cobre": dict(r=[1.0, 2.71, 5.15, 8.17], a=[1.0, 0.5, 0.22, 0.08], t=[1.0, 0.6, 0.33, 0.2],
                  golpe=(800, 5000), golpe_db=-24),
    # Estaño: gong medio, denso, con el crujido del "grito del estaño".
    "estano": dict(r=[1.0, 1.47, 2.0, 2.53, 3.12, 3.9, 4.6], a=[1.0, 0.55, 0.5, 0.42, 0.28, 0.18, 0.1],
                   t=[1.0, 0.78, 0.66, 0.52, 0.4, 0.3, 0.2], golpe=(1500, 9000), golpe_db=-16),
    # Plomo: sordo, casi no resuena.
    "plomo": dict(r=[1.0, 2.0, 2.92, 4.1], a=[1.0, 0.22, 0.09, 0.04], t=[0.32, 0.16, 0.1, 0.06],
                  golpe=(200, 2500), golpe_db=-10),
}

# Planeta -> metal, grado de la escala (Partitura, 3.3) y secta.
PLANETAS = {
    "luna": dict(metal="plata", grado=0),
    "mercurio": dict(metal="azogue", grado=1),
    "venus": dict(metal="cobre", grado=2),
    "sol": dict(metal="oro", grado=3),
    "marte": dict(metal="hierro", grado=4),
    "jupiter": dict(metal="estano", grado=5),
    "saturno": dict(metal="plomo", grado=6),
}
ORDEN_PLANETAS = ["sol", "luna", "marte", "mercurio", "venus", "jupiter", "saturno"]


def metal(nombre, f0, dur, ctx, t60=1.5, brillo=1.0, golpe=1.0, batido=0.0, ataque=0.0006):
    """Golpe de un metal planetario afinado a f0.

    t60: caída del parcial fundamental en segundos (los demás escalan con el perfil).
    brillo: >1 refuerza parciales altos, <1 los apaga.
    batido: Hz de desafinación para dobletes (cuencos, cristal).
    """
    p = PERFILES[nombre]
    fr, am, tt = [], [], []
    for k, (r, a, tr) in enumerate(zip(p["r"], p["a"], p["t"])):
        f = f0 * r * (1 + ctx.u(-0.002, 0.002) * (k > 0))
        a2 = a * (brillo ** min(k, 4)) if k else a
        fr.append(f)
        am.append(a2)
        tt.append(t60 * tr)
        if batido > 0:
            fr.append(f + batido * (1 + 0.6 * k) * ctx.u(0.7, 1.3))
            am.append(a2 * 0.8)
            tt.append(t60 * tr * 0.95)
    x = modal(dur, fr, am, tt, ctx, ataque=ataque)
    x /= max(np.max(np.abs(x)), 1e-9)
    if golpe > 0:
        g_lo, g_hi = p["golpe"]
        c = chasquido(ctx.hijo("golpe"), 0.003, g_lo, g_hi)
        x[: len(c)] += golpe * db(p["golpe_db"]) * c / (np.max(np.abs(c)) + 1e-9) * 3
    if nombre == "plomo":
        x = lp(x, 1800, 2)
    if nombre == "estano":
        cr = grito_estano(min(dur, 0.25), ctx.hijo("grito"), densidad=900)
        x = Lienzo(dur).add(x).add(cr, 0.0, -20).senal()
    return x


# ---------------------------------------------------------------------------
# Notas y escalas (la casa es Re)
# ---------------------------------------------------------------------------

_NOTA = {"C": 0, "C#": 1, "Db": 1, "D": 2, "D#": 3, "Eb": 3, "E": 4, "F": 5, "F#": 6, "Gb": 6,
         "G": 7, "G#": 8, "Ab": 8, "A": 9, "A#": 10, "Bb": 10, "B": 11}


def midi(nombre):
    """'D5' -> 74; 'Bb4' -> 70."""
    letra = nombre[:-1]
    octava = int(nombre[-1])
    return 12 * (octava + 1) + _NOTA[letra]


def hz(nombre_o_midi):
    m = midi(nombre_o_midi) if isinstance(nombre_o_midi, str) else nombre_o_midi
    return 440.0 * 2 ** ((m - 69) / 12.0)


RE_MENOR = [0, 2, 3, 5, 7, 8, 10]   # Re eolio: Re Mi Fa Sol La Si♭ Do
RE_MAYOR = [0, 2, 4, 5, 7, 9, 11]


def grado(g, octava_re=5, escala=RE_MENOR):
    """Frecuencia del grado g (0 = Re) arriba del Re de la octava dada. g puede pasar de 6."""
    o, k = divmod(g, 7)
    return hz(midi(f"D{octava_re}") + 12 * o + escala[k])


# ---------------------------------------------------------------------------
# Instrumentos
# ---------------------------------------------------------------------------


def cuerda(f0, dur, ctx, tipo="salterio", fuerza=1.0, pos=None):
    """Cuerda pulsada por modos: salterio (acero, brillante), laud o guitarra (nailon)."""
    cfg = {
        "salterio": dict(B=0.00025, k=0.9, T=2.6, alto=0.012, pos=0.12, coro=1.6),
        "laud": dict(B=0.00008, k=1.5, T=1.4, alto=0.03, pos=0.18, coro=1.0),
        "guitarra": dict(B=0.00004, k=1.8, T=1.8, alto=0.04, pos=0.2, coro=0.0),
    }[tipo]
    p = pos if pos is not None else cfg["pos"] * ctx.u(0.85, 1.15)
    fr, am, tt = [], [], []
    for nn in range(1, 40):
        f = nn * f0 * math.sqrt(1 + cfg["B"] * nn * nn)
        if f > 16000:
            break
        a = abs(math.sin(nn * math.pi * p)) / nn ** cfg["k"]
        T = cfg["T"] / (1 + cfg["alto"] * nn * nn)
        fr.append(f)
        am.append(a)
        tt.append(T)
    x = modal(dur, fr, am, tt, ctx, ataque=0.0008)
    if cfg["coro"] > 0:  # segunda cuerda del orden, desafinada unos cents
        r = 2 ** (cfg["coro"] * ctx.u(0.6, 1.0) / 1200)
        x = x + 0.8 * modal(dur, [f * r for f in fr], am, tt, ctx.hijo("coro"), ataque=0.0008)
    x /= max(np.max(np.abs(x)), 1e-9)
    pl = chasquido(ctx.hijo("pua"), 0.002, 800 if tipo != "salterio" else 2000, 9000)
    x[: len(pl)] += 0.15 * fuerza * pl / (np.max(np.abs(pl)) + 1e-9)
    if tipo == "guitarra":
        x = peq(x, 110, 1.5, 4)
        x = peq(x, 220, 2.0, 3)
        x = lp(x, 5500, 1)
    if tipo == "laud":
        x = peq(x, 180, 1.5, 3)
        x = lp(x, 7000, 1)
    return x * fuerza


def rasgueo(notas_hz, dur, ctx, tipo="guitarra", separacion=0.018, fuerza=1.0):
    """Rasgueo: cuerdas una tras otra con unos milisegundos de separación."""
    lz = Lienzo(dur)
    for k, f in enumerate(notas_hz):
        t0 = k * separacion * ctx.u(0.8, 1.2)
        lz.add(cuerda(f, dur - t0, ctx.hijo(f"c{k}"), tipo, fuerza * ctx.u(0.8, 1.0)), t0)
    return lz.senal()


def arco(f0, dur, ctx, brillo=1.0, vibrato=0.006, ataque=0.25, formantes_hz=(260, 620, 1250, 2900)):
    """Cuerda frotada (violonchelo grave): armónicos con cuerpo, vibrato tardío y ruido de arco."""
    t = tiempo(dur)
    vib = 1 + vibrato * np.sin(2 * np.pi * 5.2 * t + ctx.u(0, 6)) * np.clip((t - 0.35) / 0.4, 0, 1)
    fase = 2 * np.pi * f0 * np.cumsum(vib) / SR
    x = np.zeros_like(t)
    for nn in range(1, 36):
        if nn * f0 > 12000:
            break
        x += (1.0 / nn ** (1.15 / brillo)) * np.sin(nn * fase + ctx.u(0, 6))
    for f in formantes_hz:
        x = x + 0.5 * resonador(x, f, 3.0)
    ruido = bp(blanco(dur, ctx), 1500, 6000) * 0.02
    x = x / (np.max(np.abs(x)) + 1e-9) + ruido
    env = env_puntos(dur, [(0, 0), (ataque, 1.0), (dur * 0.7, 0.85), (dur, 0.0)])
    return lp(x * env, 7000, 1)


def cuenco(f0, dur, ctx, t60=6.0, frotado=False, batido=1.2, gemido=0.0):
    """Cuenco de cobre: golpeado con mazo suave o frotado (ataque lento).

    gemido: 0..1, el cuenco frotado 'hasta que gime' (Sal Negra): roce áspero y altura inestable.
    """
    if not frotado:
        x = metal("cobre", f0, dur, ctx, t60=t60, batido=batido, golpe=0.6, ataque=0.002)
        return lp(x, 9000, 1)
    t = tiempo(dur)
    p = PERFILES["cobre"]
    x = np.zeros_like(t)
    wob = 1 + gemido * 0.01 * np.sin(2 * np.pi * 0.7 * t) * am_aleatoria(dur, ctx, 0, 3, 1)
    fase = 2 * np.pi * np.cumsum(wob) / SR
    for k, (r, a) in enumerate(zip(p["r"], p["a"])):
        for dt in (0.0, batido * (1 + k)):
            x += a * np.sin((f0 * r + dt) * fase + ctx.u(0, 6))
    roce = bp(blanco(dur, ctx), f0 * 0.8, f0 * 6) * (0.05 + 0.4 * gemido)
    roce *= am_aleatoria(dur, ctx.hijo("roce"), 0.5, 18 + 30 * gemido, 0.8)
    x = x / (np.max(np.abs(x)) + 1e-9) + roce
    env = env_puntos(dur, [(0, 0), (dur * 0.45, 1.0), (dur * 0.8, 0.8), (dur, 0)])
    return x * env


def copa_frotada(f0, dur, ctx, batido=0.8):
    """Copa de cristal frotada: casi senoidal, ataque lento y latido suave."""
    t = tiempo(dur)
    x = (np.sin(2 * np.pi * f0 * t) + 0.9 * np.sin(2 * np.pi * (f0 + batido) * t + 1.0)
         + 0.08 * np.sin(2 * np.pi * 2 * f0 * t) + 0.03 * np.sin(2 * np.pi * 3 * f0 * t))
    x *= am_aleatoria(dur, ctx, 0, 2.5, 0.25)
    env = env_puntos(dur, [(0, 0), (min(0.35, dur * 0.4), 1.0), (dur * 0.75, 0.8), (dur, 0)])
    return x * env / 2.0


def campana_spark(f0, ctx, dur=1.1):
    """La campana del Spark: aguda, corta y dorada. Ataque nítido, cola breve."""
    x = metal("oro", f0, dur, ctx, t60=0.9, brillo=1.15, golpe=1.0)
    brillo = metal("plata", f0 * 2, dur, ctx.hijo("crot"), t60=0.45, golpe=0.0)
    return x + 0.22 * brillo


# ---------------------------------------------------------------------------
# Sal
# ---------------------------------------------------------------------------


def _granos(dur, ctx, tasa_env, f_lo=2500, f_hi=12000, dur_grano=(0.0002, 0.0012), ping=0.35,
            amp_sigma=0.8):
    """Granos de cristal: microchasquidos con densidad variable y algunos con resonancia."""
    n = n_(dur)
    x = np.zeros(n + n_(0.02))
    t = 0.0
    k = 0
    while t < dur:
        tasa = max(float(tasa_env(t)), 1e-3)
        t += ctx.rng.exponential(1.0 / tasa)
        if t >= dur:
            break
        i = n_(t)
        a = math.exp(ctx.rng.normal(0, amp_sigma))
        dg = ctx.u(*dur_grano)
        m = max(n_(dg), 4)
        g = ctx.normal(m) * np.exp(-np.linspace(0, 5, m))
        if ctx.u() < ping:
            f = ctx.u(f_lo, f_hi)
            mm = n_(ctx.u(0.002, 0.007))
            tt = np.arange(mm) / SR
            g2 = np.sin(2 * np.pi * f * tt) * np.exp(-tt / (mm / SR / 4))
            g = ajustar(g, mm) + 0.6 * g2
        x[i:i + len(g)] += a * g[: len(x) - i]
        k += 1
    return bp(x[:n], f_lo * 0.6, min(f_hi * 1.3, 20000), 2)


def fractura(ctx, fuerza=1.0, cuerpo_hz=None):
    """Fractura de halita: transitorio ancho, resonancias de cristal y un cuerpo breve."""
    d = 0.09
    c = chasquido(ctx, 0.0025, 600, 14000)
    lz = Lienzo(d)
    lz.add(c / (np.max(np.abs(c)) + 1e-9))
    ex = blanco(0.012, ctx.hijo("ex")) * env_exp(0.012, 0.006)
    res = np.zeros(n_(d))
    for k in range(4):
        f = ctx.u(1800, 6500)
        r = resonador(ajustar(ex, n_(d)), f, ctx.u(18, 40))
        res += r * ctx.u(0.4, 1.0)
    lz.add(res / (np.max(np.abs(res)) + 1e-9), 0.0, -6)
    cf = cuerpo_hz or ctx.u(220, 420)
    cuerpo = modal(0.06, [cf, cf * 1.9], [1, 0.4], [0.035, 0.02], ctx.hijo("cuerpo"))
    lz.add(cuerpo, 0.0, -9)
    return lz.senal() * fuerza


def crujido_sal(ctx, dur=0.22, pureza=0.85, granos=1.0, fracturas=2, grave=0.0):
    """El crujido de sal: fractura, granos y, con más pureza, un tono limpio.

    grave: 0..2 octavas hacia abajo para los crujidos lentos de la Cristalización.
    """
    lz = Lienzo(dur + 0.05)
    t = 0.0
    for k in range(fracturas):
        lz.add(fractura(ctx.hijo(f"f{k}"), ctx.u(0.6, 1.0) * (1.0 if k == 0 else 0.7)), t)
        t += ctx.u(0.02, 0.06)
    suciedad = np.clip((0.97 - pureza) / 0.27, 0, 1)  # 1 en Turbia, 0 en Quintaesencia
    tasa = lambda tt: 2500 * granos * np.exp(-tt / (dur * 0.45)) + 60  # noqa: E731
    gr = _granos(dur, ctx.hijo("granos"), tasa, ping=0.25 + 0.5 * (1 - suciedad))
    lz.add(gr / (np.max(np.abs(gr)) + 1e-9), 0.005, -4 + 4 * suciedad)
    if suciedad > 0:
        polvo = bp(blanco(dur, ctx.hijo("polvo")), 1500, 9000) * env_exp(dur, dur * 0.8, 0.003)
        lz.add(polvo / (np.max(np.abs(polvo)) + 1e-9), 0.0, -16 + 6 * suciedad)
    x = lz.senal()
    if grave > 0:
        x = cambiar_velocidad(x, 2 ** (-grave))
    return x


# ---------------------------------------------------------------------------
# Papel, pluma, cuero, madera, piedra
# ---------------------------------------------------------------------------


def papel(dur, ctx, densidad=180, f_lo=900, f_hi=7000, suavidad=0.0):
    """Pergamino que cruje: granos de ruido de banda ancha con densidad que cae."""
    n = n_(dur)
    x = np.zeros(n + n_(0.03))
    t = 0.0
    while True:
        t += ctx.rng.exponential(1.0 / densidad)
        if t >= dur:
            break
        m = n_(ctx.u(0.002, 0.014))
        g = ctx.normal(m) * np.exp(-np.linspace(0, ctx.u(2, 6), m))
        fc = ctx.u(f_lo, f_hi)
        g = bp(g, fc * 0.6, min(fc * 1.8, 20000), 1)
        i = n_(t)
        x[i:i + m] += math.exp(ctx.rng.normal(0, 0.7)) * g
    x = x[:n]
    if suavidad > 0:
        x = lp(x, f_hi * (1 - 0.5 * suavidad), 1)
    return x


def hoja(ctx, dur=0.32, fuerza=1.0):
    """Vuelta de hoja: barrido de aire, crujido y un pequeño aletazo al caer."""
    lz = Lienzo(dur)
    w = whoosh(dur * 0.8, ctx.hijo("w"), 700, 3200, q=1.2)
    lz.add(w / (np.max(np.abs(w)) + 1e-9), 0.0, -4)
    p = papel(dur * 0.8, ctx.hijo("p"), densidad=120) * env_campana(dur * 0.8, 0.4)
    lz.add(p / (np.max(np.abs(p)) + 1e-9), 0.0, -8)
    flap = lp(blanco(0.03, ctx.hijo("flap")), 900) * env_exp(0.03, 0.02)
    lz.add(flap / (np.max(np.abs(flap)) + 1e-9), dur * 0.72, -10)
    return lz.senal() * fuerza


def pluma(dur, ctx, trazos=2, fuerza=1.0, punto=True):
    """Pluma sobre pergamino: roce con trabas (stick-slip) y, al final, el punto."""
    n = n_(dur)
    x = np.zeros(n)
    bordes = sorted(ctx.u(0, dur * 0.85) for _ in range(trazos * 2))
    for k in range(trazos):
        a, b = bordes[2 * k], bordes[2 * k + 1]
        if b - a < 0.04:
            b = a + 0.04
        t = a
        while t < b:
            t += 1.0 / ctx.u(250, 700)
            i = n_(t)
            if i >= n - 40:
                break
            fase = (t - a) / (b - a)
            amp = math.sin(math.pi * min(fase, 1.0)) ** 0.7 * ctx.u(0.3, 1.0)
            x[i:i + 24] += amp * ctx.normal(24) * np.exp(-np.linspace(0, 4, 24))
    x = bp(x, 1800, 8000, 2)
    x = x + 0.6 * resonador(x, ctx.u(3800, 5200), 6)
    friccion = bp(blanco(dur, ctx.hijo("fr")), 2500, 9000) * 0.015
    x = x / (np.max(np.abs(x)) + 1e-9) + friccion * (np.abs(x) > 0.01)
    if punto:
        tk = chasquido(ctx.hijo("punto"), 0.002, 2000, 9000)
        x = Lienzo(dur).add(x).add(tk / (np.max(np.abs(tk)) + 1e-9), dur * 0.9, -6).senal()
    return x * fuerza


def madera(f0, ctx, dur=0.25, t60=0.12, fuerza=1.0):
    """Golpe de madera: pocos modos, caída rápida."""
    x = modal(dur, [f0, f0 * 2.57, f0 * 4.1, f0 * 5.9], [1, 0.45, 0.25, 0.12],
              [t60, t60 * 0.6, t60 * 0.4, t60 * 0.3], ctx)
    c = chasquido(ctx.hijo("c"), 0.002, 600, 6000)
    x[: len(c)] += 0.5 * c / (np.max(np.abs(c)) + 1e-9)
    return x * fuerza / (np.max(np.abs(x)) + 1e-9)


def piedra(ctx, dur=0.3, fuerza=1.0, grave=1.0):
    """Golpe de piedra: ráfaga de ruido, cuerpo grave y unas resonancias secas."""
    lz = Lienzo(dur)
    r = lp(blanco(0.05, ctx), 3000 * grave) * env_exp(0.05, 0.03)
    lz.add(r / (np.max(np.abs(r)) + 1e-9))
    c = modal(dur, [90 * grave, 160 * grave, 410 * grave], [1, 0.5, 0.3], [0.12, 0.08, 0.05], ctx.hijo("m"))
    lz.add(c / (np.max(np.abs(c)) + 1e-9), 0.0, -4)
    return lz.senal() * fuerza


def crujido_madera(dur, ctx, tasa=(25, 90), f_res=(420, 900, 1700), fuerza=1.0):
    """Madera que cruje (stick-slip): tren de impulsos con tasa variable por resonancias."""
    n = n_(dur)
    x = np.zeros(n)
    t = 0.0
    a0, a1 = tasa
    while t < dur:
        frac = t / dur
        r = a0 + (a1 - a0) * (0.5 - 0.5 * math.cos(2 * math.pi * frac * ctx.u(0.6, 1.4)))
        t += 1.0 / max(r * ctx.u(0.85, 1.15), 1.0)
        i = n_(t)
        if i >= n:
            break
        x[i] += ctx.u(0.5, 1.0) * math.sin(math.pi * frac) ** 0.5
    y = np.zeros(n)
    for f in f_res:
        y += resonador(x, f * ctx.u(0.9, 1.1), ctx.u(8, 18))
    return y * fuerza / (np.max(np.abs(y)) + 1e-9)


# ---------------------------------------------------------------------------
# Aire, cuerpos y foley
# ---------------------------------------------------------------------------


def whoosh(dur, ctx, f0=400, f1=2500, q=1.5, pico=0.6, forma=1.8, rosa_=True):
    """Barrido de aire: ruido por un paso banda que se mueve de f0 a f1."""
    x = rosa(dur, ctx) if rosa_ else blanco(dur, ctx)
    T = max(dur, 1e-3)
    y = banda_movil(x, lambda t: f0 * (f1 / f0) ** np.clip(t / T, 0, 1), q=q)
    return y * env_campana(dur, pico, forma)


def golpe_sordo(ctx, f_ini=120, f_fin=48, dur=0.25, click=0.3):
    """Golpe de cuerpo grave: senoidal que cae de tono, con un clic opcional."""
    t = tiempo(dur)
    f = f_fin + (f_ini - f_fin) * np.exp(-t / 0.035)
    x = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t / (dur * 0.28))
    if click > 0:
        c = chasquido(ctx, 0.003, 300, 4000)
        x[: len(c)] += click * c / (np.max(np.abs(c)) + 1e-9)
    return x


def pelaje(dur, ctx, brillo=1.0, flutter=(12, 35)):
    """Pelaje que se roza o se sacude: ruido con modulación rápida e irregular."""
    x = bp(blanco(dur, ctx), 1200 * brillo, 9000) * am_aleatoria(dur, ctx.hijo("am"), flutter[0], flutter[1], 1.0)
    return x


def plumas(dur, ctx):
    x = bp(blanco(dur, ctx), 900, 6500) * am_aleatoria(dur, ctx.hijo("am"), 20, 45, 1.0)
    return x + 0.5 * whoosh(dur, ctx.hijo("w"), 500, 1500, q=1.0)


def aleteo(ctx, n=3, periodo=0.11, grave=1.0):
    """Aleteos: pulsos de aire graves (alas o membranas)."""
    lz = Lienzo(n * periodo + 0.15)
    for k in range(n):
        w = whoosh(0.12, ctx.hijo(f"a{k}"), 250 * grave, 900 * grave, q=1.2, pico=0.35)
        lz.add(w, k * periodo * ctx.u(0.9, 1.1), -2 * k * 0.5)
    return lz.senal()


def viento(dur, ctx, centro=600, rango=2.0, rafagas=0.5):
    """Viento: ruido rosa con centro que deriva lentamente y ráfagas."""
    x = rosa(dur, ctx)
    deriva = lp(ctx.normal(n_(dur) + 4800), 0.6)[4800:]
    deriva = deriva / (np.max(np.abs(deriva)) + 1e-9)
    tt = tiempo(dur)

    def fc(t):
        return centro * rango ** np.interp(t, tt, deriva)
    y = banda_movil(x, fc, q=1.4, piso=0.02)
    return y * am_aleatoria(dur, ctx.hijo("raf"), 0, 1.5, rafagas)


def cadena_hierro(dur, ctx, eslabones=10, f_base=2600):
    """Cadena de hierro: eslabones que tintinean en racimos."""
    lz = Lienzo(dur)
    for k in range(eslabones):
        t = ctx.u(0, dur * 0.7) * (k / eslabones) ** 0.6 + ctx.u(0, 0.02)
        f = f_base * ctx.u(0.7, 1.6)
        m = modal(0.12, [f, f * 1.73, f * 2.6], [1, 0.6, 0.3], [0.06, 0.04, 0.03], ctx.hijo(f"e{k}"))
        lz.add(m, t, ctx.u(-10, 0))
    return lz.senal()


def raspado(dur, ctx, aspereza=1.0, f_res=(1100, 2300, 3900)):
    """Óxido raspado: ruido con trabas y resonancias metálicas."""
    x = blanco(dur, ctx) * am_aleatoria(dur, ctx.hijo("am"), 30, 120 * aspereza, 0.9)
    y = bp(x, 400, 9000)
    for f in f_res:
        y = y + 0.6 * resonador(x, f * ctx.u(0.9, 1.1), 12)
    return y * env_puntos(dur, [(0, 0), (0.03, 1), (dur * 0.7, 0.8), (dur, 0)])


def engranes(dur, ctx, tasa=14.0, tono=3200, ratchet=True):
    """Engranes de astrolabio en latón: tics con micro resonancia metálica."""
    lz = Lienzo(dur + 0.08)
    t = 0.0
    k = 0
    while t < dur:
        f = tono * ctx.u(0.85, 1.2)
        m = modal(0.05, [f, f * 1.6, f * 2.3], [1, 0.5, 0.3], [0.025, 0.018, 0.01], ctx.hijo(f"t{k}"))
        lz.add(m, t, -2 if k % 4 == 0 else ctx.u(-9, -4))
        if ratchet and k % 2 == 0:
            r = chasquido(ctx.hijo(f"r{k}"), 0.002, 3000, 12000)
            lz.add(r / (np.max(np.abs(r)) + 1e-9), t + 0.01, -14)
        t += 1.0 / (tasa * ctx.u(0.85, 1.15))
        k += 1
    return lz.senal()


def abaco(ctx, n=4, sep=0.07):
    """Cuentas de ábaco: madera contra madera, en pares."""
    lz = Lienzo(n * sep + 0.15)
    for k in range(n):
        f = ctx.u(1300, 1900)
        lz.add(madera(f, ctx.hijo(f"a{k}"), 0.08, 0.03), k * sep * ctx.u(0.8, 1.2), ctx.u(-6, 0))
        lz.add(madera(f * 1.1, ctx.hijo(f"b{k}"), 0.06, 0.02), k * sep + 0.012, -9)
    return lz.senal()


def tic_reloj(ctx, tono=1.0, fuerza=1.0):
    """Tic de reloj: clic metálico corto con cuerpo de madera."""
    m = modal(0.06, [2900 * tono, 4700 * tono, 7100 * tono], [1, 0.6, 0.4], [0.02, 0.012, 0.008], ctx)
    w = madera(900 * tono, ctx.hijo("w"), 0.06, 0.02)
    return (m / (np.max(np.abs(m)) + 1e-9) + 0.6 * w) * fuerza


def corcho(ctx, fuerza=1.0):
    """Corcho que sale: chirrido breve de fricción y el 'pop' de la presión."""
    lz = Lienzo(0.25)
    t = tiempo(0.06)
    chir = np.sin(2 * np.pi * (1300 + 900 * t / 0.06) * t) * am_aleatoria(0.06, ctx, 40, 160, 1.0)
    lz.add(chir * env_campana(0.06, 0.5) * 0.25, 0.0)
    tt = tiempo(0.05)
    pop = np.sin(2 * np.pi * np.cumsum(420 * np.exp(-tt / 0.012) + 160) / SR) * np.exp(-tt / 0.012)
    pc = chasquido(ctx.hijo("p"), 0.002, 500, 6000)
    lz.add(pop, 0.06)
    lz.add(pc / (np.max(np.abs(pc)) + 1e-9), 0.06, -8)
    return lz.senal() * fuerza


# ---------------------------------------------------------------------------
# Agua y vapor
# ---------------------------------------------------------------------------


def burbuja(f0, ctx, amp=1.0, xi=0.1):
    """Burbuja de Minnaert con subida de tono (modelo de Van den Doel)."""
    d = 0.043 * f0 + 0.0014 * f0 ** 1.5
    dur = min(6.0 / d, 0.25)
    t = tiempo(dur)
    f = f0 * (1 + xi * d * t)
    x = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-d * t)
    na = min(n_(0.0006), len(x))
    x[:na] *= np.linspace(0, 1, na)
    return amp * x


def burbujeo(dur, ctx, tasa=40.0, f_lo=500, f_hi=3500, tasa_env=None, xi=0.1):
    """Muchas burbujas al azar (distribución de tamaños log-uniforme)."""
    lz = Lienzo(dur + 0.3)
    t = 0.0
    k = 0
    while True:
        r = tasa * (tasa_env(t) if tasa_env else 1.0)
        t += ctx.rng.exponential(1.0 / max(r, 1e-3))
        if t >= dur:
            break
        f = math.exp(ctx.u(math.log(f_lo), math.log(f_hi)))
        a = (f_lo / f) ** 0.4 * ctx.u(0.3, 1.0)
        lz.add(burbuja(f, ctx, a, xi), t)
        k += 1
    return lz.senal()


def gota(f0, ctx, amp=1.0):
    """Gota que cae en agua: plip agudo (burbuja con subida marcada)."""
    return burbuja(f0, ctx, amp, xi=0.35)


def vapor(dur, ctx, brillo=1.0, soplo=0.3):
    """Vapor: siseo de banda alta con respiración lenta."""
    x = hp(blanco(dur, ctx), 1800 * brillo, 2)
    x = lp(x, 11000, 1) * am_aleatoria(dur, ctx.hijo("am"), 0, 4, 0.5)
    s = whoosh(dur, ctx.hijo("s"), 600, 1400, q=0.8, pico=0.4) * soplo
    return x / (np.max(np.abs(x)) + 1e-9) + s


def chapoteo(ctx, dur=0.6, fuerza=1.0, grave=1.0):
    """Chapoteo: golpe de agua (ruido grave), salpicadura y burbujas."""
    lz = Lienzo(dur)
    g = lp(blanco(0.12, ctx), 1200 * grave) * env_exp(0.12, 0.08, 0.004)
    lz.add(g / (np.max(np.abs(g)) + 1e-9))
    sp = bp(blanco(0.25, ctx.hijo("s")), 1500, 8000) * env_exp(0.25, 0.18, 0.006)
    lz.add(sp / (np.max(np.abs(sp)) + 1e-9), 0.005, -8)
    b = burbujeo(dur * 0.8, ctx.hijo("b"), 70, 350 * grave, 2500 * grave,
                 tasa_env=lambda t: math.exp(-t / (dur * 0.25)))
    lz.add(b / (np.max(np.abs(b)) + 1e-9), 0.01, -5)
    return lz.senal() * fuerza


def escurrir(dur, ctx, tasa=25.0, f_lo=1200, f_hi=4500):
    """Agua que escurre del pelaje: chorrito que se vuelve gotas cada vez más espaciadas."""
    lz = Lienzo(dur)
    tr = bp(blanco(dur, ctx), 1500, 7000) * env_exp(dur, dur * 0.5, 0.02)
    tr *= am_aleatoria(dur, ctx.hijo("am"), 5, 40, 0.8)
    lz.add(tr / (np.max(np.abs(tr)) + 1e-9), 0, -12)
    b = burbujeo(dur, ctx.hijo("g"), tasa, f_lo, f_hi, tasa_env=lambda t: math.exp(-t / (dur * 0.4)), xi=0.3)
    lz.add(b / (np.max(np.abs(b)) + 1e-9), 0, -2)
    return lz.senal()


def hervor(dur, ctx, intensidad=1.0):
    """Hervor: burbujas graves y densas con un rumor de fondo."""
    b = burbujeo(dur, ctx, 30 + 120 * intensidad, 140, 900 + 600 * intensidad, xi=0.05)
    rumor = lp(marron(dur, ctx.hijo("r")), 250) * 0.15 * intensidad
    return b / (np.max(np.abs(b)) + 1e-9) + rumor


# ---------------------------------------------------------------------------
# Fuego, estaño, trueno
# ---------------------------------------------------------------------------


def crepitar(dur, ctx, tasa=40.0, f_lo=1200, f_hi=9000):
    """Chasquidos de fuego o de estática."""
    n = n_(dur)
    x = np.zeros(n + 200)
    t = 0.0
    while True:
        t += ctx.rng.exponential(1.0 / tasa)
        if t >= dur:
            break
        m = ctx.i(8, 60)
        i = n_(t)
        x[i:i + m] += (ctx.rng.pareto(2.5) + 0.2) * ctx.normal(m) * np.exp(-np.linspace(0, 6, m))
    return bp(x[:n], f_lo, f_hi, 2)


def fuego(dur, ctx, crepita=1.0, rugido=1.0):
    """Fuego: rumor grave con parpadeo, siseo y chasquidos."""
    r = lp(marron(dur, ctx), 400) * am_aleatoria(dur, ctx.hijo("fl"), 2, 12, 0.7) * rugido
    s = bp(blanco(dur, ctx.hijo("s")), 2000, 7000) * 0.08
    c = crepitar(dur, ctx.hijo("c"), 25 + 50 * crepita)
    return (r / (np.max(np.abs(r)) + 1e-9) + s + c / (np.max(np.abs(c)) + 1e-9) * 0.6 * crepita)


def grito_estano(dur, ctx, densidad=600.0):
    """El 'grito del estaño': ráfagas densas de microcrujidos al doblar la barra."""
    env = am_aleatoria(dur, ctx.hijo("r"), 3, 14, 1.0) ** 2
    tt = tiempo(dur)

    def tasa(t):
        return densidad * float(np.interp(t, tt, env)) + 5
    g = _granos(dur, ctx, tasa, 1200, 7000, (0.0001, 0.0006), ping=0.15, amp_sigma=0.6)
    return g / (np.max(np.abs(g)) + 1e-9)


def estatica(dur, ctx, crujido=1.0):
    """Estática gris: siseo con modulación nerviosa y chasquidos."""
    s = bp(blanco(dur, ctx), 900, 6000) * am_aleatoria(dur, ctx.hijo("am"), 8, 60, 0.9) * 0.3
    c = crepitar(dur, ctx.hijo("c"), 60 * crujido, 1500, 8000)
    return s + c / (np.max(np.abs(c)) + 1e-9) * 0.7


def trueno_lamina(dur, ctx, fuerza=1.0, afinado=None):
    """Lámina de trueno: retumbo con aleteo y brillo metálico; afinado = Hz de una campana grave."""
    x = lp(marron(dur, ctx), 500) * am_aleatoria(dur, ctx.hijo("fl"), 4, 18, 0.8)
    x = x / (np.max(np.abs(x)) + 1e-9)
    brillo = bp(blanco(dur, ctx.hijo("b")), 2500, 8000) * am_aleatoria(dur, ctx.hijo("fl2"), 8, 30, 1.0)
    y = x + 0.15 * brillo
    env = env_puntos(dur, [(0, 0), (0.01, 1), (dur * 0.25, 0.6), (dur, 0)])
    y = y * env
    if afinado:
        c = metal("estano", afinado, dur, ctx.hijo("gong"), t60=dur * 0.8, golpe=0.5)
        y = y + 0.6 * c
    return y * fuerza


# ---------------------------------------------------------------------------
# Horror
# ---------------------------------------------------------------------------


def dron(dur, ctx, f0=38.0, armonicos=(2, 3, 4, 5), batido=0.35, presencia=1.0):
    """Grave casi inaudible con armónicos de 80 a 200 Hz para que exista en la bocina del teléfono."""
    t = tiempo(dur)
    x = np.sin(2 * np.pi * f0 * t) + 0.7 * np.sin(2 * np.pi * (f0 + batido) * t + 1.3)
    for k, h in enumerate(armonicos):
        f = f0 * h
        a = presencia * 0.35 / (1 + 0.3 * k)
        x += a * (np.sin(2 * np.pi * f * t + ctx.u(0, 6)) + np.sin(2 * np.pi * (f + batido * h * 0.7) * t))
    x *= am_aleatoria(dur, ctx, 0, 0.4, 0.4)
    return x / (np.max(np.abs(x)) + 1e-9)


def susurro(dur, ctx, silabas=5, reves=False, registro=1.0):
    """Susurro sin palabras: ruido con formantes que saltan entre vocales al azar.

    No hay voz con tono ni palabras: es aire con forma de vocal. registro desplaza los formantes.
    """
    vocales = [(700, 1200, 2600), (500, 1900, 2600), (350, 2200, 2900), (450, 900, 2500), (380, 950, 2400)]
    n = n_(dur)
    tt = tiempo(dur)
    lim = np.sort(np.concatenate([[0, dur], [ctx.u(0.05, dur - 0.05) for _ in range(silabas - 1)]]))
    centros = np.zeros((3, n))
    env = np.zeros(n)
    for k in range(silabas):
        a, b = lim[k], lim[k + 1]
        v = vocales[ctx.i(0, len(vocales) - 1)]
        m = (tt >= a) & (tt < b)
        for j in range(3):
            centros[j, m] = v[j] * registro * ctx.u(0.92, 1.08)
        largo = max(b - a, 1e-3)
        fase = np.clip((tt[m] - a) / largo, 0, 1)
        env[m] = np.sin(np.pi * fase) ** 1.5 * ctx.u(0.5, 1.0)
    for j in range(3):
        centros[j] = lp(centros[j], 25, 1)

    def fc(t):
        return [np.interp(t, tt, centros[j]) for j in range(3)]
    base = rosa(dur, ctx.hijo("r"))
    x = formantes(base, fc, (90, 140, 220), (1.0, 0.6, 0.35))
    sib = hp(blanco(dur, ctx.hijo("s")), 4500, 2) * 0.12
    x = x / (np.max(np.abs(x)) + 1e-9) + sib
    x = x * env
    if reves:
        x = x[::-1].copy()
    return x


def pulsar(dur, ctx, periodo=0.75, tono=1400):
    """Señal de radiotelescopio: clics lentos tipo púlsar con resonancia suave."""
    lz = Lienzo(dur + 0.1)
    t = ctx.u(0, periodo * 0.5)
    while t < dur:
        c = modal(0.08, [tono, tono * 1.5], [1, 0.3], [0.04, 0.02], ctx.hijo(f"{t:.3f}"), ataque=0.002)
        lz.add(c, t, ctx.u(-4, 0))
        t += periodo * ctx.u(0.98, 1.02)
    return lz.senal()


def onda_corta(dur, ctx, f_ini=900, f_fin=1150):
    """Tono de onda corta que deriva despacio, con estática; lento, nunca un láser."""
    t = tiempo(dur)
    f = f_ini + (f_fin - f_ini) * (0.5 - 0.5 * np.cos(np.pi * t / max(dur, 1e-3)))
    f = f * (1 + 0.004 * np.sin(2 * np.pi * 0.6 * t))
    x = np.sin(2 * np.pi * np.cumsum(f) / SR) * am_aleatoria(dur, ctx, 0, 3, 0.7)
    x = bp(x, 400, 3000) + 0.25 * estatica(dur, ctx.hijo("e"), 0.3)
    return x * env_campana(dur, 0.5, 1.2)


# ---------------------------------------------------------------------------
# Espacio y transformación
# ---------------------------------------------------------------------------


@functools.lru_cache(maxsize=32)
def _ir(rt60, canales, brillo, semilla):
    ctx = Ctx(semilla)
    dur = rt60 * 1.1 + 0.05
    n = n_(dur)
    t = np.arange(n) / SR
    irs = []
    for c in range(canales):
        ruido = ctx.normal(n)
        acc = np.zeros(n)
        bandas = [(None, 500, 1.15), (500, 2000, 1.0), (2000, 6000, 0.75 * brillo), (6000, None, 0.45 * brillo)]
        for lo, hi, k in bandas:
            if lo is None:
                b = lp(ruido, hi, 2)
            elif hi is None:
                b = hp(ruido, lo, 2)
            else:
                b = bp(ruido, lo, hi, 2)
            acc += b * np.exp(-6.9078 * t / (rt60 * k))
        # reflexiones tempranas de una capilla pequeña de piedra
        for k in range(8):
            i = n_(ctx.u(0.006, 0.045))
            acc[i] += ctx.u(-1, 1) * 2.0
        acc *= np.clip(t / 0.004, 0, 1)
        irs.append(acc / np.sqrt(np.sum(acc ** 2)))
    return np.stack(irs, axis=1) if canales == 2 else irs[0]


def sala(x, mezcla=0.2, rt60=1.2, predelay=0.012, estereo=False, brillo=1.0, semilla=7):
    """Reverb de convolución de la 'capilla pequeña de piedra' (la misma sala de la música)."""
    canales = 2 if estereo else 1
    ir = _ir(round(rt60, 3), canales, round(brillo, 3), semilla)
    mono = a_mono(x) if x.ndim == 2 else x
    pre = n_(predelay)
    if estereo:
        wet = np.stack([signal.fftconvolve(mono, ir[:, c]) for c in range(2)], axis=1)
        wet = np.concatenate([np.zeros((pre, 2)), wet])
        dry = x if x.ndim == 2 else np.stack([x, x], axis=1) * 0.7071
    else:
        wet = np.concatenate([np.zeros(pre), signal.fftconvolve(mono, ir)])
        dry = mono
    n = max(len(dry), len(wet))
    out = ajustar(dry, n) * (1 - mezcla * 0.5) + ajustar(wet, n) * mezcla * 2.0
    return out


def eco(x, retardo=0.18, realim=0.4, repeticiones=4, lp_hz=5000):
    """Ecos discretos que se oscurecen (el Espejo Lunar)."""
    lz = Lienzo(len(x) / SR + retardo * repeticiones + 0.05)
    lz.add(x)
    y = x
    for k in range(1, repeticiones + 1):
        y = lp(y, lp_hz, 1)
        lz.add(y, retardo * k, 20 * math.log10(realim ** k))
    return lz.senal()


def cambiar_velocidad(x, factor):
    """Cambia velocidad y tono juntos por remuestreo (factor 0.5 = una octava abajo y el doble de largo)."""
    fr = Fraction(factor).limit_denominator(97)
    return signal.resample_poly(x, fr.denominator, fr.numerator, axis=0)


def estirar_tono(x, semitonos):
    return cambiar_velocidad(x, 2 ** (semitonos / 12.0))


def coro(x, ctx, voces=3, cents=12, retardo=0.012):
    """Desafinación suave entre copias (aura de Quintaesencia)."""
    lz = Lienzo(len(x) / SR + retardo * voces + 0.02)
    lz.add(x)
    for k in range(voces):
        r = 2 ** (ctx.u(-cents, cents) / 1200)
        lz.add(cambiar_velocidad(x, r), retardo * (k + 1) * ctx.u(0.6, 1.4), -6)
    return lz.senal()


def estereo_ancho(x, ctx, ancho=0.6, retardo_ms=11):
    """Convierte mono en estéreo con un decorrelador suave (para stingers y lechos)."""
    d = n_(retardo_ms / 1000)
    a = x
    b = np.concatenate([np.zeros(d), x])[: len(x)]
    b = lp(b, 9000, 1)
    l = a + ancho * 0.5 * b
    r = a - ancho * 0.5 * b
    return np.stack([l, r], axis=1)


# ---------------------------------------------------------------------------
# Fuentes de bibliotecas libres (CC0)
# ---------------------------------------------------------------------------

USO_FUENTES = set()


@functools.lru_cache(maxsize=256)
def _cargar_fuente(nombre):
    import soundfile as sf
    ruta = os.path.join(FUENTES_DIR, nombre)
    x, sr = sf.read(ruta, always_2d=True, dtype="float64")
    m = x.mean(axis=1)
    if sr != SR:
        fr = Fraction(SR, sr)
        m = signal.resample_poly(m, fr.numerator, fr.denominator)
    m = m - np.mean(m)
    m = m / (np.max(np.abs(m)) + 1e-12)
    m.setflags(write=False)
    return m


def fuente(nombre, velocidad=1.0, hp_hz=None, lp_hz=None, recorte=None):
    """Carga una fuente CC0 de `fuentes/` como mono a 48 kHz, normalizada a pico 1.

    nombre: ruta relativa, p. ej. 'kenney_rpg_audio/book_open.ogg'.
    recorte: (inicio, fin) en segundos.
    """
    USO_FUENTES.add(nombre)
    x = np.array(_cargar_fuente(nombre), copy=True)
    if recorte:
        x = x[n_(recorte[0]):n_(recorte[1]) if recorte[1] else None]
        x = fundido(x, 0.002, 0.01)
    if velocidad != 1.0:
        x = cambiar_velocidad(x, velocidad)
    if hp_hz:
        x = hp(x, hp_hz, 2)
    if lp_hz:
        x = lp(x, lp_hz, 2)
    return x / (np.max(np.abs(x)) + 1e-12)
