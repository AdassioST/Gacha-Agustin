# Luz del combate, órdenes por arrastre, menús más ligeros y escala

Rama `combate/luz-y-gestos`, basada en `main` (5826537), en la carpeta `C:\Arcanoria Master\Gusi Gaming\gacha-ui` (se reutilizó para no reimportar Unity desde cero; `ui/interfaz` ya estaba integrada en `main`). No está en `main`: integrarla lo decide el usuario.

Trabajo de presentación: no cambia reglas, daño, economía ni probabilidades. `Data/loc.json` solo suma textos de interfaz. `Core/` solo suma el dato de tamaño de los enemigos (`EnemyDef.Size`), que lee la presentación y ninguna regla.

## Qué pidió el usuario

1. Un sistema de luz que actúe sobre brujas y enemigos, para que los recortes no se vean pegados encima del escenario. Revisar por qué en Brave Frontier no sobresalen y buscar una solución parecida.
2. Una interfaz de combate más simple: menos toques y órdenes por arrastre intuitivas.
3. Menús más chicos en proporción, con el foco en los personajes, en todas las pantallas.

## Por qué en Brave Frontier no se ven recortados

Se revisaron las cuatro capturas que mandó el usuario. Lo que integra a sus personajes con el fondo:

| Rasgo | Qué hace | Cómo se aplicó aquí |
| --- | --- | --- |
| Misma técnica para fondo y figuras | Todo es pixel art con paletas cercanas: nada brilla más que su entorno | El arte de las brujas es pintado y el fondo también, pero con luz distinta. Se iguala la luz, no la técnica |
| Personajes con la luz de la escena | En la lava todo toma naranja; en la mazmorra, azul | Luz ambiente de cielo y de suelo sacada del fondo real de cada combate |
| Sombras oscuras bajo cada unidad | Pesan sobre el suelo | Sombra de contacto más chica y una sombra proyectada con la silueta |
| Efectos que iluminan | Los ataques encienden a las unidades y al suelo | Luces dinámicas en golpes, hechizos, Quiebres y Ascensiones |
| Escenario despejado | La interfaz vive abajo; el centro es de las unidades | HUD en las orillas, figuras 12 % más grandes |

## Qué se construyó

### Sistema de luz del combate

`Presentation/BattleLighting.cs` y cuatro shaders en `Art/Ui/Resources/Ui/Shaders/`.

Al empezar cada combate, `BattleLighting.Begin` lee el fondo que de verdad se ve (bioma o zona de la Odisea): lo reduce a una textura de 64×36 en coordenadas de pantalla y a una sonda de 16×9 en la CPU. De ahí saca el color del cielo y del suelo, la luminancia de la escena, la saturación media y la dirección de la luz clave (el centroide de lo más brillante). Funciona con cualquier fondo nuevo sin configurarlo a mano.

| Pieza | Qué hace |
| --- | --- |
| `SalYAlmas/Battle/Figure` | Ilumina cada recorte: saturación y contraste al rango del fondo, luz de cielo arriba y de suelo abajo, pies oscurecidos, contraluz del lado de la luz clave (GDD 19.1, "luz de contorno"), envoltura de luz con el fondo que queda justo afuera de la silueta, niebla según la fila y hasta cuatro luces dinámicas. La normal sale de los mipmaps del alfa: no hace falta mapa de normales |
| `SalYAlmas/Battle/Shadow` | Sombra proyectada: la silueta tendida en el suelo, lejos de la luz clave. Nítida en los pies y difusa hacia la cabeza |
| `SalYAlmas/Battle/Backdrop` | Fondo con los brillos fuertes comprimidos (bokeh, luciérnagas), un charco de luz donde pisan las figuras, orillas en sombra y las mismas luces dinámicas sobre el suelo |
| `SalYAlmas/Battle/Haze` | Neblina baja con el color del bioma sobre la línea de los pies y una viñeta común a fondo y figuras |
| Luces dinámicas | Golpe: color del golpe. Hechizo, habilidad enemiga y Ascensión: color del planeta de quien lanza, más grande en cada nivel. Quiebre: color del planeta. Caída: blanco de sal. Turno: un foco cálido y tenue sobre quien actúa |
| Foco | La unidad en turno, el objetivo y la que estás apuntando recuperan parte de su color propio |

