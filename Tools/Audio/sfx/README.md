# Generador de efectos de sonido

Genera todos los efectos de sonido temporales de *Sal y Almas* como WAV, de forma reproducible. Sigue el catálogo de la sección 3 de [INTEGRACION_AUDIO.md](../../../docs/audio/INTEGRACION_AUDIO.md). Todo sale de síntesis propia, combinada en algunos sonidos con foley CC0 de Kenney. No usa generadores de audio con IA.

| Qué | Dónde |
| --- | --- |
| WAV generados | `Assets/SalYAlmas/Audio/Resources/SfxTemporal/` |
| Catálogo medido (se reescribe solo) | [catalogo.md](catalogo.md) |
| Hojas de espectrogramas por sistema | [revision/](revision/) |
| Registro de cada WAV (método, fuentes, licencia, estado) | `Audio/Licencias/sfx/registro.csv` |
| Fuentes de bibliotecas y su licencia | `Audio/Licencias/sfx/fuentes.csv` y [fuentes/](fuentes/) |

## Cómo correrlo

Requisitos: Python 3.10 o posterior con `numpy`, `scipy` y `soundfile`; `ffmpeg` en el `PATH` para medir; `Pillow` para las hojas de revisión.

```bash
pip install numpy scipy soundfile pillow

# Todo: WAV, medidas, catálogo, registro y hojas de revisión (unos 30 s con 4 núcleos)
python -I Tools/Audio/sfx/sfx.py generar

# Uno o varios IDs (el catálogo y el registro se reescriben con todo lo que hay)
python -I Tools/Audio/sfx/sfx.py generar sfx_cadena_spark sfx_combate_golpe

# Otras acciones
python -I Tools/Audio/sfx/sfx.py lista            # IDs, nivel, número de archivos
python -I Tools/Audio/sfx/sfx.py medir            # vuelve a medir con ffmpeg y reescribe los documentos
python -I Tools/Audio/sfx/sfx.py revisar combate  # hojas de espectrogramas de uno o más sistemas
python -I Tools/Audio/sfx/sfx.py huella           # huella SHA-256 de todos los WAV
```

- `-I` aísla a Python del entorno; el script agrega su propia carpeta para encontrar sus módulos.
- `--procesos N` cambia el número de procesos. `--salida DIR` escribe en otra carpeta. `--sin-revision` no dibuja las hojas.
- Una regeneración completa borra de la carpeta de salida los WAV que ya no tienen receta.
- **Determinismo:** cada archivo usa una semilla fija, derivada de su nombre (`crc32("salyalmas-sfx|<archivo>")`). Dos corridas dan los mismos bytes, con cualquier número de procesos. Compruébalo con `huella`.

## Archivos del generador

| Archivo | Qué hace |
| --- | --- |
| `sfx.py` | Línea de comandos: genera en paralelo, mide con ffmpeg, escribe catálogo y registro |
| `sintesis.py` | Bloques de síntesis reutilizables (abajo) y carga de fuentes CC0 |
| `proceso.py` | Acabado de cada archivo: paso alto, recorte, fundidos, sonoridad, limitador de pico real, revisión automática, medición con ffmpeg |
| `revision.py` | Hojas PNG de espectrograma y forma de onda |
| `recetas/` | Una receta por ID, agrupadas por sistema (`combate.py`, `cadena.py`, `magia.py`…). `__init__.py` tiene el registro, los topes de duración y las reglas de nombres; `comun.py`, las piezas que comparten varios sistemas |

## Nombres y formato

- `<sfx_id>.wav` si el ID tiene un solo archivo.
- Las variantes por parámetro llevan `__<valor>` con los valores de la sección 3.1: `__marte`, `__pesado`, `__apertura`, `__3`. Las variaciones aleatorias llevan `__v1`, `__v2`, `__v3`, solo cuando hay más de una con los mismos parámetros. Ejemplos: `sfx_combate_golpe__pesado__v2.wav`, `sfx_quiebre_marte.wav`, `sfx_sal_crujido__luna__v1.wav`.
- WAV de 48 kHz y 16 bits. Todo es mono para caber en el tope de 30 MB; FMOD pone el paneo, la cola estéreo y la reverb de cada lugar.
- Sin clics: paso alto de 25 Hz (18 Hz en el grave del horror), fundido de coseno de 1.5 ms al inicio y de 15 a 150 ms al final. Los bucles se funden en potencia constante y se procesan en tres copias seguidas, así que la costura no salta.

