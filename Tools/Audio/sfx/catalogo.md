# Catálogo de efectos generados

Lo escribe `sfx.py` después de medir cada WAV con ffmpeg (`ebur128=peak=true`). No lo edites a mano: cambia la receta y vuelve a correr `python -I Tools/Audio/sfx/sfx.py generar`.

- **IDs:** 165 · **archivos:** 470 · **tamaño total:** 29.6 MB (tope 30 MB)
- **Formato:** WAV de 48 kHz y 16 bits. Todo es mono salvo donde dice estéreo; FMOD pone el paneo y la cola estéreo.
- **Sonoridad:** máximo momentáneo (ventana de 400 ms, `M máx`) en LUFS; en los bucles, la integrada (`I`). El objetivo de cada familia está en el README.
- **Pico real:** el más alto de las variantes, en dBTP. Ninguno pasa de −1 dBTP.
- **Estado:** nivel de prioridad (A: lo que usa ya el prototipo de la Fase 1; B: el resto) y calidad: *final temporal* sirve para el prototipo; *borrador* funciona pero necesita otra pasada.

## 3.5.1 Combate

| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_combate_entrada` | Barrido de aire que cae en un golpe de tambor de marco, campana de hierro en Re y un brillo de sal | `ui:combate_inicia` (o `BattleStarted`) | 2: `v1`, `v2` | 0.84 | M máx -17.3 a -17.1 | -1.4 | A · final temporal |
| `sfx_combate_turno` | Aliada: tic suave de latón con un brillo agudo. Enemiga: toque grave de madera y plomo | `TurnStarted` (`Side`) | 4: `aliada__v1`, `aliada__v2`, `enemiga__v1`, `enemiga__v2` | 0.26 a 0.32 | M máx -24.0 a -21.0 | -1.5 | A · final temporal |
| `sfx_combate_ataque` | Barrido del arma antes del golpe; cada familia con su materia: madera, papel y agujas, cadena, latón, escoba, bronce, tijera, líquido | `anim:arma_barrido` | 13: `baculo` … `natural` | 0.23 a 0.32 | M máx -21.7 a -21.0 | -1.5 | A · final temporal |
| `sfx_combate_golpe` | Impacto físico por peso (cuerpo grave, golpe de foley y chasquido). Capas aparte: material del objetivo y contacto del arma | `HitLanded` (origen arma o eslabón físico) | 27: `ligero__v1` … `daga_ritual` | 0.13 a 0.39 | M máx -25.4 a -14.4 | -1.5 | A · final temporal |
| `sfx_combate_critico` | Capa del crítico: filo de acero que brilla, grieta de cristal y un destello que sube | `HitLanded` con `IsCrit` | 3: `v1`, `v2`, `v3` | 0.48 | M máx -16.3 | -1.5 | A · final temporal |
| `sfx_combate_dano_recibido` | Reacción del cuerpo de la aliada golpeada: pelaje, plumas, membranas, escamas o lana, con su ropa | `HitLanded` (objetivo aliada) | 8: `canida`, `felina`, `ave`, `alada`, `escamosa`, `ungulada`, `pequena`, `robusta` | 0.23 a 0.36 | M máx -21.7 a -20.0 | -1.3 | A · final temporal |
| `sfx_combate_estado_tick` | Daño por turno: quemadura (soplo de fuego), sangrado (pulso sordo y óxido, sin líquido), veneno (burbujeo ácido), electrocución (crujido de estaño) | `DotTicked` (estado: Burn, Bleed, Poison, Shock) | 4: `quemadura`, `sangrado`, `veneno`, `electrocucion` | 0.26 a 0.45 | M máx -22.2 a -20.0 | -1.5 | A · final temporal |
| `sfx_combate_escudo` | Poner: cúpula de plata que se forma (cristal que sube y se asienta). Romper: cristal que estalla y una nota de plata que cae | `ShieldGained` (poner); `HitLanded` que agota el escudo (romper) | 2: `poner`, `romper` | 0.50 a 0.69 | M máx -18.2 a -18.0 | -1.5 | A · final temporal |
| `sfx_combate_caida` | Una unidad cae: cuerpo contra el suelo, tela, polvo y un poco de sal que se desmorona | `UnitDefeated` | 2: `v1`, `v2` | 0.53 | M máx -17.4 a -17.3 | -1.5 | A · final temporal |
| `sfx_combate_revivir` | Revivir: cuenco de cobre que sube, remolino de agua y una respiración de aire (sin voz) | `Healed` desde 0 o efecto revivir de `ActionUsed` | 1 archivo | 1.13 | M máx -17.1 | -3.6 | A · final temporal |
| `sfx_combate_jefe_aviso` | El jefe carga su ataque definitivo: raíces que se tensan, acero frotado que sube y un pedal grave de plomo. Crece, nunca asusta de golpe | `EnemyCharging` | 1 archivo | 1.58 | M máx -15.2 | -1.5 | A · final temporal |
| `sfx_combate_jefe_fase` | Cambio de fase del jefe: la corteza se abre en dos, un golpe grave y la savia negra que late | `PhaseChanged` | 1 archivo | 1.97 | M máx -15.5 | -1.5 | A · final temporal · **ID nuevo** |
| `sfx_combate_granos` | Sube: un grano de sal cae en el frasco y brilla hacia arriba. Baja: el grano se deshace y la nota cae | `GrainsChanged` (o `ui:granos_cambian`) | 4: `sube__v1`, `sube__v2`, `baja__v1`, `baja__v2` | 0.32 | M máx -24.0 | -6.9 | A · final temporal |
| `sfx_combate_velocidad` | Clic de astrolabio: ×1 un tic grave, ×2 dos tics agudos | `ui:velocidad` | 2: `1`, `2` | 0.04 a 0.10 | M máx -27.0 a -25.9 | -1.5 | A · final temporal |
| `sfx_combate_auto` | Activar: mecanismo de relojería que arranca y engancha. Quitar: clic y engranes que se frenan | `ui:auto` | 2: `activar`, `quitar` | 0.32 a 0.41 | M máx -25.1 a -25.0 | -1.5 | A · final temporal |
| `sfx_combate_victoria` | Victoria: campanas de oro en arpegio de Re mayor y un brillo de sal (suena con el jingle de música) | `BattleEnded` (Victory) | 1 archivo | 0.97 | M máx -15.0 | -2.9 | A · final temporal |
| `sfx_combate_derrota` | Derrota: campana grave de plomo y la sal que se deshace hacia abajo, en Re menor | `BattleEnded` (Defeat) | 1 archivo | 1.31 | M máx -16.0 | -2.1 | A · final temporal |

## 3.5.2 Cadena y Partitura

| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_cadena_resonancia_llena` | La Resonancia llega a 100: copas de cristal que se llenan hasta un acorde abierto de Re y un brillo | `ResonanceChanged` (a 100) o `ui:resonancia_llena` | 1 archivo | 0.96 | M máx -19.1 | -9.8 | A · final temporal |
| `sfx_cadena_abrir` | Se abre la Partitura y el mundo se detiene: aire que se recoge, el libro que se abre, un último tic de reloj y una nota de cristal suspendida | `ui:cadena_abrir` | 1 archivo | 0.90 | M máx -18.2 | -6.1 | A · final temporal |
| `sfx_cadena_cerrar` | Se cierra la Partitura sin ejecutar: el libro se cierra y el aire suelta al mundo | `ui:cadena_cerrar` (cancelar la Cadena) | 1 archivo | 0.51 | M máx -20.1 | -1.5 | A · final temporal · **ID nuevo** |
| `sfx_cadena_tomar` | Tomar la carta de una bruja en la Partitura: pergamino que se levanta y un tic | `ui:partitura_tomar` | 3: `v1`, `v2`, `v3` | 0.12 | M máx -25.3 a -25.2 | -1.5 | A · final temporal |
| `sfx_cadena_soltar` | Soltar la carta en su lugar: pergamino que se asienta y un toque de madera | `ui:partitura_soltar` | 3: `v1`, `v2`, `v3` | 0.16 | M máx -24.0 | -1.5 | A · final temporal |
| `sfx_cadena_spark_previsto` | Aparece un Spark previsto: destello dorado pequeño. Desaparece: el destello se apaga y baja | `ui:preview_cambia` (resultado de `PreviewChain`) | 2: `aparece`, `desaparece` | 0.39 a 0.46 | M máx -23.0 | -10.0 | A · final temporal |
| `sfx_cadena_confirmar` | Confirmar la Partitura: trazo de pluma con rúbrica, el punto final y una campana grave suave | `ui:cadena_confirmar` | 1 archivo | 0.66 | M máx -20.0 | -1.5 | A · final temporal |
| `sfx_cadena_calculo` | El sonido del cálculo: engranes de astrolabio y cuentas de ábaco; la Calculista calcula en silencio | `ChainStarted` | 1 archivo | 0.84 | M máx -20.0 | -1.5 | A · final temporal |
| `sfx_cadena_eslabon` | Entrada de cada eslabón, sin voz: aire que se corta. Apertura breve y aguda, Enlace doble, Cierre con peso | `ChainLinkStarted` (`Role`) | 6: `apertura__v1`, `apertura__v2`, `enlace__v1`, `enlace__v2`, `cierre__v1`, `cierre__v2` | 0.22 a 0.59 | M máx -20.0 a -18.0 | -2.1 | A · final temporal |
| `sfx_partitura_nota` | Una nota por golpe en la escala de Re menor, con el metal del planeta: Luna Re, Mercurio Mi, Venus Fa, Sol Sol, Marte La, Júpiter Si♭, Saturno Do. Apertura grave, Enlace media, Cierre aguda con acorde | `HitLanded` (`EnCadena = 1`, sin Spark); `ui:partitura_escuchar` | 21: `sol__apertura` … `saturno__cierre` | 0.23 a 0.30 | M máx -22.3 a -22.0 | -1.5 | A · final temporal |
| `sfx_cadena_spark` | La campana del Spark: aguda, corta y dorada. Sube por el acorde de Re menor con cada Spark de la misma Cadena: Re6, Fa6, La6, Re7 y Fa7 | `HitLanded` (`IsSpark`), una por grupo | 5: `1`, `2`, `3`, `4`, `5` | 0.60 | M máx -15.3 a -15.0 | -1.5 | A · final temporal |
| `sfx_cadena_invitada` | La Invitada cierra la Cadena: díada de plata y oro con un barrido amplio | `ChainLinkStarted` (Cierre de la Invitada) | 1 archivo | 0.80 | M máx -18.0 | -4.2 | A · final temporal |
| `sfx_cadena_fin` | Acorde de resolución de la Cadena, más brillante con más Spark: sin Spark, Re menor en cuencos; 1 o 2, Re menor con campanas de oro; 3 o más, Re mayor (tercera de Picardía) con cascada dorada | `ChainEnded` (`Sparks`: 0, 1–2, 3+) | 3: `0`, `1`, `3` | 1.00 | M máx -17.0 | -3.5 | A · final temporal |