Ahorro de batería apaga las luces dinámicas; la luz fija y las sombras se quedan. Reducir destellos baja la intensidad de las luces de golpe al 45 %. Reducir movimiento detiene la neblina.

### Combate: HUD en las orillas y órdenes por gesto

`Presentation/BattleHud.cs` se reescribió. Conserva su API pública (`EnemyTarget`, `AllyTarget`, `InputBlocked`, `LogOpen`, `HandleWorldClick`). Respeta GDD 9.1: turnos arriba a la izquierda, Resonancia al centro, retratos con Ánima y Salinidad abajo y Granos a la derecha.

| Antes | Ahora |
| --- | --- |
| Columna de órdenes de 304 px a la derecha, con botones grandes | Sin columna. Las órdenes salen de la carta de la bruja |
| Cartas de 380×228 con tres botones de Ascensión cada una | Cartas de 268×104. La Ascensión se arrastra desde la carta |
| Rótulos "TU AQUELARRE · ALIADAS" y "ENEMIGOS" | Se quitan. Círculos y rombos siguen marcando el bando |
| Placas enemigas de 252×154 | Placas de 220×78 con vida, Firmeza, debilidades e intención |
| Línea de turnos de texto de 1180 px | Retratos de 50 px, de 520 px en total |
| Elegir objetivo y luego tocar el botón: 2 toques | Tocar la carta: Básico en 1 toque. Arrastrar: orden y objetivo en 1 gesto |

**El astrolabio de órdenes** (GDD 19.1: "los menús circulares imitan un astrolabio"):

| Gesto | Resultado |
| --- | --- |
| Tocar la carta en turno | Básico al objetivo marcado |
| Tocar una ficha sobre la carta (Básico, Autohechizo, Hechizo, Vislumbre, Cadena, Ascensión) | Esa orden al objetivo marcado |
| Arrastrar una ficha y soltarla sobre un enemigo o una aliada | Esa orden a quien la soltaste. Soltada en el vacío, se cancela |
| Sostener la carta y subir el dedo | Se abre el astrolabio: Básico, Autohechizo, Hechizo, Vislumbre y Cadena en arco; las Ascensiones más arriba, más poder cuanto más alto |
| Pasar por un pétalo y soltar sobre un enemigo | Esa orden a ese enemigo |
| Pasar por un pétalo y soltar en el vacío | Esa orden al objetivo marcado |
| Volver con el dedo a la carta | Cancela |
| Deslizar rápido hacia arriba | Hechizo, como el gesto clásico del género |
| Mantener presionada la carta sin moverla | El astrolabio queda abierto para tocar con calma (accesibilidad) |
| Arrastrar la carta de otra bruja con Ánima llena hacia arriba | Su Ascensión, también en turno enemigo |
| Arrastrar una poción del cinturón sobre una unidad | Se la da a esa unidad. Tocarla: al objetivo marcado |
| Tocar la Resonancia cuando está llena | Abre la Cadena |

Mientras sostienes una carta en turno enemigo, el director pausa la ventana antes de que actúe el enemigo (`BattleDirector.HoldDecision`). Así la Ascensión por arrastre no se escapa por la pausa de 0.55 s.

Cada pétalo muestra costo y, si no se puede, el motivo. Sostener uno abre una ficha con el nombre de la habilidad y su efecto. Las primeras seis jugadas muestran una línea que explica los gestos.

### Correcciones encontradas en el camino

