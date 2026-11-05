# Traspaso: Odisea, misiones y kits de las 24 brujas

Este documento es para el agente que siga. La cuenta anterior se quedó sin crédito a media tarea. **Casi nada de este encargo está construido:** aquí está la investigación, el diseño y un parche sin aplicar.

## Dónde trabajar

| Dato | Valor |
| --- | --- |
| Carpeta | `C:\Arcanoria Master\Gusi Gaming\gacha-fase3` (worktree) |
| Rama | `fase/3`, 4 commits por delante de `origin/main` y sin subir |
| Base verificada | `d4f6fb7`: Unity EditMode 290 de 290, `dotnet test` 288 de 288, jugador de Windows con 19 capturas sin excepciones |
| Lo posterior | Este documento y `Tools/Content/pendiente_nucleo_trucos.py`. No cambian código del juego |

Otras personas y agentes trabajan a la vez:

- **Checkout principal (`Gacha Agustin`, en `main`):** alguien tiene audio sin confirmar, incluidos `MetaHub*`, `SalYAlmasGame` y `TempSfxPlayer`. Va a chocar con `fase/3` al fusionar. No trabajes ahí.
- **Worktree `gacha-ui` (rama `ui/interfaz`):** otra sesión está haciendo una interfaz en uGUI sobre `d4f6fb7`, con archivos en `Presentation/Ui/` y cambios sin confirmar en `GuiKit`, `Theme`, `MetaHub.cs` y `SalYAlmasGame`.
- **Codex:** genera arte en `art/` y `Assets/SalYAlmas/Art/`.
- **Instrucción del usuario:** "mantente en tu línea" y no interactúes con los otros agentes. Si tocas `Meta/`, espera un choque con `ui/interfaz` y déjalo anotado.
- **Stash:** `stash@{0}` y la rama `respaldo/stash-github-desktop-0254` guardan lo que GitHub Desktop apartó el 5 de noviembre, incluido trabajo de Codex. No los sueltes ni los borres.

Commit y push: los commits de `fase/3` son locales. El usuario no ha aprobado subir ni fusionar.

## Lo que pidió el usuario (textual)

1. "Adecua lo que falta de la fase 3 dentro de lo posible sin hacer conexiones todavía de servidores, primero asegurate de que la versión que tenemos sea efectiva, desarrolla un sistema de misiones y de la odisea del mundo con el mapa interactivo, arregla detalles de sestos y da habilidades y combate a todas las brujas". Lista de pendientes que pegó: servidor y compras (fuera por ahora), móvil, audio, arte (atuendos, enemigos cápsula), idiomas y que solo 6 de 24 pelean.
2. "Hay varios agentes trabajando, mantente en tu línea y busca tener ediciones como sea necesario sin interactuar con los agentes".
3. "Investiga cómo son las misiones de brave frontier, teniendo diferentes secciones por las que se pasa para concluir una zona y adapta el modelo, de esta manera hay gameplay y continuidad, crea una historia al respecto y únela para que se sienta efectivo al desarrollar y probar, desarrolla enemigos ingeniosos en zonas creativas".
4. "Empieza en la zona sencilla y va avanzando la odisea a través de las regiones, por eso naturalmente suben de nivel los enemigos también yendo a biomas cada vez más extraños, utiliza la estética, el horror cósmico e inspiración por los biomas de terraria para adaptarlos al universo".

Interpreté "detalles de sestos" como "detalles de estos" (los pendientes de la lista). No lo confirmé.

Antes de eso pidió un home tipo Brave Frontier adaptado a la estética del juego. Ya existe (`Presentation/Meta/MetaHub.Home.cs`).

## Estado de cada pedido

| Pedido | Estado |
| --- | --- |
| Comprobar que la versión sea efectiva | Hallazgo sin arreglar (abajo). El robot de pruebas de interfaz no existe |
| Kits para las 18 brujas | Diseño abajo. Parche de trucos sin aplicar. Generador sin escribir |
| Misiones (encargos y logros) | Sin empezar |
| Odisea con mapa interactivo y zonas por secciones | Investigación y diseño. Sin código |
| Enemigos ingeniosos y biomas | Lista de ideas. Sin datos ni código |
| Móvil, audio, arte, idiomas | Sin empezar |

