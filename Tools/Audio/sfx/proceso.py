# -*- coding: utf-8 -*-
"""Acabado de cada archivo: DC, recorte, fundidos, sonoridad, pico real y escritura.

La sonoridad se calcula con BS.1770 (ponderación K) para ajustar la ganancia.
La verificación final se hace aparte con ffmpeg (ebur128 con peak=true).
"""
import math
import subprocess
import re

import numpy as np
from scipy import signal
from scipy.ndimage import minimum_filter1d

from sintesis import SR, n_, hp, ajustar

TECHO_DBTP = -1.3        # margen bajo el −1 dBTP del plan (5.7)
UMBRAL_INICIO_DB = -55   # recorte del inicio, relativo al pico
UMBRAL_FINAL_DB = -62    # recorte de la cola, relativo al pico

# Coeficientes de ponderación K de BS.1770 para 48 kHz.
_K1_B = [1.53512485958697, -2.69169618940638, 1.19839281085285]
_K1_A = [1.0, -1.69065929318241, 0.73248077421585]
_K2_B = [1.0, -2.0, 1.0]
_K2_A = [1.0, -1.99004745483398, 0.99007225036621]


def _canales(x):
    return [x] if x.ndim == 1 else [x[:, c] for c in range(x.shape[1])]


def sonoridad(x, ventana=0.4, salto=0.01):
    """Máximo de sonoridad con ventana deslizante (0.4 s = momentánea, 3 s = corto plazo), en LUFS."""
    pad = n_(0.5)
    acc = None
    for c in _canales(x):
        y = np.concatenate([c, np.zeros(pad)])
        y = signal.lfilter(_K1_B, _K1_A, y)
        y = signal.lfilter(_K2_B, _K2_A, y)
        cs = np.concatenate([[0.0], np.cumsum(y * y)])
        w = n_(ventana)
        h = max(n_(salto), 1)
        fins = np.arange(h, len(y) + 1, h)
        inis = np.maximum(fins - w, 0)
        ms = (cs[fins] - cs[inis]) / w
        acc = ms if acc is None else acc + ms
    m = float(np.max(acc)) if acc is not None and len(acc) else 0.0
    return -0.691 + 10 * math.log10(max(m, 1e-20))


def integrada(x):
    """Sonoridad integrada con compuerta absoluta y relativa (BS.1770-4)."""
    bloques = []
    acc = None
    for c in _canales(x):
        y = signal.lfilter(_K1_B, _K1_A, c)
        y = signal.lfilter(_K2_B, _K2_A, y)
        w, h = n_(0.4), n_(0.1)
        cs = np.concatenate([[0.0], np.cumsum(y * y)])
        fins = np.arange(w, len(y) + 1, h)
        ms = (cs[fins] - cs[fins - w]) / w
        acc = ms if acc is None else acc + ms
    if acc is None or not len(acc):
        return -70.0
    l = -0.691 + 10 * np.log10(np.maximum(acc, 1e-20))
    sel = acc[l > -70]
    if not len(sel):
        return -70.0
    rel = -0.691 + 10 * math.log10(np.mean(sel)) - 10
    sel = acc[(l > -70) & (l > rel)]
    return -0.691 + 10 * math.log10(np.mean(sel))


def pico_real(x):
    """Pico real en dBTP con sobremuestreo ×4."""
    m = 0.0
    for c in _canales(x):
        y = signal.resample_poly(c, 4, 1)
        m = max(m, float(np.max(np.abs(y))), float(np.max(np.abs(c))))
    return 20 * math.log10(max(m, 1e-12))


def _envolvente(x, ms=5):
    m = np.abs(x) if x.ndim == 1 else np.max(np.abs(x), axis=1)
    w = max(n_(ms / 1000), 1)
    k = np.ones(w) / w
    return np.sqrt(np.convolve(m * m, k, mode="same"))


