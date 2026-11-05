# -*- coding: utf-8 -*-
"""3.5.3 Magia y ritual."""
import math

import numpy as np

from sintesis import (SR, Lienzo, metal, madera, piedra, golpe_sordo, chasquido, whoosh, pelaje, papel, crujido_sal,
                      fractura, _granos, cuerda, cuenco, copa_frotada, gota, burbuja, burbujeo, chapoteo, fuego,
                      crepitar, grito_estano, estatica, trueno_lamina, raspado, tic_reloj, cadena_hierro, dron,
                      susurro, blanco, rosa, marron, bp, hp, lp, env_exp, env_de, exp_de, env_campana, env_puntos,
                      grado, hz, sala, fuente, tramo, n_, tiempo, cambiar_velocidad, am_aleatoria, coro, eco,
                      banda_movil, modal)
from . import receta, var
from .comun import (N, nota_planeta, destello, swell, f_planeta, crujido_planeta, golpe_astral, impacto,
                    PLANETAS_ORDEN, ORDEN_PUREZAS)

S = "magia"
DUR_RITUAL = {"largo": 0.40, "corto": 0.20}   # GDD 19.2: 0.4 s la primera vez por combate, 0.2 s después


def _granos_caen(ctx, dur, tasa=600, f_lo=3000, f_hi=12000):
    g = _granos(dur, ctx, lambda t: tasa * math.exp(-t / (dur * 0.6)) + 20, f_lo, f_hi, ping=0.4)
    return N(g)


def _salero(ctx, dur=0.09):
    """Una sacudida de salero de plata: granos contra metal."""
    lz = Lienzo(dur + 0.05)
    lz.add(_granos_caen(ctx, dur, 1800, 2500, 11000), 0, -2)
    for k in range(3):
        lz.add(N(metal("plata", ctx.u(5200, 7600), 0.05, ctx.hijo(f"p{k}"), t60=0.04, golpe=0.3)), ctx.u(0, dur * 0.6),
               -12)
    return lz.senal()


def _porcelana(ctx, f=2900):
    return N(modal(0.12, [f, f * 1.83, f * 2.71], [1, 0.5, 0.3], [0.09, 0.06, 0.04], ctx))


def _trago(ctx):
    """Un trago corto: dos burbujas graves y el líquido; sin boca ni respiración."""
    lz = Lienzo(0.16)
    lz.add(burbuja(ctx.u(260, 320), ctx, 1.0, 0.04), 0)
    lz.add(burbuja(ctx.u(330, 400), ctx.hijo("b"), 0.7, 0.04), 0.06)
    lz.add(N(lp(blanco(0.12, ctx.hijo("l")), 1500) * exp_de(np.zeros(n_(0.12)), 0.08, 0.01)), 0, -16)
    return lz.senal()


def _mordida_seca(ctx):
    """Mordida seca, sin saliva: chasquido de hueso y un cuerpo breve."""
    lz = Lienzo(0.08)
    lz.add(N(madera(ctx.u(1300, 1600), ctx, 0.06, 0.02)), 0, -2)
    lz.add(N(golpe_sordo(ctx.hijo("g"), 220, 140, 0.06, 0.0, 0.3)), 0, -10)
    return lz.senal()


def _mordida_jugosa(ctx, dur=0.15):
    """Gajo de limón mordido (150 ms): crujido húmedo y fino, sin sorber."""
    lz = Lienzo(dur + 0.03)
    jugo = _granos(dur, ctx, lambda t: 3000 * math.exp(-t / 0.05) + 200, 1200, 6000, (0.0003, 0.0015), ping=0.05,
                   amp_sigma=0.5)
    lz.add(N(jugo), 0, -2)
    lz.add(N(lp(blanco(0.06, ctx.hijo("p")), 1800) * exp_de(np.zeros(n_(0.06)), 0.04, 0.003)), 0, -8)
    return lz.senal()


def _lenguetazo(ctx):
    """Lengüetazo corto, seco y cómico: un roce que sube de banda. Sin sorber ni respiración."""
    x = blanco(0.07, ctx)
    y = banda_movil(x, lambda t: 900 * (2600 / 900) ** np.clip(t / 0.07, 0, 1), q=2.2)
    return N(y * env_campana(0.07, 0.35, 1.3))


