# -*- coding: utf-8 -*-
"""Piezas compartidas entre sistemas: notas planetarias, golpe astral, cola de la sal, brillo."""
import math

import numpy as np

from sintesis import (SR, Lienzo, PLANETAS, metal, modal, cuenco, copa_frotada, gota, burbujeo,
                      crujido_sal, fuego, crepitar, grito_estano, estatica, trueno_lamina, raspado,
                      tic_reloj, golpe_sordo, chasquido, whoosh, blanco, rosa, bp, hp, lp, env_exp,
                      env_puntos, env_campana, eco, coro, sala, grado, hz, fuente, normalizar,
                      ajustar, n_, tiempo, am_aleatoria, cambiar_velocidad, papel)

PUREZAS = {"turbia": 0.72, "destilada": 0.85, "quintaesencia": 0.98}
ORDEN_PUREZAS = ["turbia", "destilada", "quintaesencia"]
PLANETAS_ORDEN = ["sol", "luna", "marte", "mercurio", "venus", "jupiter", "saturno"]

# Octava del Re de referencia por papel de la Cadena (3.4: Apertura grave, Enlace media, Cierre aguda).
OCTAVA_PAPEL = {"apertura": 4, "enlace": 5, "cierre": 6}


def f_planeta(planeta, octava=5):
    """Frecuencia del grado del planeta en Re menor (Luna I = Re ... Saturno VII = Do)."""
    return grado(PLANETAS[planeta]["grado"], octava)


def N(x):
    return normalizar(x)


def nota_planeta(planeta, f, ctx, dur=0.5, sostener=0.15, fuerza=1.0):
    """Nota con el timbre del metal del planeta: discreta, corta y afinada.

    sostener: segundos antes de apagar la nota (las notas de la Partitura duran 150 ms).
    """
    if planeta == "luna":
        x = metal("plata", f, dur, ctx, t60=1.4, batido=0.9, golpe=0.6, ataque=0.002)
    elif planeta == "mercurio":
        t = tiempo(dur)
        g = 1 - 0.035 * np.exp(-t / 0.012) + 0.003 * np.sin(2 * np.pi * 7 * t)
        x = modal(dur, [f, 2 * f, 3 * f], [1, 0.14, 0.05], [0.7, 0.3, 0.15], ctx, glide=g, ataque=0.001)
        pl = gota(f * 2.0, ctx.hijo("plip"), 0.4)
        x = Lienzo(dur).add(N(x)).add(pl, 0.0).senal()
    elif planeta == "venus":
        x = cuenco(f, dur, ctx, t60=2.0, batido=1.0)
    elif planeta == "sol":
        x = metal("oro", f, dur, ctx, t60=1.1, brillo=1.05)
    elif planeta == "marte":
        x = metal("hierro", f, dur, ctx, t60=0.55)
    elif planeta == "jupiter":
        x = metal("estano", f, dur, ctx, t60=0.9)
    else:  # saturno
        x = metal("plomo", f, dur, ctx, t60=0.7)
        x = x + 0.25 * N(metal("plata", f, dur, ctx.hijo("eco"), t60=0.4, golpe=0))
    x = N(x)
    if sostener is not None:
        apaga = env_puntos(dur, [(0, 1), (sostener, 1), (min(sostener + 0.18, dur), 0.0), (dur, 0.0)])
        x = x * apaga
    return x * fuerza


def impacto(ctx, peso=1.0, brillo=1.0):
    """Impacto genérico con cuerpo: golpe grave, chasquido y aire."""
    dur = 0.25 + 0.2 * peso
    lz = Lienzo(dur)
    lz.add(golpe_sordo(ctx, 100 + 40 * (1 - peso), 42 + 10 * (1 - peso), dur, click=0.0), 0, -2)
    c = chasquido(ctx.hijo("c"), 0.004, 500, 12000 * brillo)
    lz.add(N(c), 0, -3)
    aire = lp(blanco(0.12, ctx.hijo("a")), 2500 * brillo) * env_exp(0.12, 0.07)
    lz.add(N(aire), 0, -8)
    return lz.senal()


