# Ambientes y enemigos para Codex: la Odisea

Encargo de arte para la Odisea de Sal y Almas (diseño en [docs/propuestas/ODISEA.md](propuestas/ODISEA.md)). Claude Code programa las zonas, las secciones y los combates. Codex pinta los fondos y los enemigos. El juego los carga solo por nombre de archivo, así que **los nombres de este documento son el contrato**.

## 1. Reglas del encargo

| Regla | Detalle |
| --- | --- |
| Carpeta | `C:\Arcanoria Master\Gusi Gaming\gacha-fase3` (worktree, rama `fase/3`) |
| Dónde guardar | Fondos en `Assets/SalYAlmas/Art/Ambientes/`, enemigos en `Assets/SalYAlmas/Art/Enemigos/`. Las fuentes grandes y el reporte, en `art/ambientes/` y `art/enemigos/` |
| Git | No hagas commits ni toques otras carpetas. Claude Code confirma tus archivos |
| Código | No edites `.cs` ni `.json`. Si un nombre no te sirve, anótalo en el reporte |
| Fondos | PNG horizontal de **1920 × 1080**, sin texto, sin personajes en primer plano, con el tercio inferior más tranquilo (ahí van las placas del combate) |
| Enemigos | PNG de **1024 × 1024** con fondo transparente, cuerpo completo, mirando a la izquierda (las brujas están a la izquierda), con 6 % de margen |
| Estilo | El de los retratos elegidos (`art/retratos/`, ancla de Marisal): pintura de pergamino al atardecer, oro y ciruela, sal en costras y cristales. Paleta del GDD 19.1 (`docs/paleta.svg`) |
| Tono | GDD 6.5: pavor antes que gore, nada de sangre, nada de sustos. Lo extraño se nota en la geometría, la escala y el cielo |
| Terraria | Es inspiración de **bioma** (superficie, subsuelo, cavernas, islas del cielo), nunca de criaturas ni de objetos concretos |
| Arcanoria | Puedes leer `C:\Arcanoria Master` para tomar ideas de atmósfera, pero **no uses ningún nombre, término, lugar ni símbolo de Arcanoria**. Es otro proyecto |
| Reporte | `art/ambientes/reporte.md`: qué entregaste, qué falta y qué decidiste por tu cuenta |

## 2. El mundo en una página

Halia es un anillo de doce regiones alrededor de un mar interior. En el centro crece el Fresno, un árbol más alto que el cielo, adonde viajan las almas por ríos de leche (los Ríos Lácteos). El árbol está enfermo, así que las almas se cristalizan en el camino y se vuelven sal. Las brujas comen esa sal para hacer magia. Arriba del cielo hay estrellas que no son estrellas: los Huéspedes, inmensos e indiferentes.

Cada región tiene cuatro zonas. Las tres primeras **bajan** y la cuarta **sube**:

| Zona | Archivo | Qué es |
| --- | --- | --- |
| La orilla | `amb_<region>_orilla.png` | La superficie del bioma: el pueblo, el camino, la luz del día |
| El subsuelo | `amb_<region>_subsuelo.png` | Lo que hay debajo: minas, cuevas, raíces. Más oscuro y estrecho |
| El corazón | `amb_<region>_corazon.png` | La sede de la Casa enemiga y el jefe. Grandioso y opresivo |
| El cielo (EX) | `amb_<region>_cielo.png` | Islas flotantes sobre la región; el cielo demasiado cerca. Hermoso y equivocado |

**Capas de horror (opcional, después de las cuatro zonas):** la misma zona en `amb_<region>_<zona>_grieta.png` (cristales de sal blanca que invaden el bioma, estrellas sin nombre de día) y `amb_<region>_<zona>_abismo.png` (el cielo bajo y lleno de ojos como constelaciones, geometría que se pliega, la gente a medio volverse sal). Si no existen, el juego tiñe el fondo base.

## 3. Prioridad 1: el prólogo y las cuatro regiones del lanzamiento

### Prólogo y pantallas generales

