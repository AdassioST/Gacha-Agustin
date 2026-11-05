# Sal y Almas: la Odisea — entrega de ambientes y enemigos

Actualización: 5 de noviembre de 2026, Ciudad de México. Carpeta de trabajo: `C:\Arcanoria Master\Gusi Gaming\gacha-fase3`. No se usó git ni se modificaron código, JSON o metadatos de Unity.

## Estado de la entrega

Hay **104 PNG guardados: 53 fondos y 51 enemigos**. La prioridad 1 está completa: 18 fondos y 26 enemigos. De la prioridad 2 existen 46 de 72 piezas; dos fondos son provisionales. De la prioridad 3 existen 14 de 32 fondos.

Faltan **44 archivos nuevos**: 26 de prioridad 2 y 18 de prioridad 3. Además hay que rehacer `amb_leo_subsuelo.png` y `amb_virgo_subsuelo.png`: sus versiones actuales están demasiado abiertas a la luz y no cumplen suficientemente el descenso cerrado. Por tanto, hay 102 piezas aceptadas visualmente y 2 provisionales.

El generador integrado devolvió `429 / usage_limit_reached`. El restablecimiento indicado por el servicio es **2026-11-06 03:11:26 UTC**, equivalente al **5 de noviembre a las 21:11:26 en Ciudad de México**. No quedan llamadas en ejecución y no se generaron sustitutos ni se contó un prompt como imagen. El trabajo queda parcial por este límite externo, no por falta de prompts.

Existe una alternativa por CLI/API que requiere `OPENAI_API_KEY` y elección explícita del usuario; no se utilizó. No se configuró reanudación automática.

## Contrato, estilo y trazabilidad

- Referencias del encargo: [Ambientes para Codex](../../docs/Ambientes%20para%20Codex.md), [ODISEA](../../docs/propuestas/ODISEA.md), tono GDD 6.5 y paleta GDD 19.1.
- Ancla de pintura: [Marisal f1 v3](../retratos/marisal/marisal_f1_v3.png), solo para pergamino, línea sepia, oro/ciruela, materiales gastados y sal. No se tomaron nombres, lugares ni símbolos del otro proyecto.
- Modo empleado: **image_gen integrado**, una llamada por imagen o revisión. Pavor por silencio, geometría y escala; sin sangre, gore ni sustos.
- Fondos finales: `Assets/SalYAlmas/Art/Ambientes/`, PNG **1920 × 1080**. Sin rótulos ni personajes en primer plano; tercio inferior de menor contraste para las placas.
- Enemigos finales: `Assets/SalYAlmas/Art/Enemigos/`, PNG **RGBA 1024 × 1024**, cuerpo completo y orientación hacia la izquierda. Margen visible de al menos 6 % en cada borde, alfa auténtico conservado.
- Fuentes: `art/ambientes/fuentes/` y `art/enemigos/fuentes/`. Cada nombre base conserva `<id>_fuente.png`; las sustituciones aceptadas tienen `_fuente_r1.png`. Las primeras versiones se conservan como historial, no como finales alternativos para cargar.
- Los prompts exactos se guardan por pieza en `fuentes/<id>_prompt.md` y, cuando corresponde, `<id>_r1_prompt.md`. Los tres primeros prompts están en los agregados [ambientes](prompts.md) y [enemigos](../enemigos/prompts.md), que también sirven de índice.
- El generador no ofrece aquí un selector numérico de resolución. [exportar.ps1](exportar.ps1) normaliza las dimensiones: recorte central mínimo a 16:9 y escalado en fondos; encaje de silueta y alfa en enemigos. No se presenta el escalado como detalle nativo. No se pintó transparencia artificial.
- Inventario completo con nombre, estado, dimensiones, alfa, margen, SHA-256 y ruta: [inventario.tsv](inventario.tsv). Lista esperada: [contrato_entrega.tsv](contrato_entrega.tsv). Control técnico: [validacion.txt](validacion.txt).

## Prioridad 1

### Prólogo y mapa — completo

