# Fase 2 · La Ascensión: estado del trabajo (traspaso)

Este documento es para quien continúe la Fase 2. El trabajo se cortó a la mitad por pedido del usuario para seguir en local. Todo lo que está aquí **compila** (`dotnet build Tests/Core.Lib`, 0 advertencias) y **las 152 pruebas pasan**: las 140 de la Fase 1 sin cambios, 7 de datos de la Fase 2 y 5 de humo.

```bash
dotnet test Tests/Core.Tests
```

## Qué ya está hecho

### 1. Ganchos de combate (núcleo de la Fase 1, sin romper nada)

- `Core/Combat/Modifiers.cs`: `ModKeys` (todas las claves de modificador, validadas al cargar datos), `StatBlock` (suma de modificadores), `ChainEcho` y `WitchLoadout`.
- `BattleSetup` ganó: `Loadouts`, `Modifiers`, `CustomEncounter`, `CustomEnemies`, `CustomPotions`, `StartHp` y `StartSalinity`. Con valores por defecto el combate es idéntico al de la Fase 1.
- `Battle` lee los modificadores en: creación de brujas y enemigos, inicio (Granos, Resonancia, Ánima, escudos y adelantos), ΣB por tipo de golpe, crítico, penetración, Spark, Firmeza, vulnerabilidad, daños por turno, retrasos, curación, escudos y ganancias de Salinidad, Ánima y Resonancia. También los Frutos: daño por retraso (Saturno), +daño contra Quemadura (Sol), crítico contra Sangrado (Marte), escudo inicial (Luna), adelanto inicial (Mercurio), cura por turno (Venus) y tope de Resonancia por Spark (Júpiter).
- `ChainPlanner` acepta ecos de arma; `PreviewChain` ya refleja `FrameShift` (Rítmico −2 y Pausado +2) y los ecos.
- `HitSource.Graft` para el daño del Fruto de Saturno.

### 2. Datos de la Fase 2 (`Assets/SalYAlmas/Data/`) y cargador (`Core/Meta/MetaLoader.cs`)

| Archivo | Contenido |
| --- | --- |
| `economy.json` | Recursos de la tabla 11.1 con fuentes y sumideros; materiales regionales, reactivos y bases |
| `roster.json` | Las 24 brujas de lanzamiento (6 con kit) |
| `progression.json` | Curva de nivel, grados C a SS, Vía, Dignidad, Talentos de Cadena, Juicio y condiciones LitRPG, recuerdos |
| `weapons.json` | 12 familias × 5 niveles × 2 variantes (120), 8 legendarias, estaciones, recetas, fusiones de la 15.3, refinación, reforja y 26 prefijos |
| `brewing.json` | Bases, reactivos, recetas de las 11 Salmueras, tres potencias y Salmueras Turbias |
| `gacha.json` | Tasas del GDD, banner de inicio, estándar y dos limitados |
| `fresno.json` | Zonas, mapa, recompensas, 7 Ramas × 15 Injertos (5 activos y 10 pendientes), Frutos, Mutaciones, maldiciones, Paria, Eco, Raíces, Ofrenda, tope semanal, eventos LitRPG |
| `fresno_enemies.json` | 13 enemigos del Fresno (3 normales por zona más los 4 jefes) |

Los enemigos y las brujas nuevas viven en archivos aparte porque las pruebas de la Fase 1 exigen exactamente 3 enemigos normales y 6 brujas en sus JSON.

### 3. Sistemas del núcleo (escritos; solo hay pruebas de humo)

