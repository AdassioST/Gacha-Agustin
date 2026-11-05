# -*- coding: utf-8 -*-
"""3.5.10 Termas y tirada del gacha. Las Termas son zona segura: todo suave y sin tensión.

Incluye los cuatro stingers de vapor de la ficha P05 de SOUNDTRACK_SUNO.md, sintetizados en Re a 80 BPM
(ya no se hacen con Suno): mus_tirada_vapor_blanco, _violeta, _dorado y mus_tirada_subida_dorada.
"""
import numpy as np

from sintesis import (SR, Lienzo, metal, madera, cuenco, copa_frotada, cuerda, rasgueo, arco, crujido_sal, _granos,
                      burbuja, burbujeo, gota, chapoteo, escurrir, vapor, hervor, pelaje, plumas, aleteo, golpe_sordo,
                      whoosh, viento, susurro, papel, hoja, blanco, rosa, marron, bp, hp, lp, env_de, exp_de,
                      env_campana, grado, hz, sala, fuente, tramo, n_, tiempo, am_aleatoria, cambiar_velocidad,
                      RE_MAYOR, estereo_ancho)
from . import receta, var
from .comun import N, destello, swell
from .combate import ESPECIES

S = "termas"
BEAT = 60.0 / 80.0  # 80 BPM (ficha P05)


# --------------------------------------------------------------------------- lechos y Termas


def _agua_quieta(ctx, d):
    lap = lp(rosa(d, ctx.hijo("l")), 900) * am_aleatoria(d, ctx.hijo("am"), 0.2, 1.2, 0.8)
    lz = Lienzo(d)
    lz.add(N(lap), 0, -6)
    for k in range(int(d * 2)):
        lz.add(gota(ctx.u(700, 1500), ctx.hijo(f"g{k}"), ctx.u(0.2, 0.5)), ctx.u(0, d - 0.3), -12)
    lz.add(N(hp(blanco(d, ctx.hijo("v")), 3500)) * am_aleatoria(d, ctx.hijo("vm"), 0, 0.6, 0.6), 0, -20)
    return lz.senal()[: n_(d)]


@receta("sfx_ambiente_termas_dia", S, "Lecho de las Termas de día: agua que lame la piedra, gotas, vapor y una brisa",
        "Carga de escena (hora local de 6:00 a 18:59)", var(v=[1]), objetivo=-30, metrica="I", bucle=True,
        estado="borrador", notas="Bucle corto por el tope de tamaño; el lecho final es estéreo en streaming.")
def amb_dia(ctx, v):
    from proceso import bucle
    d = 2.2
    lz = Lienzo(d)
    lz.add(_agua_quieta(ctx.hijo("a"), d), 0)
    lz.add(N(viento(d, ctx.hijo("w"), 700, 1.5, 0.4)), 0, -14)
    return bucle(lz.senal()[: n_(d)], 0.6)


@receta("sfx_ambiente_termas_noche", S, "Lecho de las Termas de noche: agua más quieta, gotas espaciadas y grillos lejanos",
        "Carga de escena (hora local de 19:00 a 5:59)", var(v=[1]), objetivo=-31, metrica="I", bucle=True,
        estado="borrador", notas="Bucle corto por el tope de tamaño; el lecho final es estéreo en streaming.")
def amb_noche(ctx, v):
    from proceso import bucle
    d = 2.2
    lz = Lienzo(d)
    lz.add(_agua_quieta(ctx.hijo("a"), d), 0, -2)
    t = 0.1
    while t < d - 0.3:  # grillos: trinos cortos a 4.6 kHz
        tt = tiempo(0.12)
        tr = np.sin(2 * np.pi * 4600 * tt) * (0.5 + 0.5 * np.sign(np.sin(2 * np.pi * 45 * tt))) * env_campana(0.12, 0.3)
        lz.add(lp(tr, 7000), t, -22 + ctx.u(-4, 0))
        t += ctx.u(0.35, 0.8)
    return bucle(lz.senal()[: n_(d)], 0.6)


@receta("sfx_termas_tortuga_respira", S, "La tortuga gigante respira: inhalación y exhalación lentas y graves, de aire, "
        "sin voz", "Temporizador de escena (cada 20 a 40 s)", var(v=[1]), objetivo=-26)
