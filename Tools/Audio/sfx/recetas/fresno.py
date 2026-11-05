# -*- coding: utf-8 -*-
"""3.5.9 Fresno. La tonalidad sube contigo por el árbol: Raíces en Re, Tronco en Mi, Ramas en Sol, Copa en La."""
import numpy as np

from sintesis import (SR, Lienzo, metal, madera, cuenco, copa_frotada, crujido_madera, crujido_sal, burbujeo, gota,
                      golpe_sordo, whoosh, viento, pluma, papel, dron, crepitar, blanco, rosa, marron, bp, hp, lp,
                      env_de, exp_de, env_campana, grado, hz, sala, fuente, tramo, n_, tiempo, am_aleatoria,
                      cambiar_velocidad, eco, PLANETAS)
from . import receta, var
from .comun import N, nota_planeta, destello, swell, f_planeta, PLANETAS_ORDEN

S = "fresno"
ZONAS = {"raices": "D", "tronco": "E", "ramas": "G", "copa": "A"}


def _hojas(ctx, dur, nivel=0.0):
    x = tramo(fuente("kenney_impact_sounds/footstep_grass_00%d.ogg" % ctx.i(0, 2), velocidad=ctx.u(0.9, 1.2),
                     hp_hz=900), dur)
    return x * 10 ** (nivel / 20)


@receta("sfx_fresno_zona_entrar", S, "Entrar a una zona del Fresno: Raíces (madera húmeda y cuenco grave en Re), Tronco "
        "(ceniza que cruje, Mi), Ramas (viento entre hojas, Sol), Copa (aire alto y cristal, La)", "`f2:zona_entrar`",
        var(zona=list(ZONAS)), objetivo=-18)
def zona_entrar(ctx, v):
    z = v["zona"]
    raiz = hz(ZONAS[z] + "3")
    lz = Lienzo(1.8)
    lz.add(N(swell(0.5, ctx.hijo("sw"), 200, 1800, 0.9)), 0, -10)
    if z == "raices":
        lz.add(N(crujido_madera(0.8, ctx.hijo("c"), (15, 50), (220, 480, 900))), 0.1, -8)
        lz.add(N(cuenco(raiz, 1.4, ctx.hijo("b"), t60=2.0)), 0.4, -3)
        lz.add(N(burbujeo(0.6, ctx.hijo("g"), 20, 200, 800, xi=0.05)), 0.3, -16)
    elif z == "tronco":
        lz.add(N(crepitar(0.8, ctx.hijo("cr"), 40, 1000, 6000)), 0.1, -10)
        lz.add(N(metal("hierro", raiz * 2, 1.2, ctx.hijo("b"), t60=1.2)), 0.4, -4)
        lz.add(N(cuenco(raiz, 1.3, ctx.hijo("c"), t60=1.8)), 0.42, -6)
    elif z == "ramas":
        lz.add(N(viento(1.5, ctx.hijo("v"), 900, 2.0)), 0, -10)
        lz.add(_hojas(ctx.hijo("h"), 0.6), 0.3, -10)
        for k, semi in enumerate([0, 7, 12]):
            lz.add(N(metal("oro", raiz * 2 * 2 ** (semi / 12), 1.2, ctx.hijo(f"o{k}"), t60=1.2)), 0.4 + 0.08 * k, -5 - k)
    else:
        lz.add(N(viento(1.5, ctx.hijo("v"), 2500, 1.5)), 0, -12)
        for k, semi in enumerate([0, 7, 12, 16]):
            lz.add(N(copa_frotada(raiz * 4 * 2 ** (semi / 12), 1.2, ctx.hijo(f"c{k}"))), 0.3 + 0.06 * k, -6 - k)
        lz.add(N(destello(ctx.hijo("d"), 0.8, 5000, 6)), 0.5, -12)
    return sala(lz.senal(), 0.15, 0.8)


@receta("sfx_fresno_nodo_elegir", S, "Elegir un nodo del mapa: toque de madera y una hoja", "`ui:nodo_elegir`",
        var(v=[1, 2]), objetivo=-24)
