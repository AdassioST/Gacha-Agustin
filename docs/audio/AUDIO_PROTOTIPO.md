# Audio del prototipo

Cómo suenan la música, el combate y el hub de la Fase 2. La integración sigue siendo **temporal**, preparada para migrar a FMOD según [INTEGRACION_AUDIO.md](INTEGRACION_AUDIO.md), sin cambiar las reglas del núcleo.

## Teclas

| Tecla | Qué hace |
| --- | --- |
| **M** | Silencia o activa la música |
| **N** | Silencia o activa los efectos |

Las dos se recuerdan entre sesiones y no responden mientras escribes en un campo de texto, como el de la semilla.

## Música

- **Pistas:** las de Suno, en `Assets/SalYAlmas/Audio/Resources/MusicaTemporal/`.
- **Qué suena en cada pantalla:** lo decide `musica_temporal.json`. La asignación vigente está en [TOMAS_LOTE_2.md](TOMAS_LOTE_2.md).
- **Código:** `TempMusicPlayer` (`Assets/SalYAlmas/Presentation/`). Hace fundidos de potencia constante de 1,5 s, alterna pistas al azar y baja el volumen con la Cadena abierta. Spark, Ascensiones y stingers de tirada piden espacio a la música, con entrada rápida y recuperación suave. La victoria y la derrota suenan una vez.
- **Continuidad:** hub y formación comparten `Menu`, con las mismas tres canciones y sin reiniciar la pista al cambiar de pantalla o pestaña. M silencia inmediatamente las dos fuentes y conserva la posición.

## Efectos

- **Archivos:** 470 WAV de 165 efectos, 29,6 MB, en `Assets/SalYAlmas/Audio/Resources/SfxTemporal/`. Los genera `Tools/Audio/sfx/` con síntesis propia y foley CC0 de Kenney ([README del generador](../../Tools/Audio/sfx/README.md)). Suno no se usa para efectos.
- **Código:** `TempSfxPlayer` (`Assets/SalYAlmas/Presentation/`). Escucha `BattleDirector.EventPresented`, que se dispara cuando un evento **se muestra** en pantalla. Así el golpe suena cuando se ve, también en la Cadena, donde cada golpe cae en su frame.
- **Nombres:** el reproductor busca la variante más específica que exista (`id__valor__valor`, luego `id__valor`, luego `id`) y elige al azar entre sus variaciones `__vN`. Si cambias o agregas WAV, el índice `sfx_indice.json` se reescribe solo al correr el generador.
- **Importación:** `TempAudioImport` (Editor) conserva los 48 kHz e importa los efectos en PCM, descomprimidos al cargar. Se evita ADPCM en notas, campanas y colas suaves. La música sigue en streaming Vorbis. El catálogo se precarga al iniciar el reproductor para evitar cargas en el frame de un golpe.

### Mezcla y prioridades

`SfxVoicePool` limita la mezcla a 16 voces activas y cuatro salidas de 20 ms para retirar colas sin cortes bruscos. Cada variante comparte el límite de repetición de su familia; la interfaz tiene un intervalo mínimo de 80 ms y dos voces por familia. Las otras familias admiten tres. Las firmas de Spark, Partitura, Ascensión y tirada tienen prioridad sobre golpes, detalles e interfaz. No se modifica el pitch de las notas.

N detiene inmediatamente las voces, cancela las colas programadas y silencia los ambientes. Al reactivar, los bucles conservan su contexto. La cristalización sale con un fundido cuando la bruja cae, cambia de banda o termina el combate; al cambiar de combate se limpian los sonidos pendientes. Las variaciones evitan repetir el mismo archivo consecutivamente y usan un generador de azar separado del de Unity y del núcleo.

Niveles iniciales: música 0,55 y efectos 0,75; dentro de efectos, firmas 0,90, acciones 0,78, detalles 0,58 e interfaz 0,48. Son ganancias multiplicativas, no valores de sonoridad del archivo. Los límites de voces y estas ganancias dejan espacio a la suma; cualquier cambio debe comprobarse grabando la salida completa.

### Hub

Las pestañas y el Libro tienen sonidos propios. Las tiradas reproducen el vapor y el stinger de la rareza más alta **cuando aparecen los resultados**; diez resultados no disparan diez stingers juntos. No se adelanta información de rareza. Forja, reforja, refinado, ingredientes, salmuera y receta nueva responden al resultado de la operación. El Fresno conecta ruta, nodo, zona, ofrenda, injerto, mutación, compra, descanso y entrenamiento. Una operación fallida usa la señal de error.

Caldero, Termas y las cuatro zonas del Fresno tienen lechos con entrada y salida gradual. Estos siete bucles se ampliaron a 3,4 s útiles dentro del presupuesto de 30 MB. Siguen siendo ambientes provisionales; los ambientes largos y estéreo del plan necesitan un presupuesto de streaming separado.

### Banco de escucha y grabación