def tortuga(ctx, v):
    d = 2.2
    x = rosa(d, ctx)
    y = bp(x, 150, 900) * env_de(x, [(0, 0), (0.8, 1), (1.0, 0.7), (1.1, 0.9), (2.0, 0.0), (d, 0)])
    y = lp(y, 700 if v == 1 else 550)
    return y


@receta("sfx_termas_tocar", S, "Tocar a una bruja (cabeza, mano, sombrero, cola): una palmadita suave en pelaje, plumas, "
        "membranas, escamas o lana", "`ui:termas_tocar` (`Especie`)", var(especie=ESPECIES), objetivo=-22)
def tocar(ctx, v):
    e = v["especie"]
    lz = Lienzo(0.45)
    lz.add(tramo(fuente("kenney_rpg_audio/cloth_4.ogg", velocidad=1.1, hp_hz=250), 0.25), 0, -8)
    if e in ("ave",):
        lz.add(N(plumas(0.3, ctx.hijo("p"))) * env_campana(0.3, 0.2), 0, -4)
    elif e == "alada":
        lz.add(N(aleteo(ctx.hijo("a"), 1, 0.1, 1.6)), 0, -6)
    elif e == "escamosa":
        x = bp(blanco(0.3, ctx), 1200, 5000) * am_aleatoria(0.3, ctx.hijo("am"), 30, 90, 0.8) * env_campana(0.3, 0.3)
        lz.add(N(x), 0, -6)
    else:
        br = {"canida": 1.0, "felina": 1.1, "ungulada": 0.7, "pequena": 1.3, "robusta": 0.6}[e]
        lz.add(N(pelaje(0.3, ctx.hijo("p"), br)) * env_campana(0.3, 0.25), 0, -4)
    lz.add(N(golpe_sordo(ctx.hijo("g"), 260, 160, 0.08, 0.0, 0.2)), 0.02, -14)
    return lz.senal()


@receta("sfx_termas_bano", S, "Entrar y salir del agua termal: chapoteo suave, burbujas y agua que escurre",
        "`ui:bano`", var(accion=["entrar", "salir"]), objetivo=-21)
def bano(ctx, v):
    lz = Lienzo(1.3)
    if v["accion"] == "entrar":
        lz.add(N(chapoteo(ctx.hijo("c"), 0.8, 1.0, 0.8)), 0, -2)
        lz.add(N(burbujeo(0.8, ctx.hijo("b"), 60, 300, 1500)), 0.1, -10)
    else:
        lz.add(N(whoosh(0.3, ctx.hijo("w"), 300, 900, q=1.0, pico=0.6)), 0, -10)
        lz.add(N(chapoteo(ctx.hijo("c"), 0.5, 0.6, 1.1)), 0.15, -4)
        lz.add(N(escurrir(0.8, ctx.hijo("e"), 30)), 0.3, -4)
    return lz.senal()


@receta("sfx_termas_paralaje_baja", S, "El baño baja el Paralaje: los susurros se apagan bajo el agua y queda una nota "
        "limpia de cristal", "`meta:paralaje_baja` en las Termas", var(v=[1]), objetivo=-21)
def termas_paralaje_baja(ctx, v):
    lz = Lienzo(2.1)
    s = susurro(1.1, ctx.hijo("s"), 5, registro=0.9)
    lz.add(N(lp(s, 1800)) * env_de(s, [(0, 0.8), (1.1, 0)]), 0, -14)
    lz.add(N(burbujeo(1.2, ctx.hijo("b"), 50, 300, 1600)), 0.2, -10)
    lz.add(N(copa_frotada(grado(2, 5, RE_MAYOR), 1.4, ctx.hijo("c"))), 0.6, -6)
    return sala(lz.senal(), 0.18, 0.8)


# --------------------------------------------------------------------------- tirada


def _punado(ctx, d, densidad):
    sn = tramo(fuente("kenney_impact_sounds/footstep_snow_00%d.ogg" % ctx.i(0, 3), velocidad=ctx.u(1.3, 1.6),
                      hp_hz=1500), min(d, 0.25))
    g = _granos(d, ctx.hijo("g"), lambda t: densidad * np.exp(-t / (d * 0.5)) + 30, 2500, 11000, ping=0.3)
    return Lienzo(d).add(N(sn), 0, -4.4).add(N(g), 0, -6).senal()


