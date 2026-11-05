# -*- coding: utf-8 -*-
"""3.5.6 Aspectos y Reacciones Híbridas. Los Aspectos son intervalos musicales en la tonalidad del combate (Re)."""
import numpy as np

from sintesis import (SR, Lienzo, metal, cuerda, cuenco, copa_frotada, crujido_sal, fractura, _granos, burbujeo,
                      grito_estano, estatica, trueno_lamina, raspado, cadena_hierro, tic_reloj, golpe_sordo, whoosh,
                      fuego, blanco, bp, hp, lp, env_de, exp_de, grado, hz, sala, n_, tiempo, cambiar_velocidad, modal,
                      crepitar)
from . import receta, var
from .comun import N, destello, impacto

S = "aspectos"


def _intervalo(ctx, f1, f2, t2=0.0, dur=0.9, rugoso=0.0):
    """Díada de salterio y campana de plata: el timbre neutro de los Aspectos."""
    lz = Lienzo(dur + t2 + 0.1)
    for k, (f, t) in enumerate([(f1, 0.0), (f2, t2)]):
        lz.add(N(cuerda(f, dur, ctx.hijo(f"c{k}"), "salterio", 1.0)), t, -2)
        lz.add(N(metal("plata", f * 2, dur, ctx.hijo(f"p{k}"), t60=0.9, golpe=0.3)), t, -9)
    return lz.senal()


@receta("sfx_aspecto_conjuncion", S, "Conjunción: unísono reforzado. Dos Re que laten y se funden, con su octava",
        "`AspectFormed` (Conjuncion)", var(v=[1]), nivel="A", objetivo=-18)
def conjuncion(ctx, v):
    f = grado(0 if v == 1 else 4, 5)
    lz = Lienzo(1.1)
    t = tiempo(0.9)
    batido = 1 + 0.006 * np.exp(-t / 0.25)
    lz.add(N(cuerda(f, 0.9, ctx.hijo("a"), "salterio")), 0, -2)
    y = np.sin(2 * np.pi * np.cumsum(f * batido) / SR) * np.exp(-t / 0.4)
    lz.add(N(y), 0.0, -8)
    lz.add(N(metal("oro", f * 2, 0.9, ctx.hijo("o"), t60=1.0)), 0.06, -5)
    return lz.senal()


@receta("sfx_aspecto_oposicion", S, "Oposición: una octava (Re4 y Re5) que se quiebra antes de la Reacción: la nota de "
        "arriba se desafina y cruje", "`AspectFormed` (Oposicion)", var(v=[1]), nivel="A", objetivo=-18)
def oposicion(ctx, v):
    f = grado(0, 4)
    lz = Lienzo(1.1)
    lz.add(N(cuerda(f, 0.9, ctx.hijo("a"), "salterio")), 0, -2)
    t = tiempo(0.8)
    caida = 1 - 0.03 * np.clip((t - 0.25) / 0.3, 0, 1)
    arriba = np.sin(2 * np.pi * np.cumsum(2 * f * caida) / SR) * np.exp(-t / 0.5)
    lz.add(N(arriba), 0.0, -5)
    lz.add(N(metal("plata", f * 4, 0.6, ctx.hijo("p"), t60=0.5)), 0, -10)
    lz.add(N(fractura(ctx.hijo("f"), 1.0)), 0.3 + 0.05 * v, -8)
    lz.add(N(crujido_sal(ctx.hijo("s"), 0.25, 0.75, fracturas=1)), 0.32 + 0.05 * v, -12)
    return lz.senal()


@receta("sfx_aspecto_sextil", S, "Sextil: tercera mayor rápida que adelanta (Fa a La, o Si♭ a Re)",
        "`AspectFormed` (Sextil)", var(v=[1]), nivel="A", objetivo=-18)
def sextil(ctx, v):
    f1, f2 = (grado(2, 5), grado(4, 5)) if v == 1 else (grado(5, 4), grado(7, 4))
    lz = Lienzo(1.0)
    lz.add(_intervalo(ctx, f1, f2, 0.07, 0.8), 0)
    lz.add(N(whoosh(0.15, ctx.hijo("w"), 1500, 5000, q=2.0, pico=0.8)), 0.0, -14)
    return lz.senal()


@receta("sfx_aspecto_trigono", S, "Trígono: quinta justa cálida (Re y La) y un grano de sal", "`AspectFormed` (Trigono)",
        var(v=[1]), nivel="A", objetivo=-18)
def trigono(ctx, v):
    f = grado(0, 4 if v == 1 else 5)
    lz = Lienzo(1.2)
    lz.add(N(cuenco(f, 1.0, ctx.hijo("c1"), t60=1.8)), 0, -4)
    lz.add(N(cuenco(f * 1.5, 1.0, ctx.hijo("c2"), t60=1.8)), 0.01, -5)
    lz.add(_intervalo(ctx, f * 2, f * 3, 0.0, 0.8), 0.0, -4)
    g = _granos(0.05, ctx.hijo("g"), lambda t: 400, 3000, 10000, ping=0.6)
    lz.add(N(g), 0.18, -10)
    return lz.senal()


@receta("sfx_aspecto_cuadratura", S, "Cuadratura: segunda menor áspera (La y Si♭, o Mi y Fa) en hierro, con roce",
        "`AspectFormed` (Cuadratura)", var(v=[1]), nivel="A", objetivo=-18)
