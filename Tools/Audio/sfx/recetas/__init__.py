# -*- coding: utf-8 -*-
"""Registro de recetas.

Cada receta es una función que recibe (ctx, v) y devuelve una señal. `v` es el
diccionario de la variante (por ejemplo {"peso": "pesado", "v": 2}); si la única
variación es aleatoria (v1, v2, v3), `v` es solo el número. El
decorador `@receta` guarda los datos del catálogo junto a la función:
sistema, descripción, disparador, variantes, sonoridad objetivo y estado.

Los módulos de cada sistema se importan al final para llenar RECETAS.
"""
import itertools
import zlib
from dataclasses import dataclass, field

RECETAS = {}

# Sonidos de chasquido o granulares (factor de cresta de más de 20 dB): el limitador puede trabajar más para que
# lleguen a su sonoridad sin que el pico real pase de −1 dBTP.
LIMITE_GRANULAR = 9.0
GRANULARES = {"sfx_sal_crujido", "sfx_combate_estado_tick", "sfx_quiebre_jupiter", "sfx_ui_tocar", "sfx_combate_golpe",
              "sfx_taller_yunque", "sfx_cadena_tomar", "sfx_combate_velocidad", "sfx_ui_cancelar", "sfx_ui_pestana",
              "sfx_fresno_nodo_elegir", "sfx_ascension_impacto", "sfx_combate_ataque", "sfx_tirada_tomar"}