def nodo_elegir(ctx, v):
    lz = Lienzo(0.3)
    lz.add(N(madera(ctx.u(700, 900) * (1 if v == 1 else 1.12), ctx, 0.15, 0.06)), 0, -2)
    lz.add(_hojas(ctx.hijo("h"), 0.18), 0.0, -14)
    return lz.senal()


@receta("sfx_fresno_ruta", S, "La ruta se dibuja en el mapa: trazo largo de pluma sobre pergamino", "`ui:ruta_trazo`",
        var(v=[1]), objetivo=-24)
def ruta(ctx, v):
    return pluma(0.55, ctx, trazos=1 + v, punto=False)


@receta("sfx_fresno_injerto", S, "Elegir un Injerto: madera que acepta la rama y la nota del metal de su planeta",
        "`f2:injerto_elegido` (planeta de la Rama)", var(planeta=PLANETAS_ORDEN), objetivo=-20)
def injerto(ctx, v):
    p = v["planeta"]
    lz = Lienzo(0.8)
    lz.add(N(crujido_madera(0.2, ctx.hijo("c"), (40, 120), (500, 1100, 2000))), 0, -8)
    lz.add(nota_planeta(p, f_planeta(p, 5), ctx.hijo("n"), 0.7, None), 0.08, -2)
    return lz.senal()


@receta("sfx_fresno_fruto", S, "Se activa un Fruto (tres Injertos de una Rama): arpegio de tres notas del metal del "
        "planeta y hojas que se abren", "`f2:fruto_activado`", var(planeta=PLANETAS_ORDEN), objetivo=-17)
def fruto(ctx, v):
    p = v["planeta"]
    g = PLANETAS[p]["grado"]
    lz = Lienzo(1.2)
    for k, d in enumerate([0, 2, 4]):
        lz.add(nota_planeta(p, grado(g + d, 5), ctx.hijo(f"n{k}"), 0.9, None), 0.09 * k, -2 - k)
    lz.add(_hojas(ctx.hijo("h"), 0.5), 0.15, -12)
    lz.add(N(destello(ctx.hijo("d"), 0.5, 4000, 4)), 0.2, -14)
    return lz.senal()


@receta("sfx_fresno_mutacion", S, "Aceptar una Mutación del Árbol: corteza que se retuerce y un cuenco desafinado",
        "`f2:mutacion`", var(v=[1]), objetivo=-19)
def mutacion(ctx, v):
    lz = Lienzo(1.2)
    lz.add(N(crujido_madera(0.7, ctx.hijo("c"), (20, 140), (300, 700, 1500))), 0, -4)
    lz.add(N(cuenco(hz("D3") * (1.03 if v == 1 else 0.97), 1.0, ctx.hijo("b"), t60=1.5, batido=5.0)), 0.2, -6)
    lz.add(N(burbujeo(0.4, ctx.hijo("s"), 40, 150, 600, xi=0.05)), 0.3, -14)
    return lz.senal()


@receta("sfx_fresno_maldicion", S, "Aceptar una maldición: campana de plomo y bronce, más grave con cada una (Re4, Do4, "
        "Si♭3, La3, Sol3)", "`f2:maldicion_aceptada` (número de maldiciones, 1 a 5)", var(maldicion=[1, 2, 3, 4, 5]),
        objetivo=-19)
def maldicion(ctx, v):
    n = v["maldicion"]
    f = hz(["D4", "C4", "Bb3", "A3", "G3"][n - 1])
    lz = Lienzo(1.1)
    lz.add(N(metal("oro", f, 1.0, ctx.hijo("b"), t60=1.0, brillo=0.55)), 0, -2)
    lz.add(N(metal("plomo", f / 2, 0.5, ctx.hijo("p"), t60=0.5)), 0, -6)
    lz.add(N(dron(1.0, ctx.hijo("d"), f / 4, (2, 3), 0.4)) * env_de(np.zeros(n_(1.0)), [(0, 1), (1.0, 0)]), 0, -16 + n)
    return lz.senal()