def _gamuza(ctx, dur):
    """Sal fina sobre gamuza: roce suave y granos finos."""
    r = bp(rosa(dur, ctx), 700, 4500) * am_aleatoria(dur, ctx.hijo("am"), 10, 40, 0.8) * env_campana(dur, 0.4)
    return N(r) * 0.6 + 0.4 * _granos_caen(ctx.hijo("g"), dur, 900, 3500, 10000)


def _rasguno_tierra(ctx, dur=0.08):
    return tramo(fuente("kenney_impact_sounds/footstep_grass_00%d.ogg" % ctx.i(0, 2), velocidad=ctx.u(1.3, 1.6),
                        hp_hz=400), dur, 0.03)


RITUALES = {
    "marisal": "Muerde un cristal de sal de roca: mordida seca, sin saliva, y la halita que se parte en dos",
    "selenia": "Disuelve sal en té negro: cucharita contra porcelana, granos que caen al té y un trago corto",
    "solenne": "Sal gruesa en un gajo de limón con chile: granos sobre la pulpa, chile como roce de papel y una mordida "
               "jugosa de 150 ms",
    "cervanda": "Salero de plata con modales de corte: dos sacudidas exactas y la cuchara de plata",
    "tormentina": "Lame sal del dorso de la pata: sal fina sobre gamuza y un lengüetazo corto, seco y cómico",
    "cupria": "Su Ascendente le pone la sal en la lengua: sal que cae de dedos astrales, con reverberación (sin boca)",
    "muertita": "Esconde la sal bajo la lengua y se desmaya: una pizca de granos y una caída blanda y cómica",
    "topacia": "Saca la sal de la tierra con las garras y la masca: dos rasguños en la tierra y dos crujidos",
}
RECATADO = {"solenne": "sin mordida: solo sal y gajo", "tormentina": "sin lengüetazo: sal sacudida de la pata"}


