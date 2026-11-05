# -*- coding: utf-8 -*-
"""3.5.2 Cadena y Partitura."""
import numpy as np

from sintesis import (Lienzo, metal, madera, golpe_sordo, chasquido, whoosh, engranes, abaco, tic_reloj, pluma,
                      copa_frotada, cuerda, cuenco, campana_spark, blanco, bp, hp, lp, env_exp, env_de, exp_de,
                      grado, hz, sala, fuente, tramo, n_, cambiar_velocidad, RE_MAYOR)
from . import receta, var
from .comun import N, nota_planeta, destello, swell, f_planeta, OCTAVA_PAPEL, PLANETAS_ORDEN

S = "cadena"

# 3.4: la campana del Spark toca la tónica y sube por las notas del acorde: 1.ª, 3.ª, 5.ª, 8.ª y 10.ª.
# En Re menor (la tonalidad del combate): Re, Fa, La, Re y Fa, desde Re6.
SPARK_GRADOS = {1: 0, 2: 2, 3: 4, 4: 7, 5: 9}


@receta("sfx_cadena_resonancia_llena", S, "La Resonancia llega a 100: copas de cristal que se llenan hasta un acorde "
        "abierto de Re y un brillo", "`ResonanceChanged` (a 100) o `ui:resonancia_llena`", var(v=[1]), nivel="A",
        objetivo=-19)
def resonancia_llena(ctx, v):
    lz = Lienzo(1.3)
    for k, g in enumerate([0, 4, 7]):
        lz.add(N(copa_frotada(grado(g, 5), 0.9, ctx.hijo(f"c{k}"))), 0.08 * k, -4 - k)
    lz.add(N(metal("plata", grado(7, 5), 0.8, ctx.hijo("p"), t60=1.0, golpe=0.5)), 0.42, -4)
    lz.add(N(destello(ctx.hijo("d"), 0.6, 5000, 5)), 0.45, -10)
    return sala(lz.senal(), 0.15, 1.0)


@receta("sfx_cadena_abrir", S, "Se abre la Partitura y el mundo se detiene: aire que se recoge, el libro que se abre, "
        "un último tic de reloj y una nota de cristal suspendida", "`ui:cadena_abrir`", var(v=[1]), nivel="A",
        objetivo=-18)
def abrir(ctx, v):
    lz = Lienzo(1.4)
    w = whoosh(0.45, ctx.hijo("w"), 3000, 400, q=1.0, pico=0.85, forma=2.0)
    lz.add(N(w), 0, -6)
    lz.add(fuente("kenney_rpg_audio/book_open.ogg", velocidad=0.85, hp_hz=200), 0.38, -6)
    lz.add(N(tic_reloj(ctx.hijo("t"), 0.9)), 0.42, -8)
    c = copa_frotada(grado(0, 5), 0.9, ctx.hijo("c"))
    lz.add(N(c) * env_de(c, [(0, 0), (0.05, 1), (0.9, 0)]), 0.44, -8)
    lz.add(N(metal("plata", grado(4, 5), 0.8, ctx.hijo("p"), t60=1.0, golpe=0.2)), 0.44, -10)
    return sala(lz.senal(), 0.15, 1.0)


@receta("sfx_cadena_cerrar", S, "Se cierra la Partitura sin ejecutar: el libro se cierra y el aire suelta al mundo",
        "`ui:cadena_cerrar` (cancelar la Cadena)", var(v=[1]), nivel="A", objetivo=-20, nuevo=True,
        notas="ID nuevo: el catálogo solo tiene `sfx_cadena_abrir`; la Partitura también se cierra sin confirmar.")
def cerrar(ctx, v):
    lz = Lienzo(0.8)
    lz.add(fuente("kenney_rpg_audio/book_close.ogg", velocidad=0.95, hp_hz=150), 0, -4)
    lz.add(N(whoosh(0.45, ctx.hijo("w"), 500, 2500, q=1.0, pico=0.3)), 0.05, -8)
    lz.add(N(metal("plata", grado(4, 4), 0.5, ctx.hijo("p"), t60=0.5, golpe=0.1)), 0.02, -14)
    return lz.senal()


@receta("sfx_cadena_tomar", S, "Tomar la carta de una bruja en la Partitura: pergamino que se levanta y un tic",
        "`ui:partitura_tomar`", var(v=[1, 2, 3]), nivel="A", objetivo=-25)
def tomar(ctx, v):
    lz = Lienzo(0.2)
    lz.add(tramo(fuente("kenney_rpg_audio/book_flip_3.ogg", velocidad=[1.5, 1.65, 1.4][v - 1], hp_hz=900), 0.12, 0.04),
           0, -4)
    lz.add(N(madera(ctx.u(1800, 2300), ctx.hijo("m"), 0.06, 0.02)), 0, -12)
    return lz.senal()


