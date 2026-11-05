# -*- coding: utf-8 -*-
"""3.5.11 Libro Mayor e interfaz: pergamino, pluma, cuero, latón y pan de oro."""
import numpy as np

from sintesis import (SR, Lienzo, metal, madera, golpe_sordo, chasquido, whoosh, pluma, papel, hoja, engranes,
                      tic_reloj, susurro, blanco, rosa, bp, hp, lp, env_de, exp_de, env_campana, grado, hz, sala, fuente,
                      tramo, n_, tiempo, RE_MAYOR)
from . import receta, var
from .comun import N, destello

S = "interfaz"


@receta("sfx_ui_tocar", S, "Toque genérico: un golpecito de pergamino y la punta de la pluma", "`ui:tocar`",
        var(v=[1, 2, 3, 4]), nivel="A", objetivo=-25)
def ui_tocar(ctx, v):
    lz = Lienzo(0.15)
    p = papel(0.09, ctx.hijo("p"), 300, 1500, 7000)
    lz.add(N(p) * exp_de(p, 0.08, 0.002), 0, -3)
    lz.add(N(chasquido(ctx.hijo("c"), 0.0015, 2500, 9000)), 0.002, -8)
    lz.add(N(madera(ctx.u(1500, 2100), ctx.hijo("m"), 0.08, 0.03)), 0, -6)
    return lz.senal()


@receta("sfx_ui_confirmar", S, "Confirmar: rúbrica corta de pluma, un toque de madera y una campanita de Re",
        "`ui:confirmar`", var(v=[1, 2, 3]), nivel="A", objetivo=-22)
def ui_confirmar(ctx, v):
    lz = Lienzo(0.45)
    lz.add(N(pluma(0.12, ctx.hijo("p"), 1, punto=False)), 0, -8)
    lz.add(N(madera(ctx.u(1100, 1300), ctx.hijo("m"), 0.08, 0.03)), 0.1, -6)
    lz.add(N(metal("oro", grado([0, 4, 7][v - 1], 5), 0.4, ctx.hijo("o"), t60=0.45)), 0.1, -4)
    return lz.senal()


@receta("sfx_ui_cancelar", S, "Volver: hoja que regresa y un toque más grave", "`ui:cancelar`", var(v=[1, 2, 3]),
        nivel="A", objetivo=-23)
def ui_cancelar(ctx, v):
    lz = Lienzo(0.35)
    lz.add(tramo(fuente("kenney_rpg_audio/book_flip_3.ogg", velocidad=[1.2, 1.35, 1.1][v - 1], hp_hz=500), 0.18), 0, -6)
    lz.add(N(madera(ctx.u(600, 750), ctx.hijo("m"), 0.12, 0.05)), 0.05, -4)
    return lz.senal()


@receta("sfx_ui_error", S, "Acción imposible: tic sordo de plomo, doble y apagado", "`ui:error`", var(v=[1, 2]),
        nivel="A", objetivo=-23)
def ui_error(ctx, v):
    lz = Lienzo(0.3)
    for k in range(2):
        lz.add(N(metal("plomo", hz("D3") * (1 if v == 1 else 0.94), 0.15, ctx.hijo(f"p{k}"), t60=0.12)), 0.09 * k, -2)
        lz.add(N(golpe_sordo(ctx.hijo(f"g{k}"), 160, 110, 0.08, 0.0, 0.3)), 0.09 * k, -10)
    return lz.senal()


@receta("sfx_ui_pestana", S, "Cambiar de pestaña: vuelta de hoja del Libro Mayor", "`ui:pestana`", var(v=[1, 2, 3]),
        objetivo=-24)
def ui_pestana(ctx, v):
    return tramo(fuente("kenney_rpg_audio/book_flip_%d.ogg" % v, velocidad=1.15, hp_hz=300), 0.4)


@receta("sfx_ui_deslizar", S, "Deslizar listas: roce breve de papel (máximo uno cada 80 ms)", "`ui:deslizar`",
        var(v=[1, 2, 3]), objetivo=-28)
def ui_deslizar(ctx, v):
    x = bp(rosa(0.12, ctx), 1500, 8000) * env_campana(0.12, 0.3)
    return x + 0.4 * N(papel(0.12, ctx.hijo("p"), 120)) * np.max(np.abs(x))


@receta("sfx_ui_astrolabio", S, "Menú radial: engranes de latón que avanzan una posición", "`ui:astrolabio_paso`",
        var(v=[1, 2, 3]), objetivo=-26)
def ui_astrolabio(ctx, v):
    return engranes(0.06, ctx, 30, [3000, 3400, 3800][v - 1], True)


@receta("sfx_ui_notificacion", S, "Aviso: dos notas de campanita (La y Re)", "`ui:notificacion`", var(v=[1, 2]),
        objetivo=-21)
def ui_notificacion(ctx, v):
    lz = Lienzo(0.75)
    a, b = ("A5", "D6") if v == 1 else ("F#5", "A5")
    lz.add(N(metal("plata", hz(a), 0.5, ctx.hijo("a"), t60=0.5, golpe=0.4)), 0, -2)
    lz.add(N(metal("plata", hz(b), 0.5, ctx.hijo("b"), t60=0.55, golpe=0.4)), 0.11, -2)
    return lz.senal()