## Sonoridad

El plan (5.7) da el máximo de corto plazo de cada mezcla: −12 LUFS en combate, −16 en Termas y menús, −14 en horror (siempre bajo el combate) y −10 en la Ascensión III. Como los efectos duran menos de 3 s, el generador ajusta el **máximo momentáneo** (ventana de 400 ms, BS.1770) de cada archivo a un objetivo por familia. Así el prototipo, que todavía no tiene FMOD, ya suena balanceado. En FMOD se pueden normalizar a −3 dBFS de pico y balancear contra la mezcla de referencia, como dice el plan.

| Familia | Objetivo (M máx, LUFS) | Ejemplos |
| --- | --- | --- |
| Lectura de combate (P1) | −13 a −15 | Spark, Quiebre de Firmeza, Reacciones, Éxtasis, Ascensiones |
| Golpes | −14 pesado, −16.5 medio, −19 ligero; capas de material y arma a −20 | `sfx_combate_golpe` |
| Magia | −16 a −20 | Golpe Astral, crujido, rituales, canalizar |
| Partitura | −22 | Notas (el plan pide la capa unos 14 dB bajo los golpes; aquí queda unos 6 dB abajo para el prototipo sin música) |
| Interfaz | −21 a −28 | Botones, Libro Mayor, deslizar |
| Termas y tirada | −17 a −26 | Vapor, emergencia, tortuga; stingers a −16 y −17 |
| Horror | −18 a −24 | Nunca más fuerte que el combate |
| Bucles | −28 a −32 LUFS integrados | Lechos, caldero, termómetro, grave del horror |

- **Pico real:** techo de −1.3 dBTP medido con sobremuestreo ×4. Un limitador con 1.5 ms de anticipación puede quitar hasta 6 dB (9 dB en sonidos de granos y chasquidos) antes de bajar la ganancia. Nunca recorta.
- Los clics muy cortos (`sfx_ui_tocar`, `sfx_combate_velocidad`) quedan unos dB bajo su objetivo: en 400 ms de ventana un clic de 50 ms siempre mide poco, y subirlo más solo lo aplastaría.
- **Medición final:** `ffmpeg -af apad=pad_dur=0.5,ebur128=peak=true`. El catálogo da el máximo momentáneo (o la integrada en bucles) y el pico real más alto de cada ID. La generación falla si un archivo pasa de −1 dBTP o si el total pasa de 30 MB.

## Diseño por familia

La casa es Re (SOUNDTRACK_SUNO.md 3.7): el combate, la Partitura y el Éxtasis están en Re menor; la tirada resuelve en Re mayor. Campanas, notas y stingers están afinados a esa escala con temperamento igual y La = 440 Hz.

### Metales por planeta (`sintesis.metal`, `PERFILES`)

Síntesis modal: cada metal es una suma de parciales con su proporción, su amplitud y su caída, más un transitorio de golpe filtrado. El mismo timbre aparece en el Golpe Astral, el Quiebre, la cola del ritual y la nota de la Partitura (plan 3.3).

| Planeta | Metal | Cómo se arma | Grado en Re menor |
| --- | --- | --- | --- |
| Luna | Plata | Casi senoidal, parciales 1, 2, 3 y 4.16 muy débiles, dobletes que laten, cola larga; copa frotada | I: Re |
| Mercurio | Azogue | Senoidal que sube 3.5 % al empezar (líquida), vibrato de 7 Hz, una gota encima | II: Mi |
| Venus | Cobre | Cuenco: parciales 1, 2.71, 5.15 y 8.17 en dobletes que laten, mazo blando | III: Fa |
| Sol | Oro | Campanilla de latón: parciales 1, 2, 2.76, 3, 4.07…, golpe brillante, chisporroteo | IV: Sol |
| Marte | Hierro | Yunque: parciales de barra libre (1, 2.76, 5.40, 8.93), caída corta, golpe seco; óxido raspado | V: La |
| Júpiter | Estaño | Gong medio denso más el "grito del estaño" (ráfagas de microcrujidos) y estática | VI: Si♭ |
| Saturno | Plomo | Parciales que mueren en 0.3 s, paso bajo a 1.8 kHz, tic de reloj | VII: Do |