@receta("sfx_cadena_soltar", S, "Soltar la carta en su lugar: pergamino que se asienta y un toque de madera",
        "`ui:partitura_soltar`", var(v=[1, 2, 3]), nivel="A", objetivo=-24)
def soltar(ctx, v):
    lz = Lienzo(0.25)
    lz.add(tramo(fuente("kenney_rpg_audio/book_place_%d.ogg" % (1 + (v - 1) % 2), velocidad=[1.4, 1.5, 1.3][v - 1],
                        hp_hz=250), 0.18, 0.05), 0, -4)
    lz.add(N(madera(ctx.u(900, 1200), ctx.hijo("m"), 0.08, 0.03)), 0.005, -10)
    return lz.senal()


@receta("sfx_cadena_spark_previsto", S, "Aparece un Spark previsto: destello dorado pequeño. Desaparece: el destello se "
        "apaga y baja", "`ui:preview_cambia` (resultado de `PreviewChain`)", var(cambio=["aparece", "desaparece"]),
        nivel="A", objetivo=-23)
def spark_previsto(ctx, v):
    lz = Lienzo(0.5)
    if v["cambio"] == "aparece":
        lz.add(N(metal("oro", grado(4, 6), 0.4, ctx.hijo("a"), t60=0.45)), 0, -4)
        lz.add(N(metal("oro", grado(7, 6), 0.4, ctx.hijo("b"), t60=0.45)), 0.05, -6)
        lz.add(N(destello(ctx.hijo("d"), 0.3, 6000, 3)), 0.02, -12)
    else:
        x = metal("oro", grado(7, 6), 0.4, ctx.hijo("a"), t60=0.35, golpe=0.2)
        x = lp(x, 3500)
        lz.add(N(x), 0, -6)
        lz.add(N(lp(metal("oro", grado(4, 6), 0.35, ctx.hijo("b"), t60=0.3, golpe=0.1), 3000)), 0.06, -8)
    return lz.senal()


@receta("sfx_cadena_confirmar", S, "Confirmar la Partitura: trazo de pluma con rúbrica, el punto final y una campana "
        "grave suave", "`ui:cadena_confirmar`", var(v=[1]), nivel="A", objetivo=-20)
def confirmar(ctx, v):
    lz = Lienzo(0.9)
    lz.add(N(pluma(0.35, ctx.hijo("p"), trazos=2, punto=True)), 0, -2)
    lz.add(N(metal("oro", grado(0, 4), 0.6, ctx.hijo("c"), t60=0.8, brillo=0.8)), 0.33, -8)
    lz.add(N(metal("oro", grado(4, 4), 0.6, ctx.hijo("c2"), t60=0.8, brillo=0.8)), 0.335, -12)
    return lz.senal()


@receta("sfx_cadena_calculo", S, "El sonido del cálculo: engranes de astrolabio y cuentas de ábaco; la Calculista "
        "calcula en silencio", "`ChainStarted`", var(v=[1]), nivel="A", objetivo=-20)
def calculo(ctx, v):
    lz = Lienzo(1.1)
    e = engranes(0.8, ctx.hijo("e"), 18 if v == 1 else 24, 3400)
    lz.add(N(e) * env_de(e, [(0, 0.4), (0.3, 1), (0.8, 0.6), (0.9, 0)]), 0, -6)
    lz.add(N(abaco(ctx.hijo("a"), 5, 0.09)), 0.1, -4)
    lz.add(N(tic_reloj(ctx.hijo("t"), 1.0)), 0.82, -6)
    return lz.senal()


@receta("sfx_cadena_eslabon", S, "Entrada de cada eslabón, sin voz: aire que se corta. Apertura breve y aguda, Enlace "
        "doble, Cierre con peso", "`ChainLinkStarted` (`Role`)", var(papel=["apertura", "enlace", "cierre"], v=[1, 2]),
        nivel="A", objetivo=lambda v: {"apertura": -20, "enlace": -20, "cierre": -18}[v["papel"]])
def eslabon(ctx, v):
    p = v["papel"]
    lz = Lienzo(0.6)
    if p == "apertura":
        lz.add(N(whoosh(0.22, ctx, 900, 4500, q=1.8, pico=0.7)), 0)
    elif p == "enlace":
        lz.add(N(whoosh(0.16, ctx, 700, 3000, q=1.7, pico=0.6)), 0)
        lz.add(N(whoosh(0.18, ctx.hijo("b"), 1000, 4000, q=1.7, pico=0.6)), 0.11, -2)
    else:
        lz.add(N(whoosh(0.4, ctx, 200, 2500, q=1.2, pico=0.8, forma=2.2)), 0)
        lz.add(N(golpe_sordo(ctx.hijo("g"), 110, 55, 0.25, 0.0)), 0.36, -10)
    return lz.senal()


