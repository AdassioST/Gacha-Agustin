# Fase 1 · El Aquelarre: núcleo de combate y magia

Prototipo jugable del combate de *Sal y Almas*. Toda la lógica vive en un núcleo de C# puro, determinista y probado. La presentación usa cápsulas y una interfaz IMGUI que solo consume eventos.

## Estado

| Parte | Estado |
| --- | --- |
| Núcleo, datos y pruebas | **Verificado**: 140 pruebas pasan con `dotnet test` |
| Presentación y script de Editor | **Compila** contra los ensamblados de referencia de Unity 2021.3 (como build de Editor y como build de jugador). **Falta abrirlo en Unity 6**: la sesión que lo construyó no tenía Unity |
| Pruebas EditMode | Las mismas pruebas del núcleo, más 4 que solo corren en Unity. Falta correrlas en el Test Runner |
| Balance del jefe | Simulado: el piloto automático gana el 100 % de los combates. El orden de la Cadena cambia el daño recibido casi al doble |

## Estructura

```
Assets/SalYAlmas/
  Core/                  núcleo (asmdef SalYAlmas.Core, noEngineReferences = true)
    Combat/              Battle (turnos, acciones, Cadena, daño, efectos, IA), AutoPilot, azar con semilla
    Magic/               planetas, Aspectos y Reacciones, pureza, bandas de Salinidad
    Chain/               ChainPlanner: la Partitura, el Spark y PreviewChain
    Data/                lector JSON, definiciones, reglas, validación
    Events/              eventos tipados y el formateador del registro LitRPG
  Data/                  rules, witches, enemies, encounters y potions (.json)
  Presentation/          escena: cápsulas, HUD, pantalla de Cadena, formación (asmdef SalYAlmas.Presentation)
  Editor/                menú Sal y Almas > Construir escena de combate (asmdef SalYAlmas.Editor)
  Tests/EditMode/        pruebas NUnit compartidas con dotnet; Unity/ tiene las que solo corren en Unity
Tests/
  Core.Lib/              compila el núcleo como .NET Standard 2.1 y C# 9, el perfil de Unity 6
  Core.Tests/            dotnet test: corre las pruebas compartidas y las de aislamiento del núcleo
  Core.Sim/              simulador de consola para registros, balance y tiempos
Packages/manifest.json   URP 17, Test Framework y módulos de Unity 6 LTS
```

## Cómo correr las pruebas

Requiere el SDK de .NET 8:

```bash
dotnet test Tests/Core.Tests
```

En Unity: **Window > General > Test Runner > EditMode > Run All**. Ahí corren las mismas pruebas del núcleo dentro del runtime de Unity, más las de `Tests/EditMode/Unity` (los JSON como TextAsset, el constructor de la escena y el determinismo en Mono).

## Cómo abrir la escena

1. En Unity Hub, **Add project from disk** con la raíz del repositorio. `ProjectSettings/ProjectVersion.txt` pide 6000.0.23f1; cualquier Unity 6000.0 LTS sirve.
2. Espera a que importe los paquetes y genere los `.meta`. **Haz commit de los `.meta` generados** para que los GUID queden fijos.
3. Menú **Sal y Almas > Construir escena de combate**. Activa URP si hace falta, crea los materiales por planeta y guarda `Assets/SalYAlmas/Scenes/Combate.unity`.
4. Play. También existe **Sal y Almas > Jugar combate**, que construye la escena si falta y la reproduce.

En la escena:

- **Formación**: hasta cuatro brujas, una Invitada, Vía Mayor o Menor por bruja, cinturón, encuentro, semilla y Día planetario.
- **Combate**: toca la placa o la cápsula de un enemigo para elegirlo como objetivo, y un retrato para elegir aliada (para pociones y Hechizos de apoyo). Las Ascensiones se lanzan desde cada retrato, también en la pausa antes del turno enemigo.
- **Cadena**: con 100 de Resonancia, **Abrir Cadena**. Elige y reordena; la Partitura muestra cada golpe como nota y los Spark previstos en dorado. Puedes guardar la Partitura para el modo automático.
- Arriba a la derecha están x1/x2 y Auto.
- **Final**: tiempo real, Cadenas jugadas contra la mejor posible, y tres opciones: **Ver repetición** (vuelve a jugar las mismas órdenes y comprueba que los eventos sean idénticos), **Jugar otra vez** y **Guardar registro** (texto con el registro y las órdenes en `Application.persistentDataPath`).