def cuadratura(ctx, v):
    f1, f2 = (grado(4, 4), grado(5, 4)) if v == 1 else (grado(1, 5), grado(2, 5))
    lz = Lienzo(1.0)
    lz.add(_intervalo(ctx, f1, f2, 0.0, 0.8), 0)
    lz.add(N(metal("hierro", f1, 0.6, ctx.hijo("h1"), t60=0.6)), 0, -6)
    lz.add(N(metal("hierro", f2, 0.6, ctx.hijo("h2"), t60=0.6)), 0.0, -6)
    lz.add(N(raspado(0.3, ctx.hijo("r"), 1.2)), 0.02, -14)
    return lz.senal()


@receta("sfx_reaccion_eclipse", S, "Eclipse (Sol con Saturno): un acorde de oro brillante que el plomo se traga, y "
        "200 ms de silencio antes de un último tic grave", "`ReactionTriggered` (Eclipse) o `AspectFormed` con "
        "reacción", var(v=[1]), nivel="A", objetivo=-15)
def eclipse(ctx, v):
    lz = Lienzo(1.6)
    oro = Lienzo(0.75)
    for k, semi in enumerate([0, 4, 7, 12]):
        oro.add(N(metal("oro", grado(3, 5) * 2 ** (semi / 12), 0.75, ctx.hijo(f"o{k}"), t60=1.2)), 0.01 * k, -2)
    x = oro.senal()
    tt = tiempo(len(x) / SR)
    # el plomo se traga el brillo: paso bajo que se cierra y volumen que cae a cero en 0.62 s
    from sintesis import filtro_variable
    x = filtro_variable(x, lambda f, t: 1.0 / (1.0 + (f / np.maximum(6000 * np.exp(-t / 0.12), 120)) ** 4))
    x = x * env_de(x, [(0, 1), (0.3, 0.8), (0.5, 0.0), (0.75, 0.0)])
    lz.add(N(x), 0, -2)
    lz.add(N(golpe_sordo(ctx.hijo("g"), 90, 45, 0.2, 0.5)), 0.32, -3)
    lz.add(N(metal("plomo", hz("C3"), 0.22, ctx.hijo("p"), t60=0.15)), 0.32, -5)
    # 200 ms de silencio (0.55 a 0.75 s) y un tic grave de plomo
    lz.add(N(tic_reloj(ctx.hijo("t"), 0.7)), 0.76, -4)
    return lz.senal()


@receta("sfx_reaccion_helada_negra", S, "Helada Negra (Luna con Saturno): escarcha en cámara lenta, crujidos de hielo "
        "estirados, plata que baja y plomo grave", "`ReactionTriggered` (HeladaNegra)", var(v=[1]), nivel="A",
        objetivo=-15)
def helada_negra(ctx, v):
    lz = Lienzo(2.0)
    hielo = crujido_sal(ctx.hijo("h"), 0.5, 0.95, granos=2.0, fracturas=3)
    lz.add(N(cambiar_velocidad(hielo, 0.35)), 0, -2)
    t = tiempo(1.4)
    f = grado(0, 6) * (1 - 0.18 * t / 1.4)
    plata = (np.sin(2 * np.pi * np.cumsum(f) / SR) + 0.2 * np.sin(4 * np.pi * np.cumsum(f) / SR)) * np.exp(-t / 0.7)
    lz.add(N(plata), 0.05, -8)
    lz.add(N(metal("plomo", hz("D3"), 0.8, ctx.hijo("p"), t60=0.8)), 0.0, -6)
    lz.add(N(copa_frotada(grado(0, 5) * (1 if v == 1 else 0.94), 1.4, ctx.hijo("c"))), 0.1, -12)
    return sala(lz.senal(), 0.15, 1.3)


@receta("sfx_reaccion_espinas_rojas", S, "Espinas Rojas (Marte con Venus): espinas de hierro que brotan y un cuenco de "
        "cobre que cura", "`ReactionTriggered` (EspinasRojas)", var(v=[1]), nivel="A", objetivo=-15)
def espinas_rojas(ctx, v):
    lz = Lienzo(1.8)
    for k in range(5):
        lz.add(N(metal("hierro", grado(4 + k % 3, 5) * ctx.u(0.98, 1.02), 0.4, ctx.hijo(f"e{k}"), t60=0.35,
                       brillo=1.3)), 0.045 * k, -3 - k * 0.5)
        lz.add(N(raspado(0.08, ctx.hijo(f"r{k}"), 2.0)), 0.045 * k, -12)
    lz.add(N(cuenco(grado(2, 4), 1.3, ctx.hijo("c"), t60=2.0)), 0.35, -3)
    lz.add(N(cuenco(grado(4, 4), 1.3, ctx.hijo("c2"), t60=2.0)), 0.38 + 0.03 * v, -7)
    return sala(lz.senal(), 0.12, 1.1)


@receta("sfx_reaccion_tempestad", S, "Tempestad (Mercurio con Júpiter): rayo en cadena hecho de estaño y azogue: grito "
        "del estaño, crepitar, lámina de trueno y gotas rápidas. Cada golpe extra usa su propio `HitLanded`",
        "`ReactionTriggered` (Tempestad)", var(v=[1]), nivel="A", objetivo=-15)
def tempestad(ctx, v):
    lz = Lienzo(1.6)
    for k in range(3):
        lz.add(N(grito_estano(0.2, ctx.hijo(f"g{k}"), 1500)), 0.12 * k, -2 - k)
        lz.add(N(crepitar(0.22, ctx.hijo(f"c{k}"), 300, 1800, 9000)), 0.12 * k, -6)
    lz.add(N(trueno_lamina(1.2, ctx.hijo("t"), 1.0, afinado=grado(5, 3))), 0.1, -3)
    for k in range(6):
        lz.add(N(metal("azogue", grado(1 + k, 5 + (v - 1)), 0.3, ctx.hijo(f"a{k}"), t60=0.3, golpe=0.3)), 0.05 + 0.04 * k,
               -10)
    return lz.senal()