def recortar(x, fundido_ini=0.0015, preroll=0.001):
    """Quita silencios de los extremos sin cortar la cola y aplica fundidos de coseno."""
    env = _envolvente(x)
    pico = np.max(env) if len(env) else 0
    if pico <= 0:
        return x
    sobre_ini = np.nonzero(env > pico * 10 ** (UMBRAL_INICIO_DB / 20))[0]
    sobre_fin = np.nonzero(env > pico * 10 ** (UMBRAL_FINAL_DB / 20))[0]
    i0 = max(int(sobre_ini[0]) - n_(preroll), 0)
    i1 = min(int(sobre_fin[-1]) + n_(0.01), len(x))
    y = np.array(x[i0:i1], dtype=float, copy=True)
    nf = min(max(n_(0.012), int(len(y) * 0.06)), n_(0.12), len(y) // 2)
    ni = min(n_(fundido_ini), len(y) // 4)
    if ni > 1:
        w = 0.5 - 0.5 * np.cos(np.linspace(0, math.pi, ni))
        y[:ni] = (y[:ni].T * w).T
    if nf > 1:
        w = 0.5 + 0.5 * np.cos(np.linspace(0, math.pi, nf))
        y[-nf:] = (y[-nf:].T * w).T
    return y


def bucle(x, cruce=0.25):
    """Convierte una señal en bucle sin costura: el final se funde con el inicio (potencia constante)."""
    nc = n_(cruce)
    cuerpo = x[: len(x) - nc].copy()
    cola = x[len(x) - nc:]
    t = np.linspace(0, math.pi / 2, nc)
    fi, fo = np.sin(t), np.cos(t)
    cuerpo[:nc] = (cuerpo[:nc].T * fi).T + (cola.T * fo).T
    return cuerpo


def limitar(x, techo_lin, liberacion=0.06):
    """Limitador con anticipación: la ganancia baja antes del pico y sube despacio. Nunca recorta."""
    env = np.abs(x) if x.ndim == 1 else np.max(np.abs(x), axis=1)
    sobre = signal.resample_poly(env, 4, 1)
    sobre = np.max(np.abs(sobre).reshape(-1, 4), axis=1)[: len(env)]
    env = np.maximum(env, sobre) * 1.05
    g = np.minimum(1.0, techo_lin / np.maximum(env, 1e-12))
    anti = n_(0.0015)
    g = minimum_filter1d(g, size=2 * anti + 1, mode="nearest")
    a = math.exp(-1.0 / (liberacion * SR))
    g = signal.lfilter([1 - a], [1, -a], g - 1.0) + 1.0
    g = np.minimum(g, minimum_filter1d(g, size=2 * anti + 1, mode="nearest"))
    g = np.convolve(g, np.ones(anti) / anti, mode="same")
    return (x.T * g).T


def acabar(x, objetivo, metrica="M", es_bucle=False, hp_hz=25.0, limite_db=3.0):
    """Cadena completa de acabado. Devuelve (señal, informe)."""
    x = np.asarray(x, dtype=float)
    if not es_bucle:
        x = np.concatenate([x, np.zeros((n_(0.01),) + x.shape[1:])])
    # 1. DC y subgraves inútiles
    x = hp(x, hp_hz, 2)
    # 2. Recorte y fundidos
    if not es_bucle:
        x = recortar(x)
    # 3. Ganancia hacia el objetivo de sonoridad
    if metrica == "I":
        actual = integrada(x)
    else:
        actual = sonoridad(x, 0.4 if metrica == "M" else 3.0)
    g = objetivo - actual
    x = x * 10 ** (g / 20)
    # 4. Pico real
    tp = pico_real(x)
    exceso = tp - TECHO_DBTP
    if exceso > 0:
        if exceso <= limite_db:
            x = limitar(x, 10 ** ((TECHO_DBTP - 0.2) / 20))
        else:
            x = limitar(x * 10 ** (-(exceso - limite_db) / 20), 10 ** ((TECHO_DBTP - 0.2) / 20))
        for _ in range(6):
            tp = pico_real(x)
            if tp <= TECHO_DBTP:
                break
            x = x * 10 ** ((TECHO_DBTP - tp - 0.05) / 20)
    inf = dict(
        duracion=len(x) / SR,
        m_max=sonoridad(x, 0.4),
        s_max=sonoridad(x, 3.0),
        tp=pico_real(x),
        objetivo=objetivo,
        metrica=metrica,
    )
    return x, inf


def revisar(x, es_bucle=False):
    """Pruebas automáticas: DC, bordes, saturación, cola cortada y costura del bucle."""
    avisos = []
    canales = _canales(x)
    for c in canales:
        if abs(float(np.mean(c))) > 2e-3:
            avisos.append("DC")
        if np.max(np.abs(c)) >= 0.999:
            avisos.append("saturación")
        if not es_bucle:
            if abs(c[0]) > 2e-3 or abs(c[-1]) > 2e-3:
                avisos.append("borde con clic")
            pico = np.max(np.abs(c)) + 1e-12
            cola = c[-n_(0.03):-n_(0.012)] if len(c) > n_(0.05) else c[-5:]
            if len(cola) and np.sqrt(np.mean(cola ** 2)) / pico > 10 ** (-38 / 20):
                avisos.append("cola cortada")
        else:
            d0 = abs(c[0] - c[-1])
            esc = np.sqrt(np.mean(c ** 2)) + 1e-12
            if d0 > 6 * esc * 0.2 + 0.02:
                avisos.append("costura del bucle")
    return sorted(set(avisos))


def escribir(ruta, x):
    import soundfile as sf
    sf.write(ruta, np.clip(x, -1.0, 32766 / 32768), SR, subtype="PCM_16")


_RE_M = re.compile(r"M:\s*(-?[\d.]+|-inf)\s+S:\s*(-?[\d.]+|-inf)")
_RE_I = re.compile(r"I:\s*(-?[\d.]+)\s+LUFS")
_RE_TP = re.compile(r"Peak:\s*(-?[\d.]+|-inf)\s+dBFS")


def medir_ffmpeg(ruta):
    """Mide con ffmpeg ebur128 (peak=true): máximo momentáneo, máximo de corto plazo, integrada y pico real."""
    cmd = ["ffmpeg", "-nostats", "-hide_banner", "-i", ruta, "-af",
           "apad=pad_dur=0.5,ebur128=peak=true:framelog=info", "-f", "null", "-"]
    p = subprocess.run(cmd, capture_output=True, text=True)
    salida = p.stderr
    ms, ss = [], []
    for m in _RE_M.finditer(salida):
        a, b = m.group(1), m.group(2)
        ms.append(float(a) if a != "-inf" else -200.0)
        ss.append(float(b) if b != "-inf" else -200.0)
    resumen = salida[salida.rfind("Summary:"):]
    mi = _RE_I.search(resumen)
    mt = _RE_TP.search(resumen)
    return dict(
        m_max=max(ms) if ms else None,
        s_max=max(ss) if ss else None,
        i=float(mi.group(1)) if mi else None,
        tp=float(mt.group(1)) if mt and mt.group(1) != "-inf" else None,
    )