## 3.5.3 Magia y ritual

| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_ritual_marisal` | Muerde un cristal de sal de roca: mordida seca, sin saliva, y la halita que se parte en dos | `ActionUsed` (Hechizo), `RitualLargo` | 3: `largo__v1`, `largo__v2`, `corto` | 0.18 a 0.34 | M máx -19.7 a -19.5 | -1.5 | A · final temporal |
| `sfx_ritual_selenia` | Disuelve sal en té negro: cucharita contra porcelana, granos que caen al té y un trago corto | `ActionUsed` (Hechizo), `RitualLargo` | 3: `largo__v1`, `largo__v2`, `corto` | 0.40 a 0.53 | M máx -19.0 | -4.8 | A · final temporal |
| `sfx_ritual_solenne` | Sal gruesa en un gajo de limón con chile: granos sobre la pulpa, chile como roce de papel y una mordida jugosa de 150 ms | `ActionUsed` (Hechizo), `RitualLargo` | 5: `largo__v1`, `largo__v2`, `corto`, `recatado__largo`, `recatado__corto` | 0.17 a 0.33 | M máx -22.0 a -19.1 | -1.5 | A · final temporal |
| `sfx_ritual_cervanda` | Salero de plata con modales de corte: dos sacudidas exactas y la cuchara de plata | `ActionUsed` (Hechizo), `RitualLargo` | 3: `largo__v1`, `largo__v2`, `corto` | 0.17 a 0.39 | M máx -19.1 a -19.0 | -1.5 | A · final temporal |
| `sfx_ritual_tormentina` | Lame sal del dorso de la pata: sal fina sobre gamuza y un lengüetazo corto, seco y cómico | `ActionUsed` (Hechizo), `RitualLargo` | 5: `largo__v1`, `largo__v2`, `corto`, `recatado__largo`, `recatado__corto` | 0.18 a 0.35 | M máx -19.5 a -19.1 | -1.5 | A · final temporal |
| `sfx_ritual_cupria` | Su Ascendente le pone la sal en la lengua: sal que cae de dedos astrales, con reverberación (sin boca) | `ActionUsed` (Hechizo), `RitualLargo` | 3: `largo__v1`, `largo__v2`, `corto` | 0.55 | M máx -19.0 | -1.5 | A · final temporal |
| `sfx_ritual_muertita` | Esconde la sal bajo la lengua y se desmaya: una pizca de granos y una caída blanda y cómica | `ActionUsed` (Hechizo), `RitualLargo` | 3: `largo__v1`, `largo__v2`, `corto` | 0.19 a 0.33 | M máx -19.3 a -19.2 | -1.5 | A · final temporal |
| `sfx_ritual_topacia` | Saca la sal de la tierra con las garras y la masca: dos rasguños en la tierra y dos crujidos | `ActionUsed` (Hechizo), `RitualLargo` | 3: `largo__v1`, `largo__v2`, `corto` | 0.22 a 0.38 | M máx -20.1 a -19.6 | -1.5 | A · final temporal |
| `sfx_sal_crujido` | La firma del juego: fractura de halita y granos (120 a 250 ms) con la cola del metal del planeta a −8 dB (hierro, plata, oro, plomo, estaño, cobre o azogue). Pureza 0.85 (grado B) | Anidado en `sfx_ritual_*` (al terminar el gesto) y en el Altar | 14: `sol__v1` … `saturno__v2` | 0.26 a 0.45 | M máx -19.4 a -18.1 | -1.5 | A · final temporal |
| `sfx_magia_canalizar` | La bruja canaliza y el Ascendente se prepara: el metal del planeta crece en 0.6 s (cristal frotado, gotas, cuenco frotado, fuego, acero frotado, estática, pedal de plomo) | `ActionUsed` (Hechizo), después del ritual | 7: `sol`, `luna`, `marte`, `mercurio`, `venus`, `jupiter`, `saturno` | 0.62 | M máx -20.4 a -20.0 | -2.0 | A · final temporal |
| `sfx_magia_ascendente_aparece` | El Ascendente aparece con la firma de su signo: cuernos de hierro (Aries), piedra y cobre (Tauro), eco en espejo (Géminis), caparazón (Cáncer), corona de fuego (Leo), espigas (Virgo), platos de balanza (Libra), aguijón de obsidiana (Escorpio), cuerda de arco (Sagitario), agua y pezuña (Capricornio), cántaro (Acuario), dos peces de luz (Piscis) | `anim:ascendente_invoca` | 12: `aries` … `piscis` | 0.42 a 0.49 | M máx -19.4 a -19.0 | -1.5 | A · final temporal |
| `sfx_magia_golpe_astral` | Impacto del Golpe Astral con la escalera de pureza: Turbia es ruido y grano, Destilada es tono con textura, Quintaesencia es tono puro con aura (tabla 3.3) | `HitLanded` (origen Hechizo) | 21: `sol__turbia` … `saturno__quintaesencia` | 0.34 a 0.45 | M máx -18.6 a -16.0 | -1.5 | A · final temporal |
| `sfx_magia_autohechizo` | Aguja y Sello: agujas de cristal, un siseo breve y el sello que se estampa (sin gore). Hilos: cuerdas que se tensan y suben. Frenesí: Sal Negra que cruje y el gruñido grave del Ascendente (cuenco frotado, no voz) | `ActionUsed` (Autohechizo, `SelfCastForm`) | 3: `aguja_sello`, `hilos`, `frenesi` | 0.63 a 0.83 | M máx -18.0 | -2.3 | A · final temporal |
| `sfx_magia_vislumbre` | Vislumbre: 2 s de fusión con la forma del grado siguiente. Sube un barrido, el metal se transforma y cae un acorde: Turbia con grano, Destilada con tono y textura, Quintaesencia pura y con aura | `ActionUsed` (Vislumbre); el valor es la fase del grado siguiente | 3: `turbia`, `destilada`, `quintaesencia` | 1.53 | M máx -17.2 a -17.1 | -3.7 | A · final temporal |
| `sfx_magia_sal_negra` | Capa de Sal Negra: crujido húmedo y grave, y un cuenco frotado hasta que gime | `ActionUsed` (gasta Sal Negra) | 1 archivo | 0.99 | M máx -18.0 | -2.9 | A · final temporal |
| `sfx_magia_ascendente_retira` | El Ascendente se retira: brillo que se recoge hacia atrás y un barrido que baja | `anim:ascendente_retira` | 1 archivo | 0.49 | M máx -22.0 | -6.2 | A · final temporal |

## 3.5.4 Salinidad por banda

| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_salinidad_baja` | Baja de banda: alivio, la sal se disuelve en burbujas finas y una nota de cristal que desciende | `SalinityBandChanged` (banda nueva menor) | 1 archivo | 0.69 | M máx -20.2 | -7.7 | A · final temporal |
| `sfx_salinidad_salobre` | Entra a Salobre (40 a 69): costra fina que cruje y un pulso suave | `SalinityBandChanged` (Salobre) | 1 archivo | 0.57 | M máx -20.0 | -1.6 | A · final temporal |
| `sfx_salinidad_sobresalada` | Entra a Sobresalada (70 a 99): costra más densa y una segunda menor tensa en plata | `SalinityBandChanged` (Sobresalada) | 1 archivo | 0.86 | M máx -19.2 | -7.2 | A · final temporal |
| `sfx_salinidad_cristalizacion` | Entra a Cristalización Turbia (100 a 149): crujidos lentos de sal (una y dos octavas abajo), cristal que se tensa y un pedal grave | `SalinityBandChanged` (CristalizacionTurbia) | 1 archivo | 1.32 | M máx -18.0 | -1.5 | A · final temporal |
| `sfx_salinidad_cristal_bucle` | Bucle de cristales que crecen mientras la bruja siga en Cristalización Turbia | Mismo evento; se detiene al bajar de banda | 1 archivo | 2.00 | I -31.0 | -7.7 | A · final temporal · bucle |
| `sfx_salinidad_perdida_vida` | Pierde 5 % de vida por turno en Cristalización: grieta de cristal y un pulso sordo | `TurnStarted` (bruja en Cristalización Turbia) | 1 archivo | 0.43 | M máx -20.0 | -2.9 | A · final temporal |
| `sfx_salinidad_extasis` | Éxtasis Abisal: el Ascendente toma el control. Crece un pedal grave, el cuenco gime, susurros sin palabras al revés y la sal estalla; sube, no salta | `SalinityBandChanged` (ExtasisAbisal) o `EcstasyStarted` | 1 archivo | 2.36 | M máx -14.0 | -2.5 | A · final temporal |
| `sfx_salinidad_desalada` | Sale del Éxtasis y queda Desalada: barrido largo que baja, la sal se disuelve y una campana suave | `SalinityBandChanged` (de ExtasisAbisal a 60) o `EcstasyEnded` | 1 archivo | 1.07 | M máx -19.1 | -6.0 | A · final temporal |