@receta("sfx_fresno_santuario", S, "Guardar almitas en un Santuario: cuenco cálido y un racimo suave de campanitas",
        "`f2:almitas_guardadas`", var(v=[1]), objetivo=-18)
def santuario(ctx, v):
    lz = Lienzo(1.3)
    lz.add(N(cuenco(grado(0, 4), 1.2, ctx.hijo("c"), t60=2.0)), 0, -3)
    for k, g in enumerate([7, 9, 11, 14]):
        lz.add(N(metal("plata", grado(g, 5, [0, 2, 4, 5, 7, 9, 11]), 0.6, ctx.hijo(f"a{k}"), t60=0.6, golpe=0.3)),
               0.15 + 0.07 * k, -8 - k)
    return sala(lz.senal(), 0.15, 0.8)


@receta("sfx_fresno_descanso", S, "Elegir descanso en el Santuario: cuenco suave y un soplo de viento",
        "`f2:santuario_eleccion` (descanso)", var(v=[1]), objetivo=-20)
def descanso(ctx, v):
    lz = Lienzo(1.2)
    lz.add(N(viento(1.1, ctx.hijo("v"), 500, 1.5, 0.3)) * env_campana(1.1, 0.4), 0, -10)
    lz.add(N(cuenco(grado(2, 4), 1.0, ctx.hijo("c"), t60=1.8)), 0.1, -3)
    return lz.senal()


@receta("sfx_fresno_entrenar", S, "Elegir entrenamiento en el Santuario: dos golpes de báculo y una campana de hierro",
        "`f2:santuario_eleccion` (entrenar)", var(v=[1]), objetivo=-20)
def entrenar(ctx, v):
    lz = Lienzo(1.1)
    lz.add(fuente("kenney_impact_sounds/impact_wood_light_000.ogg", hp_hz=120), 0, -2)
    lz.add(fuente("kenney_impact_sounds/impact_wood_light_002.ogg", hp_hz=120), 0.18, -3)
    lz.add(N(metal("hierro", grado(4, 4), 0.8, ctx.hijo("h"), t60=0.8)), 0.36, -4)
    return lz.senal()


@receta("sfx_fresno_mancha_aparece", S, "La Mancha de Sal aparece en su nodo: sal oscura que se extiende con un crepitar "
        "grave y un pedal que sube", "`f2:mancha_en_nodo`", var(v=[1]), objetivo=-20)
def mancha_aparece(ctx, v):
    lz = Lienzo(1.4)
    lz.add(N(crujido_sal(ctx.hijo("s"), 0.9, 0.72, granos=1.0, fracturas=0, grave=1.0)), 0, -4)
    lz.add(N(dron(1.2, ctx.hijo("d"), hz("D1"), (2, 3, 4), 0.4)) * env_de(np.zeros(n_(1.2)), [(0, 0), (0.8, 1), (1.2, 0)]),
           0, -8)
    return lz.senal()


@receta("sfx_fresno_mancha_recuperar", S, "Recuperar las almitas perdidas: cascada de campanitas que sube y un brillo "
        "de sal", "`f2:mancha_recuperada`", var(v=[1]), objetivo=-17)
def mancha_recuperar(ctx, v):
    lz = Lienzo(1.3)
    for k, g in enumerate([0, 2, 4, 7, 9, 11, 14]):
        lz.add(N(metal("plata", grado(g, 5, [0, 2, 4, 5, 7, 9, 11]), 0.6, ctx.hijo(f"a{k}"), t60=0.6, golpe=0.4)),
               0.06 * k, -4 - 0.5 * k)
    lz.add(N(crujido_sal(ctx.hijo("s"), 0.3, 0.97, fracturas=1)), 0.42, -8)
    return sala(lz.senal(), 0.12, 0.8)


@receta("sfx_fresno_mancha_absorbida", S, "El Fresno absorbe la Mancha y devuelve 30 % como Savia: madera que gime, "
        "savia que baja y una campanita", "`f2:mancha_absorbida`", var(v=[1]), objetivo=-19)