Entregados `amb_vado.png`, `mapa_anillo.png` y `enemigo_figura_sal.png`. El vado tiene leche cuajada, sal y juncos, figuras minerales a distancia y Fresno muy lejano. El mapa reúne los doce biomas alrededor del mar lechoso y el Fresno central, sin etiquetas. La Figura de Sal es una persona adulta vestida, susurrante y mineralizada, extendida hacia la izquierda. Revisión visual y de alfa completadas.

### Aries — Montes de Hierro Rojo — completo

Cuatro fondos: `amb_aries_orilla.png`, `amb_aries_subsuelo.png`, `amb_aries_corazon.png`, `amb_aries_cielo.png`.

6 enemigos: Escoria Viva (`enemigo_escoria_viva.png`); Perro de Fragua (`enemigo_perro_fragua.png`); Soldada de Crisol (`enemigo_soldada_crisol.png`); Capitana Ferrán (`enemigo_ferran.png`); Carnero de Hierro (`enemigo_carnero_hierro.png`); El Ave a Medias (`enemigo_ave_a_medias.png`).

Dientes verticales de hierro rojo, luz cobriza y sal rosada. La mina estrecha concentra el descenso; el crisol monumental y las islas de mena llevan la escala a lo imposible. La Escoria Viva se rehízo como masa rastrera de costra negra y burbujas naranjas, corrigiendo la primera interpretación humanoide.

Hoja de revisión: [hoja_aries.png](hoja_aries.png). Pendientes de capas opcionales: ver prioridad 3.

### Cáncer — Costa de las Conchas — completo

Cuatro fondos: `amb_cancer_orilla.png`, `amb_cancer_subsuelo.png`, `amb_cancer_corazon.png`, `amb_cancer_cielo.png`.

5 enemigos: Medusa de Salmuera (`enemigo_medusa_salmuera.png`); Gaviota Hueca (`enemigo_gaviota_hueca.png`); Cangrejo Salinero (`enemigo_cangrejo_salinero.png`); Madre Tenaza (`enemigo_madre_tenaza.png`); Medusa del Cielo (`enemigo_medusa_cielo.png`).

Siluetas de concha y granito rosa; nácar y verde azulado en la cueva. El corazón es la ciudad sobre el cangrejo y el cielo invierte la marea. Las dos medusas se corrigieron a animales cnidarios; la gaviota quedó aviar, con torso de concha vacía.

Hoja de revisión: [hoja_cancer.png](hoja_cancer.png). Pendientes de capas opcionales: ver prioridad 3.

### Libra — Ducado de las Balanzas — completo

Cuatro fondos: `amb_libra_orilla.png`, `amb_libra_subsuelo.png`, `amb_libra_corazon.png`, `amb_libra_cielo.png`.

5 enemigos: Moneda Andante (`enemigo_moneda_andante.png`); Guardia de Contrapeso (`enemigo_guardia_contrapeso.png`); Tesorera de Almas (`enemigo_tesorera_almas.png`); La Duquesa del Fiel (`enemigo_duquesa_fiel.png`); Contador sin Rostro (`enemigo_contador_sin_rostro.png`).

Simetrías, logias, bronce y ámbar en superficie. La bóveda se corrigió a azul pizarra y ciruela fría, con almas como luz; la balanza ducal y las páginas-isla amplían la opresión. Se conserva el orden contable como amenaza; el Contador no tiene rostro y su toga es de páginas sin escritura legible.

Hoja de revisión: [hoja_libra.png](hoja_libra.png). Pendientes de capas opcionales: ver prioridad 3.

### Capricornio — Salinas de Obsidiana — completo

Cuatro fondos: `amb_capricornio_orilla.png`, `amb_capricornio_subsuelo.png`, `amb_capricornio_corazon.png`, `amb_capricornio_cielo.png`.

6 enemigos: Gota de Obsidiana (`enemigo_gota_obsidiana.png`); Garza de Obsidiana (`enemigo_garza_obsidiana.png`); Sorbedor de Poza (`enemigo_sorbedor.png`); La Boca Vieja (`enemigo_boca_vieja.png`); La Boca Nueva (`enemigo_boca_nueva.png`); Serpiente de Nube (`enemigo_serpiente_nube.png`).