@receta("sfx_tirada_tomar", S, "La Calculista toma Sales de Baño: un puñado chico (1) o grande (10) que cruje en la mano",
        "`ui:tirada`", var(cantidad=[1, 10]), objetivo=-22)
def tirada_tomar(ctx, v):
    return _punado(ctx, 0.3 if v["cantidad"] == 1 else 0.6, 500 if v["cantidad"] == 1 else 1400)


@receta("sfx_tirada_sales_caer", S, "Las sales caen al manantial: un 'plop' y granos (1), o una cascada de plops (10)",
        "`tl:sales_caen`", var(cantidad=[1], v=[1, 2]) + var(cantidad=[10], v=[1]), objetivo=-20)
def sales_caer(ctx, v):
    if v["cantidad"] == 1:
        lz = Lienzo(0.6)
        lz.add(N(chapoteo(ctx.hijo("c"), 0.4, 0.5, 1.4)), 0, -4)
        lz.add(gota(ctx.u(700, 1000), ctx.hijo("g"), 1.0), 0, -2)
        return lz.senal()
    lz = Lienzo(1.4)
    for k in range(10):
        t = 0.05 + k * 0.09 * ctx.u(0.7, 1.3)
        lz.add(gota(ctx.u(600, 1300), ctx.hijo(f"g{k}"), ctx.u(0.6, 1.0)), t, -2)
        lz.add(N(chapoteo(ctx.hijo(f"c{k}"), 0.25, 0.4, 1.5)), t, -10)
    return lz.senal()


@receta("sfx_tirada_burbujas", S, "Las sales se disuelven: efervescencia fina que sube", "`tl:disolver`", var(v=[1]),
        objetivo=-22)
def burbujas(ctx, v):
    x = burbujeo(1.2, ctx, 260, 1500, 6500, tasa_env=lambda t: max(min(t / 0.3, 1.0), 0.1) * np.exp(-t / 0.9), xi=0.2)
    return x


@receta("sfx_tirada_vapor", S, "Vapor por rareza (suena con el color, nunca antes): blanco es un siseo suave; violeta "
        "suma brillos de cristal en Si♭; dorado es una columna más grande con destellos de oro en Re mayor",
        "`tl:vapor` (`Rareza`)", var(rareza=[3, 4, 5]), objetivo=lambda v: {3: -20, 4: -19, 5: -18}[v["rareza"]])
def tirada_vapor(ctx, v):
    r = v["rareza"]
    d = 1.5
    lz = Lienzo(d + 0.2)
    x = vapor(d, ctx.hijo("v"), 1.0 if r < 5 else 0.8, 0.4 + 0.2 * (r - 3))
    lz.add(N(x) * env_de(x, [(0, 0), (0.25, 1), (d * 0.6, 0.7), (d, 0)]), 0, -3)
    if r == 4:
        for k, n in enumerate(["Bb5", "D6", "F6"]):
            lz.add(N(copa_frotada(hz(n), 0.9, ctx.hijo(f"c{k}"))), 0.25 + 0.12 * k, -12)
    if r == 5:
        lz.add(N(swell(0.6, ctx.hijo("s"), 200, 1500, 0.8)), 0, -8)
        for k, n in enumerate(["D6", "F#6", "A6", "D7"]):
            lz.add(N(metal("oro", hz(n), 0.8, ctx.hijo(f"o{k}"), t60=0.8)), 0.3 + 0.1 * k, -12)
        lz.add(N(destello(ctx.hijo("d"), 0.8, 5000, 7)), 0.35, -14)
    return lz.senal()


@receta("sfx_tirada_subida", S, "El vapor pasa de violeta a dorado: un barrido que sube y los brillos cambian de cristal "
        "a oro", "`tl:vapor_sube`", var(v=[1]), objetivo=-17)
def tirada_subida(ctx, v):
    lz = Lienzo(2.0)
    lz.add(N(swell(1.0, ctx.hijo("s"), 300, 5000, 0.9)), 0, -6)
    lz.add(N(vapor(1.6, ctx.hijo("v"), 0.9, 0.5)) * env_campana(1.6, 0.6), 0, -10)
    for k, n in enumerate(["Bb5", "D6", "F6"]):
        lz.add(N(copa_frotada(hz(n), 0.6, ctx.hijo(f"c{k}"))), 0.1 + 0.15 * k, -12)
    for k, n in enumerate(["D6", "F#6", "A6", "D7"]):
        lz.add(N(metal("oro", hz(n), 0.8, ctx.hijo(f"o{k}"), t60=0.8)), 0.95 + 0.07 * k, -6 - k)
    return lz.senal()


