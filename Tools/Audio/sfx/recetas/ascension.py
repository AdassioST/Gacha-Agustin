# -*- coding: utf-8 -*-
"""3.5.7 Ascensiones."""
import numpy as np

from sintesis import (SR, Lienzo, metal, cuenco, copa_frotada, crujido_sal, fractura, golpe_sordo, whoosh, tic_reloj,
                      cadena_hierro, raspado, campana_spark, dron, blanco, rosa, marron, bp, hp, lp, env_de, exp_de,
                      env_campana, grado, hz, sala, fuente, tramo, n_, tiempo, cambiar_velocidad, coro, am_aleatoria)
from . import receta, var
from .comun import N, destello, swell, impacto, golpe_astral, f_planeta, PLANETAS_ORDEN, campanas_acorde
from .magia import canalizar

S = "ascension"


@receta("sfx_ascension_carga", S, "El anillo de Ánima llega a 100, 200 o 300: una, dos o tres notas de oro que se "
        "apilan (Re, Fa, La) con un brillo", "`AnimaChanged` (cruza 100, 200 o 300) o `ui:anima_umbral`",
        var(nivel=[1, 2, 3]), nivel="A", objetivo=-21)
def carga(ctx, v):
    n = v["nivel"]
    lz = Lienzo(0.9)
    lz.add(N(whoosh(0.25, ctx.hijo("w"), 800, 4000, q=1.5, pico=0.85)), 0, -12)
    for k, g in enumerate([0, 2, 4][:n]):
        lz.add(N(metal("oro", grado(g, 5), 0.7, ctx.hijo(f"o{k}"), t60=0.7)), 0.2 + 0.06 * k, -3 - k)
    lz.add(N(destello(ctx.hijo("d"), 0.4, 5000, 2 + n)), 0.22, -12)
    return lz.senal()


@receta("sfx_ascension_lanzar", S, "El jugador lanza la Ascensión y el tiempo se congela: aire que se recoge, un último "
        "tic y un golpe de cristal; más grave y más ancho por nivel", "`AscensionCast` (`Level`)",
        var(nivel=[1, 2, 3]), nivel="A", objetivo=lambda v: [-17, -16, -15][v["nivel"] - 1])
def lanzar(ctx, v):
    n = v["nivel"]
    lz = Lienzo(1.4)
    lz.add(N(whoosh(0.4 + 0.1 * n, ctx.hijo("w"), 4000, 300, q=1.0, pico=0.9, forma=2.2)), 0, -4)
    t = 0.4 + 0.1 * n
    lz.add(N(tic_reloj(ctx.hijo("t"), 0.9)), t, -6)
    lz.add(N(copa_frotada(grado(0, 6 - (n == 3)), 0.8, ctx.hijo("c"))) * env_de(np.zeros(n_(0.8)), [(0, 1), (0.8, 0)]),
           t, -6)
    lz.add(N(metal("plata", grado(4, 5), 0.8, ctx.hijo("p"), t60=1.0, golpe=0.8)), t, -6)
    if n >= 2:
        lz.add(N(golpe_sordo(ctx.hijo("g"), 100, 45, 0.5, 0.0)), t, -6)
    if n == 3:
        lz.add(N(dron(0.9, ctx.hijo("d"), hz("D1"), (2, 3, 4), 0.4)) * env_de(np.zeros(n_(0.9)), [(0, 1), (0.9, 0)]),
               t, -8)
    return sala(lz.senal(), 0.15, 1.2)


@receta("sfx_ascension_i", S, "Ascensión I (1 a 1.5 s): el metal del planeta se carga y suelta la técnica del Ascendente "
        "en un golpe de Quintaesencia", "`AscensionCast` (`Level` 1)", var(planeta=PLANETAS_ORDEN), nivel="A",
        objetivo=-15, notas="Un archivo por planeta; la capa de signo la da `sfx_magia_ascendente_aparece`.")
def asc_i(ctx, v):
    p = v["planeta"]
    lz = Lienzo(1.4)
    c = canalizar(ctx.hijo("c"), {"planeta": p})[n_(0.3):]
    lz.add(N(c), 0, -6)
    lz.add(N(golpe_astral(p, "quintaesencia", ctx.hijo("g"))), 0.36, 0)
    lz.add(N(impacto(ctx.hijo("i"), 0.9)), 0.36, -4)
    return sala(lz.senal(), 0.14, 1.1)