- **`GuiKit.Line` se torcía fuera de 16:9 exacto.** `GUIUtility.RotateAroundPivot` no respeta la escala ni el corrimiento que aplica `GuiKit.BeginScreen`, así que en casi todos los teléfonos las líneas IMGUI salían desplazadas. Afectaba los conectores de las placas enemigas, el mapa del Fresno y la línea de apuntado. Ahora la rotación se compone en el espacio del lienzo.
- **Panel de resultado** de 1000×620 casi vacío: ahora mide lo que trae (820 de ancho al volver al Fresno o a la Odisea).

### Menos toques fuera del combate

- **Fresno:** elegir un nodo de combate ya es la decisión de pelear. Antes había que tocar el nodo y después "Pelear". El panel "Pelear" queda de respaldo si algo falla.
- **Cadena:** el medidor de Resonancia la abre directamente.

### Menús más chicos, personajes más grandes

| Pantalla | Cambio |
| --- | --- |
| Barra superior | De 130 a 108 px; cuenta y monedas al 82 % |
| Navegación | De 132 a 110 px; iconos y medallón al 84 % |
| Inicio | La favorita crece de 860 a 930 px y baja. La columna de destinos pasa de 640 a 548 px y unos 15 % menos de alto. Equipo, día planetario y acciones, más compactos |
| Aquelarre | Título y filtros más bajos; el folio sube 70 px y los retratos crecen 12 % |
| Manantial | Lista de manantiales al 86 % y ficha del banner al 88 %; la destacada gana el centro |
| Combate | Ver arriba y la sección de escala |

### Escala: brujas más chicas y enemigos a su tamaño

Segundo pedido del usuario en esta rama: que los personajes no se vean tan grandes, que queden mejor centrados y que los enemigos gigantes ocupen el escenario que les toca, para leer el tamaño de cada cosa y que el mundo impresione.

**Dato nuevo en el núcleo.** `EnemyDef.Size` (`small`, `medium`, `large`, `giant`, `colossal`), opcional en el JSON. Sin dato vale `medium`, o `large` si el enemigo es jefe. Solo lo lee la presentación: no cambia daño, turnos ni ninguna regla, y los combates grabados dan lo mismo. Se asignó a los 42 enemigos según su descripción (11 chicos, 14 medianos, 6 grandes, 7 gigantes, 4 colosos). El Ascendente de los Juicios es `giant`: un espíritu astral que sobrepasa a su bruja. `FresnoService.Scaled` conserva el tamaño.

| Tamaño | Altura respecto a una bruja | Ejemplos |
| --- | --- | --- |
| Chico | 0.62 | Gota de Obsidiana, Larva, Corneja |
| Mediano | 0.95 | Cristalizado, Figura de Sal |
| Grande | 1.35 | Soldada del Crisol, Duquesa Fiel |
| Gigante | 2.2 | Guardián, Carnero de Hierro, Medusa del Cielo |
| Coloso | 3.1 | Madre Tenaza, Boca Vieja, Serpiente Nube |

**Escenario.**

- Las brujas bajan al 80 % y la formación se acerca al centro: el aquelarre ocupa de x≈350 a 800 y los enemigos de x≈1040 a 1560, con suelo visible entre los pies y las cartas.
- Los gigantes y colosos toman la franja derecha (x de 1020 a 1890) y se la reparten si son dos. Su escolta queda a los lados y un poco al frente.
- Ninguna cabeza sube de `BattleStage.HeadLimit` (y=186): si el tamaño pedido no cabe, la figura se encoge lo justo. Con dos gigantes, ninguno pasa del ancho que le toca.
- Con un gigante en escena la cámara se abre (FOV 36 → 40; con coloso, 44): caben enteros y las brujas se ven pequeñas a su lado.
- La base de gigantes y colosos se funde con el suelo (`_BaseFade` del shader de figuras). Algunos recortes, como el de las raíces del Guardián, terminan en un corte recto que a tamaño mediano tapaba la neblina.

**HUD.**

