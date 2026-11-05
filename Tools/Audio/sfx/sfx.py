# -*- coding: utf-8 -*-
"""Generador de efectos de sonido de Sal y Almas.

Uso (desde la raíz del repositorio):

    python -I Tools/Audio/sfx/sfx.py generar                 # todo: WAV, medidas, catálogo, registro y revisión
    python -I Tools/Audio/sfx/sfx.py generar sfx_cadena_spark sfx_combate_golpe
    python -I Tools/Audio/sfx/sfx.py lista                   # IDs, variantes y estado
    python -I Tools/Audio/sfx/sfx.py medir                   # vuelve a medir con ffmpeg y reescribe los documentos
    python -I Tools/Audio/sfx/sfx.py revisar [sistema ...]   # hojas de espectrogramas

Requisitos: Python 3.10+, numpy, scipy, soundfile; ffmpeg para medir; Pillow para las hojas.
"""
import argparse
import csv
import datetime
import glob
import hashlib
import os
import sys
import time
from concurrent.futures import ProcessPoolExecutor, ThreadPoolExecutor

AQUI = os.path.dirname(os.path.abspath(__file__))
if AQUI not in sys.path:
    sys.path.insert(0, AQUI)  # con -I Python no agrega la carpeta del script

import numpy as np  # noqa: E402

import sintesis  # noqa: E402
import proceso  # noqa: E402
from recetas import RECETAS, SISTEMAS, cargar, semilla, archivos  # noqa: E402

RAIZ = os.path.abspath(os.path.join(AQUI, "..", "..", ".."))
SALIDA = os.path.join(RAIZ, "Assets", "SalYAlmas", "Audio", "Resources", "SfxTemporal")
LICENCIAS = os.path.join(RAIZ, "Audio", "Licencias", "sfx")
REVISION = os.path.join(AQUI, "revision")
CATALOGO = os.path.join(AQUI, "catalogo.md")
TOPE_MB = 30.0

LICENCIA_SINTETICA = "Propia del estudio (síntesis original, sin material de terceros)"
LICENCIA_MEZCLA = "Propia del estudio; incluye material CC0 1.0 de Kenney (sin atribución obligatoria)"


def trabajos():
    """Lista de (receta, variante, archivo) en orden estable."""
    cargar()
    out = []
    for r in RECETAS.values():
        for v, a in archivos(r):
            out.append((r.id, v, a))
    return out


def render_uno(args):
    """Genera un archivo. Corre en un proceso aparte; devuelve su informe."""
    id_, v, archivo, salida = args
    cargar()
    r = RECETAS[id_]
    sintesis.USO_FUENTES.clear()
    ctx = sintesis.Ctx(semilla(archivo))
    # Si la única variación es aleatoria (v1, v2...), la receta recibe el número; si no, el diccionario.
    x = r.fn(ctx, v["v"] if list(v.keys()) == ["v"] else v)
    x = np.asarray(x, dtype=float)
    md = r.max_dur_de(v)
    if md and not r.bucle:
        x = proceso.tope_duracion(x, md)
    if r.estereo and x.ndim == 1:
        x = sintesis.estereo_ancho(x, ctx.hijo("est"))
    if not r.estereo and x.ndim == 2:
        x = x.mean(axis=1)
    if r.bucle:
        n = len(x)
        x3 = np.concatenate([x, x, x]) if x.ndim == 1 else np.concatenate([x, x, x], axis=0)
        y, inf = proceso.acabar(x3, r.objetivo_de(v), r.metrica, es_bucle=True, hp_hz=r.hp_hz,
                                limite_db=r.limite_de(), liberacion=r.liberacion_de())
        x = y[n:2 * n]
        inf["duracion"] = len(x) / sintesis.SR
    else:
        x, inf = proceso.acabar(x, r.objetivo_de(v), r.metrica, es_bucle=False, hp_hz=r.hp_hz,
                                limite_db=r.limite_de(), liberacion=r.liberacion_de())
    avisos = proceso.revisar(x, r.bucle)
    ruta = os.path.join(salida, archivo + ".wav")
    proceso.escribir(ruta, x)
    inf.update(archivo=archivo, id=id_, variante=v, fuentes=sorted(sintesis.USO_FUENTES), avisos=avisos,
               canales=1 if x.ndim == 1 else 2)
    return inf


