# -*- coding: utf-8 -*-
"""3.5.1 Combate."""
import numpy as np

from sintesis import (Lienzo, metal, madera, piedra, golpe_sordo, chasquido, whoosh, pelaje, plumas, aleteo,
                      cadena_hierro, raspado, crujido_sal, crujido_madera, engranes, tic_reloj, fuego, crepitar,
                      burbujeo, chapoteo, grito_estano, estatica, cuenco, copa_frotada, dron, blanco, rosa, marron,
                      bp, hp, lp, env_exp, tramo, env_de, exp_de, env_puntos, env_campana, fuente, grado, hz, sala, n_, tiempo,
                      cambiar_velocidad, am_aleatoria, Ctx)
from . import receta, var
from .comun import N, impacto, destello, swell, campanas_acorde

S = "combate"

FAMILIAS = ["baculo", "grimorio_agujas", "guadana_sal", "incensario", "astrolabio", "rifle_alquimico", "escoba",
            "campana_bronce", "tijeras_hilandera", "caldero_mano", "varita", "daga_ritual"]
MATERIALES = ["pelaje", "sal", "metal_liquido", "madera", "astral", "piedra"]
ESPECIES = ["canida", "felina", "ave", "alada", "escamosa", "ungulada", "pequena", "robusta"]
PESO_OBJ = {"ligero": -19.0, "medio": -16.5, "pesado": -14.0}


def _e(x, t60, ataque=0.0005):
    """Envolvente exponencial del largo exacto de x."""
    from sintesis import SR
    return x * env_exp(len(x) / SR, t60, ataque)[: len(x)]


@receta("sfx_combate_entrada", S, "Barrido de aire que cae en un golpe de tambor de marco, campana de hierro en Re y "
        "un brillo de sal", "`ui:combate_inicia` (o `BattleStarted`)", var(v=[1, 2]), nivel="A", objetivo=-17)
def entrada(ctx, v):
    lz = Lienzo(1.4)
    lz.add(N(swell(0.45, ctx.hijo("sw"), 250, 2200)), 0, -6)
    t = 0.45
    lz.add(N(golpe_sordo(ctx.hijo("g"), 95, 44, 0.5, 0.4)), t, -1)
    raiz = {1: "D3", 2: "D3", 3: "A2"}[v]
    lz.add(N(metal("hierro", hz(raiz) * 2, 0.9, ctx.hijo("c"), t60=0.9)), t, -7)
    lz.add(N(metal("hierro", hz("D4") * (1.5 if v == 2 else 2), 0.8, ctx.hijo("c2"), t60=0.7)), t + 0.005, -11)
    lz.add(N(crujido_sal(ctx.hijo("s"), 0.25, 0.85, fracturas=1)), t + 0.02, -12)
    return sala(lz.senal(), 0.12, rt60=0.9)


@receta("sfx_combate_turno", S, "Aliada: tic suave de latón con un brillo agudo. Enemiga: toque grave de madera y "
        "plomo", "`TurnStarted` (`Side`)", var(bando=["aliada", "enemiga"], v=[1, 2]), nivel="A",
        objetivo=lambda v: -24 if v["bando"] == "aliada" else -21)
def turno(ctx, v):
    if v["bando"] == "aliada":
        lz = Lienzo(0.35)
        lz.add(N(tic_reloj(ctx.hijo("t"), 1.15 if v["v"] == 1 else 1.3)), 0, -4)
        lz.add(N(metal("plata", grado(4 if v["v"] == 1 else 7, 5), 0.3, ctx.hijo("p"), t60=0.35, golpe=0.2)), 0.004, -6)
        return lz.senal()
    lz = Lienzo(0.4)
    lz.add(N(madera(hz("D3") * (1 if v["v"] == 1 else 1.12), ctx.hijo("m"), 0.3, 0.12)), 0, -2)
    lz.add(N(metal("plomo", hz("D3"), 0.35, ctx.hijo("p"), t60=0.3)), 0, -4)
    lz.add(N(golpe_sordo(ctx.hijo("g"), 80, 45, 0.25, 0.1)), 0, -8)
    return lz.senal()


def _swing(ctx, dur, f0, f1, q=1.4, pico=0.55):
    return N(whoosh(dur, ctx, f0, f1, q=q, pico=pico, forma=1.6))