| Archivo | Prompt |
| --- | --- |
| `amb_vado.png` | Vado de un río de leche blanca cuajada que se detuvo y cruje como hielo, orillas de sal, juncos petrificados, figuras de sal a medio hacer que parecen personas. Atardecer. En el fondo, muy lejos y altísimo, la silueta de un árbol más alto que las nubes. Melancólico y quieto |
| `mapa_anillo.png` | Mapa pintado visto desde arriba, como un pergamino antiguo: un anillo de doce tierras distintas alrededor de un mar interior lechoso, con un árbol colosal en una isla en el centro. Cada tierra con su bioma (montañas rojas, llanuras con molinos, ciudad de canales, costa de conchas, desierto dorado, abadía entre huertos, ciudad de puentes y balanzas, pantanos amarillos, estepa, salinas negras, islas de hielo, isla de hongos luminosos). Sin texto ni etiquetas. 1920 × 1080 |

### Aries: Montes de Hierro Rojo

Inspiración: Iberia medieval, fortalezas sobre minas. Bioma: montaña y cavernas. Conflicto: una Casa guerrera saquea las minas de sal y da de beber sal fundida sin refinar a sus soldadas, que crecen y olvidan sus nombres.

| Archivo | Prompt |
| --- | --- |
| `amb_aries_orilla.png` | Pueblo minero de piedra rojiza en una ladera, torres de hierro rojo que salen de la montaña como dientes, humo de un incendio, carretas con costales de sal blanca, castillo al fondo. Luz de atardecer cobriza |
| `amb_aries_subsuelo.png` | Túnel de mina con vigas de madera, paredes de sal rosada y vetas de hierro, faroles de aceite. La sal de las paredes forma caras y manos borrosas, como si alguien respirara detrás. Estrecho y cálido |
| `amb_aries_corazon.png` | Patio de una fortaleza de hierro rojo con crisoles gigantes de sal fundida que brillan naranja, cadenas, estandartes con un carnero. Al fondo, una catedral natural de cristales de sal en la boca de la mina |
| `amb_aries_cielo.png` | Islas flotantes de mena de hierro roja sobre las montañas, con cascadas de arena de sal que caen al vacío. Una estrella grande en el cielo que no debería estar ahí, con un brillo que parece un párpado |

### Cáncer: Costa de las Conchas

Inspiración: Bretaña, mareas enormes, granito, faros, menhires. Bioma: océano y costa. Conflicto: cangrejos gigantes hunden los barcos salineros, y una Casa vive en el caparazón de uno.

| Archivo | Prompt |
| --- | --- |
| `amb_cancer_orilla.png` | Marea baja en una costa de granito rosa, barcas de vela parda varadas cargadas de sal, casas hechas dentro de conchas gigantes, un faro de piedra, menhires cubiertos de percebes. Neblina dorada |
| `amb_cancer_subsuelo.png` | Cueva marina que solo se abre con la marea baja, paredes de nácar iridiscente, pozas con medusas de luz, restos de barcos atrapados en la roca. Verde azulado con reflejos perla |
| `amb_cancer_corazon.png` | Un pueblo entero construido sobre el caparazón de un cangrejo colosal semihundido en la bahía, con escaleras, ropa tendida y una campana. Las pinzas salen del agua como torres. Tormenta que se acerca |
| `amb_cancer_cielo.png` | La marea sube hacia el cielo: columnas de agua que flotan en el aire, peces y conchas suspendidos, islas de arena boca abajo, medusas enormes que navegan entre las nubes. Hermoso y equivocado |

### Libra: Ducado de las Balanzas

Inspiración: ciudades-Estado italianas del Renacimiento y la banca. Bioma: la mazmorra (laberinto de ladrillo, cadenas, bóvedas). Conflicto: todo se paga con almas, y las cuentas se llevan en libros enormes.

| Archivo | Prompt |
| --- | --- |
| `amb_libra_orilla.png` | Ciudad de canales y casas de banca con logias de arcos, puentes con forma de balanza, mercaderes con libros de cuentas, faroles con almitas encerradas como luciérnagas. Atardecer ámbar |
| `amb_libra_subsuelo.png` | Bóvedas bajo el banco: pasillos de ladrillo que se repiten, rejas, cadenas, estantes infinitos de frascos con almitas etiquetadas, pesas de bronce enormes. Laberinto frío y ordenado |
| `amb_libra_corazon.png` | Salón ducal con una balanza colosal de oro en el centro, en un platillo un montón de sal blanca y en el otro una multitud de almitas luminosas. Mosaicos, columnas y un trono vacío |
| `amb_libra_cielo.png` | Páginas de un libro gigante flotando como islas, con renglones de luz; balanzas colgando de las estrellas con cadenas larguísimas; el cielo cuadriculado como papel de contabilidad |

### Capricornio: Salinas de Obsidiana