| Sistema | Archivo | Estado |
| --- | --- | --- |
| Libro contable | `Core/Economy/Ledger.cs` | Rechaza flujos no declarados y saldos negativos. Guarda 1 000 movimientos y totales por flujo |
| Guardado | `Core/Save/` | Serializador JSON por reflexión, `ISaveStore` con `FileSaveStore` y `MemorySaveStore`, `PlayerProfile`. Guardar y cargar da el mismo JSON (prueba de humo) |
| Fachada | `Core/Meta/MetaGame.cs` | Perfil nuevo y perfil de prueba, azar con semilla guardado en el perfil, día como entrada |
| Progreso | `Core/Progress/ProgressionService.cs` | Nivel 100·n^1.5, grados 20 000·1.8^k con sal refinada y nivel mínimo, Vía (un cambio gratis), Dignidad, Talentos y `LoadoutBuilder` |
| Juicio | `Core/Progress/TrialService.cs` | Genera al Ascendente con el kit de la bruja, evalúa las nueve condiciones y cobra solo si gana (reintentos sin costo) |
| Yunque y Altar | `Core/Crafting/Forge.cs` | Recetas con estaciones (incluye dos estaciones a la vez), fusiones, reforja por peso, equipar y refinar sal |
| Caldero | `Core/Crafting/Cauldron.cs` | Base + sal + reactivo + almitas, potencia por pureza, Turbia al fallar, Recetario y definiciones para el combate |
| Termas | `Core/Gacha/Termas.cs` | Pity suave y duro, 4★ cada 10, 75/25, contador por tipo, exclusión, Polvo Astral, recetas y Sal Cruda, banner de inicio, selector 300, historial. `GachaSimulation` verificada: ~52.9 y ~66.1 |
| Fresno | `Core/Fresno/` | Generador con semilla, runs, Injertos, Frutos, Mutaciones, maldiciones, Paria, eventos, Santuario, Mercader, Caldero, Nido, almitas sueltas, Mancha, Savia, Eco, Raíces, Ofrenda y tope semanal |

## Qué falta

1. **Pruebas mínimas del prompt** (`Assets/SalYAlmas/Tests/EditMode/Phase2/`): generador con semilla fija (huella exacta), Mancha en sus tres casos (recuperar, morir antes y pasar de largo o terminar), Eco, curvas de nivel y grado, condiciones de Juicio, recetas y fusiones (15.3), prefijos que mueven el Spark en `PreviewChain`, tasas del gacha, exclusión de duplicados, guardado y carga sin pérdida, y libro contable.
2. **Piloto de runs** (`AutoClimber`): elegir nodos, Injertos y eventos para jugar runs completas en pruebas y en el simulador. Con él falta la prueba de aceptación de "tres runs seguidas" (la tercera con Eco, Mancha recuperada o absorbida y una Raíz comprada).
3. **Balance del Fresno**: no se ha simulado una run completa. Hay que ajustar vidas por zona, almitas por nodo y el tope semanal con el piloto.
4. **Simulador** (`Tests/Core.Sim`): comandos `gacha`, `fresno` y `juicio`.
5. **Interfaz de prueba en Unity** (IMGUI, mismo estilo que la Fase 1): hub con mapa del Fresno, pantalla de grados con las tres fases (placeholders), Yunque, Caldero, Termas (termómetro de pity, vapor por rareza, historial) y libro contable. Los combates del Fresno y de los Juicios deben reutilizar `BattleDirector`, `BattleHud` y `ChainScreen`. `SalYAlmasGame` necesita un modo de combate "externo" con un botón Continuar que devuelva el `Battle` al hub. Hay que agregar los JSON nuevos al constructor de escena (o leerlos de `Application.dataPath` en el Editor).
6. **Archivos `.meta`**: los archivos y carpetas nuevos de `Assets/` no los tienen. Unity los genera al abrir el proyecto; hay que hacer commit de ellos.
7. **docs/FASE2.md** final con valores // TUNING, Injertos pendientes y huecos del GDD (la lista de abajo es el punto de partida).

## Decisiones tomadas (interpretaciones)

