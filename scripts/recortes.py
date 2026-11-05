"""Recortes de las brujas elegidas: cuerpo sin fondo y bustos cuadrados.

Toma los retratos con "selected": true de art/retratos/manifest.json, les quita el
fondo con BiRefNet (rembg) y escribe en art/recortes/<id>/:

  <id>_f<fase>_cuerpo.png      cuerpo completo, RGBA, recortado al contorno
  <id>_f<fase>_busto.png       busto cuadrado a la resolución de origen, alfa suave
  <id>_f<fase>_busto_160.png   busto a 160 px nativos, alfa duro
  <id>_f<fase>_busto_320.png   el de 160 ampliado 2x con vecino más cercano

El encuadre de cada busto vive en art/recortes/encuadres.json (centro y lado en
píxeles del retrato original de 1024x1536) para poder ajustarlo a mano.

Uso:
  pip install "rembg[cpu]"
  python scripts/recortes.py todo            # cortar + bustos + hojas
  python scripts/recortes.py cortar [ids]    # solo quitar fondos (lento: ~30 s por imagen en CPU)
  python scripts/recortes.py limpiar [ids]   # descarta islas sueltas en cuerpos ya cortados
  python scripts/recortes.py bustos [ids]    # rehace bustos desde los cuerpos ya cortados
  python scripts/recortes.py hojas           # hojas de contacto
  python scripts/recortes.py procesar art/bustos/brea/brea_f1_neutral_v1.png [...]
      bustos nuevos de docs/Bustos para Codex.md: escribe junto a cada imagen
      <nombre>_recorte.png, <nombre>_160.png y <nombre>_320.png

[ids] filtra por clave, por ejemplo: marisal_f1 brea_f1
"""
from pathlib import Path
import hashlib
import json
import subprocess
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'art' / 'retratos'
OUT = ROOT / 'art' / 'recortes'
FRAMES = OUT / 'encuadres.json'
MANIFEST = OUT / 'manifest.json'
MODEL = 'birefnet-general'
ALPHA_FLOOR = 10      # alfa por debajo de esto se vuelve 0 (ruido del modelo)
MARGIN = 8            # margen transparente alrededor del cuerpo recortado
BRIDGE = 6            # px que se unen al agrupar piezas; lo que quede más lejos de la figura se descarta
NATIVE = 160          # lado del busto nativo, como la referencia "160 native / 2x nearest"
PALETTE = 0           # colores del busto nativo; 0 = sin cuantizar (cuantizar apaga los ojos)
ORDER = ['marisal', 'selenia', 'solenne', 'cervanda', 'tormentina', 'cupria',
         'brea', 'cempa', 'muertita', 'salitre', 'topacia', 'axa']
NAMES = {'marisal': 'Marisal Hojadelobo', 'selenia': 'Selenia de Plata', 'solenne': 'Solenne',
         'cervanda': 'Cervanda', 'tormentina': 'Tormentina', 'cupria': 'Cupria', 'brea': 'Brea',
         'cempa': 'Cempa', 'muertita': 'Muertita', 'salitre': 'Salitre', 'topacia': 'Topacia',
         'axa': 'Axa'}


def chosen():
    data = json.loads((SOURCE / 'manifest.json').read_text(encoding='utf-8'))
    rows = [x for x in data['images'] if x.get('selected')]
    rows.sort(key=lambda x: (x['phase'], ORDER.index(x['id'])))
    return [{'key': f"{x['id']}_f{x['phase']}", 'id': x['id'], 'phase': x['phase'],
             'source': x['file']} for x in rows]


def load_manifest():
    if MANIFEST.exists():
        return json.loads(MANIFEST.read_text(encoding='utf-8'))
    return {'schema_version': 1, 'model': MODEL, 'tool': 'rembg + pymatting',
            'script': 'scripts/recortes.py', 'items': {}}


def save_manifest(data):
    MANIFEST.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def matte(src):
    """Quita el fondo con BiRefNet y limpia el halo del fondo en los bordes. Devuelve RGBA."""
    import onnxruntime as ort
    from pymatting import estimate_foreground_ml
    from rembg import new_session, remove

    opts = ort.SessionOptions()
    opts.enable_cpu_mem_arena = False
    session = new_session(MODEL, sess_opts=opts)
    rgb = Image.open(src).convert('RGB')
    mask = remove(rgb, session=session, only_mask=True)
    alpha = np.asarray(mask, dtype=np.float64) / 255.0
    alpha[alpha < ALPHA_FLOOR / 255.0] = 0.0
    fg = estimate_foreground_ml(np.asarray(rgb, dtype=np.float64) / 255.0, alpha)
    rgba = np.dstack([np.clip(fg, 0, 1), alpha])
    return Image.fromarray((rgba * 255 + 0.5).astype(np.uint8), 'RGBA')