## Simulador

```bash
dotnet run --project Tests/Core.Sim -- log guardian 7      # registro LitRPG de un combate automático
dotnet run --project Tests/Core.Sim -- balance guardian 100 # estadísticas con cuatro estilos de juego
dotnet run --project Tests/Core.Sim -- chains              # Spark previstos de cada orden de Cadena
dotnet run --project Tests/Core.Sim -- tiempo guardian 50  # minutos estimados en la escena
```

## Balance actual (Guardián, equipo por defecto, 50 combates)

| Estilo | Victorias | Turnos de bruja | Vida final | Vida más baja de una bruja | Caídas |
| --- | --- | --- | --- | --- | --- |
| Mejor orden de Cadena | 100 % | 46 | 70 % | 39 % | 0 |
| Peor orden de Cadena | 100 % | 49 | 41 % | 15 % | 0 |
| Sin Cadenas | 100 % | 51 | 23 % | 1 % | 0.56 |
| Sin Cadenas ni Ascensiones | 0 % | 53 | 0 % | 0 % | 4 |

Tiempo estimado en la escena: 1.7 min solo de reproducción a x1. Con 7 s de decisión por turno y 30 s por Cadena salen unos 8.6 min, por debajo de los 15 de la prueba. La misma Cadena da de 0 a 8 Spark según el orden (de 0 a 10 con el Topo).

## Reglas que interpreté

Donde el GDD no es explícito, elegí esto. Todas se pueden cambiar.

| Tema | Decisión |
| --- | --- |
| Empate de AV | Primero las brujas, luego la de más Velocidad y al final la de menor posición |
| Avanzar o retrasar a quien está actuando | Se aplica cuando termina su turno, sobre el AV nuevo |
| "Turno sin lanzar magia" (−10) | Magia es Hechizo, Autohechizo, Vislumbre, Ascensión, un turno de Éxtasis o ser eslabón de una Cadena que abre ella |
| Bandas de Salinidad | No se acumulan: cada banda tiene solo el efecto de su fila (lectura literal de 7.4) |
| Daño "del Ascendente" | Hechizo (Golpe Astral), movimientos de Cadena, Ascensión y Éxtasis. El básico no cuenta |
| Éxtasis Abisal | Empieza al llegar a 150. Si ocurre en su propio turno, ese turno termina normal y el Ascendente toma los dos siguientes: usa su Cierre con daño ×2 contra un enemigo al azar. La Salinidad queda fija en 150. No obedece, no entra en Cadenas y no lanza Ascensiones |
| Desalada | Un turno sin magia: solo puede hacer el ataque básico |
| Reparto de Firmeza | Cada golpe quita la Firmeza de su movimiento repartida según *p* y 1 − *p*, solo por los planetas débiles |
| Quiebre | Dura hasta el siguiente turno del enemigo, cuando recupera toda su Firmeza |
| Unidad del Spark | Cada golpe que coincide es un Spark: +50 % a ese golpe, +2 de Ánima a su dueña, +1 almita y +10 de Resonancia (tope +20 por Cadena) |
| Conjunción "+25 % al siguiente eslabón" | Al eslabón que completa el par |
| Trígono "cura 5 %" | A las dos brujas del par |
| Cuadratura "+10 a quien lo provoca" | A la segunda bruja del par |
| Sextil | La segunda bruja avanza 15 % al terminar la Cadena |
| Tempestad | Dos golpes extra de la bruja que la provoca, a todos los enemigos, en los frames 10 y 24 de su eslabón. Salen en la Partitura |
| Espinas Rojas | Cada tick del sangrado cura a cada bruja el 30 % de ese daño |
| Abrir Cadena | Reemplaza la acción principal en cualquier momento del turno de una bruja; la que abre no tiene que estar en la Cadena |
| Ascensión "en cualquier momento" | En cualquier punto de decisión, también antes de cada turno enemigo. No gasta el turno |
| Ascensiones "al mismo tiempo" | Las que se lanzan en la misma ventana (sin turnos de por medio) se enciman y pueden hacer Spark entre sí; solo la segunda recibe el +50 % |
| Aspecto entre Ascensiones | Entre dos Ascensiones seguidas de brujas distintas en el mismo ciclo |
| Hilos | Cuesta el 10 % de la vida máxima y no puede matar. El turno extra no permite otros Hilos ni otra poción |
| Frenesí de Sal Negra | Gasta 1 Sal Negra. El +100 % de daño dura su turno y el −50 % de defensa dura hasta el final de su siguiente turno (si no, nunca coincidiría con un turno enemigo) |
| Vislumbre | Pureza y estadísticas del grado siguiente (ATQ y Velocidad), +15 % al Enlace y al Cierre |
| Regencia | Solo la aporta la Invitada, como dice 9.1 |
| Pociones | Una por turno de bruja. "El siguiente golpe" del Vinagre de Marte es la siguiente acción con golpes |
| Jefe | Actúa dos veces por turno, carga su ataque definitivo un turno antes y quebrarlo lo cancela |

