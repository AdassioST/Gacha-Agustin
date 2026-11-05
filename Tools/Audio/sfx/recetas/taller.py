# -*- coding: utf-8 -*-
"""3.5.12 Taller (crafting): Yunque Astral, Caldero de Bruja y Altar del Árbol."""
import numpy as np

from sintesis import (SR, Lienzo, metal, madera, cuenco, copa_frotada, crujido_sal, _granos, burbuja, burbujeo, gota,
                      chapoteo, hervor, fuego, golpe_sordo, chasquido, whoosh, engranes, tic_reloj, pluma, hoja,
                      blanco, rosa, bp, hp, lp, env_de, exp_de, env_campana, grado, hz, sala, fuente, tramo, n_,
                      tiempo, RE_MAYOR, PLANETAS)
from . import receta, var
from .comun import N, destello, swell, f_planeta, nota_planeta, crujido_planeta, impacto, PLANETAS_ORDEN

S = "taller"


@receta("sfx_taller_yunque", S, "Martillo del Yunque Astral: golpe de forja en Re, más brillante por nivel del arma "
        "(Rústico, Forjado, Alquímico, Astral, Quintaesencia)", "`f2:forja` (nivel 1 a 5)", var(nivel=[1, 2, 3, 4, 5]),
        objetivo=-17)
def yunque(ctx, v):
    n = v["nivel"]
    lz = Lienzo(0.9)
    lz.add(tramo(fuente("kenney_impact_sounds/impact_metal_heavy_003.ogg", velocidad=1.0, hp_hz=100), 0.3), 0, -6)
    lz.add(N(metal("hierro", hz("D4"), 0.7, ctx.hijo("h"), t60=0.5 + 0.1 * n, brillo=0.9 + 0.1 * n)), 0, -2)
    if n >= 3:
        lz.add(N(metal("oro", hz("A5"), 0.7, ctx.hijo("o"), t60=0.6)), 0.005, -14 + 2 * (n - 3))
    if n >= 4:
        lz.add(N(destello(ctx.hijo("d"), 0.5, 5000, 2 + n)), 0.02, -14)
    if n == 5:
        lz.add(N(copa_frotada(hz("D6"), 0.7, ctx.hijo("c"))), 0.0, -14)
    return lz.senal()


@receta("sfx_taller_reforja", S, "Reforja: el astrolabio gira, se frena y se detiene en un prefijo con un clic y una "
        "campanita", "`f2:reforja`", var(v=[1]), objetivo=-19)
def reforja(ctx, v):
    lz = Lienzo(1.6)
    t, paso, k = 0.0, 0.04, 0
    while t < 1.1:
        lz.add(N(tic_reloj(ctx.hijo(f"t{k}"), 1.2)), t, -6 - k * 0.15)
        t += paso
        paso *= 1.12
        k += 1
    lz.add(tramo(fuente("kenney_rpg_audio/metal_latch.ogg", velocidad=1.1, hp_hz=500), 0.2), t, -4)
    lz.add(N(metal("oro", grado(4 if v == 1 else 7, 5), 0.5, ctx.hijo("o"), t60=0.5)), t + 0.03, -6)
    return lz.senal()


@receta("sfx_taller_fusion", S, "Fusión legendaria: cuencos que giran, un brillo que sube y un acorde de campanas de Re "
        "mayor con un golpe", "`f2:fusion`", var(v=[1]), objetivo=-15)
def fusion(ctx, v):
    lz = Lienzo(2.5)
    lz.add(N(cuenco(hz("D3"), 1.4, ctx.hijo("c1"), frotado=True)), 0, -6)
    lz.add(N(cuenco(hz("A3"), 1.4, ctx.hijo("c2"), frotado=True)), 0.1, -8)
    lz.add(N(swell(1.1, ctx.hijo("s"), 300, 5000, 0.9)), 0.1, -6)
    t = 1.2
    lz.add(N(impacto(ctx.hijo("i"), 0.8)), t, -6)
    for k, n in enumerate(["D4", "F#4", "A4", "D5", "F#5"]):
        lz.add(N(metal("oro", hz(n), 1.2, ctx.hijo(f"o{k}"), t60=1.2)), t + 0.02 * k, -5 - k)
    lz.add(N(destello(ctx.hijo("d"), 0.9, 5000, 9)), t, -10)
    return sala(lz.senal(), 0.18, 0.8)


@receta("sfx_taller_caldero", S, "Hervor del Caldero de Bruja (bucle): burbujas graves y un fuego bajo",
        "Pantalla del Caldero", var(v=[1]), objetivo=-28, metrica="I", bucle=True, estado="borrador")
def caldero(ctx, v):
    from proceso import bucle
    d = 2.0
    lz = Lienzo(d)
    lz.add(N(hervor(d, ctx.hijo("h"), 0.7)), 0, -2)
    lz.add(N(fuego(d, ctx.hijo("f"), 0.4, 0.8)), 0, -12)
    return bucle(lz.senal()[: n_(d)], 0.6)