def _ritual(bruja, ctx, dur, recatado=False):
    largo = dur > 0.3
    lz = Lienzo(dur)
    if bruja == "marisal":
        t0, t1 = (0.06, 0.22) if largo else (0.02, 0.1)
        lz.add(_mordida_seca(ctx.hijo("m")), t0, -2)
        lz.add(N(fractura(ctx.hijo("f1"), 1.0)), t0 + 0.005, -1)
        lz.add(N(crujido_sal(ctx.hijo("c1"), 0.1, 0.8, granos=1.6, fracturas=0)), t0 + 0.004, -3)
        lz.add(N(fractura(ctx.hijo("f2"), 0.8)), t1, -4)
        lz.add(N(crujido_sal(ctx.hijo("c2"), 0.08, 0.8, granos=1.4, fracturas=0)), t1 + 0.003, -5)
        lz.add(_granos_caen(ctx.hijo("g"), dur - t1 - 0.02, 500), t1 + 0.01, -14)
    elif bruja == "selenia":
        lz.add(_porcelana(ctx.hijo("p1"), 2900), 0.0, -6)
        if largo:
            lz.add(_porcelana(ctx.hijo("p2"), 3050), 0.1, -9)
        n = 7 if largo else 3
        for k in range(n):
            lz.add(gota(ctx.u(2500, 5000), ctx.hijo(f"g{k}"), ctx.u(0.2, 0.5)), 0.04 + k * (0.025 if largo else 0.02),
                   -6)
        lz.add(_trago(ctx.hijo("t")), dur - 0.17 if largo else dur - 0.12, -4)
    elif bruja == "solenne":
        d_sal = 0.16 if largo else 0.08
        lz.add(_granos_caen(ctx.hijo("s"), d_sal, 900, 2000, 9000), 0.0, -6)
        lz.add(N(papel(d_sal, ctx.hijo("chile"), 260, 2500, 9000)), 0.03, -12)
        if recatado:
            lz.add(_mordida_jugosa(ctx.hijo("gajo"), 0.06) * 0.5, d_sal + 0.02, -8)
        else:
            lz.add(_mordida_jugosa(ctx.hijo("b"), 0.15 if largo else 0.09), d_sal + 0.02, -1)
    elif bruja == "cervanda":
        t0, t1 = (0.03, 0.16) if largo else (0.01, 0.075)
        lz.add(_salero(ctx.hijo("s1")), t0, -2)
        lz.add(_salero(ctx.hijo("s2")), t1, -2)
        if largo:
            lz.add(N(metal("plata", hz("D7"), 0.12, ctx.hijo("cuchara"), t60=0.12, golpe=0.8)), 0.29, -6)
    elif bruja == "tormentina":
        if recatado:
            for k, t in enumerate([0.02, 0.12] if largo else [0.01, 0.07]):
                lz.add(N(golpe_sordo(ctx.hijo(f"pata{k}"), 300, 180, 0.05, 0.1, 0.2)), t, -6)
                lz.add(N(pelaje(0.06, ctx.hijo(f"pel{k}"))) * exp_de(np.zeros(n_(0.06)), 0.05), t, -10)
            lz.add(_granos_caen(ctx.hijo("g"), dur * 0.6, 700), dur * 0.3, -8)
        else:
            d_g = 0.2 if largo else 0.1
            lz.add(_gamuza(ctx.hijo("gm"), d_g), 0, -4)
            lz.add(_lenguetazo(ctx.hijo("l")), d_g + 0.03, -2)
    elif bruja == "cupria":
        sw = whoosh(dur, ctx.hijo("w"), 2000, 6000, q=1.0, pico=0.5)
        lz.add(N(sw), 0, -12)
        lz.add(_granos_caen(ctx.hijo("g"), dur * 0.9, 900, 3500, 12000), 0.02, -4)
        lz.add(N(metal("plata", grado(2, 6), dur, ctx.hijo("p"), t60=0.5, golpe=0.1)), 0.02, -14)
    elif bruja == "muertita":
        lz.add(_granos_caen(ctx.hijo("g"), 0.1 if largo else 0.05, 700), 0.0, -6)
        t = 0.22 if largo else 0.08
        lz.add(tramo(fuente("kenney_impact_sounds/impact_soft_medium_002.ogg", velocidad=1.1, hp_hz=80), 0.12), t, -2)
        lz.add(tramo(fuente("kenney_rpg_audio/cloth_3.ogg", velocidad=1.2, hp_hz=300), 0.12), t - 0.03, -8)
    else:  # topacia
        lz.add(_rasguno_tierra(ctx.hijo("r1")), 0.0, -4)
        if largo:
            lz.add(_rasguno_tierra(ctx.hijo("r2")), 0.09, -6)
        for k, t in enumerate([0.22, 0.31] if largo else [0.09, 0.15]):
            lz.add(N(crujido_sal(ctx.hijo(f"c{k}"), 0.06, 0.75, granos=0.4, fracturas=1)), t, -3 - 2 * k)
    x = lz.senal()
    if bruja == "cupria":
        x = sala(x, 0.35, 1.4)
    return x


def _registrar_ritual(bruja):
    variantes = var(duracion=["largo"], v=[1, 2]) + var(duracion=["corto"], v=[1])
    if bruja in RECATADO:
        variantes += var(modo=["recatado"], duracion=["largo", "corto"])
    notas = "Solo el gesto del ritual (0.40 s el largo, 0.20 s el corto). Al terminar suena `sfx_sal_crujido` con el " \
            "planeta dominante de la bruja: es el evento anidado del plan (3.2)."
    if bruja in RECATADO:
        notas += f" Modo recatado: {RECATADO[bruja]}."

    @receta(f"sfx_ritual_{bruja}", S, RITUALES[bruja], "`ActionUsed` (Hechizo), `RitualLargo`", variantes, nivel="A",
            objetivo=-19, notas=notas)
    def _fn(ctx, v):
        return _ritual(bruja, ctx, DUR_RITUAL[v["duracion"]], v.get("modo") == "recatado")


for _b in RITUALES:
    _registrar_ritual(_b)