Espejos negros, terrazas, cacao y cactus bajo tormenta ciruela. Obsidiana y magma en el descenso; dos columnas-remolino estacionarias dominan la laguna, con barcas pequeñas entre ellas. Se regeneraron los seis enemigos para quitarles rasgos humanoides o de carnero ajenos a sus siluetas. Gota y garza son literales; Sorbedor y Bocas son remolinos; la serpiente es larga y mineral. El corazón r1 sustituyó dos pilares rígidos por tornados de sal. Pulido opcional: retirar un relieve de carnero que el ancla dejó en el subsuelo para reforzar la identidad local.

Hoja de revisión: [hoja_capricornio.png](hoja_capricornio.png). Pendientes de capas opcionales: ver prioridad 3.

La tabla del encargo asigna el nativo de Capricornio al Sorbedor de Poza; ODISEA también menciona a la Boca Vieja como ejemplo de retraso. Se respetaron todos los nombres contractuales; Claude Code decide su asignación de combate.

### Invasores compartidos — completos

Cristalizado (`enemigo_cristalizado.png`), Sin Carta (`enemigo_sin_carta.png`) y Huésped Menor (`enemigo_huesped_menor.png`). El primero conserva ropa bajo sal erizada; el segundo tiene círculo natal vacío y cuerpo negro mate sin rasgos; el tercero es geometría metálica plegada con ojos constelados. No se inventaron invasores adicionales. [Hoja de siluetas](hoja_invasores.png).

## Prioridad 2 — ocho regiones

Los 40 nombres siguientes son propuestas de arte y diseño para esta prioridad; los archivos de los enemigos de prioridad 1 conservan íntegro su contrato. Las mecánicas son propuestas **sin implementar**. Cada región conserva su descripción detallada y los prompts en `art/enemigos/<region>_diseno.md`.

### Llanuras de Cobre (tauro) — completo

Presentes: 9/9 (4 fondos, 5 enemigos). Horizontales de molinos, pradera y sauces; cobre oxidado, trigo y miel. El subsuelo r1 ya es cerrado, con raíces, ruedas y luz verde baja. La gran molienda y los molinos invertidos aumentan la extrañeza.

Entregado: `amb_tauro_orilla.png`, `amb_tauro_subsuelo.png`, `amb_tauro_corazon.png`, `amb_tauro_cielo.png`, `enemigo_molinero_sal_dulce.png`, `enemigo_liebre_cobriza.png`, `enemigo_espantajo_olvido.png`, `enemigo_madre_molienda.png`, `enemigo_ciervo_sin_sombra.png`.

[Hoja de revisión](hoja_tauro.png). [Diseño detallado](../enemigos/tauro_diseno.md).

Enemigos y mecánicas propuestas:

- **Molinero de Sal Dulce** — Nativo; Roba Granos; priorizarlo. Archivo: `enemigo_molinero_sal_dulce.png` (entregado).
- **Liebre Cobriza** — Común; Se adelanta tras esquivar; golpear con precisión. Archivo: `enemigo_liebre_cobriza.png` (entregado).
- **Espantajo del Olvido** — Común; Reduce la Resonancia un turno. Archivo: `enemigo_espantajo_olvido.png` (entregado).
- **Madre de la Molienda** — Jefa; Convierte los Granos robados en escudo; quebrar su molino. Archivo: `enemigo_madre_molienda.png` (entregado).
- **Ciervo sin Sombra** — Jefe EX; Alterna evasión y Firmeza cada turno. Archivo: `enemigo_ciervo_sin_sombra.png` (entregado).

### Ciudad Espejo (geminis) — completo

Presentes: 9/9 (4 fondos, 5 enemigos). Campaniles gemelos, canales de azogue, plata, cian opalino y lavanda. El subsuelo duplica puertas con prismas; el corazón encierra el viento y el cielo duplica ciudades. Frasco de Viento y Viento sin Envase se regeneraron como objetos y cintas de aire, sin figuras humanas añadidas.

