# -*- coding: utf-8 -*-
"""3.5.5 Quiebre por planeta. `sfx_quiebre_firmeza` suena primero; después la capa del planeta."""
import numpy as np

from sintesis import (SR, Lienzo, metal, crujido_sal, fractura, cuenco, copa_frotada, burbujeo, gota, fuego, crepitar,
                      grito_estano, estatica, trueno_lamina, raspado, cadena_hierro, tic_reloj, golpe_sordo, whoosh,
                      blanco, bp, hp, lp, env_de, exp_de, env_campana, grado, hz, sala, fuente, tramo, n_, tiempo,
                      cambiar_velocidad, modal)
from . import receta, var
from .comun import N, destello, f_planeta, impacto

S = "quiebre"


@receta("sfx_quiebre_firmeza", S, "La Firmeza llega a cero: una placa de sal que se rompe con una grieta dorada "
        "(varias fracturas, cristal que estalla y un brillo de oro que sube)", "`BreakTriggered` (siempre primero)",
        var(v=[1, 2, 3]), nivel="A", objetivo=-14)
def firmeza(ctx, v):
    lz = Lienzo(1.1)
    lz.add(N(impacto(ctx.hijo("i"), 0.8)), 0, -4)
    for k in range(4):
        lz.add(N(fractura(ctx.hijo(f"f{k}"), 1.0)), 0.025 * k * ctx.u(0.7, 1.3), -2 - 2 * k)
    lz.add(fuente("kenney_impact_sounds/impact_glass_heavy_00%d.ogg" % [1, 4, 1][v - 1], velocidad=[0.8, 0.9, 0.7][v - 1],
                  hp_hz=150), 0.01, -5)
    lz.add(N(crujido_sal(ctx.hijo("s"), 0.45, 0.85, granos=1.5, fracturas=0)), 0.05, -8)
    for k, g in enumerate([3, 7, 10]):  # grieta dorada: Sol, Re y Fa agudos que suben
        lz.add(N(metal("oro", grado(g, 5), 0.7, ctx.hijo(f"o{k}"), t60=0.8)), 0.08 + 0.05 * k, -8 - k)
    return lz.senal()


@receta("sfx_quiebre_sol", S, "Quemadura: llamarada que sube y un siseo con chasquidos", "`BreakTriggered` (Sol)",
        var(v=[1]), nivel="A", objetivo=-17)
def q_sol(ctx, v):
    lz = Lienzo(1.1)
    lz.add(N(whoosh(0.6, ctx.hijo("w"), 200, 2000, q=0.9, pico=0.2)), 0, -2)
    f = fuego(1.0, ctx.hijo("f"), crepita=1.5)
    lz.add(N(f) * env_de(f, [(0, 0), (0.05, 1), (1.0, 0)]), 0, -2)
    lz.add(N(hp(blanco(0.6, ctx.hijo("s")), 3000)) * env_de(np.zeros(n_(0.6)), [(0, 0), (0.05, 1), (0.6, 0)]), 0.05, -12)
    return lz.senal()


@receta("sfx_quiebre_luna", S, "Encierro: jaula de plata que se cierra (cerrojo y barrotes) con crótalos",
        "`BreakTriggered` (Luna)", var(v=[1]), nivel="A", objetivo=-17)
def q_luna(ctx, v):
    lz = Lienzo(1.2)
    lz.add(N(whoosh(0.25, ctx.hijo("w"), 3000, 800, q=1.4, pico=0.8)), 0, -8)
    lz.add(tramo(fuente("kenney_rpg_audio/metal_latch.ogg", velocidad=0.85, hp_hz=300), 0.25), 0.2, -4)
    for k in range(4):
        lz.add(N(metal("plata", grado(k * 2, 5) * (1 if v == 1 else 1.5), 0.6, ctx.hijo(f"b{k}"), t60=0.6, golpe=0.8)),
               0.2 + 0.03 * k, -6 - k)
    lz.add(N(metal("plata", grado(7, 6), 0.9, ctx.hijo("c"), t60=1.2, batido=0.8)), 0.24, -8)
    return lz.senal()


@receta("sfx_quiebre_marte", S, "Sangrado fuerte: cadena de hierro que se tensa y óxido raspado, sin chapoteo",
        "`BreakTriggered` (Marte)", var(v=[1]), nivel="A", objetivo=-17)