| Tema | Decisión |
| --- | --- |
| Pity suave | La tirada 60 ya lleva +7 %: p(n) = 0.006 + 0.07·(n − 59) desde n = 60. Así el promedio es 52.9 tiradas (1.89 %) y la destacada 66.1, los números del GDD. Con la otra lectura (+7 % desde la 61) sale 53.5 |
| "4 zonas de 10 nodos" | Cada zona tiene 10 pisos; una ruta pasa por 10 nodos. Los pisos 2 a 9 tienen tres carriles con bifurcaciones |
| Costo(n) | Almitas para subir del nivel n − 1 al n |
| Nivel mínimo por grado | Igual al nivel del Ascendente de su Juicio: C 40, B 50, A 60, S 70, SS 80 |
| Cobro del grado | Se exige poder pagar antes del Juicio, pero se cobra solo al ganarlo: perder no cuesta nada |
| "En seis turnos" | Seis rondas (ciclos de AV) |
| "Solo con brujas 3★" | Las aliadas son 3★ (la bruja del Juicio puede ser de cualquier rareza) |
| Mancha | Pasa a la siguiente run como nodo "Mancha de Sal" en la misma posición. Si esa run termina sin recuperarla (muerte, abandono o victoria por otro carril), el Fresno la absorbe y devuelve el 30 % como Savia |
| Abandonar | Cuenta como caer: las almitas sueltas quedan en una Mancha |
| Eco | No se gasta al usarlo; una run usa a lo más un Eco |
| Brujas caídas en la run | Tras una victoria vuelven con 10 % de vida |
| Exclusión con destacada ya obtenida | El banner se comporta como estándar (no hay 75/25) |
| Armas | Cada familia define el básico y, si tiene ecos, agrega golpes a la Cadena; el arma recién fabricada no tiene prefijo hasta reforjarla |
| Savia como ingrediente | Dos recetas de 14.2 piden "savia": se usa la Savia del Fresno (50 por preparación) |

## Huecos del GDD encontrados hasta ahora

- La curva de nivel cuesta 1.64 millones de almitas hasta el 70 y 2.29 millones hasta el 80, mientras que todos los grados juntos cuestan 447 392. La meta de "nivel 70 en siete días" exige unas 234 000 almitas diarias; el GDD no define ingresos diarios de almitas.
- La Sal Negra existe como recurso (11.1) y como asignación por combate (Fase 1, 2 por combate). No está definido si el Vislumbre gasta la del inventario.
- "Nuevo efecto de Hechizo" (grado B) y "mejora el Autohechizo" (grado A) dependen de cada kit; se aproximaron con +10 % al Hechizo y −20 % al costo de los Hilos.
- Solo 6 de las 24 brujas tienen kit de combate; las otras 18 se pueden obtener pero no pelean ni hacen Juicios.
- Los nombres de las 4★ y 3★ que el GDD y `docs/Retratos para Codex.md` no dan son propuestas (Grajilla, Madroña, Risca, Nacarina, Centella, Véspera, Ixbalam, Estrige, Migaja, Vellón, Púa, Charca). La Zarigüeya y el Topo de la Fase 1 son Muertita y Topacia en los retratos, con otro signo de Ascendente (Escorpio en lugar de Cáncer para Muertita).
- Las estadísticas de la Calculista (13.2) son de la Fase 3; los eventos del Fresno usan una base provisional de 10 más 2 por bruja afín.

## Valores // TUNING de la Fase 2

Todos están marcados en los JSON con `// TUNING`. Los más importantes para el balance: crecimiento de estadísticas por nivel (5 %), sal refinada por grado, vida y ataque del Ascendente en los Juicios, ATQ por nivel de arma (80 a 720), afinidad de planeta (+8 %), refinación (5 a 1), almitas por zona (1 500 a 4 000), multiplicadores de élite (×2.5) y jefe (×6), tope semanal (300 000 almitas) y su tasa a Savia (10 %), Savia por zona, costos de las Raíces, umbrales de la Ofrenda (50 000·n^1.5) y Polvo Astral por duplicado imposible (40 y 8).