- Jefes y gigantes no llevan placa sobre la cabeza (sobre un coloso no cabe). Su vida, Firmeza, debilidades e intención van en una **barra de jefe** arriba, bajo la Resonancia (x de 560 a 1360, y de 120 a 178). Caben dos lado a lado; un tercero usa placa normal.
- Con barra de jefe, las placas de la escolta bajan y la pista de gestos se acomoda bajo la barra en vez de taparla.
- Los corchetes de objetivo y la zona de toque usan el ancho real de cada recorte: el corchete abraza al cangrejo entero y un toque en su pinza lo elige. Ante un gigante detrás, la escolta del frente sigue siendo tocable.
- Los números de daño de un gigante salen del pecho, no de la cabeza, para no tapar la barra.
- Si el nombre no cabe en una placa, se usa el nombre corto del dato ("Gota" en vez de "Gota de Obsidi…").

**Recorrido de capturas `escala`** (solo en desarrollo): un coloso con escolta, dos gigantes, el jefe gigante solo y una oleada mixta, cada uno con un Hechizo apuntado al primer enemigo.

## Cómo correrlo

Unity 6000.6.7f1. Compila el player de desarrollo:

```bash
"/c/Program Files/Unity/Hub/Editor/6000.6.7f1/Editor/Unity.exe" -batchmode -quit -projectPath "$(pwd -W)" -executeMethod SalYAlmas.EditorTools.UiBuild.Player -buildTarget Win64 -logFile Logs/build.log
```

Capturas del combate. El recorrido nuevo deja el astrolabio abierto (`combate_1b_abanico`), sostiene un Hechizo apuntado al segundo enemigo (`combate_1c_apuntar`), lo suelta (`combate_1d_hechizo`) y escribe en `combate_gestos.txt` la orden que llegó al núcleo:

```bash
Build/Revision/SalYAlmas.exe -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -capturas "$(pwd -W)/capturas/luz" combate
```

`-bioma salinas` (o `tronco`, `ramas`, `copa`), puesto antes de `-capturas`, fuerza el fondo para revisar la luz en cada bioma. Solo existe en desarrollo.

Comparación antes y después: `python scripts/combate_luz_comparar.py antes.png despues.png salida.png`.

Escala de enemigos: cambia `combate` por `escala` (o pon los dos). Deja en la carpeta `escala_1_coloso` a `escala_4_mixta`, cada uno con su `_apuntar`, y `escala.txt`.

## Valores // TUNING