@receta("sfx_sal_crujido", S, "La firma del juego: fractura de halita y granos (120 a 250 ms) con la cola del metal del "
        "planeta a −8 dB (hierro, plata, oro, plomo, estaño, cobre o azogue). Pureza 0.85 (grado B)",
        "Anidado en `sfx_ritual_*` (al terminar el gesto) y en el Altar", var(planeta=PLANETAS_ORDEN, v=[1, 2]),
        nivel="A", objetivo=-18)
def sal_crujido(ctx, v):
    return crujido_planeta(v["planeta"], ctx, 0.85, 0.5)


@receta("sfx_magia_canalizar", S, "La bruja canaliza y el Ascendente se prepara: el metal del planeta crece en 0.6 s "
        "(cristal frotado, gotas, cuenco frotado, fuego, acero frotado, estática, pedal de plomo)",
        "`ActionUsed` (Hechizo), después del ritual", var(planeta=PLANETAS_ORDEN), nivel="A", objetivo=-20)
def canalizar(ctx, v):
    p = v["planeta"]
    d = 0.7
    f = f_planeta(p, 5)
    lz = Lienzo(d + 0.2)
    sube = lambda x: x * env_de(x, [(0, 0), (d * 0.85, 1), (d, 0.6), (d + 0.15, 0)])  # noqa: E731
    lz.add(N(swell(d, ctx.hijo("sw"), 400, 2500)), 0, -12)
    if p == "luna":
        lz.add(N(sube(copa_frotada(f, d + 0.15, ctx))), 0, -2)
    elif p == "mercurio":
        for k in range(6):
            lz.add(gota(f * (1 + 0.25 * k), ctx.hijo(f"g{k}"), 0.4 + 0.1 * k), 0.08 * k, -4)
        lz.add(N(sube(copa_frotada(f * 2, d + 0.15, ctx.hijo("c")))), 0, -10)
    elif p == "venus":
        lz.add(N(sube(cuenco(f, d + 0.15, ctx, frotado=True))), 0, -2)
    elif p == "sol":
        lz.add(N(sube(fuego(d + 0.15, ctx, crepita=0.8))), 0, -4)
        lz.add(N(sube(copa_frotada(f * 2, d + 0.15, ctx.hijo("c")))), 0, -10)
    elif p == "marte":
        t = tiempo(d + 0.15)
        x = sum(a * np.sin(2 * np.pi * f * r * t) for r, a in [(1, 1), (2.76, 0.5), (5.4, 0.2)])
        x = x + 0.2 * bp(blanco(d + 0.15, ctx), 2000, 7000)
        lz.add(N(sube(x)), 0, -3)
    elif p == "jupiter":
        lz.add(N(sube(estatica(d + 0.15, ctx, 0.6))), 0, -4)
        lz.add(N(sube(grito_estano(d + 0.15, ctx.hijo("g"), 300))), 0, -8)
    else:
        lz.add(N(sube(dron(d + 0.15, ctx, f / 4, (2, 3, 4), 0.5))), 0, -3)
        for k in range(3):
            lz.add(N(tic_reloj(ctx.hijo(f"t{k}"), 0.8)), 0.15 + 0.2 * k, -10)
    return lz.senal()


SIGNOS = ["aries", "tauro", "geminis", "cancer", "leo", "virgo", "libra", "escorpio", "sagitario", "capricornio",
          "acuario", "piscis"]


@receta("sfx_magia_ascendente_aparece", S, "El Ascendente aparece con la firma de su signo: cuernos de hierro (Aries), "
        "piedra y cobre (Tauro), eco en espejo (Géminis), caparazón (Cáncer), corona de fuego (Leo), espigas (Virgo), "
        "platos de balanza (Libra), aguijón de obsidiana (Escorpio), cuerda de arco (Sagitario), agua y pezuña "
        "(Capricornio), cántaro (Acuario), dos peces de luz (Piscis)", "`anim:ascendente_invoca`",
        var(signo=SIGNOS), nivel="A", objetivo=-19)