def keep_main(image):
    """Deja solo la figura principal y descarta islas sueltas: restos del Ascendente, reflejos y chispas."""
    from scipy import ndimage

    alpha = np.asarray(image.getchannel('A'))
    solid = alpha >= 128
    groups, count = ndimage.label(ndimage.binary_dilation(solid, iterations=BRIDGE))
    if count <= 1:
        return image, 0.0
    sizes = ndimage.sum(solid, groups, range(1, count + 1))
    main = groups == int(np.argmax(sizes)) + 1
    kept = np.where(main, alpha, 0).astype(np.uint8)
    removed = 1.0 - float(kept.sum(dtype=np.int64)) / float(alpha.sum(dtype=np.int64))
    result = image.copy()
    result.putalpha(Image.fromarray(kept, 'L'))
    return result, removed


def bbox(image):
    ys, xs = np.nonzero(np.asarray(image.getchannel('A')))
    return [max(int(xs.min()) - MARGIN, 0), max(int(ys.min()) - MARGIN, 0),
            min(int(xs.max()) + 1 + MARGIN, image.width), min(int(ys.max()) + 1 + MARGIN, image.height)]


def body_row(image, box, removed):
    return {'cuerpo_en_origen': box, 'cuerpo_size': [box[2] - box[0], box[3] - box[1]],
            'alfa_descartado_en_islas': round(removed, 4)}


def cut_one(item):
    """Recorta un retrato elegido. Corre en su propio proceso: BiRefNet usa mucha memoria."""
    src = ROOT / item['source']
    image, removed = keep_main(matte(src))
    box = bbox(image)
    target = OUT / item['id'] / f"{item['key']}_cuerpo.png"
    target.parent.mkdir(parents=True, exist_ok=True)
    image.crop(box).save(target, optimize=True)
    return {'source': item['source'], 'source_sha256': sha(src),
            'cuerpo': target.relative_to(ROOT).as_posix(), **body_row(image, box, removed)}


def clean(items):
    """Aplica keep_main a cuerpos ya cortados, sin volver a correr el modelo."""
    data = load_manifest()
    for item in items:
        row = data['items'][item['key']]
        image, removed = keep_main(full_canvas(row))
        box = bbox(image)
        image.crop(box).save(ROOT / row['cuerpo'], optimize=True)
        row.update(body_row(image, box, removed))
        print(f"limpio {item['key']}: {removed:.1%} del alfa estaba en islas sueltas", flush=True)
    save_manifest(data)


def cut(items):
    data = load_manifest()
    for item in items:
        print(f"cortando {item['key']} ...", flush=True)
        result = subprocess.run([sys.executable, __file__, '_uno', json.dumps(item)],
                                capture_output=True, text=True, check=True)
        row = json.loads(result.stdout.strip().splitlines()[-1])
        data['items'].setdefault(item['key'], {}).update(row)
        save_manifest(data)


def full_canvas(row):
    """Devuelve el cuerpo recortado pegado sobre un lienzo del tamaño del original."""
    with Image.open(ROOT / row['source']) as src:
        size = src.size
    canvas = Image.new('RGBA', size, (0, 0, 0, 0))
    with Image.open(ROOT / row['cuerpo']) as body:
        canvas.paste(body, tuple(row['cuerpo_en_origen'][:2]))
    return canvas


def to_native(bust):
    """Busto de 160 px: reducción Lanczos, paleta opcional y alfa binario."""
    small = bust.resize((NATIVE, NATIVE), Image.Resampling.LANCZOS)
    alpha = small.getchannel('A').point(lambda a: 255 if a >= 128 else 0)
    rgb = Image.new('RGB', small.size, (0, 0, 0))
    rgb.paste(small.convert('RGB'), mask=alpha)
    if PALETTE:
        rgb = rgb.quantize(colors=PALETTE, method=Image.Quantize.MEDIANCUT,
                           dither=Image.Dither.NONE).convert('RGB')
    native = rgb.convert('RGBA')
    native.putalpha(alpha)
    return native