## Valores // TUNING

Todos están marcados en el código o en los JSON. Los de reglas viven en `Data/rules.json`; una prueba verifica que ese archivo no cambie ningún valor que venga del GDD.

**Reglas (`rules.json`)**

| Valor | Propuesta |
| --- | --- |
| Ánima por eslabón de Cadena | 10 |
| Ánima por golpe recibido | 8 |
| Ánima y Resonancia por turno de Éxtasis | 30 y 8 |
| Salinidad por Ascensión | 0 |
| La Cristalización Turbia puede matar | No (deja 1 de vida) |
| Multiplicador de estadísticas por grado D, C, B, A, S, SS | 1.00, 1.05, 1.10, 1.16, 1.22, 1.28 |
| Bono del Vislumbre a Enlace y Cierre / Apoteosis | +15 % / +10 % de daño |
| Desfase de Ascensiones encimadas | 0 frames |
| Espinas Rojas | sangrado 0.6 × ATQ, 2 turnos |
| Tempestad | golpes en los frames 10 y 24, 0.5 × ATQ, 10 de Firmeza cada uno |
| Daños por turno del Quiebre | 2 turnos; Sol 0.5, Marte 0.8, Mercurio 0.4, Júpiter 0.4 × ATQ (salta 50 %) |
| Retraso extra del Quiebre | Luna +20 %, Mercurio +15 %, Saturno +35 % |
| Encanto | −40 % de daño al siguiente ataque |
| El Quiebre cancela la carga | Sí |
| Sal Negra del Frenesí | 1 |
| Crítico base | 5 % y +50 % de daño; enemigos 0 % |
| Sal Negra por combate | 2 |
| Límite de turnos | 400 |
| Duración de Leche de Luna | 2 turnos |
| Cargas por ranura del cinturón | 3 |

**Brujas (`witches.json`)**: todas las estadísticas salvo las de Marisal, que vienen de la ficha del Libro Mayor (13.1); todos los multiplicadores, Firmeza y frames de golpe; las Regencias; el nombre *Azogue Tonante* para la mezcla de Tormentina; el retraso del Hechizo de Selenia (12 %); y los signos de Ascendente de la Zarigüeya (Cáncer) y el Topo (Tauro).

**Enemigos (`enemies.json`)**: todo. El Guardián tiene nivel 52, 100 000 de vida, 1 600 de ATQ, 110 de Velocidad, 240 de Firmeza, débil a Sol, Marte y Júpiter, dos acciones por turno y segunda fase al 50 %.

## Contradicciones con el prompt o dentro del GDD