Entregado: `amb_geminis_orilla.png`, `amb_geminis_subsuelo.png`, `amb_geminis_corazon.png`, `amb_geminis_cielo.png`, `enemigo_doble_azogue.png`, `enemigo_polilla_prisma.png`, `enemigo_frasco_viento.png`, `enemigo_hermana_reflejada.png`, `enemigo_viento_sin_envase.png`.

[Hoja de revisión](hoja_geminis.png). [Diseño detallado](../enemigos/geminis_diseno.md).

Enemigos y mecánicas propuestas:

- **Doble de Azogue** — Nativo; Devuelve parte del daño; pocos golpes grandes. Archivo: `enemigo_doble_azogue.png` (entregado).
- **Polilla de Prisma** — Común; Divide su golpe entre dos objetivos. Archivo: `enemigo_polilla_prisma.png` (entregado).
- **Frasco de Viento** — Común; Adelanta a un aliado al abrirse. Archivo: `enemigo_frasco_viento.png` (entregado).
- **La Hermana Reflejada** — Jefa; Invoca un reflejo que se disipa al quebrar Firmeza. Archivo: `enemigo_hermana_reflejada.png` (entregado).
- **Viento sin Envase** — Jefe EX; Cambia el orden de acción; interrumpir la carga. Archivo: `enemigo_viento_sin_envase.png` (entregado).

### Desierto Áureo (leo) — archivos presentes, un fondo provisional

Presentes: 9/9 (4 fondos, 5 enemigos). Dunas y arcos de herradura, oro viejo, arenisca y sombras ciruela. El cielo suspende arena y rebaños solares. El subsuelo actual parece una cisterna abierta: **provisional, rehacer con techo continuo y luz mínima**.

Entregado: `amb_leo_orilla.png`, `amb_leo_subsuelo.png`, `amb_leo_corazon.png`, `amb_leo_cielo.png`, `enemigo_becerro_solar.png`, `enemigo_escarabajo_aureo.png`, `enemigo_chacal_celosa.png`, `enemigo_pastora_ultimo_sol.png`, `enemigo_leon_mediodia_vacio.png`.

[Hoja de revisión](hoja_leo.png). [Diseño detallado](../enemigos/leo_diseno.md).

Enemigos y mecánicas propuestas:

- **Becerro Solar** — Nativo; Se enfurece con Hechizos; usar Cadena y básicos. Archivo: `enemigo_becerro_solar.png` (entregado).
- **Escarabajo Áureo** — Común; Acumula Firmeza al defenderse. Archivo: `enemigo_escarabajo_aureo.png` (entregado).
- **Chacal de Celosía** — Común; Marca a la bruja con más Resonancia. Archivo: `enemigo_chacal_celosa.png` (entregado).
- **Pastora del Último Sol** — Jefa; Protege el rebaño y contraataca al recibir magia. Archivo: `enemigo_pastora_ultimo_sol.png` (entregado).
- **León del Mediodía Vacío** — Jefe EX; Su luz borra mejoras durante la carga; quebrarlo. Archivo: `enemigo_leon_mediodia_vacio.png` (entregado).

### Abadía de las Herbolarias (virgo) — archivos presentes, un fondo provisional

Presentes: 9/9 (4 fondos, 5 enemigos). Hojas densas y claustros, salvia y jade con luz filtrada. El corazón se enreda en injertos y el cielo eleva los huertos. El subsuelo actual enseña jardín y cielo: **provisional, rehacer como galería de raíces completamente enterrada**.

Entregado: `amb_virgo_orilla.png`, `amb_virgo_subsuelo.png`, `amb_virgo_corazon.png`, `amb_virgo_cielo.png`, `enemigo_novicia_sin_alma.png`, `enemigo_tijera_enredadera.png`, `enemigo_corzo_injertado.png`, `enemigo_abadesa_huerto.png`, `enemigo_flor_sin_alma.png`.

