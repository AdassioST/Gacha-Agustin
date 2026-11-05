# -*- coding: utf-8 -*-
"""3.5.8 Pociones (Salmueras)."""
import numpy as np

from sintesis import (SR, Lienzo, metal, cuenco, copa_frotada, corcho, burbuja, burbujeo, gota, chapoteo, crujido_sal,
                      tic_reloj, golpe_sordo, whoosh, blanco, bp, hp, lp, env_de, exp_de, env_campana, grado, hz, sala,
                      fuente, tramo, n_, tiempo, am_aleatoria, modal)
from . import receta, var
from .comun import N, destello

S = "pociones"


@receta("sfx_pocion_cinturon", S, "Tocar una ranura del cinturón: cuero y el vidrio del frasco que tintinea",
        "`ui:cinturon`", var(v=[1, 2]), nivel="A", objetivo=-25)
def cinturon(ctx, v):
    lz = Lienzo(0.35)
    lz.add(tramo(fuente("kenney_rpg_audio/belt_handle_%d.ogg" % v, velocidad=1.1, hp_hz=300), 0.22), 0, -4)
    f = ctx.u(2600, 3200)
    lz.add(N(modal(0.15, [f, f * 2.3, f * 3.9], [1, 0.5, 0.3], [0.08, 0.05, 0.03], ctx)), 0.04, -10)
    return lz.senal()


@receta("sfx_pocion_destapar", S, "Corcho que sale y oleaje según la base: agua termal (limpia y burbujeante), vinagre "
        "(efervescente y ácido) o leche lunar (espesa, con un brillo de plata)", "`ActionUsed` (Poción)",
        var(base=["agua_termal", "vinagre", "leche_lunar"]), nivel="A", objetivo=-21)
def destapar(ctx, v):
    b = v["base"]
    lz = Lienzo(0.8)
    lz.add(N(corcho(ctx.hijo("c"))), 0, -2)
    if b == "agua_termal":
        lz.add(N(burbujeo(0.4, ctx.hijo("b"), 90, 700, 2600)), 0.12, -8)
    elif b == "vinagre":
        fz = hp(blanco(0.5, ctx.hijo("f")), 4000) * am_aleatoria(0.5, ctx.hijo("am"), 20, 80, 0.8)
        lz.add(N(fz) * env_de(fz, [(0, 0), (0.03, 1), (0.5, 0)]), 0.1, -10)
        lz.add(N(burbujeo(0.35, ctx.hijo("b"), 200, 1800, 5000)), 0.1, -10)
    else:
        lz.add(N(burbujeo(0.4, ctx.hijo("b"), 40, 250, 900, xi=0.04)), 0.12, -8)
        lz.add(N(metal("plata", grado(0, 6), 0.5, ctx.hijo("p"), t60=0.6, batido=0.8, golpe=0.2)), 0.14, -12)
    return lz.senal()


@receta("sfx_pocion_beber", S, "Un trago corto: dos burbujas graves del líquido en el frasco, sin boca ni respiración",
        "`ActionUsed` (Poción)", var(v=[1, 2]), nivel="A", objetivo=-22)
def beber(ctx, v):
    lz = Lienzo(0.5)
    for k in range(2 + (v == 2)):
        lz.add(burbuja(ctx.u(240, 340), ctx.hijo(f"b{k}"), 1.0, 0.04), 0.03 + 0.11 * k)
        lz.add(N(lp(blanco(0.08, ctx.hijo(f"l{k}")), 1400) * exp_de(np.zeros(n_(0.08)), 0.06, 0.005)), 0.03 + 0.11 * k,
               -14)
    return lz.senal()


EFECTOS = ["cura", "escudo", "velocidad", "retraso", "anima", "salinidad"]


@receta("sfx_pocion_efecto", S, "Brillo según el efecto: cura (cuencos de cobre), escudo (cúpula de plata), velocidad "
        "(gotas de azogue que suben), retraso (reloj de plomo que se frena), Ánima (campanas que suben), Salinidad "
        "(sal que se disuelve)", "`ActionUsed` (efecto de la poción)", var(efecto=EFECTOS), nivel="A", objetivo=-19)
def efecto(ctx, v):
    e = v["efecto"]
    lz = Lienzo(1.1)
    if e == "cura":
        for k, g in enumerate([2, 4, 7]):
            lz.add(N(cuenco(grado(g, 4), 0.9, ctx.hijo(f"c{k}"), t60=1.5)), 0.08 * k, -3 - k)
    elif e == "escudo":
        lz.add(N(copa_frotada(grado(0, 5), 0.8, ctx.hijo("c"))), 0, -4)
        lz.add(N(metal("plata", grado(7, 5), 0.7, ctx.hijo("p"), t60=0.9, batido=0.9)), 0.2, -4)
    elif e == "velocidad":
        for k in range(6):
            lz.add(N(metal("azogue", grado(1 + k, 5), 0.3, ctx.hijo(f"a{k}"), t60=0.3, golpe=0.3)), 0.05 * k, -4)
        lz.add(N(whoosh(0.35, ctx.hijo("w"), 800, 5000, q=1.5, pico=0.7)), 0, -10)
    elif e == "retraso":
        t, paso = 0.0, 0.12
        for k in range(5):
            lz.add(N(tic_reloj(ctx.hijo(f"t{k}"), 0.8 - 0.04 * k)), t, -3 - k)
            t += paso
            paso *= 1.3
        lz.add(N(metal("plomo", hz("C3"), 0.5, ctx.hijo("p"), t60=0.5)), 0, -6)
    elif e == "anima":
        for k, g in enumerate([0, 2, 4, 7]):
            lz.add(N(metal("oro", grado(g, 5), 0.6, ctx.hijo(f"o{k}"), t60=0.7)), 0.07 * k, -4 - k)
        lz.add(N(destello(ctx.hijo("d"), 0.5, 5000, 4)), 0.2, -12)
    else:
        lz.add(N(burbujeo(0.7, ctx.hijo("b"), 250, 2000, 7000, tasa_env=lambda t: np.exp(-t / 0.3), xi=0.2)), 0, -4)
        for k, g in enumerate([4, 2, 0]):
            lz.add(N(metal("plata", grado(g, 5), 0.4, ctx.hijo(f"p{k}"), t60=0.5, golpe=0.2)), 0.05 + 0.1 * k, -8)
    return sala(lz.senal(), 0.12, 1.0)


@receta("sfx_pocion_turbia", S, "Salmuera Turbia con efecto aleatorio: burbujeo raro, burbujas que suben y bajan de "
        "tono y un 'plop' final", "`ActionUsed` (Salmuera Turbia)", var(v=[1]), nivel="A", objetivo=-20)
def turbia(ctx, v):
    lz = Lienzo(1.1)
    for k in range(9):
        f0 = ctx.u(200, 900)
        xi = ctx.u(-0.25, 0.6)
        lz.add(burbuja(f0, ctx.hijo(f"b{k}"), ctx.u(0.5, 1.0), xi), ctx.u(0, 0.6))
    t = tiempo(0.25)
    f = 500 * (1 + 0.8 * np.sin(2 * np.pi * 5 * t)) if v == 1 else 300 + 600 * t / 0.25
    gl = np.sin(2 * np.pi * np.cumsum(f) / SR) * env_campana(0.25, 0.3)
    lz.add(N(gl), 0.55, -10)
    lz.add(burbuja(180, ctx.hijo("plop"), 1.0, 0.02), 0.85)
    return lz.senal()
