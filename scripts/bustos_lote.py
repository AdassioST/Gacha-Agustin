"""Registro y preparación de las generaciones integradas de bustos.

No genera imágenes. Conserva cada original y normaliza a 1024 px con la
autorización del usuario; recortes.py sigue siendo el procesador de alfa/HUD.
"""
from pathlib import Path
import argparse
import hashlib
import json
import shutil
from datetime import datetime, timezone

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'art' / 'bustos'
MANIFEST = OUT / 'manifest.json'


def read():
    return json.loads(MANIFEST.read_text(encoding='utf-8'))


def save(data):
    MANIFEST.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def relative(path):
    return path.relative_to(ROOT).as_posix()


def stamp():
    return datetime.now(timezone.utc).isoformat()


def ingest(index, variant, source):
    data = read()
    entry = data['entries'][index]
    stem = f"{entry['id']}_f{entry['phase']}_neutral_v{variant}"
    target = OUT / entry['id'] / (stem + '.png')
    original = OUT / '_originales' / entry['id'] / (stem + '.png')
    if target.exists() or original.exists():
        raise FileExistsError(stem)
    source = Path(source).resolve()
    if not source.is_file():
        raise FileNotFoundError(source)
    target.parent.mkdir(parents=True, exist_ok=True)
    original.parent.mkdir(parents=True, exist_ok=True)
    shutil.move(str(source), original)
    with Image.open(original) as image:
        original_size = list(image.size)
        image.convert('RGB').resize((1024, 1024), Image.Resampling.LANCZOS).save(target, optimize=True)
    row = dict(id=entry['id'], phase=entry['phase'], expression='neutral', variant=variant,
               file=relative(target), original_file=relative(original),
               generated_source=str(source), original_size=original_size, size=[1024, 1024],
               original_sha256=hashlib.sha256(original.read_bytes()).hexdigest(),
               sha256=hashlib.sha256(target.read_bytes()).hexdigest(),
               prompt=entry['prompt'], references=entry['references'],
               reference_roles=['character close-up', 'full character', 'pixel-art style', 'style and framing'],
               date='2026-11-05', ingested_at_utc=stamp(), selected=False,
               model='not verifiable through integrated tool',
               normalized_to_1024=True, normalization_authorized_by_user=True,
               processing_status='pending', review_status='pending', observations=[])
    data['images'].append(row)
    data['generation_attempts'].append(dict(id=entry['id'], phase=entry['phase'], expression='neutral',
        variant=variant, status='generated', file=row['file'], recorded_at_utc=stamp()))
    data['total_generation_calls'] = len(data['generation_attempts'])
    data['successful_generations'] = len(data['images'])
    data['normalization'] = dict(authorized=True, target_size=[1024, 1024],
        method='Pillow Lanczos; original preserved without modification',
        model='not verifiable through integrated tool')
    save(data)
    print(row['file'])


def review(index, variant, status, notes):
    data = read()
    entry = data['entries'][index]
    row = next(x for x in data['images'] if x['id'] == entry['id'] and x['phase'] == entry['phase'] and x['variant'] == variant)
    target = ROOT / row['file']
    outputs = {}
    for suffix, expected in [('recorte', (1024, 1024)), ('160', (160, 160)), ('320', (320, 320))]:
        path = target.with_name(target.stem + '_' + suffix + '.png')
        with Image.open(path) as image:
            if image.size != expected or image.mode != 'RGBA':
                raise ValueError(f'{path}: {image.size}, {image.mode}')
        outputs[suffix] = relative(path)
    row.update(outputs=outputs, processing_status='complete', review_status=status,
               reviewed_files=[outputs['recorte'], outputs['320']], reviewed_at_utc=stamp(),
               review_method='view_image against section 9 and source references', observations=notes)
    if status == 'failed':
        data['failures'].append(dict(file=row['file'], reasons=notes, variant=variant))
    save(data)
    print(row['file'], status)


def sheet():
    data = read()
    rows = [x for x in data['images'] if x.get('outputs')]
    columns = 6
    cell_w, cell_h = 344, 364
    canvas = Image.new('RGB', (columns * cell_w, ((len(rows) + columns - 1) // columns) * cell_h), '#16151B')
    draw = ImageDraw.Draw(canvas)
    try:
        font = ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf', 18)
    except OSError:
        font = ImageFont.load_default(size=18)
    for i, row in enumerate(rows):
        x, y = (i % columns) * cell_w + 12, (i // columns) * cell_h + 8
        with Image.open(ROOT / row['outputs']['320']) as image:
            canvas.paste(image, (x, y), image)
        label = f"{row['id']} f{row['phase']} v{row['variant']}"
        if row['review_status'] != 'passed':
            label += ' [REVISAR]'
        draw.text((x + 160, y + 326), label, anchor='mt', font=font,
                  fill='#E8DFC9' if row['review_status'] == 'passed' else '#FF9898')
    path = OUT / 'hoja_bustos.png'
    canvas.save(path, optimize=True)
    data['contact_sheets'] = [relative(path)]
    save(data)
    print(relative(path))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    subs = parser.add_subparsers(dest='command', required=True)
    p = subs.add_parser('ingest')
    p.add_argument('index', type=int)
    p.add_argument('variant', type=int)
    p.add_argument('source')
    p = subs.add_parser('review')
    p.add_argument('index', type=int)
    p.add_argument('variant', type=int)
    p.add_argument('status', choices=['passed', 'failed'])
    p.add_argument('notes', nargs='+')
    subs.add_parser('sheet')
    args = parser.parse_args()
    if args.command == 'ingest':
        ingest(args.index, args.variant, args.source)
    elif args.command == 'review':
        review(args.index, args.variant, args.status, args.notes)
    else:
        sheet()