La escalera de pureza (Turbia, Destilada, Quintaesencia) sigue la tabla 3.3: Turbia es ruido y grano; Destilada, tono con textura; Quintaesencia, tono puro con aura (acordes, ecos, coro suave).

### La sal (`crujido_sal`, `fractura`, `_granos`)

- **Fractura:** un transitorio de banda ancha, cuatro resonancias de cristal al azar entre 1.8 y 6.5 kHz y un cuerpo breve de 220 a 420 Hz.
- **Granos:** microchasquidos con densidad que cae, amplitud lognormal y algunos con una resonancia aguda de 2 a 7 ms. Ningún grano nace si no alcanza a terminar.
- **Pureza:** con 0.70 domina el polvo y la fractura sucia; con 0.85 pesan igual fractura y tono; desde 0.97 entra un armónico limpio de plata.
- **Lento:** `grave=1` o `2` baja el crujido una o dos octavas por remuestreo, para la Cristalización Turbia y los Cristalizados (plan 3.2).
- Se mezcla con un vidrio ligero de Kenney a −14 dB para darle cuerpo de cristal real.

### La Partitura y el Spark

- **Notas** (`sfx_partitura_nota__<planeta>__<papel>`): el grado del planeta en Re menor, 150 ms y una caída de 180 ms. Apertura en la octava de Re4, Enlace en Re5 y Cierre en Re6 con la tercera y la quinta del grado, dentro de la escala.
- **Spark** (`sfx_cadena_spark__1` a `__5`): campana de oro con un brillo de plata a la octava. Sigue la regla 3.4 del plan: tónica y notas del acorde (1.ª, 3.ª, 5.ª, 8.ª y 10.ª), o sea Re6, Fa6, La6, Re7 y Fa7. Es aguda y corta; la Campana de Bronce (arma) va en Re3 con una onda diferida, para que nunca se confundan.
- **Aspectos** (intervalos en Re, con salterio y plata): Conjunción, unísono que late y se funde; Oposición, octava que se desafina y cruje; Sextil, tercera mayor rápida; Trígono, quinta justa en cuencos y un grano; Cuadratura, segunda menor en hierro.

### Otras familias

| Familia | Bloque | Cómo se arma |
| --- | --- | --- |
| Cuerdas pulsadas | `cuerda`, `rasgueo` | Modos de cuerda con inharmonicidad, peine por posición de pulsación y caída por parcial. Salterio (acero, órdenes dobles que laten), laúd y guitarra de nailon con resonancias de caja |
| Violonchelo grave | `arco` | Armónicos con formantes de caja, vibrato tardío y ruido de arco (solo como raíz en los stingers) |
| Cuencos y cristal | `cuenco`, `copa_frotada` | Golpeado o frotado; el cuenco "gime" con roce áspero y altura inestable para la Sal Negra |
| Papel y pergamino | `papel`, `hoja` | Granos de ruido de banda ancha; la vuelta de hoja suma un barrido y un aletazo. Mezclado con libros de Kenney |
| Pluma | `pluma` | Tren de trabas (*stick-slip*) de 250 a 700 por segundo, resonancia de la punta y el punto final |
| Agua y vapor | `burbuja`, `burbujeo`, `gota`, `chapoteo`, `escurrir`, `vapor`, `hervor` | Burbujas de Minnaert con subida de tono (modelo de Van den Doel); el vapor es siseo agudo con respiración lenta |
| Golpes y cuerpos | `golpe_sordo`, `madera`, `piedra`, `pelaje`, `plumas`, `aleteo` | Cuerpo grave que cae de tono con saturación suave (armónicos de 100 a 300 Hz para la bocina del teléfono), más golpes y telas de Kenney |
| Viento | `viento`, `whoosh` | Ruido rosa por un paso banda que deriva o barre |
| Fuego, estaño, trueno | `fuego`, `crepitar`, `grito_estano`, `estatica`, `trueno_lamina` | Rumor con parpadeo y chasquidos; lámina de trueno con aleteo |
| Mecanismos | `engranes`, `abaco`, `tic_reloj`, `cadena_hierro`, `raspado`, `corcho` | Tics de latón, cuentas de madera, eslabones, óxido |
| Susurros | `susurro` | Ruido rosa por tres formantes que saltan entre vocales al azar, con anchos de susurro (200 a 380 Hz). No hay voz con tono ni palabras; al revés para el Salmódico invertido. Nada de respiraciones ni sonidos sugerentes (GDD 5) |
| Horror | `dron`, `pulsar`, `onda_corta` | Pedal de 37 Hz con armónicos de 80 a 200 Hz; clics lentos de púlsar y tonos de onda corta que derivan despacio (radiotelescopio, nunca láser). Todo entra con fundido: sin sustos (GDD 6.5) |
| Sala | `sala` | Convolución con una respuesta sintética de capilla pequeña de piedra, la misma idea de la música. Máximo 0.8 s: la reverb real la pone FMOD |

