# La Odisea: el anillo libre y el cielo que avanza

Propuesta para la Fase 3 (2026-11-05, Claude Code). Junta dos pedidos que parecían opuestos: el **GDD 6** pide empezar en la región de tu signo y avanzar en el sentido que quieras, y **el usuario** pidió empezar en lo sencillo, con enemigos de más nivel y biomas cada vez más extraños. Todo número es `// TUNING`.

## 1. La idea: separar el dónde del cuándo

> **El lugar lo eliges tú. El cielo avanza solo.**

- **El dónde es libre (GDD 6).** Empiezas en la región de tu signo y la Tortuga sigue el anillo en la dirección que elijas.
- **El cuándo es lineal (pedido del usuario).** El nivel de los enemigos, lo extraño del bioma y la capa de horror dependen de **cuántas regiones llevas**, no de cuál visitas.

Las dos cosas ya estaban en el GDD. El GDD 6 dice que las regiones no visitadas "se corrompen un poco más" con cada eclipse. Entonces la región que visitas tarde siempre está más corrompida que la primera, la visites en el orden que la visites. Y el GDD 6.5 numera sus capas por "regiones 1 a 4", "5 a 8" y "9 a 12". Con un inicio libre, esos números solo pueden ser **el orden en que las visitas**. Esta propuesta lee así las dos secciones y le da al eclipse un efecto en el juego, no solo en la historia.

Terraria funciona igual. El mapa no cambia: lo que cambia es el mundo después de cada hito. Al vencer al Muro de Carne, el modo difícil esparce el Sagrado y la Corrupción por todos los biomas, y los enemigos de todas partes se vuelven más fuertes. Aquí el hito es el eclipse.

## 2. El dónde: el anillo de la Tortuga

| Regla | Detalle |
| --- | --- |
| Inicio | El prólogo pasa en el vado del Río Lácteo de tu signo. Al terminarlo, la Tortuga atraca en tu región |
| Dos sentidos | **Directo** (en el orden del zodiaco: Aries → Tauro → Géminis…) o **Retrógrado** (al revés). Son los nombres de la astrología para el movimiento de los planetas |
| El arco | Las regiones visitadas forman un arco continuo alrededor de tu signo. Una región nueva se abre si es vecina de una terminada |
| Dos frentes | El arco tiene dos puntas, así que puede haber dos regiones nuevas a medias, una por sentido. Si una te atora, sigues por la otra. Nunca te quedas sin camino |
| Volver | Puedes regresar a cualquier región del arco: Expediciones, zona EX y la salina del día. Una misión a medias queda en pausa mientras haces una Expedición |
| Regiones sin construir | Mientras no estén las 12, el anillo salta a la siguiente región construida. Las que no existen siguen corrompiéndose y, cuando salgan en una versión, entran al nivel que le toca a cada quien |
| El Descendente | La región opuesta a tu signo está siempre a seis pasos, así que **la visitas siempre en la Grieta o el Abismo**. Su Casa es tu rival personal: manda agentes a otras regiones desde el principio y la enfrentas tarde, en su versión extraña |

Pares opuestos: Aries y Libra, Tauro y Escorpio, Géminis y Sagitario, Cáncer y Capricornio, Leo y Acuario, Virgo y Piscis.

## 3. El cuándo: la Marea de Sal

La **Marea de Sal** es el nivel del mundo. La propuesta de meta-juego ya la tenía, de 0 a 5 (MP-10). Aquí la mueven los eclipses del GDD.

- Cada **dos regiones terminadas** hay un eclipse (GDD 6) y la Marea sube 1. Con 12 regiones hay cinco eclipses en el camino (Marea 0 a 5) y un sexto al cerrar el anillo, que abre la Copa Astral.
- Cuando entras por primera vez a una región, **toma la Marea de ese momento y se queda así**. Es la regla del GDD: lo visitado ya no se corrompe.
- *n* es el número de regiones terminadas cuando llegas. Tu región de signo es *n* = 0.

