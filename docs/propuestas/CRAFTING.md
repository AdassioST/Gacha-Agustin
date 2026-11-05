# Propuesta de crafting: armas, reforja, estaciones y Salmueras

Propuesta por aprobar · 5 de noviembre de 2026 · amplía las secciones 14 y 15 del GDD

**Estado.** Es una propuesta. Solo cuenta cuando el GDD la cite o la apruebes (`CLAUDE.md`). Mientras tanto, es la guía para llenar los huecos H-05, H-06 y H-07 de `docs/HUECOS.md`, y en parte H-01, H-11 y C-22. Todo número que no viene del GDD es `// TUNING`.

**Qué amplía.**

| Sección del GDD | Qué agrega este documento |
| --- | --- |
| 9.5 y 9.6, Cadena y Spark | Cómo pone el arma los frames de la Cadena, la rejilla de 2 frames y la matriz de Spark entre familias |
| 11, Economía | Costos de crafting, refinación y reforja, y una escala de almitas coherente |
| 12.6, Arma | Qué arma usa cada bruja |
| 14, Salmueras | Recetas en tres potencias, bases, reactivos, Salmuera Turbia y Recetario |
| 15, Armas | Patrones de golpe, 120 armas, 8 legendarias, 15 estaciones y 25 prefijos |

**Cómo leer las tablas.**

- Los frames son a 60 fps y cuentan desde el inicio del movimiento. En la Cadena cuentan desde el inicio del eslabón; el eslabón *n* empieza 24·(*n*−1) frames después del primero (GDD 9.5).
- **Mano** son los golpes de la bruja con su arma. **Voz** son los golpes de su Ascendente. Ver la sección 1.1.
- Los IDs van en `snake_case`, sin acentos, y son los mismos en datos, arte y localización. Las armas usan `familia_nivel_variante`, por ejemplo `daga_rustico_a`.
- Las almitas están en la escala que propongo en la sección 11 (nivel con `40·n^1.5`). Si se queda la escala actual del GDD, multiplica todas las almitas de este documento por 2.5.
- "Sal del sello" es la sal del planeta del sello del arma: una Daga con sello de Marte pide Sal Férrea.
- Las estaciones regionales se instalan en la Tortuga al terminar el capítulo de su región. "Dos estaciones" significa que las dos deben estar instaladas.

## Las decisiones en una página

1. **El arma es la mano; el Ascendente, la voz.** El arma pone todos los golpes del ataque básico y los golpes de mano de la Apertura, el Enlace y el Cierre. El Hechizo y la voz son de la bruja. Resumen: el planeta decide el Aspecto, el arma decide el Spark y la voz es la firma de la bruja.
2. **Rejilla de 2 frames.** Todo frame de golpe es par, porque las animaciones se hacen a 30 fps. Hay Spark si dos notas caen en la misma columna de 2 frames o en columnas vecinas.
3. **Doce patrones equilibrados.** De 144 pares ordenados de familias, 48 hacen Spark natural en eslabones seguidos (33 %). Cada familia tiene entre 5 y 11 pares, y su índice de equipo va de 330 a 445. El Grimorio es el mejor para Spark sin dominar.
4. **El orden manda.** Las mismas cuatro armas dan 9 golpes con Spark en un orden y 0 en el orden inverso. Las familias tempranas buscan ir después y las tardías antes.
5. **El nivel sube números, nunca frames.** Patrón, reparto, rasgo y animación son iguales en los cinco niveles y en las dos variantes. La variante cambia el sello planetario: una pasiva pequeña que vale el doble si coincide con el planeta de la bruja.
6. **Cualquier bruja usa cualquier familia.** Cada una tiene una familia afín que da +25 % a la secundaria del arma.
7. **Una estación regional por familia.** Las 12 regiones del GDD tienen su estación; cada una abre los niveles Alquímico a Quintaesencia de una familia y una función extra. 57 armas piden dos estaciones.
8. **El Péndulo de Resonancia cuenta entre las 120.** Es la Campana Alquímica *b*, hecha con la fusión del GDD 15.3. Las 8 legendarias son laterales: mismo ATQ que Quintaesencia y un efecto propio.
9. **Reforja sin frustración y sin tienda.** 25 prefijos, solo Rítmico y Pausado mueven frames (±2), el prefijo se conserva al mejorar, el Yunque recuerda los últimos prefijos y el Temple garantiza Legendario a las 25 reforjas.
10. **Salmueras en tres potencias.** Los números del GDD 14.2 son la potencia mejorada. 11 reactivos, 8 efectos de Salmuera Turbia y un Recetario que se llena jugando, sin wiki.
11. **Una escala de almitas coherente.** Con `40·n^1.5` para el nivel, nivel, grados y arma pesan 2 : 1 : 1. Un arma de Rústico a Quintaesencia cuesta 416 300 almitas, unos 4.5 días de juego.

## 1. Reglas de diseño del catálogo

### 1.1 El arma es la mano; el Ascendente, la voz

Cada movimiento tiene hasta dos capas de golpes.

| Acción | Mano (el arma) | Voz (el Ascendente) | ¿Puede hacer Spark? |
| --- | --- | --- | --- |
| Ataque básico | Todos los golpes, con el patrón de la familia | Ninguno | No: en un turno normal nadie más golpea |
| Hechizo | Ninguno | El Golpe Astral, con los frames del kit (GDD 8.2) | No, por la misma razón |
| Apertura y Enlace | Patrón de Cadena de la familia, 70 % del daño | 1 golpe del kit, 30 % del daño | Sí |
| Cierre | Patrón de Cadena de la familia, 40 % del daño | 1 o 2 golpes del kit, 60 % del daño | Sí |
| Ascensión | Ninguno | Toda la cinemática | Solo con otra Ascensión (GDD 9.6) |

**Decisión.** El arma reemplaza los frames del ataque básico y pone la capa de mano de los tres movimientos de Cadena. No toca el Hechizo ni la voz. Tampoco desplaza los golpes de la bruja ni suma golpes encima: la mano de cada eslabón es siempre el patrón de su familia.

**Por qué.**

- **El ataque básico nunca hace Spark.** Por turnos, solo una bruja actúa a la vez. Si el arma cambiara solo el básico, como sugiere el GDD 19.2, el crafting no tocaría el Spark y no se cumplirían el GDD 9.6 ni el 15. Por eso el patrón también vive en la Cadena.
- **Reemplazar todo borraría a la bruja.** Si el arma decidiera todos los golpes de la Cadena, cuatro brujas con Báculo serían idénticas en la Partitura. La voz conserva la firma de cada una.
- **Solo desplazar es pobre.** Un desplazamiento es un número; un patrón es una forma. Con desplazamientos, las familias se distinguirían por un solo valor y el acertijo se volvería una resta.
- **Solo sumar golpes satura.** Los movimientos de la Fase 1 traen de 2 a 5 golpes. Con un arma encima, un eslabón llegaría a 10 golpes y casi todo haría Spark. Con mano y voz quedan de 2 a 7 golpes por eslabón.
- **Producción.** El GDD 15.5 pide una animación por familia: es la mano. El Ascendente ya tiene su animación por bruja: es la voz.

**Datos de la Fase 1.** Sus movimientos de Cadena traen todos los golpes juntos. En la Fase 2B, el último golpe de la Apertura y del Enlace, y los dos últimos del Cierre, pasan a ser la voz. Los demás se borran, porque ahora los pone el arma. Las partes de la voz se renormalizan para sumar 1. Ejemplo: el Enlace de Marisal, `[2, 22, 46]`, queda con la voz en 46. El diseñador de kits puede mover la voz después.

**Dónde poner la voz.** En frames pares entre 40 y 90, después de casi toda la mano. Así la Partitura se lee como "arma y luego Ascendente", y la voz cae donde golpean los eslabones vecinos, que es donde puede darle a cada bruja sus propios Spark.

**Prefijos y Sendero.** Rítmico y Pausado mueven todos los golpes de la bruja en la Cadena, mano y voz: es el `FrameShift` por bruja que ya tiene la Fase 1. La Directora de Coro mueve el eslabón entero. El Incensario es la única familia que mueve la voz (sección 2.2). Las Reacciones Híbridas, como la Tempestad, siguen sumando sus golpes como en la Fase 1.

### 1.2 La rejilla de 2 frames

Todos los frames de golpe son pares. Las animaciones se hacen a 30 fps y se reproducen a 60 (GDD 19.5), así que cada pose dura 2 frames. Un golpe en frame impar caería entre dos poses. La separación entre eslabones (24), los prefijos (±2) y los datos de la Fase 1 ya son pares.

Consecuencia: la distancia entre dos golpes siempre es par, y **hay Spark si la distancia es 0 o 2**. La Partitura se puede dibujar en columnas de 2 frames: dos notas en la misma columna o en columnas vecinas hacen Spark.

Reglas para el validador de datos:

- Todo frame de golpe es par y cabe en el eslabón (108 frames como máximo, GDD 9.5).
- La Directora de Coro mueve en pasos de 2, de −6 a +6.
- El patrón de Cadena tiene los mismos intervalos y el mismo reparto que el ataque básico (sección 2.1).

### 1.3 Qué cambia con el nivel

El nivel sube números, nunca frames.

- Suben el ATQ del arma, su secundaria y su sello (secciones 3 y 4).
- No cambian el patrón, el reparto, el rasgo de familia ni la animación. Son iguales en los cinco niveles y en las dos variantes.
- Así, un Spark que el jugador resolvió con armas Rústicas sigue funcionando cuando las mejora. Lo único que mueve frames desde el crafting son los prefijos Rítmico y Pausado.

### 1.4 Quién usa qué arma

- Cualquier bruja usa cualquier familia, sin importar rareza ni papel. Lleva una sola arma.
- El arma se cambia libremente fuera de combate.
- Cada bruja tiene una **familia afín** en su ficha. Con ella, la secundaria del arma vale 25 % más.
- Toda bruja llega con una Rústica de su familia afín. Nunca hay bruja sin arma.
- La Invitada usa el arma que su dueña tenía equipada. Una Invitada NPC usa la de sus datos.

Familias afines propuestas. Las demás van en la ficha de cada kit (Fase 6).

| Bruja | Familia afín | Por qué |
| --- | --- | --- |
| Marisal | Guadaña de Sal | Exguardia de minas de Aries; la Fragua de Hierro Rojo está en su tierra |
| Selenia de Plata | Tijeras de Hilandera | Su Autohechizo es Hilos |
| Solenne | Rifle Alquímico | Comerciante del Desierto Áureo; la legendaria del Rifle es de Sol |
| Cervanda | Báculo | Guardiana y Sabia de Saturno; el báculo es su vara de corte |
| Tormentina | Escoba | Mensajera hiperactiva; pura velocidad |
| Cupria | Caldero de mano | Herbolaria; el Caldero potencia las Salmueras |
| Muertita | Incensario encadenado | Incienso de funeral para la que se hace la muerta |
| Topacia | Varita | Su ritual son dos puñados de granos de sal; la Varita genera Granos |

### 1.5 Cómo se evita que una familia domine el Spark

1. **Mismo daño total.** Todas las familias tienen el mismo ATQ por nivel, y el patrón solo reparte el daño del movimiento. Más golpes no es más daño: es más oportunidades de Spark, cada una más chica.
2. **El Spark es relativo.** Una familia hace Spark con ciertas vecinas, no con todas. Ninguna pasa de 11 de sus 24 pares posibles (sección 2.3).
3. **Frecuencia contra tamaño.** El Grimorio hace Spark con más vecinas, pero cada aguja lleva 20 % de su capa. La Varita hace Spark con pocas, pero cuando lo logra toda su capa gana +50 %.
4. **La posición importa.** Las familias tempranas hacen Spark sobre todo como segunda de un par y las tardías como primera. Las centrales sirven en cualquier lugar, pero ninguna pasa de 6 pares en una misma posición; las especialistas llegan a 7 u 8 en la suya.
5. **Topes.** La Resonancia por Spark ya tiene tope de +20 por Cadena (GDD 9.6). Agrego uno: el Ánima por Spark tiene tope de +10 por bruja y Cadena, para que los golpes pequeños no aceleren las Ascensiones.
6. **Contrapeso explícito.** El Báculo gana +15 % si no hace Spark. Es la familia estable que pide el GDD.
7. **Reglas que rotan.** Un Tránsito de cada tres puede dar +10 % de daño de Spark a dos familias, como hacen el Día planetario y las Ramas con los planetas (GDD 17.3).
8. **Prueba automática.** El simulador de la Fase 1.5 corre los 144 pares en cada build y falla si una familia supera en más de 20 % la mediana del índice de equipo.

## 2. Las doce familias

### 2.1 Patrones de golpes

| Familia | ID | Ataque básico | Cadena: golpes de mano | Entrada | Reparto del daño (%) |
| --- | --- | --- | --- | --: | --- |
| Báculo | `baculo` | 18, 28, 38 | 12, 22, 32 | −6 | 30 / 30 / 40 |
| Grimorio de Agujas | `grimorio` | 14, 20, 26, 32, 38 | 18, 24, 30, 36, 42 | +4 | 20 / 20 / 20 / 20 / 20 |
| Guadaña de Sal | `guadana` | 16, 42 | 26, 52 | +10 | 45 / 55 |
| Incensario encadenado | `incensario` | 20, 30, 46 | 36, 46, 62 | +16 | 25 / 30 / 45 |
| Astrolabio | `astrolabio` | 16, 46 | 22, 52 | +6 | 85 / 15 |
| Rifle Alquímico | `rifle` | 30, 38 | 44, 52 | +14 | 65 / 35 |
| Escoba | `escoba` | 20, 24, 28 | 8, 12, 16 | −12 | 33 / 33 / 34 |
| Campana de Bronce | `campana` | 12, 42 | 6, 36 | −6 | 30 / 70 |
| Tijeras de Hilandera | `tijeras` | 20, 24 | 30, 34 | +10 | 50 / 50 |
| Caldero de mano | `caldero_mano` | 14, 24, 34, 44 | 24, 34, 44, 54 | +10 | 20 / 25 / 25 / 30 |
| Varita | `varita` | 12 | 28 | +16 | 100 |
| Daga Ritual | `daga` | 12, 18, 24, 30 | 14, 20, 26, 32 | +2 | 25 / 25 / 25 / 25 |