def destello(ctx, dur=0.6, f_base=4000, notas=6, nivel=-8):
    """Brillo de cristal: varias campanitas agudas al azar (brillo dorado o de sal)."""
    lz = Lienzo(dur)
    for k in range(notas):
        f = f_base * ctx.u(0.8, 2.2)
        m = metal("plata", f, 0.35, ctx.hijo(f"d{k}"), t60=0.3, golpe=0.3)
        lz.add(N(m), ctx.u(0, dur * 0.5), nivel + ctx.u(-6, 0))
    return lz.senal()


def cola_planeta(planeta, ctx, dur=0.55):
    """Cola del metal del planeta para el crujido de sal (3.2): 300 a 600 ms."""
    f = f_planeta(planeta, 5)
    lz = Lienzo(dur)
    if planeta == "marte":       # chispa y placa de acero
        lz.add(N(metal("hierro", f, dur, ctx, t60=0.5)), 0, 0)
        ch = crepitar(0.08, ctx.hijo("ch"), 300, 3000, 12000)
        lz.add(N(ch), 0, -10)
    elif planeta == "luna":      # crótalo suave
        lz.add(N(metal("plata", f * 2, dur, ctx, t60=0.9, batido=0.7, golpe=0.3)), 0)
    elif planeta == "sol":       # chisporroteo
        lz.add(N(metal("oro", f * 2, dur, ctx, t60=0.6)), 0, -2)
        lz.add(N(crepitar(dur * 0.7, ctx.hijo("cr"), 70, 2000, 9000)) * env_exp(dur * 0.7, 0.4), 0, -6)
    elif planeta == "saturno":   # golpe sordo y tic de reloj
        lz.add(N(metal("plomo", f / 2, dur, ctx, t60=0.4)), 0)
        lz.add(N(tic_reloj(ctx.hijo("tic"), 0.9)), 0.22, -6)
    elif planeta == "jupiter":   # grito del estaño y estática
        lz.add(N(grito_estano(dur * 0.6, ctx.hijo("g"), 500)), 0, -3)
        lz.add(N(metal("estano", f, dur, ctx.hijo("m"), t60=0.6, golpe=0.2)), 0.01, -4)
    elif planeta == "venus":     # cuenco cantor
        lz.add(N(cuenco(f, dur, ctx, t60=1.6)), 0)
    else:                        # mercurio: gotas
        for k in range(3):
            lz.add(gota(f * (1.5 + 0.5 * k), ctx.hijo(f"g{k}"), 0.8), 0.03 + 0.06 * k)
        lz.add(N(metal("azogue", f * 2, dur, ctx.hijo("m"), t60=0.5, golpe=0)), 0, -6)
    return lz.senal()


def crujido_planeta(planeta, ctx, pureza=0.85, dur_cola=0.5, grave=0.0):
    """sfx_sal_crujido: crujido de 120 a 250 ms con la cola del metal a −8 dB."""
    lz = Lienzo(0.3 + dur_cola)
    cr = crujido_sal(ctx.hijo("sal"), dur=0.2, pureza=pureza, fracturas=2, grave=grave)
    lz.add(N(cr), 0)
    sn = fuente("kenney_impact_sounds/impact_glass_light_00%d.ogg" % ctx.i(0, 3), velocidad=ctx.u(1.1, 1.4),
                hp_hz=1500)
    lz.add(sn * env_exp(len(sn) / SR, 0.08), 0.0, -14)
    tono = pureza >= 0.85
    cola_db = -8 if not tono else -8 + 4 * (pureza - 0.85) / 0.15
    lz.add(N(cola_planeta(planeta, ctx.hijo("cola"), dur_cola)), 0.05, cola_db)
    if pureza >= 0.97:  # armónico limpio: la sal suena casi a campana
        lz.add(N(metal("plata", f_planeta(planeta, 6), 0.5, ctx.hijo("limpio"), t60=0.7, golpe=0)), 0.04, -10)
    return lz.senal()