def ascendente_aparece(ctx, v):
    s = v["signo"]
    lz = Lienzo(1.0)
    lz.add(N(whoosh(0.3, ctx.hijo("w"), 300, 2500, q=1.2, pico=0.85)), 0, -10)
    t = 0.25
    if s == "aries":
        lz.add(N(metal("hierro", hz("A3"), 0.5, ctx.hijo("a"), t60=0.5)), t, -2)
        lz.add(N(metal("hierro", hz("D4"), 0.5, ctx.hijo("b"), t60=0.5)), t + 0.07, -3)
        lz.add(N(golpe_sordo(ctx.hijo("g"), 110, 50, 0.3, 0.3)), t, -6)
    elif s == "tauro":
        lz.add(N(piedra(ctx.hijo("p"), 0.4, 1.0, 0.8)), t, -2)
        lz.add(N(cuenco(hz("F3"), 0.7, ctx.hijo("c"), t60=1.2)), t + 0.02, -4)
    elif s == "geminis":
        x = metal("azogue", hz("A5"), 0.4, ctx.hijo("a"), t60=0.5, golpe=0.6)
        lz.add(N(x), t, -2)
        lz.add(N(x[::-1].copy()) * 0.8, t + 0.18, -6)
        lz.add(N(metal("azogue", hz("A6"), 0.3, ctx.hijo("b"), t60=0.3)), t + 0.36, -6)
    elif s == "cancer":
        for k in range(2):
            lz.add(N(madera(ctx.u(1500, 1900), ctx.hijo(f"c{k}"), 0.08, 0.03)), t + 0.06 * k, -3)
        lz.add(N(chapoteo(ctx.hijo("a"), 0.4, 0.6, 1.3)), t + 0.1, -8)
    elif s == "leo":
        lz.add(N(fuego(0.6, ctx.hijo("f"), 0.6)) * env_campana(0.6, 0.3), t - 0.1, -4)
        for k, n in enumerate(["G4", "B4", "D5"]):
            lz.add(N(metal("oro", hz(n), 0.6, ctx.hijo(f"o{k}"), t60=0.7)), t + 0.02 * k, -6)
    elif s == "virgo":
        lz.add(tramo(fuente("kenney_impact_sounds/footstep_grass_001.ogg", velocidad=1.1, hp_hz=800), 0.4), t - 0.1, -6)
        lz.add(N(metal("plata", hz("E5"), 0.6, ctx.hijo("p"), t60=0.8, batido=0.7)), t + 0.1, -4)
    elif s == "libra":
        lz.add(N(metal("oro", hz("C5"), 0.6, ctx.hijo("a"), t60=0.7)), t, -3)
        lz.add(N(metal("oro", hz("C5") * 1.007, 0.6, ctx.hijo("b"), t60=0.7)), t + 0.16, -4)
        lz.add(N(cadena_hierro(0.3, ctx.hijo("c"), 5, 3500)), t, -14)
    elif s == "escorpio":
        lz.add(N(raspado(0.25, ctx.hijo("r"), 1.5, (2800, 4100, 6200))), t - 0.05, -6)
        lz.add(N(metal("plata", hz("C7"), 0.3, ctx.hijo("p"), t60=0.25, golpe=1.0)), t + 0.2, -3)
    elif s == "sagitario":
        x = cuerda(hz("D3"), 0.6, ctx.hijo("cuerda"), "laud", 1.0)
        tt = tiempo(0.6)
        lz.add(N(x) * np.exp(-tt / 0.25), t, -3)
        lz.add(N(whoosh(0.2, ctx.hijo("flecha"), 1500, 5000, q=2.0, pico=0.4)), t + 0.02, -6)
    elif s == "capricornio":
        lz.add(N(chapoteo(ctx.hijo("a"), 0.5, 0.7, 0.8)), t, -6)
        lz.add(N(madera(ctx.u(900, 1100), ctx.hijo("p"), 0.1, 0.04)), t + 0.05, -6)
        lz.add(N(metal("plomo", hz("D3"), 0.5, ctx.hijo("c"), t60=0.6)), t, -6)
    elif s == "acuario":
        lz.add(N(burbujeo(0.6, ctx.hijo("b"), 140, 500, 2500)), t - 0.05, -4)
        lz.add(N(copa_frotada(hz("A5"), 0.6, ctx.hijo("c"))), t, -8)
    else:  # piscis
        for k, f in enumerate([hz("F6"), hz("D6")]):
            lz.add(N(metal("plata", f, 0.5, ctx.hijo(f"p{k}"), t60=0.6, golpe=0.3)), t + 0.12 * k, -4)
            lz.add(N(burbujeo(0.3, ctx.hijo(f"b{k}"), 60, 1500, 4000)), t + 0.12 * k, -12)
    return sala(lz.senal(), 0.12, 1.0)


