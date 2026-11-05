"""Evidencia reproducible de la auditoria: capturas reales, contraste y hashes.

Uso: python scripts/auditoria_ui.py. Requiere Pillow. No modifica capturas.
Los contrastes son de colores declarados, no una certificacion del render.
"""
from pathlib import Path
import hashlib
import json
import xml.etree.ElementTree as ET
from PIL import Image, ImageDraw, ImageFont, ImageOps, ImageStat

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs/auditoria-ui"
BEFORE = OUT / "antes-1920"
AFTER = OUT / "entrega-1920"
FONT_PATH = Path("C:/Windows/Fonts/segoeui.ttf")


def font(size):
    return ImageFont.truetype(str(FONT_PATH), size) if FONT_PATH.exists() else ImageFont.load_default()


def luminance(color):
    rgb = [int(color[i:i + 2], 16) / 255 for i in (1, 3, 5)]
    linear = [v / 12.92 if v <= .04045 else ((v + .055) / 1.055) ** 2.4 for v in rgb]
    return sum(v * w for v, w in zip(linear, (.2126, .7152, .0722)))


def contrast(a, b):
    light, dark = sorted((luminance(a), luminance(b)), reverse=True)
    return round((light + .05) / (dark + .05), 2)


def comparison(name, filename, gray=False):
    # Solo escala para la hoja comparativa; los PNG originales siguen intactos.
    canvas = Image.new("RGB", (1920, 614), "#0B0B14")
    draw = ImageDraw.Draw(canvas)
    for index, (folder, label) in enumerate(((BEFORE, "ANTES"), (AFTER, "DESPUES"))):
        im = Image.open(folder / filename).convert("RGB")
        if gray:
            im = ImageOps.grayscale(im).convert("RGB")
        im = im.resize((960, 540), Image.Resampling.LANCZOS)
        canvas.paste(im, (index * 960, 58))
        draw.text((index * 960 + 24, 12), label, font=font(28), fill="#EDE3CC")
    canvas.save(OUT / name)


def details():
    # Recortes a escala 1:1: el texto conserva su altura en pixeles de origen.
    canvas = Image.new("RGB", (1200, 600), "#0B0B14")
    draw = ImageDraw.Draw(canvas)
    boxes = [(BEFORE, (16, 872, 396, 1068), (20, 62), "ANTES: aliada"),
             (AFTER, (16, 840, 396, 1068), (420, 62), "DESPUES: aliada"),
             (BEFORE, (1068, 384, 1292, 500), (20, 378), "ANTES: enemigo"),
             (AFTER, (1032, 316, 1296, 480), (420, 378), "DESPUES: enemigo")]
    for folder, box, pos, label in boxes:
        im = Image.open(folder / "combate_1_inicio.png").convert("RGB")
        canvas.paste(im.crop(box), pos)
        draw.text((pos[0], pos[1] - 38), label, font=font(23), fill="#EDE3CC")
    draw.text((830, 72), "PNG a escala 1:1\n\nVida fuera de la barra\n\nBando + forma + numero\n\nOro para el objetivo\n\nTextura de baja intensidad", font=font(23), fill="#EDE3CC", spacing=10)
    canvas.save(OUT / "detalle-combate.png")