@receta("sfx_combate_ataque", S, "Barrido del arma antes del golpe; cada familia con su materia: madera, papel y agujas, "
        "cadena, latón, escoba, bronce, tijera, líquido", "`anim:arma_barrido`",
        var(arma=FAMILIAS + ["natural"]), nivel="A", objetivo=-21,
        notas="`natural` cubre zarpazos, mordidas y patadas de las brujas sin arma del prototipo de la Fase 1.")
def ataque(ctx, v):
    a = v["arma"]
    lz = Lienzo(0.6)
    if a == "baculo":
        lz.add(_swing(ctx, 0.35, 250, 900, 1.2), 0)
        lz.add(N(crujido_madera(0.15, ctx.hijo("c"), (40, 90))), 0.05, -16)
    elif a == "grimorio_agujas":
        lz.add(fuente("kenney_rpg_audio/book_flip_3.ogg", velocidad=1.2, hp_hz=800), 0, -6)
        for k in range(5):
            lz.add(_swing(ctx.hijo(f"a{k}"), 0.08, 4000, 9000, 2.5, 0.5), 0.05 + 0.03 * k, -10)
    elif a == "guadana_sal":
        lz.add(_swing(ctx, 0.48, 350, 2600, 1.1, 0.6), 0)
        lz.add(N(crujido_sal(ctx.hijo("s"), 0.3, 0.8, granos=0.6, fracturas=0)), 0.18, -18)
    elif a == "incensario":
        lz.add(_swing(ctx, 0.5, 200, 700, 1.3, 0.7), 0)
        lz.add(N(cadena_hierro(0.35, ctx.hijo("c"), 8, 3200)), 0.05, -12)
        lz.add(N(hp(blanco(0.4, ctx.hijo("h")), 3000)) * env_campana(0.4, 0.5), 0.1, -24)
    elif a == "astrolabio":
        lz.add(_swing(ctx, 0.3, 500, 1800, 1.5), 0)
        lz.add(N(engranes(0.25, ctx.hijo("e"), 30, 3800, False)), 0.0, -12)
    elif a == "rifle_alquimico":
        lz.add(fuente("kenney_rpg_audio/metal_latch.ogg", velocidad=0.9, hp_hz=300), 0, -4)
        lz.add(N(bp(blanco(0.25, ctx.hijo("p")), 2000, 8000)) * env_campana(0.25, 0.6), 0.12, -14)
    elif a == "escoba":
        cerdas = bp(blanco(0.4, ctx.hijo("c")), 2500, 10000) * am_aleatoria(0.4, ctx.hijo("am"), 30, 90, 0.9)
        lz.add(_swing(ctx, 0.4, 300, 1500, 1.0), 0, -2)
        lz.add(N(cerdas) * env_campana(0.4, 0.55), 0, -9)
    elif a == "campana_bronce":
        lz.add(_swing(ctx, 0.45, 150, 500, 1.1, 0.65), 0)
        hum = metal("oro", hz("D2") * 2, 0.5, ctx.hijo("b"), t60=1.5, brillo=0.6, golpe=0.0)
        lz.add(N(hum) * env_campana(0.5, 0.6), 0.0, -18)
    elif a == "tijeras_hilandera":
        lz.add(fuente("kenney_rpg_audio/draw_knife_3.ogg", velocidad=1.3, hp_hz=1500), 0, -6)
        lz.add(_swing(ctx, 0.25, 900, 3000, 1.6), 0.02, -6)
    elif a == "caldero_mano":
        lz.add(_swing(ctx, 0.35, 250, 900, 1.2), 0, -3)
        lz.add(N(burbujeo(0.3, ctx.hijo("b"), 90, 400, 2000)), 0.05, -10)
    elif a == "varita":
        lz.add(_swing(ctx, 0.2, 1500, 6000, 1.8, 0.6), 0)
        lz.add(N(destello(ctx.hijo("d"), 0.25, 5000, 3)), 0.1, -10)
    elif a == "daga_ritual":
        lz.add(tramo(fuente("kenney_rpg_audio/knife_slice.ogg", velocidad=1.25, hp_hz=1200), 0.3), 0, -5)
        lz.add(_swing(ctx, 0.18, 1200, 4000, 1.8), 0, -6)
    else:  # natural: zarpazo o patada
        lz.add(_swing(ctx, 0.22, 500, 2200, 1.5, 0.5), 0)
        lz.add(tramo(fuente("kenney_rpg_audio/cloth_2.ogg", velocidad=1.3, hp_hz=600), 0.25), 0.0, -10)
    return lz.senal()


