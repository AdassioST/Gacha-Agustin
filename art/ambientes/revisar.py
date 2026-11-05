"""Read-only PNG inspection; writes the delivery inventory beside this script."""
from pathlib import Path
import csv
import hashlib
from datetime import datetime
from collections import Counter, defaultdict
from PIL import Image

root = Path(__file__).resolve().parents[2]
out = root / 'art' / 'ambientes'
with (out / 'contrato_entrega.tsv').open(encoding='utf-8-sig', newline='') as stream:
    expected = list(csv.DictReader(stream, delimiter='\t'))
provisional = {'amb_leo_subsuelo', 'amb_virgo_subsuelo'}
rows, errors = [], []
hashes = defaultdict(list)
for job in expected:
    enemy = job['tipo'] == 'enemigo'
    folder = 'Enemigos' if enemy else 'Ambientes'
    path = root / 'Assets' / 'SalYAlmas' / 'Art' / folder / (job['id'] + '.png')
    row = {**job, 'estado': 'pendiente', 'dimensiones': '', 'modo': '', 'alfa': '', 'margen_min_px': '', 'sha256': '', 'ruta': path.relative_to(root).as_posix()}
    if path.exists():
        with Image.open(path) as im:
            im.load()
            row['dimensiones'] = f'{im.width}x{im.height}'
            row['modo'] = im.mode
            size = (1024, 1024) if enemy else (1920, 1080)
            if im.format != 'PNG' or im.size != size:
                errors.append(job['id'] + ': formato o dimensiones incorrectos')
            if enemy:
                if im.mode != 'RGBA':
                    errors.append(job['id'] + ': no es RGBA')
                else:
                    alpha = im.getchannel('A')
                    low, high = alpha.getextrema()
                    row['alfa'] = f'{low}..{high}'
                    box = alpha.point(lambda a: 255 if a > 2 else 0).getbbox()
                    if box:
                        margin = min(box[0], box[1], im.width-box[2], im.height-box[3])
                        row['margen_min_px'] = str(margin)
                        if margin < 62 or low != 0 or high != 255:
                            errors.append(job['id'] + ': alfa o margen menor de 6 %')
                    else:
                        errors.append(job['id'] + ': silueta vacia')
            row['sha256'] = hashlib.sha256(path.read_bytes()).hexdigest()
            hashes[row['sha256']].append(job['id'])
        kind = 'enemigos' if enemy else 'ambientes'
        sources = root / 'art' / kind / 'fuentes'
        if not list(sources.glob(job['id'] + '_fuente*.png')):
            errors.append(job['id'] + ': falta fuente original')
        prompt_files = list(sources.glob(job['id'] + '*prompt.md'))
        aggregate = root / 'art' / kind / 'prompts.md'
        if not prompt_files and not (aggregate.exists() and job['id'] in aggregate.read_text(encoding='utf-8-sig')):
            errors.append(job['id'] + ': falta prompt')
        row['estado'] = 'provisional_rehacer_subsuelo' if job['id'] in provisional else 'entregado'
    rows.append(row)
for ids in hashes.values():
    if len(ids) > 1:
        errors.append('PNG duplicado: ' + ', '.join(ids))
known = {row['ruta'] for row in rows}
for folder in ('Ambientes', 'Enemigos'):
    for path in (root / 'Assets' / 'SalYAlmas' / 'Art' / folder).glob('*.png'):
        if path.relative_to(root).as_posix() not in known:
            errors.append('Nombre fuera de contrato: ' + path.name)
with (out / 'inventario.tsv').open('w', encoding='utf-8-sig', newline='') as stream:
    writer = csv.DictWriter(stream, fieldnames=list(rows[0]), delimiter='\t')
    writer.writeheader()
    writer.writerows(rows)
present = [row for row in rows if row['estado'] != 'pendiente']
counts = Counter(row['tipo'] for row in present)
lines = [
    'VALIDACION DE ENTREGA - Sal y Almas: la Odisea',
    'Fecha local: ' + datetime.now().astimezone().isoformat(timespec='seconds'),
    f'Contrato total: {len(rows)} archivos (82 fondos, 66 enemigos).',
    f'Archivos presentes: {len(present)} ({counts["ambiente"]} fondos, {counts["enemigo"]} enemigos).',
    f'Faltan por generar: {len(rows)-len(present)}.',
    f'Presentes provisionales: {sum(row["estado"].startswith("provisional") for row in rows)}.',
    f'Errores tecnicos: {len(errors)}.',
    'Controles: lectura PNG completa, nombres, dimensiones, RGBA, alfa real, margen >= 6 %, fuentes, prompts, SHA-256 sin duplicados.',
    'Orientacion, tono y composicion: revision visual del agente en imagenes y hojas de contacto; no se infieren con este script.',
    'No se ha abierto Unity ni verificado la integracion en combate.',
    '',
    'ERRORES:', *errors,
    '', 'PROVISIONALES:', *[row['id'] for row in rows if row['estado'].startswith('provisional')],
    '', 'PENDIENTES:', *[row['id'] for row in rows if row['estado'] == 'pendiente'],
]
(out / 'validacion.txt').write_text('\n'.join(lines) + '\n', encoding='utf-8')
print('\n'.join(lines[:10]))
if errors:
    print('\n'.join(errors))
raise SystemExit(1 if errors else 0)
