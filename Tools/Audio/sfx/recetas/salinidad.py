# -*- coding: utf-8 -*-
"""3.5.4 Salinidad por banda."""
import numpy as np

from sintesis import (Lienzo, metal, crujido_sal, fractura, _granos, cuenco, copa_frotada, burbujeo, gota, dron,
                      susurro, crujido_madera, golpe_sordo, whoosh, blanco, rosa, bp, hp, lp, env_de, exp_de,
                      env_campana, grado, hz, sala, n_, tiempo, cambiar_velocidad, coro, fuente, tramo)
from . import receta, var
from .comun import N, destello, swell, impacto

S = "salinidad"


def _crecer(ctx, dur, tasa=200, grave=0.0, pureza=0.8):
    """Cristales que crecen: crujidos dispersos de sal, a veces bajados de octava."""
    lz = Lienzo(dur + 0.4)
    t = ctx.u(0.0, 0.05)
    k = 0
    while t < dur:
        c = crujido_sal(ctx.hijo(f"c{k}"), ctx.u(0.06, 0.14), pureza, granos=0.5, fracturas=1, grave=grave)
        lz.add(N(c), t, ctx.u(-10, -2))
        t += ctx.rng.exponential(1.0 / (tasa / 60.0))
        k += 1
    return lz.senal()


@receta("sfx_salinidad_baja", S, "Baja de banda: alivio, la sal se disuelve en burbujas finas y una nota de cristal que "
        "desciende", "`SalinityBandChanged` (banda nueva menor)", var(v=[1]), nivel="A", objetivo=-20)
def baja(ctx, v):
    lz = Lienzo(1.0)
    lz.add(N(burbujeo(0.7, ctx.hijo("b"), 220, 2000, 7000, tasa_env=lambda t: np.exp(-t / 0.35), xi=0.2)), 0, -4)
    for k, g in enumerate([7, 4, 2] if v == 1 else [9, 7, 4]):
        lz.add(N(metal("plata", grado(g, 5), 0.5, ctx.hijo(f"n{k}"), t60=0.6, golpe=0.2)), 0.05 + 0.12 * k, -6 - 2 * k)
    return sala(lz.senal(), 0.12, 0.9)


@receta("sfx_salinidad_salobre", S, "Entra a Salobre (40 a 69): costra fina que cruje y un pulso suave",
        "`SalinityBandChanged` (Salobre)", var(v=[1]), nivel="A", objetivo=-20)
def salobre(ctx, v):
    lz = Lienzo(0.9)
    lz.add(_crecer(ctx.hijo("c"), 0.5, 360), 0, -2)
    lz.add(N(golpe_sordo(ctx.hijo("g"), 90, 60, 0.3, 0.0)), 0.0, -14)
    lz.add(N(metal("plata", grado(1, 5), 0.5, ctx.hijo("p"), t60=0.5, golpe=0.1)), 0.05, -12)
    return lz.senal()


@receta("sfx_salinidad_sobresalada", S, "Entra a Sobresalada (70 a 99): costra más densa y una segunda menor tensa en "
        "plata", "`SalinityBandChanged` (Sobresalada)", var(v=[1]), nivel="A", objetivo=-19)
def sobresalada(ctx, v):
    lz = Lienzo(1.1)
    lz.add(_crecer(ctx.hijo("c"), 0.7, 600, 0.0, 0.78), 0, -2)
    lz.add(N(copa_frotada(grado(4, 5), 0.9, ctx.hijo("a"))), 0.05, -8)
    lz.add(N(copa_frotada(grado(5, 5), 0.9, ctx.hijo("b"))), 0.12, -10)
    return lz.senal()


@receta("sfx_salinidad_cristalizacion", S, "Entra a Cristalización Turbia (100 a 149): crujidos lentos de sal (una y dos "
        "octavas abajo), cristal que se tensa y un pedal grave", "`SalinityBandChanged` (CristalizacionTurbia)",
        var(v=[1]), nivel="A", objetivo=-18)
def cristalizacion(ctx, v):
    lz = Lienzo(1.7)
    lz.add(_crecer(ctx.hijo("c"), 1.0, 360, 1.0, 0.75), 0, -2)
    lz.add(N(crujido_sal(ctx.hijo("g"), 0.3, 0.75, fracturas=2, grave=2.0)), 0.2, -4)
    lz.add(N(dron(1.5, ctx.hijo("d"), hz("D1") * (1 if v == 1 else 1.06), (2, 3, 4, 5), 0.5)) *
           env_de(np.zeros(n_(1.5)), [(0, 0), (0.4, 1), (1.5, 0)]), 0, -10)
    return lz.senal()