@receta("sfx_tirada_quietud", S, "El agua se aquieta: el rumor se apaga hasta el silencio y queda un solo brillo",
        "`tl:agua_quieta`", var(v=[1]), objetivo=-25)
def quietud(ctx, v):
    lz = Lienzo(1.6)
    a = _agua_quieta(ctx.hijo("a"), 0.9)
    lz.add(N(a) * env_de(a, [(0, 1), (0.9, 0)]), 0, -6)
    lz.add(N(metal("plata", hz("A6"), 0.8, ctx.hijo("p"), t60=0.9, golpe=0.2)), 0.95, -4)
    return lz.senal()


@receta("sfx_tirada_constelacion", S, "5★: la constelación de su Ascendente se dibuja; cada estrella toca una nota "
        "(firma provisional en Re mayor pentatónico)", "`tl:constelacion`", var(v=[1]), objetivo=-19,
        estado="borrador",
        notas="Borrador: la firma real son los dos primeros compases del leitmotiv de cada 5★ (solista aislado), que "
              "todavía no existen. Reemplazar por bruja (`__marisal`, etc.) cuando haya leitmotiv.")
def constelacion(ctx, v):
    lz = Lienzo(2.5)
    notas = ["A5", "D6", "E6", "F#6", "A6", "D7"]
    for k, n in enumerate(notas):
        t = k * BEAT / 2
        lz.add(N(metal("plata", hz(n), 0.8, ctx.hijo(f"e{k}"), t60=0.9, golpe=0.4)), t, -4)
        lz.add(N(metal("oro", hz(n) * 2, 0.4, ctx.hijo(f"o{k}"), t60=0.4)), t, -14)
    return sala(lz.senal(), 0.18, 0.8)


@receta("sfx_tirada_silueta", S, "Una silueta bajo el agua: oleaje sordo que sube y burbujas apagadas",
        "`tl:silueta`", var(v=[1]), objetivo=-22)
def silueta(ctx, v):
    lz = Lienzo(1.4)
    x = lp(rosa(1.2, ctx.hijo("r")), 400)
    lz.add(N(x) * env_de(x, [(0, 0), (0.8, 1), (1.2, 0)]), 0, -2)
    lz.add(N(lp(burbujeo(1.0, ctx.hijo("b"), 30, 200, 900), 1200)), 0.2, -8)
    return lz.senal()


def _emerge(ctx, e):
    lz = Lienzo(1.5)
    sube = lp(rosa(0.35, ctx.hijo("s")), 1200)
    lz.add(N(sube) * env_de(sube, [(0, 0), (0.3, 1), (0.35, 0.2)]), 0, -8)
    lz.add(N(chapoteo(ctx.hijo("c"), 0.5, 1.0, 0.9)), 0.27, -2)
    lz.add(N(escurrir(0.7, ctx.hijo("e"), 35 if e != "robusta" else 25)), 0.35, -5)
    if e == "ave":
        lz.add(N(plumas(0.3, ctx.hijo("p"))) * env_campana(0.3, 0.3), 0.4, -10)
    elif e == "alada":
        lz.add(N(aleteo(ctx.hijo("a"), 2, 0.12, 1.2)), 0.45, -10)
    elif e == "escamosa":
        x = bp(blanco(0.4, ctx.hijo("d")), 800, 4000) * env_campana(0.4, 0.3)
        lz.add(N(x), 0.3, -12)
    else:
        lz.add(N(pelaje(0.3, ctx.hijo("p"), 0.8)) * env_campana(0.3, 0.3), 0.4, -12)
    return lz.senal()


@receta("sfx_tirada_emergencia", S, "La bruja emerge del manantial: el agua sube, se abre y escurre de su pelaje, plumas, "
        "alas o escamas", "`tl:emerge` (`Especie`, `Rareza`)", var(especie=ESPECIES), objetivo=-18,
        notas="Una variante por grupo de especie; la rareza la pone la música (`mus_tirada_emergencia_5`).")
def emergencia(ctx, v):
    return _emerge(ctx, v["especie"])


