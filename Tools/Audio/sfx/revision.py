# -*- coding: utf-8 -*-
"""Hojas de revisión visual: espectrograma y forma de onda de cada archivo, una hoja por sistema.

Sirven para revisar sin escuchar: clics (líneas verticales de banda ancha), cortes (cola que termina en
seco), DC (forma de onda desplazada), saturación (forma de onda plana arriba) y colas cortadas.
"""
import glob
import math
import os

import numpy as np
from scipy import signal

from recetas import RECETAS, SISTEMAS, cargar, archivos

ANCHO, ALTO_ESP, ALTO_ONDA, ALTO_TXT = 236, 72, 22, 24
RANGO_DB = 72.0
COLUMNAS = 4
F_MIN, F_MAX = 40.0, 20000.0

_PALETA = np.array([
    [0, 0, 0], [28, 16, 68], [79, 18, 123], [129, 37, 129], [181, 54, 122],
    [229, 80, 100], [251, 135, 97], [254, 194, 135], [252, 253, 191]], dtype=float)


def _color(v):
    """v en [0, 1] -> RGB con una paleta tipo 'magma'."""
    v = np.clip(v, 0, 1) * (len(_PALETA) - 1)
    i = np.floor(v).astype(int)
    i2 = np.minimum(i + 1, len(_PALETA) - 1)
    f = (v - i)[..., None]
    return (_PALETA[i] * (1 - f) + _PALETA[i2] * f).astype(np.uint8)


def celda(ruta, etiqueta, detalle):
    from PIL import Image, ImageDraw, ImageFont
    import soundfile as sf
    x, sr = sf.read(ruta, always_2d=True, dtype="float64")
    m = x.mean(axis=1)
    n = len(m)
    hop = max(int(math.ceil(n / ANCHO)), 16)
    nper = 1024
    f, t, Z = signal.stft(np.concatenate([np.zeros(nper // 2), m, np.zeros(nper)]), sr, window="hann",
                          nperseg=nper, noverlap=max(nper - hop, 0), boundary=None, padded=True)
    S = 20 * np.log10(np.abs(Z) * 2 / np.sum(np.hanning(nper)) + 1e-9)
    cols = min(S.shape[1], ANCHO)
    S = S[:, :cols]
    # eje de frecuencia logarítmico
    filas = np.geomspace(F_MAX, F_MIN, ALTO_ESP)
    idx = np.clip(np.searchsorted(f, filas), 0, len(f) - 1)
    img = S[idx, :]
    tope = float(np.max(S)) if S.size else 0.0
    img = (img - (tope - RANGO_DB)) / RANGO_DB  # cada celda contra su propio máximo
    rgb = np.zeros((ALTO_ESP, ANCHO, 3), dtype=np.uint8)
    rgb[:, :cols] = _color(img)
    # forma de onda (mín y máx por columna)
    onda = np.full((ALTO_ONDA, ANCHO, 3), 18, dtype=np.uint8)
    paso = n / ANCHO
    mid = ALTO_ONDA // 2
    for c in range(ANCHO):
        a, b = int(c * paso), max(int((c + 1) * paso), int(c * paso) + 1)
        seg = m[a:b] if a < n else np.zeros(1)
        lo, hi = float(np.min(seg)), float(np.max(seg))
        y0 = int(mid - hi * (mid - 1))
        y1 = int(mid - lo * (mid - 1))
        onda[max(y0, 0):min(y1 + 1, ALTO_ONDA), c] = (120, 200, 255)
        if hi >= 0.99 or lo <= -0.99:
            onda[:, c] = (255, 40, 40)
    onda[mid, :] = np.maximum(onda[mid, :], 60)
    im = Image.new("RGB", (ANCHO, ALTO_TXT + ALTO_ESP + ALTO_ONDA), (12, 12, 12))
    im.paste(Image.fromarray(rgb), (0, ALTO_TXT))
    im.paste(Image.fromarray(onda), (0, ALTO_TXT + ALTO_ESP))
    d = ImageDraw.Draw(im)
    try:
        fuente = ImageFont.load_default(size=10)
    except TypeError:
        fuente = ImageFont.load_default()
    d.text((2, 1), etiqueta[:44], fill=(235, 235, 235), font=fuente)
    d.text((2, 12), detalle[:48], fill=(160, 200, 160), font=fuente)
    return im


def hojas(salida, destino, sistemas=None, medidas=None):
    from PIL import Image
    cargar()
    os.makedirs(destino, exist_ok=True)
    for clave, titulo in SISTEMAS:
        if sistemas and clave not in sistemas:
            continue
        lista = []
        for r in RECETAS.values():
            if r.sistema != clave:
                continue
            for v, a in archivos(r):
                ruta = os.path.join(salida, a + ".wav")
                if os.path.exists(ruta):
                    lista.append((a, ruta))
        if not lista:
            continue
        celdas = []
        for a, ruta in lista:
            import soundfile as sf
            info = sf.info(ruta)
            det = f"{info.frames / info.samplerate:.2f} s"
            if medidas and a in medidas:
                mm = medidas[a]
                if mm.get("m_max") is not None:
                    det += f" · M {mm['m_max']:.1f}"
                if mm.get("tp") is not None:
                    det += f" · TP {mm['tp']:.1f}"
            celdas.append(celda(ruta, a.replace("sfx_", "", 1), det))
        filas = int(math.ceil(len(celdas) / COLUMNAS))
        w, h = celdas[0].size
        hoja = Image.new("RGB", (COLUMNAS * (w + 4) + 4, filas * (h + 4) + 4), (40, 40, 40))
        for k, c in enumerate(celdas):
            hoja.paste(c, (4 + (k % COLUMNAS) * (w + 4), 4 + (k // COLUMNAS) * (h + 4)))
        ruta = os.path.join(destino, f"{clave}.png")
        hoja = hoja.convert("P", palette=Image.ADAPTIVE, colors=64)
        hoja.save(ruta, optimize=True)
        print(f"revisión: {ruta} ({len(celdas)} archivos)")
    for viejo in glob.glob(os.path.join(destino, "*.png")):
        if os.path.basename(viejo)[:-4] not in {c for c, _ in SISTEMAS}:
            os.remove(viejo)