@receta("sfx_ui_recompensa", S, "Recompensa recibida: arpegio de campanas de oro en Re mayor y un roce de pergamino",
        "`ui:recompensa`", var(v=[1, 2]), objetivo=-19)
def ui_recompensa(ctx, v):
    lz = Lienzo(1.0)
    lz.add(tramo(fuente("kenney_rpg_audio/book_place_2.ogg", velocidad=1.3, hp_hz=300), 0.15), 0, -12)
    for k, g in enumerate([0, 2, 4, 7] if v == 1 else [4, 7, 9, 11]):
        lz.add(N(metal("oro", grado(g, 5, RE_MAYOR), 0.7, ctx.hijo(f"o{k}"), t60=0.7)), 0.05 + 0.07 * k, -3 - k)
    return lz.senal()


@receta("sfx_ui_compra", S, "Compra confirmada, neutral: el sello sobre el pergamino y una nota tranquila; sin monedas",
        "`ui:compra_ok`", var(v=[1]), objetivo=-21)
def ui_compra(ctx, v):
    lz = Lienzo(0.9)
    lz.add(N(golpe_sordo(ctx.hijo("s"), 180, 90, 0.15, 0.4, 0.3)), 0, -4)
    lz.add(N(papel(0.1, ctx.hijo("p"), 200)), 0, -12)
    lz.add(N(metal("plata", grado(4, 5), 0.6, ctx.hijo("n"), t60=0.6, golpe=0.2)), 0.05, -6)
    return lz.senal()


@receta("sfx_libro_abrir", S, "Abrir el Libro Mayor: cuero que se abre y un brillo de pan de oro", "`ui:libro_abrir`",
        var(v=[1, 2]), objetivo=-21)
def libro_abrir(ctx, v):
    lz = Lienzo(0.7)
    lz.add(tramo(fuente("kenney_rpg_audio/book_open.ogg", velocidad=[0.9, 1.0][v - 1], hp_hz=150), 0.3), 0, -2)
    lz.add(tramo(fuente("kenney_rpg_audio/handle_small_leather.ogg", velocidad=1.0, hp_hz=300), 0.25), 0.0, -8)
    lz.add(N(destello(ctx.hijo("d"), 0.4, 6000, 4)), 0.1, -14)
    return lz.senal()


@receta("sfx_libro_cerrar", S, "Cerrar el Libro Mayor: tapas de cuero que se juntan", "`ui:libro_cerrar`",
        var(v=[1, 2]), objetivo=-21)
def libro_cerrar(ctx, v):
    lz = Lienzo(0.5)
    lz.add(tramo(fuente("kenney_rpg_audio/book_close.ogg", velocidad=[1.0, 0.9][v - 1], hp_hz=120), 0.25), 0, -2)
    lz.add(tramo(fuente("kenney_rpg_audio/drop_leather.ogg", velocidad=1.2, hp_hz=200), 0.25), 0.01, -10)
    return lz.senal()


@receta("sfx_libro_folio", S, "Una ventana de estado se escribe: pluma que anota números en trazos cortos", "`ui:folio`",
        var(v=[1, 2, 3]), objetivo=-22)
def libro_folio(ctx, v):
    return pluma(0.6, ctx, trazos=2 + v, punto=True)


@receta("sfx_libro_titulo", S, "Título ganado: sello de lacre que se estampa y una campanita", "`ui:titulo`",
        var(v=[1, 2]), objetivo=-19)
def libro_titulo(ctx, v):
    lz = Lienzo(1.2)
    lz.add(N(lp(blanco(0.12, ctx.hijo("cera")), 1500) * env_campana(0.12, 0.4)), 0, -10)
    lz.add(N(golpe_sordo(ctx.hijo("s"), 160, 80, 0.2, 0.5, 0.4)), 0.12, -2)
    lz.add(N(chasquido(ctx.hijo("c"), 0.002, 1500, 6000)), 0.13, -12)
    for k, g in enumerate([0, 4, 7] if v == 1 else [4, 7, 11]):
        lz.add(N(metal("oro", grado(g, 5, RE_MAYOR), 0.9, ctx.hijo(f"o{k}"), t60=0.9)), 0.2 + 0.06 * k, -6 - k)
    return lz.senal()


@receta("sfx_libro_falla", S, "Con Paralaje alto la ventana falla: una sílaba susurrada al revés (ruido formántico, sin "
        "palabras) y papel que se rasga", "`ui:falla`", var(v=[1, 2, 3, 4]), objetivo=-22)
def libro_falla(ctx, v):
    lz = Lienzo(0.7)
    lz.add(N(susurro(0.35, ctx.hijo("s"), 1, reves=True, registro=0.8 + 0.1 * v)), 0, -4)
    rasgo = papel(0.25, ctx.hijo("r"), 900, 700, 6000)
    lz.add(N(rasgo) * env_de(rasgo, [(0, 0.3), (0.05, 1), (0.25, 0)]), 0.3, -4)
    return lz.senal()
