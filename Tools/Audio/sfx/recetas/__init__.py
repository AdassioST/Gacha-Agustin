# -*- coding: utf-8 -*-
"""Registro de recetas.

Cada receta es una función que recibe (ctx, v) y devuelve una señal. `v` es el
diccionario de la variante (por ejemplo {"peso": "pesado", "v": 2}). El
decorador `@receta` guarda los datos del catálogo junto a la función:
sistema, descripción, disparador, variantes, sonoridad objetivo y estado.

Los módulos de cada sistema se importan al final para llenar RECETAS.
"""
import itertools
import zlib
from dataclasses import dataclass, field

RECETAS = {}

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
    extra: dict = field(default_factory=dict)

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