def _golpe_peso(ctx, peso, v):
    """Capa base del impacto según el peso: cuerpo grave, golpe de foley y chasquido."""
    archivos = {
        "ligero": ["impact_punch_medium_000", "impact_punch_medium_003", "impact_soft_medium_002"],
        "medio": ["impact_punch_medium_003", "impact_punch_heavy_000", "impact_punch_medium_000"],
        "pesado": ["impact_punch_heavy_000", "impact_punch_heavy_002", "impact_soft_heavy_001"],
    }[peso]
    f = archivos[(v - 1) % 3]
    vel = {"ligero": 1.25, "medio": 1.0, "pesado": 0.85}[peso] * ctx.u(0.95, 1.05)
    dur = {"ligero": 0.28, "medio": 0.36, "pesado": 0.5}[peso]
    lz = Lienzo(dur)
    fol = fuente(f"kenney_impact_sounds/{f}.ogg", velocidad=vel, hp_hz=70)
    lz.add(_e(tramo(fol, dur), dur * 0.7), 0, 0)
    f0, f1 = {"ligero": (170, 80), "medio": (130, 55), "pesado": (105, 42)}[peso]
    lz.add(N(golpe_sordo(ctx.hijo("g"), f0, f1, dur, click=0.0)), 0, {"ligero": -8, "medio": -5, "pesado": -2}[peso])
    c = chasquido(ctx.hijo("c"), 0.003, 900, 9000)
    lz.add(N(c), 0, {"ligero": -8, "medio": -6, "pesado": -5}[peso])
    if peso == "pesado":
        lz.add(N(lp(blanco(0.35, ctx.hijo("d")), 900) * env_exp(0.35, 0.3, 0.01)), 0.01, -12)
    return lz.senal()


def _capa_material(ctx, m):
    lz = Lienzo(0.4)
    if m == "pelaje":
        lz.add(fuente("kenney_impact_sounds/impact_soft_medium_000.ogg", hp_hz=60), 0, -2)
        lz.add(N(pelaje(0.2, ctx.hijo("p"))) * env_exp(0.2, 0.15), 0, -10)
    elif m == "sal":
        lz.add(N(crujido_sal(ctx.hijo("s"), 0.18, 0.8, fracturas=2)), 0, 0)
        lz.add(fuente("kenney_impact_sounds/impact_glass_light_002.ogg", velocidad=0.9, hp_hz=600), 0, -8)
    elif m == "metal_liquido":
        ch = chapoteo(ctx.hijo("c"), 0.35, grave=0.7)
        lz.add(N(ch), 0, -2)
        t = tiempo(0.35)
        g = 1 + 0.25 * (1 - np.exp(-t / 0.08))
        from sintesis import modal
        lz.add(N(modal(0.35, [620, 1710, 3190], [1, 0.5, 0.3], [0.3, 0.2, 0.12], ctx.hijo("m"), glide=g)), 0, -6)
    elif m == "madera":
        lz.add(fuente("kenney_impact_sounds/impact_wood_medium_001.ogg", hp_hz=60), 0, -2)
        lz.add(N(madera(ctx.u(380, 520), ctx.hijo("w"), 0.3, 0.1)), 0, -6)
    elif m == "astral":
        lz.add(N(destello(ctx.hijo("d"), 0.35, 3500, 5, 0)), 0, -2)
        lz.add(N(whoosh(0.3, ctx.hijo("w"), 3000, 900, q=1.2, pico=0.1)), 0, -8)
    else:  # piedra
        lz.add(tramo(fuente("kenney_impact_sounds/impact_mining_00%d.ogg" % ctx.elegir([0, 2]), hp_hz=80), 0.4), 0, -2)
        lz.add(N(piedra(ctx.hijo("p"), 0.3)), 0, -6)
    return lz.senal()