@receta("sfx_partitura_nota", S, "Una nota por golpe en la escala de Re menor, con el metal del planeta: Luna Re, "
        "Mercurio Mi, Venus Fa, Sol Sol, Marte La, Júpiter Si♭, Saturno Do. Apertura grave, Enlace media, Cierre aguda "
        "con acorde", "`HitLanded` (`EnCadena = 1`, sin Spark); `ui:partitura_escuchar`",
        var(planeta=PLANETAS_ORDEN, papel=["apertura", "enlace", "cierre"]), nivel="A", objetivo=-22,
        notas="Notas de 150 ms con caída natural. El Cierre suma la tercera y la quinta del grado, dentro de la escala.")
def partitura_nota(ctx, v):
    pl, papel = v["planeta"], v["papel"]
    from sintesis import PLANETAS
    g = PLANETAS[pl]["grado"]
    oct_ = OCTAVA_PAPEL[papel]
    lz = Lienzo(0.45)
    lz.add(nota_planeta(pl, grado(g, oct_), ctx.hijo("n"), 0.45, 0.15), 0)
    if papel == "cierre":
        lz.add(nota_planeta(pl, grado(g + 2, oct_), ctx.hijo("t"), 0.45, 0.15), 0.004, -5)
        lz.add(nota_planeta(pl, grado(g + 4, oct_), ctx.hijo("q"), 0.45, 0.15), 0.008, -7)
    return lz.senal()


@receta("sfx_cadena_spark", S, "La campana del Spark: aguda, corta y dorada. Sube por el acorde de Re menor con cada "
        "Spark de la misma Cadena: Re6, Fa6, La6, Re7 y Fa7", "`HitLanded` (`IsSpark`), una por grupo",
        var(spark=[1, 2, 3, 4, 5]), nivel="A", objetivo=-15,
        notas="`spark` es `SparkIndice` (1 a 5). Distinta de la Campana de Bronce (grave y con onda diferida).")
def spark(ctx, v):
    f = grado(SPARK_GRADOS[v["spark"]], 6)
    lz = Lienzo(1.1)
    lz.add(N(campana_spark(f, ctx.hijo("c"), 1.0)), 0)
    lz.add(N(campana_spark(f * 2, ctx.hijo("o"), 0.6)), 0.0, -14)
    return sala(lz.senal(), 0.08, 0.8)


@receta("sfx_cadena_invitada", S, "La Invitada cierra la Cadena: díada de plata y oro con un barrido amplio",
        "`ChainLinkStarted` (Cierre de la Invitada)", var(v=[1]), nivel="A", objetivo=-18)
def invitada(ctx, v):
    lz = Lienzo(1.2)
    lz.add(N(whoosh(0.35, ctx.hijo("w"), 300, 3000, q=1.1, pico=0.8)), 0, -6)
    base = grado(0 if v == 1 else 4, 5)
    lz.add(N(metal("plata", base, 0.9, ctx.hijo("p"), t60=1.0, batido=0.8)), 0.3, -3)
    lz.add(N(metal("oro", base * 1.5, 0.9, ctx.hijo("o"), t60=1.0)), 0.31, -6)
    lz.add(N(destello(ctx.hijo("d"), 0.5, 4500, 5)), 0.32, -12)
    return sala(lz.senal(), 0.15, 1.0)


@receta("sfx_cadena_fin", S, "Acorde de resolución de la Cadena, más brillante con más Spark: sin Spark, Re menor en "
        "cuencos; 1 o 2, Re menor con campanas de oro; 3 o más, Re mayor (tercera de Picardía) con cascada dorada",
        "`ChainEnded` (`Sparks`: 0, 1–2, 3+)", var(sparks=[0, 1, 3]), nivel="A", objetivo=-17)
def fin(ctx, v):
    s = v["sparks"]
    lz = Lienzo(1.8)
    tercera = 4 if s >= 3 else 3
    raiz = hz("D4")
    for k, semi in enumerate([0, 7, 12, 12 + tercera]):
        lz.add(N(cuenco(raiz * 2 ** (semi / 12), 1.6, ctx.hijo(f"c{k}"), t60=2.0)), 0.015 * k, -4 - k)
    if s >= 1:
        for k, semi in enumerate([12, 12 + tercera, 19, 24]):
            lz.add(N(metal("oro", raiz * 2 ** (semi / 12), 1.4, ctx.hijo(f"o{k}"), t60=1.3)), 0.05 + 0.06 * k,
                   -6 - k if s >= 3 else -10 - k)
    if s >= 3:
        lz.add(N(destello(ctx.hijo("d"), 0.9, 4000, 9)), 0.25, -8)
        for k, g in enumerate([7, 9, 11, 14]):
            lz.add(N(metal("oro", grado(g, 5, RE_MAYOR), 0.8, ctx.hijo(f"a{k}"), t60=0.8)), 0.35 + 0.07 * k, -10)
    return sala(lz.senal(), 0.18, 1.2)
