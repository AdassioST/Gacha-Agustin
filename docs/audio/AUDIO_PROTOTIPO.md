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
- **Santuario:** el nodo de Santuario del Fresno activa `FresnoSantuario` mientras está abierta su decisión y vuelve al contexto del hub al salir.

## Efectos

- **Archivos:** 470 WAV de 165 efectos, 29,6 MB, en `Assets/SalYAlmas/Audio/Resources/SfxTemporal/`. Los genera `Tools/Audio/sfx/` con síntesis propia y foley CC0 de Kenney ([README del generador](../../Tools/Audio/sfx/README.md)). Suno no se usa para efectos.
- **Interfaz uGUI:** otros 16 WAV de síntesis propia en `Assets/SalYAlmas/Art/Ui/Resources/Ui/sfx/`, generados por `scripts/ui_sfx.py`. Su procedencia y SHA-256 se registran en `Audio/Licencias/ui/registro.csv`. El límite de 30 MB anterior corresponde al banco de 470; estos 16 archivos son adicionales.
- **Código:** `TempSfxPlayer` (`Assets/SalYAlmas/Presentation/`). Escucha `BattleDirector.EventPresented`, que se dispara cuando un evento **se muestra** en pantalla. Así el golpe suena cuando se ve, también en la Cadena, donde cada golpe cae en su frame.
- **Nombres:** el reproductor busca la variante más específica que exista (`id__valor__valor`, luego `id__valor`, luego `id`) y elige al azar entre sus variaciones `__vN`. Si cambias o agregas WAV, el índice `sfx_indice.json` se reescribe solo al correr el generador.
- **Importación:** `TempAudioImport` (Editor) conserva la frecuencia original (48 kHz del banco y 44,1 kHz de uGUI) e importa los 486 efectos en PCM, descomprimidos al cargar. Se evita compresión con pérdida en notas, campanas y colas suaves. La música sigue en streaming Vorbis. El reproductor carga explícitamente los datos de audio al iniciar para evitar cargas en el frame de un golpe.

### Mezcla y prioridades

`SfxVoicePool` limita la mezcla a 16 voces activas y cuatro salidas de 20 ms para retirar colas sin cortes bruscos. El combate y ambas interfaces comparten este reproductor; uGUI ya no acumula `PlayOneShot` en fuentes independientes. Cada variante comparte el límite de repetición de su familia; la interfaz tiene un intervalo mínimo de 80 ms y dos voces por familia. Las otras familias admiten tres. Las firmas de Spark, Partitura, Ascensión y tirada tienen prioridad sobre golpes, detalles e interfaz. No se modifica el pitch de las notas; cada gota y estrella conserva el suyo durante toda su voz.

N detiene inmediatamente las voces, cancela las colas programadas y silencia los ambientes. Al reactivar, los bucles conservan su contexto. La cristalización sale con un fundido cuando la bruja cae, cambia de banda o termina el combate; al cambiar de combate se limpian los sonidos pendientes. Las variaciones evitan repetir el mismo archivo consecutivamente y usan un generador de azar separado del de Unity y del núcleo.

Niveles iniciales: música 0,55 y efectos 0,65; dentro de efectos, firmas 0,90, acciones 0,78, detalles 0,58 e interfaz 0,48. uGUI aplica además su ganancia de 0,80. Son ganancias multiplicativas, no valores de sonoridad del archivo. Los límites de voces y estas ganancias dejan espacio a la suma; cualquier cambio debe comprobarse midiendo la salida completa.

### Hub

Las pestañas y el Libro tienen sonidos propios. Las tiradas reproducen el vapor y el stinger de la rareza más alta **cuando aparecen los resultados**; diez resultados no disparan diez stingers juntos. No se adelanta información de rareza. Forja, reforja, refinado, ingredientes, salmuera y receta nueva responden al resultado de la operación. El Fresno conecta ruta, nodo, zona, ofrenda, injerto, mutación, compra, descanso y entrenamiento. Una operación fallida usa la señal de error.

El Aquelarre y el tablero de encargos usan las tres variantes de papel al desplazarse con gesto o rueda; los reinicios de posición al navegar o filtrar quedan mudos. En uGUI, la tirada de 3★ también tiene firma. Al saltar se conserva la rareza correcta y las tarjetas usan acentos breves, sin repetir acordes completos. Cambiar de revelado cancela sus sonidos pendientes; cerrar o repetir retira las colas con 80 ms de salida. Las firmas de tirada piden espacio a la música a través del mismo sistema de ducking del combate.

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
- **Lo que falta:** los once efectos de pantallas o interacciones aún no implementadas conservan su carácter provisional. El inventario de armas no expone todavía todas sus propiedades como eventos de presentación en combate. La secuencia de tirada uGUI sí tiene sus eventos conectados.
- **Validación:** Unity 6000.6.7f1, Windows estéreo a 48 kHz; importación PCM de los 486 efectos, 328 pruebas del Editor aprobadas y 19 comprobaciones de integración en Play. El build Windows x64 termina sin errores; conserva 13 advertencias previas de obsolescencia, serialización y una variable sin uso, ajenas al audio. Trece comprobaciones adicionales en el ejecutable verifican inclusión, decodificación y reproducción de los bancos, música en streaming, scroll, firma de 3★ y silencio compartido. Los `.meta` se generan con Unity.
- **Escucha:** las medidas no sustituyen una sesión de escucha humana con auriculares y altavoces. El banco de escucha y la grabación permiten aprobar timbre, fatiga y balance final; no se considera realizado ese control subjetivo.

Referencia de importación: [Unity, Audio Clip](https://docs.unity.com/en-us/engine/6000.3/manual/audio/reference/class-audio-clip).

## Pulido a partir de la captura completa (2026-11-05)

La captura original de 45:13 conserva el estado anterior como referencia. Se corrigieron el scroll mudo del Aquelarre, la firma de 3★ sin disparador, la selección de música del Santuario, el reproductor de interfaz separado y sus colas al silenciar. El inicio de `drop` y `page` ahora parte de cero; el riser tiene una salida de 100 ms y sus últimos 10 ms caen de −18,0 a −52,5 dBFS RMS. La regeneración de los tres WAV produce archivos idénticos. Los otros 483 efectos conservan sus muestras.

Se protegieron los callbacks de nombre, estrellas y avance contra saltos, cierre y repetición de la tirada. También se restauró la textura de render activa tras simular el agua para evitar la advertencia al cerrar resultados.

La verificación interna de 32 s combinó ráfagas de interfaz y combate automático contra el guardián: 269 eventos presentados, −18,93 LUFS integrados y −2,85 dBTP, sin saturación. Pasaron 19 comprobaciones en Play sobre gesto frente a movimiento programático, silencio inmediato, pitch independiente, rareza, cancelación, colas, prioridad, límite de voces, Santuario y avance de frames. Se usó exclusivamente un perfil en memoria. Esta prueba breve no sustituye la escucha humana. El perfil persistente del jugador conserva su SHA-256 anterior a las pruebas.
