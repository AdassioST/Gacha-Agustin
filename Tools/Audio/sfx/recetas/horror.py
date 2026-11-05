# -*- coding: utf-8 -*-
"""3.5.14 Horror y Paralaje. Pavor antes que gore, nada de sustos repentinos (GDD 6.5).

Todo entra con fundidos lentos y queda por debajo del combate. Los Huéspedes suenan a radiotelescopio
(estática, clics lentos tipo púlsar, onda corta), nunca a láser.
"""
import numpy as np

from sintesis import (SR, Lienzo, metal, cuenco, copa_frotada, crujido_sal, crujido_madera, dron, susurro, pulsar,
                      onda_corta, estatica, whoosh, blanco, rosa, marron, bp, hp, lp, env_de, exp_de, env_campana, grado,
                      hz, sala, n_, tiempo, cambiar_velocidad, coro, RE_MAYOR)
from . import receta, var
from .comun import N

S = "horror"


def _entrada_lenta(x, sube=0.4, baja=0.5):
    d = len(x) / SR
    return x * env_de(x, [(0, 0), (min(sube, d / 2), 1), (max(d - baja, d / 2), 1), (d, 0)])


@receta("sfx_horror_huesped", S, "Huéspedes: señales de radiotelescopio; estática, clics lentos de púlsar y un tono de "
        "onda corta que deriva", "Escena o `anim:`", var(v=[1]), objetivo=-22)
def huesped(ctx, v):
    d = 2.2
    lz = Lienzo(d)
    lz.add(N(estatica(d, ctx.hijo("e"), 0.4)), 0, -12)
    lz.add(N(pulsar(d, ctx.hijo("p"), [0.55, 0.8, 0.37][v - 1], [1300, 900, 1700][v - 1])), 0, -4)
    lz.add(N(onda_corta(d, ctx.hijo("o"), [900, 650, 1200][v - 1], [1150, 600, 1500][v - 1])), 0, -8)
    return _entrada_lenta(lz.senal()[: n_(d)], 0.4, 0.6)


@receta("sfx_horror_salmodico_reves", S, "Susurro de Salmódico al revés: aire con forma de vocales, sin palabras ni voz, "
        "invertido (va con subtítulo de glifos)", "Escena con Paralaje 2 o más", var(v=[1, 2]), objetivo=-24)
def salmodico_reves(ctx, v):
    x = susurro(1.3, ctx, 3 + v, reves=True, registro=[0.85, 0.95, 0.8, 1.0][v - 1])
    return sala(_entrada_lenta(x, 0.2, 0.3), 0.3, 0.8)


@receta("sfx_horror_grave", S, "Grave casi inaudible (bucle): pedal bajo 40 Hz con armónicos de 80 a 200 Hz para que "
        "exista en la bocina del teléfono", "Lecho", var(v=[1]), objetivo=-32, metrica="I", bucle=True, hp_hz=18,
        estado="borrador", notas="El plan lo pide a 24 kHz en streaming; aquí va a 48 kHz como el resto.")
def grave(ctx, v):
    from proceso import bucle
    d = 2.0
    x = dron(d + 0.8, ctx, hz("D1"), (2, 3, 4, 5), 0.25, 1.2)
    return bucle(x[: n_(d + 0.8)], 0.8)


@receta("sfx_horror_cristalizado", S, "Estatuas de sal que todavía susurran: susurro apagado y cristal que cruje despacio",
        "Emisores 3D", var(v=[1]), objetivo=-24)
def cristalizado(ctx, v):
    lz = Lienzo(1.7)
    s = susurro(1.3, ctx.hijo("s"), 4, reves=(v == 2), registro=0.85)
    lz.add(N(lp(s, 3500)), 0.1, -4)
    lz.add(N(crujido_sal(ctx.hijo("c"), 0.3, 0.75, granos=0.8, fracturas=1, grave=1.5)), 0.5 + 0.2 * v, -8)
    return _entrada_lenta(lz.senal(), 0.3, 0.3)


@receta("sfx_horror_cielo", S, "El cielo cruje cuando la cámara deja de mirar: un crujido enorme y lento, y un retumbo "
        "lejano", "Escena", var(v=[1]), objetivo=-24)