@receta("sfx_taller_ingrediente", S, "Echar un ingrediente al caldero: base (chorro), sal (granos), reactivo (gota que "
        "burbujea) o almitas (campanitas que se hunden)", "`ui:ingrediente`",
        var(ingrediente=["base", "sal", "reactivo", "almitas"]), objetivo=-22)
def ingrediente(ctx, v):
    i = v["ingrediente"]
    lz = Lienzo(0.75)
    if i == "base":
        lz.add(N(burbujeo(0.45, ctx.hijo("b"), 260, 400, 2500, xi=0.15)), 0, -2)
        lz.add(N(bp(blanco(0.45, ctx.hijo("c")), 800, 5000) * env_campana(0.45, 0.3)), 0, -10)
    elif i == "sal":
        g = _granos(0.4, ctx.hijo("g"), lambda t: 1500 * np.exp(-t / 0.2) + 50, 2500, 10000, ping=0.3)
        lz.add(N(g), 0, -2)
        lz.add(N(burbujeo(0.4, ctx.hijo("b"), 80, 1500, 5000)), 0.1, -10)
    elif i == "reactivo":
        lz.add(gota(900, ctx.hijo("g"), 1.0), 0, -2)
        lz.add(N(burbujeo(0.5, ctx.hijo("b"), 200, 1500, 6000, xi=0.25)), 0.05, -6)
    else:
        for k, g_ in enumerate([7, 9, 11]):
            lz.add(N(metal("plata", grado(g_, 5, RE_MAYOR), 0.4, ctx.hijo(f"a{k}"), t60=0.4, golpe=0.3)), 0.05 * k, -4)
        lz.add(gota(700, ctx.hijo("g"), 0.8), 0.18, -6)
    return lz.senal()


@receta("sfx_taller_salmuera_ok", S, "Salmuera lista: el burbujeo se resuelve en cuencos de cobre; una nota (básica), "
        "dos (mejorada) o una tríada con brillo (maestra)", "`f2:salmuera_ok`",
        var(calidad=["basica", "mejorada", "maestra"]), objetivo=-19)
def salmuera_ok(ctx, v):
    c = v["calidad"]
    lz = Lienzo(1.2)
    lz.add(N(burbujeo(0.35, ctx.hijo("b"), 120, 300, 1800)), 0, -8)
    notas = {"basica": [0], "mejorada": [0, 4], "maestra": [0, 2, 4, 7]}[c]
    for k, g in enumerate(notas):
        lz.add(N(cuenco(grado(g + 2, 4), 1.0, ctx.hijo(f"c{k}"), t60=1.4)), 0.25 + 0.06 * k, -3 - k)
    if c == "maestra":
        lz.add(N(destello(ctx.hijo("d"), 0.6, 5000, 6)), 0.3, -12)
    return lz.senal()


@receta("sfx_taller_salmuera_turbia", S, "Falla: Salmuera Turbia; borbotones torpes, un cuenco desafinado y un 'plop'",
        "`f2:salmuera_turbia`", var(v=[1]), objetivo=-20)
def salmuera_turbia(ctx, v):
    lz = Lienzo(1.2)
    for k in range(6):
        lz.add(burbuja(ctx.u(150, 400), ctx.hijo(f"b{k}"), ctx.u(0.6, 1.0), ctx.u(-0.1, 0.1)), ctx.u(0, 0.4))
    lz.add(N(cuenco(hz("F3") * (1.04 if v == 1 else 0.96), 0.9, ctx.hijo("c"), t60=1.0, batido=7.0)), 0.3, -6)
    lz.add(burbuja(140, ctx.hijo("plop"), 1.0, 0.02), 0.7)
    return lz.senal()


@receta("sfx_taller_altar", S, "Refinar sal en el Altar del Árbol: el crujido de grano sucio se aclara hasta el tono "
        "puro del metal del planeta (barrido de Pureza de 0.70 a 1.00)", "`f2:refinar` (`Pureza`)",
        var(planeta=PLANETAS_ORDEN), objetivo=-19)
def altar(ctx, v):
    p = v["planeta"]
    lz = Lienzo(1.4)
    lz.add(N(crujido_sal(ctx.hijo("t"), 0.25, 0.72, fracturas=2)), 0, -2)
    lz.add(N(crujido_sal(ctx.hijo("d"), 0.2, 0.85, fracturas=1)), 0.3, -6)
    lz.add(nota_planeta(p, f_planeta(p, 5), ctx.hijo("n"), 0.9, None), 0.5, -4)
    lz.add(N(metal("plata", f_planeta(p, 6), 0.8, ctx.hijo("l"), t60=0.8, golpe=0)), 0.55, -10)
    return lz.senal()


@receta("sfx_taller_receta", S, "Receta nueva en el Recetario: hoja, pluma y una campanita", "`f2:receta_nueva`",
        var(v=[1]), objetivo=-20)
def receta_nueva(ctx, v):
    lz = Lienzo(1.0)
    lz.add(N(hoja(ctx.hijo("h"), 0.3)), 0, -6)
    lz.add(N(pluma(0.3, ctx.hijo("p"), 1)), 0.25, -6)
    lz.add(N(metal("oro", grado(4, 5, RE_MAYOR), 0.5, ctx.hijo("o"), t60=0.6)), 0.55, -4)
    return lz.senal()