- **Ataque básico:** frames del clip completo, que dura de 60 a 72 frames (GDD 19.2).
- **Cadena:** es el mismo clip sin la anticipación y con una entrada: la bruja espera su momento dentro del eslabón. Los intervalos y el reparto son idénticos al básico. **Entrada** es la diferencia de frames entre los dos.
- **Reparto:** porcentaje de la mano que lleva cada golpe. En el básico es todo el daño; en la Cadena, la parte de mano del movimiento (70 % o 40 %, sección 1.1).
- El Báculo conserva en la Cadena el patrón de referencia: 12, 22 y 32.

### 2.2 Identidad de cada familia

Cada familia cubre un papel de combate y hay dos por papel. El rasgo de familia es igual en los cinco niveles.

| Familia | Fortaleza (GDD 15.1) | Secundaria | Papel que favorece | Rasgo de familia |
| --- | --- | --- | --- | --- |
| Báculo | Estable, para empezar | Vida % | Guardiana | **Constancia:** si ninguno de sus golpes de mano hace Spark en una Cadena, esos golpes hacen +15 % |
| Grimorio de Agujas | El mejor para Spark | Ánima ganada % | Tejedora | **Costura:** si tres o más de sus agujas hacen Spark, la siguiente bruja de la Cadena gana +10 de Ánima |
| Guadaña de Sal | Daño en área | Ruptura de Firmeza % | Rompedora | **Siega:** sus dos golpes también dan a los demás enemigos, con 40 % del daño |
| Incensario encadenado | Mueve los frames hacia atrás | Curación % | Sanadora | **Humo lento:** la voz de su bruja cae 4 frames después |
| Astrolabio | Potencia los Aspectos | ATQ % | Tejedora | **Marca de Aspecto:** el Aspecto que su eslabón forma con el siguiente vale 25 % más; se suma al +50 % de la Astróloga |
| Rifle Alquímico | Penetración de resistencia | Penetración % | Ofensiva | **Tiro de gracia:** +20 % de daño contra enemigos Quebrados |
| Escoba | Velocidad | VEL | Rompedora | **Viento de cola:** su bruja empieza cada combate con 15 % de acción adelantada |
| Campana de Bronce | Retrasos | Potencia de retraso % | Saboteadora | **Eco diferido:** su segundo golpe retrasa 8 % al objetivo, o 15 % si hace Spark |
| Tijeras de Hilandera | Mejora el Autohechizo de Hilos | Daño crítico % | Saboteadora | **Hilo corto:** Hilos cuesta 5 % de vida en vez de 10 %, y su turno extra hace +15 %. Con cualquier bruja: si sus dos golpes hacen Spark, quitan una mejora al enemigo |
| Caldero de mano | Potencia las Salmueras | Potencia de Salmuera % | Sanadora | **Salpicadura:** una Salmuera de un solo objetivo que use su bruja salpica a la aliada con menos vida, al 50 % |
| Varita | Genera Granos | Reducción de daño % | Guardiana | **Chispa:** el primer ataque básico de su bruja en cada ronda da +1 Grano extra; máximo +1 por ronda en todo el equipo |
| Daga Ritual | Crítico | Prob. de crítico % | Ofensiva | **Cuarto filo:** su cuarto golpe tiene +20 % de probabilidad de crítico |

Notas:

- "Ronda" es un ciclo de AV, como propone HUECOS C-11.
- Uso "Reducción de daño" y no "DEF" porque la fórmula de daño del GDD 9.8 no tiene estadística de defensa (ver la sección 15).
- Leo "mueve los frames hacia atrás" (GDD 15.1) como atrasarlos: los golpes en arco del Incensario llegan tarde y arrastran a la voz.

### 2.3 Matriz de Spark entre familias

Fila: la familia del eslabón *n*, primera del par. Columna: la del eslabón *n*+1, que empieza 24 frames después. Solo cuenta la mano; la voz de cada bruja suma Spark aparte.

- **✦** Spark natural: algún golpe cae a 2 frames o menos.
- **◇** Spark con un prefijo: Rítmico o Pausado en una de las dos.
- **○** Spark a 4 o 6 frames: con la Directora de Coro, o con Rítmico en una y Pausado en la otra (4).
- **·** Sin Spark de mano ni con 6 frames de ajuste.

| 1.ª ↓ · 2.ª → | Báculo | Grimorio | Guadaña | Incensario | Astrolabio | Rifle | Escoba | Campana | Tijeras | Caldero | Varita | Daga | ✦ como 1.ª |
| --- | :-: | :-: | :-: | :-: | :-: | :-: | :-: | :-: | :-: | :-: | :-: | :-: | :-: |
| **Báculo** | ◇ | · | · | · | · | · | ✦ | ✦ | · | · | · | ○ | 2 |
| **Grimorio** | ✦ | ✦ | ○ | · | ◇ | · | ✦ | ✦ | · | ○ | · | ✦ | 5 |
| **Guadaña** | ◇ | ✦ | ✦ | ○ | ○ | · | ○ | ◇ | ✦ | ◇ | ✦ | ✦ | 5 |
| **Incensario** | ✦ | ✦ | ◇ | ✦ | ✦ | ○ | ✦ | ✦ | ◇ | ✦ | ○ | ✦ | 8 |
| **Astrolabio** | ◇ | ✦ | ✦ | ○ | ○ | · | · | ○ | ✦ | ◇ | ✦ | ✦ | 5 |
| **Rifle** | ✦ | ✦ | ✦ | ○ | ✦ | · | ◇ | ○ | ✦ | ◇ | ✦ | ✦ | 7 |
| **Escoba** | · | · | · | · | · | · | · | · | · | · | · | · | 0 |
| **Campana** | ✦ | ○ | · | · | · | · | ✦ | ○ | · | · | · | ✦ | 3 |
| **Tijeras** | ✦ | ○ | · | · | · | · | ✦ | ✦ | · | · | · | ◇ | 3 |
| **Caldero** | ✦ | ✦ | ◇ | ○ | ✦ | · | ✦ | ◇ | ✦ | ◇ | ✦ | ✦ | 7 |
| **Varita** | ○ | · | · | · | · | · | ◇ | ✦ | · | · | · | · | 1 |
| **Daga** | ◇ | · | · | · | · | · | ✦ | ✦ | · | · | · | ○ | 2 |
| **✦ como 2.ª** | 6 | 6 | 3 | 1 | 3 | 0 | 7 | 6 | 4 | 1 | 4 | 7 | **48** |

**Conteo:** 48 de 144 pares ordenados hacen Spark natural (33 %). Un prefijo suma 17 más (65 en total, 45 %). La Directora de Coro, o un prefijo en cada bruja, suma 19 más (84, 58 %). Los otros 60 no hacen Spark de mano, aunque la voz o una Tempestad todavía pueden darlo.

| Familia | Spark como 1.ª | Como 2.ª | Total | Con un prefijo | Daño extra de su capa por par | Índice de equipo | Posición que favorece |
| --- | --: | --: | --: | --: | --: | --: | --- |
| Báculo | 2 | 6 | 8 | 13 | 22 % | 382 | Temprana: Enlace o Cierre |
| Grimorio de Agujas | 5 | 6 | 11 | 12 | 15 % | 445 | Central: cualquier lugar |
| Guadaña de Sal | 5 | 3 | 8 | 13 | 26 % | 378 | Central: cualquier lugar |
| Incensario encadenado | 8 | 1 | 9 | 11 | 21 % | 406 | Tardía: Apertura o Enlace |
| Astrolabio | 5 | 3 | 8 | 11 | 21 % | 345 | Central: cualquier lugar |
| Rifle Alquímico | 7 | 0 | 7 | 9 | 31 % | 418 | Tardía: Apertura o Enlace |
| Escoba | 0 | 7 | 7 | 9 | 26 % | 354 | Temprana: Enlace o Cierre |
| Campana de Bronce | 3 | 6 | 9 | 11 | 24 % | 399 | Temprana: Enlace o Cierre |
| Tijeras de Hilandera | 3 | 4 | 7 | 9 | 29 % | 330 | Central: cualquier lugar |
| Caldero de mano | 7 | 1 | 8 | 15 | 20 % | 420 | Tardía: Apertura o Enlace |
| Varita | 1 | 4 | 5 | 6 | 50 % | 332 | Temprana: Enlace o Cierre |
| Daga Ritual | 2 | 7 | 9 | 11 | 18 % | 389 | Temprana: Enlace o Cierre |

Cómo leer las columnas:

- **Total:** pares naturales en los que participa la familia, de 24 posibles (12 como primera y 12 como segunda).
- **Con un prefijo:** pares ✦ o ◇.
- **Daño extra de su capa por par:** cuánto sube, en promedio, el daño de su propia mano en un par con Spark. La Varita marca 50 % porque su único golpe recibe el +50 % completo.
- **Índice de equipo:** daño extra de la mano, propio y de la compañera, sumado en todos sus pares, en puntos de porcentaje de una capa. Sirve para comparar familias, no es daño real.
- **Posición:** temprana si hace Spark sobre todo como segunda, tardía si sobre todo como primera.

Lo que dice la matriz:

- **Nadie domina.** Los pares naturales van de 5 (Varita) a 11 (Grimorio). Ninguna familia hace Spark con más de 8 vecinas en una misma posición.
- **El Grimorio es el mejor para Spark, como dice el GDD,** porque tiene más pares y sirve en cualquier lugar. Su índice de equipo (445) es 6 % mayor que el siguiente, y cada aguja solo lleva 20 % de su mano.
- **Dos familias están atadas a su posición.** La Escoba llega antes que nadie y nunca hace Spark de mano como primera de un par: rinde de Enlace o de Cierre. El Rifle dispara tarde y nunca lo hace como segunda: rinde de Apertura o de Enlace.
- **Tres familias hacen Spark consigo mismas:** Grimorio, Guadaña e Incensario. Dos brujas con Guadaña seguidas siegan a compás.
- **Con familias al azar,** una Cadena de cuatro tiene en promedio un par seguido con Spark de mano (3 × 48 / 144). Eligiendo armas y orden se llega a tres.
- **Eslabones no seguidos (48 frames de separación):** 8 de 144 pares hacen Spark natural, siempre con una familia tardía o central adelante y una temprana dos lugares después: Guadaña → Campana, Incensario → Báculo, Incensario → Escoba, Incensario → Daga, Astrolabio → Campana, Rifle → Campana, Caldero → Escoba, Caldero → Campana. El Incensario es el que más lejos llega. A 72 frames ningún par coincide.

**Ejemplo resuelto.** Cuatro brujas con Incensario, Grimorio, Daga y Escoba, en ese orden:

| Frame | Eslabón | Familia | Spark |
| --: | :-: | --- | :-: |
| 36 | 1 | Incensario | |
| 42 | 2 | Grimorio | |
| 46 | 1 | Incensario | ✦ con 48 |
| 48 | 2 | Grimorio | ✦ con 46 |
| 54 | 2 | Grimorio | |
| 60 | 2 | Grimorio | ✦ con 62 |
| 62 | 1 | Incensario | ✦ con 60 y 62 |
| 62 | 3 | Daga | ✦ con 60 y 62 |
| 66 | 2 | Grimorio | ✦ con 68 |
| 68 | 3 | Daga | ✦ con 66 |
| 74 | 3 | Daga | |
| 80 | 3 | Daga | ✦ con 80 |
| 80 | 4 | Escoba | ✦ con 80 |
| 84 | 4 | Escoba | |
| 88 | 4 | Escoba | |

Son 9 golpes con Spark de 15. Con las mismas cuatro armas en orden inverso (Escoba, Daga, Grimorio, Incensario) no hay ninguno. Eso es el pilar 2, "el orden es poder", llevado al crafting.

**Ejemplo de prefijo.** Rifle seguido de Escoba no hace Spark (◇). Con Pausado en la Escoba, su último golpe cae en el frame 42 y el primero del Rifle en el 44: Spark. Rítmico en el Rifle logra lo mismo (42 contra 40). Rítmico en la Escoba la aleja y no pasa nada. La Partitura debe mostrar ese "casi" para que el jugador sepa qué prefijo buscar.

## 3. Niveles

| Nivel | ATQ del arma | Secundaria | Bruja sugerida | Se desbloquea | Estación | Materiales clave | Almitas | Prefijos posibles |
| --- | --: | --: | --: | --- | --- | --- | --: | --- |
| Rústico | 60 | ×1 | Nivel 1 | Desde el inicio | Yunque Astral | Astilla de Fresno, Costra de Sal, 2 Sal Cruda | 4 000 | Menores |
| Forjado | 150 | ×1.7 | Nivel 20 | Primera Esquirla Astral (primera élite) | Yunque Astral | Arma Rústica, Esquirla Astral, 4 Sal Cruda | 12 000 | Menores |
| Alquímico | 270 | ×2.6 | Nivel 40 | Al instalar la estación de la región de la familia | Estación regional | Arma Forjada, 6 de material común, 3 Sal Destilada | 36 000 | Menores y mayores |
| Astral | 420 | ×3.6 | Nivel 55 | Al vencer al jefe de esa región | Regional y Yunque Astral | Arma Alquímica, material de jefe, 5 Sal Destilada | 100 000 | Todos, Legendario incluido |
| Quintaesencia | 600 | ×4.7 | Nivel 70 | Primera Hoja de Alma | Regional y Altar del Árbol | Arma Astral, 2 Hoja de Alma, 3 Sal Quintaesencia | 250 000 | Todos |

- El ATQ del arma se suma plano al de la bruja. Como referencia, las brujas de la Fase 1 tienen de 1 000 a 1 840 de ATQ en el nivel 50. Una Quintaesencia aporta cerca de 25 % del ATQ de una bruja en el nivel 80.
- Todas las familias tienen el mismo ATQ en cada nivel.
- Cada nivel consume el arma del nivel anterior de la misma familia, de cualquier variante, como en los árboles de Terraria. Si ya no la quieres, la Fragua de Hierro Rojo la funde (sección 8).

Valor de la secundaria por nivel:

| Secundaria | Familia | Rústico | Forjado | Alquímico | Astral | Quintaesencia |
| --- | --- | --: | --: | --: | --: | --: |
| Vida % | Báculo | 4 | 7 | 10 | 14 | 18 |
| Ánima ganada % | Grimorio de Agujas | 3 | 5 | 8 | 11 | 14 |
| Ruptura de Firmeza % | Guadaña de Sal | 5 | 9 | 13 | 18 | 24 |
| Curación % | Incensario encadenado | 4 | 7 | 10 | 14 | 18 |
| ATQ % | Astrolabio | 3 | 5 | 8 | 11 | 14 |
| Penetración % | Rifle Alquímico | 2 | 4 | 6 | 8 | 10 |
| VEL | Escoba | 2 | 4 | 6 | 9 | 12 |
| Potencia de retraso % | Campana de Bronce | 5 | 9 | 13 | 18 | 24 |
| Daño crítico % | Tijeras de Hilandera | 6 | 10 | 15 | 20 | 26 |
| Potencia de Salmuera % | Caldero de mano | 6 | 10 | 15 | 20 | 26 |
| Reducción de daño % | Varita | 2 | 4 | 6 | 8 | 10 |
| Prob. de crítico % | Daga Ritual | 2 | 4 | 6 | 8 | 10 |