| Valor | Dónde | Por qué |
| --- | --- | --- |
| Exposición de figuras `0.74 + luma·0.7`, entre 0.78 y 1.02 | `BattleLighting.ApplyProbe` | En escenas oscuras bajan sin quedar más oscuras que su fondo |
| Tinte de cielo 30 %, de suelo 36 % (×0.88) | `BattleLighting.ApplyProbe` | Toman el color del bioma sin perder el suyo |
| Saturación `0.84 + sat·0.35`, contraste 0.95 | `BattleLighting.ApplyProbe` | El arte pintado entra al rango del fondo |
| Niebla 0.03 al frente y 0.11 al fondo; negros +0.015 | `BattleLighting` | Profundidad sin velo |
| Envoltura de luz 0.3 | `BattleLighting.ApplyProbe` | El borde toma el fondo sin brillar |
| Contraluz 0.55 a 0.85 según la luz clave | `BattleLighting.ApplyProbe` | GDD 19.1 |
| Pies: 62 % de luz en el 18 % inferior | Shader `Figure` | Contacto con el suelo |
| Foco: recupera 50 % del color propio | Shader `Figure` | El ojo va a quien actúa y a su objetivo |
| Sombra proyectada de 62 % de la altura; opacidad 0.38 a 0.62 | `SalYAlmasGame.CastShadow`, `BattleLighting` | Más fuerte en escenas claras |
| Sombra de contacto de 0.62×0.2 del ancho | `SalYAlmasGame.ContactShadow` | La proyectada hace el resto |
| Fondo del bioma de 0.68 a 0.84; zona ×0.92 | `BattleStage`, `SalYAlmasGame.SpawnBackdrop` | Ya no se separa por contraste |
| Brillos del fondo comprimidos desde 0.62 | Shader `Backdrop` | El bokeh deja de competir |
| Neblina: opacidad 0.18 a 0.3, grosor 0.04 a 0.11 de pantalla | `BattleLighting` | Esconde el corte de los pies |
| Luces: golpe 0.95 a 1.25; hechizo 1.4; Quiebre 1.8; Ascensión 2.0 a 2.8; foco del turno 0.32 | `UnitView`, `BattleDirector`, `BattleLighting` | Jerarquía de efectos |
| Reducir destellos: luces al 45 % | `SalYAlmasGame.ApplySettings` | Accesibilidad |
| Puestos de pies (y=788 al frente, 690 a 702 atrás), formación de x≈350 a 1560 y FOV 36 | `BattleStage` | Formación centrada con suelo visible |
| Brujas al 80 % | `SalYAlmasGame.WitchScale` | Personajes menos grandes |
| Alturas por tamaño 0.62, 0.95, 1.35, 2.2, 3.1 | `BattleStage.SizeFactor` | Se lee el tamaño de cada cosa |
| Franja de gigantes x 1020 a 1890, pies en y=762; escolta a los lados | `BattleStage` | Los gigantes ocupan su lado |
| Techo de cabezas y=186; ancho de gigante ×1.15 de su franja | `BattleStage.HeadLimit`, `SalYAlmasGame.FitScale` | Nada queda bajo el HUD |
| FOV 40 con gigante, 44 con coloso | `BattleStage.FovFor` | Caben enteros |
| Base fundida en 8 % de la altura (gigantes) | `SalYAlmasGame.SpawnEnemy`, shader `Figure` | Esconde cortes rectos |
| Barra de jefe x 560 a 1360, y 120, 58 de alto; dos como máximo | `BattleHudLayout` | Bajo la Resonancia, sobre las cabezas |
| Corchete: 80 % del ancho del recorte; toque ×1.18; altura hasta 900 | `BattleHud.HalfWidth` | Siluetas anchas |
| Números de daño desde 2.3 de altura como máximo | `UnitView.FloatPoint` | No tapan la barra de jefe |
| Cartas de 268×104, alzada de 14 px | `BattleHud` | Más escenario |
| Astrolabio: radio 140, pétalos de 46, Ascensiones cada 96 | `BattleHud` | Alcance del pulgar |
| Fichas de 100×36 cada 106 | `BattleHud.LayChips` | Legibles a 1080 |
| Toque: 16 px y 0.4 s para dejar abierto; deslizar: 70 px | `BattleHud` | Distinguir toque, arrastre y pulsación |
| Placas enemigas de 220×78 en 4×4 celdas | `BattleHudLayout` | Sin columna de órdenes |
| Barras uGUI de 108 y 110 px; contenido al 82 % y 84 % | `UiShell` | Menús más chicos |
| Home, Aquelarre y Manantial | `UiHome`, `UiCoven`, `UiSummon` | Ver la tabla de menús |

## Pruebas

| Prueba | Resultado |
| --- | --- |
| `dotnet test Tests/Core.Tests` | 313 de 313 (suma el tamaño de los enemigos: tamaños esperados, jefes nunca menores que grande, valor inválido rechazado) |
| EditMode en Unity (incluye `BattleHudLayoutTests` con barras de jefe y placas de escolta) | 330 de 330 (`Logs/escala-20261105/editmode.xml`) |
| Recorrido `escala` a 1920×1080 y 2400×1080 | `errores.txt` vacío; el Hechizo apunta al gigante o coloso de cada caso (`escala.txt`) |
| Compilación del player de desarrollo Win64 | Sin errores de C# ni de shaders |
| Recorrido de capturas a 1920×1080 (Raíces y Salinas) y 2400×1080 | `errores.txt` vacío. Combate automático hasta la victoria |
| Gesto simulado: sostener a Tormentina, armar el Hechizo, apuntar a Corneja y soltar | Llega al núcleo `Spell actor=4 target=101` (`combate_gestos.txt`) |