def generar(ids, salida, procesos):
    os.makedirs(salida, exist_ok=True)
    lista = [t for t in trabajos() if not ids or t[0] in ids]
    if ids:
        faltan = set(ids) - {t[0] for t in lista}
        if faltan:
            sys.exit(f"IDs desconocidos: {', '.join(sorted(faltan))}")
    else:  # regeneración completa: borra lo que ya no tiene receta
        validos = {t[2] + ".wav" for t in lista}
        for f in glob.glob(os.path.join(salida, "*.wav")):
            if os.path.basename(f) not in validos:
                os.remove(f)
    t0 = time.time()
    args = [(i, v, a, salida) for i, v, a in lista]
    informes = []
    if procesos <= 1:
        resultados = map(render_uno, args)
        ex = None
    else:
        ex = ProcessPoolExecutor(max_workers=procesos)
        resultados = ex.map(render_uno, args, chunksize=4)
    for inf in resultados:
        informes.append(inf)
        if inf["avisos"]:
            print(f"  aviso {inf['archivo']}: {', '.join(inf['avisos'])}")
    if ex is not None:
        ex.shutdown()
    print(f"{len(informes)} archivos en {time.time() - t0:.1f} s")
    return informes


def medir(salida, procesos):
    """Mide todos los WAV con ffmpeg ebur128 (peak=true)."""
    rutas = sorted(glob.glob(os.path.join(salida, "*.wav")))
    with ThreadPoolExecutor(max_workers=procesos) as ex:
        medidas = list(ex.map(proceso.medir_ffmpeg, rutas))
    res = {}
    import soundfile as sf
    for ruta, m in zip(rutas, medidas):
        info = sf.info(ruta)
        m.update(duracion=info.frames / info.samplerate, canales=info.channels, bytes=os.path.getsize(ruta))
        res[os.path.basename(ruta)[:-4]] = m
    return res


def leer_fuentes_registro():
    """Fuentes usadas por cada archivo según el registro anterior (para regenerar un solo ID)."""
    ruta = os.path.join(LICENCIAS, "registro.csv")
    res = {}
    if os.path.exists(ruta):
        with open(ruta, encoding="utf-8") as f:
            for fila in csv.DictReader(f):
                fu = fila.get("fuentes", "—")
                res[fila["archivo"][:-4]] = [] if fu in ("", "—") else [x.strip() for x in fu.split(";")]
    return res