def _capa_arma(ctx, a):
    lz = Lienzo(0.5)
    if a == "baculo":
        lz.add(fuente("kenney_impact_sounds/impact_wood_light_000.ogg", hp_hz=100), 0)
        lz.add(N(madera(310, ctx, 0.25, 0.09)), 0, -6)
    elif a == "grimorio_agujas":
        for k in range(7):
            f = ctx.u(4500, 8000)
            from sintesis import modal
            lz.add(N(modal(0.06, [f, f * 1.6], [1, 0.4], [0.03, 0.02], ctx.hijo(f"a{k}"))), 0.018 * k, ctx.u(-10, -3))
    elif a == "guadana_sal":
        lz.add(tramo(fuente("kenney_rpg_audio/knife_slice_2.ogg", velocidad=0.85, hp_hz=600), 0.35), 0, -2)
        lz.add(N(crujido_sal(ctx.hijo("s"), 0.25, 0.8, fracturas=1)), 0.01, -6)
    elif a == "incensario":
        lz.add(N(metal("oro", hz("A4"), 0.45, ctx, t60=0.5, brillo=0.7)), 0, -2)
        lz.add(N(cadena_hierro(0.3, ctx.hijo("c"), 7)), 0.02, -9)
    elif a == "astrolabio":
        lz.add(N(metal("oro", hz("E5"), 0.45, ctx, t60=0.6)), 0, -2)
        lz.add(N(metal("oro", hz("E5") * 1.5, 0.4, ctx.hijo("b"), t60=0.4)), 0.01, -8)
    elif a == "rifle_alquimico":
        lz.add(N(golpe_sordo(ctx.hijo("g"), 160, 50, 0.4, 0.8)), 0, -1)
        lz.add(N(hp(blanco(0.35, ctx.hijo("f")), 2500) * am_aleatoria(0.35, ctx.hijo("am"), 20, 80, 0.8)
                 * env_exp(0.35, 0.3)), 0.02, -10)
        lz.add(N(crujido_sal(ctx.hijo("s"), 0.2, 0.8, fracturas=1)), 0.01, -10)
    elif a == "escoba":
        lz.add(N(bp(blanco(0.2, ctx), 2000, 9000) * env_exp(0.2, 0.15)), 0, -3)
        lz.add(fuente("kenney_impact_sounds/impact_wood_light_002.ogg", hp_hz=150), 0, -6)
    elif a == "campana_bronce":
        # registro grave y onda diferida: nunca se confunde con la campana del Spark
        lz.add(N(metal("oro", hz("D3"), 0.5, ctx, t60=1.2, brillo=0.6)), 0, -2)
        lz.add(N(lp(metal("oro", hz("D3"), 0.4, ctx.hijo("o"), t60=0.8, brillo=0.5), 1500)), 0.12, -6)
    elif a == "tijeras_hilandera":
        lz.add(tramo(fuente("kenney_rpg_audio/metal_click.ogg", velocidad=1.3, hp_hz=1500), 0.2), 0, -2)
        lz.add(tramo(fuente("kenney_rpg_audio/metal_click.ogg", velocidad=1.45, hp_hz=1500), 0.2), 0.07, -4)
    elif a == "caldero_mano":
        lz.add(N(chapoteo(ctx, 0.4, 0.8, 1.2)), 0, -2)
    elif a == "varita":
        lz.add(N(chasquido(ctx, 0.002, 2000, 14000)), 0, -2)
        lz.add(N(metal("plata", hz("A6"), 0.3, ctx.hijo("p"), t60=0.3)), 0, -6)
    else:  # daga_ritual
        lz.add(tramo(fuente("kenney_rpg_audio/knife_slice.ogg", velocidad=1.4, hp_hz=1500), 0.22), 0, -2)
        lz.add(N(metal("hierro", hz("A6"), 0.25, ctx, t60=0.2)), 0, -10)
    return lz.senal()


@receta("sfx_combate_golpe", S, "Impacto físico por peso (cuerpo grave, golpe de foley y chasquido). Capas aparte: "
        "material del objetivo y contacto del arma", "`HitLanded` (origen arma o eslabón físico)",
        var(peso=["ligero", "medio", "pesado"], v=[1, 2, 3]) + var(material=MATERIALES) + var(arma=FAMILIAS),
        nivel="A", objetivo=lambda v: PESO_OBJ.get(v.get("peso"), -20.0),
        notas="El peso sale del daño relativo a la vida máxima del objetivo (3.1). Material y arma son capas que "
              "suenan encima, a −20 LUFS.")
def golpe(ctx, v):
    if "peso" in v:
        return _golpe_peso(ctx, v["peso"], v["v"])
    if "material" in v:
        return _capa_material(ctx, v["material"])
    return _capa_arma(ctx, v["arma"])


@receta("sfx_combate_critico", S, "Capa del crítico: filo de acero que brilla, grieta de cristal y un destello que sube",
        "`HitLanded` con `IsCrit`", var(v=[1, 2, 3]), nivel="A", objetivo=-16)