## 4. Sellos planetarios: qué distingue a la variante *a* de la *b*

Las dos variantes de un nivel comparten patrón, animación, ATQ y secundaria. Cambian el modelo, el material, los efectos visuales (GDD 15.5) y el **sello**: un planeta con una pasiva pequeña. Si el sello coincide con el planeta dominante de la bruja, la pasiva vale el doble.

| Sello | Pasiva | Rústico | Forjado | Alquímico | Astral | Quintaesencia |
| --- | --- | --: | --: | --: | --: | --: |
| Sol | +X % de daño en sus golpes en área | 2 | 3 | 4 | 6 | 8 |
| Luna | Escudo de X % de su vida máxima al empezar el combate | 3 | 5 | 7 | 9 | 12 |
| Marte | +X % de daño contra un solo objetivo | 2 | 3 | 4 | 6 | 8 |
| Mercurio | Adelanta X % su primera acción | 2 | 3 | 4 | 6 | 8 |
| Venus | Cura X % de su vida máxima al terminar cada Cadena en la que participa | 1 | 2 | 3 | 4 | 5 |
| Júpiter | +X de Ánima al empezar el combate | 5 | 8 | 12 | 16 | 20 |
| Saturno | Sus retrasos son X % más fuertes | 4 | 6 | 9 | 12 | 16 |

Cómo se reparten los sellos:

- Las dos variantes de un nivel siempre tienen planetas distintos.
- Cada familia cubre los siete planetas en sus 10 armas.
- En cada nivel, cada planeta aparece en 3 o 4 de las 24 armas.
- Regla de datos: con los planetas en el orden Sol, Luna, Marte, Mercurio, Venus, Júpiter y Saturno (índices 0 a 6), el sello de la variante *a* en el nivel *t* (0 a 4) es `(base + 2t) mod 7`, y el de la *b* es ese más 3. La base de cada familia está en la sección 12.
- La receta pide la sal del sello. Por eso el sello también decide qué sal gastas.

## 5. Materiales

Todos se ganan jugando. Ninguno se vende (GDD 16.4).

| Material | ID | Dónde se consigue | Para qué |
| --- | --- | --- | --- |
| Astilla de Fresno | `astilla_fresno` | Árboles muertos, barriles y cajas de cualquier región; el Fresno | Rústicos |
| Costra de Sal | `costra_sal` | Vetas de sal y salinas de cualquier región | Rústicos y Forjados |
| Esquirla Astral | `esquirla_astral` | Élites de cualquier región y nodos Élite del Fresno | Forjados a Astrales |
| Almita de Espectro | `almita_espectro` | Raíces Putrefactas del Fresno y Pantanos de Azufre | Péndulo de Resonancia |
| Hoja de Alma | `hoja_alma` | Copa Astral del Fresno (2 o 3 por run) y recompensa semanal del Fresno (2) | Quintaesencia y legendarias |
| Sal Cruda, Destilada y Quintaesencia | `sal_<planeta>_<pureza>` | GDD 11.1; se refina en el Altar del Árbol (sección 11.3) | Todas las recetas, según el sello |
| Sal Negra | `sal_negra` | GDD 11.1 | Elixir de Sal Negra y Salmueras malditas |

Materiales regionales. El común cae de los enemigos de la región; el de jefe, de la Casa de la región y de la versión repetible de ese combate.

| Región | Familia | Material común | Material de jefe |
| --- | --- | --- | --- |
| Aries, Montes de Hierro Rojo | Guadaña de Sal | Mena de Hierro Rojo (`mena_hierro_rojo`) | Cuerno del Carnero Corrupto (`cuerno_carnero_corrupto`) |
| Tauro, Llanuras de Cobre | Campana de Bronce | Cobre de Molino (`cobre_molino`) | Badajo de Sal Dulce (`badajo_sal_dulce`) |
| Géminis, Ciudad Espejo | Grimorio de Agujas | Vidrio de Canal (`vidrio_canal`) | Odre de Viento (`odre_viento`) |
| Cáncer, Costa de las Conchas | Varita | Nácar de Marea (`nacar_marea`) | Tenaza de Cangrejo Gigante (`tenaza_cangrejo_gigante`) |
| Leo, Desierto Áureo | Astrolabio | Arena Áurea (`arena_aurea`) | Cuerno del Rebaño Solar (`cuerno_rebano_solar`) |
| Virgo, Abadía de las Herbolarias | Tijeras de Hilandera | Lino de la Abadía (`lino_abadia`) | Filtro de la Hechicera (`filtro_hechicera`) |
| Libra, Ducado de las Balanzas | Báculo | Moneda de Alma (`moneda_alma`) | Fiel de la Balanza (`fiel_balanza`) |
| Escorpio, Pantanos de Azufre | Caldero de mano | Azufre de Pantano (`azufre_pantano`) | Óbolo del Río Lácteo (`obolo_rio_lacteo`) |
| Sagitario, Estepas del Centauro | Rifle Alquímico | Crin de Centauro (`crin_centauro`) | Ojo del Gigante (`ojo_gigante`) |
| Capricornio, Salinas de Obsidiana | Daga Ritual | Lasca de Obsidiana (`lasca_obsidiana`) | Perla de Remolino (`perla_remolino`) |
| Acuario, Archipiélago de Hielo | Escoba | Abedul Escarchado (`abedul_escarchado`) | Caracola del Canto (`caracola_canto`) |
| Piscis, Isla de la Ninfa | Incensario encadenado | Lágrima de Almáciga (`lagrima_almaciga`) | Velo de la Isla (`velo_isla`) |

Cada familia vive en la región que mejor le queda: el astrolabio andalusí en Leo, los carillones de los Países Bajos en Tauro, la imprenta veneciana en Géminis, las tenazas del cangrejo en Cáncer, el incensario bizantino en Piscis, la obsidiana de las salinas en Capricornio, las escobas de abedul en Acuario.

## 6. Catálogo de 120 armas

Todas las recetas siguen la misma plantilla por nivel. Cambian el material regional de la familia y la sal del sello. Las armas del Caldero de mano se llaman *Calderillo* para no confundirlas con el Caldero de Bruja.

| Nivel | Plantilla | Almitas | Estación |
| --- | --- | --: | --- |
| Rústico | 6 Astilla de Fresno, 4 Costra de Sal, 2 sal del sello cruda | 4 000 | Yunque Astral |
| Forjado | Rústica de la familia, 6 Costra de Sal, 2 Esquirla Astral, 4 sal del sello cruda | 12 000 | Yunque Astral |
| Alquímico | Forjada de la familia, 6 material común, 2 Esquirla Astral, 3 sal del sello destilada | 36 000 | Estación regional |
| Astral | Alquímica de la familia, 8 material común, 2 material de jefe, 4 Esquirla Astral, 5 sal del sello destilada | 100 000 | Regional y Yunque Astral |
| Quintaesencia | Astral de la familia, 4 material de jefe, 2 Hoja de Alma, 3 sal del sello quintaesencia | 250 000 | Regional y Altar del Árbol |

Excepciones, todas del GDD 15.3:

- **Daga de Sal Oxidada** (`daga_rustico_a`): Rústica normal.
- **Campana de Bronce** (`campana_forjado_b`): Forjada normal. Además la suelta la Casa de Tauro la primera vez que la vences. Interpreto "botín de la segunda región" como Tauro, el segundo signo (ver la sección 15).
- **Péndulo de Resonancia** (`campana_alquimico_b`): no usa la plantilla. Es la fusión del GDD y pide dos estaciones.

### 6.1 Báculo

Región y estación: Libra, Mesa del Ensayador. Material común: Moneda de Alma. Material de jefe: Fiel de la Balanza.

