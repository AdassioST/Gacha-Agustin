# -*- coding: utf-8 -*-
"""3.5.13 Almitas: el campanilleo de las almas (efecto firma, GDD 20.4)."""
import numpy as np

from sintesis import (SR, Lienzo, metal, cuenco, copa_frotada, cuerda, whoosh, gota, blanco, bp, lp, env_de, exp_de,
                      env_campana, grado, hz, sala, n_, RE_MAYOR)
from . import receta, var
from .comun import N, destello

S = "almitas"
PENTA = [0, 2, 4, 7, 9]  # Re mayor pentatónico: Re, Mi, Fa#, La, Si


def _campanilla(ctx, g, oct_=6, nivel=0.0, dur=0.45):
    o, k = divmod(g, 5)
    f = grado(0, oct_) * 2 ** ((PENTA[k] + 12 * o) / 12)
    x = metal("plata", f, dur, ctx, t60=0.4, golpe=0.5) + 0.3 * metal("oro", f * 2, dur, ctx.hijo("o"), t60=0.25)
    return N(x) * 10 ** (nivel / 20)


@receta("sfx_almita_soltar", S, "Algo muere o se rompe y suelta almitas: campanilleo agudo y breve",
        "`UnitDefeated` (enemigo) o `deriv:almitas_soltadas`, `f2:`", var(v=[1, 2, 3, 4]), nivel="A", objetivo=-24)
def soltar(ctx, v):
    lz = Lienzo(0.6)
    for k in range(3 + v % 2):
        lz.add(_campanilla(ctx.hijo(f"c{k}"), ctx.i(0, 7)), ctx.u(0, 0.15), ctx.u(-6, 0))
    return lz.senal()


@receta("sfx_almita_recoger", S, "Recoger almitas: arpegio pentatónico que crece con la cantidad (pocas, varias, muchas)",
        "`ui:almitas_recogidas`", var(cantidad=["pocas", "varias", "muchas"]), objetivo=-21)
def recoger(ctx, v):
    n = {"pocas": 3, "varias": 5, "muchas": 8}[v["cantidad"]]
    lz = Lienzo(1.1)
    for k in range(n):
        lz.add(_campanilla(ctx.hijo(f"c{k}"), k, 5 if n < 8 else 5), 0.055 * k, -1 - 0.3 * k)
    return lz.senal()


@receta("sfx_almita_nombre", S, "Almita con nombre que carga un recuerdo: campanilla lenta sobre un salterio lejano",
        "`f2:almita_nombre`", var(v=[1]), objetivo=-20)
def nombre(ctx, v):
    lz = Lienzo(1.6)
    for k, g in enumerate([4, 3, 0]):
        lz.add(N(cuerda(hz("D4") * 2 ** (PENTA[g] / 12), 1.2, ctx.hijo(f"s{k}"), "salterio", 0.7)), 0.25 * k, -8)
    lz.add(_campanilla(ctx.hijo("c"), 7, 6, 0, 0.9), 0.05)
    return sala(lz.senal(), 0.25, 0.8)


@receta("sfx_almita_spark", S, "Almita extra del Spark: una sola campanilla brillante encima de la campana",
        "`HitLanded` (`IsSpark`), capa", var(v=[1, 2]), nivel="A", objetivo=-22)
def almita_spark(ctx, v):
    return _campanilla(ctx, 8 + v, 6)


@receta("sfx_almita_ascendente", S, "El Ascendente come almitas al subir de Dignidad: campanillas que se recogen al "
        "revés hacia un cuenco grave", "`f2:dignidad`", var(v=[1]), objetivo=-20)
def ascendente(ctx, v):
    lz = Lienzo(1.2)
    tinte = Lienzo(0.6)
    for k in range(5):
        tinte.add(_campanilla(ctx.hijo(f"c{k}"), ctx.i(0, 9)), ctx.u(0, 0.2), ctx.u(-6, 0))
    from sintesis import fundido
    x = fundido(tinte.senal()[::-1].copy(), 0.0, 0.02)
    lz.add(N(x), 0, -2)
    lz.add(N(whoosh(0.5, ctx.hijo("w"), 4000, 500, q=1.2, pico=0.8)), 0.1, -10)
    lz.add(N(cuenco(hz("D3") if v == 1 else hz("A2"), 0.8, ctx.hijo("b"), t60=1.4)), len(x) / SR, -4)
    return lz.senal()


@receta("sfx_almita_flotar", S, "Almitas que flotan hacia el Fresno (bucle): campanillas sueltas y aire",
        "Escena de exploración", var(v=[1]), objetivo=-32, metrica="I", bucle=True, estado="borrador")
def flotar(ctx, v):
    from proceso import bucle
    d = 2.0
    lz = Lienzo(d + 0.6)
    t = 0.1
    k = 0
    while t < d + 0.2:
        lz.add(_campanilla(ctx.hijo(f"c{k}"), ctx.i(0, 9), 6, ctx.u(-12, -4)), t)
        t += ctx.u(0.25, 0.6)
        k += 1
    lz.add(N(bp(blanco(d + 0.6, ctx.hijo("a")), 2000, 8000)) * 0.3, 0, -18)
    return bucle(lz.senal()[: n_(d + 0.6)], 0.6)