def critico(ctx, v):
    lz = Lienzo(0.55)
    lz.add(tramo(fuente("kenney_rpg_audio/knife_slice_2.ogg", velocidad=[1.4, 1.55, 1.3][v - 1], hp_hz=2000), 0.3), 0, -5)
    lz.add(N(metal("hierro", grado(4 + v, 5), 0.5, ctx, t60=0.45, brillo=1.3)), 0, -6)
    lz.add(fuente("kenney_impact_sounds/impact_glass_light_00%d.ogg" % (v - 1), hp_hz=900), 0, -9)
    lz.add(N(destello(ctx.hijo("d"), 0.4, 5000, 4, 0)), 0.03, -12)
    return lz.senal()


def _cuerpo_especie(ctx, e):
    lz = Lienzo(0.45)
    tela = tramo(fuente("kenney_rpg_audio/cloth_%d.ogg" % ctx.i(1, 4), velocidad=ctx.u(1.1, 1.4), hp_hz=300), 0.3)
    if e in ("canida", "felina", "pequena"):
        vel = {"canida": 1.0, "felina": 1.1, "pequena": 1.35}[e]
        lz.add(fuente("kenney_impact_sounds/impact_soft_medium_00%d.ogg" % ctx.elegir([0, 2]), velocidad=vel,
                      hp_hz=80), 0, -2)
        lz.add(N(pelaje(0.25, ctx.hijo("p"), 1.0 + 0.3 * (e == "pequena"))) * env_exp(0.25, 0.2), 0, -8)
        lz.add(tela, 0.01, -10)
    elif e == "ave":
        lz.add(fuente("kenney_impact_sounds/impact_soft_medium_000.ogg", velocidad=1.2, hp_hz=120), 0, -4)
        lz.add(N(plumas(0.3, ctx.hijo("p"))) * env_exp(0.3, 0.25), 0, -4)
    elif e == "alada":
        lz.add(N(aleteo(ctx.hijo("a"), 2, 0.08, 1.4)), 0, -3)
        lz.add(N(bp(blanco(0.3, ctx.hijo("polvo")), 3000, 10000) * env_exp(0.3, 0.25)), 0.02, -14)
        lz.add(fuente("kenney_impact_sounds/impact_soft_medium_002.ogg", velocidad=1.25, hp_hz=120), 0, -6)
    elif e == "escamosa":
        esc = bp(blanco(0.3, ctx), 1500, 7000) * am_aleatoria(0.3, ctx.hijo("am"), 40, 120, 0.9)
        lz.add(N(esc) * env_exp(0.3, 0.25), 0, -4)
        lz.add(fuente("kenney_impact_sounds/impact_soft_medium_000.ogg", hp_hz=100), 0, -4)
    elif e == "ungulada":
        lz.add(tramo(fuente("kenney_impact_sounds/impact_soft_heavy_001.ogg", velocidad=1.1, hp_hz=70), 0.35), 0, -2)
        lz.add(N(madera(ctx.u(600, 800), ctx.hijo("cuerno"), 0.2, 0.08)), 0.01, -12)
        lz.add(N(pelaje(0.3, ctx.hijo("lana"), 0.7)) * env_exp(0.3, 0.25), 0, -10)
    else:  # robusta
        lz.add(tramo(fuente("kenney_impact_sounds/impact_soft_heavy_001.ogg", velocidad=0.9, hp_hz=50), 0.45), 0, 0)
        lz.add(N(golpe_sordo(ctx.hijo("g"), 90, 40, 0.35, 0)), 0, -6)
        lz.add(N(pelaje(0.35, ctx.hijo("p"), 0.6)) * env_exp(0.35, 0.3), 0, -10)
    return lz.senal()


@receta("sfx_combate_dano_recibido", S, "Reacción del cuerpo de la aliada golpeada: pelaje, plumas, membranas, escamas "
        "o lana, con su ropa", "`HitLanded` (objetivo aliada)",
        var(especie=ESPECIES), nivel="A", objetivo=-20,
        notas="Las brujas del prototipo son cánidas (loba, zorra), alada (polilla) y pequeñas (liebre, zarigüeya, topa).")
def dano_recibido(ctx, v):
    return _cuerpo_especie(ctx, v["especie"])


@receta("sfx_combate_estado_tick", S, "Daño por turno: quemadura (soplo de fuego), sangrado (pulso sordo y óxido, sin "
        "líquido), veneno (burbujeo ácido), electrocución (crujido de estaño)",
        "`DotTicked` (estado: Burn, Bleed, Poison, Shock)",
        var(estado=["quemadura", "sangrado", "veneno", "electrocucion"]), nivel="A", objetivo=-20)