def busts(items):
    data = load_manifest()
    frames = json.loads(FRAMES.read_text(encoding='utf-8'))
    for item in items:
        row = data['items'][item['key']]
        frame = frames[item['key']]
        cx, cy, side = frame['centro'][0], frame['centro'][1], frame['lado']
        box = (cx - side // 2, cy - side // 2, cx - side // 2 + side, cy - side // 2 + side)
        bust = full_canvas(row).crop(box)
        folder = OUT / item['id']
        paths = {name: folder / f"{item['key']}_{name}.png" for name in ('busto', 'busto_160', 'busto_320')}
        bust.save(paths['busto'], optimize=True)
        native = to_native(bust)
        native.save(paths['busto_160'], optimize=True)
        native.resize((NATIVE * 2, NATIVE * 2), Image.Resampling.NEAREST).save(paths['busto_320'], optimize=True)
        row.update({'busto_en_origen': list(box), 'busto_lado_origen': side,
                    **{name: p.relative_to(ROOT).as_posix() for name, p in paths.items()}})
        print(f"busto {item['key']} lado {side}", flush=True)
    data['native_size'] = NATIVE
    data['native_palette_colors'] = PALETTE
    save_manifest(data)


def font(size):
    for name in ('C:/Windows/Fonts/segoeui.ttf', '/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',
                 '/System/Library/Fonts/Supplemental/Arial.ttf'):
        if Path(name).exists():
            return ImageFont.truetype(name, size)
    return ImageFont.load_default()


def sheets(items):
    data = load_manifest()
    title, label = font(22), font(15)
    # Cuerpos sobre pergamino.
    cw, ch, cols = 220, 360, 6
    rows = (len(items) + cols - 1) // cols
    sheet = Image.new('RGB', (cols * cw + 24, rows * (ch + 30) + 60), '#E9DFC8')
    draw = ImageDraw.Draw(sheet)
    draw.text((16, 14), f'Sal y Almas - recortes de cuerpo - {len(items)} brujas', font=title, fill='#121016')
    for i, item in enumerate(items):
        x, y = 16 + (i % cols) * cw, 56 + (i // cols) * (ch + 30)
        with Image.open(ROOT / data['items'][item['key']]['cuerpo']) as body:
            body = body.copy()
        body.thumbnail((cw - 12, ch), Image.Resampling.LANCZOS)
        sheet.paste(body, (x + (cw - 12 - body.width) // 2, y + ch - body.height), body)
        draw.text((x, y + ch + 4), item['key'], font=label, fill='#121016')
    sheet.save(OUT / 'hoja_cuerpos.png', optimize=True)
    # Bustos como la referencia: 160 nativos, 2x nearest, fondo oscuro.
    tile, cols = NATIVE * 2, 5
    rows = (len(items) + cols - 1) // cols
    sheet = Image.new('RGB', (cols * (tile + 16) + 16, rows * (tile + 36) + 52), '#16151B')
    draw = ImageDraw.Draw(sheet)
    draw.text((16, 12), f'Bustos recortados / {NATIVE} nativo / 2x nearest', font=title, fill='#E9DFC8')
    for i, item in enumerate(items):
        x, y = 16 + (i % cols) * (tile + 16), 48 + (i // cols) * (tile + 36)
        with Image.open(ROOT / data['items'][item['key']]['busto_320']) as bust:
            back = Image.new('RGBA', bust.size, '#2A2733')
            back.alpha_composite(bust.convert('RGBA'))
            sheet.paste(back.convert('RGB'), (x, y))
        phase = 'Turbia' if item['phase'] == 1 else 'Quintaesencia'
        draw.text((x, y + tile + 6), f"{NAMES[item['id']]} · {phase}", font=label, fill='#E9DFC8')
    sheet.save(OUT / 'hoja_bustos.png', optimize=True)
    data['contact_sheets'] = ['art/recortes/hoja_cuerpos.png', 'art/recortes/hoja_bustos.png']
    save_manifest(data)


def process_one(path):
    """Busto nuevo generado (cuadrado, fondo plano): recorte + 160 nativo + 320 nearest."""
    path = Path(path).resolve()
    image = matte(path)
    side = max(image.size)
    if image.width != image.height:
        square = Image.new('RGBA', (side, side), (0, 0, 0, 0))
        square.paste(image, ((side - image.width) // 2, (side - image.height) // 2))
        image = square
    stem = path.with_suffix('')
    image.save(f'{stem}_recorte.png', optimize=True)
    native = to_native(image)
    native.save(f'{stem}_160.png', optimize=True)
    native.resize((NATIVE * 2, NATIVE * 2), Image.Resampling.NEAREST).save(f'{stem}_320.png', optimize=True)
    return [f'{stem.name}_recorte.png', f'{stem.name}_160.png', f'{stem.name}_320.png']


def main(argv):
    if argv and argv[0] == '_uno':
        print(json.dumps(cut_one(json.loads(argv[1]))))
        return
    if argv and argv[0] == '_procesar':
        print(json.dumps(process_one(argv[1])))
        return
    if argv and argv[0] == 'procesar':
        for path in argv[1:]:
            print(f'procesando {path} ...', flush=True)
            result = subprocess.run([sys.executable, __file__, '_procesar', path],
                                    capture_output=True, text=True, check=True)
            print('  ' + ', '.join(json.loads(result.stdout.strip().splitlines()[-1])), flush=True)
        return
    command, keys = (argv[0] if argv else 'todo'), set(argv[1:])
    items = [x for x in chosen() if not keys or x['key'] in keys]
    OUT.mkdir(parents=True, exist_ok=True)
    if command in ('cortar', 'todo'):
        cut(items)
    if command == 'limpiar':
        clean(items)
    if command in ('bustos', 'todo'):
        busts(items)
    if command in ('hojas', 'todo'):
        sheets(chosen())


if __name__ == '__main__':
    main(sys.argv[1:])