def contact_sheet():
    files = sorted(AFTER.glob("*.png"))
    canvas = Image.new("RGB", (1280, ((len(files) + 3) // 4) * 206), "#0B0B14")
    draw = ImageDraw.Draw(canvas)
    for i, path in enumerate(files):
        im = Image.open(path).convert("RGB")
        im.thumbnail((316, 178), Image.Resampling.LANCZOS)
        x, y = i % 4 * 320, i // 4 * 206
        canvas.paste(im, (x, y))
        draw.text((x + 5, y + 180), path.stem, font=font(13), fill="#EDE3CC")
    canvas.save(OUT / "recorrido.png")


def test_results():
    logs = ROOT / "Logs/integracion-20261105"
    result = {}
    unity = logs / "editmode.xml"
    if not unity.exists():
        unity = OUT / "pruebas/editmode.xml"
    if unity.exists():
        r = ET.parse(unity).getroot()
        result["Unity_EditMode"] = {k: r.attrib.get(k) for k in ("total", "passed", "failed", "skipped", "result")}
    core = logs / "auditoria-ui.trx"
    if not core.exists():
        core = OUT / "pruebas/nucleo.trx"
    if core.exists():
        r = ET.parse(core).find(".//{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}Counters")
        result["Core_dotnet"] = dict(r.attrib)
    return result


def main():
    manifest = {"baseline_commit": "e4ee1d2", "tests": test_results(), "captures": []}
    for folder in (BEFORE, AFTER, OUT / "entrega-2400", OUT / "accesibilidad-1280", OUT / "interaccion-1280"):
        errors = folder / "errores.txt"
        if errors.exists() and errors.stat().st_size:
            raise ValueError(f"Recorrido con errores: {errors}")
        for path in sorted(folder.glob("*.png")):
            if folder.name == "interaccion-1280" and path.stem.startswith("home"):
                continue  # Este recorrido valida combate; el inicio definitivo está en entrega-*.
            with Image.open(path) as im:
                # Descarta capturas negras de una ventana minimizada o sin swapchain.
                deviation = sum(ImageStat.Stat(im.convert("RGB").resize((160, 90))).stddev)
                if deviation < 5:
                    raise ValueError(f"Captura vacia: {path}")
                manifest["captures"].append({"file": str(path.relative_to(OUT)).replace("\\", "/"), "size": list(im.size), "sha256": hashlib.sha256(path.read_bytes()).hexdigest()})
    states = [(OUT / "interaccion-1280" / f"interaccion_{i:02}.txt").read_text(encoding="utf-8") for i in range(1, 6)]
    assert "Enemigo: 100 / Cristalizado" in states[0]
    assert "Enemigo: 101 / Corneja" in states[1]
    assert "Aliada: 1 / Marisal" in states[2]
    assert states[2] == states[3], "El toque sobre el HUD cambio una seleccion"
    assert "Basic actor=4 target=101" in states[4]
    manifest["interaction_checks"] = {"enemy_plate": "passed", "ally_card": "passed", "hud_click_through": "passed", "command_target": "passed"}
    pairs = {"Marte_anterior_sobre_abismo": ("#C7301A", "#0B0B14"), "Jupiter_anterior_sobre_abismo": ("#6262EA", "#0B0B14"), "Texto_planeta_actual_sobre_abismo": ("#EDE3CC", "#0B0B14"), "Aliada_sobre_abismo": ("#9CCDBA", "#0B0B14"), "Enemigo_sobre_abismo": ("#F09591", "#0B0B14"), "Texto_vida_actual_sobre_abismo": ("#F6F4EE", "#0B0B14")}
    manifest["declared_color_contrast"] = {name: {"foreground": fg, "background": bg, "ratio": contrast(fg, bg)} for name, (fg, bg) in pairs.items()}
    manifest["contrast_scope"] = "Colores sRGB declarados sobre abismo opaco, sin antialiasing ni textura. No acredita contraste de toda la interfaz."
    # Medicion localizada del render: fondo vacio del rotulo de bando, sin texto.
    im = Image.open(AFTER / "combate_1_inicio.png").convert("RGB")
    samples = [im.getpixel((x, y)) for y in range(181, 207) for x in range(1490, 1510)]
    ratios = [contrast("#F09591", "#%02x%02x%02x" % rgb) for rgb in samples]
    manifest["render_sample_enemy_header"] = {"rect": [1490, 181, 1510, 207], "foreground_declared": "#F09591", "background_pixels": len(samples), "min_ratio": min(ratios), "max_ratio": max(ratios), "scope": "Solo fondo de este rotulo; no incluye glifos antialias ni otras superficies."}
    (OUT / "verificacion.json").write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    comparison("antes-despues-combate.png", "combate_1_inicio.png")
    comparison("antes-despues-inicio.png", "home.png")
    comparison("combate-sin-color.png", "combate_1_inicio.png", gray=True)
    details()
    contact_sheet()
    print(f"Verificadas {len(manifest['captures'])} capturas. Comparativas y verificacion.json guardados en {OUT}")
    print(json.dumps(manifest["tests"]))


if __name__ == "__main__":
    main()