Inspiración: Mesoamérica, salinas de Zapotitlán y Cuyutlán, obsidiana, cacao. Bioma: obsidiana y capas profundas. Conflicto: dos remolinos de sal (la Boca Vieja, que traga barcas, y la Boca Nueva, que traga gente y deja las barcas) se comen las caravanas.

| Archivo | Prompt |
| --- | --- |
| `amb_capricornio_orilla.png` | Salinas como espejos negros hasta el horizonte, pozas de sal en terrazas, tendederos de cacao secándose, barcas de obsidiana, cerros áridos con cactus columnares. Cielo de tormenta morado y naranja |
| `amb_capricornio_subsuelo.png` | Caverna de obsidiana con vidrio negro y venas de magma que alumbran rojo, estalactitas de sal, un río subterráneo de salmuera que gira en espiral |
| `amb_capricornio_corazon.png` | Dos columnas colosales de sal que giran sin moverse de sitio sobre una laguna negra, una a cada lado; entre ellas, barcas pequeñas. Escala abrumadora, viento visible |
| `amb_capricornio_cielo.png` | Arriba de los remolinos: las dos columnas terminan en una nube con forma de ojo, islas de obsidiana flotando, granos de sal que suben como nieve al revés |

## 4. Enemigos de la prioridad 1

Cada región tiene un **nativo** con una mecánica, enemigos comunes y jefes. Hay tres invasores que aparecen en cualquier región cuando el cielo avanza. La mecánica está para que el diseño la deje leer en la silueta.

| Archivo | Nombre | Región | Papel | Prompt |
| --- | --- | --- | --- | --- |
| `enemigo_escoria_viva.png` | Escoria Viva | Aries | Común | Masa de escoria de hierro fundida que se arrastra, con costra negra y grietas naranjas, ojos como burbujas |
| `enemigo_perro_fragua.png` | Perro de Fragua | Aries | Común | Perro flaco de hierro remachado con brasas en el pecho y collar de cadena |
| `enemigo_soldada_crisol.png` | Soldada de Crisol | Aries | Nativa: sube su ataque cada turno | Soldada con armadura roja y casco de carnero, piel con vetas de sal brillante, mirada vacía, más grande de lo normal |
| `enemigo_ferran.png` | Capitana Ferrán | Aries | Jefa | Capitana veterana de armadura roja abollada, capa con carnero bordado, espada ancha, gesto cansado. Adulta, digna, no monstruosa |
| `enemigo_carnero_hierro.png` | Carnero de Hierro | Aries | Jefe (su espíritu) | Carnero espectral enorme de hierro rojo translúcido, cuernos en espiral, una grieta en el pecho con una estrella dentro |
| `enemigo_ave_a_medias.png` | El Ave a Medias | Aries EX | Jefe EX | Algo hecho de sal que tiene forma de ave pero no la entendió: alas de más, cuello que se dobla mal, plumas de cristal |
| `enemigo_medusa_salmuera.png` | Medusa de Salmuera | Cáncer | Común | Medusa gorda de salmuera turbia con cristales de sal flotando dentro |
| `enemigo_gaviota_hueca.png` | Gaviota Hueca | Cáncer | Común | Gaviota con el cuerpo de concha vacía y el pico de nácar |
| `enemigo_cangrejo_salinero.png` | Cangrejo Salinero | Cáncer | Nativo: protege a los demás | Cangrejo grande con una pinza enorme alzada como escudo y sal incrustada en el caparazón |
| `enemigo_madre_tenaza.png` | Madre Tenaza | Cáncer | Jefa | Cangrejo colosal con casitas y escaleras sobre el caparazón, pinzas como torres, ojos de faro |
| `enemigo_medusa_cielo.png` | Medusa del Cielo | Cáncer EX | Jefa EX | Medusa inmensa que flota entre nubes con tentáculos de agua que caen hacia arriba |
| `enemigo_moneda_andante.png` | Moneda Andante | Libra | Común | Moneda de bronce del tamaño de un perro que rueda de canto, con una cara acuñada que parpadea |
| `enemigo_guardia_contrapeso.png` | Guardia de Contrapeso | Libra | Común | Guardia de armadura pesada con una pesa de bronce colgada del brazo |
| `enemigo_tesorera_almas.png` | Tesorera de Almas | Libra | Nativa: cobra almitas | Mujer alta con libro de cuentas encadenado, anteojos de oro y frascos con almitas en el cinturón |
| `enemigo_duquesa_fiel.png` | La Duquesa del Fiel | Libra | Jefa | Duquesa renacentista con un vestido que se abre como platillos de balanza, la aguja de la balanza como cetro |
| `enemigo_contador_sin_rostro.png` | Contador sin Rostro | Libra EX | Jefe EX | Figura con toga hecha de páginas, sin cara, con manos de pluma que escriben en el aire |
| `enemigo_gota_obsidiana.png` | Gota de Obsidiana | Capricornio | Común | Gota de vidrio negro que rebota, con un reflejo morado y una burbuja de magma dentro |
| `enemigo_garza_obsidiana.png` | Garza de Obsidiana | Capricornio | Común | Garza de plumas de vidrio negro y patas largas, pico de navaja |
| `enemigo_sorbedor.png` | Sorbedor de Poza | Capricornio | Nativo: retrasa al equipo | Criatura pequeña que es un remolino con boca, de salmuera negra y espuma de sal |
| `enemigo_boca_vieja.png` | La Boca Vieja | Capricornio | Jefa (dúo) | Remolino de sal colosal con una boca vieja de dientes de sal en el centro, que traga barcas |
| `enemigo_boca_nueva.png` | La Boca Nueva | Capricornio | Jefa (dúo) | Remolino gemelo más limpio y blanco, con una boca joven y amable que da más miedo |
| `enemigo_serpiente_nube.png` | Serpiente de Nube | Capricornio EX | Jefa EX | Serpiente larguísima hecha de nubes de sal y escamas de obsidiana, con plumas de cacao tostado |
| `enemigo_figura_sal.png` | Figura de Sal | Prólogo | Común | Persona a medio volverse estatua de sal que todavía susurra, con la mano estirada |
| `enemigo_cristalizado.png` | Cristalizado | Invasor (Grieta) | Firmeza alta | Cuerpo humano cubierto de cristales de sal blanca que crecen hacia afuera como erizo; aún se ve la ropa |
| `enemigo_sin_carta.png` | Sin Carta | Invasor (Abismo) | Inmune a los Aspectos | Figura sin rasgos, de un negro sin brillo, con un círculo vacío donde iría una rueda natal |
| `enemigo_huesped_menor.png` | Huésped Menor | Invasor (Abismo) | Cambia de debilidad | Geometría imposible de metal líquido que se pliega, con ojos dispuestos como una constelación |