## Evidencia

En `docs/luz-y-gestos/`, capturas reales del player:

- `antes-despues-combate.jpg`, `detalle-brujas-antes-despues.jpg`: misma semilla y mismo encuentro, antes y después.
- `combate_raices.jpg`, `combate_salinas.jpg`: la luz en un bioma oscuro y en uno claro, sin tocar valores entre uno y otro.
- `astrolabio.jpg`, `arrastre_apuntar.jpg`, `salinas_apuntar.jpg`: el abanico abierto y un Hechizo apuntado por arrastre.
- `luz_del_hechizo.jpg`, `combate_automatico.jpg`: luces dinámicas de hechizo y de golpe.
- `resultado.jpg`: panel de resultado compacto.
- `antes-despues-inicio.jpg`, `antes-despues-aquelarre.jpg`, `inicio.jpg`, `aquelarre.jpg`, `manantial.jpg`: menús.
- `movil_20x9_apuntar.jpg`, `movil_20x9_inicio.jpg`: proporción de teléfono.
- `escala_coloso.jpg`, `escala_coloso_apuntar.jpg`: la Madre Tenaza con su escolta, barra de jefe y corchete a su ancho.
- `escala_dos_gigantes.jpg`, `escala_jefe_gigante.jpg`, `escala_mixta.jpg`: dos gigantes repartidos, el Guardián solo con la base fundida y una oleada chica, mediana y grande.
- `escala_movil_20x9.jpg`: el coloso en proporción de teléfono.

## Huecos y pendientes

- **Teléfono real sin probar.** Los gestos se probaron con el mismo código que usa un dedo (`Track` y `Release`), pero sin eventos táctiles del sistema. Falta medir en Android e iOS el alcance del pulgar, el área segura y que el arrastre no se confunda con el desplazamiento.
- **Pantallas IMGUI densas.** Forja, Caldero, Libro Mayor y el mapa del Fresno no se rediseñaron. Siguen siendo candidatas a migrar a uGUI.
- **Arte de enemigos procedural.** El golem gris sigue resaltando más que las brujas pintadas aunque toma la luz del bioma. La solución de fondo es arte propio.
- **Las sombras proyectadas casi no se ven en biomas muy oscuros.** Es correcto (no hay luz que las forme), pero el contacto con el suelo depende ahí de la sombra de contacto y la neblina.
- **El texto grande puede apretar las cartas de 268 px.** Revisar con "texto grande" activo.
- **HUD de combate en 16:9 dentro de pantallas anchas.** Sigue en IMGUI: en 20:9 deja franjas a los lados (ya pasaba). Migrarlo a uGUI lo resolvería y aprovecharía el ancho.
- **Los números de daño suben sobre la placa del enemigo** y a veces la tapan un instante.
- **Tamaños asignados por descripción.** Los 42 tamaños de `enemies.json`, `fresno_enemies.json` y `odyssey.json` salen de lo que dice cada descripción ("cangrejo colosal", "remolino pequeño"). Conviene que el usuario los confirme; cambiar uno es editar su `"size"`.
- **Recortes anchos en la orilla.** La Medusa del Cielo, a la derecha de dos gigantes, asoma un poco fuera de pantalla. Es un encuadre aceptable en gacha, pero si molesta se baja el ×1.15 de `FitScale`.
- **Escolta con nombres largos.** Las placas de 220 px usan el nombre corto si el largo no cabe; algunos nombres cortos del dato siguen siendo largos.
- **Integración con `auditoria/estabilizacion`.** Esa rama, sin integrar todavía, cambia 5 líneas de `DrawEnemyPlates` (protege contra más de seis placas). Esta rama reescribió ese método con la misma protección (`i < resolved.Count`): al integrar, quédate con la versión de esta rama.
- **Sin prueba con personas** de comprensión de gestos: los tiempos y aciertos están por medir.