@receta("sfx_ascension_ii", S, "Ascensión II (2 s, compacta por el tope de tamaño): carga larga, dos golpes del planeta y un acorde de su metal que "
        "sube para todo el equipo", "`AscensionCast` (`Level` 2)", var(planeta=PLANETAS_ORDEN), nivel="A",
        objetivo=-14)
def asc_ii(ctx, v):
    p = v["planeta"]
    lz = Lienzo(2.2)
    lz.add(N(swell(0.55, ctx.hijo("sw"), 200, 3000, 0.9)), 0, -6)
    lz.add(N(canalizar(ctx.hijo("c"), {"planeta": p})[n_(0.2):]), 0.0, -4)
    lz.add(N(golpe_astral(p, "quintaesencia", ctx.hijo("g1"))), 0.55, 0)
    lz.add(N(impacto(ctx.hijo("i1"), 0.8)), 0.55, -4)
    lz.add(N(golpe_astral(p, "destilada", ctx.hijo("g2"))), 0.85, -2)
    from sintesis import PLANETAS
    g0 = PLANETAS[p]["grado"]
    for k, g in enumerate([g0, g0 + 2, g0 + 4, g0 + 7]):
        lz.add(N(metal(PLANETAS[p]["metal"] if p not in ("saturno", "marte") else "oro", grado(g, 4), 1.2,
                       ctx.hijo(f"a{k}"), t60=1.4)), 1.05 + 0.08 * k, -6 - k)
    lz.add(N(destello(ctx.hijo("d"), 0.8, 4000, 6)), 1.15, -12)
    return sala(lz.senal(), 0.16, 1.3)


def _iii_marisal(ctx, largo):
    """Ascensión III de Marisal: el Carnero de Acero Carmesí. Do frigio a 120 BPM (ficha P16)."""
    b = 0.5  # un tiempo a 120 BPM
    d = 7.2 if largo else 2.6
    lz = Lienzo(d)
    if largo:
        # 3 tiempos de preparación: aire que se recoge y cadenas que se tensan
        lz.add(N(swell(1.4, ctx.hijo("sw"), 150, 2500, 0.8)), 0, -6, 0)
        lz.add(N(cadena_hierro(1.2, ctx.hijo("cad"), 18, 2200)), 0.2, -12, -0.4)
        lz.add(N(raspado(1.0, ctx.hijo("r"), 1.2)), 0.4, -14, 0.4)
        # 2 tiempos de ritual: la halita se parte
        lz.add(N(fractura(ctx.hijo("f1"))), 3 * b, -4, 0)
        lz.add(N(fractura(ctx.hijo("f2"), 0.8)), 3 * b + 0.18, -6, 0)
        lz.add(N(metal("hierro", hz("C3"), 1.0, ctx.hijo("cmp"), t60=1.2)), 3 * b, -8, 0)
        # 6 tiempos de clímax: la embestida del carnero, cascos que aceleran y yunques en los tiempos
        t = 5 * b
        paso = 0.42
        k = 0
        while t < 11 * b - 0.05:
            pan = -0.3 if k % 2 else 0.3
            lz.add(N(golpe_sordo(ctx.hijo(f"c{k}"), 110, 48, 0.3, 0.5)), t, -6 + min(k, 6) * 0.5, pan)
            lz.add(tramo(fuente("kenney_impact_sounds/impact_mining_000.ogg", velocidad=1.2, hp_hz=300), 0.25), t,
                   -14, pan)
            t += paso
            paso = max(paso * 0.88, 0.16)
            k += 1
        for j, n in enumerate(["C4", "Db4", "C4", "G3", "C4", "C4"]):
            lz.add(N(metal("hierro", hz(n), 0.6, ctx.hijo(f"y{j}"), t60=0.6)), 5 * b + j * b, -8, 0.2 * (j % 3 - 1))
        lz.add(N(swell(1.2, ctx.hijo("sw2"), 300, 5000, 1.0)), 9.6 * b, -6, 0)
        t_imp = 11 * b
    else:
        lz.add(N(swell(0.5, ctx.hijo("sw"), 200, 3000, 0.9)), 0, -6, 0)
        lz.add(N(fractura(ctx.hijo("f1"))), 0.35, -6, 0)
        for k in range(3):
            lz.add(N(golpe_sordo(ctx.hijo(f"c{k}"), 110, 48, 0.25, 0.5)), 0.5 + 0.17 * k, -7, 0.3 * (-1) ** k)
        t_imp = 1.0
    # 3 tiempos de impacto: el golpe final, la sal y el hierro estallan
    lz.add(N(impacto(ctx.hijo("imp"), 1.3)), t_imp, 0, 0)
    lz.add(N(golpe_astral("marte", "quintaesencia", ctx.hijo("ga"))), t_imp, -3, 0)
    lz.add(N(crujido_sal(ctx.hijo("sal"), 0.5, 0.97, granos=2.0, fracturas=3)), t_imp + 0.01, -5, -0.2)
    lz.add(fuente("kenney_impact_sounds/impact_metal_heavy_001.ogg", velocidad=0.75, hp_hz=80), t_imp, -6, 0.2)
    # campanas que quedan sonando (Do, Mi♭, Sol)
    for k, n in enumerate(["C4", "Eb4", "G4", "C5"]):
        lz.add(N(metal("hierro" if k == 0 else "oro", hz(n), 1.4, ctx.hijo(f"b{k}"), t60=1.6)), t_imp + 0.05 + 0.04 * k,
               -10 - k, 0.3 * (k - 1.5) / 1.5)
    return sala(lz.senal(), 0.2, 0.8)