1. **Autohechizo de las 3★.** El prompt pide Autohechizo para las seis brujas, pero 12.1 dice que el kit de 4★ es "completo, con Autohechizo". La Zarigüeya y el Topo son 3★, así que **no tienen Autohechizo** (gana el GDD) y el validador lo exige.
2. **Ascensión III inalcanzable.** Requiere grado S (9.7) y las brujas de la fase son de grado B. El Vislumbre de B lleva a A, no a S, así que en la prueba con jugadores nadie verá la III. Está implementada y probada con un grado S forzado.
3. **Cuatro o cinco bandas.** La tabla 4.4 dice "medidor de 0 a 150 con cuatro bandas"; 7.4 y el prompt tienen cinco (el Éxtasis Abisal es una banda). Usé cinco.
4. **DEF sin uso.** La ficha del Libro Mayor (13.1) muestra "DEF 610", pero la fórmula de 9.8 solo usa niveles y R_DEF. Implementé la fórmula tal cual, así que la DEF no está en los datos.
5. **Nombres de mezcla.** 7.2 llama *Humo Óseo* a "Saturno con Luna", y 18.2 le da ese nombre a Selenia (Luna con Saturno). Concluí que el nombre es del par sin importar el orden. Entonces la Zarigüeya también es *Humo Óseo* y el Topo es *Escarcha Sangrienta*, como Marisal.

## Huecos del GDD

- **Dos Reacciones no aparecen con este elenco.** Los Aspectos usan el planeta dominante, y ninguna de las seis brujas tiene a Venus ni a Mercurio como dominante. Así, Espinas Rojas no puede ocurrir y Tempestad solo aparece si Solenne o Tormentina toman la Vía Menor (la formación permite elegirla). Las pruebas cubren las cuatro con brujas sintéticas.
- **La secta "variable" de Mercurio no importa.** Con la prioridad de 7.3, cualquier par con Mercurio es Conjunción, Oposición o Sextil; nunca llega a Trígono ni a Cuadratura.
- **Quién da la Regencia.** 9.1 la da a la Invitada, pero cada bruja tiene una. No se dice si la líder del equipo propio también aporta la suya.
- **El jefe contra cuatro brujas.** Con una sola acción por turno, el Guardián actuaba unas 8 veces en 60 turnos de bruja y no era una amenaza. Le di dos acciones por turno. El GDD no habla de acciones múltiples ni de resistencia de jefes a los retrasos.
- **Sin números.** No hay coeficientes para los daños por turno y los retrasos del Quiebre, Ánima por eslabón o por golpe recibido, efectos concretos de cada Ascensión y Regencia, duración de escudos y mejoras de pociones, ni crítico base.
- **Desalada y el kit descontrolado del Éxtasis** no tienen definición; ver la tabla de interpretaciones.
- **Granos de las Aperturas.** Las Apertura de Selenia, Solenne, Zarigüeya y Topo generan Granos ("su Apertura marca o genera Granos", 9.5), pero el GDD no dice cuántos; usé 1.
- **Unidad del Spark** (por golpe o por par) y **cómo se reparte la Firmeza** entre planetas: ver interpretaciones.

## Decisiones técnicas

- **Lector JSON propio.** El núcleo no puede usar `JsonUtility` (es de UnityEngine) ni paquetes externos. El lector propio acepta comentarios para marcar `// TUNING` y reporta errores con línea.
- **Azar propio** (xoshiro256** con SplitMix64) detrás de `IRandomSource`, para que la misma semilla dé lo mismo en .NET, Mono e IL2CPP.
- **IMGUI para la interfaz de prueba.** Se arma entera desde código y no depende de prefabs, TextMeshPro ni del Input System. Además pude compilarla aquí. Para la Fase 3 hay que pasar a uGUI o UI Toolkit con el estilo del Libro Mayor.
- **URP por reflexión** en el script de Editor: si el paquete cambia, el Editor sigue compilando y avisa en consola.
- **Sin `.meta`** en el repositorio todavía: los genera Unity la primera vez (ver "Cómo abrir la escena").

## Qué medir en la prueba con jugadores

- Tiempo hasta vencer al Guardián (sale en la pantalla final y en el registro guardado).
- Spark de cada Cadena contra la mejor posible: la pantalla final muestra las dos cifras.
- Si la persona reordena la Partitura antes de confirmar, y si guarda alguna.
- Si entiende la carga del Juicio del Fresno y la cancela (por Quiebre o con la Muerte Fingida de la Zarigüeya).