@receta("sfx_tirada_sacudida", S, "Se sacude según su especie: las cánidas enteras (ráfagas de agua y orejas), las felinas "
        "breve y molesta, las aves esponjan las plumas, las aladas aletean, las escamosas escurren, las unguladas sacuden "
        "la cabeza, las pequeñas rápido y agudo, las robustas lento y pesado", "`tl:sacudida` (`Especie`)",
        var(especie=ESPECIES), objetivo=-19)
def sacudida(ctx, v):
    e = v["especie"]
    lz = Lienzo(1.1)
    cfg = {"canida": (6, 0.075, 1.0), "felina": (3, 0.07, 1.1), "ave": (4, 0.06, 1.3), "alada": (3, 0.1, 1.0),
           "escamosa": (2, 0.12, 0.8), "ungulada": (4, 0.1, 0.8), "pequena": (7, 0.05, 1.4),
           "robusta": (4, 0.13, 0.7)}[e]
    n, per, br = cfg
    for k in range(n):
        t = 0.02 + k * per
        if e == "ave":
            lz.add(N(plumas(per * 1.4, ctx.hijo(f"p{k}"))) * env_campana(per * 1.4, 0.4), t, -3)
        elif e == "alada":
            lz.add(N(aleteo(ctx.hijo(f"a{k}"), 1, per, 1.2)), t, -3)
        else:
            lz.add(N(whoosh(per * 1.3, ctx.hijo(f"w{k}"), 400 * br, 2200 * br, q=1.0, pico=0.5)), t, -6)
            lz.add(N(pelaje(per * 1.3, ctx.hijo(f"f{k}"), br)) * env_campana(per * 1.3, 0.5), t, -6)
        for j in range(3):  # gotas que salen volando
            lz.add(gota(ctx.u(1500, 4000), ctx.hijo(f"g{k}{j}"), 0.4), t + ctx.u(0, per), -14)
    if e == "canida":
        for k in range(2):  # orejas que golpean
            lz.add(N(golpe_sordo(ctx.hijo(f"o{k}"), 320, 200, 0.05, 0.1, 0.2)), 0.1 + k * 0.15, -12)
    if e == "felina":  # se lame la pata una vez: un roce seco y breve
        x = bp(blanco(0.06, ctx.hijo("l")), 900, 2600) * env_campana(0.06, 0.35)
        lz.add(N(x), 0.4, -12)
    if e == "ungulada":
        lz.add(N(madera(ctx.u(650, 800), ctx.hijo("cuerno"), 0.12, 0.05)), 0.22, -12)
    return lz.senal()


@receta("sfx_tirada_tarjeta", S, "Aparece su tarjeta: pergamino que se desliza y una campanita según la rareza (plata, "
        "díada violeta en Si♭ y Re, tríada de oro en Re mayor)", "`tl:tarjeta` (`Rareza`)", var(rareza=[3, 4, 5]),
        objetivo=-20)
def tarjeta(ctx, v):
    r = v["rareza"]
    lz = Lienzo(0.9)
    lz.add(tramo(fuente("kenney_rpg_audio/book_flip_2.ogg", velocidad=1.3, hp_hz=600), 0.2), 0, -8)
    notas = {3: ["A5"], 4: ["Bb5", "D6"], 5: ["D6", "F#6", "A6"]}[r]
    for k, n in enumerate(notas):
        lz.add(N(metal("plata" if r < 5 else "oro", hz(n), 0.6, ctx.hijo(f"n{k}"), t60=0.6)), 0.08 + 0.05 * k, -4 - k)
    return lz.senal()


@receta("sfx_tirada_saltar", S, "Saltar la animación: un barrido de pergamino breve", "`ui:saltar` (tirada)",
        var(v=[1]), objetivo=-24)
def tirada_saltar(ctx, v):
    lz = Lienzo(0.4)
    lz.add(N(whoosh(0.25, ctx.hijo("w"), 800, 3500, q=1.4, pico=0.7)), 0, -4)
    lz.add(tramo(fuente("kenney_rpg_audio/book_flip_3.ogg", velocidad=1.4, hp_hz=900), 0.15), 0.05, -6)
    return lz.senal()


@receta("sfx_tirada_termometro", S, "El agua se calienta con el pity (bucle): hervor suave; FMOD sube la intensidad con "
        "el parámetro `Pity`", "`ui:termometro` (pity 0 a 70)", var(v=[1]), objetivo=-30, metrica="I", bucle=True,
        estado="borrador", notas="Un solo bucle tibio; la región que hierve (pity suave) se arma en FMOD subiendo "
                                 "el tono y la densidad, o con un segundo bucle en producción.")