def estado_tick(ctx, v):
    e = v["estado"]
    lz = Lienzo(0.5)
    if e == "quemadura":
        lz.add(N(whoosh(0.3, ctx.hijo("w"), 300, 1400, q=1.0, pico=0.25)), 0, -4)
        lz.add(N(fuego(0.4, ctx.hijo("f"), crepita=1.2)) * env_exp(0.4, 0.35, 0.01), 0, -4)
    elif e == "sangrado":
        lz.add(N(golpe_sordo(ctx.hijo("g"), 75, 45, 0.3, 0)), 0, -2)
        lz.add(N(raspado(0.22, ctx.hijo("r"), 0.8)), 0.03, -10)
        lz.add(N(cadena_hierro(0.2, ctx.hijo("c"), 3, 2200)), 0.05, -14)
    elif e == "veneno":
        lz.add(N(burbujeo(0.4, ctx.hijo("b"), 120, 700, 3200)), 0, -2)
        lz.add(N(bp(blanco(0.4, ctx.hijo("s")), 3500, 9000) * env_exp(0.4, 0.35, 0.02)), 0, -12)
    else:
        lz.add(N(grito_estano(0.25, ctx.hijo("g"), 1200)), 0, -2)
        lz.add(N(estatica(0.3, ctx.hijo("e"))) * env_exp(0.3, 0.25), 0, -8)
    return lz.senal()


@receta("sfx_combate_escudo", S, "Poner: cúpula de plata que se forma (cristal que sube y se asienta). Romper: cristal "
        "que estalla y una nota de plata que cae", "`ShieldGained` (poner); `HitLanded` que agota el escudo (romper)",
        var(accion=["poner", "romper"], v=[1]), nivel="A", objetivo=-18)
def escudo(ctx, v):
    if v["accion"] == "poner":
        lz = Lienzo(0.9)
        lz.add(N(swell(0.3, ctx.hijo("s"), 1200, 5000, 1.4)), 0, -10)
        base = grado(0 if v["v"] == 1 else 4, 5)
        lz.add(N(copa_frotada(base, 0.7, ctx.hijo("c"))), 0.1, -6)
        lz.add(N(metal("plata", base * 2, 0.6, ctx.hijo("p"), t60=0.8, batido=1.0)), 0.28, -4)
        return sala(lz.senal(), 0.15, 0.9)
    lz = Lienzo(0.8)
    lz.add(fuente("kenney_impact_sounds/impact_glass_heavy_00%d.ogg" % (1 if v["v"] == 1 else 4), hp_hz=200), 0, -1)
    lz.add(N(crujido_sal(ctx.hijo("s"), 0.3, 0.9, fracturas=2)), 0, -8)
    t = tiempo(0.6)
    f = grado(4, 6) * (1 - 0.25 * t / 0.6)
    caida = np.sin(2 * np.pi * np.cumsum(f) / 48000) * env_exp(0.6, 0.5, 0.005)
    lz.add(N(caida), 0.03, -12)
    return lz.senal()


@receta("sfx_combate_caida", S, "Una unidad cae: cuerpo contra el suelo, tela, polvo y un poco de sal que se desmorona",
        "`UnitDefeated`", var(v=[1, 2]), nivel="A", objetivo=-17)
def caida(ctx, v):
    lz = Lienzo(1.0)
    lz.add(fuente("kenney_rpg_audio/cloth_1.ogg", velocidad=0.9, hp_hz=200), 0, -10)
    t = 0.12 + 0.03 * v
    lz.add(fuente("kenney_impact_sounds/impact_soft_heavy_001.ogg", velocidad=[0.9, 1.0, 0.8][v - 1], hp_hz=40), t, 0)
    lz.add(N(golpe_sordo(ctx.hijo("g"), 90, 38, 0.5, 0.2)), t, -5)
    lz.add(N(lp(blanco(0.5, ctx.hijo("p")), 2500) * env_exp(0.5, 0.45, 0.01)), t + 0.01, -16)
    lz.add(N(crujido_sal(ctx.hijo("s"), 0.4, 0.75, granos=0.5, fracturas=0)), t + 0.05, -16)
    return lz.senal()


@receta("sfx_combate_revivir", S, "Revivir: cuenco de cobre que sube, remolino de agua y una respiración de aire "
        "(sin voz)", "`Healed` desde 0 o efecto revivir de `ActionUsed`", var(v=[1]), nivel="A", objetivo=-17)