## 3.5.5 Quiebre por planeta

| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_quiebre_firmeza` | La Firmeza llega a cero: una placa de sal que se rompe con una grieta dorada (varias fracturas, cristal que estalla y un brillo de oro que sube) | `BreakTriggered` (siempre primero) | 3: `v1`, `v2`, `v3` | 0.68 | M máx -14.3 a -14.2 | -1.5 | A · final temporal |
| `sfx_quiebre_sol` | Quemadura: llamarada que sube y un siseo con chasquidos | `BreakTriggered` (Sol) | 1 archivo | 0.78 | M máx -17.4 | -1.5 | A · final temporal |
| `sfx_quiebre_luna` | Encierro: jaula de plata que se cierra (cerrojo y barrotes) con crótalos | `BreakTriggered` (Luna) | 1 archivo | 0.73 | M máx -17.0 | -2.8 | A · final temporal |
| `sfx_quiebre_marte` | Sangrado fuerte: cadena de hierro que se tensa y óxido raspado, sin chapoteo | `BreakTriggered` (Marte) | 1 archivo | 0.66 | M máx -17.0 | -2.8 | A · final temporal |
| `sfx_quiebre_mercurio` | Veneno y retraso: azogue que burbujea, pesado y metálico, y gotas que resbalan | `BreakTriggered` (Mercurio) | 1 archivo | 0.78 | M máx -17.2 | -5.1 | A · final temporal |
| `sfx_quiebre_venus` | Encanto: acorde suave de cuencos de cobre (Fa, La, Do) | `BreakTriggered` (Venus) | 1 archivo | 0.99 | M máx -17.0 | -7.8 | A · final temporal |
| `sfx_quiebre_jupiter` | Electrocución que salta: grito del estaño, arcos que crepitan y una lámina de trueno. Los saltos son las chispas que van a cada enemigo cercano | `BreakTriggered` (Júpiter); saltos con cada `HitLanded` del salto | 4: `quiebre`, `salto__v1`, `salto__v2`, `salto__v3` | 0.30 a 0.76 | M máx -20.4 a -17.3 | -1.5 | A · final temporal |
| `sfx_quiebre_saturno` | Gravedad: plomo que cae con todo su peso y un reloj que se frena | `BreakTriggered` (Saturno) | 1 archivo | 1.00 | M máx -17.2 | -1.5 | A · final temporal |

## 3.5.6 Aspectos y Reacciones Híbridas

| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_aspecto_conjuncion` | Conjunción: unísono reforzado. Dos Re que laten y se funden, con su octava | `AspectFormed` (Conjuncion) | 1 archivo | 0.81 | M máx -18.0 | -6.3 | A · final temporal |
| `sfx_aspecto_oposicion` | Oposición: una octava (Re4 y Re5) que se quiebra antes de la Reacción: la nota de arriba se desafina y cruje | `AspectFormed` (Oposicion) | 1 archivo | 0.81 | M máx -18.0 | -6.3 | A · final temporal |
| `sfx_aspecto_sextil` | Sextil: tercera mayor rápida que adelanta (Fa a La, o Si♭ a Re) | `AspectFormed` (Sextil) | 1 archivo | 0.80 | M máx -18.0 | -6.2 | A · final temporal |
| `sfx_aspecto_trigono` | Trígono: quinta justa cálida (Re y La) y un grano de sal | `AspectFormed` (Trigono) | 1 archivo | 0.79 | M máx -18.0 | -5.2 | A · final temporal |
| `sfx_aspecto_cuadratura` | Cuadratura: segunda menor áspera (La y Si♭, o Mi y Fa) en hierro, con roce | `AspectFormed` (Cuadratura) | 1 archivo | 0.79 | M máx -18.0 | -3.0 | A · final temporal |
| `sfx_reaccion_eclipse` | Eclipse (Sol con Saturno): un acorde de oro brillante que el plomo se traga, y 200 ms de silencio antes de un último tic grave | `ReactionTriggered` (Eclipse) o `AspectFormed` con reacción | 1 archivo | 0.79 | M máx -15.0 | -1.7 | A · final temporal |
| `sfx_reaccion_helada_negra` | Helada Negra (Luna con Saturno): escarcha en cámara lenta, crujidos de hielo estirados, plata que baja y plomo grave | `ReactionTriggered` (HeladaNegra) | 1 archivo | 1.46 | M máx -15.0 | -3.1 | A · final temporal |
| `sfx_reaccion_espinas_rojas` | Espinas Rojas (Marte con Venus): espinas de hierro que brotan y un cuenco de cobre que cura | `ReactionTriggered` (EspinasRojas) | 1 archivo | 1.10 | M máx -15.1 | -3.2 | A · final temporal |
| `sfx_reaccion_tempestad` | Tempestad (Mercurio con Júpiter): rayo en cadena hecho de estaño y azogue: grito del estaño, crepitar, lámina de trueno y gotas rápidas. Cada golpe extra usa su propio `HitLanded` | `ReactionTriggered` (Tempestad) | 1 archivo | 1.09 | M máx -15.2 | -1.5 | A · final temporal |