[Hoja de revisión](hoja_virgo.png). [Diseño detallado](../enemigos/virgo_diseno.md).

Enemigos y mecánicas propuestas:

- **Novicia sin Alma** — Nativa; Cura a las demás; prioridad de objetivo. Archivo: `enemigo_novicia_sin_alma.png` (entregado).
- **Tijera de Enredadera** — Común; Reduce la curación recibida un turno. Archivo: `enemigo_tijera_enredadera.png` (entregado).
- **Corzo Injertado** — Común; Regenera vida mientras conserve Firmeza. Archivo: `enemigo_corzo_injertado.png` (entregado).
- **Abadesa del Huerto** — Jefa; Traslada curación entre aliados; quebrar sus raíces. Archivo: `enemigo_abadesa_huerto.png` (entregado).
- **Flor sin Alma** — Jefa EX; Absorbe Resonancia para abrir una corola de escudo. Archivo: `enemigo_flor_sin_alma.png` (entregado).

### Pantanos de Azufre (escorpio) — completo

Presentes: 9/9 (4 fondos, 5 enemigos). Sauces torcidos, pilotes y azufre amarillo sobre agua violeta negra. El descenso se cierra en turba y raíces; el corazón es un embarcadero de campanas ante el río de los muertos y el cielo invierte el río.

Entregado: `amb_escorpio_orilla.png`, `amb_escorpio_subsuelo.png`, `amb_escorpio_corazon.png`, `amb_escorpio_cielo.png`, `enemigo_fatua_azufre.png`, `enemigo_sapo_campanero.png`, `enemigo_barquero_juncos.png`, `enemigo_guardiana_vado_hondo.png`, `enemigo_campana_bajo_cielo.png`.

[Hoja de revisión](hoja_escorpio.png). [Diseño detallado](../enemigos/escorpio_diseno.md).

Enemigos y mecánicas propuestas:

- **Fatua del Azufre** — Nativa; Atrae ataques de un objetivo; usar daño en área. Archivo: `enemigo_fatua_azufre.png` (entregado).
- **Sapo Campanero** — Común; Su campana retrasa a la siguiente bruja. Archivo: `enemigo_sapo_campanero.png` (entregado).
- **Barquero de Juncos** — Común; Protege a la Fatua durante un turno. Archivo: `enemigo_barquero_juncos.png` (entregado).
- **Guardiana del Vado Hondo** — Jefa; Alterna provocación y golpe en área. Archivo: `enemigo_guardiana_vado_hondo.png` (entregado).
- **Campana bajo el Cielo** — Jefa EX; Silencia un Hechizo anunciado; romper Firmeza a tiempo. Archivo: `enemigo_campana_bajo_cielo.png` (entregado).

### Estepas del Centauro (sagitario) — parcial

Presentes: 1/9 (1 fondos, 0 enemigos). Solo orilla entregada: estepa cobriza, tiendas de fieltro, piedras de meteorito y guardián colosal distante integrado en el relieve. Faltan subsuelo, corazón, cielo y los cinco enemigos.

Entregado: `amb_sagitario_orilla.png`.

[Hoja de revisión](hoja_sagitario.png). [Diseño detallado](../enemigos/sagitario_diseno.md).

Enemigos y mecánicas propuestas:

- **Cría de Estrella** — Nativa; Explota al caer; decidir el orden de bajas. Archivo: `enemigo_cria_estrella.png` (pendiente).
- **Corcel Meteórico** — Común; Carga si no recibe un golpe antes de su turno. Archivo: `enemigo_corcel_meteorico.png` (pendiente).
- **Arquera de Fieltro** — Común; Apunta a la bruja más rápida. Archivo: `enemigo_arquera_fieltro.png` (pendiente).
- **Gigante del Ojo Único** — Jefe; Carga un golpe muy anunciado; quebrar antes. Archivo: `enemigo_gigante_ojo_unico.png` (pendiente).
- **Centauro del Horizonte Roto** — Jefe EX; Cambia su debilidad según la estrella que porta. Archivo: `enemigo_centauro_horizonte_roto.png` (pendiente).

