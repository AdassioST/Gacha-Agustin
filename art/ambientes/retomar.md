# Continuación del encargo

Estado al 5 de noviembre de 2026: 104/148 PNG presentes, con dos fondos provisionales; quedan 44 archivos nuevos y dos correcciones. Ver [reporte](reporte.md), [inventario](inventario.tsv) y [validación](validacion.txt).

El generador integrado devolvió 429 / usage_limit_reached y anunció restablecimiento el 6 de noviembre a las 03:11:26 UTC, equivalente al **5 de noviembre a las 21:11:26 en Ciudad de México**. No se configuró reanudación automática. No repetir solicitudes mientras siga vigente el límite.

## Restricciones para continuar

- Trabajar en `C:\Arcanoria Master\Gusi Gaming\gacha-fase3`. No usar git, no hacer commits, no editar código, JSON, metadatos ni otras carpetas.
- Releer `docs/Ambientes para Codex.md` y el reporte; mantener nombres contractuales y GDD 6.5.
- Usar image_gen integrado, una llamada por pieza. La alternativa CLI/API exige elección explícita del usuario y `OPENAI_API_KEY`; no cambiar de vía silenciosamente.
- Antes de editar un archivo local, abrirlo con view_image. Para capas Grieta/Abismo, el fondo base de la misma región y zona es el objetivo de edición. Marisal es referencia de pintura, nunca contenido que copiar.
- Los prompts completos pendientes están en `art/ambientes/pendientes/` y `art/enemigos/pendientes/`. Los detalles y mecánicas de prioridad 2 están en `art/enemigos/<region>_diseno.md`. El prompt pendiente no prueba generación.

## Orden pendiente

1. `amb_leo_subsuelo` y `amb_virgo_subsuelo`: usar los prompts r1 preparados. Ambas escenas deben tener techo continuo, estar claramente enterradas y ser más oscuras/estrechas que superficie. Conservar los originales; sustituir el PNG del juego solo tras revisar la corrección.
2. Sagitario: tres fondos restantes y cinco enemigos (8).
3. Acuario: cuatro fondos y cinco enemigos (9).
4. Piscis: cuatro fondos y cinco enemigos (9).
5. Corazón de Capricornio Grieta/Abismo (2), después subsuelo y cielo de las cuatro cardinales (16).

## Guardado y revisión

Conservar la imagen original sin tocar en `art/ambientes/fuentes/` o `art/enemigos/fuentes/`. Para exportar, ejecutar desde la raíz del worktree:

```powershell
& '.\art\ambientes\exportar.ps1' -Source '<ruta absoluta devuelta por image_gen>' -Id '<id>'
& '.\art\ambientes\exportar.ps1' -Source '<ruta absoluta devuelta por image_gen>' -Id '<id>' -Enemy
```

Son ejemplos alternativos según tipo de archivo. Para una revisión aceptada de una pieza existente, añadir `-Revision r1` (o el siguiente sufijo libre). El exportador rechaza sobreescritura sin revisión y conserva los originales. Guardar también el prompt exacto en `fuentes/<id>_prompt.md` o `fuentes/<id>_r1_prompt.md`.

Revisar fondo sin texto ni personajes cercanos, tercio inferior tranquilo y progresión de profundidad; enemigo con cuerpo entero, anatomía coherente, dirección izquierda, silueta legible y alfa real. No aceptar humanoides cuando el sujeto es un objeto o animal: los prompts correctivos deben empezar por el sujeto literal.

Al terminar cada región, crear su hoja y actualizar el reporte:

```powershell
& '.\art\ambientes\hoja.ps1' -Region '<region>'
& '.\art\ambientes\hoja.ps1' -Region '<region>' -Layer 'grieta'
& '.\art\ambientes\hoja.ps1' -Region '<region>' -Layer 'abismo'
python '.\art\ambientes\revisar.py'
```

Cuando se acepten los subsuelos corregidos, quitar sus identificadores del conjunto `provisional` en `art/ambientes/revisar.py` antes de regenerar el inventario. Este script solo lee los PNG y escribe reportes de arte, no modifica imágenes.

La integración, importación y comprobación dentro de Unity corresponden a Claude Code. Mantener los nombres exactos: no añadir sufijos r1 a los archivos finales cargados por el juego.

## Archivos nuevos faltantes (44)

### Prioridad 2

- `amb_sagitario_subsuelo.png`
- `amb_sagitario_corazon.png`
- `amb_sagitario_cielo.png`
- `enemigo_cria_estrella.png`
- `enemigo_corcel_meteorico.png`
- `enemigo_arquera_fieltro.png`
- `enemigo_gigante_ojo_unico.png`
- `enemigo_centauro_horizonte_roto.png`
- `amb_acuario_orilla.png`
- `amb_acuario_subsuelo.png`
- `amb_acuario_corazon.png`
- `amb_acuario_cielo.png`
- `enemigo_cantora_sumergida.png`
- `enemigo_foca_escarcha.png`
- `enemigo_farol_iceberg.png`
- `enemigo_senora_agua_quieta.png`
- `enemigo_ballena_aurora.png`
- `amb_piscis_orilla.png`
- `amb_piscis_subsuelo.png`
- `amb_piscis_corazon.png`
- `amb_piscis_cielo.png`
- `enemigo_ninfa_hospitalaria.png`
- `enemigo_pez_micelio.png`
- `enemigo_anfora_esporas.png`
- `enemigo_anfitriona_sin_despedida.png`
- `enemigo_sueno_dos_peces.png`

### Prioridad 3

- `amb_aries_subsuelo_grieta.png`
- `amb_aries_subsuelo_abismo.png`
- `amb_aries_cielo_grieta.png`
- `amb_aries_cielo_abismo.png`
- `amb_cancer_subsuelo_grieta.png`
- `amb_cancer_subsuelo_abismo.png`
- `amb_cancer_cielo_grieta.png`
- `amb_cancer_cielo_abismo.png`
- `amb_libra_subsuelo_grieta.png`
- `amb_libra_subsuelo_abismo.png`
- `amb_libra_cielo_grieta.png`
- `amb_libra_cielo_abismo.png`
- `amb_capricornio_corazon_grieta.png`
- `amb_capricornio_corazon_abismo.png`
- `amb_capricornio_subsuelo_grieta.png`
- `amb_capricornio_subsuelo_abismo.png`
- `amb_capricornio_cielo_grieta.png`
- `amb_capricornio_cielo_abismo.png`