def golpe_astral(planeta, pureza, ctx):
    """Impacto del Golpe Astral con la escalera de pureza de 3.3 (Turbia, Destilada, Quintaesencia)."""
    f = f_planeta(planeta, 4)
    lz = Lienzo(0.7)
    lz.add(impacto(ctx.hijo("imp"), 0.6), 0, -9 if pureza != "turbia" else -7)
    h = ctx.hijo(planeta)
    if planeta == "luna":
        if pureza == "turbia":      # Bruma Salina: siseo filtrado
            s = bp(blanco(0.6, h), 2500, 7000) * env_puntos(0.6, [(0, 0), (0.03, 1), (0.6, 0)])
            lz.add(N(s), 0, -6)
            lz.add(N(copa_frotada(f * 2, 0.6, h.hijo("c"))), 0, -16)
        elif pureza == "destilada":  # Marea de Plata: tono que va y viene
            x = metal("plata", f * 2, 0.7, h, t60=1.5, batido=3.0, golpe=0.6)
            x = x * (0.6 + 0.4 * np.sin(2 * np.pi * 6 * tiempo(0.7)))
            lz.add(N(x), 0, -4)
            lz.add(N(whoosh(0.35, h.hijo("w"), 1500, 600, q=1.2, pico=0.2)), 0, -12)
        else:                        # Espejo Lunar: senoidal limpia con eco
            t = tiempo(0.3)
            s = np.sin(2 * np.pi * f * 2 * t) * env_exp(0.3, 0.6, 0.002)
            lz.add(N(eco(s, 0.11, 0.5, 4, 6000)), 0, -3)
    elif planeta == "mercurio":
        if pureza == "turbia":      # Humo Ácido: burbujeo y siseo
            lz.add(N(burbujeo(0.5, h, 160, 600, 3000)), 0, -4)
            lz.add(N(bp(blanco(0.5, h.hijo("s")), 3000, 9000) * env_exp(0.5, 0.35)), 0, -10)
        elif pureza == "destilada":  # Azogue Vivo: gotas afinadas que se mueven
            for k, g_ in enumerate([0, 2, 4, 1]):
                lz.add(nota_planeta("mercurio", grado(1 + g_, 5), h.hijo(f"n{k}"), 0.35, 0.08), 0.01 + 0.055 * k,
                       -3 - k)
        else:                        # Mente Relámpago: arpegio veloz
            for k, g_ in enumerate([1, 3, 5, 8, 10, 12]):
                lz.add(N(metal("azogue", grado(g_, 5), 0.3, h.hijo(f"a{k}"), t60=0.35, golpe=0.4)), 0.025 * k, -4)
    elif planeta == "venus":
        if pureza == "turbia":      # Cardenillo: cuenco desafinado con ruido
            lz.add(N(cuenco(f * 1.03, 0.7, h, t60=1.2, batido=6.0)), 0, -4)
            lz.add(N(bp(rosa(0.4, h.hijo("r")), 400, 3000) * env_exp(0.4, 0.3)), 0, -10)
        elif pureza == "destilada":  # Savia de Cobre: cuenco limpio y hojas
            lz.add(N(cuenco(f, 0.7, h, t60=2.0)), 0, -3)
            hoj = fuente("kenney_impact_sounds/footstep_grass_00%d.ogg" % h.i(0, 2), velocidad=1.2, hp_hz=1500)
            lz.add(hoj[: n_(0.4)] * env_exp(0.4, 0.3), 0.0, -12)
        else:                        # Jardín Esmeralda: acorde de cuencos y brotes
            for k, g_ in enumerate([2, 4, 6]):
                lz.add(N(cuenco(grado(g_, 4), 0.7, h.hijo(f"c{k}"), t60=2.2)), 0.012 * k, -5)
            lz.add(N(crepitar(0.3, h.hijo("b"), 50, 3000, 9000)), 0.05, -16)
    elif planeta == "sol":
        if pureza == "turbia":      # Ceniza Salobre: crepitar con ruido
            lz.add(N(fuego(0.6, h, crepita=1.5, rugido=0.6)) * env_exp(0.6, 0.5, 0.005), 0, -4)
        elif pureza == "destilada":  # Fuego Pálido: fuego limpio con campanilla
            lz.add(N(whoosh(0.5, h, 300, 1800, q=1.0, pico=0.15)), 0, -6)
            lz.add(N(fuego(0.5, h.hijo("f"), crepita=0.4)) * env_exp(0.5, 0.4, 0.01), 0, -8)
            lz.add(N(metal("oro", f * 4, 0.6, h.hijo("c"), t60=0.8)), 0.01, -6)
        else:                        # Corona Solar: acorde mayor de campanas (Sol mayor sobre IV)
            for k, semi in enumerate([0, 4, 7, 12]):
                lz.add(N(metal("oro", f * 2 * 2 ** (semi / 12), 0.7, h.hijo(f"c{k}"), t60=1.0)), 0.008 * k, -5)
    elif planeta == "marte":
        if pureza == "turbia":      # Herrumbre: raspado y polvo
            lz.add(N(raspado(0.4, h, 1.5)), 0, -4)
            lz.add(N(bp(blanco(0.5, h.hijo("p")), 800, 5000) * env_exp(0.5, 0.4)), 0.02, -12)
        elif pureza == "destilada":  # Sangre Férrea: golpe de forja
            lz.add(N(metal("hierro", f, 0.6, h, t60=0.6)), 0, -2)
            lz.add(fuente("kenney_impact_sounds/impact_metal_heavy_00%d.ogg" % h.elegir([1, 3]), hp_hz=200), 0, -6)
        else:                        # Acero Carmesí: hoja de acero frotada con arco
            t = tiempo(0.7)
            x = sum(a * np.sin(2 * np.pi * f * 2 * r * t) for r, a in [(1, 1), (2.76, 0.5), (5.4, 0.25)])
            x = x * env_puntos(0.7, [(0, 0), (0.04, 1), (0.5, 0.6), (0.7, 0)])
            x = x + 0.1 * bp(blanco(0.7, h.hijo("arco")), 2000, 7000) * env_campana(0.7, 0.3)
            lz.add(N(x), 0, -6)
            lz.add(N(metal("hierro", f * 2, 0.5, h.hijo("g"), t60=0.4)), 0, -6)
    elif planeta == "jupiter":
        if pureza == "turbia":      # Estática Gris: crepitar
            lz.add(N(estatica(0.5, h)) * env_exp(0.5, 0.4, 0.003), 0, -4)
        elif pureza == "destilada":  # Trueno de Estaño: lámina y grito
            lz.add(N(trueno_lamina(0.6, h)), 0, -4)
            lz.add(N(grito_estano(0.3, h.hijo("g"))), 0, -6)
        else:                        # Ley Tonante: trueno afinado con campana grave
            lz.add(N(trueno_lamina(0.7, h, afinado=f / 2)), 0, -3)
            lz.add(N(metal("estano", f, 0.7, h.hijo("c"), t60=1.0)), 0, -6)
    else:  # saturno
        if pureza == "turbia":      # Ceniza Gris: golpe sordo
            lz.add(N(golpe_sordo(h, 90, 40, 0.4, 0.5)), 0, -2)
            lz.add(N(lp(blanco(0.3, h.hijo("p")), 1200) * env_exp(0.3, 0.2)), 0, -10)
        elif pureza == "destilada":  # Plomo Lento: golpe estirado en el tiempo
            g_ = metal("plomo", f / 2, 0.35, h, t60=0.4)
            lz.add(N(cambiar_velocidad(g_, 0.6)), 0, -2)
        else:                        # Hora Final: campanada grave de reloj, cortada
            c = metal("oro", f / 2, 0.7, h, t60=2.0, brillo=0.7)
            c = c * env_puntos(0.7, [(0, 1), (0.4, 1), (0.48, 0), (0.7, 0)])
            lz.add(lp(N(c), 3500), 0, -3)
    return lz.senal()


def swell(dur, ctx, f0=300, f1=3000, q=1.0):
    """Subida de aire (reverse swell) para anticipar un momento."""
    return whoosh(dur, ctx, f0, f1, q=q, pico=0.92, forma=2.2)


def campanas_acorde(semitonos, raiz_hz, ctx, dur=1.2, metal_="oro", t60=1.4, arpegio=0.0, nivel=0.0):
    lz = Lienzo(dur)
    for k, s in enumerate(semitonos):
        m = metal(metal_, raiz_hz * 2 ** (s / 12), dur, ctx.hijo(f"b{k}"), t60=t60)
        lz.add(N(m), arpegio * k, nivel - 2 * (k > 0))
    return lz.senal()