def revivir(ctx, v):
    lz = Lienzo(1.6)
    lz.add(N(swell(0.7, ctx.hijo("s"), 300, 1800, 0.9)), 0, -8)
    lz.add(N(burbujeo(0.9, ctx.hijo("b"), 50, 500, 2200, tasa_env=lambda t: min(t / 0.6, 1.0))), 0, -14)
    base = grado(2, 4)
    lz.add(N(cuenco(base, 1.2, ctx.hijo("c"), frotado=True)), 0.15, -8)
    for k, g in enumerate([2, 4, 7 if v == 1 else 9]):
        lz.add(N(cuenco(grado(g, 5), 0.9, ctx.hijo(f"n{k}"), t60=1.4)), 0.55 + 0.09 * k, -6 - k)
    return sala(lz.senal(), 0.18, 1.1)


@receta("sfx_combate_jefe_aviso", S, "El jefe carga su ataque definitivo: raíces que se tensan, acero frotado que sube "
        "y un pedal grave de plomo. Crece, nunca asusta de golpe", "`EnemyCharging`", var(v=[1]), nivel="A",
        objetivo=-15)
def jefe_aviso(ctx, v):
    d = 1.8
    lz = Lienzo(d)
    lz.add(N(dron(d, ctx.hijo("d"), 36.7, (2, 3, 4, 6), 0.4)) * env_puntos(d, [(0, 0), (1.2, 1), (d, 0)]), 0, -6)
    lz.add(N(crujido_madera(1.5, ctx.hijo("r"), (15, 70), (300, 650, 1300))) * env_puntos(1.5, [(0, 0), (1.3, 1), (1.5, 0)]),
           0.1, -6)
    t = tiempo(1.5)
    f = hz("A3") * 2 ** ((t / 1.5) * (1 if v == 1 else 0.5))
    tension = sum(a * np.sin(2 * np.pi * np.cumsum(f * r) / 48000) for r, a in [(1, 1), (2.76, 0.4), (5.4, 0.2)])
    tension = tension * env_puntos(1.5, [(0, 0), (1.35, 1), (1.5, 0)]) + 0.1 * bp(blanco(1.5, ctx.hijo("a")), 2000, 6000) * env_puntos(1.5, [(0, 0), (1.3, 1), (1.5, 0)])
    lz.add(N(tension), 0.2, -10)
    return lz.senal()


@receta("sfx_combate_jefe_fase", S, "Cambio de fase del jefe: la corteza se abre en dos, un golpe grave y la savia negra "
        "que late", "`PhaseChanged`", var(v=[1]), nivel="A", objetivo=-15, nuevo=True,
        notas="ID nuevo: el catálogo no tiene sonido para `PhaseChanged`, que el prototipo sí emite (el Guardián).")
def jefe_fase(ctx, v):
    lz = Lienzo(2.2)
    lz.add(fuente("kenney_rpg_audio/creak_2.ogg", velocidad=0.6, hp_hz=80), 0, -6)
    lz.add(N(crujido_madera(0.6, ctx.hijo("c"), (30, 160), (250, 520, 1100))), 0.2, -4)
    lz.add(fuente("kenney_impact_sounds/impact_plank_medium_000.ogg", velocidad=0.6, hp_hz=40), 0.75, 0)
    lz.add(N(golpe_sordo(ctx.hijo("g"), 85, 36, 0.7, 0.3)), 0.75, -3)
    for k in range(3):
        lz.add(N(golpe_sordo(ctx.hijo(f"l{k}"), 60, 40, 0.3, 0)), 1.1 + 0.42 * k, -10 - 3 * k)
        lz.add(N(burbujeo(0.2, ctx.hijo(f"s{k}"), 60, 120, 400, xi=0.05)), 1.12 + 0.42 * k, -18 - 3 * k)
    return lz.senal()


@receta("sfx_combate_granos", S, "Sube: un grano de sal cae en el frasco y brilla hacia arriba. Baja: el grano se "
        "deshace y la nota cae", "`GrainsChanged` (o `ui:granos_cambian`)", var(cambio=["sube", "baja"], v=[1, 2]),
        nivel="A", objetivo=-24)