| Regiones terminadas al llegar (*n*) | Marea | Capa (GDD 6.5) | Nivel de la región | Qué agrega el cielo |
| --- | --- | --- | --- | --- |
| 0 y 1 | 0 | Superficie | 5 a 17 | Solo los nativos del bioma |
| 2 y 3 | 1 | Superficie | 17 a 29 | Nativos con un rasgo de eclipse |
| 4 y 5 | 2 | Grieta | 29 a 41 | Llegan los **Cristalizados**; estrellas sin nombre |
| 6 y 7 | 3 | Grieta | 41 a 53 | Cristalizados en élite; el Ascendente de la Casa abre su ventana |
| 8 y 9 | 4 | Abismo | 53 a 65 | Llegan **los Sin Carta**; el cielo demasiado cerca |
| 10 y 11 | 5 | Abismo | 65 a 77 | **Huéspedes menores**; la Casa ya fue cosechada |
| Copa Astral | 6 | Abismo | 77 a 80 | La Jardinera y los Huéspedes |

**Nivel base de la región:** `5 + 6·n`. Las zonas suman 0, +2 y +4, el jefe +6 y la zona EX +8. Así la curva es igual a la del orden fijo que proponía el traspaso: sube parejo, sea cual sea el camino.

**El Fresno se alinea con la Marea:** su primera zona tiene enemigos de nivel 52, y esa es la Marea 4. Propuesta: el Fresno se abre en el cuarto eclipse (8 regiones) o con un Ascenso corto de prueba antes. Hasta que haya 8 regiones construidas, su primera zona escala con la Marea más alta disponible.

### Expediciones con Marea elegible

En una región terminada puedes repetir zonas como **Expedición** (solo combates y botín, sin escenas). Eliges la Marea, desde la que tenía al llegar hasta la Marea actual. Tu región de inicio también sirve para farmear al nivel 70. Además, la propuesta de meta-juego pedía poder bajar la Marea: aquí bajarla es elegir una más baja en la Expedición.

## 4. Dentro de una región: Brave Frontier hacia abajo, EX hacia arriba

```
Región (Casa, bioma, folio del Libro Mayor)
  Zona 1  La orilla      (superficie del bioma)        nivel base
  Zona 2  El subsuelo    (minas, raíces, cuevas)       +2
  Zona 3  El corazón     (la sede de la Casa)          +4, jefe +6
  Zona EX El cielo       (islas flotantes, la ventana) +8, una capa más extraña
    Misión: escenas de historia al inicio y al final
      Secciones: 3 a 5 combates seguidos; la vida y la Salinidad se arrastran
        Entre secciones: botín, tesoro o un evento corto
          Jefe
```

- Las zonas **bajan** como las capas de Terraria: superficie, subsuelo, cavernas. Abajo están los muertos.
- La zona EX **sube**: islas flotantes y el cielo demasiado cerca. Arriba está lo que se los come. Siempre muestra **una capa más** que la región. Por eso una región de Superficie deja ver la Grieta en su EX, y sirve de anzuelo.
- Ganar una sección no regresa al mapa: se ve el botín y se sigue. Las caídas vuelven con 10 % de vida a la siguiente sección. Perder repite la sección sin costo y sin perder el botín (GDD 11.2).
- No hay energía (GDD 16.4). El botín diario de Expedición baja a ×0.25 después de 20 secciones.

## 5. Enemigos: las mecánicas se suman con el tiempo

Cada región tiene un **nativo** que enseña una sola mecánica y aparece en todas sus capas. Las capas no cambian a los nativos: les **suman** invasores y rasgos compartidos. Al principio aprendes una regla por región. Al final las mezclas.

| Región | Nativo | Mecánica | Cómo se le gana |
| --- | --- | --- | --- |
| Aries | Soldada de Crisol | Sube su ataque cada turno que sigue viva | Matarla pronto |
| Tauro | Molinero de Sal Dulce | Roba Granos | Matarlo primero |
| Géminis | Doble de Azogue | Devuelve parte del daño | Pegar con pocos golpes grandes |
| Cáncer | Cangrejo Salinero | Protege a sus compañeros mientras viva | Romperlo antes que a los demás |
| Leo | Becerro Solar | Se enfurece si le lanzan Hechizos | Cadena y ataques básicos |
| Virgo | Novicia sin Alma | Cura a las demás | Prioridad de objetivo |
| Libra | Tesorera de Almas | Te quita almitas del botín si el combate se alarga | Terminar rápido |
| Escorpio | Fatua del Azufre | Atrae los ataques de un solo objetivo | Daño en área |
| Sagitario | Cría de Estrella | Explota al caer | Ordenar quién cae primero |
| Capricornio | Boca Vieja | Retrasa a todo el equipo cada dos turnos | Quebrarla antes de que cargue |
| Acuario | Cantora Sumergida | Sube la Salinidad de la bruja más limpia | Autohechizo y descanso |
| Piscis | Ninfa Hospitalaria | Cura a las brujas pero les quita Resonancia | Terminar antes de que pese |