Falta: `amb_sagitario_subsuelo.png`, `amb_sagitario_corazon.png`, `amb_sagitario_cielo.png`, `enemigo_cria_estrella.png`, `enemigo_corcel_meteorico.png`, `enemigo_arquera_fieltro.png`, `enemigo_gigante_ojo_unico.png`, `enemigo_centauro_horizonte_roto.png`.

### Archipiélago de Hielo (acuario) — pendiente

Presentes: 0/9 (0 fondos, 0 enemigos). Pendiente completo. Dirección prevista: plataformas de hielo, basalto, techos nórdicos, cian perla y ciruela pizarra; canto y océano negro como presión.

[Diseño detallado](../enemigos/acuario_diseno.md).

Enemigos y mecánicas propuestas:

- **Cantora Sumergida** — Nativa; Sube Salinidad de la bruja más limpia; descanso y Autohechizo. Archivo: `enemigo_cantora_sumergida.png` (pendiente).
- **Foca de Escarcha** — Común; Gana escudo tras recibir un Hechizo. Archivo: `enemigo_foca_escarcha.png` (pendiente).
- **Farol de Iceberg** — Común; Marca a quien descanse; golpear para apagarlo. Archivo: `enemigo_farol_iceberg.png` (pendiente).
- **Señora del Agua Quieta** — Jefa; Congela la iniciativa del equipo durante su canto. Archivo: `enemigo_senora_agua_quieta.png` (pendiente).
- **Ballena de Aurora** — Jefa EX; Acumula Salinidad con cada nota; interrumpir su carga. Archivo: `enemigo_ballena_aurora.png` (pendiente).

Falta: `amb_acuario_orilla.png`, `amb_acuario_subsuelo.png`, `amb_acuario_corazon.png`, `amb_acuario_cielo.png`, `enemigo_cantora_sumergida.png`, `enemigo_foca_escarcha.png`, `enemigo_farol_iceberg.png`, `enemigo_senora_agua_quieta.png`, `enemigo_ballena_aurora.png`.

### Isla de la Ninfa (piscis) — pendiente

Presentes: 0/9 (0 fondos, 0 enemigos). Pendiente completo. Dirección prevista: cúpulas y columnas entre hongos, violeta opalino, teal y esporas doradas; hospitalidad demasiado quieta y tiempo ausente.

[Diseño detallado](../enemigos/piscis_diseno.md).

Enemigos y mecánicas propuestas:

- **Ninfa Hospitalaria** — Nativa; Cura a las brujas pero quita Resonancia; terminar pronto. Archivo: `enemigo_ninfa_hospitalaria.png` (pendiente).
- **Pez de Micelio** — Común; Drena una mejora al recibir curación. Archivo: `enemigo_pez_micelio.png` (pendiente).
- **Ánfora de Esporas** — Común; Libera una nube que reduce precisión un turno. Archivo: `enemigo_anfora_esporas.png` (pendiente).
- **Anfitriona sin Despedida** — Jefa; Ofrece curación a cambio de Resonancia; quebrar su invitación. Archivo: `enemigo_anfitriona_sin_despedida.png` (pendiente).
- **Sueño de los Dos Peces** — Jefe EX; Alterna curación y pérdida de Resonancia por fase. Archivo: `enemigo_sueno_dos_peces.png` (pendiente).

Falta: `amb_piscis_orilla.png`, `amb_piscis_subsuelo.png`, `amb_piscis_corazon.png`, `amb_piscis_cielo.png`, `enemigo_ninfa_hospitalaria.png`, `enemigo_pez_micelio.png`, `enemigo_anfora_esporas.png`, `enemigo_anfitriona_sin_despedida.png`, `enemigo_sueno_dos_peces.png`.

## Prioridad 3 — Grieta y Abismo

14 fondos entregados de 32. Grieta conserva el bioma e introduce crecimiento blanco y estrellas diurnas; Abismo baja el cielo, pliega el espacio y dispone ojos como constelaciones. Las figuras de sal son diminutas y distantes, sin heridas ni personajes en primer plano. Las variantes parten de su fondo base como objetivo de edición, no como simple filtro.