@receta("sfx_magia_golpe_astral", S, "Impacto del Golpe Astral con la escalera de pureza: Turbia es ruido y grano, "
        "Destilada es tono con textura, Quintaesencia es tono puro con aura (tabla 3.3)", "`HitLanded` (origen Hechizo)",
        var(planeta=PLANETAS_ORDEN, pureza=ORDEN_PUREZAS), nivel="A", objetivo=-16,
        notas="Las brujas del prototipo son de grado B: usan `destilada`. El Vislumbre de B lleva a A, también Destilada.")
def golpe_astral_(ctx, v):
    return golpe_astral(v["planeta"], v["pureza"], ctx)


@receta("sfx_magia_autohechizo", S, "Aguja y Sello: agujas de cristal, un siseo breve y el sello que se estampa (sin "
        "gore). Hilos: cuerdas que se tensan y suben. Frenesí: Sal Negra que cruje y el gruñido grave del Ascendente "
        "(cuenco frotado, no voz)", "`ActionUsed` (Autohechizo, `SelfCastForm`)",
        var(forma=["aguja_sello", "hilos", "frenesi"], v=[1]), nivel="A", objetivo=-18)
def autohechizo(ctx, v):
    f = v["forma"]
    lz = Lienzo(1.1)
    if f == "aguja_sello":
        for k in range(4 if v["v"] == 1 else 3):
            ff = ctx.u(5000, 8000)
            lz.add(N(modal(0.1, [ff, ff * 1.5], [1, 0.4], [0.05, 0.03], ctx.hijo(f"a{k}"))), 0.04 * k, -6)
            lz.add(N(whoosh(0.06, ctx.hijo(f"w{k}"), 3000, 9000, q=2.5, pico=0.8)), 0.04 * k - 0.0 if k else 0, -12)
        t = 0.25
        lz.add(N(golpe_sordo(ctx.hijo("sello"), 140, 70, 0.3, 0.6)), t, -2)
        lz.add(N(metal("oro", grado(3, 4), 0.5, ctx.hijo("o"), t60=0.6)), t, -6)
        lz.add(N(hp(blanco(0.25, ctx.hijo("siseo")), 3500)) * env_de(np.zeros(n_(0.25)), [(0, 0), (0.01, 1), (0.25, 0)]),
               t + 0.01, -14)
    elif f == "hilos":
        for k, n in enumerate(["D3", "A3", "D4"]):
            t = tiempo(0.8)
            x = cuerda(hz(n), 0.8, ctx.hijo(f"c{k}"), "salterio", 0.8)
            x = cambiar_velocidad(x, 1.0)
            lz.add(N(x), 0.05 * k, -4 - 2 * k)
        sube = tiempo(0.6)
        f0 = hz("D5") * (1 + 0.5 * sube / 0.6) if v["v"] == 1 else hz("A4") * (1 + 0.5 * sube / 0.6)
        tens = np.sin(2 * np.pi * np.cumsum(f0) / SR) + 0.3 * np.sin(4 * np.pi * np.cumsum(f0) / SR)
        lz.add(N(tens * env_de(tens, [(0, 0), (0.1, 1), (0.6, 0.0)])), 0.15, -10)
        lz.add(tramo(fuente("kenney_rpg_audio/creak_3.ogg", velocidad=1.4, hp_hz=600), 0.25), 0.2, -14)
    else:
        lz.add(N(crujido_sal(ctx.hijo("s"), 0.3, 0.72, fracturas=3, grave=1.0)), 0, -2)
        g = cuenco(hz("D3") * (1 if v["v"] == 1 else 1.06), 0.9, ctx.hijo("g"), frotado=True, gemido=1.0)
        lz.add(N(g), 0.1, -4)
        lz.add(N(lp(marron(0.8, ctx.hijo("r")), 200) * am_aleatoria(0.8, ctx.hijo("am"), 8, 25, 0.9)
                 * env_campana(0.8, 0.4)), 0.15, -8)
    return lz.senal()