### Stingers de la tirada

`mus_tirada_vapor_blanco`, `_violeta`, `_dorado` y `mus_tirada_subida_dorada` siguen la ficha P05 de SOUNDTRACK_SUNO.md: Re y 80 BPM, con los acordes de la ficha (Re menor; Si♭ y Fa mayor; Re mayor; Si♭ que modula a Re mayor por La). Se hacen con guitarra, salterio, campanas, cuencos y violonchelo sintetizados. El coro sin palabras de la ficha se cambia por cuencos frotados: aquí no hay voces.

## Fuentes de bibliotecas

La red de la sesión bloqueó kenney.nl, OpenGameArt, Freesound, Sonniss, archive.org y Wikimedia. GitHub sí respondió, y ahí hay espejos de dos paquetes de Kenney:

| Paquete | Espejo | Licencia |
| --- | --- | --- |
| RPG Audio (libros, telas, cuero, cerrojos, cuchillos, crujidos) | `Boyquotes/kenney-rpg-audio-for-godot`, commit `22eb79b` | CC0 1.0 en el `LICENSE` del repositorio |
| Impact Sounds (vidrio, madera, metal, piedra, golpes, pasos en nieve y pasto) | `Boyquotes/kenney-impact-sounds-for-godot`, commit `999dd16` | CC0 1.0 en el `LICENSE` del repositorio |

Los OGG no traen etiquetas de autor ni de licencia. La licencia de cada archivo es la del repositorio y la que Kenney da a sus paquetes; conviene confirmarla en kenney.nl cuando haya red. Las 50 fuentes que usan las recetas están en `fuentes/`, con la copia de cada licencia, y una por renglón en `fuentes.csv`. Se usan como capas (foley real de papel, tela, vidrio, madera y golpes) y nunca como firma: la sal, los metales y la campana del Spark son síntesis.

## Presupuesto de tamaño

El tope es de 30 MB para todos los WAV. Se cuida con tres perillas en `recetas/__init__.py`:

- `PRESUPUESTO`: tope de duración por ID, pensado desde su estructura (el último golpe más su cola). Pasado el tope, la cola se funde en exponencial.
- `TOPE_POR_NIVEL`: tope para los IDs que no están en la tabla.
- Las variantes de cada receta: los eventos frecuentes (golpes, turnos, notas, crujido, rituales, Partitura, interfaz) tienen variaciones; los raros, una sola.

Además, el recorte final quita la cola por debajo de −62 dB del pico y aplica un fundido de coseno. El total actual está al principio de [catalogo.md](catalogo.md).

## Revisión sin escuchar

1. **Pruebas automáticas** en cada archivo: silencio, DC sostenido, saturación, clic en los bordes, corte brusco (la envolvente cae más de 35 dB en 8 ms después de 30 ms sostenidos) y costura de los bucles. Salen como `aviso` en la consola.
2. **Pico real y tamaño** con ffmpeg: la generación falla si algo se pasa.
3. **Hojas de revisión** (`revision/<sistema>.png`): espectrograma logarítmico de 40 Hz a 20 kHz (72 dB de rango, contra el máximo de cada archivo), forma de onda (rojo si satura), duración, M máx y pico real. Un clic es una línea vertical de banda ancha; un corte, una cola que termina en seco; el DC, una forma de onda desplazada.

Quedan cinco avisos de «cola cortada»: cuatro variantes de `sfx_ritual_solenne` y `sfx_taller_altar__sol`. La inspección de la envolvente sitúa las caídas en huecos internos de los granos/crujidos o en el fundido final; los cinco WAV terminan en cero y los últimos 10 ms están a −88,7 dBFS RMS o menos. Son avisos del detector, no errores de borde. Se conservan para la escucha humana; no se han aprobado subjetivamente.

## Cómo agregar un sonido

1. Busca su sistema en `recetas/` y escribe una función con el decorador:

```python
@receta("sfx_combate_ejemplo", S, "Qué suena, en una frase", "`EventoDelNucleo` (parámetro)",
        var(planeta=PLANETAS_ORDEN, v=[1, 2]), nivel="A", objetivo=-18)
def ejemplo(ctx, v):
    lz = Lienzo(0.8)
    lz.add(N(metal("hierro", f_planeta(v["planeta"], 5), 0.6, ctx.hijo("m"), t60=0.5)), 0, -2)
    lz.add(N(crujido_sal(ctx.hijo("s"), 0.2, 0.85)), 0.02, -6)
    return lz.senal()
```

2. Usa siempre `ctx` para el azar (`ctx.u`, `ctx.i`, `ctx.hijo("etiqueta")`); nunca `random` ni `np.random` sin semilla.
3. `var(...)` arma las variantes: los ejes de parámetro dan `__<valor>` y `v` da `__vN`. Si la única variación es `v`, la función recibe el número; si no, el diccionario.
4. Si dura más de 0.9 s (nivel A) o 0.8 s (nivel B), agrega su tope a `PRESUPUESTO`.
5. Si es de granos o chasquidos, agrégalo a `GRANULARES` para que el limitador trabaje grano por grano.
6. Para usar una fuente nueva de una biblioteca: verifica su licencia (solo CC0 o dominio público), cópiala a `fuentes/<paquete>/`, cárgala con `fuente("paquete/archivo.ogg")` y agrégala a `Audio/Licencias/sfx/fuentes.csv`. El registro anota solo qué fuentes usa cada WAV.
7. Corre `python -I Tools/Audio/sfx/sfx.py generar <id>` y revisa la hoja de su sistema.

## Diferencias con el plan y huecos

| Tema | Plan | Aquí | Por qué |
| --- | --- | --- | --- |
| `PhaseChanged` | Sin sonido | ID nuevo `sfx_combate_jefe_fase` | El prototipo lo emite (fase 2 del Guardián) |
| Cerrar la Partitura | Solo `sfx_cadena_abrir` | ID nuevo `sfx_cadena_cerrar` | La Partitura también se cierra sin confirmar |
| Stingers de vapor | Suno, en estéreo y con coro | Síntesis mono, sin coro | Decisión del usuario; tope de tamaño; sin voces |
| Ritual y crujido | El ritual contiene el crujido como evento anidado | El WAV del ritual es solo el gesto (0.40 s o 0.20 s); el crujido es aparte | Así la cola cambia de metal con la Vía Menor sin duplicar archivos. El prototipo dispara el crujido al terminar el gesto |
| IDs de bruja | El plan y el arte usan `muertita` y `topacia` | Igual | Los datos de la Fase 1 usan `zarigueya` y `topo`: hay que mapearlos |
| Rituales | 24 brujas | 8: las seis Regentes y las dos Aprendizas del prototipo | Las demás no tienen ritual definido (pregunta 12 del plan) |
| Ascensión III | Una por bruja, estéreo | Solo Marisal, mono | Es la 5★ del vertical slice; la III no se alcanza en la prueba de la Fase 1 |
| Ascensión I y II | 1 por signo, capa de planeta | 1 por planeta; la capa de signo es `sfx_magia_ascendente_aparece` | Tamaño |
| Variaciones | Las del catálogo (hasta 4 × 12 × 3 en golpes) | Menos: ver el catálogo | Tope de 30 MB; FMOD alterna las que haya |
| Constelación 5★ | Firma del leitmotiv de cada 5★ | Borrador genérico en Re mayor | Los leitmotivs todavía no existen |
| Lechos y bucles | Estéreo en streaming | Bucles mono de 2 a 2.2 s, en borrador | Tamaño |
| Prefijo `TEMP_` | Para pistas temporales (10.5 y 12) | La carpeta se llama `SfxTemporal` y los archivos no llevan prefijo | Así lo pidió el usuario; la prueba de CI de `TEMP_` debe mirar la carpeta |

## Valores TUNING

Todo número de este generador es una propuesta: objetivos de sonoridad, topes de duración, techo de −1.3 dBTP, umbral de cola de −34 dB, octavas de la Partitura, proporciones y caídas de cada metal, densidades de granos, tempos de las sacudidas por especie. Están juntos en `recetas/__init__.py` (topes), en los decoradores de cada receta (objetivos) y en `PERFILES` de `sintesis.py` (metales).