## Primero: el hueco que hace inefectiva la versión actual

No lo he medido en una partida real; sale de los datos.

- Una cuenta nueva tiene brujas de nivel 1, y el Fresno tiene enemigos fijos de nivel 52 en adelante. Hoy el Fresno es imposible para quien no use el perfil de prueba.
- Subir del nivel 1 al 40 cuesta unas 400 000 almitas, y una misión de historia da de 6 000 a 12 000. No hay otro ingreso antes del Fresno.
- En la historia, los enemigos escalan con el nivel del equipo (`StoryService.PrepareBattle`), así que subir de nivel no sirve de nada.
- La decisión D-02 (`100·n^1.5` contra `40·n^1.5`) sigue sin respuesta del usuario. El GDD manda: hasta que decida, sigue en 100.

La nueva Odisea progresiva resuelve casi todo esto: los enemigos suben por región, no por equipo. Quita el escalado por equipo, y usa las secciones repetibles y los encargos como ingreso.

**Falta también probar la interfaz de verdad.** Las capturas solo comprueban que cada pantalla se dibuja. Nadie ha presionado sus botones. Propuesta: un robot en `GuiKit.Button` que, en compilación de desarrollo, reciba una lista de etiquetas y las "presione", y falle si una no aparece. Las zonas invisibles (`GUI.Button` con `GUIStyle.none`) tendrían que pasar por un `GuiKit.Hit(rect, id)`. Se corre con el recorrido de `Presentation/Meta/ScreenWalk.cs`. El guion mínimo es: crear Calculista, prólogo con primera Cadena, banner de inicio, home, primera zona, un encargo.

## El modelo de Brave Frontier

Lo que pude confirmar en su wiki:

- **Estructura:** mundo → región → zonas → misiones. [Mistral](https://bravefrontierglobal.fandom.com/wiki/Mistral) tiene 9 zonas: las primeras son temáticas, una torre cierra la región y la última es una zona EX.
- **Misiones:** [Adventurer's Prairie](https://bravefrontierglobal.fandom.com/wiki/Adventurer%27s_Prairie) tiene 3 misiones de 5 a 6 batallas, con energía de 3 a 5, experiencia, y un jefe que no se captura y suelta material.
- **Recompensa:** [terminar una zona da 1 gema](https://bravefrontierglobal.fandom.com/wiki/Quests). Las mazmorras bonus se abren al limpiar todas las zonas del mapa, con etapas de enemigos al azar y un jefe.
- **Refuerzo:** antes de salir se elige un sexto miembro, un amigo o desconocido.

Lo que **no pude confirmar**, porque mis búsquedas no lo cubrieron: el detalle de lo que pasa entre batallas. De memoria, la vida se arrastra entre batallas y hay botín por recoger, pero verifícalo si importa. Otras páginas de la wiki se pueden leer con el navegador integrado; `WebFetch` devuelve 402.

## Diseño propuesto: la Odisea

```
Mundo (anillo de Halia)
  → Región (bioma con su Casa)
    → Zona (3 por región, más una EX al terminarla)
      → Misión (historia al inicio y al final)
        → Secciones (4 a 6 combates seguidos; vida y Salinidad se arrastran; botín y tesoros)
          → Jefe
```

| Regla | Propuesta (`// TUNING`) |
| --- | --- |
| Continuidad | Vida, Salinidad y cinturón se arrastran por sección, como en el Fresno. Las caídas vuelven con 10 % |
| Derrota | Reintenta la sección sin costo y sin perder botín. El GDD 11.2 dice que en la historia las almas no se pierden |
| Energía | No existe (GDD 16.4). Limita el botín diario: después de 20 secciones, ×0.25 |
| Repetir | Misión terminada = Expedición: solo secciones, sin escenas, con botín |
| Estrellas por misión | Terminar, sin caídas, con al menos un Spark. Dan Sales de Baño una vez |
| Limpiar zona | Sales de Baño, el equivalente a la gema |
| Zona EX | Se abre al terminar la región, con enemigos más fuertes y sin historia |
| Mapa | Anillo de las 12 regiones con el Fresno al centro y la Tortuga como marcador. Tocar una región abre su mapa con zonas en ruta y puntos de interés, incluida una salina con cosecha diaria |

Encaja con el código actual: `StoryScene` ya tiene combates. Agrega `RegionDef.Zones`, botín en `StoryBattle` y un estado de misión en `CalculistaState` (vida, Salinidad, caídas). Cambia `PrepareBattle` para usar `StartHp` y `StartSalinity`, que `BattleSetup` ya soporta.

### Progresión: de lo sencillo a lo extraño

**Esto contradice al GDD 6,** que pide empezar en la región de tu signo y orden libre, con eclipses que corrompen lo no visitado. El usuario pidió explícitamente lo contrario, así que sigue su instrucción y **actualiza el GDD o repórtalo** (`CLAUDE.md` dice que gana el GDD y se reporta, pero esta es una orden directa del dueño). Las pruebas `LasDosRegionesSeTerminanEnCualquierOrden…`, `HayDosRegiones…` y el mapeo de signos en `story.json` van a cambiar. Falta decidir qué hace ahora el signo y el eclipse. Sugerencia: el signo da el bono de estadística y un diálogo propio; el eclipse sube a la región siguiente de estado, sin bloquear.

Orden propuesto. Conserva las regiones y conflictos del GDD 6, y las tres capas de horror del GDD 6.5. Los niveles son un punto de partida.

| # | Región (GDD) | Inspiración de Terraria | Adaptación | Capa | Nivel |
| --- | --- | --- | --- | --- | --- |
| 0 | Vado de Sal (prólogo) | Superficie al empezar | Río de leche cuajada, ya en `story.json` | Superficie | 3 a 5 |
| 1 | Tauro, Llanuras de Cobre | Bosque | Molinos; pueblo que olvida con sal dulce | Superficie | 5 a 12 |
| 2 | Cáncer, Costa de las Conchas | Océano | Mareas y cangrejos que hunden barcos | Superficie | 12 a 18 |
| 3 | Aries, Montes de Hierro Rojo | Cavernas | Minas y fortalezas, ya escritas | Superficie | 18 a 24 |
| 4 | Leo, Desierto Áureo | Desierto | Arena viva y rebaños solares | Superficie | 24 a 30 |
| 5 | Virgo, Abadía de las Herbolarias | Selva | Huertos que digieren visitantes | Grieta | 30 a 36 |
| 6 | Capricornio, Salinas de Obsidiana | Obsidiana y capas profundas | Remolinos, ya escritos | Grieta | 36 a 42 |
| 7 | Géminis, Ciudad Espejo | Sagrado (prismas) | Canales y reflejos que actúan solos | Grieta | 42 a 48 |
| 8 | Acuario, Archipiélago de Hielo | Tundra | Cantos bajo el hielo | Grieta | 48 a 54 |
| 9 | Libra, Ducado de las Balanzas | Mazmorra | Banco-laberinto donde todo se paga con almas | Abismo | 54 a 60 |
| 10 | Escorpio, Pantanos de Azufre | Corrupción y Carmesí; Inframundo | Bajar al Río Lácteo | Abismo | 60 a 66 |
| 11 | Sagitario, Estepas del Centauro | Meteorito y eclipse | Un ojo gigante; estrellas caídas | Abismo | 66 a 72 |
| 12 | Piscis, Isla de la Ninfa | Hongos luminosos | Una isla preciosa de la que nadie quiere irse | Abismo | 72 a 76 |
| Fin | Copa Astral | Pilares y evento lunar | La Jardinera y los Huéspedes | Abismo | 76 a 80 |

Mantén el tono del GDD 6.5: pavor antes que gore, sin sustos repentinos, cuerpo contenido, las Termas siempre seguras. Lo de Terraria es inspiración de bioma, no copiar nombres ni criaturas.

### Hilo de la historia (borrador mío, sin aprobar)

El Libro Mayor tiene folios arrancados, y la Calculista es la única que puede leerlo. Cada Casa guarda un folio de la "Cuenta de la Cosecha". La Tortuga da la vuelta al anillo para reunirlos. El lector cree que es para curar al Fresno. Las capas revelan que es la libreta de pedidos de los Huéspedes. En la Copa, según refinó o digirió, llega a Cosecha, Ceniza o Libro en Blanco, y al final secreto con Paralaje máximo (GDD 13.5). Lo escrito para Aries y Capricornio en `story.json` se reparte en zonas: la capitana Ferrán, perdonada, vuelve en Capricornio. Ojo: con el orden nuevo, Aries (3) sigue antes que Capricornio (6), así que esa continuidad funciona, pero las variantes `primera_capricornio` sobran.

Cada región recluta a alguien. El GDD 18.4 pide que todas las 3★ se puedan reclutar por historia, y la propuesta de meta-juego recomienda sumar a Ixbalam (Sol) y Centella (Júpiter), para cubrir los siete planetas.

### Enemigos ingeniosos (ideas, por validar)

| Bioma | Enemigo | Mecánica | Cómo se le gana |
| --- | --- | --- | --- |
| Llanuras | Molinero de Sal Dulce | Roba Granos | Matarlo primero |
| Costa | Cangrejo Salinero | Protege a sus compañeros mientras viva | Romperlo antes que a los demás |
| Montes | Minero Cristalizado | Al caer suelta Salinidad a todas | Rematar con la Salinidad baja |
| Desierto | Becerro Solar | Se enfurece si le lanzan Hechizos | Pegarle con Cadena y básicos |
| Abadía | Novicia sin Alma | Cura a las demás | Prioridad de objetivo |
| Salinas | Boca Vieja | Retrasa a todo el equipo cada 2 turnos | Quebrarla antes de que cargue |
| Ciudad Espejo | Doble de Azogue | Devuelve parte del daño | Daño por turno o Ascensión |
| Hielo | Cantora Sumergida | Sube la Salinidad de quien está más limpia | Autohechizo y descanso |
| Balanzas | Tesorera de Almas | Cobra almitas sueltas o vida | Terminar rápido |
| Pantanos | Los Sin Carta | Inmunes a Aspectos (GDD 6.5) | Spark y Firmeza, no Reacciones |
| Estepas | Cría de Estrella | Explota al caer | Ordenar quién cae primero |
| Isla | Ninfa Hospitalaria | Cura a las brujas mientras les quita Resonancia | No dejarse consentir |
| Copa | Huésped menor | Cambia de debilidad por fase | Leer la placa |

Cada enemigo necesita una pista en su placa (`Traits.Hint`). Para quitar las cápsulas sin arte, propongo una silueta generada por código según `Traits.Shape` y su planeta, en `CreateFigure` de `SalYAlmasGame`.

## Kits de las 18 brujas

**Restricción clave:** las pruebas de la Fase 1 exigen exactamente 6 brujas en `witches.json`. Ponlas en `Data/witches_ext.json` y súmalas sobre un `GameDatabase.Clone()` dentro de `MetaLoader.Load`, para que `TestKit.RealDb()` siga con 6.

Lo que eso obliga a cambiar:

- `MetaDataSources` lee el archivo nuevo; `SalYAlmasGame.MetaSources` y `BattleSceneBuilder` lo asignan.
- `MetaGame.TestProfile` itera `Combat.WitchOrder`: fíjalo a las seis de siempre, o las pruebas de la Fase 2 cambian.
- `SalYAlmasGame.StartBattle` usa `_db` para `BattleDirector`: los combates externos deben usar `_hub.Game.Combat`.
- `roster.json`: todas con `"kit": true`. `MetaDataTests` espera 6: actualízala.
- `Presentation`: quita las etiquetas "sin kit".

Hazlos con un generador en Python (`Tools/Content/gen_kits.py`) que combine una plantilla por papel con una tabla de nombres, para no escribir 900 líneas a mano. Cada kit necesita básico, Hechizo, Apertura, Enlace, Cierre y tres Ascensiones (formato en `witches.json`). Reglas del cargador: las 3★ no llevan Autohechizo y las 4★ y 5★ sí; un eslabón respeta `MaxLinkFrames`. Para variar el Spark, recuerda que el eslabón n empieza 24·(n−1) frames después y la ventana es de 2 frames: un último golpe en 26 con un primero en 2 hace Spark.

| Bruja | Rareza | Planetas | Papel | Idea |
| --- | --- | --- | --- | --- |
| Cervanda | 5★ | Saturno, Júpiter | Guardiana | Provocación, escudos al equipo, gravedad (retraso) |
| Cupria | 5★ | Venus, Luna | Sanadora | Cura, limpia y veneno moral |
| Brea | 4★ | Saturno, Luna | Rompedora | Firmeza alta; balanza |
| Grajilla | 4★ | Mercurio, Saturno | Saboteadora | Disipa y retrasa |
| Madroña | 4★ | Venus, Marte | Guardiana | Provocación con mucha vida |
| Risca | 4★ | Marte, Mercurio | Ofensiva | Sangrado con varios golpes |
| Nacarina | 4★ | Luna, Júpiter | Sanadora | Curación con adelanto |
| Centella | 4★ | Júpiter, Marte | Rompedora | Electrocución y Firmeza |
| Véspera | 4★ | Saturno, Venus | Ofensiva con drenaje | Se cura al golpear |
| Cempa | 4★ | Mercurio, Sol | Saboteadora | Embaucadora: disipa y gana Resonancia |
| Ixbalam | 4★ | Sol, Saturno | Ofensiva | Quemadura |
| Estrige | 4★ | Luna, Mercurio | Tejedora | Mejora de ataque y adelanto |
| Salitre | 3★ | Venus, Saturno | Ofensiva contra jefes | **Truco del GDD 17:** empieza intoxicada y su Hechizo es `maxHpBurst` |
| Axa | 3★ | Luna, Venus | Sanadora | **Truco del GDD 17:** `reviveOnce` 0.30, y su Cierre cura a todas |
| Migaja | 3★ | Mercurio, Luna | Tejedora | Adelantos |
| Vellón | 3★ | Venus, Luna | Sanadora | Cura pequeña |
| Púa | 3★ | Marte, Sol | Rompedora | Firmeza con espinas |
| Charca | 3★ | Luna, Júpiter | Guardiana | Escudos pequeños |

Plantilla inicial de estadísticas: las 5★ como las de `witches.json`; ×0.9 para 4★ y ×0.8 para 3★. Las `MainAction` de `AutoPilot` ya usan Hechizos de apoyo sobre la aliada con menos vida.

Agrega un comando `kits` al simulador con la prueba estándar de daño: **ninguna 5★ nueva supera en más de 5 % a la mejor de su papel** (GDD 17.1). Cada 3★ debe tener un nicho medible.

### Parche sin aplicar: `Tools/Content/pendiente_nucleo_trucos.py`

Se escribió justo antes del corte. **Nunca se ha ejecutado ni compilado.** Se corre desde la raíz con `python Tools/Content/pendiente_nucleo_trucos.py` y avisa con `NO MATCH` si un reemplazo falla. Bórralo cuando lo apliques.

| Agrega | Falta después de aplicarlo |
| --- | --- |
| Efectos `taunt`, `maxHpBurst`, `stealGrains`, y la provocación al elegir objetivo | Comprobar que `AddGrains` acepta negativos |
| Pasivas `reviveOnce` y `startPoison`, y el evento `UnitRevived` | Sonido en `TempSfxPlayer` y animación en `BattleDirector` |
| Clase `EnemyTraits`, efectos al morir, `GameDatabase.Clone`, `DataLoader.AddWitches` | **Leer `traits` en `DataLoader.ReadEnemies`** y copiarlos en `FresnoService.Scaled` |
| | **Conectar** `ReflectPct`, `AspectImmune`, `ProtectAllies` y `SpellEnrage` en `Battle.Damage` y `Battle.Chain`: hoy solo son campos |

Revisa también que el reemplazo de `public bool IsBoss;` caiga en `EnemyDef` y no en otra clase. Si cambia un combate de la Fase 1, deben seguir pasando los combates grabados.

## Sistema de misiones

Diseño en `docs/propuestas/META_Y_ONBOARDING.md`, secciones 4, 5 y 8 (sin aprobar; los valores llevan `// TUNING`):

- **Encargos diarios:** 4 al día, cada uno con 15 Sales de Baño, 100 de Crecimiento, 100 de experiencia del Grimorio y 5 000 almitas. Bono de 10 000 almitas por los cuatro. Regla: un ancla, una hazaña, uno de Termas y uno de taller.
- **Encargos semanales:** 6 (3 fijos y 3 al azar), sin Sales de Baño.
- **Logros:** la sección 8.2 trae 30 de ejemplo. Nunca piden tirar, pagar ni juntar brujas del gacha, y nunca "entra X días seguidos".
- **Altura:** el nivel de cuenta sale del Crecimiento (curva en 4.2). Hoy `MetaHub.DrawTopBar` usa el nivel de Ofrenda como sustituto: cámbialo.

Propuesta de código: `Core/World/MissionService.cs` con `Report(tipo, cantidad)` llamado desde Fresno, historia, combate, crafting y Termas. La rotación es determinista con el día. Datos en `Data/missions.json`. Tablero en el home y aviso en la barra inferior. Pruebas: rotación, recompensa una sola vez, reinicio diario, y que el libro contable cuadre. Declara las fuentes nuevas en `economy.json`, o `Ledger.Apply` lanza excepción.

## Pendientes sin servidor

| Tema | Qué se puede hacer | Nota |
| --- | --- | --- |
| Móvil | Intentar compilar en Android: el módulo está instalado, pero no sé si el SDK y el NDK también. iOS no: falta el módulo y una Mac | Hazlo en el worktree: cambiar de plataforma reimporta todo. Primero decide D-14: 6000.6.7f1 no es LTS |
| Fps | Contador en desarrollo, medido en Windows como referencia | Las metas de 60 y 30 fps solo se validan en teléfono |
| Audio | La sesión de `main` ya trabaja en esto. Solo conecta los efectos nuevos con `PullSequence.Sfx`, que es genérico | Sin FMOD ni pistas separadas no hay capas reales |
| Atuendos | Sigue el teñido hasta que haya arte o modelos | Pídele a Codex variantes |
| Enemigos | Siluetas por código, ver arriba | |
| Idiomas | Hay mucho texto en código en `Presentation/Meta/`. Muévelo a `loc.json` si hay tiempo | `ui/interfaz` usa TextMeshPro; revisa antes de duplicar. Las traducciones no están revisadas |
| Accesibilidad | "Reducir destellos" no cubre el combate: conéctalo a `UnitView.Flash` | |

## Orden sugerido

1. Aplica el parche, termina `EnemyTraits` y deja las pruebas en verde.
2. Genera los 18 kits, con simulador y prueba de 5 %. Es lo que desbloquea abrir tiradas.
3. Implementa zonas, secciones, botín y Expedición en `StoryService`. Reordena regiones y quita el escalado por equipo.
4. Escribe el contenido: 3 zonas por región, con un generador en Python. Empieza por las regiones 1 a 3, y prueba que se sienta bien antes de escribir las 12.
5. Encargos, logros y Altura.
6. Mapa de la Odisea y pantalla de misión con la pista de secciones.
7. Robot de interfaz y `dotnet run --project Tests/Core.Sim -- historia`, con una cuenta nueva que gaste sus almitas en niveles. Ajusta hasta que se pueda terminar.
8. Actualiza `docs/FASE3.md`, `docs/HUECOS.md` y el GDD 6, y escribe tu reporte.

## Cómo probar

```bash
dotnet test Tests/Core.Tests
```

```bash
dotnet run --project Tests/Core.Sim -- historia 20
```

```bash
dotnet run --project Tests/Core.Sim -- fresno 200
```

Para Unity en modo batch, desde el worktree y con `W="$(pwd -W)"`:

```bash
"/c/Program Files/Unity/Hub/Editor/6000.6.7f1/Editor/Unity.exe" -batchmode -projectPath "$W" -runTests -testPlatform EditMode -testResults "$W/Logs/r/editmode.xml" -logFile "$W/Logs/r/editmode.log"
```

```bash
"/c/Program Files/Unity/Hub/Editor/6000.6.7f1/Editor/Unity.exe" -batchmode -projectPath "$W" -executeMethod SalYAlmas.EditorTools.ScreenTour.BuildPlayer -quit -logFile "$W/Logs/r/build.log"
```

```bash
Builds/Windows/SalYAlmas.exe -recorrido "$W/Logs/r/shots" -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -logFile "$W/Logs/r/player.log"
```

Revisa las capturas abriéndolas: varios errores solo se vieron así. `-recorrido` borra el perfil guardado.

## Trampas del entorno

- **Heredocs en la herramienta Bash:** varias veces fallaron con `unexpected EOF`. Escribe los archivos con la herramienta Write y corre Python desde ahí.
- **Saltos de línea:** el repositorio mezcla CRLF y LF. Los scripts deben normalizar antes de reemplazar y conservar lo que había.
- **Unity:** cada corrida puede modificar `Assets/UniversalRenderPipelineGlobalSettings.asset`. No lo incluyas en los commits (`git checkout --` antes de agregar). También genera `.meta` y reconstruye `Scenes/Combate.unity`.
- **`git add`:** usa rutas concretas. Un `git add Assets/SalYAlmas/Tests` ya metió por accidente un archivo de Codex.
- **Stash compartido:** nunca uses `git stash` ni `pop` sin más.
- **Mensajes de commit:** terminan con `Co-Authored-By`, según el recordatorio de atribución de tu sesión.

## Referencias

| Tema | Dónde |
| --- | --- |
| Reglas del repositorio | `CLAUDE.md` (el GDD manda; `// TUNING`; español de México; una fase por rama) |
| Mundo y horror | `docs/GDD.md` 6 y 6.5; LitRPG en 13; elenco en 17 y 18; Fresno en 10 |
| Decisiones y huecos | `docs/HOJA_DE_RUTA.md` (D-01 a D-15), `docs/HUECOS.md` (H-13, H-14, H-16, H-27, H-28) |
| Propuestas | `docs/propuestas/META_Y_ONBOARDING.md`, `FRESNO.md`, `CRAFTING.md` |
| Reportes | `docs/FASE1.md`, `docs/FASE2.md`, `docs/FASE3.md` |
| Historia actual | `Assets/SalYAlmas/Data/story.json` |
| Código | `Core/World/StoryService.cs`, `Services.cs`, `WorldLoader.cs`; `Presentation/Meta/` |
| Audio | `docs/audio/AUDIO_PROTOTIPO.md` |
| Capturas de la base | `Logs/f3/shots/` (ignoradas por git) |
| Memoria | `C:\Users\Adass\.claude\projects\C--Arcanoria-Master-Gusi-Gaming\memory\` |
| Brave Frontier | [Quests](https://bravefrontierglobal.fandom.com/wiki/Quests), [Mistral](https://bravefrontierglobal.fandom.com/wiki/Mistral), [Adventurer's Prairie](https://bravefrontierglobal.fandom.com/wiki/Adventurer%27s_Prairie) |

## Decisiones que solo el usuario puede tomar

| Decisión | Mi recomendación |
| --- | --- |
| D-02: escala de almitas | `40·n^1.5`, como dice la hoja de ruta |
| Subir y fusionar `fase/3` | Sí, después de integrar con `ui/interfaz` y el audio de `main` |
| Guion | Es borrador mío, y hay que aprobarlo o sustituirlo (H-14) |
| GDD 6: cambiar a progresión lineal | Ya lo pidió; falta reflejarlo en el GDD |