@receta("sfx_magia_vislumbre", S, "Vislumbre: 2 s de fusión con la forma del grado siguiente. Sube un barrido, el "
        "metal se transforma y cae un acorde: Turbia con grano, Destilada con tono y textura, Quintaesencia pura y con "
        "aura", "`ActionUsed` (Vislumbre); el valor es la fase del grado siguiente",
        var(fase=ORDEN_PUREZAS), nivel="A", objetivo=-17)
def vislumbre(ctx, v):
    fase = v["fase"]
    d = 2.0
    lz = Lienzo(d + 0.3)
    lz.add(N(swell(1.2, ctx.hijo("sw"), 200, 4000, 0.9)), 0, -6)
    lz.add(N(crujido_sal(ctx.hijo("s"), 1.0, {"turbia": 0.72, "destilada": 0.85, "quintaesencia": 0.98}[fase],
                         granos=1.5, fracturas=1, grave=1.0)), 0.1, -12)
    lz.add(N(cuenco(hz("D3"), 1.4, ctx.hijo("c"), frotado=True)), 0.2, -8)
    t = 1.15
    acorde = {"turbia": [0, 3, 7], "destilada": [0, 7, 12, 15], "quintaesencia": [0, 7, 12, 16, 19]}[fase]
    for k, semi in enumerate(acorde):
        m = metal("oro" if fase != "turbia" else "hierro", hz("D4") * 2 ** (semi / 12), 1.0, ctx.hijo(f"a{k}"),
                  t60=1.2)
        lz.add(N(m), t + 0.03 * k, -5 - k)
    lz.add(N(impacto(ctx.hijo("i"), 0.5)), t, -6)
    if fase == "turbia":
        lz.add(N(crepitar(0.6, ctx.hijo("cr"), 60)), t, -12)
    elif fase == "quintaesencia":
        lz.add(N(destello(ctx.hijo("d"), 0.8, 5000, 8)), t + 0.05, -8)
    x = lz.senal()
    if fase == "quintaesencia":
        x = coro(x, ctx.hijo("coro"), 2, 8)
    return sala(x, 0.18, 1.2)


@receta("sfx_magia_sal_negra", S, "Capa de Sal Negra: crujido húmedo y grave, y un cuenco frotado hasta que gime",
        "`ActionUsed` (gasta Sal Negra)", var(v=[1]), nivel="A", objetivo=-18)
def sal_negra(ctx, v):
    lz = Lienzo(1.2)
    cr = crujido_sal(ctx.hijo("s"), 0.3, 0.72, fracturas=2, grave=1.5)
    lz.add(N(lp(cr, 3000)), 0, -2)
    lz.add(N(burbujeo(0.3, ctx.hijo("b"), 50, 150, 500, xi=0.05)), 0.02, -12)
    lz.add(N(cuenco(hz("C3") * (1 if v == 1 else 1.03), 1.0, ctx.hijo("c"), frotado=True, gemido=1.0)), 0.1, -4)
    return lz.senal()


@receta("sfx_magia_ascendente_retira", S, "El Ascendente se retira: brillo que se recoge hacia atrás y un barrido que "
        "baja", "`anim:ascendente_retira`", var(v=[1]), nivel="A", objetivo=-22)
def ascendente_retira(ctx, v):
    lz = Lienzo(0.8)
    d = destello(ctx.hijo("d"), 0.5, 4000, 5, 0)
    lz.add(N(d[::-1].copy()), 0, -6)
    lz.add(N(whoosh(0.5, ctx.hijo("w"), 2500, 300, q=1.1, pico=0.3)), 0.2, -4)
    return lz.senal()