## 5. Prioridad 2: las otras ocho regiones

Mismo formato: cuatro zonas por región, más su nativo, dos comunes, una jefa y una jefa EX. Inventa los enemigos con el mismo criterio y anótalos en el reporte con su mecánica propuesta.

| Región | Inspiración | Bioma | Conflicto | Nativo propuesto |
| --- | --- | --- | --- | --- |
| `tauro` Llanuras de Cobre | Países Bajos, molinos | Bosque y pradera | Un pueblo come una sal dulce que borra recuerdos | Molinero de Sal Dulce (roba Granos) |
| `geminis` Ciudad Espejo | Venecia, canales | Sagrado: prismas y cristal | Dos hermanas mensajeras y un viento embotellado | Doble de Azogue (devuelve daño) |
| `leo` Desierto Áureo | Al-Ándalus y Magreb | Desierto | Tocar los rebaños solares trae castigo | Becerro Solar (se enfurece con Hechizos) |
| `virgo` Abadía de las Herbolarias | Monasterios centroeuropeos | Selva | Huertos que vuelven bestias sin alma a los visitantes | Novicia sin Alma (cura a las demás) |
| `escorpio` Pantanos de Azufre | Folclor de Europa del Este | Pantano y descenso | Hay que bajar al río de los muertos | Fatua del Azufre (atrae los golpes) |
| `sagitario` Estepas del Centauro | Nómadas de la estepa | Meteorito | Un gigante de un ojo custodia el paso | Cría de Estrella (explota al caer) |
| `acuario` Archipiélago de Hielo | Escandinavia e Islandia | Tundra | Cantos que atraen hacia el agua helada | Cantora Sumergida (sube la Salinidad) |
| `piscis` Isla de la Ninfa | Bizancio y Grecia | Hongos luminosos | Una isla de la que nadie quiere irse | Ninfa Hospitalaria (cura y quita Resonancia) |

## 6. Prioridad 3: capas de Grieta y Abismo

Las variantes `_grieta` y `_abismo` de las cuatro regiones del lanzamiento, empezando por la orilla y el corazón. Cuando las haya, el juego las usa en Expediciones con Marea alta.