| Capa | Invasor | Rasgo |
| --- | --- | --- |
| Marea 1 | Rasgo de eclipse en los nativos | +10 % de vida y una pista nueva en la placa |
| Grieta | **Cristalizados** (GDD 6.5) | Firmeza alta; al caer sueltan Salinidad a todas |
| Abismo | **Los Sin Carta** (GDD 6.5) | Los Aspectos no les afectan |
| Abismo y Copa | **Huésped menor** | Cambia de debilidad por fase; hay que leer la placa |

Todo enemigo lleva una pista en su placa (`Traits.Hint`). Las formas cristalizada y abisal de cada nativo se generan con una plantilla, no a mano.

## 6. La historia: un hilo fijo y cuentas sueltas

El orden de las regiones cambia, pero la trama principal se cuenta igual para todos.

- **El hilo fijo vive en la Tortuga.** Cada eclipse trae un **Interludio** en las Termas, que siempre son seguras: seis escenas en orden fijo que llevan la revelación de Superficie a Grieta y Abismo.
- **El Libro Mayor se ordena solo.** Cada Casa guarda un folio arrancado. Como la Calculista no está escrita en el libro, el libro se reordena para ella: el tercer folio que recuperas es siempre la página 3, sin importar de qué región salió. Así la Cuenta de la Cosecha se lee en orden en cualquier ruta.
- **Las regiones son cuentas sueltas del collar.** Cada episodio regional se entiende solo. Las marcas `primera_<región>` y `despues_<región>` y lo que hiciste antes (Ferrán perdonada vuelve en otra región) le dan continuidad.
- **La capa cambia lo que ves, no lo que pasa.** La misión es la misma en las tres capas. Cambian el nivel, los invasores, el cielo y tres textos de bisagra: la llegada, la revelación del jefe y el final local (las variantes de GDD 6 y HUECOS H-16).
- **Paralaje y Marea son cosas distintas.** La Marea es lo que el mundo *es*: lo mueve el eclipse y no se baja. El Paralaje es lo que tú *ves*: lo suben los encuentros cósmicos y lo baja el baño. Se suman: Superficie con Paralaje alto deja ver grietas antes de tiempo.

Costo de contenido por región: un esqueleto de misiones, los textos de bisagra en tres capas (unos 15 textos cortos) y las formas de enemigos generadas.

## 7. Cómo conecta con todo lo demás

| Sistema | Qué recibe de la Odisea | Qué le da a la Odisea |
| --- | --- | --- |
| Termas (gacha) | Sales de Baño por zona, estrellas, región e Interludio | Brujas nuevas, que abren chequeos y variantes de historia |
| Reclutamiento | Una 3★ por región (GDD 18.4) | Equipo para el siguiente frente |
| Yunque y Altar | El material regional de cada sección; la estación regional al terminar la región | Armas de su familia, que son el mejor básico contra el nativo |
| Caldero | Reactivos de Expedición y tesoros | Pociones para el cinturón, que se arrastra entre secciones |
| Niveles y grados | Almitas por sección, que escalan con el nivel de la región | Poder para la siguiente Marea |
| Juicios | Sal de la salina regional y tesoros de la EX | Grados, que piden el nivel de la Marea siguiente |
| Fresno | Se abre en la Marea 4 | Savia y Raíces, que ayudan en el Abismo |
| Encargos y logros | Secciones, jefes, estrellas y regiones cuentan | Sales de Baño, almitas y Crecimiento |
| Altura (cuenta) | Crecimiento por todo lo jugado | Desbloqueos: automático, Expedición, Fresno |
| Calculista | Experiencia de Sendero, Títulos y marcas | Chequeos, que abren atajos y tesoros entre secciones |

## 8. Reglas para el código

| Pieza | Cambio |
| --- | --- |
| `StoryRules` | `RegionLevel(n)`, `TideOf(n)`, `LayerOf(tide)`. `MaxCorruption` pasa a 5: la corrupción **es** la Marea |
| `RegionDef` | `Zones` (orilla, subsuelo, corazón, EX) con las misiones de cada una, el nativo, el material y la recompensa de zona |
| `CalculistaState` | `ArrivalTide` por región; misión en curso con sección, vida, Salinidad y caídas |
| `StoryService.CanStart` | Una región nueva se abre si es vecina en el anillo construido de una región terminada, o si es la de tu signo |
| `StoryService.PrepareBattle` | Nivel por región, zona y sección. **Ya no escala con el equipo.** Usa `StartHp` y `StartSalinity` |
| Pruebas que cambiaron | "Dos regiones en cualquier orden" pasó a "el anillo se abre en los dos sentidos desde tu signo". El eclipse que corrompe lo no visitado y el signo que da bono siguen igual |