def termometro(ctx, v):
    from proceso import bucle
    d = 2.0
    x = hervor(d, ctx, 0.4)
    return bucle(x[: n_(d)], 0.6)


@receta("sfx_tirada_polvo_astral", S, "El resultado se vuelve Polvo Astral: un brillo que se dispersa hacia arriba",
        "`tl:polvo_astral`", var(v=[1]), objetivo=-21)
def polvo_astral(ctx, v):
    lz = Lienzo(1.2)
    lz.add(N(whoosh(0.8, ctx.hijo("w"), 1500, 7000, q=1.0, pico=0.3)), 0, -10)
    lz.add(N(destello(ctx.hijo("d"), 0.8, 5000, 10)), 0, -2)
    return lz.senal()


# --------------------------------------------------------------------------- stingers de vapor (ficha P05)


def _tremolo(ctx, notas, t0, t1, paso=BEAT / 8, nivel=-8):
    lz = Lienzo(t1 + 1.5)
    t = t0
    k = 0
    while t < t1:
        f = hz(notas[k % len(notas)])
        lz.add(N(cuerda(f, 0.6, ctx.hijo(f"t{k}"), "salterio", ctx.u(0.6, 0.9))), t, nivel)
        t += paso
        k += 1
    return lz.senal()


def _cascada(ctx, notas, t0, paso=BEAT / 2, nivel=-6):
    lz = Lienzo(t0 + len(notas) * paso + 1.2)
    for k, n in enumerate(notas):
        lz.add(N(metal("oro", hz(n), 1.1, ctx.hijo(f"c{k}"), t60=1.1)), t0 + k * paso, nivel - 0.5 * k)
    return lz.senal()


def _arpegio(ctx, notas, t0, paso=BEAT / 4, tipo="salterio", nivel=-4):
    lz = Lienzo(t0 + len(notas) * paso + 1.5)
    for k, n in enumerate(notas):
        lz.add(N(cuerda(hz(n), 1.4, ctx.hijo(f"a{k}"), tipo)), t0 + k * paso, nivel)
    return lz.senal()


def _cuencos(ctx, notas, t0, dur, nivel=-10):
    lz = Lienzo(t0 + dur + 0.2)
    for k, n in enumerate(notas):
        lz.add(N(cuenco(hz(n), dur, ctx.hijo(f"b{k}"), frotado=True)), t0, nivel - k)
    return lz.senal()


NOTAS_P05 = "Música (ficha P05 de SOUNDTRACK_SUNO.md), sintetizada aquí por decisión del usuario: Re y 80 BPM. El " \
            "coro sin palabras de la ficha se reemplaza por cuencos frotados (sin voces). Mono por el tope de tamaño."


@receta("mus_tirada_vapor_blanco", S, "Stinger 3★ en Re menor: rasgueo suave de guitarra de nailon y una campanita, con "
        "la cola de la capilla", "`tl:vapor` (3★), después del vapor", var(v=[1]), objetivo=-17,
        notas=NOTAS_P05)
def vapor_blanco(ctx, v):
    lz = Lienzo(3.0)
    lz.add(N(rasgueo([hz(n) for n in ["D3", "A3", "D4", "F4", "A4", "D5"]], 2.6, ctx.hijo("g"), "guitarra", 0.025)), 0,
           -2)
    lz.add(N(metal("plata", hz("A5"), 1.6, ctx.hijo("b"), t60=1.6, golpe=0.4)), BEAT, -8)
    return sala(lz.senal(), 0.22, 0.8)


@receta("mus_tirada_vapor_violeta", S, "Stinger 4★: arpegio de salterio que sube y se posa en Si♭ mayor, luego Fa mayor, "
        "con campanitas y cuencos que crecen", "`tl:vapor` (4★)", var(v=[1]), objetivo=-17,
        notas=NOTAS_P05)