def escribir_registro(medidas, fuentes_por_archivo):
    os.makedirs(LICENCIAS, exist_ok=True)
    ruta = os.path.join(LICENCIAS, "registro.csv")
    cargar()
    with open(ruta, "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f, lineterminator="\n")
        w.writerow(["archivo", "id", "variante", "metodo", "fuentes", "licencia_resultante", "estado", "nivel",
                    "nuevo_id"])
        for id_, v, archivo in trabajos():
            if archivo not in medidas:
                continue
            r = RECETAS[id_]
            fu = fuentes_por_archivo.get(archivo, [])
            metodo = "mezcla" if fu else "sintético"
            variante = archivo[len(id_) + 2:] or "—"
            w.writerow([archivo + ".wav", id_, variante, metodo, " ; ".join(fu) or "—",
                        LICENCIA_MEZCLA if fu else LICENCIA_SINTETICA, r.estado_de(v), r.nivel,
                        "sí" if r.nuevo else "no"])
    print(f"registro: {ruta}")


def _rango(vals, fmt="{:.1f}"):
    vals = [x for x in vals if x is not None]
    if not vals:
        return "—"
    a, b = min(vals), max(vals)
    return fmt.format(a) if abs(a - b) < 0.05 else f"{fmt.format(a)} a {fmt.format(b)}"


def escribir_catalogo(medidas):
    cargar()
    total_b = sum(m["bytes"] for m in medidas.values())
    n_ids = sum(1 for r in RECETAS.values() if any(a in medidas for _, a in archivos(r)))
    lineas = [
        "# Catálogo de efectos generados",
        "",
        "Lo escribe `sfx.py` después de medir cada WAV con ffmpeg (`ebur128=peak=true`). No lo edites a mano: "
        "cambia la receta y vuelve a correr `python -I Tools/Audio/sfx/sfx.py generar`.",
        "",
        f"- **IDs:** {n_ids} · **archivos:** {len(medidas)} · **tamaño total:** {total_b / 1e6:.1f} MB "
        f"(tope {TOPE_MB:.0f} MB)",
        "- **Formato:** WAV de 48 kHz y 16 bits. Todo es mono salvo donde dice estéreo; FMOD pone el paneo y la "
        "cola estéreo.",
        "- **Sonoridad:** máximo momentáneo (ventana de 400 ms, `M máx`) en LUFS; en los bucles, la integrada (`I`). "
        "El objetivo de cada familia está en el README.",
        "- **Pico real:** el más alto de las variantes, en dBTP. Ninguno pasa de −1 dBTP.",
        "- **Estado:** nivel de prioridad (A: lo que usa ya el prototipo de la Fase 1; B: el resto) y calidad: "
        "*final temporal* sirve para el prototipo; *borrador* funciona pero necesita otra pasada.",
        "",
    ]
    for clave, titulo in SISTEMAS:
        recs = [r for r in RECETAS.values() if r.sistema == clave]
        if not recs:
            continue
        lineas += [f"## {titulo}", "",
                   "| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |",
                   "| --- | --- | --- | --- | --- | --- | --- | --- |"]
        for r in recs:
            arch = [a for _, a in archivos(r)]
            ms = [medidas[a] for a in arch if a in medidas]
            if not ms:
                continue
            nombres = [a[len(r.id) + 2:] for a in arch]
            nombres = [n for n in nombres if n]
            if not nombres:
                var_txt = "1 archivo"
            elif len(nombres) <= 8:
                var_txt = f"{len(nombres)}: " + ", ".join(f"`{n}`" for n in nombres)
            else:
                var_txt = f"{len(nombres)}: `{nombres[0]}` … `{nombres[-1]}`"
            if r.bucle:
                son = "I " + _rango([m["i"] for m in ms])
            else:
                son = "M máx " + _rango([m["m_max"] for m in ms])
            tp = max((m["tp"] for m in ms if m["tp"] is not None), default=None)
            estados = sorted({r.estado_de(v) for v in r.variantes})
            est = f"{r.nivel} · " + " y ".join(estados)
            if r.estereo:
                est += " · estéreo"
            if r.bucle:
                est += " · bucle"
            if r.nuevo:
                est += " · **ID nuevo**"
            idtxt = f"`{r.id}`"
            lineas.append(f"| {idtxt} | {r.desc} | {r.disparador} | {var_txt} | "
                          f"{_rango([m['duracion'] for m in ms], '{:.2f}')} | {son} | "
                          f"{'—' if tp is None else f'{tp:.1f}'} | {est} |")
        lineas.append("")
    with open(CATALOGO, "w", encoding="utf-8") as f:
        f.write("\n".join(lineas))
    print(f"catálogo: {CATALOGO} ({total_b / 1e6:.1f} MB en {len(medidas)} archivos)")
    return total_b


def comprobar(medidas):
    malos = []
    for a, m in sorted(medidas.items()):
        if m["tp"] is not None and m["tp"] > -1.0:
            malos.append(f"{a}: pico real {m['tp']:.2f} dBTP")
    total = sum(m["bytes"] for m in medidas.values()) / 1e6
    if total > TOPE_MB:
        malos.append(f"tamaño total {total:.1f} MB > {TOPE_MB} MB")
    for x in malos:
        print("  ERROR", x)
    return not malos


def escribir_indice(salida):
    """Índice que lee el prototipo de Unity (TempSfxPlayer): Resources no deja listar archivos en tiempo de ejecución."""
    import json
    nombres = sorted(os.path.splitext(os.path.basename(f))[0] for f in glob.glob(os.path.join(salida, "*.wav")))
    ruta = os.path.join(salida, "sfx_indice.json")
    with open(ruta, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(json.dumps({"files": nombres}, ensure_ascii=False, indent=1) + "\n")
    print(f"índice: {ruta} ({len(nombres)} archivos)")


def huella(salida):
    h = hashlib.sha256()
    for f in sorted(glob.glob(os.path.join(salida, "*.wav"))):
        h.update(os.path.basename(f).encode())
        with open(f, "rb") as fh:
            h.update(fh.read())
    return h.hexdigest()[:16]


def main():
    ap = argparse.ArgumentParser(description="Genera los efectos de sonido de Sal y Almas.")
    ap.add_argument("accion", choices=["generar", "lista", "medir", "revisar", "huella"])
    ap.add_argument("ids", nargs="*", help="IDs (generar) o sistemas (revisar); vacío = todos")
    ap.add_argument("--salida", default=SALIDA)
    ap.add_argument("--procesos", type=int, default=os.cpu_count() or 2)
    ap.add_argument("--sin-revision", action="store_true", help="no dibuja las hojas de espectrogramas")
    a = ap.parse_args()

    if a.accion == "lista":
        cargar()
        for clave, titulo in SISTEMAS:
            print(f"\n{titulo}")
            for r in RECETAS.values():
                if r.sistema == clave:
                    print(f"  [{r.nivel}] {r.id}  ({len(r.variantes)})  {r.estado if isinstance(r.estado, str) else '…'}")
        print(f"\n{len(RECETAS)} IDs, {len(trabajos())} archivos")
        return
    if a.accion == "huella":
        print(huella(a.salida))
        return
    if a.accion == "revisar":
        import revision
        revision.hojas(a.salida, REVISION, a.ids)
        return

    previas = leer_fuentes_registro()
    if a.accion == "generar":
        informes = generar(a.ids, a.salida, a.procesos)
        for inf in informes:
            previas[inf["archivo"]] = inf["fuentes"]
    medidas = medir(a.salida, a.procesos)
    escribir_registro(medidas, previas)
    escribir_catalogo(medidas)
    escribir_indice(a.salida)
    ok = comprobar(medidas)
    if a.accion == "generar" and not a.sin_revision:
        import revision
        revision.hojas(a.salida, REVISION, [], medidas)
    print("huella:", huella(a.salida), datetime.date.today().isoformat())
    if not ok:
        sys.exit(1)


if __name__ == "__main__":
    main()