## 9. Qué sale primero

1. **Lanzamiento con las cuatro cardinales** (D-13): Aries, Cáncer, Libra y Capricornio, una por elemento. Tu signo te lleva a la de su elemento. En el anillo construido quedan una junto a otra, así que tu primera vecina tiene Marea 0 y las dos siguientes Marea 1. Todo el lanzamiento queda en Superficie, como pide el GDD 6.5 para las regiones 1 a 4.
2. **Versión 2:** las cuatro fijas (Tauro, Leo, Escorpio, Acuario). Para quien ya terminó las cuatro primeras, entran en Grieta. Para quien llega nuevo, en Superficie.
3. **Versión 3:** las cuatro mutables (Géminis, Virgo, Sagitario, Piscis) y la Copa Astral.

Cardinal, fijo y mutable es la otra división de la astrología: lo que empieza, lo que sostiene y lo que se disuelve. Encaja con salir por oleadas, aunque cada jugadora las vea en la capa que le toque.

## 10. Lo que se pierde y cómo se compensa

| Se pierde | Compensación |
| --- | --- |
| Una secuencia de biomas fija (bosque → océano → …) | Cada región baja de la superficie a lo profundo, y la Marea dobla todos los biomas hacia lo extraño |
| Escribir cada región para un solo nivel | Esqueleto común y textos de bisagra por capa; enemigos generados |
| Un villano final de región único | El Descendente es un rival personal que siempre llega tarde |

## 11. Texto propuesto para el GDD 6

Reemplaza el párrafo "Estructura no lineal" cuando el usuario lo apruebe:

> **Estructura:** el jugador empieza en la región de su signo y la Tortuga sigue el anillo en sentido Directo o Retrógrado. Las regiones visitadas forman un arco, y la siguiente se abre al terminar una vecina. Cada dos regiones hay un **eclipse** que sube la **Marea de Sal**. Las regiones no visitadas toman la Marea del momento en que llegas: su nivel, sus enemigos, sus eventos y su final local. Por eso la odisea va de lo sencillo a lo extraño en cualquier orden. El reloj de eclipses nunca bloquea al jugador ni le hace perder progreso. Las capas de la sección 6.5 se cuentan por el orden de visita.

## 12. Estado de la implementación (2026-11-05)

| Pieza | Estado |
| --- | --- |
| Anillo, Marea, nivel por llegada, cuatro zonas, secciones, botín, Expediciones, invasores | Hecho en el núcleo, con pruebas (`OdysseyTests`) |
| Cuatro regiones cardinales | Aries y Capricornio con enemigos regionales; Cáncer y Libra nuevas, con guion en borrador |
| Interludios del eclipse, folios reordenados y el Descendente | Diseño. Falta escribirlos: el núcleo ya marca el eclipse y sabe cuál es tu Descendente |
| Textos de bisagra por capa | Algunos, con `minCorruption`. Faltan los del Abismo |
| Huésped menor que cambia de debilidad | Falta: hoy se pliega y devuelve daño |
| Fresno alineado con la Marea | Falta: su primera zona sigue en nivel 52 |

Valores con que quedó (`Data/odyssey.json`, generado por `Tools/Content/gen_odyssey.py`): nivel `5 + 6·n`, zonas +0, +2, +4 y +8, jefe +2, fuerza de sección 0.9, almitas por sección `100 · nivel^1.5`, material al 60 %, sal al 35 %, tesoro al 20 %, 100 Sales por zona, 20 secciones diarias de Expedición con botín completo y ×0.25 después.

Balance medido con `dotnet run --project Tests/Core.Sim -- historia 3`: las ocho rutas (cuatro signos, dos sentidos) terminan las cuatro regiones sin Expediciones extra. El equipo queda de 1 a 4 niveles debajo de cada región. Una cuenta nueva gana unas 4 000 Sales de Baño (25 tiradas) sin contar los diarios, 90 materiales regionales y llega a Altura 14.