def granos(ctx, v):
    lz = Lienzo(0.35)
    if v["cambio"] == "sube":
        lz.add(tramo(fuente("kenney_impact_sounds/impact_glass_light_00%d.ogg" % (v["v"] + 1), velocidad=1.6, hp_hz=1500), 0.12), 0, -6)
        lz.add(N(metal("plata", grado(4, 5), 0.25, ctx.hijo("a"), t60=0.3, golpe=0.2)), 0.0, -6)
        lz.add(N(metal("plata", grado(7, 5), 0.25, ctx.hijo("b"), t60=0.3, golpe=0.2)), 0.06, -4)
    else:
        lz.add(N(crujido_sal(ctx.hijo("s"), 0.12, 0.8, granos=0.5, fracturas=1)), 0, -6)
        lz.add(N(metal("plata", grado(4, 5), 0.25, ctx.hijo("a"), t60=0.3, golpe=0.2)), 0.0, -6)
        lz.add(N(metal("plata", grado(2, 5), 0.25, ctx.hijo("b"), t60=0.3, golpe=0.2)), 0.06, -6)
    return lz.senal()


@receta("sfx_combate_velocidad", S, "Clic de astrolabio: ×1 un tic grave, ×2 dos tics agudos", "`ui:velocidad`",
        var(velocidad=[1, 2]), nivel="A", objetivo=-25)
def velocidad(ctx, v):
    lz = Lienzo(0.25)
    if v["velocidad"] == 1:
        lz.add(N(tic_reloj(ctx, 0.85)), 0)
    else:
        lz.add(N(tic_reloj(ctx, 1.2)), 0)
        lz.add(N(tic_reloj(ctx.hijo("b"), 1.35)), 0.06, -2)
    return lz.senal()


@receta("sfx_combate_auto", S, "Activar: mecanismo de relojería que arranca y engancha. Quitar: clic y engranes que se "
        "frenan", "`ui:auto`", var(estado=["activar", "quitar"]), nivel="A", objetivo=-25)
def auto(ctx, v):
    lz = Lienzo(0.5)
    if v["estado"] == "activar":
        e = N(engranes(0.28, ctx, 22, 3000))
        lz.add(e * env_de(e, [(0, 0.3), (0.28, 1), (0.36, 1)]), 0, -4)
        lz.add(tramo(fuente("kenney_rpg_audio/metal_latch.ogg", velocidad=1.3, hp_hz=600), 0.15), 0.28, -4)
    else:
        lz.add(tramo(fuente("kenney_rpg_audio/metal_click.ogg", velocidad=1.2, hp_hz=800), 0.15), 0, -4)
        e = engranes(0.3, ctx, 12, 2400)
        lz.add(N(e) * env_de(e, [(0, 1), (0.3, 0)]), 0.05, -6)
    return lz.senal()


@receta("sfx_combate_victoria", S, "Victoria: campanas de oro en arpegio de Re mayor y un brillo de sal (suena con el "
        "jingle de música)", "`BattleEnded` (Victory)", var(v=[1]), nivel="A", objetivo=-15)
def victoria(ctx, v):
    lz = Lienzo(1.8)
    notas = ["D5", "F#5", "A5", "D6"] if v == 1 else ["A4", "D5", "F#5", "A5", "D6"]
    for k, n in enumerate(notas):
        lz.add(N(metal("oro", hz(n), 1.4, ctx.hijo(f"n{k}"), t60=1.4)), 0.07 * k, -3 - 0.5 * k)
    lz.add(N(destello(ctx.hijo("d"), 0.8, 4500, 7)), 0.3, -10)
    lz.add(N(golpe_sordo(ctx.hijo("g"), 90, 45, 0.4, 0.2)), 0, -10)
    return sala(lz.senal(), 0.18, 1.2)


@receta("sfx_combate_derrota", S, "Derrota: campana grave de plomo y la sal que se deshace hacia abajo, en Re menor",
        "`BattleEnded` (Defeat)", var(v=[1]), nivel="A", objetivo=-16)
def derrota(ctx, v):
    lz = Lienzo(2.4)
    lz.add(N(metal("plomo", hz("D3"), 1.2, ctx.hijo("p"), t60=1.2)), 0, -2)
    lz.add(N(metal("oro", hz("D4"), 2.0, ctx.hijo("o"), t60=2.0, brillo=0.6)), 0.0, -9)
    lz.add(N(metal("oro", hz("F4"), 1.8, ctx.hijo("o2"), t60=1.8, brillo=0.6)), 0.0, -14)
    sal = crujido_sal(ctx.hijo("s"), 1.2, 0.8, granos=0.5, fracturas=1, grave=1.0)
    lz.add(N(sal) * env_de(sal, [(0, 1), (len(sal) / 48000, 0)]), 0.25, -12)
    return sala(lz.senal(), 0.2, 1.4)