### Aries — Montes de Hierro Rojo — 4/8

Entregado: `amb_aries_orilla_grieta.png`, `amb_aries_orilla_abismo.png`, `amb_aries_corazon_grieta.png`, `amb_aries_corazon_abismo.png`.

Falta: `amb_aries_subsuelo_grieta.png`, `amb_aries_subsuelo_abismo.png`, `amb_aries_cielo_grieta.png`, `amb_aries_cielo_abismo.png`.

Hojas: [Grieta](hoja_aries_grieta.png), [Abismo](hoja_aries_abismo.png).

### Cáncer — Costa de las Conchas — 4/8

Entregado: `amb_cancer_orilla_grieta.png`, `amb_cancer_orilla_abismo.png`, `amb_cancer_corazon_grieta.png`, `amb_cancer_corazon_abismo.png`.

Falta: `amb_cancer_subsuelo_grieta.png`, `amb_cancer_subsuelo_abismo.png`, `amb_cancer_cielo_grieta.png`, `amb_cancer_cielo_abismo.png`.

Hojas: [Grieta](hoja_cancer_grieta.png), [Abismo](hoja_cancer_abismo.png).

### Libra — Ducado de las Balanzas — 4/8

Entregado: `amb_libra_orilla_grieta.png`, `amb_libra_orilla_abismo.png`, `amb_libra_corazon_grieta.png`, `amb_libra_corazon_abismo.png`.

Falta: `amb_libra_subsuelo_grieta.png`, `amb_libra_subsuelo_abismo.png`, `amb_libra_cielo_grieta.png`, `amb_libra_cielo_abismo.png`.

Hojas: [Grieta](hoja_libra_grieta.png), [Abismo](hoja_libra_abismo.png).

### Capricornio — Salinas de Obsidiana — 2/8

Entregado: `amb_capricornio_orilla_grieta.png`, `amb_capricornio_orilla_abismo.png`.

Falta: `amb_capricornio_corazon_grieta.png`, `amb_capricornio_corazon_abismo.png`, `amb_capricornio_subsuelo_grieta.png`, `amb_capricornio_subsuelo_abismo.png`, `amb_capricornio_cielo_grieta.png`, `amb_capricornio_cielo_abismo.png`.

Hojas: [Grieta](hoja_capricornio_grieta.png), [Abismo](hoja_capricornio_abismo.png).

## Validación y límites de integración

Los 104 PNG se leyeron completos y pasaron formato, nombres esperados, dimensiones, alfa real y margen de al menos 6 % en enemigos; no hay archivos finales duplicados por SHA-256. Todos tienen fuente y prompt guardados. El margen se mide sobre alfa mayor que 2 para excluir el borde de interpolación casi invisible.

La revisión visual abarcó siluetas, dirección izquierda, tono y composición mediante las imágenes originales y hojas de contacto, incluida miniatura de enemigos a 64 px. No equivale a una prueba dentro de Unity: **no se abrió el juego ni se comprobó el encaje con las placas reales del combate**. Los dos subsuelos provisionales siguen diferenciados en el inventario y requieren revisión antes de aceptarlos como finales.

Solo se escribieron los directorios de arte indicados. No se editaron `.cs`, `.json` ni `.meta`; no se hizo commit ni se usó git.

## Reanudación concreta

1. Corregir los dos subsuelos provisionales con sus prompts `pendientes/*_r1_prompt.md`.
2. Completar Sagitario (8), Acuario (9) y Piscis (9): 26 piezas.
3. Completar las 18 capas pendientes, empezando por corazón de Capricornio.
4. Revisar las nuevas imágenes, exportar, regenerar hojas y ejecutar el control técnico; actualizar este reporte al cerrar cada región.

Los **44 prompts de archivos faltantes** están en `art/ambientes/pendientes/` y `art/enemigos/pendientes/`; hay además dos prompts de corrección de subsuelo. No son entregables generados. [Guía de continuación](retomar.md).