# Tope de duración por ID (segundos), pensado desde la estructura de cada sonido: el último golpe más su
# cola. Al pasar el tope, la cola se funde en exponencial. Es la perilla para caber en los 30 MB de WAV; la
# reverb de cada lugar la agrega FMOD después. Los IDs que no están aquí usan TOPE_POR_NIVEL.
TOPE_POR_NIVEL = {"A": 0.9, "B": 0.8}
PRESUPUESTO = {
    # combate
    "sfx_combate_entrada": 1.0, "sfx_combate_escudo": 0.75, "sfx_combate_caida": 0.8, "sfx_combate_revivir": 1.3,
    "sfx_combate_jefe_aviso": 1.6, "sfx_combate_jefe_fase": 2.2, "sfx_combate_victoria": 1.2,
    "sfx_combate_derrota": 1.7, "sfx_combate_golpe": 0.42, "sfx_combate_ataque": 0.32,
    # cadena
    "sfx_cadena_resonancia_llena": 1.0, "sfx_cadena_abrir": 1.0, "sfx_cadena_fin": 1.2, "sfx_cadena_spark": 0.8,
    "sfx_cadena_invitada": 0.9, "sfx_cadena_calculo": 1.0, "sfx_partitura_nota": 0.32,
    # magia
    "sfx_sal_crujido": 0.5, "sfx_magia_canalizar": 0.62, "sfx_magia_ascendente_aparece": 0.5,
    "sfx_magia_golpe_astral": 0.45, "sfx_magia_autohechizo": 0.85, "sfx_magia_vislumbre": 1.9,
    "sfx_magia_sal_negra": 1.0, "sfx_magia_ascendente_retira": 0.6, "sfx_ritual_cupria": 0.7,
    # salinidad
    "sfx_salinidad_baja": 0.8, "sfx_salinidad_sobresalada": 0.9, "sfx_salinidad_cristalizacion": 1.4,
    "sfx_salinidad_extasis": 2.5, "sfx_salinidad_desalada": 1.3,
    # quiebre
    "sfx_quiebre_firmeza": 0.9, "sfx_quiebre_sol": 0.9, "sfx_quiebre_luna": 0.9, "sfx_quiebre_mercurio": 0.9,
    "sfx_quiebre_venus": 1.1, "sfx_quiebre_jupiter": 0.9, "sfx_quiebre_saturno": 1.3,
    # aspectos y reacciones
    "sfx_reaccion_eclipse": 1.1, "sfx_reaccion_helada_negra": 1.6, "sfx_reaccion_espinas_rojas": 1.3,
    "sfx_reaccion_tempestad": 1.3,
    # ascensiones
    "sfx_ascension_carga": 0.7, "sfx_ascension_lanzar": 1.1, "sfx_ascension_i": 1.0, "sfx_ascension_ii": 1.7,
    "sfx_ascension_iii_marisal": lambda v: 7.5 if v.get("version") == "largo" else 2.8,
    "sfx_ascension_impacto": 1.0, "sfx_ascension_duo": 1.3, "sfx_ascension_saltar": 0.4,
    # pociones
    "sfx_pocion_efecto": 0.65, "sfx_pocion_turbia": 1.0,
    # fresno
    "sfx_fresno_zona_entrar": 1.0, "sfx_fresno_fruto": 0.6, "sfx_fresno_injerto": 0.55, "sfx_fresno_mutacion": 1.0,
    "sfx_fresno_maldicion": 0.6, "sfx_fresno_santuario": 1.0, "sfx_fresno_descanso": 1.0,
    "sfx_fresno_entrenar": 0.9, "sfx_fresno_mancha_aparece": 1.2, "sfx_fresno_mancha_recuperar": 1.0,
    "sfx_fresno_mancha_absorbida": 1.2, "sfx_fresno_ofrenda": 1.2,
    # termas y tirada
    "sfx_termas_tortuga_respira": 1.8, "sfx_termas_bano": 1.1, "sfx_termas_tocar": 0.4,
    "sfx_termas_paralaje_baja": 1.8, "sfx_tirada_tomar": 0.6, "sfx_tirada_sales_caer": 1.2,
    "sfx_tirada_burbujas": 1.0, "sfx_tirada_vapor": 1.05, "sfx_tirada_subida": 1.6, "sfx_tirada_quietud": 1.4,
    "sfx_tirada_constelacion": 1.8, "sfx_tirada_silueta": 1.2, "sfx_tirada_emergencia": 0.95,
    "sfx_tirada_sacudida": 0.8, "sfx_tirada_tarjeta": 0.6, "sfx_tirada_saltar": 0.35, "sfx_tirada_polvo_astral": 0.9,
    # stingers de la ficha P05: duración de la tabla más 0.5 a 1 s de cola
    "mus_tirada_vapor_blanco": 2.6, "mus_tirada_vapor_violeta": 3.6, "mus_tirada_vapor_dorado": 4.7,
    "mus_tirada_subida_dorada": 5.6,
    # interfaz
    "sfx_ui_tocar": 0.12, "sfx_ui_confirmar": 0.45, "sfx_ui_cancelar": 0.35, "sfx_ui_error": 0.3,
    "sfx_ui_pestana": 0.4, "sfx_ui_deslizar": 0.15, "sfx_ui_astrolabio": 0.16, "sfx_ui_notificacion": 0.7,
    "sfx_ui_recompensa": 0.9, "sfx_libro_abrir": 0.7, "sfx_libro_cerrar": 0.5, "sfx_libro_folio": 0.7,
    "sfx_libro_titulo": 1.0, "sfx_libro_falla": 0.65,
    # taller
    "sfx_taller_yunque": 0.7, "sfx_taller_reforja": 1.4, "sfx_taller_fusion": 2.0, "sfx_taller_ingrediente": 0.6,
    "sfx_taller_salmuera_ok": 0.9, "sfx_taller_salmuera_turbia": 0.9, "sfx_taller_altar": 0.75,
    "sfx_taller_receta": 0.9,
    # almitas
    "sfx_almita_soltar": 0.5, "sfx_almita_recoger": 0.9, "sfx_almita_nombre": 1.3, "sfx_almita_spark": 0.45,
    "sfx_almita_ascendente": 1.1,
    # horror
    "sfx_horror_huesped": 1.7, "sfx_horror_salmodico_reves": 1.1, "sfx_horror_cristalizado": 1.3,
    "sfx_horror_cielo": 1.8, "sfx_horror_caida": 2.0, "sfx_paralaje_sube": 1.4, "sfx_paralaje_baja": 1.6,
}

