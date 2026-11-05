"""Track built-in image generations and create Pillow contact sheets. No API calls."""
from pathlib import Path
from datetime import datetime, timezone, timedelta
import hashlib
import json
import sys
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
BASE = ROOT / 'art' / 'retratos'
MANIFEST = BASE / 'manifest.json'


def save(data):
    MANIFEST.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def register(data, payload):
    for item in payload:
        source = Path(item['source']).resolve()
        if not source.is_relative_to(Path('C:/Users/Adass/.codex/generated_images').resolve()):
            raise ValueError('Unexpected generation source')
        name = item.get('filename', f"{item['id']}_f{item['phase']}_v{item['variant']}.png")
        target = BASE / item['id'] / name
        if target.exists():
            raise FileExistsError(target)
        target.parent.mkdir(parents=True, exist_ok=True)
        source.rename(target)
        with Image.open(target) as image:
            size = list(image.size)
            mode = image.mode
        row = dict(item)
        row.update(file=target.relative_to(ROOT).as_posix(), model=None,
                   model_requested='gpt-image-2', model_identifier_exposed_by_tool=False,
                   tool='built-in image_gen', size=size, color_mode=mode,
                   date=datetime.now(timezone(timedelta(hours=-6))).isoformat(),
                   selected=False, sha256=hashlib.sha256(target.read_bytes()).hexdigest(),
                   review={'status': 'pending_view_image', 'failure_reasons': []})
        data['images'].append(row)
        data['generations_used'] += 1
        data['generation_calls'] += 1
        data['successful_generations'] += 1
    save(data)


def review(data, payload):
    for item in payload:
        row = next(x for x in data['images'] if x['file'] == item['file'])
        row['review'] = {'method': 'view_image', **item['review']}
        if item.get('selected'):
            for other in data['images']:
                if other['id'] == row['id'] and other['phase'] == row['phase']:
                    other['selected'] = False
            row['selected'] = True
    save(data)


def sheets(data, include_failed=False):
    font = ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf', 16)
    small = ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf', 12)
    title = ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf', 23)
    for phase in (1, 3):
        rows = [x for x in data['images'] if x['phase'] == phase
                and (include_failed or x['review']['status'].startswith('passed'))
                and x.get('kind', 'variant') == 'variant']
        if not rows:
            continue
        rows.sort(key=lambda x: (next(i for i, p in enumerate(data['plans'])
                                     if p['id'] == x['id'] and p['phase'] == phase), x['variant'], x.get('attempt', 0)))
        cols = 5
        cw, ch = 240, 444
        sheet = Image.new('RGB', (cols*cw+24, ((len(rows)+cols-1)//cols)*ch+68), '#E9DFC8')
        draw = ImageDraw.Draw(sheet)
        label = 'todas las variantes / descartes marcados' if include_failed else 'variantes aprobadas'
        draw.text((16, 14), f'Sal y Almas - Fase {phase} - {len(rows)} {label}', font=title, fill='#121016')
        for index, row in enumerate(rows):
            x = 16 + (index % cols)*cw
            y = 60 + (index // cols)*ch
            with Image.open(ROOT / row['file']) as original:
                preview = original.copy()
                preview.thumbnail((216, 324), Image.Resampling.LANCZOS)
                sheet.paste(preview, (x, y))
                tiny = original.copy()
                tiny.thumbnail((64, 64), Image.Resampling.LANCZOS)
                sheet.paste(tiny, (x, y+354))
            draw.text((x, y+328), Path(row['file']).name, font=font, fill='#121016')
            failed = row['review']['status'] == 'failed'
            state = ' / DESCARTADA' if failed else (' / elegida' if row['selected'] else '')
            draw.text((x+51, y+372), '64 px' + state, font=small, fill='#7A1E2C' if failed else '#3F4150')
        suffix = '_todas' if include_failed else ''
        filename = f'hoja_f{phase}{suffix}.png'
        sheet.save(BASE / filename)
        data['contact_sheets'][f'phase{phase}{suffix}'] = {'file': f'art/retratos/{filename}', 'status': 'current', 'variants': len(rows)}
    save(data)


if __name__ == '__main__':
    data = json.loads(MANIFEST.read_text(encoding='utf-8'))
    command = sys.argv[1]
    if command == 'register':
        register(data, json.load(sys.stdin))
    elif command == 'review':
        review(data, json.load(sys.stdin))
    elif command == 'sheets':
        sheets(data)
    elif command == 'sheets-all':
        sheets(data, include_failed=True)
    else:
        raise ValueError(command)
    print(json.dumps({'generations': data['generations_used'], 'images': len(data['images']),
                      'selected': sum(x['selected'] for x in data['images'])}))