## 3.5.7 Ascensiones

| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_ascension_carga` | El anillo de Ánima llega a 100, 200 o 300: una, dos o tres notas de oro que se apilan (Re, Fa, La) con un brillo | `AnimaChanged` (cruza 100, 200 o 300) o `ui:anima_umbral` | 3: `1`, `2`, `3` | 0.57 | M máx -21.0 | -6.0 | A · final temporal |
| `sfx_ascension_lanzar` | El jugador lanza la Ascensión y el tiempo se congela: aire que se recoge, un último tic y un golpe de cristal; más grave y más ancho por nivel | `AscensionCast` (`Level`) | 3: `1`, `2`, `3` | 0.94 | M máx -17.0 a -15.5 | -1.5 | A · final temporal |
| `sfx_ascension_i` | Ascensión I (1 a 1.5 s): el metal del planeta se carga y suelta la técnica del Ascendente en un golpe de Quintaesencia | `AscensionCast` (`Level` 1) | 7: `sol`, `luna`, `marte`, `mercurio`, `venus`, `jupiter`, `saturno` | 0.83 a 0.93 | M máx -15.3 a -15.0 | -1.5 | A · final temporal |
| `sfx_ascension_ii` | Ascensión II (2 s, compacta por el tope de tamaño): carga larga, dos golpes del planeta y un acorde de su metal que sube para todo el equipo | `AscensionCast` (`Level` 2) | 7: `sol`, `luna`, `marte`, `mercurio`, `venus`, `jupiter`, `saturno` | 1.52 | M máx -14.4 a -14.1 | -1.5 | A · final temporal |
| `sfx_ascension_iii_marisal` | Ascensión III de Marisal, el Carnero de Acero Carmesí (capa de efectos bajo `mus_ascension_iii_marisal`): cadenas que se tensan, la halita que se parte, la embestida con yunques en Do frigio a 120 BPM y el impacto de hierro y sal | `AscensionCast` (`Level` 3) → `tl:` pista de FMOD en la Timeline | 2: `largo`, `corto` | 2.02 a 6.16 | M máx -13.4 a -12.5 | -1.5 | A · final temporal |
| `sfx_ascension_impacto` | Impacto de la Ascensión tras la pausa de 3 frames: golpe enorme con el metal del planeta y la sal que estalla | `tl:impacto` | 7: `sol`, `luna`, `marte`, `mercurio`, `venus`, `jupiter`, `saturno` | 0.73 a 0.79 | M máx -14.1 a -13.1 | -1.5 | A · final temporal |
| `sfx_ascension_duo` | Dos Ascensiones encimadas con Spark: dos campanas doradas (Re y La) y un doble golpe | `HitLanded` (`IsSpark`, origen Ascensión) | 1 archivo | 0.91 | M máx -13.3 | -1.5 | A · final temporal |
| `sfx_ascension_saltar` | Saltar la cinemática: corte limpio al impacto, un barrido breve hacia atrás | `ui:saltar` | 1 archivo | 0.35 | M máx -20.0 | -4.5 | A · final temporal |

## 3.5.8 Pociones

| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_pocion_cinturon` | Tocar una ranura del cinturón: cuero y el vidrio del frasco que tintinea | `ui:cinturon` | 2: `v1`, `v2` | 0.19 | M máx -25.0 | -3.2 | A · final temporal |
| `sfx_pocion_destapar` | Corcho que sale y oleaje según la base: agua termal (limpia y burbujeante), vinagre (efervescente y ácido) o leche lunar (espesa, con un brillo de plata) | `ActionUsed` (Poción) | 3: `agua_termal`, `vinagre`, `leche_lunar` | 0.57 a 0.71 | M máx -21.7 a -21.0 | -1.5 | A · final temporal |
| `sfx_pocion_beber` | Un trago corto: dos burbujas graves del líquido en el frasco, sin boca ni respiración | `ActionUsed` (Poción) | 2: `v1`, `v2` | 0.38 a 0.48 | M máx -22.0 | -8.2 | A · final temporal |
| `sfx_pocion_efecto` | Brillo según el efecto: cura (cuencos de cobre), escudo (cúpula de plata), velocidad (gotas de azogue que suben), retraso (reloj de plomo que se frena), Ánima (campanas que suben), Salinidad (sal que se disuelve) | `ActionUsed` (efecto de la poción) | 6: `cura`, `escudo`, `velocidad`, `retraso`, `anima`, `salinidad` | 0.53 a 0.65 | M máx -20.1 a -19.0 | -1.5 | A · final temporal |
| `sfx_pocion_turbia` | Salmuera Turbia con efecto aleatorio: burbujeo raro, burbujas que suben y bajan de tono y un 'plop' final | `ActionUsed` (Salmuera Turbia) | 1 archivo | 0.75 | M máx -20.0 | -5.9 | A · final temporal |