# Orden y nombre de los sistemas (sección 3.5 de INTEGRACION_AUDIO.md).
SISTEMAS = [
    ("combate", "3.5.1 Combate"),
    ("cadena", "3.5.2 Cadena y Partitura"),
    ("magia", "3.5.3 Magia y ritual"),
    ("salinidad", "3.5.4 Salinidad por banda"),
    ("quiebre", "3.5.5 Quiebre por planeta"),
    ("aspectos", "3.5.6 Aspectos y Reacciones Híbridas"),
    ("ascension", "3.5.7 Ascensiones"),
    ("pociones", "3.5.8 Pociones"),
    ("fresno", "3.5.9 Fresno"),
    ("termas", "3.5.10 Termas y tirada del gacha"),
    ("interfaz", "3.5.11 Libro Mayor e interfaz"),
    ("taller", "3.5.12 Taller (crafting)"),
    ("almitas", "3.5.13 Almitas"),
    ("horror", "3.5.14 Horror y Paralaje"),
]


@dataclass
class Receta:
    id: str
    sistema: str
    desc: str
    disparador: str
    fn: object
    variantes: list
    nivel: str = "B"
    objetivo: object = -20.0     # LUFS; número o función(v)
    metrica: str = "M"           # M: máximo momentáneo; S: corto plazo; I: integrada (bucles)
    estereo: bool = False
    bucle: bool = False
    estado: str = "final temporal"
    nuevo: bool = False          # ID que no está en el catálogo del plan
    notas: str = ""
    hp_hz: float = 25.0
    limite_db: float = 6.0       # reducción máxima del limitador antes de bajar la ganancia
    max_dur: object = None       # segundos; número o función(v). Ver PRESUPUESTO
    extra: dict = field(default_factory=dict)

    def liberacion_de(self):
        """Liberación del limitador: muy rápida en texturas de granos (actúa grano por grano)."""
        return 0.006 if self.limite_de() == LIMITE_GRANULAR else 0.03

    def limite_de(self):
        return LIMITE_GRANULAR if (self.id in GRANULARES or self.id.startswith("sfx_ritual_")) else self.limite_db

    def max_dur_de(self, v):
        m = self.max_dur if self.max_dur is not None else PRESUPUESTO.get(self.id)
        if m is None:
            m = TOPE_POR_NIVEL[self.nivel]
        return m(v) if callable(m) else m

    def objetivo_de(self, v):
        return self.objetivo(v) if callable(self.objetivo) else self.objetivo

    def estado_de(self, v):
        return self.estado(v) if callable(self.estado) else self.estado


def var(**ejes):
    """Producto de ejes de variación, en orden: var(peso=[...], v=[1, 2])."""
    claves = list(ejes.keys())
    return [dict(zip(claves, combo)) for combo in itertools.product(*[ejes[k] for k in claves])]


def nombre_archivo(id_, v):
    partes = [id_]
    for k, val in v.items():
        partes.append(f"v{val}" if k == "v" else str(val))
    return "__".join(partes)


def archivos(r):
    """Pares (variante, nombre de archivo) de una receta.

    La variación aleatoria `__vN` solo aparece cuando hay más de un archivo con los mismos valores de parámetro:
    un ID con un solo archivo se llama `<sfx_id>.wav` y uno con un solo archivo por planeta, `<sfx_id>__luna.wav`.
    """
    cuenta = {}
    for v in r.variantes:
        clave = tuple((k, x) for k, x in v.items() if k != "v")
        cuenta[clave] = cuenta.get(clave, 0) + 1
    res = []
    for v in r.variantes:
        clave = tuple((k, x) for k, x in v.items() if k != "v")
        visible = v if cuenta[clave] > 1 else {k: x for k, x in v.items() if k != "v"}
        res.append((v, nombre_archivo(r.id, visible)))
    return res


def semilla(archivo):
    return zlib.crc32(f"salyalmas-sfx|{archivo}".encode("utf-8"))


def receta(id, sistema, desc, disparador, variantes=None, **kw):
    def deco(fn):
        if id in RECETAS:
            raise ValueError(f"ID repetido: {id}")
        RECETAS[id] = Receta(id=id, sistema=sistema, desc=desc, disparador=disparador, fn=fn,
                             variantes=variantes if variantes is not None else [{}], **kw)
        return fn
    return deco


def cargar():
    """Importa todos los módulos de recetas (llena RECETAS)."""
    from . import (combate, cadena, magia, salinidad, quiebre, aspectos, ascension,  # noqa: F401
                   pociones, fresno, termas, interfaz, taller, almitas, horror)
    return RECETAS