def vapor_violeta(ctx, v):
    lz = Lienzo(4.0)
    lz.add(_arpegio(ctx.hijo("a"), ["Bb3", "D4", "F4", "Bb4", "D5", "F5"], 0.0, BEAT / 4), 0, -3)
    lz.add(N(rasgueo([hz(n) for n in ["Bb2", "F3", "Bb3", "D4", "F4"]], 1.6, ctx.hijo("g1"), "guitarra")), 1.5 * BEAT, -4)
    lz.add(N(rasgueo([hz(n) for n in ["F2", "C3", "F3", "A3", "C4", "F4"]], 2.0, ctx.hijo("g2"), "guitarra")), 3 * BEAT,
           -4)
    lz.add(_cuencos(ctx.hijo("c"), ["F4", "A4", "C5"], 1.0, 2.4), 0, -6)
    for k, n in enumerate(["F6", "A6", "C7"]):
        lz.add(N(metal("plata", hz(n), 1.0, ctx.hijo(f"b{k}"), t60=1.0, golpe=0.4)), 3 * BEAT + 0.1 * k, -12)
    return sala(lz.senal(), 0.22, 0.8)


@receta("mus_tirada_vapor_dorado", S, "Stinger 5★: florece un acorde radiante de Re mayor: raíz de violonchelo, trémolo "
        "de salterio, cascada de campanas agudas y cuencos que crecen", "`tl:vapor` (5★)", var(v=[1]), objetivo=-16, notas=NOTAS_P05)
def vapor_dorado(ctx, v):
    lz = Lienzo(5.0)
    cello = arco(hz("D2"), 3.6, ctx.hijo("vc"), ataque=0.4)
    lz.add(N(cello), 0, -6)
    lz.add(N(rasgueo([hz(n) for n in ["D3", "A3", "D4", "F#4", "A4", "D5"]], 3.0, ctx.hijo("g"), "guitarra", 0.03)),
           0, -4)
    lz.add(_tremolo(ctx.hijo("t"), ["A4", "D5", "F#5", "D5"], BEAT, 4 * BEAT), 0, -6)
    lz.add(_cascada(ctx.hijo("c"), ["D7", "A6", "F#6", "D6", "A5", "F#5"], BEAT * 0.5, BEAT / 2), 0, -6)
    lz.add(_cuencos(ctx.hijo("b"), ["D4", "F#4", "A4"], 0.3, 3.4), 0, -8)
    lz.add(N(destello(ctx.hijo("d"), 1.0, 5000, 8)), BEAT * 0.5, -14)
    return sala(lz.senal(), 0.24, 0.8)


@receta("mus_tirada_subida_dorada", S, "Stinger de la subida sorpresa: empieza como el arpegio suave de Si♭ mayor, se "
        "levanta y modula a un Re mayor brillante con campanas", "`tl:vapor_sube` (en lugar del violeta)", var(v=[1]),
        objetivo=-16, notas=NOTAS_P05)
def subida_dorada(ctx, v):
    lz = Lienzo(6.0)
    lz.add(_arpegio(ctx.hijo("a"), ["Bb3", "D4", "F4", "Bb4", "D5", "F5", "Bb5"], 0.0, BEAT / 4), 0, -4)
    lz.add(N(rasgueo([hz(n) for n in ["Bb2", "F3", "Bb3", "D4", "F4"]], 1.4, ctx.hijo("g1"), "guitarra")), 1.5 * BEAT,
           -6)
    lz.add(N(swell(1.0, ctx.hijo("sw"), 300, 4000, 0.9)), 2 * BEAT, -8)
    lz.add(N(rasgueo([hz(n) for n in ["A2", "E3", "A3", "C#4", "E4", "A4"]], 1.0, ctx.hijo("g2"), "guitarra")), 3 * BEAT,
           -6)
    t = 4 * BEAT
    lz.add(N(arco(hz("D2"), 2.6, ctx.hijo("vc"), ataque=0.3)), t, -6)
    lz.add(N(rasgueo([hz(n) for n in ["D3", "A3", "D4", "F#4", "A4", "D5"]], 2.6, ctx.hijo("g3"), "guitarra", 0.03)), t,
           -3)
    lz.add(_tremolo(ctx.hijo("t"), ["A4", "D5", "F#5", "D5"], t, t + 3 * BEAT), 0, -7)
    lz.add(_cascada(ctx.hijo("c"), ["D7", "A6", "F#6", "D6", "A5"], t, BEAT / 2), 0, -6)
    lz.add(_cuencos(ctx.hijo("b"), ["D4", "F#4", "A4"], t, 2.2), 0, -9)
    return sala(lz.senal(), 0.24, 0.8)