def mancha_absorbida(ctx, v):
    lz = Lienzo(1.5)
    lz.add(N(crujido_madera(1.0, ctx.hijo("c"), (12, 40), (180, 380, 760))), 0, -4)
    lz.add(N(burbujeo(0.8, ctx.hijo("b"), 50, 120, 500, xi=-0.05)), 0.2, -10)
    lz.add(N(metal("plata", grado(4, 5), 0.6, ctx.hijo("p"), t60=0.6, golpe=0.2)), 0.9, -8)
    return lz.senal()


@receta("sfx_fresno_eco", S, "Guardar un Eco: nota de cristal que se repite en ecos que se apagan", "`f2:eco_guardado`",
        var(v=[1]), objetivo=-21)
def eco_(ctx, v):
    x = metal("plata", grado(4, 5), 0.25, ctx, t60=0.3, golpe=0.4)
    return eco(N(x), 0.14, 0.5, 4, 4500)


@receta("sfx_fresno_ofrenda", S, "Ofrenda: el árbol crece, madera que cruje hacia arriba, hojas y un acorde que se abre",
        "`f2:ofrenda`", var(v=[1]), objetivo=-19)
def ofrenda(ctx, v):
    lz = Lienzo(1.4)
    lz.add(N(crujido_madera(0.8, ctx.hijo("c"), (20, 120), (400, 900, 1800))), 0, -6)
    lz.add(_hojas(ctx.hijo("h"), 0.6), 0.3, -10)
    for k, g in enumerate([0, 4, 7] if v == 1 else [2, 4, 9]):
        lz.add(N(cuenco(grado(g, 4), 1.0, ctx.hijo(f"c{k}"), t60=1.6)), 0.4 + 0.1 * k, -5 - k)
    return lz.senal()


def _lecho(ctx, dur, zona):
    from proceso import bucle
    d = dur + 0.6
    lz = Lienzo(d)
    if zona == "raices":
        lz.add(N(lp(marron(d, ctx.hijo("r")), 300)), 0, -6)
        for k in range(5):
            lz.add(gota(ctx.u(900, 1800), ctx.hijo(f"g{k}"), 0.5), ctx.u(0, d - 0.3), -8)
        cr = crujido_madera(d, ctx.hijo("c"), (6, 20), (200, 420, 800))
        lz.add(N(cr) * am_aleatoria(d, ctx.hijo("cm"), 0, 0.7, 1.0)[: len(cr)] ** 3, 0, -14)  # crujidos sueltos
    elif zona == "tronco":
        lz.add(N(viento(d, ctx.hijo("v"), 400, 1.6, 0.4)), 0, -6)
        lz.add(N(crepitar(d, ctx.hijo("cr"), 8, 1200, 6000)), 0, -16)
    elif zona == "ramas":
        lz.add(N(viento(d, ctx.hijo("v"), 900, 2.0, 0.6)), 0, -6)
        lz.add(N(bp(blanco(d, ctx.hijo("h")), 2500, 8000) * am_aleatoria(d, ctx.hijo("am"), 2, 9, 0.9)), 0, -16)
    else:
        lz.add(N(viento(d, ctx.hijo("v"), 2000, 1.6, 0.5)), 0, -8)
        lz.add(N(copa_frotada(hz("A5"), d, ctx.hijo("c"))) * env_campana(d, 0.5, 1.0), 0, -18)
    return bucle(lz.senal()[: n_(d)], 0.6)


for _z in ZONAS:
    def _reg(z):
        @receta(f"sfx_ambiente_{z}", S, {"raices": "Lecho de las Raíces: humedad grave, gotas y madera que cruje lejos",
                                         "tronco": "Lecho del Tronco: viento hueco y ceniza que crepita",
                                         "ramas": "Lecho de las Ramas: viento entre hojas",
                                         "copa": "Lecho de la Copa: aire alto y un cristal que respira"}[z],
                "Carga de escena", var(v=[1]), objetivo=-30, metrica="I", bucle=True, estado="borrador",
                notas="Bucle corto de 2 s por el tope de tamaño; el lecho final va en streaming (6.1).")
        def _fn(ctx, v):
            return _lecho(ctx, 2.0, z)
    _reg(_z)
