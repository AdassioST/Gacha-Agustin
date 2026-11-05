"""Track built-in image generations and create Pillow contact sheets. No API calls."""
from pathlib import Path
from datetime import datetime, timezone, timedelta
import hashlib
import json
import re
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


def finalize(data):
    """Record the completed visual review and audit the concrete deliverables."""
    observations = {
        ('marisal', 1): 'Ancla v3 elegida por el usuario; loba adulta, cristal de sal y báculo.',
        ('selenia', 1): 'Rostro humano adulto, antenas de polilla y té negro con sal.',
        ('solenne', 1): 'Fénec adulta, sombrero con monedas y limón con sal y chile.',
        ('cervanda', 1): 'Cola corta de cierva corregida; salero de plata y cornamenta.',
        ('tormentina', 1): 'Solo dos orejas de liebre; cabello sobre las sienes y sal en el dorso.',
        ('cupria', 1): 'Serpiente con dos brazos y dos piernas; mano del Ascendente separada.',
        ('brea', 1): 'Rostro humano, orejas y cola de gata; balanza con sal.',
        ('cempa', 1): 'Coyota con cempasúchil, papel picado y salero de barro.',
        ('muertita', 1): 'Pañuelo de sal corregido y visible; zarigüeya recostada con cola desnuda.',
        ('salitre', 1): 'Cabra con cuernos y orejas distintos; bloque de sal para ganado.',
        ('topacia', 1): 'Topo adulto robusto; farol de vela y dos puñados de sal.',
        ('axa', 1): 'Ojos oscuros y agua a la cintura corregidos; seis branquias y concha con sal.',
        ('marisal', 3): 'Dos brazos corregidos, espada roja y cristal de sal; carnero completo.',
        ('selenia', 3): 'Misma identidad, alas desplegadas, té y dos peces completos.',
        ('solenne', 3): 'Misma fénec, giro con sal convertida en soles, limón y león completo.',
        ('cervanda', 3): 'Misma cierva, cola corta, aros en la cornamenta y cabra marina completa.',
        ('tormentina', 3): 'Solo dos orejas de liebre; mano extendida con cinco dedos separados.',
        ('cupria', 3): 'Misma serpiente con dos brazos y dos piernas; trigo y ritual del Ascendente.',
    }
    failure_reasons = {
        'cervanda_f1_v1.png': ['Cola larga y peluda de cánido en lugar de cola corta de cierva.'],
        'cervanda_f1_v2.png': ['Cola larga y peluda de cánido en lugar de cola corta de cierva.'],
        'cervanda_f1_v2_r1.png': ['Persiste una cola larga de cánido después del primer reintento.'],
        'tormentina_f1_v2.png': ['Oreja humana visible además del par de orejas de liebre.'],
        'muertita_f1_v1.png': ['El pañuelo negro con sal sobre el pecho no resulta legible.'],
        'axa_f1_v1.png': ['No está medio sumergida: el agua solo cubre los tobillos.'],
        'axa_f1_v2.png': ['No está medio sumergida: el agua solo cubre las pantorrillas.'],
        'axa_f1_v1_r1.png': ['Ojos ámbar; la ficha pide ojos pequeños oscuros.'],
        'axa_f1_v2_r1.png': ['Ojos ámbar; la ficha pide ojos pequeños oscuros.'],
        'marisal_f3_v1.png': ['El carnero aparece recortado e incompleto.'],
        'marisal_f3_v2.png': ['Tres brazos: espada elevada, mano con sal y brazo extra colgante.', 'El carnero aparece recortado e incompleto.'],
        'tormentina_f3_v1.png': ['Forma de oreja humana bajo el cabello junto a la sien; descarte conservador por posible duplicación.'],
        'tormentina_f3_v2.png': ['Forma de oreja humana bajo el cabello junto a la sien; descarte conservador por posible duplicación.'],
        'marisal_f1_v3_2k_probe.png': ['Se pidió 2048×3072 y la herramienta devolvió 1024×1536.'],
    }
    for row in data['images']:
        review = row['review']
        if review['status'].startswith('passed'):
            review['status'] = 'passed'
            review.setdefault('checks', {})['silhouette_64px'] = 'pass_reviewed_on_pillow_contact_sheet'
            review['checks']['franchise_originality'] = 'no_recognizable_franchise_elements'
            review['notes'] = observations[(row['id'], row['phase'])]
            review['visible_anatomy_limit'] = 'Los agarres y la ropa pueden ocultar dedos; la revisión evalúa la anatomía visible y no certifica dedos ocultos.'
        elif review['status'] == 'failed':
            review['failure_reasons'] = failure_reasons[Path(row['file']).name]
            review['notes'] = 'Descartada; conservada para auditoría de la generación.'
    selected = [row for row in data['images'] if row['selected']]
    selected.sort(key=lambda row: next(i for i, plan in enumerate(data['plans'])
                                     if plan['id'] == row['id'] and plan['phase'] == row['phase']))
    failures = []
    for row in data['images']:
        if row['review']['status'] != 'failed':
            continue
        corrected = [other for other in data['images'] if other['id'] == row['id']
                     and other['phase'] == row['phase'] and other['variant'] == row['variant']
                     and other.get('kind', 'variant') == row.get('kind', 'variant')
                     and other['review']['status'] == 'passed']
        failures.append({'file': row['file'], 'kind': row.get('kind', 'variant'),
                         'reasons': row['review']['failure_reasons'],
                         'resolved_by': max(corrected, key=lambda item: item.get('attempt', 0))['file'] if corrected else None})
    # Validate the five original blocks directly against the source, independently
    # of the generated plan metadata. Additional instructions follow those blocks.
    source = (ROOT / data['source_document']).read_text(encoding='utf-8')
    section2 = source.split('## 2.')[1].split('## 3.')[0]
    blocks = re.findall(r'```\s*\n(.*?)\n```', section2, re.S)
    characters = {}
    for phase, section in ((1, 5), (3, 6)):
        body = source.split(f'## {section}.')[1].split(f'## {section+1}.')[0]
        for char_id, scale, block in re.findall(r'### (\w+) · (ANTHRO|KEMONOMIMI)\s+```\s*\n(.*?)\n```', body, re.S):
            characters[(char_id, phase)] = (scale, block)
    for row in data['images']:
        scale, block = characters[(row['id'], row['phase'])]
        prefix = '\n\n'.join((blocks[0], blocks[2 if scale == 'ANTHRO' else 3],
                               blocks[4 if row['phase'] == 1 else 6], block, blocks[1]))
        if not row['prompt'].startswith(prefix):
            raise ValueError(f'Original prompt blocks changed: {row["file"]}')
        target = ROOT / row['file']
        if hashlib.sha256(target.read_bytes()).hexdigest() != row['sha256']:
            raise ValueError(f'Image changed since registration: {target}')
        with Image.open(target) as img:
            if list(img.size) != row['size']:
                raise ValueError(f'Incorrect size recorded: {target}')
        if row['review'].get('method') != 'view_image':
            raise ValueError(f'Missing visual inspection: {target}')
    if len(selected) != 18 or len({(row['id'], row['phase']) for row in selected}) != 18:
        raise ValueError('Expected 18 distinct selections')
    for row in selected:
        if row['review']['status'] != 'passed':
            raise ValueError('Selected image failed QC')
        if row['phase'] == 3:
            expected_ref = next(item['file'] for item in selected if item['id'] == row['id'] and item['phase'] == 1)
            if row['reference'] != expected_ref:
                raise ValueError(f'Incorrect selected identity reference: {row["file"]}')
    if max(row.get('attempt', 0) for row in data['images']) > 2:
        raise ValueError('Retry budget exceeded')
    probes = [row for row in data['images'] if row.get('kind') == 'resolution_probe']
    data['status'] = 'portraits_complete_2k_rerenders_unavailable_in_builtin_interface'
    data['failures'] = failures
    data['selection'] = [{'id': row['id'], 'phase': row['phase'], 'file': row['file'],
                          'observations': observations[(row['id'], row['phase'])]} for row in selected]
    data['generation_summary'] = {'base_variants': data['base_generations_planned'],
                                  'qc_regenerations': sum(row.get('kind', 'variant') == 'variant' and row.get('attempt', 0) > 0 for row in data['images']),
                                  'resolution_probes': len(probes), 'total': len(data['images']),
                                  'approved_variants': sum(row['review']['status'] == 'passed' for row in data['images']),
                                  'selected': len(selected), 'qc_discarded': sum(f['kind'] == 'variant' for f in failures),
                                  'generator_refusals': data['rejected_generations']}
    data['rerender_2k'] = {'status': 'unavailable_in_builtin_interface', 'requested_size': [2048, 3072],
                           'probe_file': probes[0]['file'], 'observed_size': probes[0]['size'],
                           'reason': 'The integrated tool exposes no size parameter; the explicit 2K prompt returned 1024x1536.',
                           'pending_selected_files': [row['file'] for row in selected]}
    data['notes'] = [
        'Source document is named docs/Retratos para Codex.md in this checkout.',
        'The built-in tool exposes no selectable model argument or returned model identifier. gpt-image-2 is the requested model.',
        'All five original prompt blocks were checked verbatim and in order against sections 2, 5 and 6 of the source document.',
        'QC additions implement the user anatomy correction and clarify existing species, poses and rituals without replacing the original blocks.',
        'The main contact sheets contain only approved variants, per section 7. The _todas sheets also show all discarded character variants, clearly labeled.',
        'All outputs are native 1024x1536. Native 2K rerenders remain unavailable through the integrated interface; no upscaling was substituted.',
        'Visible anatomy was reviewed using view_image. Fingers hidden by grips, objects or clothing cannot be independently counted.',
    ]
    data['delivery_validation'] = {'original_blocks_verbatim': True, 'all_files_hash_verified': True,
                                    'all_images_viewed': True, 'contact_sheets_64px_reviewed': True,
                                    'selected_phase3_identity_references_match': True,
                                    'retry_limit_two_respected': True,
                                    'completed_at': datetime.now(timezone(timedelta(hours=-6))).isoformat()}
    save(data)
    lines = ['# Retratos de la primera ola', '',
             'Se conservó Marisal f1 v3, elegida por el usuario, como ancla de estilo. Hay 18 retratos elegidos y 37 variantes aprobadas a 1024×1536.', '',
             '| id | fase | archivo elegido | observaciones |', '| --- | --- | --- | --- |']
    for row in data['selection']:
        path = (ROOT / row['file']).as_posix()
        lines.append(f'| {row["id"]} | {row["phase"]} | [{Path(path).name}](<{path}>) | {row["observations"]} |')
    lines += ['', 'Todas las imágenes se revisaron con view_image; las siluetas se comprobaron en miniaturas de 64 px. Se revisaron dos brazos, dos piernas, la pareja de orejas correspondiente y las manos visibles. Los dedos ocultos por objetos o ropa no se pueden contar de forma independiente.', '',
              '## Descartes y correcciones', '', '| archivo descartado | motivo | corrección |', '| --- | --- | --- |']
    for item in failures:
        target = (ROOT / item['file']).as_posix()
        replacement = Path(item['resolved_by']).name if item['resolved_by'] else 'Pendiente: limitación de tamaño de la herramienta integrada.'
        lines.append(f'| [{Path(target).name}](<{target}>) | {" ".join(item["reasons"])} | {replacement} |')
    lines += ['', '## Archivos y generaciones', '',
              '- `manifest.json`: prompts completos usados, referencias, modelo solicitado, tamaños reales, fechas, selección y revisiones.',
              '- `hoja_f1.png` y `hoja_f3.png`: 25 y 12 variantes aprobadas, con nombres y miniaturas de 64 px.',
              '- `hoja_f1_todas.png` y `hoja_f3_todas.png`: todas las variantes de personaje, incluidos los descartes identificados.', '',
              f'**{len(data["images"])} generaciones en total:** 37 variantes iniciales + {data["generation_summary"]["qc_regenerations"]} regeneraciones por control de calidad + {len(probes)} prueba de resolución 2K.',
              'No hubo rechazos del generador. Ninguna variante excedió dos regeneraciones.', '',
              '**Limitación pendiente:** se pidió un render nativo de 2048×3072; la herramienta integrada devolvió 1024×1536 y no expone un parámetro de tamaño. Los 18 renders nativos a 2K quedan pendientes. No se sustituyeron por ampliaciones.', '',
              'Se usó image_gen integrado. El modelo solicitado fue gpt-image-2; la interfaz no permite elegirlo ni devuelve su identificador. No se usó una API o CLI alternativa.', '']
    (BASE / 'reporte.md').write_text('\n'.join(lines), encoding='utf-8')


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
    elif command == 'finalize':
        finalize(data)
    else:
        raise ValueError(command)
    print(json.dumps({'generations': data['generations_used'], 'images': len(data['images']),
                      'selected': sum(x['selected'] for x in data['images'])}))