## 3.5.9 Fresno

| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_fresno_zona_entrar` | Entrar a una zona del Fresno: Raíces (madera húmeda y cuenco grave en Re), Tronco (ceniza que cruje, Mi), Ramas (viento entre hojas, Sol), Copa (aire alto y cristal, La) | `f2:zona_entrar` | 4: `raices`, `tronco`, `ramas`, `copa` | 0.86 a 1.01 | M máx -18.0 | -4.0 | B · final temporal |
| `sfx_fresno_nodo_elegir` | Elegir un nodo del mapa: toque de madera y una hoja | `ui:nodo_elegir` | 2: `v1`, `v2` | 0.14 | M máx -24.4 | -1.5 | B · final temporal |
| `sfx_fresno_ruta` | La ruta se dibuja en el mapa: trazo largo de pluma sobre pergamino | `ui:ruta_trazo` | 1 archivo | 0.34 | M máx -24.3 | -1.5 | B · final temporal |
| `sfx_fresno_injerto` | Elegir un Injerto: madera que acepta la rama y la nota del metal de su planeta | `f2:injerto_elegido` (planeta de la Rama) | 7: `sol`, `luna`, `marte`, `mercurio`, `venus`, `jupiter`, `saturno` | 0.30 a 0.48 | M máx -20.3 a -20.0 | -1.5 | B · final temporal |
| `sfx_fresno_fruto` | Se activa un Fruto (tres Injertos de una Rama): arpegio de tres notas del metal del planeta y hojas que se abren | `f2:fruto_activado` | 7: `sol`, `luna`, `marte`, `mercurio`, `venus`, `jupiter`, `saturno` | 0.52 a 0.59 | M máx -17.2 a -17.0 | -1.5 | B · final temporal |
| `sfx_fresno_mutacion` | Aceptar una Mutación del Árbol: corteza que se retuerce y un cuenco desafinado | `f2:mutacion` | 1 archivo | 0.76 | M máx -19.0 | -3.7 | B · final temporal |
| `sfx_fresno_maldicion` | Aceptar una maldición: campana de plomo y bronce, más grave con cada una (Re4, Do4, Si♭3, La3, Sol3) | `f2:maldicion_aceptada` (número de maldiciones, 1 a 5) | 5: `1`, `2`, `3`, `4`, `5` | 0.52 | M máx -19.0 | -2.9 | B · final temporal |
| `sfx_fresno_santuario` | Guardar almitas en un Santuario: cuenco cálido y un racimo suave de campanitas | `f2:almitas_guardadas` | 1 archivo | 0.87 | M máx -18.0 | -6.6 | B · final temporal |
| `sfx_fresno_descanso` | Elegir descanso en el Santuario: cuenco suave y un soplo de viento | `f2:santuario_eleccion` (descanso) | 1 archivo | 0.84 | M máx -20.4 | -9.2 | B · final temporal |
| `sfx_fresno_entrenar` | Elegir entrenamiento en el Santuario: dos golpes de báculo y una campana de hierro | `f2:santuario_eleccion` (entrenar) | 1 archivo | 0.63 | M máx -20.3 | -1.5 | B · final temporal |
| `sfx_fresno_mancha_aparece` | La Mancha de Sal aparece en su nodo: sal oscura que se extiende con un crepitar grave y un pedal que sube | `f2:mancha_en_nodo` | 1 archivo | 1.20 | M máx -20.0 | -4.6 | B · final temporal |
| `sfx_fresno_mancha_recuperar` | Recuperar las almitas perdidas: cascada de campanitas que sube y un brillo de sal | `f2:mancha_recuperada` | 1 archivo | 0.78 | M máx -17.0 | -7.5 | B · final temporal |
| `sfx_fresno_mancha_absorbida` | El Fresno absorbe la Mancha y devuelve 30 % como Savia: madera que gime, savia que baja y una campanita | `f2:mancha_absorbida` | 1 archivo | 0.95 | M máx -19.1 | -3.4 | B · final temporal |
| `sfx_fresno_eco` | Guardar un Eco: nota de cristal que se repite en ecos que se apagan | `f2:eco_guardado` | 1 archivo | 0.64 | M máx -21.0 | -5.8 | B · final temporal |
| `sfx_fresno_ofrenda` | Ofrenda: el árbol crece, madera que cruje hacia arriba, hojas y un acorde que se abre | `f2:ofrenda` | 1 archivo | 0.98 | M máx -19.0 | -6.9 | B · final temporal |
| `sfx_ambiente_raices` | Lecho de las Raíces: humedad grave, gotas y madera que cruje lejos | Carga de escena | 1 archivo | 3.40 | I -30.3 | -7.9 | B · borrador · bucle |
| `sfx_ambiente_tronco` | Lecho del Tronco: viento hueco y ceniza que crepita | Carga de escena | 1 archivo | 3.40 | I -30.2 | -16.0 | B · borrador · bucle |
| `sfx_ambiente_ramas` | Lecho de las Ramas: viento entre hojas | Carga de escena | 1 archivo | 3.40 | I -30.2 | -16.9 | B · borrador · bucle |
| `sfx_ambiente_copa` | Lecho de la Copa: aire alto y un cristal que respira | Carga de escena | 1 archivo | 3.40 | I -30.2 | -18.5 | B · borrador · bucle |

## 3.5.10 Termas y tirada del gacha

| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_ambiente_termas_dia` | Lecho de las Termas de día: agua que lame la piedra, gotas, vapor y una brisa | Carga de escena (hora local de 6:00 a 18:59) | 1 archivo | 3.40 | I -30.2 | -14.5 | B · borrador · bucle |
| `sfx_ambiente_termas_noche` | Lecho de las Termas de noche: agua más quieta, gotas espaciadas y grillos lejanos | Carga de escena (hora local de 19:00 a 5:59) | 1 archivo | 3.40 | I -31.2 | -15.7 | B · borrador · bucle |
| `sfx_termas_tortuga_respira` | La tortuga gigante respira: inhalación y exhalación lentas y graves, de aire, sin voz | Temporizador de escena (cada 20 a 40 s) | 1 archivo | 1.68 | M máx -26.2 | -14.1 | B · final temporal |
| `sfx_termas_tocar` | Tocar a una bruja (cabeza, mano, sombrero, cola): una palmadita suave en pelaje, plumas, membranas, escamas o lana | `ui:termas_tocar` (`Especie`) | 8: `canida`, `felina`, `ave`, `alada`, `escamosa`, `ungulada`, `pequena`, `robusta` | 0.23 a 0.30 | M máx -22.0 | -2.3 | B · final temporal |
| `sfx_termas_bano` | Entrar y salir del agua termal: chapoteo suave, burbujas y agua que escurre | `ui:bano` | 2: `entrar`, `salir` | 0.88 | M máx -21.4 a -21.0 | -3.8 | B · final temporal |
| `sfx_termas_paralaje_baja` | El baño baja el Paralaje: los susurros se apagan bajo el agua y queda una nota limpia de cristal | `meta:paralaje_baja` en las Termas | 1 archivo | 1.78 | M máx -21.0 | -8.5 | B · final temporal |
| `sfx_tirada_tomar` | La Calculista toma Sales de Baño: un puñado chico (1) o grande (10) que cruje en la mano | `ui:tirada` | 2: `1`, `10` | 0.30 a 0.60 | M máx -22.1 a -22.0 | -1.5 | B · final temporal |
| `sfx_tirada_sales_caer` | Las sales caen al manantial: un 'plop' y granos (1), o una cascada de plops (10) | `tl:sales_caen` | 3: `1__v1`, `1__v2`, `10` | 0.23 a 0.87 | M máx -20.3 a -20.0 | -1.5 | B · final temporal |
| `sfx_tirada_burbujas` | Las sales se disuelven: efervescencia fina que sube | `tl:disolver` | 1 archivo | 0.96 | M máx -22.1 | -8.7 | B · final temporal |
| `sfx_tirada_vapor` | Vapor por rareza (suena con el color, nunca antes): blanco es un siseo suave; violeta suma brillos de cristal en Si♭; dorado es una columna más grande con destellos de oro en Re mayor | `tl:vapor` (`Rareza`) | 3: `3`, `4`, `5` | 1.01 | M máx -20.0 a -18.0 | -6.6 | B · final temporal |
| `sfx_tirada_subida` | El vapor pasa de violeta a dorado: un barrido que sube y los brillos cambian de cristal a oro | `tl:vapor_sube` | 1 archivo | 1.36 | M máx -17.1 | -6.9 | B · final temporal |
| `sfx_tirada_quietud` | El agua se aquieta: el rumor se apaga hasta el silencio y queda un solo brillo | `tl:agua_quieta` | 1 archivo | 1.29 | M máx -25.0 | -11.0 | B · final temporal |
| `sfx_tirada_constelacion` | 5★: la constelación de su Ascendente se dibuja; cada estrella toca una nota (firma provisional en Re mayor pentatónico) | `tl:constelacion` | 1 archivo | 1.69 | M máx -19.2 | -8.5 | B · borrador |
| `sfx_tirada_silueta` | Una silueta bajo el agua: oleaje sordo que sube y burbujas apagadas | `tl:silueta` | 1 archivo | 1.12 | M máx -22.1 | -7.7 | B · final temporal |
| `sfx_tirada_emergencia` | La bruja emerge del manantial: el agua sube, se abre y escurre de su pelaje, plumas, alas o escamas | `tl:emerge` (`Especie`, `Rareza`) | 8: `canida`, `felina`, `ave`, `alada`, `escamosa`, `ungulada`, `pequena`, `robusta` | 0.72 a 0.93 | M máx -18.4 a -18.0 | -1.4 | B · final temporal |
| `sfx_tirada_sacudida` | Se sacude según su especie: las cánidas enteras (ráfagas de agua y orejas), las felinas breve y molesta, las aves esponjan las plumas, las aladas aletean, las escamosas escurren, las unguladas sacuden la cabeza, las pequeñas rápido y agudo, las robustas lento y pesado | `tl:sacudida` (`Especie`) | 8: `canida`, `felina`, `ave`, `alada`, `escamosa`, `ungulada`, `pequena`, `robusta` | 0.28 a 0.56 | M máx -19.6 a -19.0 | -1.6 | B · final temporal |
| `sfx_tirada_tarjeta` | Aparece su tarjeta: pergamino que se desliza y una campanita según la rareza (plata, díada violeta en Si♭ y Re, tríada de oro en Re mayor) | `tl:tarjeta` (`Rareza`) | 3: `3`, `4`, `5` | 0.48 | M máx -20.0 | -5.5 | B · final temporal |
| `sfx_tirada_saltar` | Saltar la animación: un barrido de pergamino breve | `ui:saltar` (tirada) | 1 archivo | 0.25 | M máx -24.0 | -6.7 | B · final temporal |
| `sfx_tirada_termometro` | El agua se calienta con el pity (bucle): hervor suave; FMOD sube la intensidad con el parámetro `Pity` | `ui:termometro` (pity 0 a 70) | 1 archivo | 1.40 | I -30.4 | -18.0 | B · borrador · bucle |
| `sfx_tirada_polvo_astral` | El resultado se vuelve Polvo Astral: un brillo que se dispersa hacia arriba | `tl:polvo_astral` | 1 archivo | 0.68 | M máx -21.0 | -11.6 | B · final temporal |
| `mus_tirada_vapor_blanco` | Stinger 3★ en Re menor: rasgueo suave de guitarra de nailon y una campanita, con la cola de la capilla | `tl:vapor` (3★), después del vapor | 1 archivo | 1.90 | M máx -17.0 | -4.7 | B · final temporal |
| `mus_tirada_vapor_violeta` | Stinger 4★: arpegio de salterio que sube y se posa en Si♭ mayor, luego Fa mayor, con campanitas y cuencos que crecen | `tl:vapor` (4★) | 1 archivo | 3.19 | M máx -17.2 | -4.0 | B · final temporal |
| `mus_tirada_vapor_dorado` | Stinger 5★: florece un acorde radiante de Re mayor: raíz de violonchelo, trémolo de salterio, cascada de campanas agudas y cuencos que crecen | `tl:vapor` (5★) | 1 archivo | 3.73 | M máx -16.1 | -4.2 | B · final temporal |
| `mus_tirada_subida_dorada` | Stinger de la subida sorpresa: empieza como el arpegio suave de Si♭ mayor, se levanta y modula a un Re mayor brillante con campanas | `tl:vapor_sube` (en lugar del violeta) | 1 archivo | 5.31 | M máx -16.1 | -4.9 | B · final temporal |