def q_marte(ctx, v):
    lz = Lienzo(1.0)
    lz.add(N(cadena_hierro(0.5, ctx.hijo("c"), 14, 2400)), 0, -2)
    lz.add(N(raspado(0.5, ctx.hijo("r"), 1.6)), 0.15, -5)
    lz.add(N(metal("hierro", hz("A3"), 0.6, ctx.hijo("m"), t60=0.6)), 0.0, -6)
    return lz.senal()


@receta("sfx_quiebre_mercurio", S, "Veneno y retraso: azogue que burbujea, pesado y metálico, y gotas que resbalan",
        "`BreakTriggered` (Mercurio)", var(v=[1]), nivel="A", objetivo=-17)
def q_mercurio(ctx, v):
    lz = Lienzo(1.1)
    lz.add(N(burbujeo(0.8, ctx.hijo("b"), 140, 300, 1600, tasa_env=lambda t: np.exp(-t / 0.5), xi=0.04)), 0, -2)
    for k in range(5):
        lz.add(N(metal("azogue", grado(1 + 2 * k, 5) * (1 if v == 1 else 0.75), 0.4, ctx.hijo(f"g{k}"), t60=0.4,
                       golpe=0.3)), 0.05 + 0.09 * k, -8 - k)
    return lz.senal()


@receta("sfx_quiebre_venus", S, "Encanto: acorde suave de cuencos de cobre (Fa, La, Do)", "`BreakTriggered` (Venus)",
        var(v=[1]), nivel="A", objetivo=-17)
def q_venus(ctx, v):
    lz = Lienzo(1.4)
    for k, g in enumerate([2, 4, 6] if v == 1 else [2, 4, 7]):
        lz.add(N(cuenco(grado(g, 4), 1.3, ctx.hijo(f"c{k}"), t60=2.2)), 0.07 * k, -3 - k)
    lz.add(N(destello(ctx.hijo("d"), 0.5, 3000, 4)), 0.1, -14)
    return sala(lz.senal(), 0.15, 1.1)


@receta("sfx_quiebre_jupiter", S, "Electrocución que salta: grito del estaño, arcos que crepitan y una lámina de "
        "trueno. Los saltos son las chispas que van a cada enemigo cercano", "`BreakTriggered` (Júpiter); saltos con "
        "cada `HitLanded` del salto", var(tipo=["quiebre"], v=[1]) + var(tipo=["salto"], v=[1, 2, 3]), nivel="A",
        objetivo=lambda v: -17 if v["tipo"] == "quiebre" else -20)
def q_jupiter(ctx, v):
    if v["tipo"] == "salto":
        lz = Lienzo(0.35)
        lz.add(N(grito_estano(0.18, ctx.hijo("g"), 1500)), 0, -2)
        lz.add(N(crepitar(0.2, ctx.hijo("c"), 300, 2000, 9000)), 0, -6)
        lz.add(N(metal("estano", grado(5 + v["v"], 5), 0.3, ctx.hijo("m"), t60=0.3)), 0, -10)
        return lz.senal()
    lz = Lienzo(1.1)
    lz.add(N(grito_estano(0.4, ctx.hijo("g"), 1200)), 0, -2)
    lz.add(N(estatica(0.6, ctx.hijo("e"), 1.5)) * env_de(np.zeros(n_(0.6)), [(0, 1), (0.6, 0)]), 0.05, -6)
    lz.add(N(trueno_lamina(0.8, ctx.hijo("t"), afinado=grado(5, 3))), 0.08, -6)
    return lz.senal()


@receta("sfx_quiebre_saturno", S, "Gravedad: plomo que cae con todo su peso y un reloj que se frena",
        "`BreakTriggered` (Saturno)", var(v=[1]), nivel="A", objetivo=-17)
def q_saturno(ctx, v):
    lz = Lienzo(1.6)
    lz.add(N(whoosh(0.25, ctx.hijo("w"), 1200, 200, q=1.2, pico=0.7)), 0, -10)
    lz.add(N(golpe_sordo(ctx.hijo("g"), 80, 34, 0.6, 0.6)), 0.2, -1)
    lz.add(N(metal("plomo", hz("C3"), 0.5, ctx.hijo("p"), t60=0.5)), 0.2, -4)
    t = 0.45
    paso = 0.13
    for k in range(5):
        lz.add(N(tic_reloj(ctx.hijo(f"t{k}"), 0.85 - 0.05 * k)), t, -6 - 1.5 * k)
        t += paso
        paso *= 1.35
    return lz.senal()