@receta("sfx_ascension_iii_marisal", S, "Ascensión III de Marisal, el Carnero de Acero Carmesí (capa de efectos bajo "
        "`mus_ascension_iii_marisal`): cadenas que se tensan, la halita que se parte, la embestida con yunques en Do "
        "frigio a 120 BPM y el impacto de hierro y sal", "`AscensionCast` (`Level` 3) → `tl:` pista de FMOD en la "
        "Timeline", var(version=["largo", "corto"]), nivel="A", objetivo=lambda v: -12 if v["version"] == "largo" else -13,
        notas="Mono para cuidar el tope de 30 MB; FMOD lo abre en la Timeline. Solo Marisal (la 5★ del vertical slice). La III no se alcanza en la prueba de la Fase 1 "
        "(requiere grado S); las demás brujas quedan para su kit de audio.")
def asc_iii_marisal(ctx, v):
    return _iii_marisal(ctx, v["version"] == "largo")


@receta("sfx_ascension_impacto", S, "Impacto de la Ascensión tras la pausa de 3 frames: golpe enorme con el metal del "
        "planeta y la sal que estalla", "`tl:impacto`", var(planeta=PLANETAS_ORDEN), nivel="A", objetivo=-13)
def asc_impacto(ctx, v):
    p = v["planeta"]
    lz = Lienzo(1.4)
    lz.add(N(impacto(ctx.hijo("i"), 1.3)), 0, -1)
    lz.add(N(golpe_astral(p, "quintaesencia", ctx.hijo("g"))), 0, -3)
    lz.add(N(crujido_sal(ctx.hijo("s"), 0.4, 0.97, granos=2.0, fracturas=3)), 0.01, -6)
    from sintesis import PLANETAS
    lz.add(N(metal(PLANETAS[p]["metal"], f_planeta(p, 3), 1.2, ctx.hijo("m"), t60=1.2)), 0, -8)
    return sala(lz.senal(), 0.14, 1.2)


@receta("sfx_ascension_duo", S, "Dos Ascensiones encimadas con Spark: dos campanas doradas (Re y La) y un doble golpe",
        "`HitLanded` (`IsSpark`, origen Ascensión)", var(v=[1]), nivel="A", objetivo=-13)
def asc_duo(ctx, v):
    lz = Lienzo(1.8)
    lz.add(N(campana_spark(grado(0, 6), ctx.hijo("a"), 1.2)), 0, -2)
    lz.add(N(campana_spark(grado(4, 6), ctx.hijo("b"), 1.2)), 0.012, -3)
    lz.add(N(impacto(ctx.hijo("i1"), 1.2)), 0, -2)
    lz.add(N(impacto(ctx.hijo("i2"), 1.0)), 0.09, -5)
    lz.add(N(destello(ctx.hijo("d"), 0.8, 4500, 8)), 0.05, -8)
    return sala(lz.senal(), 0.15, 1.2)


@receta("sfx_ascension_saltar", S, "Saltar la cinemática: corte limpio al impacto, un barrido breve hacia atrás",
        "`ui:saltar`", var(v=[1]), nivel="A", objetivo=-20)
def asc_saltar(ctx, v):
    lz = Lienzo(0.4)
    w = whoosh(0.3, ctx.hijo("w"), 600, 3500, q=1.4, pico=0.9, forma=2.5)
    lz.add(N(w), 0, -2)
    lz.add(tramo(fuente("kenney_rpg_audio/book_flip_2.ogg", velocidad=1.6, hp_hz=800), 0.15), 0.22, -8)
    return lz.senal()