## 3.5.11 Libro Mayor e interfaz

| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_ui_tocar` | Toque genérico: un golpecito de pergamino y la punta de la pluma | `ui:tocar` | 4: `v1`, `v2`, `v3`, `v4` | 0.08 | M máx -26.9 a -25.4 | -1.5 | A · final temporal |
| `sfx_ui_confirmar` | Confirmar: rúbrica corta de pluma, un toque de madera y una campanita de Re | `ui:confirmar` | 3: `v1`, `v2`, `v3` | 0.36 | M máx -22.0 | -2.6 | A · final temporal |
| `sfx_ui_cancelar` | Volver: hoja que regresa y un toque más grave | `ui:cancelar` | 3: `v1`, `v2`, `v3` | 0.15 | M máx -23.6 a -23.4 | -1.5 | A · final temporal |
| `sfx_ui_error` | Acción imposible: tic sordo de plomo, doble y apagado | `ui:error` | 2: `v1`, `v2` | 0.18 | M máx -23.0 | -1.7 | A · final temporal |
| `sfx_ui_pestana` | Cambiar de pestaña: vuelta de hoja del Libro Mayor | `ui:pestana` | 3: `v1`, `v2`, `v3` | 0.19 a 0.41 | M máx -24.3 a -24.0 | -1.5 | B · final temporal |
| `sfx_ui_deslizar` | Deslizar listas: roce breve de papel (máximo uno cada 80 ms) | `ui:deslizar` | 3: `v1`, `v2`, `v3` | 0.13 | M máx -28.0 | -8.7 | B · final temporal |
| `sfx_ui_astrolabio` | Menú radial: engranes de latón que avanzan una posición | `ui:astrolabio_paso` | 3: `v1`, `v2`, `v3` | 0.07 | M máx -26.0 | -3.1 | B · final temporal |
| `sfx_ui_notificacion` | Aviso: dos notas de campanita (La y Re) | `ui:notificacion` | 2: `v1`, `v2` | 0.53 | M máx -21.0 | -8.4 | B · final temporal |
| `sfx_ui_recompensa` | Recompensa recibida: arpegio de campanas de oro en Re mayor y un roce de pergamino | `ui:recompensa` | 2: `v1`, `v2` | 0.70 | M máx -19.0 | -7.1 | B · final temporal |
| `sfx_ui_compra` | Compra confirmada, neutral: el sello sobre el pergamino y una nota tranquila; sin monedas | `ui:compra_ok` | 1 archivo | 0.57 | M máx -21.0 | -7.7 | B · final temporal |
| `sfx_libro_abrir` | Abrir el Libro Mayor: cuero que se abre y un brillo de pan de oro | `ui:libro_abrir` | 2: `v1`, `v2` | 0.54 | M máx -21.0 | -1.4 | B · final temporal |
| `sfx_libro_cerrar` | Cerrar el Libro Mayor: tapas de cuero que se juntan | `ui:libro_cerrar` | 2: `v1`, `v2` | 0.22 | M máx -22.3 a -22.0 | -1.5 | B · final temporal |
| `sfx_libro_folio` | Una ventana de estado se escribe: pluma que anota números en trazos cortos | `ui:folio` | 3: `v1`, `v2`, `v3` | 0.38 a 0.52 | M máx -22.3 a -22.2 | -1.5 | B · final temporal |
| `sfx_libro_titulo` | Título ganado: sello de lacre que se estampa y una campanita | `ui:titulo` | 2: `v1`, `v2` | 0.79 | M máx -19.0 | -6.1 | B · final temporal |
| `sfx_libro_falla` | Con Paralaje alto la ventana falla: una sílaba susurrada al revés (ruido formántico, sin palabras) y papel que se rasga | `ui:falla` | 4: `v1`, `v2`, `v3`, `v4` | 0.52 | M máx -22.1 a -22.0 | -5.9 | B · final temporal |

## 3.5.12 Taller (crafting)

| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_taller_yunque` | Martillo del Yunque Astral: golpe de forja en Re, más brillante por nivel del arma (Rústico, Forjado, Alquímico, Astral, Quintaesencia) | `f2:forja` (nivel 1 a 5) | 5: `1`, `2`, `3`, `4`, `5` | 0.26 a 0.64 | M máx -19.5 a -17.0 | -1.5 | B · final temporal |
| `sfx_taller_reforja` | Reforja: el astrolabio gira, se frena y se detiene en un prefijo con un clic y una campanita | `f2:reforja` | 1 archivo | 1.30 | M máx -19.9 | -1.5 | B · final temporal |
| `sfx_taller_fusion` | Fusión legendaria: cuencos que giran, un brillo que sube y un acorde de campanas de Re mayor con un golpe | `f2:fusion` | 1 archivo | 1.74 | M máx -15.0 | -3.0 | B · final temporal |
| `sfx_taller_caldero` | Hervor del Caldero de Bruja (bucle): burbujas graves y un fuego bajo | Pantalla del Caldero | 1 archivo | 3.40 | I -28.1 | -14.1 | B · borrador · bucle |
| `sfx_taller_ingrediente` | Echar un ingrediente al caldero: base (chorro), sal (granos), reactivo (gota que burbujea) o almitas (campanitas que se hunden) | `ui:ingrediente` | 4: `base`, `sal`, `reactivo`, `almitas` | 0.44 a 0.53 | M máx -22.3 a -22.0 | -1.5 | B · final temporal |
| `sfx_taller_salmuera_ok` | Salmuera lista: el burbujeo se resuelve en cuencos de cobre; una nota (básica), dos (mejorada) o una tríada con brillo (maestra) | `f2:salmuera_ok` | 3: `basica`, `mejorada`, `maestra` | 0.75 a 0.83 | M máx -19.0 | -5.9 | B · final temporal |
| `sfx_taller_salmuera_turbia` | Falla: Salmuera Turbia; borbotones torpes, un cuenco desafinado y un 'plop' | `f2:salmuera_turbia` | 1 archivo | 0.78 | M máx -20.0 | -7.9 | B · final temporal |
| `sfx_taller_altar` | Refinar sal en el Altar del Árbol: el crujido de grano sucio se aclara hasta el tono puro del metal del planeta (barrido de Pureza de 0.70 a 1.00) | `f2:refinar` (`Pureza`) | 7: `sol`, `luna`, `marte`, `mercurio`, `venus`, `jupiter`, `saturno` | 0.71 | M máx -19.9 a -19.0 | -1.5 | B · final temporal |
| `sfx_taller_receta` | Receta nueva en el Recetario: hoja, pluma y una campanita | `f2:receta_nueva` | 1 archivo | 0.79 | M máx -20.0 | -3.9 | B · final temporal |