En Unity, abre **Sal y Almas > Audio > Banco de escucha**, entra en Play y filtra por nombre para escuchar cualquiera de los 470 archivos a través de la mezcla real. Los controles de volumen de la ventana duran esa sesión de Play. **Sal y Almas > Audio > Grabar salida (15 s)** guarda `Logs/audio-review.wav`, con un informe de pico y muestras saturadas en consola. La captura no altera la señal ni se incluye en builds. `Logs/` no se versiona.

Para regenerar los siete lechos: `python -I Tools/Audio/sfx/sfx.py generar sfx_ambiente_raices sfx_ambiente_tronco sfx_ambiente_ramas sfx_ambiente_copa sfx_ambiente_termas_dia sfx_ambiente_termas_noche sfx_taller_caldero --sin-revision`. Para comprobar el catálogo entero: `python -I Tools/Audio/sfx/sfx.py medir --sin-revision`. Si ffmpeg falla o no devuelve las medidas, la revisión falla; no aprueba datos incompletos.

### Qué evento dispara cada efecto

| Evento | Efecto |
| --- | --- |
| Empieza el combate | `sfx_combate_entrada` |
| Turno | `sfx_combate_turno` (aliada o enemiga); si la bruja está en Cristalización Turbia, `sfx_salinidad_perdida_vida` |
| Ataque básico | `sfx_combate_ataque__natural` y, al pegar, `sfx_combate_golpe` (ligero, medio o pesado según el daño contra la vida máxima) |
| Hechizo | Ritual de la bruja (largo la primera vez del combate, corto después), crujido de sal y aparición del Ascendente por su signo, canalización y golpe astral del planeta dominante |
| Autohechizo, Vislumbre | `sfx_magia_autohechizo` (su forma; el Frenesí suma Sal Negra), `sfx_magia_vislumbre` |
| Poción | Cinturón, destapar (según la base), beber y el efecto (cura, escudo, velocidad, retraso, salinidad o Ánima) |
| Cadena | Abrir y cerrar la pantalla, confirmar, cálculo, un eslabón por papel, una nota de la Partitura por golpe y la campana del Spark, que sube con cada Spark de la misma Cadena; fin según cuántos Spark hubo |
| Crítico, escudo, daño a una aliada | `sfx_combate_critico`, `sfx_combate_escudo` (poner o romper), `sfx_combate_dano_recibido` según la especie |
| Aspectos y Reacciones | Un intervalo por Aspecto; un sonido por Reacción Híbrida |
| Quiebre | Rotura de Firmeza y el efecto del planeta |
| Salinidad | Un sonido por banda al subir, otro al bajar, otro al salir del Éxtasis y un bucle mientras alguien esté en Cristalización |
| Granos, Resonancia y Ánima | Granos que suben o bajan, Resonancia llena y carga de cada nivel de Ascensión |
| Ascensión | Lanzar, la técnica por planeta (la III de Marisal tiene la suya) e impacto |
| Jefe | Aviso de carga y cambio de fase |
| Caída | `sfx_combate_caida`; si es enemiga, almitas que se sueltan |
| Fin del combate | `sfx_combate_victoria` o `sfx_combate_derrota` |
| Cualquier botón | `sfx_ui_tocar` |

## Límites conocidos

- **IDs de las 3★:** los datos de la Fase 1 todavía dicen `zarigueya` y `topo`. El reproductor los traduce a `muertita` y `topacia` hasta que la Fase 1.5 los renombre (HUECOS C-26).
- **Lo que falta:** los eventos de animaciones que aún no existen (por ejemplo, la secuencia completa de una tirada) conservan sus archivos sin inventar temporizadores que desincronicen sonido e imagen. El inventario de armas no expone todavía todas sus propiedades como eventos de presentación en combate.
- **Validación:** importación PCM de los 470 clips comprobada en Unity 6000.6.7f1, Windows estéreo a 48 kHz; medición ffmpeg del catálogo completo, regeneración idéntica de los siete lechos y 255 pruebas del Editor aprobadas. Quince comprobaciones adicionales en Play cubren rarezas, silencios, colas pendientes, ambientes y prioridades. Captura de 45 s de combate automático a velocidad ×2: 599 eventos presentados, 134 golpes, nueve eslabones de Cadena y cuatro Ascensiones; −17,0 LUFS integrados, pico real −1,6 dBTP y cero muestras saturadas. Es una muestra de integración, no una garantía para cualquier combinación posible. El build Windows x64 se genera sin errores ni advertencias y los `.meta` se generan con Unity.
- **Escucha:** las medidas no sustituyen una sesión de escucha humana con auriculares y altavoces. El banco de escucha y la grabación permiten aprobar timbre, fatiga y balance final; no se considera realizado ese control subjetivo.

Referencia de importación: [Unity, Audio Clip](https://docs.unity.com/en-us/engine/6000.3/manual/audio/reference/class-audio-clip).