def cielo(ctx, v):
    lz = Lienzo(2.3)
    c = crujido_madera(1.8, ctx.hijo("c"), (6, 24), (140, 310, 620))
    lz.add(N(c), 0.1, -2)
    lz.add(N(lp(marron(2.0, ctx.hijo("r")), 120)), 0, -10)
    return _entrada_lenta(lz.senal(), 0.5, 0.5)


@receta("sfx_horror_caida", S, "El Ascendente en Caída muestra un instante su forma: acorde de cristal invertido que "
        "crece, se quiebra y deja un susurro. Sube, no salta", "`SalinityBandChanged` (a 150) con Paralaje 3",
        var(v=[1]), objetivo=-18)
def caida(ctx, v):
    lz = Lienzo(2.7)
    acorde = Lienzo(1.4)
    for k, semi in enumerate([0, 1, 6, 11]):
        acorde.add(N(copa_frotada(hz("D5") * 2 ** (semi / 12), 1.4, ctx.hijo(f"c{k}"))), 0, -3)
    lz.add(N(acorde.senal()[::-1].copy()), 0, -2)
    lz.add(N(dron(1.6, ctx.hijo("d"), hz("D1"), (2, 3, 5), 0.6)) * env_de(np.zeros(n_(1.6)), [(0, 0), (1.3, 1), (1.6, 0)]),
           0, -8)
    lz.add(N(crujido_sal(ctx.hijo("q"), 0.4, 0.72, fracturas=2, grave=1.0)), 1.35, -6)
    lz.add(N(susurro(1.0, ctx.hijo("s"), 4, reves=True, registro=0.8)), 1.5, -14)
    return sala(lz.senal(), 0.25, 0.8)


@receta("sfx_paralaje_sube", S, "Sube el Paralaje: un acorde desafinado de cristal y un susurro que crece; más denso y "
        "más grave por nivel (medio, alto, máximo)", "`meta:paralaje_sube` (nivel 1 a 3)", var(nivel=[1, 2, 3]),
        objetivo=lambda v: [-21, -20, -19][v["nivel"] - 1])
def paralaje_sube(ctx, v):
    n = v["nivel"]
    lz = Lienzo(2.3)
    semis = [[0, 7], [0, 6, 13], [0, 1, 6, 13]][n - 1]
    for k, semi in enumerate(semis):
        c = copa_frotada(hz("D5") * 2 ** ((semi - 12 * (n == 3)) / 12) * (1 + 0.004 * k), 1.8, ctx.hijo(f"c{k}"))
        lz.add(N(c), 0.1 * k, -4 - k)
    s = susurro(1.6, ctx.hijo("s"), 4 + n, reves=(n >= 2), registro=0.9)
    lz.add(N(s) * env_de(s, [(0, 0), (1.4, 1), (1.6, 0)]), 0.2, -14 + 2 * n)
    if n >= 2:
        lz.add(N(dron(1.8, ctx.hijo("d"), hz("D1"), (2, 3, 4), 0.5)) * env_campana(1.8, 0.6), 0, -12)
    return _entrada_lenta(lz.senal(), 0.3, 0.3)


@receta("sfx_paralaje_baja", S, "Baja el Paralaje (Termas, Agua de Olvido): la disonancia se resuelve en un acorde "
        "limpio de cristal y agua", "`meta:paralaje_baja`", var(v=[1]), objetivo=-20)
def paralaje_baja(ctx, v):
    lz = Lienzo(2.1)
    for k, semi in enumerate([1, 6]):
        c = copa_frotada(hz("D5") * 2 ** (semi / 12), 0.9, ctx.hijo(f"d{k}"))
        lz.add(N(c) * env_de(c, [(0, 1), (0.9, 0)]), 0, -8)
    for k, semi in enumerate([0, 4, 7]):
        lz.add(N(copa_frotada(hz("D5") * 2 ** (semi / 12), 1.4, ctx.hijo(f"c{k}"))), 0.6, -4 - k)
    from sintesis import burbujeo
    lz.add(N(burbujeo(1.2, ctx.hijo("b"), 40, 400, 2000)), 0.3, -14)
    return _entrada_lenta(lz.senal(), 0.2, 0.4)