## 3.5.13 Almitas

| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_almita_soltar` | Algo muere o se rompe y suelta almitas: campanilleo agudo y breve | `UnitDefeated` (enemigo) o `deriv:almitas_soltadas`, `f2:` | 4: `v1`, `v2`, `v3`, `v4` | 0.36 | M máx -24.0 | -8.4 | A · final temporal |
| `sfx_almita_recoger` | Recoger almitas: arpegio pentatónico que crece con la cantidad (pocas, varias, muchas) | `ui:almitas_recogidas` | 3: `pocas`, `varias`, `muchas` | 0.52 a 0.68 | M máx -21.0 | -8.2 | B · final temporal |
| `sfx_almita_nombre` | Almita con nombre que carga un recuerdo: campanilla lenta sobre un salterio lejano | `f2:almita_nombre` | 1 archivo | 1.11 | M máx -20.0 | -8.8 | B · final temporal |
| `sfx_almita_spark` | Almita extra del Spark: una sola campanilla brillante encima de la campana | `HitLanded` (`IsSpark`), capa | 2: `v1`, `v2` | 0.42 | M máx -22.0 | -9.6 | A · final temporal |
| `sfx_almita_ascendente` | El Ascendente come almitas al subir de Dignidad: campanillas que se recogen al revés hacia un cuenco grave | `f2:dignidad` | 1 archivo | 0.85 | M máx -20.1 | -9.2 | B · final temporal |
| `sfx_almita_flotar` | Almitas que flotan hacia el Fresno (bucle): campanillas sueltas y aire | Escena de exploración | 1 archivo | 2.00 | I -31.8 | -15.6 | B · borrador · bucle |

## 3.5.14 Horror y Paralaje

| ID | Sonido | Disparador | Variantes | Duración (s) | Sonoridad (LUFS) | Pico real (dBTP) | Estado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `sfx_horror_huesped` | Huéspedes: señales de radiotelescopio; estática, clics lentos de púlsar y un tono de onda corta que deriva | Escena o `anim:` | 1 archivo | 1.58 | M máx -22.2 | -10.9 | B · final temporal |
| `sfx_horror_salmodico_reves` | Susurro de Salmódico al revés: aire con forma de vocales, sin palabras ni voz, invertido (va con subtítulo de glifos) | Escena con Paralaje 2 o más | 2: `v1`, `v2` | 1.03 | M máx -24.4 a -24.0 | -9.9 | B · final temporal |
| `sfx_horror_grave` | Grave casi inaudible (bucle): pedal bajo 40 Hz con armónicos de 80 a 200 Hz para que exista en la bocina del teléfono | Lecho | 1 archivo | 2.00 | I -32.4 | -18.4 | B · borrador · bucle |
| `sfx_horror_cristalizado` | Estatuas de sal que todavía susurran: susurro apagado y cristal que cruje despacio | Emisores 3D | 1 archivo | 1.20 | M máx -24.0 | -10.9 | B · final temporal |
| `sfx_horror_cielo` | El cielo cruje cuando la cámara deja de mirar: un crujido enorme y lento, y un retumbo lejano | Escena | 1 archivo | 1.74 | M máx -24.2 | -4.6 | B · final temporal |
| `sfx_horror_caida` | El Ascendente en Caída muestra un instante su forma: acorde de cristal invertido que crece, se quiebra y deja un susurro. Sube, no salta | `SalinityBandChanged` (a 150) con Paralaje 3 | 1 archivo | 1.82 | M máx -18.0 | -8.4 | B · final temporal |
| `sfx_paralaje_sube` | Sube el Paralaje: un acorde desafinado de cristal y un susurro que crece; más denso y más grave por nivel (medio, alto, máximo) | `meta:paralaje_sube` (nivel 1 a 3) | 3: `1`, `2`, `3` | 1.38 | M máx -21.1 a -19.1 | -9.0 | B · final temporal |
| `sfx_paralaje_baja` | Baja el Paralaje (Termas, Agua de Olvido): la disonancia se resuelve en un acorde limpio de cristal y agua | `meta:paralaje_baja` | 1 archivo | 1.60 | M máx -20.0 | -9.2 | B · final temporal |