@receta("sfx_salinidad_cristal_bucle", S, "Bucle de cristales que crecen mientras la bruja siga en Cristalización Turbia",
        "Mismo evento; se detiene al bajar de banda", var(v=[1]), nivel="A", objetivo=-30, metrica="I", bucle=True)
def cristal_bucle(ctx, v):
    from proceso import bucle
    d = 2.0
    lz = Lienzo(d + 0.5)
    lz.add(_crecer(ctx.hijo("c"), d + 0.3, 70, 1.0, 0.75), 0, 0)
    lz.add(N(lp(rosa(d + 0.5, ctx.hijo("r")), 600)) * 0.5, 0, -18)
    x = lz.senal()[: n_(d + 0.5)]
    return bucle(x, 0.5)


@receta("sfx_salinidad_perdida_vida", S, "Pierde 5 % de vida por turno en Cristalización: grieta de cristal y un pulso "
        "sordo", "`TurnStarted` (bruja en Cristalización Turbia)", var(v=[1]), nivel="A", objetivo=-20)
def perdida_vida(ctx, v):
    lz = Lienzo(0.6)
    lz.add(N(fractura(ctx.hijo("f"), 1.0)), 0, -2)
    lz.add(N(crujido_sal(ctx.hijo("c"), 0.2, 0.75, fracturas=1, grave=1.0)), 0.02, -6)
    lz.add(N(golpe_sordo(ctx.hijo("g"), 80, 50, 0.35, 0.0)), 0.0, -8)
    return lz.senal()


@receta("sfx_salinidad_extasis", S, "Éxtasis Abisal: el Ascendente toma el control. Crece un pedal grave, el cuenco "
        "gime, susurros sin palabras al revés y la sal estalla; sube, no salta", "`SalinityBandChanged` (ExtasisAbisal) "
        "o `EcstasyStarted`", var(v=[1]), nivel="A", objetivo=-14)
def extasis(ctx, v):
    d = 2.6
    lz = Lienzo(d + 0.6)
    lz.add(N(dron(d, ctx.hijo("d"), hz("D1"), (2, 3, 4, 6), 0.6)) * env_de(np.zeros(n_(d)), [(0, 0), (1.7, 1), (d, 0)]),
           0, -6)
    lz.add(N(cuenco(hz("D3"), 1.8, ctx.hijo("c"), frotado=True, gemido=1.0)), 0.2, -6)
    s = susurro(1.4, ctx.hijo("s"), 6, reves=True, registro=0.85)
    lz.add(N(s) * env_de(s, [(0, 0), (1.2, 1), (1.4, 0.2)]), 0.3, -12)
    lz.add(N(swell(1.6, ctx.hijo("sw"), 150, 3000, 0.9)), 0.1, -8)
    t = 1.75
    lz.add(N(impacto(ctx.hijo("i"), 1.0)), t, -3)
    lz.add(N(crujido_sal(ctx.hijo("x"), 0.4, 0.72, fracturas=3, grave=1.0)), t, -2)
    lz.add(N(metal("hierro" if v == 1 else "plomo", hz("D3"), 1.0, ctx.hijo("m"), t60=1.0)), t, -6)
    return sala(lz.senal(), 0.18, 1.3)


@receta("sfx_salinidad_desalada", S, "Sale del Éxtasis y queda Desalada: barrido largo que baja, la sal se disuelve y "
        "una campana suave", "`SalinityBandChanged` (de ExtasisAbisal a 60) o `EcstasyEnded`", var(v=[1]),
        nivel="A", objetivo=-19)
def desalada(ctx, v):
    lz = Lienzo(1.5)
    lz.add(N(whoosh(1.0, ctx.hijo("w"), 2500, 250, q=1.0, pico=0.25)), 0, -6)
    lz.add(N(burbujeo(0.9, ctx.hijo("b"), 120, 1500, 6000, tasa_env=lambda t: np.exp(-t / 0.4), xi=0.2)), 0.1, -8)
    lz.add(N(cuenco(grado(0 if v == 1 else 4, 4), 1.2, ctx.hijo("c"), t60=2.0)), 0.25, -6)
    return sala(lz.senal(), 0.15, 1.1)