| ID | Nombre | Nivel | Var. | Sello | Materiales | Almitas | Estación |
| --- | --- | --- | :-: | --- | --- | --: | --- |
| `baculo_rustico_a` | Báculo de Cardenillo | Rústico | a | Venus | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Cúprica cruda | 4 000 | Yunque Astral |
| `baculo_rustico_b` | Báculo de Ceniza Salobre | Rústico | b | Sol | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Áurea cruda | 4 000 | Yunque Astral |
| `baculo_forjado_a` | Báculo de Plomo Fundido | Forjado | a | Saturno | Báculo rústico (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Plúmbea cruda | 12 000 | Yunque Astral |
| `baculo_forjado_b` | Báculo de Hierro Templado | Forjado | b | Marte | Báculo rústico (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Férrea cruda | 12 000 | Yunque Astral |
| `baculo_alquimico_a` | Báculo de Marea de Plata | Alquímico | a | Luna | Báculo forjado (cualquiera), 6 Moneda de Alma, 2 Esquirla Astral, 3 Sal Argenta destilada | 36 000 | Mesa del Ensayador |
| `baculo_alquimico_b` | Báculo de Savia de Cobre | Alquímico | b | Venus | Báculo forjado (cualquiera), 6 Moneda de Alma, 2 Esquirla Astral, 3 Sal Cúprica destilada | 36 000 | Mesa del Ensayador |
| `baculo_astral_a` | Báculo del Crepúsculo | Astral | a | Mercurio | Báculo alquímico (cualquiera), 8 Moneda de Alma, 2 Fiel de la Balanza, 4 Esquirla Astral, 5 Sal de Azogue destilada | 100 000 | Mesa del Ensayador y Yunque Astral |
| `baculo_astral_b` | Báculo de la Última Esfera | Astral | b | Saturno | Báculo alquímico (cualquiera), 8 Moneda de Alma, 2 Fiel de la Balanza, 4 Esquirla Astral, 5 Sal Plúmbea destilada | 100 000 | Mesa del Ensayador y Yunque Astral |
| `baculo_quintaesencia_a` | Báculo de Ley Tonante | Quintaesencia | a | Júpiter | Báculo astral (cualquiera), 4 Fiel de la Balanza, 2 Hoja de Alma, 3 Sal Estánnea quintaesencia | 250 000 | Mesa del Ensayador y Altar del Árbol |
| `baculo_quintaesencia_b` | Báculo de Espejo Lunar | Quintaesencia | b | Luna | Báculo astral (cualquiera), 4 Fiel de la Balanza, 2 Hoja de Alma, 3 Sal Argenta quintaesencia | 250 000 | Mesa del Ensayador y Altar del Árbol |

### 6.2 Grimorio de Agujas

Región y estación: Géminis, Prensa del Espejo. Material común: Vidrio de Canal. Material de jefe: Odre de Viento.

| ID | Nombre | Nivel | Var. | Sello | Materiales | Almitas | Estación |
| --- | --- | --- | :-: | --- | --- | --: | --- |
| `grimorio_rustico_a` | Grimorio de Cardenillo | Rústico | a | Venus | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Cúprica cruda | 4 000 | Yunque Astral |
| `grimorio_rustico_b` | Grimorio de Ceniza Salobre | Rústico | b | Sol | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Áurea cruda | 4 000 | Yunque Astral |
| `grimorio_forjado_a` | Grimorio de Plomo Fundido | Forjado | a | Saturno | Grimorio rústico (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Plúmbea cruda | 12 000 | Yunque Astral |
| `grimorio_forjado_b` | Grimorio de Hierro Templado | Forjado | b | Marte | Grimorio rústico (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Férrea cruda | 12 000 | Yunque Astral |
| `grimorio_alquimico_a` | Grimorio de Marea de Plata | Alquímico | a | Luna | Grimorio forjado (cualquiera), 6 Vidrio de Canal, 2 Esquirla Astral, 3 Sal Argenta destilada | 36 000 | Prensa del Espejo |
| `grimorio_alquimico_b` | Grimorio de Savia de Cobre | Alquímico | b | Venus | Grimorio forjado (cualquiera), 6 Vidrio de Canal, 2 Esquirla Astral, 3 Sal Cúprica destilada | 36 000 | Prensa del Espejo |
| `grimorio_astral_a` | Grimorio del Crepúsculo | Astral | a | Mercurio | Grimorio alquímico (cualquiera), 8 Vidrio de Canal, 2 Odre de Viento, 4 Esquirla Astral, 5 Sal de Azogue destilada | 100 000 | Prensa del Espejo y Yunque Astral |
| `grimorio_astral_b` | Grimorio de la Última Esfera | Astral | b | Saturno | Grimorio alquímico (cualquiera), 8 Vidrio de Canal, 2 Odre de Viento, 4 Esquirla Astral, 5 Sal Plúmbea destilada | 100 000 | Prensa del Espejo y Yunque Astral |
| `grimorio_quintaesencia_a` | Grimorio de Ley Tonante | Quintaesencia | a | Júpiter | Grimorio astral (cualquiera), 4 Odre de Viento, 2 Hoja de Alma, 3 Sal Estánnea quintaesencia | 250 000 | Prensa del Espejo y Altar del Árbol |
| `grimorio_quintaesencia_b` | Grimorio de Espejo Lunar | Quintaesencia | b | Luna | Grimorio astral (cualquiera), 4 Odre de Viento, 2 Hoja de Alma, 3 Sal Argenta quintaesencia | 250 000 | Prensa del Espejo y Altar del Árbol |

### 6.3 Guadaña de Sal

Región y estación: Aries, Fragua de Hierro Rojo. Material común: Mena de Hierro Rojo. Material de jefe: Cuerno del Carnero Corrupto.

| ID | Nombre | Nivel | Var. | Sello | Materiales | Almitas | Estación |
| --- | --- | --- | :-: | --- | --- | --: | --- |
| `guadana_rustico_a` | Guadaña de Estática Gris | Rústico | a | Júpiter | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Estánnea cruda | 4 000 | Yunque Astral |
| `guadana_rustico_b` | Guadaña de Bruma Salina | Rústico | b | Luna | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Argenta cruda | 4 000 | Yunque Astral |
| `guadana_forjado_a` | Guadaña de Oro Batido | Forjado | a | Sol | Guadaña rústica (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Áurea cruda | 12 000 | Yunque Astral |
| `guadana_forjado_b` | Guadaña de Azogue Sellado | Forjado | b | Mercurio | Guadaña rústica (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal de Azogue cruda | 12 000 | Yunque Astral |
| `guadana_alquimico_a` | Guadaña de Sangre Férrea | Alquímico | a | Marte | Guadaña forjada (cualquiera), 6 Mena de Hierro Rojo, 2 Esquirla Astral, 3 Sal Férrea destilada | 36 000 | Fragua de Hierro Rojo |
| `guadana_alquimico_b` | Guadaña de Trueno de Estaño | Alquímico | b | Júpiter | Guadaña forjada (cualquiera), 6 Mena de Hierro Rojo, 2 Esquirla Astral, 3 Sal Estánnea destilada | 36 000 | Fragua de Hierro Rojo |
| `guadana_astral_a` | Guadaña del Alba | Astral | a | Venus | Guadaña alquímica (cualquiera), 8 Mena de Hierro Rojo, 2 Cuerno del Carnero Corrupto, 4 Esquirla Astral, 5 Sal Cúprica destilada | 100 000 | Fragua de Hierro Rojo y Yunque Astral |
| `guadana_astral_b` | Guadaña del Mediodía | Astral | b | Sol | Guadaña alquímica (cualquiera), 8 Mena de Hierro Rojo, 2 Cuerno del Carnero Corrupto, 4 Esquirla Astral, 5 Sal Áurea destilada | 100 000 | Fragua de Hierro Rojo y Yunque Astral |
| `guadana_quintaesencia_a` | Guadaña de Hora Final | Quintaesencia | a | Saturno | Guadaña astral (cualquiera), 4 Cuerno del Carnero Corrupto, 2 Hoja de Alma, 3 Sal Plúmbea quintaesencia | 250 000 | Fragua de Hierro Rojo y Altar del Árbol |
| `guadana_quintaesencia_b` | Guadaña de Acero Carmesí | Quintaesencia | b | Marte | Guadaña astral (cualquiera), 4 Cuerno del Carnero Corrupto, 2 Hoja de Alma, 3 Sal Férrea quintaesencia | 250 000 | Fragua de Hierro Rojo y Altar del Árbol |

### 6.4 Incensario encadenado

Región y estación: Piscis, Sahumadero de la Ninfa. Material común: Lágrima de Almáciga. Material de jefe: Velo de la Isla.

| ID | Nombre | Nivel | Var. | Sello | Materiales | Almitas | Estación |
| --- | --- | --- | :-: | --- | --- | --: | --- |
| `incensario_rustico_a` | Incensario de Humo Ácido | Rústico | a | Mercurio | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal de Azogue cruda | 4 000 | Yunque Astral |
| `incensario_rustico_b` | Incensario de Ceniza Gris | Rústico | b | Saturno | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Plúmbea cruda | 4 000 | Yunque Astral |
| `incensario_forjado_a` | Incensario de Estaño Sonoro | Forjado | a | Júpiter | Incensario rústico (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Estánnea cruda | 12 000 | Yunque Astral |
| `incensario_forjado_b` | Incensario de Plata Fría | Forjado | b | Luna | Incensario rústico (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Argenta cruda | 12 000 | Yunque Astral |
| `incensario_alquimico_a` | Incensario de Fuego Pálido | Alquímico | a | Sol | Incensario forjado (cualquiera), 6 Lágrima de Almáciga, 2 Esquirla Astral, 3 Sal Áurea destilada | 36 000 | Sahumadero de la Ninfa |
| `incensario_alquimico_b` | Incensario de Azogue Vivo | Alquímico | b | Mercurio | Incensario forjado (cualquiera), 6 Lágrima de Almáciga, 2 Esquirla Astral, 3 Sal de Azogue destilada | 36 000 | Sahumadero de la Ninfa |
| `incensario_astral_a` | Incensario del Lucero Rojo | Astral | a | Marte | Incensario alquímico (cualquiera), 8 Lágrima de Almáciga, 2 Velo de la Isla, 4 Esquirla Astral, 5 Sal Férrea destilada | 100 000 | Sahumadero de la Ninfa y Yunque Astral |
| `incensario_astral_b` | Incensario del Rey Errante | Astral | b | Júpiter | Incensario alquímico (cualquiera), 8 Lágrima de Almáciga, 2 Velo de la Isla, 4 Esquirla Astral, 5 Sal Estánnea destilada | 100 000 | Sahumadero de la Ninfa y Yunque Astral |
| `incensario_quintaesencia_a` | Incensario de Jardín Esmeralda | Quintaesencia | a | Venus | Incensario astral (cualquiera), 4 Velo de la Isla, 2 Hoja de Alma, 3 Sal Cúprica quintaesencia | 250 000 | Sahumadero de la Ninfa y Altar del Árbol |
| `incensario_quintaesencia_b` | Incensario de Corona Solar | Quintaesencia | b | Sol | Incensario astral (cualquiera), 4 Velo de la Isla, 2 Hoja de Alma, 3 Sal Áurea quintaesencia | 250 000 | Sahumadero de la Ninfa y Altar del Árbol |

### 6.5 Astrolabio

Región y estación: Leo, Mesa del Astrolabista. Material común: Arena Áurea. Material de jefe: Cuerno del Rebaño Solar.

| ID | Nombre | Nivel | Var. | Sello | Materiales | Almitas | Estación |
| --- | --- | --- | :-: | --- | --- | --: | --- |
| `astrolabio_rustico_a` | Astrolabio de Ceniza Gris | Rústico | a | Saturno | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Plúmbea cruda | 4 000 | Yunque Astral |
| `astrolabio_rustico_b` | Astrolabio de Sal Oxidada | Rústico | b | Marte | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Férrea cruda | 4 000 | Yunque Astral |
| `astrolabio_forjado_a` | Astrolabio de Plata Fría | Forjado | a | Luna | Astrolabio rústico (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Argenta cruda | 12 000 | Yunque Astral |
| `astrolabio_forjado_b` | Astrolabio de Cobre Martillado | Forjado | b | Venus | Astrolabio rústico (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Cúprica cruda | 12 000 | Yunque Astral |
| `astrolabio_alquimico_a` | Astrolabio de Azogue Vivo | Alquímico | a | Mercurio | Astrolabio forjado (cualquiera), 6 Arena Áurea, 2 Esquirla Astral, 3 Sal de Azogue destilada | 36 000 | Mesa del Astrolabista |
| `astrolabio_alquimico_b` | Astrolabio de Plomo Lento | Alquímico | b | Saturno | Astrolabio forjado (cualquiera), 6 Arena Áurea, 2 Esquirla Astral, 3 Sal Plúmbea destilada | 36 000 | Mesa del Astrolabista |
| `astrolabio_astral_a` | Astrolabio del Rey Errante | Astral | a | Júpiter | Astrolabio alquímico (cualquiera), 8 Arena Áurea, 2 Cuerno del Rebaño Solar, 4 Esquirla Astral, 5 Sal Estánnea destilada | 100 000 | Mesa del Astrolabista y Yunque Astral |
| `astrolabio_astral_b` | Astrolabio del Plenilunio | Astral | b | Luna | Astrolabio alquímico (cualquiera), 8 Arena Áurea, 2 Cuerno del Rebaño Solar, 4 Esquirla Astral, 5 Sal Argenta destilada | 100 000 | Mesa del Astrolabista y Yunque Astral |
| `astrolabio_quintaesencia_a` | Astrolabio de Corona Solar | Quintaesencia | a | Sol | Astrolabio astral (cualquiera), 4 Cuerno del Rebaño Solar, 2 Hoja de Alma, 3 Sal Áurea quintaesencia | 250 000 | Mesa del Astrolabista y Altar del Árbol |
| `astrolabio_quintaesencia_b` | Astrolabio de Mente Relámpago | Quintaesencia | b | Mercurio | Astrolabio astral (cualquiera), 4 Cuerno del Rebaño Solar, 2 Hoja de Alma, 3 Sal de Azogue quintaesencia | 250 000 | Mesa del Astrolabista y Altar del Árbol |

### 6.6 Rifle Alquímico

Región y estación: Sagitario, Polvorín de la Estepa. Material común: Crin de Centauro. Material de jefe: Ojo del Gigante.

| ID | Nombre | Nivel | Var. | Sello | Materiales | Almitas | Estación |
| --- | --- | --- | :-: | --- | --- | --: | --- |
| `rifle_rustico_a` | Rifle de Bruma Salina | Rústico | a | Luna | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Argenta cruda | 4 000 | Yunque Astral |
| `rifle_rustico_b` | Rifle de Cardenillo | Rústico | b | Venus | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Cúprica cruda | 4 000 | Yunque Astral |
| `rifle_forjado_a` | Rifle de Azogue Sellado | Forjado | a | Mercurio | Rifle rústico (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal de Azogue cruda | 12 000 | Yunque Astral |
| `rifle_forjado_b` | Rifle de Plomo Fundido | Forjado | b | Saturno | Rifle rústico (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Plúmbea cruda | 12 000 | Yunque Astral |
| `rifle_alquimico_a` | Rifle de Trueno de Estaño | Alquímico | a | Júpiter | Rifle forjado (cualquiera), 6 Crin de Centauro, 2 Esquirla Astral, 3 Sal Estánnea destilada | 36 000 | Polvorín de la Estepa |
| `rifle_alquimico_b` | Rifle de Marea de Plata | Alquímico | b | Luna | Rifle forjado (cualquiera), 6 Crin de Centauro, 2 Esquirla Astral, 3 Sal Argenta destilada | 36 000 | Polvorín de la Estepa |
| `rifle_astral_a` | Rifle del Mediodía | Astral | a | Sol | Rifle alquímico (cualquiera), 8 Crin de Centauro, 2 Ojo del Gigante, 4 Esquirla Astral, 5 Sal Áurea destilada | 100 000 | Polvorín de la Estepa y Yunque Astral |
| `rifle_astral_b` | Rifle del Crepúsculo | Astral | b | Mercurio | Rifle alquímico (cualquiera), 8 Crin de Centauro, 2 Ojo del Gigante, 4 Esquirla Astral, 5 Sal de Azogue destilada | 100 000 | Polvorín de la Estepa y Yunque Astral |
| `rifle_quintaesencia_a` | Rifle de Acero Carmesí | Quintaesencia | a | Marte | Rifle astral (cualquiera), 4 Ojo del Gigante, 2 Hoja de Alma, 3 Sal Férrea quintaesencia | 250 000 | Polvorín de la Estepa y Altar del Árbol |
| `rifle_quintaesencia_b` | Rifle de Ley Tonante | Quintaesencia | b | Júpiter | Rifle astral (cualquiera), 4 Ojo del Gigante, 2 Hoja de Alma, 3 Sal Estánnea quintaesencia | 250 000 | Polvorín de la Estepa y Altar del Árbol |

### 6.7 Escoba

Región y estación: Acuario, Secadero de Abedul. Material común: Abedul Escarchado. Material de jefe: Caracola del Canto.

| ID | Nombre | Nivel | Var. | Sello | Materiales | Almitas | Estación |
| --- | --- | --- | :-: | --- | --- | --: | --- |
| `escoba_rustico_a` | Escoba de Sal Oxidada | Rústico | a | Marte | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Férrea cruda | 4 000 | Yunque Astral |
| `escoba_rustico_b` | Escoba de Estática Gris | Rústico | b | Júpiter | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Estánnea cruda | 4 000 | Yunque Astral |
| `escoba_forjado_a` | Escoba de Cobre Martillado | Forjado | a | Venus | Escoba rústica (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Cúprica cruda | 12 000 | Yunque Astral |
| `escoba_forjado_b` | Escoba de Oro Batido | Forjado | b | Sol | Escoba rústica (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Áurea cruda | 12 000 | Yunque Astral |
| `escoba_alquimico_a` | Escoba de Plomo Lento | Alquímico | a | Saturno | Escoba forjada (cualquiera), 6 Abedul Escarchado, 2 Esquirla Astral, 3 Sal Plúmbea destilada | 36 000 | Secadero de Abedul |
| `escoba_alquimico_b` | Escoba de Sangre Férrea | Alquímico | b | Marte | Escoba forjada (cualquiera), 6 Abedul Escarchado, 2 Esquirla Astral, 3 Sal Férrea destilada | 36 000 | Secadero de Abedul |
| `escoba_astral_a` | Escoba del Plenilunio | Astral | a | Luna | Escoba alquímica (cualquiera), 8 Abedul Escarchado, 2 Caracola del Canto, 4 Esquirla Astral, 5 Sal Argenta destilada | 100 000 | Secadero de Abedul y Yunque Astral |
| `escoba_astral_b` | Escoba del Alba | Astral | b | Venus | Escoba alquímica (cualquiera), 8 Abedul Escarchado, 2 Caracola del Canto, 4 Esquirla Astral, 5 Sal Cúprica destilada | 100 000 | Secadero de Abedul y Yunque Astral |
| `escoba_quintaesencia_a` | Escoba de Mente Relámpago | Quintaesencia | a | Mercurio | Escoba astral (cualquiera), 4 Caracola del Canto, 2 Hoja de Alma, 3 Sal de Azogue quintaesencia | 250 000 | Secadero de Abedul y Altar del Árbol |
| `escoba_quintaesencia_b` | Escoba de Hora Final | Quintaesencia | b | Saturno | Escoba astral (cualquiera), 4 Caracola del Canto, 2 Hoja de Alma, 3 Sal Plúmbea quintaesencia | 250 000 | Secadero de Abedul y Altar del Árbol |

### 6.8 Campana de Bronce

Región y estación: Tauro, Fundición del Molino. Material común: Cobre de Molino. Material de jefe: Badajo de Sal Dulce.

| ID | Nombre | Nivel | Var. | Sello | Materiales | Almitas | Estación |
| --- | --- | --- | :-: | --- | --- | --: | --- |
| `campana_rustico_a` | Campana de Ceniza Gris | Rústico | a | Saturno | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Plúmbea cruda | 4 000 | Yunque Astral |
| `campana_rustico_b` | Campana de Sal Oxidada | Rústico | b | Marte | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Férrea cruda | 4 000 | Yunque Astral |
| `campana_forjado_a` | Campana de Plata Fría | Forjado | a | Luna | Campana rústica (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Argenta cruda | 12 000 | Yunque Astral |
| `campana_forjado_b` | Campana de Bronce | Forjado | b | Venus | Campana rústica (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Cúprica cruda | 12 000 | Yunque Astral |
| `campana_alquimico_a` | Campana de Azogue Vivo | Alquímico | a | Mercurio | Campana forjada (cualquiera), 6 Cobre de Molino, 2 Esquirla Astral, 3 Sal de Azogue destilada | 36 000 | Fundición del Molino |
| `campana_alquimico_b` | Péndulo de Resonancia | Alquímico | b | Saturno | Campana de Bronce, Daga de Sal Oxidada, 1 Almita de Espectro, 3 Sal Plúmbea destilada | 36 000 | Fundición del Molino y Altar del Árbol |
| `campana_astral_a` | Campana del Rey Errante | Astral | a | Júpiter | Campana alquímica (cualquiera), 8 Cobre de Molino, 2 Badajo de Sal Dulce, 4 Esquirla Astral, 5 Sal Estánnea destilada | 100 000 | Fundición del Molino y Yunque Astral |
| `campana_astral_b` | Campana del Plenilunio | Astral | b | Luna | Campana alquímica (cualquiera), 8 Cobre de Molino, 2 Badajo de Sal Dulce, 4 Esquirla Astral, 5 Sal Argenta destilada | 100 000 | Fundición del Molino y Yunque Astral |
| `campana_quintaesencia_a` | Campana de Corona Solar | Quintaesencia | a | Sol | Campana astral (cualquiera), 4 Badajo de Sal Dulce, 2 Hoja de Alma, 3 Sal Áurea quintaesencia | 250 000 | Fundición del Molino y Altar del Árbol |
| `campana_quintaesencia_b` | Campana de Mente Relámpago | Quintaesencia | b | Mercurio | Campana astral (cualquiera), 4 Badajo de Sal Dulce, 2 Hoja de Alma, 3 Sal de Azogue quintaesencia | 250 000 | Fundición del Molino y Altar del Árbol |

### 6.9 Tijeras de Hilandera

Región y estación: Virgo, Telar de la Hilandera. Material común: Lino de la Abadía. Material de jefe: Filtro de la Hechicera.

| ID | Nombre | Nivel | Var. | Sello | Materiales | Almitas | Estación |
| --- | --- | --- | :-: | --- | --- | --: | --- |
| `tijeras_rustico_a` | Tijeras de Ceniza Salobre | Rústico | a | Sol | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Áurea cruda | 4 000 | Yunque Astral |
| `tijeras_rustico_b` | Tijeras de Humo Ácido | Rústico | b | Mercurio | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal de Azogue cruda | 4 000 | Yunque Astral |
| `tijeras_forjado_a` | Tijeras de Hierro Templado | Forjado | a | Marte | Tijeras rústicas (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Férrea cruda | 12 000 | Yunque Astral |
| `tijeras_forjado_b` | Tijeras de Estaño Sonoro | Forjado | b | Júpiter | Tijeras rústicas (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Estánnea cruda | 12 000 | Yunque Astral |
| `tijeras_alquimico_a` | Tijeras de Savia de Cobre | Alquímico | a | Venus | Tijeras forjadas (cualquiera), 6 Lino de la Abadía, 2 Esquirla Astral, 3 Sal Cúprica destilada | 36 000 | Telar de la Hilandera |
| `tijeras_alquimico_b` | Tijeras de Fuego Pálido | Alquímico | b | Sol | Tijeras forjadas (cualquiera), 6 Lino de la Abadía, 2 Esquirla Astral, 3 Sal Áurea destilada | 36 000 | Telar de la Hilandera |
| `tijeras_astral_a` | Tijeras de la Última Esfera | Astral | a | Saturno | Tijeras alquímicas (cualquiera), 8 Lino de la Abadía, 2 Filtro de la Hechicera, 4 Esquirla Astral, 5 Sal Plúmbea destilada | 100 000 | Telar de la Hilandera y Yunque Astral |
| `tijeras_astral_b` | Tijeras del Lucero Rojo | Astral | b | Marte | Tijeras alquímicas (cualquiera), 8 Lino de la Abadía, 2 Filtro de la Hechicera, 4 Esquirla Astral, 5 Sal Férrea destilada | 100 000 | Telar de la Hilandera y Yunque Astral |
| `tijeras_quintaesencia_a` | Tijeras de Espejo Lunar | Quintaesencia | a | Luna | Tijeras astrales (cualquiera), 4 Filtro de la Hechicera, 2 Hoja de Alma, 3 Sal Argenta quintaesencia | 250 000 | Telar de la Hilandera y Altar del Árbol |
| `tijeras_quintaesencia_b` | Tijeras de Jardín Esmeralda | Quintaesencia | b | Venus | Tijeras astrales (cualquiera), 4 Filtro de la Hechicera, 2 Hoja de Alma, 3 Sal Cúprica quintaesencia | 250 000 | Telar de la Hilandera y Altar del Árbol |

### 6.10 Caldero de mano

Región y estación: Escorpio, Alambique de Azufre. Material común: Azufre de Pantano. Material de jefe: Óbolo del Río Lácteo.

| ID | Nombre | Nivel | Var. | Sello | Materiales | Almitas | Estación |
| --- | --- | --- | :-: | --- | --- | --: | --- |
| `caldero_mano_rustico_a` | Calderillo de Bruma Salina | Rústico | a | Luna | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Argenta cruda | 4 000 | Yunque Astral |
| `caldero_mano_rustico_b` | Calderillo de Cardenillo | Rústico | b | Venus | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Cúprica cruda | 4 000 | Yunque Astral |
| `caldero_mano_forjado_a` | Calderillo de Azogue Sellado | Forjado | a | Mercurio | Calderillo rústico (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal de Azogue cruda | 12 000 | Yunque Astral |
| `caldero_mano_forjado_b` | Calderillo de Plomo Fundido | Forjado | b | Saturno | Calderillo rústico (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Plúmbea cruda | 12 000 | Yunque Astral |
| `caldero_mano_alquimico_a` | Calderillo de Trueno de Estaño | Alquímico | a | Júpiter | Calderillo forjado (cualquiera), 6 Azufre de Pantano, 2 Esquirla Astral, 3 Sal Estánnea destilada | 36 000 | Alambique de Azufre |
| `caldero_mano_alquimico_b` | Calderillo de Marea de Plata | Alquímico | b | Luna | Calderillo forjado (cualquiera), 6 Azufre de Pantano, 2 Esquirla Astral, 3 Sal Argenta destilada | 36 000 | Alambique de Azufre |
| `caldero_mano_astral_a` | Calderillo del Mediodía | Astral | a | Sol | Calderillo alquímico (cualquiera), 8 Azufre de Pantano, 2 Óbolo del Río Lácteo, 4 Esquirla Astral, 5 Sal Áurea destilada | 100 000 | Alambique de Azufre y Yunque Astral |
| `caldero_mano_astral_b` | Calderillo del Crepúsculo | Astral | b | Mercurio | Calderillo alquímico (cualquiera), 8 Azufre de Pantano, 2 Óbolo del Río Lácteo, 4 Esquirla Astral, 5 Sal de Azogue destilada | 100 000 | Alambique de Azufre y Yunque Astral |
| `caldero_mano_quintaesencia_a` | Calderillo de Acero Carmesí | Quintaesencia | a | Marte | Calderillo astral (cualquiera), 4 Óbolo del Río Lácteo, 2 Hoja de Alma, 3 Sal Férrea quintaesencia | 250 000 | Alambique de Azufre y Altar del Árbol |
| `caldero_mano_quintaesencia_b` | Calderillo de Ley Tonante | Quintaesencia | b | Júpiter | Calderillo astral (cualquiera), 4 Óbolo del Río Lácteo, 2 Hoja de Alma, 3 Sal Estánnea quintaesencia | 250 000 | Alambique de Azufre y Altar del Árbol |

### 6.11 Varita

Región y estación: Cáncer, Pila de Nácar. Material común: Nácar de Marea. Material de jefe: Tenaza de Cangrejo Gigante.

| ID | Nombre | Nivel | Var. | Sello | Materiales | Almitas | Estación |
| --- | --- | --- | :-: | --- | --- | --: | --- |
| `varita_rustico_a` | Varita de Ceniza Salobre | Rústico | a | Sol | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Áurea cruda | 4 000 | Yunque Astral |
| `varita_rustico_b` | Varita de Humo Ácido | Rústico | b | Mercurio | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal de Azogue cruda | 4 000 | Yunque Astral |
| `varita_forjado_a` | Varita de Hierro Templado | Forjado | a | Marte | Varita rústica (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Férrea cruda | 12 000 | Yunque Astral |
| `varita_forjado_b` | Varita de Estaño Sonoro | Forjado | b | Júpiter | Varita rústica (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Estánnea cruda | 12 000 | Yunque Astral |
| `varita_alquimico_a` | Varita de Savia de Cobre | Alquímico | a | Venus | Varita forjada (cualquiera), 6 Nácar de Marea, 2 Esquirla Astral, 3 Sal Cúprica destilada | 36 000 | Pila de Nácar |
| `varita_alquimico_b` | Varita de Fuego Pálido | Alquímico | b | Sol | Varita forjada (cualquiera), 6 Nácar de Marea, 2 Esquirla Astral, 3 Sal Áurea destilada | 36 000 | Pila de Nácar |
| `varita_astral_a` | Varita de la Última Esfera | Astral | a | Saturno | Varita alquímica (cualquiera), 8 Nácar de Marea, 2 Tenaza de Cangrejo Gigante, 4 Esquirla Astral, 5 Sal Plúmbea destilada | 100 000 | Pila de Nácar y Yunque Astral |
| `varita_astral_b` | Varita del Lucero Rojo | Astral | b | Marte | Varita alquímica (cualquiera), 8 Nácar de Marea, 2 Tenaza de Cangrejo Gigante, 4 Esquirla Astral, 5 Sal Férrea destilada | 100 000 | Pila de Nácar y Yunque Astral |
| `varita_quintaesencia_a` | Varita de Espejo Lunar | Quintaesencia | a | Luna | Varita astral (cualquiera), 4 Tenaza de Cangrejo Gigante, 2 Hoja de Alma, 3 Sal Argenta quintaesencia | 250 000 | Pila de Nácar y Altar del Árbol |
| `varita_quintaesencia_b` | Varita de Jardín Esmeralda | Quintaesencia | b | Venus | Varita astral (cualquiera), 4 Tenaza de Cangrejo Gigante, 2 Hoja de Alma, 3 Sal Cúprica quintaesencia | 250 000 | Pila de Nácar y Altar del Árbol |

### 6.12 Daga Ritual

Región y estación: Capricornio, Horno de Obsidiana. Material común: Lasca de Obsidiana. Material de jefe: Perla de Remolino.

| ID | Nombre | Nivel | Var. | Sello | Materiales | Almitas | Estación |
| --- | --- | --- | :-: | --- | --- | --: | --- |
| `daga_rustico_a` | Daga de Sal Oxidada | Rústico | a | Marte | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Férrea cruda | 4 000 | Yunque Astral |
| `daga_rustico_b` | Daga de Estática Gris | Rústico | b | Júpiter | 6 Astilla de Fresno, 4 Costra de Sal, 2 Sal Estánnea cruda | 4 000 | Yunque Astral |
| `daga_forjado_a` | Daga de Cobre Martillado | Forjado | a | Venus | Daga rústica (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Cúprica cruda | 12 000 | Yunque Astral |
| `daga_forjado_b` | Daga de Oro Batido | Forjado | b | Sol | Daga rústica (cualquiera), 6 Costra de Sal, 2 Esquirla Astral, 4 Sal Áurea cruda | 12 000 | Yunque Astral |
| `daga_alquimico_a` | Daga de Plomo Lento | Alquímico | a | Saturno | Daga forjada (cualquiera), 6 Lasca de Obsidiana, 2 Esquirla Astral, 3 Sal Plúmbea destilada | 36 000 | Horno de Obsidiana |
| `daga_alquimico_b` | Daga de Sangre Férrea | Alquímico | b | Marte | Daga forjada (cualquiera), 6 Lasca de Obsidiana, 2 Esquirla Astral, 3 Sal Férrea destilada | 36 000 | Horno de Obsidiana |
| `daga_astral_a` | Daga del Plenilunio | Astral | a | Luna | Daga alquímica (cualquiera), 8 Lasca de Obsidiana, 2 Perla de Remolino, 4 Esquirla Astral, 5 Sal Argenta destilada | 100 000 | Horno de Obsidiana y Yunque Astral |
| `daga_astral_b` | Daga del Alba | Astral | b | Venus | Daga alquímica (cualquiera), 8 Lasca de Obsidiana, 2 Perla de Remolino, 4 Esquirla Astral, 5 Sal Cúprica destilada | 100 000 | Horno de Obsidiana y Yunque Astral |
| `daga_quintaesencia_a` | Daga de Mente Relámpago | Quintaesencia | a | Mercurio | Daga astral (cualquiera), 4 Perla de Remolino, 2 Hoja de Alma, 3 Sal de Azogue quintaesencia | 250 000 | Horno de Obsidiana y Altar del Árbol |
| `daga_quintaesencia_b` | Daga de Hora Final | Quintaesencia | b | Saturno | Daga astral (cualquiera), 4 Perla de Remolino, 2 Hoja de Alma, 3 Sal Plúmbea quintaesencia | 250 000 | Horno de Obsidiana y Altar del Árbol |

## 7. Legendarias

Reglas:

- **Laterales, no verticales.** Mismo ATQ que Quintaesencia (600) y la secundaria de su familia en valor de Quintaesencia. No hay nivel por encima (GDD 17.2).
- Conservan el patrón, el rasgo y la animación de su familia, salvo el Reloj. Agregan un efecto, un modelo propio y su propio sello.
- La fusión conserva el prefijo del arma núcleo: la Quintaesencia de su familia; en el Reloj, el Péndulo.
- Se pueden fabricar más de una vez. No propongo límite por equipo; lo decide el simulador.
- **Kompu gacha:** ninguna fusión pide nada que salga del gacha (GDD 2).
- Cuatro familias no tienen legendaria al lanzamiento: Báculo, Grimorio de Agujas, Caldero de mano y Varita. El Grimorio, a propósito, porque ya es el mejor para Spark. Las otras tres pueden llegar en las versiones 1.1 a 1.3 (GDD 22.1: de 6 a 10 armas por versión). Las cuatro aparecen como ingrediente de alguna fusión.

El árbol de ejemplo del GDD 15.3 queda así:

1. *Daga de Sal Oxidada* + *Campana de Bronce* + *Almita de Espectro* = **Péndulo de Resonancia** (Campana Alquímica *b*, sello de Saturno).
2. *Péndulo de Resonancia* + cuatro armas Astrales con sello de Sol, Luna, Marte y Saturno = **Reloj del Eclipse Verdadero**. Hay 14 armas Astrales con esos sellos, así que el jugador elige cuáles fundir.

| Legendaria | ID | Familia | Sello | Fusión (más 250 000 almitas) | Estaciones | Efecto especial |
| --- | --- | --- | --- | --- | --- | --- |
| Reloj del Eclipse Verdadero | `reloj_eclipse_verdadero` | Astrolabio | Sol y Saturno | Péndulo de Resonancia, una Astral con sello de Sol, una de Luna, una de Marte y una de Saturno, 3 Hoja de Alma, 2 Sal Áurea quintaesencia, 2 Sal Plúmbea quintaesencia | Altar del Árbol y Mesa del Astrolabista | Sus golpes cambian de forma: en cada Cadena, su mano usa el patrón de una de las cuatro Astrales que lo forjaron, en orden Sol, Luna, Marte y Saturno. La Partitura muestra cuál toca. Su básico es el del Astrolabio |
| Guadaña del Último Asiento | `guadana_ultimo_asiento` | Guadaña de Sal | Saturno | Guadaña quintaesencia, Campana astral, 4 Cuerno del Carnero Corrupto, 2 Hoja de Alma, 3 Sal Plúmbea quintaesencia | Altar del Árbol y Fragua de Hierro Rojo | Cada golpe de Siega con Spark retrasa 5 % a todos los enemigos; máximo 15 % por Cadena |
| Colmillo de Obsidiana | `colmillo_obsidiana` | Daga Ritual | Marte | Daga quintaesencia, Guadaña astral, 4 Perla de Remolino, 2 Hoja de Alma, 3 Sal Férrea quintaesencia | Altar del Árbol y Horno de Obsidiana | Sus críticos aplican una carga de Sangrado; máximo tres por enemigo |
| Escoba del Viento Embotellado | `escoba_viento_embotellado` | Escoba | Mercurio | Escoba quintaesencia, Grimorio astral, 4 Odre de Viento, 2 Hoja de Alma, 3 Sal de Azogue quintaesencia | Altar del Árbol y Secadero de Abedul | Si su eslabón hace Spark, su bruja adelanta 15 % su siguiente acción después de la Cadena |
| Incensario del Jardín Cerrado | `incensario_jardin_cerrado` | Incensario encadenado | Venus | Incensario quintaesencia, Calderillo astral, 4 Velo de la Isla, 2 Hoja de Alma, 3 Sal Cúprica quintaesencia | Altar del Árbol y Sahumadero de la Ninfa | Cada eslabón que sigue al suyo en la Cadena cura a su bruja 4 % de su vida máxima |
| Campana del Primer Trueno | `campana_primer_trueno` | Campana de Bronce | Júpiter | Campana quintaesencia, Varita astral, 4 Badajo de Sal Dulce, 2 Hoja de Alma, 3 Sal Estánnea quintaesencia | Altar del Árbol y Fundición del Molino | Su onda salta a un segundo enemigo con 50 % del daño; el salto no hace Spark |
| Tijeras de la Marea Partida | `tijeras_marea_partida` | Tijeras de Hilandera | Luna | Tijeras quintaesencia, Báculo astral, 4 Filtro de la Hechicera, 2 Hoja de Alma, 3 Sal Argenta quintaesencia | Altar del Árbol y Telar de la Hilandera | Al usar Hilos, su bruja gana un escudo de 10 % de su vida máxima; si sus dos golpes hacen Spark, el escudo pasa a la aliada con menos vida |
| Espingarda del Mediodía | `espingarda_mediodia` | Rifle Alquímico | Sol | Rifle quintaesencia, Astrolabio astral, 4 Ojo del Gigante, 2 Hoja de Alma, 3 Sal Áurea quintaesencia | Altar del Árbol y Polvorín de la Estepa | Su golpe fuerte deja Quemadura, y contra enemigos con Quemadura ignora 10 % más de RES |

El sello del Reloj cuenta como Sol y como Saturno: es la Reacción Eclipse (GDD 7.3).

## 8. Estaciones

### 8.1 Estaciones base, en la Tortuga desde el inicio

| Estación | ID | Qué hace |
| --- | --- | --- |
| Caldero de Bruja | `caldero_bruja` | Salmueras en tres potencias, Salmuera Turbia y Recetario. Vende las bases: vinagre y, en luna llena, leche lunar |
| Yunque Astral | `yunque_astral` | Armas Rústicas y Forjadas. Reforja con Memoria y Temple. **Ensayo:** pone cualquier arma del Recetario en tus Partituras guardadas antes de fabricarla. Segunda estación de las Astrales |
| Altar del Árbol | `altar_arbol` | Refina sal: Cruda a Destilada y Destilada a Quintaesencia. Fusiones: Péndulo y legendarias. Segunda estación de las Quintaesencia |

### 8.2 Estaciones regionales

Cada una se gana al terminar el capítulo de su región y se instala en el caparazón de la Tortuga, donde también se ve como decoración. Abre los niveles Alquímico, Astral y Quintaesencia de su familia y una función extra. Como las regiones van en cualquier orden (GDD 6), cada jugador abre familias en un orden distinto.

| Región | Estación | ID | Familia | Además |
| --- | --- | --- | --- | --- |
| Aries | **Fragua de Hierro Rojo** (GDD) | `fragua_hierro_rojo` | Guadaña de Sal | **Recocido:** funde un arma que ya no usas y devuelve la mitad de sus materiales, sin almitas |
| Tauro | Fundición del Molino | `fundicion_molino` | Campana de Bronce | **Molienda:** 3 Sal Cruda de un planeta por 2 de otro |
| Géminis | Prensa del Espejo | `prensa_espejo` | Grimorio de Agujas | **Espejo:** cambia un arma a su otra variante del mismo nivel y conserva el prefijo, por 25 % de las almitas de ese nivel |
| Cáncer | Pila de Nácar | `pila_nacar` | Varita | **Leche lunar** todos los días, no solo en luna llena |
| Leo | Mesa del Astrolabista | `mesa_astrolabista` | Astrolabio | **Carta doble:** el sello también vale el doble con el planeta secundario de la bruja |
| Virgo | **Telar de la Hilandera** (GDD) | `telar_hilandera` | Tijeras de Hilandera | **Memoria larga:** el Yunque recuerda 6 prefijos por arma en vez de 3 |
| Libra | Mesa del Ensayador | `mesa_ensayador` | Báculo | **Reforja dirigida:** eliges la categoría del prefijo por el doble de costo |
| Escorpio | Alambique de Azufre | `alambique_azufre` | Caldero de mano | **Salmueras malditas:** una mejorada con 1 Sal Negra sube a maestra sin Quintaesencia, y quien la bebe gana +20 de Salinidad |
| Sagitario | Polvorín de la Estepa | `polvorin_estepa` | Rifle Alquímico | **Trueque:** 2 materiales comunes de una región por 1 de otra |
| Capricornio | **Horno de Obsidiana** (GDD) | `horno_obsidiana` | Daga Ritual | **Salina:** refinar Cruda a Destilada cuesta 25 % menos almitas |
| Acuario | Secadero de Abedul | `secadero_abedul` | Escoba | **Destilación en frío:** refinar Destilada a Quintaesencia cuesta 25 % menos almitas |
| Piscis | Sahumadero de la Ninfa | `sahumadero_ninfa` | Incensario encadenado | **Sueño de la Ninfa:** una vez al día, el Caldero te dice qué saldría de una mezcla antes de prepararla |

### 8.3 Recetas de dos estaciones

| Receta | Estaciones | Cuántas |
| --- | --- | --: |
| Armas Astrales | Estación regional de la familia y Yunque Astral | 24 |
| Armas Quintaesencia | Estación regional de la familia y Altar del Árbol | 24 |
| Péndulo de Resonancia | Fundición del Molino y Altar del Árbol | 1 |
| Legendarias | Altar del Árbol y estación regional de su familia | 8 |
| Salmueras malditas, todas menos el Agua de Olvido | Alambique de Azufre y Caldero de Bruja | 10 |

En total, 57 armas piden dos estaciones.

## 9. Reforja

### 9.1 Reglas

- Cada arma tiene un prefijo. Al fabricarla sale uno al azar entre los que permite su nivel.
- Reforjar en el Yunque Astral cambia el prefijo por otro al azar. Nunca sale el mismo dos veces seguidas.
- **Mejorar conserva el prefijo.** Fusionar también: la legendaria hereda el del arma núcleo.
- Prefijos por nivel: los **menores** salen en todos; los **mayores**, desde Alquímico; **Legendario**, desde Astral.
- El azar usa el generador con semilla del núcleo, como todo lo demás.

| Nivel del arma | Almitas | Sal del sello, cruda |
| --- | --: | --: |
| Rústico | 400 | 1 |
| Forjado | 1 200 | 2 |
| Alquímico | 3 500 | 3 |
| Astral | 8 000 | 4 |
| Quintaesencia y legendaria | 15 000 | 5 |

### 9.2 Los 25 prefijos

Los siete del GDD 15.4 están marcados. Los pesos suman 1 000; la probabilidad real es el peso entre la suma de los prefijos que permite el nivel.

| Prefijo | ID | Categoría | Rango | Efecto | Peso |
| --- | --- | --- | --- | --- | --: |
| Mordiente | `mordiente` | Daño | Menor | +5 % de daño | 70 |
| Plúmbeo | `plumbeo` | Daño | Menor | +9 % de daño; −3 de VEL | 55 |
| Certero | `certero` | Daño | Menor | +5 % de probabilidad de crítico | 60 |
| Turbio (GDD) | `turbio` | Daño | Mayor | +12 % de daño; +5 de Salinidad por Hechizo | 25 |
| Desquiciado (GDD) | `desquiciado` | Daño | Mayor | +15 % de probabilidad de crítico; −10 % de defensa (en la fórmula 9.8, R_DEF de 0.10 cuando la golpean) | 20 |
| Corrosivo | `corrosivo` | Daño | Mayor | +20 % de daño crítico | 20 |
| Rítmico (GDD) | `ritmico` | Ritmo | Menor | Todos sus golpes caen 2 frames antes | 50 |
| Pausado (GDD) | `pausado` | Ritmo | Menor | Todos sus golpes caen 2 frames después; +8 % de daño | 50 |
| Volátil | `volatil` | Ritmo | Menor | +4 de VEL | 60 |
| Armónico | `armonico` | Ritmo | Mayor | Sus golpes con Spark hacen +65 % en vez de +50 % | 20 |
| Templado | `templado` | Sal | Menor | Sus Hechizos suben +7 de Salinidad en vez de +10 | 60 |
| Salobre | `salobre` | Sal | Menor | +8 % de daño mientras su Salinidad está en 40 o más | 55 |
| Salino (GDD) | `salino` | Sal | Mayor | +1 Grano de Sal al iniciar el combate; máximo +1 por equipo | 20 |
| Destilado | `destilado` | Sal | Mayor | +10 % de daño de su planeta dominante | 20 |
| Coral | `coral` | Ánima | Menor | +15 de Ánima al iniciar el combate | 60 |
| Resonante | `resonante` | Ánima | Menor | Su ataque básico da +5 de Resonancia en vez de +4 | 55 |
| Astral (GDD) | `astral` | Ánima | Mayor | +10 % de Ánima ganada | 20 |
| Calcinado | `calcinado` | Defensa | Menor | +6 % de vida | 70 |
| Sereno | `sereno` | Defensa | Menor | −8 % de daño recibido mientras su Salinidad es Estable | 55 |
| Fijo | `fijo` | Defensa | Mayor | −12 % de daño recibido; −3 de VEL | 20 |
| Tenaz | `tenaz` | Defensa | Mayor | Una vez por combate, sobrevive con 1 de vida a un golpe mortal | 10 |
| Herbolario | `herbolario` | Utilidad | Menor | +15 % de potencia de las Salmueras que usa | 55 |
| Peregrino | `peregrino` | Utilidad | Menor | +5 % de almitas al ganar; máximo uno por equipo | 40 |
| Quebrador | `quebrador` | Utilidad | Mayor | +15 % de daño a la Firmeza | 20 |
| Legendario (GDD) | `legendario` | General | Legendario | +12 % de daño, +5 % de probabilidad de crítico, +8 % de Ánima ganada y +3 de VEL | 10 |

Resumen: 14 menores (795 de peso), 10 mayores (195) y Legendario (10). Por categoría: daño 6, ritmo 4, sal 4, Ánima 3, defensa 4, utilidad 3 y general 1. Los nombres nuevos vienen del vocabulario alquímico: mordiente, calcinación, fijación, volatilidad.

**Legendario en números.** Es el mejor prefijo general porque suma casi todo sin costo: el daño de Turbio sin su Salinidad, el crítico de Certero, Ánima y velocidad. No es el mejor en todo: no mueve frames, así que un Rítmico que cierra un Spark puede valer más en una Cadena concreta. Esa es la decisión que queremos.

### 9.3 Prefijos de ritmo: por qué solo ±2

Solo Rítmico y Pausado mueven frames. No propongo prefijos de ±1 ni de ±4.

- **±1 es una trampa.** En la rejilla de 2 frames, un arma con ±1 vuelve impares todas sus distancias a las demás. Los golpes que estaban a 0 pasan a 1 y siguen haciendo Spark; los que estaban a 2 de un lado pasan a 1 y siguen; los del otro lado pasan a 3 y lo pierden. Ningún golpe que estaba a 4 llega a 2. Es un prefijo que solo puede quitar Spark. Además, un golpe en frame impar cae entre dos poses de una animación hecha a 30 fps.
- **±4 le quita su papel a la Directora de Coro.** El Sendero mueve un eslabón hasta 6 frames (GDD 13.3); si un prefijo moviera 4, el Sendero sobraría. Con ±4 y ±2 juntos, además, casi todos los pares de la matriz se volverían alcanzables y el acertijo desaparecería.
- La categoría de ritmo se completa con prefijos que no mueven frames: Volátil (velocidad) y Armónico (Spark más fuerte).

### 9.4 Protección contra la frustración

Nada de esto se vende. Ni reforjas, ni Temple, ni Memoria (GDD 16.4).

1. **Memoria del Yunque.** El Yunque recuerda los últimos 3 prefijos de cada arma (6 con el Telar). Volver a uno cuesta lo mismo que una reforja. Así nunca pierdes un buen prefijo por probar.
2. **Temple.** Contador de cuenta, visible y que nunca caduca. Cada reforja de un arma Astral, Quintaesencia o legendaria que no da Legendario suma 1. La reforja número 25 te deja elegir cualquier prefijo, y el contador vuelve a 0 cuando sale o eliges Legendario. Es el pity de la reforja.
3. **Reforja dirigida** (Mesa del Ensayador). Eliges la categoría y el prefijo sale de ella. Cuesta el doble y nunca da Legendario.
4. **Reforja barata y sube.** Como mejorar conserva el prefijo, puedes buscar Rítmico en una Rústica y llevarlo hasta Quintaesencia.
5. **Sin bloqueo.** Con un solo prefijo por arma, bloquearlo es lo mismo que no reforjar. La Memoria cubre ese caso.

Cuentas:

- Rítmico en una Rústica: 50 de 795, unas 16 reforjas, 6 400 almitas y 16 Sal Cruda. Con reforja dirigida de ritmo: 50 de 160, unas 3 reforjas, 2 600 almitas.
- Legendario en una Astral: 1 %. En promedio 22 reforjas, 178 000 almitas. Como máximo 25, 200 000 almitas, unos 2 días de juego. Solo 21 % de los jugadores lo saca antes del Temple, así que el Temple es el camino normal.

## 10. Salmueras

### 10.1 Reglas de receta

- **Receta = base + sal + reactivo + almitas** (GDD 14.1). Una unidad de cada ingrediente y sale una Salmuera.
- **La pureza fija la potencia:** Sal Cruda da la básica, Destilada la mejorada y Quintaesencia la maestra.
- **Los números del GDD 14.2 son la potencia mejorada.** Las brujas de la Fase 1 están en grado B, la fase Destilada. La básica da cerca de 60 % y la maestra cerca de 150 %.
- Tres Salmueras no tienen planeta: Elixir de Sal Negra, Esencia de Alma y Agua de Olvido. Usan una **sal de anclaje**: cualquier sal planetaria, cuya pureza fija la potencia.
- Descubrir una receta revela sus tres potencias.
- Almitas por potencia: básica 200, mejorada 800, maestra 3 000. La Esencia de Alma pide además sus "almitas concentradas" (GDD 14.2): 1 000, 3 000 y 8 000 más; por eso su fila marca 1 200, 3 800 y 11 000. Así convierte almitas en Ánima y no crea almitas de la nada (GDD 14.3).

### 10.2 Bases y reactivos

| Base | ID | Dónde se consigue |
| --- | --- | --- |
| Agua termal | `agua_termal` | Gratis e ilimitada en las Termas |
| Vinagre | `vinagre` | Caldero de Bruja, 50 almitas |
| Leche lunar | `leche_lunar` | Caldero de Bruja en noches de luna llena real (GDD 7.7), 100 almitas; todos los días con la Pila de Nácar |

| Reactivo | ID | Dónde se consigue | Salmuera |
| --- | --- | --- | --- |
| Arena Áurea | `arena_aurea` | Desierto Áureo (Leo) | Salmuera Áurea (es el *oro potable* de los alquimistas) |
| Rocío del Árbol | `rocio_arbol` | Fresno: Nidos de Almas y Santuarios | Destilado de Rocío del Árbol |
| Sal Negra | `sal_negra` | Fresno, élites y eventos de luna nueva (GDD 11.1) | Elixir de Sal Negra |
| Fuego fatuo | `fuego_fatuo` | Raíces Putrefactas del Fresno; Pantanos de Azufre (Escorpio) | Esencia de Alma |
| Flor de sal | `flor_sal` | Salinas de Obsidiana (Capricornio); cualquier salina en luna llena | Filtro de Pureza |
| Nácar de Marea | `nacar_marea` | Costa de las Conchas (Cáncer) | Leche de Luna |
| Mena de Hierro Rojo | `mena_hierro_rojo` | Montes de Hierro Rojo (Aries) | Vinagre de Marte |
| Pluma de vencejo | `pluma_vencejo` | Ciudad Espejo (Géminis) y Estepas del Centauro (Sagitario) | Tónico de Azogue |
| Lágrima de Almáciga | `lagrima_almaciga` | Isla de la Ninfa (Piscis) y Abadía de las Herbolarias (Virgo) | Bálsamo Cúprico |
| Arena de reloj | `arena_reloj` | Ducado de las Balanzas (Libra) | Plomo Líquido |
| Sal dulce | `sal_dulce` | Llanuras de Cobre (Tauro), la sal que borra recuerdos | Agua de Olvido |

- Cuatro reactivos son también materiales de armas (Arena Áurea, Nácar de Marea, Mena de Hierro Rojo y Lágrima de Almáciga). Así hay menos objetos que aprender.
- El Mercader de Almas del Fresno vende todos los reactivos regionales con almitas. Ningún jugador se queda sin una Salmuera por el orden en que recorre las regiones.

### 10.3 Matriz de recetas

| Salmuera | ID | Base | Sal | Reactivo | Almitas | Básica (Cruda) | Mejorada (Destilada, GDD) | Maestra (Quintaesencia) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Salmuera Áurea | `salmuera_aurea` | Agua termal | Áurea | Arena Áurea | 200 / 800 / 3 000 | +1 Grano y +6 % de ATQ por 2 turnos | +1 Grano y +10 % de ATQ por 2 turnos | +1 Grano y +15 % de ATQ por 3 turnos |
| Destilado de Rocío del Árbol | `destilado_rocio` | Agua termal | Argenta | Rocío del Árbol | 200 / 800 / 3 000 | −25 de Salinidad; adelanta 15 % | −40 de Salinidad; adelanta 30 % | −60 de Salinidad; adelanta 40 % |
| Elixir de Sal Negra | `elixir_sal_negra` | Vinagre | Anclaje | Sal Negra | 200 / 800 / 3 000 | Siguiente Golpe Astral ×1.5; curas invertidas 1 turno | ×2; curas invertidas 1 turno | ×2.5; curas invertidas 1 turno |
| Esencia de Alma | `esencia_alma` | Agua termal | Anclaje | Fuego fatuo | 1 200 / 3 800 / 11 000 | +20 de Ánima | +30 de Ánima | +45 de Ánima |
| Filtro de Pureza | `filtro_pureza` | Agua termal | Cualquier Destilada o Quintaesencia | Flor de sal | — / 800 / 3 000 | No existe | Un Vislumbre sin gastar Sal Negra | Un Vislumbre sin Sal Negra y sin +30 de Salinidad |
| Leche de Luna | `leche_luna` | Leche lunar | Argenta | Nácar de Marea | 200 / 800 / 3 000 | Escudo de 12 % a todas por 2 turnos | Escudo de 20 % a todas por 2 turnos | Escudo de 30 % a todas por 2 turnos |
| Vinagre de Marte | `vinagre_marte` | Vinagre | Férrea | Mena de Hierro Rojo | 200 / 800 / 3 000 | El siguiente golpe quita +30 % de Firmeza | +50 % | +80 % |
| Tónico de Azogue | `tonico_azogue` | Agua termal | de Azogue | Pluma de vencejo | 200 / 800 / 3 000 | +12 de VEL por 2 turnos | +20 de VEL por 2 turnos | +30 de VEL por 2 turnos |
| Bálsamo Cúprico | `balsamo_cuprico` | Agua termal | Cúprica | Lágrima de Almáciga | 200 / 800 / 3 000 | Cura 15 % y limpia 1 estado | Cura 25 % y limpia 1 estado | Cura 35 % y limpia 2 estados |
| Plomo Líquido | `plomo_liquido` | Vinagre | Plúmbea | Arena de reloj | 200 / 800 / 3 000 | Retrasa 12 % a un enemigo | Retrasa 20 % | Retrasa 30 % |
| Agua de Olvido | `agua_olvido` | Agua termal | Anclaje | Sal dulce | 200 / 800 / 3 000 | Fuera de combate: −12 de Paralaje | −20 de Paralaje | −30 de Paralaje |

Cambios frente al GDD 14.2, todos marcados en la sección 15:

- El Destilado de Rocío usa Rocío del Árbol en vez de Savia, como propone HUECOS C-22.
- El Bálsamo Cúprico usa Lágrima de Almáciga en vez de "savia", por la misma razón.
- El Filtro de Pureza no tiene básica, porque el GDD ya le pide Sal Destilada.
- La duración de 2 turnos de la Leche de Luna es la que eligió la Fase 1.

### 10.4 Salmuera Turbia

Sale cuando la mezcla no es ninguna receta. Se guarda y se lleva en el cinturón como cualquier Salmuera (máximo cinco Turbias) y su efecto se sortea al beberla, con la semilla del combate. Nunca castiga fuerte: lo peor es +10 de Salinidad o un retraso de 5 %, y nunca empuja al Éxtasis Abisal.

| Efecto | ID | Peso | Qué hace |
| --- | --- | --: | --- |
| Hipo de burbujas | `turbia_hipo` | 20 | +5 de Ánima; la bruja hipa burbujas un turno |
| Estornudo de sal | `turbia_estornudo` | 15 | −10 de Salinidad; estornuda una nube de sal |
| Pelaje esponjado | `turbia_pelaje` | 15 | +5 de VEL un turno; el pelaje o las plumas se esponjan |
| Pompa astral | `turbia_pompa` | 12 | Escudo de 5 % de su vida que revienta con el primer golpe y suena a pompa de jabón |
| Mareo dulce | `turbia_mareo` | 10 | Cura 5 % y retrasa 5 % su siguiente acción |
| Lengua azul | `turbia_lengua` | 10 | No hace nada; la lengua le queda azul el resto del combate |
| Eco de un desconocido | `turbia_eco` | 10 | Dice una línea de un alma con nombre (un fragmento de historia, GDD 11.2) y suelta 1 almita |
| Salinidad caprichosa | `turbia_capricho` | 8 | ±10 de Salinidad al azar, con tope de 149 |

Un guiño de "kk": si Salitre, la cabra que se intoxica con sal, bebe una Turbia, siempre le sale Hipo de burbujas y gana +10 de Ánima extra. Le encanta.

### 10.5 Descubrimiento sin wiki: el Recetario

1. **Todo está a la vista.** Desde el inicio, el Recetario muestra las 11 Salmueras con su efecto y tres casillas en blanco: base, sal y reactivo.
2. **La Herbolaria llena casillas.** En cuanto has tenido un ingrediente, ella escribe en qué receta va (GDD 15.5). Con los tres, la receta queda completa y la preparas sin experimentar. En el peor caso, juegas y la receta se completa sola.
3. **Experimentar es un atajo divertido.** El Caldero acepta cualquier base, sal y reactivo. El vapor da una pista con los colores de las Termas (GDD 16.1): **blanco** si la mezcla no se parece a ninguna receta que te falta, **violeta** si acertaste dos de tres ingredientes de una, **dorado** si es una receta nueva.
4. **Sueño de la Ninfa** (Piscis): una vez al día, el Caldero te dice qué saldría antes de mezclar.
5. **La Turbia es colección.** El Recetario guarda los 8 efectos que has visto. Encontrarlos todos da el título *Catadora de Turbias*.
6. **Armas:** la Herbolaria muestra toda receta de arma que use algo que ya tuviste, y el Ensayo del Yunque te deja ver sus notas en tu Partitura antes de fabricarla.

## 11. Economía del crafting

### 11.1 Las escalas del GDD no cuadran entre sí

`Costo(n) = 100·n^1.5` es el costo de pasar del nivel *n* al *n*+1. La meta del GDD 11.3, nivel 70 en siete días, fija el ingreso diario: 1 610 673 / 7 ≈ **230 000 almitas al día**.

| Concepto | Cuenta | Almitas | Días de juego |
| --- | --- | --: | --: |
| Nivel 1 a 80 | Σ 100·n^1.5, n = 1 a 79 | 2 254 066 | 9.8 |
| Nivel 1 a 70 | n = 1 a 69 | 1 610 673 | 7.0 |
| Nivel 70 a 80 | n = 70 a 79 | 643 393 | 2.8 |
| Grados D a SS | 20 000 · (1 + 1.8 + 1.8² + 1.8³ + 1.8⁴) = 20 000 · 22.37 | 447 392 | 1.9 |
| Solo S a SS | 20 000 · 1.8⁴ | 209 952 | 0.9 |

Problemas:

1. **Los grados cuestan 20 % de los niveles.** El GDD los presenta como la progresión principal (pilar 3), pero pesan cinco veces menos.
2. **SS no es meta de largo plazo en almitas.** El GDD 12.3 dice que lo es porque concentra 47 % del costo de grados, pero cuesta menos de un día. Lo que lo frena es el Juicio V y el nivel 80, no las almitas.
3. **Los últimos diez niveles tampoco** son un "sumidero de largo plazo" (11.3): cuestan 2.8 días.
4. **El crafting no tiene dónde caer.** Si un arma pesa como el nivel, cuesta más que todos los grados juntos. Si pesa como los grados, es un trámite.

### 11.2 Escala propuesta

Adopto la recomendación D-02 de la hoja de ruta: **el nivel cuesta `40·n^1.5`**. Los grados se quedan como en el GDD. La meta de siete días se mantiene, así que el día de juego baja a 644 269 / 7 ≈ **92 000 almitas**. Diseño el crafting en días de juego para que se pueda reescalar.

| Concepto | Almitas | Días de juego | Peso relativo |
| --- | --: | --: | --: |
| Nivel 1 a 80 | 901 626 | 9.8 | 2 |
| Nivel 70 a 80 | 257 357 | 2.8 | |
| Grados D a SS (sin cambio) | 447 392 | 4.9 | 1 |
| Arma de Rústico a Quintaesencia | 416 300 | 4.5 | 1 |
| Legendaria, con las armas que funde | 832 600 a 942 800 | 9 a 10 | 2 |
| Una bruja completa: nivel 80, SS y arma Quintaesencia | 1 765 318 | 19 | |

Con esta escala, nivel, grados y arma pesan 2 : 1 : 1. Una legendaria pesa lo que subir una bruja del 1 al 80. Si se prefiere la otra salida de HUECOS H-01 (grados con base 80 000 y nivel sin cambio), multiplica todas las almitas de este documento por 2.5. El arma seguiría pesando 4.5 días, pero los grados pasarían a 7.8 días, casi lo mismo que el nivel.

### 11.3 Refinar sal en el Altar del Árbol

| Refinación | Entra | Almitas | Con estación regional |
| --- | --- | --: | --: |
| 1 Sal Destilada | 3 Sal Cruda del mismo planeta | 400 | 300 con el Horno de Obsidiana |
| 1 Sal Quintaesencia | 3 Sal Destilada del mismo planeta | 2 500 | 1 875 con el Secadero de Abedul |

Una Quintaesencia equivale a 9 Sal Cruda y 3 700 almitas. Esto también responde la parte de refinación de HUECOS H-11.

### 11.4 Costo de un arma de Rústico a Quintaesencia

| Paso | Receta | Sal | Refinar | Acumulado | Equivale a |
| --- | --: | --- | --: | --: | --- |
| Rústico | 4 000 | 2 Cruda | 0 | 4 000 | Poco más que el paso del nivel 20 (3 578) |
| Forjado | 12 000 | 4 Cruda | 0 | 16 000 | Unos 3 niveles cerca del 20 |
| Alquímico | 36 000 | 3 Destilada | 1 200 | 53 200 | Casi 4 niveles cerca del 40 (10 119 cada uno) |
| Astral | 100 000 | 5 Destilada | 2 000 | 155 200 | Unos 6 niveles cerca del 55 (16 316 cada uno) |
| Quintaesencia | 250 000 | 3 Quintaesencia | 11 100 | 416 300 | Los últimos diez niveles (257 357) |

Cuentas: recetas 4 000 + 12 000 + 36 000 + 100 000 + 250 000 = 402 000. Refinar: 3 × 400 + 5 × 400 + 3 × (2 500 + 3 × 400) = 1 200 + 2 000 + 11 100 = 14 300. Total: **416 300 almitas** y 57 Sal Cruda del sello. Materiales: 6 Astillas, 10 Costras, 8 Esquirlas, 14 comunes y 6 de jefe de la región, y 2 Hojas de Alma.

Con la escala actual del GDD, para pesar los mismos 4.5 días el arma tendría que costar 1 040 000 almitas: más del doble de todos los grados. Esa es la prueba de que las escalas del GDD no cuadran.

### 11.5 Legendarias, reforja y Salmueras

- **Legendaria típica:** línea Quintaesencia de su familia (416 300) + línea Astral de la otra familia (155 200) + 250 000 de fusión + 3 Quintaesencia (11 100) = **832 600**, unos 9 días.
- **Reloj del Eclipse Verdadero:** Péndulo (4 000 de la Daga, 16 000 de la Campana de Bronce, 36 000 y 1 200 de la fusión = 57 200) + 4 líneas Astrales (620 800) + 250 000 + 4 Quintaesencia (14 800) = **942 800**, unos 10 días. Es la más cara porque es la del GDD.
- **Reforja:** buscar Rítmico cuesta unas 6 400 almitas; Legendario, 178 000 en promedio y 200 000 como máximo (sección 9.4).
- **Salmueras:** básica 200 almitas y 1 Cruda; mejorada 800 y 1 Destilada (1 200 en total); maestra 3 000 y 1 Quintaesencia (6 700 en total). Un cinturón de tres maestras para una run del Fresno cuesta unas 20 000 almitas: un sumidero de consumo para veteranos.

### 11.6 Para la tabla de ingresos (H-01)

El crafting asume unas 92 000 almitas por día de juego. Cuando exista la tabla de ingresos por actividad, este número es el que hay que comprobar. Si el ingreso real resulta distinto, todos los costos de este documento se escalan con un solo factor `// TUNING`.

## 12. Datos para la Fase 2B

Formato sugerido, compatible con el de la Fase 1 (`[frame, parte]`, partes que suman 1).

```jsonc
// families.json
{ "id": "guadana", "name": "Guadaña de Sal", "sealBase": 5,
  "basicHits": [[16, 0.45], [42, 0.55]],
  "chainHits": [[26, 0.45], [52, 0.55]],          // TUNING
  "secondary": { "stat": "toughnessDamage", "byTier": [0.05, 0.09, 0.13, 0.18, 0.24] },  // TUNING
  "trait": "siega", "region": "aries", "station": "fragua_hierro_rojo" }

// weapons.json
{ "id": "guadana_astral_b", "family": "guadana", "tier": "astral", "variant": "b", "seal": "sol",
  "recipe": { "previousTierOf": "guadana",
              "items": [["mena_hierro_rojo", 8], ["cuerno_carnero_corrupto", 2], ["esquirla_astral", 4], ["sal_sol_destilada", 5]],
              "almitas": 100000 },                 // TUNING
  "stations": ["fragua_hierro_rojo", "yunque_astral"] }
```

Bases de sello por familia: Báculo 4, Grimorio 4, Guadaña 5, Incensario 3, Astrolabio 6, Rifle 1, Escoba 2, Campana 6, Tijeras 0, Caldero de mano 1, Varita 0, Daga 2.

Pruebas que propongo para la Fase 2B:

- **Prueba dorada de la matriz:** `PreviewChain` con dos brujas sin voz da los 144 resultados de la sección 2.3.
- **Prefijo:** Rifle seguido de Escoba no hace Spark; con Pausado en la Escoba, sí (frames 44 y 42).
- **Orden:** Incensario, Grimorio, Daga y Escoba dan 9 golpes con Spark; en orden inverso, 0.
- **Validador:** frames pares; básico y Cadena con los mismos intervalos; repartos que suman 100; sellos que cumplen la regla de la sección 4; toda receta apunta a IDs que existen.

## 13. Valores TUNING

Todo lo que no viene del GDD. Agrupado por sistema.

| Sistema | Valores |
| --- | --- |
| Capas | Mano 70 % y voz 30 % en Apertura y Enlace; 40 % y 60 % en el Cierre. Regla de migración de la voz de la Fase 1. Voz entre los frames 40 y 90 |
| Spark | Tope de +10 de Ánima por Spark por bruja y Cadena. Directora de Coro en pasos de 2. Umbral del simulador: 20 % sobre la mediana del índice de equipo. Tránsito con +10 % de daño de Spark a dos familias |
| Patrones | Los 12 patrones de básico y de Cadena, sus entradas y sus repartos (sección 2.1) |
| Rasgos | Constancia +15 %; Costura 3 agujas y +10 de Ánima; Siega 40 %; Humo lento +4 frames; Marca de Aspecto +25 %; Tiro de gracia +20 %; Viento de cola 15 %; Eco diferido 8 % y 15 %; Hilo corto 5 % y +15 %; Salpicadura 50 %; Chispa +1 Grano por ronda; Cuarto filo +20 % |
| Niveles | ATQ 60, 150, 270, 420 y 600; multiplicador de secundaria y sus 60 valores; nivel de bruja sugerido; afinidad +25 % |
| Sellos | Los 35 valores de pasiva; afinidad ×2; regla de reparto y bases por familia |
| Recetas | Cantidades de la plantilla por nivel; almitas 4 000, 12 000, 36 000, 100 000 y 250 000; receta del Péndulo; fusiones legendarias y su costo de 250 000 |
| Materiales | Hoja de Alma: 2 o 3 por run de la Copa Astral y 2 por semana; fuentes de cada material; precios de los reactivos en el Mercader de Almas, por definir |
| Estaciones | Recocido 50 %; Molienda 3 por 2; Espejo 25 %; Trueque 2 por 1; Salina y Destilación en frío −25 %; Malditas +20 de Salinidad; Memoria 3 y 6 |
| Refinación | Destilada: 3 Cruda y 400 almitas; Quintaesencia: 3 Destilada y 2 500 almitas |
| Reforja | Costos por nivel; 25 pesos; Temple en 25; reforja dirigida ×2; rangos por nivel |
| Prefijos | Todos los valores de los 18 prefijos nuevos y los números de Legendario |
| Salmueras | Potencias básica y maestra de las 11; almitas 200, 800 y 3 000; almitas concentradas 1 000, 3 000 y 8 000; precio del vinagre (50) y la leche lunar (100) |
| Turbia | Los 8 efectos y sus pesos; tope de Salinidad en 149 |
| Economía | Nivel con `40·n^1.5`; día de juego de 92 000 almitas; factor 2.5 para la escala actual |

## 14. Preguntas para el diseñador

| # | Pregunta | Mi recomendación |
| --: | --- | --- |
| 1 | ¿El arma pone la mano y el Ascendente la voz, o prefieres que el arma solo desplace los frames de la bruja? | Mano y voz. Es lo único que hace que el GDD 9.6 y 15 se cumplan sin borrar a la bruja |
| 2 | ¿Cualquier bruja puede usar cualquier familia? | Sí, con familia afín (+25 % a la secundaria). Restringir familias encoge el acertijo del Spark |
| 3 | ¿Bajamos el nivel a `40·n^1.5` (D-02) o subimos los grados? | Bajar el nivel. Mantiene los grados del GDD y deja nivel, grados y arma en 2 : 1 : 1 |
| 4 | ¿Mejorar un arma conserva su prefijo? | Sí. Es la protección más simple contra la frustración |
| 5 | ¿Las legendarias tienen límite por equipo? | No por ahora. Son laterales; el simulador decide si hace falta un límite de dos |
| 6 | ¿Los números del GDD 14.2 son la potencia básica o la mejorada? | Mejorada, porque la Fase 1 juega en grado B |
| 7 | "Máximo cinco de cada tipo" (GDD 14.3): ¿por Salmuera o por Salmuera y potencia? | Por Salmuera y potencia. Si no, una básica estorba a una maestra |
| 8 | ¿"Segunda región" (GDD 15.3) es Tauro? | Sí: segundo signo, Llanuras de Cobre, y el bronce es cobre con estaño |
| 9 | ¿Qué da el gacha cuando ya tienes todas las 3★ (GDD 12.2)? | Polvo Astral, como cuando el banner está completo. Ni recetas ni Sal Cruda (sección 15) |
| 10 | La almita extra del Spark (GDD 9.6) es simbólica frente a 92 000 al día. ¿La cambiamos? | Que tenga 10 % de ser una almita con nombre, con su fragmento de historia |
| 11 | ¿Báculo, Caldero de mano y Varita reciben legendaria en 1.1 a 1.3? | Sí, una por versión. El Grimorio espera a ver datos de uso |
| 12 | ¿Ascensiones y Tempestad cuentan para el tope de Ánima por Spark? | Las Ascensiones no (no son Cadena); la Tempestad sí |
| 13 | ¿Las estaciones regionales se quedan en su región o viajan en la Tortuga? | Viajan: se instalan en el caparazón. Las regiones van en cualquier orden y volver a cada una para fabricar sería tedioso. "Dos estaciones a la vez" pasa a ser "las dos instaladas" |

## 15. Contradicciones y huecos del GDD

| # | Dónde | Problema | Propuesta |
| --: | --- | --- | --- |
| 1 | 19.2 contra 9.6 y 15 | 19.2 ata los frames de la familia al ataque básico, pero el básico nunca puede hacer Spark: en un turno normal solo golpea una bruja. Si el arma cambiara solo el básico, no cambiaría ningún Spark | El patrón de la familia también pone la mano de la Cadena (sección 1.1) |
| 2 | 15.3 y 15.5 | El Péndulo de Resonancia es "Alquímico", pero no es una de las 120 (12 × 5 × 2) ni una de las 8 legendarias | Es la Campana Alquímica *b*: cuenta entre las 120 y su receta es la fusión |
| 3 | 15.3 y 6 | "Botín de la segunda región": las regiones van en cualquier orden, así que no hay una segunda fija | Tauro, segundo signo; la Campana de Bronce también se fabrica |
| 4 | 12.2 contra 16.4 | Con las 3★ completas, el gacha da "recetas de armas y Sal Cruda". Las Sales de Baño se compran (Perlas 1:1), así que sería vender sal de forma indirecta, y la sal refinada sube grados, lo que contradice "nada de lo que sale del gacha sube grados" en la misma 12.2. Además, con la Herbolaria las recetas ya son gratis | Ese resultado da Polvo Astral |
| 5 | 14.1 contra 14.2 | La potencia la fija la pureza (Cruda = básica), pero el Filtro de Pureza pide Sal Destilada como ingrediente | El Filtro solo existe en mejorada y maestra |
| 6 | 14.2 y 11.1 | El Bálsamo Cúprico pide "savia", que se confunde con la Savia, moneda del Fresno. Es el mismo problema que HUECOS C-22 marca en el Destilado de Rocío | Lágrima de Almáciga como reactivo |
| 7 | 14.1 contra 15.5 | Las Salmueras "se descubren experimentando", pero la Herbolaria "muestra todo lo que puedes fabricar con cualquier objeto que ya hayas tenido" | La Herbolaria completa una casilla por ingrediente que tuviste; experimentar es el atajo (sección 10.5) |
| 8 | 15.1 contra 17 | "El mejor para Spark" choca con la doctrina contra el power creep si significa mejor en todo | Mejor en número de pares (11), no en daño por Spark; índice de equipo solo 6 % sobre el siguiente |
| 9 | 13.1 y 7.4 contra 9.8 | La ficha muestra DEF 610 y varios efectos bajan la defensa, pero la fórmula de daño no usa una estadística de defensa: solo niveles y R_DEF. La Fase 1 tampoco tiene DEF | Las armas no dan DEF; los efectos de "defensa" se leen como R_DEF. Hay que decidir si DEF existe |
| 10 | 11.3 | No dice si *n* es el nivel al que llegas o del que sales: 2 325 520 contra 2 254 066 del 1 al 80 | Es el paso de *n* a *n*+1, como usa HUECOS |
| 11 | 12.3 | "SS se vuelve una meta de largo plazo" porque es 47 % del costo de grados, pero cuesta menos de un día del ingreso que implica el 11.3 | Ver la sección 11; lo que hace largo el SS es el Juicio V |
| 12 | 11.3 | "Los últimos diez niveles son el sumidero de largo plazo", pero cuestan 2.8 días | El sumidero de largo plazo es el elenco completo y la Ofrenda; ajustar el texto |
| 13 | 9.6 | "Ese golpe hace +50 %" no dice si el bono es para uno o para los dos golpes que coinciden | La Fase 1 se lo da a cada golpe marcado como Spark, y este documento asume lo mismo |
| 14 | 9.6 | La almita extra por Spark no pesa en la economía: 1 almita contra unas 92 000 al día | Pregunta 10 |
| 15 | 15.4 y 19.5 | No es contradicción, pero es una regla implícita: Rítmico, Pausado y las animaciones a 30 fps solo encajan si todo frame es par | Hacerla explícita en el GDD 9.6 (sección 1.2) |
| 16 | 13.3 | La Directora de Coro mueve "hasta 6 frames" sin decir el paso | Pasos de 2, por la misma regla |
| 17 | 14.3 | "Máximo cinco de cada tipo" no contempla tres potencias | Pregunta 7 |
