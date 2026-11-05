# Guion de Cáncer y Libra para story.json. BORRADOR de Claude Code (HUECOS H-14): falta la aprobación del usuario.
# Lo usa gen_odyssey.py. Cada región: cinco misiones repartidas en orilla, subsuelo y corazón (odyssey.json).

# Personajes que no son brujas: su retrato sale de Art/Ambientes o Art/Npc (encargo de Codex, prioridad 4).
NPC = {'Ferrán': 'npc_ferran', 'Doña Tecuani': 'npc_tecuani', 'Almiranta Corvina': 'npc_corvina', 'Maela': 'npc_maela', 'Aduanera': 'npc_aduanera', 'La Duquesa': 'npc_duquesa'}

def sc(id_, text, speaker=None, portrait=None, nxt=None, variants=None, choices=None, battle=None, effects=None):
    if speaker in NPC and not portrait: portrait = NPC[speaker]
    d = {'id': id_}
    if speaker: d['speaker'] = speaker
    if portrait: d['portrait'] = portrait
    d['text'] = text
    if variants: d['variants'] = [{'when': w, 'text': t} for w, t in variants]
    if choices: d['choices'] = choices
    if battle: d['battle'] = battle
    if effects: d['effects'] = effects
    if nxt: d['next'] = nxt
    return d

def ch(text, nxt, stat=None, need=None, when=None, **effects):
    d = {'text': text}
    if when: d['when'] = when
    if stat: d['stat'] = stat; d['need'] = need
    if effects: d['effects'] = effects
    d['next'] = nxt
    return d

def bt(enemies, nxt, boss=False, strength=1.0):
    d = {'enemies': enemies, 'strength': strength}
    if boss: d['boss'] = True
    d['next'] = nxt
    return d

CANCER = {
    'id': 'cancer', 'sign': 'cancer', 'name': 'Costa de las Conchas', 'theme': 'Mareas, granito y casas dentro de conchas',
    'echo': 'Los lestrigones',
    'missions': [
        {'id': 'can_1', 'name': 'Marea baja en Kerbruma', 'description': 'Los cangrejos hunden los barcos salineros.', 'start': 'k1',
         'rewards': {'res': {'almitas': 6000, 'sales_de_bano': 80}},
         'scenes': [
             sc('k1', 'La marea se retiró tanto que el mar parece otro país. Las barcas salineras quedaron varadas en el lodo, de lado, con la carga intacta. Nadie se acerca a descargarlas.',
                variants=[({'marks': ['primera_aries']}, 'Todavía hueles a fragua. Aquí todo huele a algas y a sal mojada. La marea se retiró tanto que el mar parece otro país, y las barcas salineras quedaron varadas de lado, con la carga intacta. Nadie se acerca a descargarlas.'),
                          ({'minCorruption': 1}, 'La marea se retiró, y no del todo hacia el mar: hay charcos que suben por las rocas como si alguien los jalara. Las barcas salineras quedaron varadas de lado, con la carga intacta. Nadie se acerca a descargarlas.')],
                nxt='k2'),
             sc('k2', '¿Vienes por la sal? Llévatela, si puedes. La última que bajó a descargar no volvió. Algo camina bajo el lodo cuando la marea está baja, y tiene pinzas.', speaker='Maela',
                battle=bt(['medusa_salmuera', 'cangrejo_salinero'], 'k3')),
             sc('k3', 'Lo viste. No es un cangrejo cualquiera. Los grandes nunca salen del agua, y aun así los barcos se hunden en la marea alta, como si alguien los jalara del fondo.', speaker='Maela',
                choices=[ch('Descargar la sal de las barcas', 'k4', stat='oficio', need=5, marks=['descargo_barcas'], res={'sal_cruda.luna': 4}),
                         ch('Preguntar quién compra la sal hundida', 'k4', marks=['pregunto_casa'], souls=10)]),
             sc('k4', 'En la punta de la bahía hay un faro de piedra. Su luz no gira: apunta fija hacia el mar, hacia un punto donde el agua da vueltas en círculo.'),
         ]},
        {'id': 'can_2', 'name': 'Lo que deja la marea', 'description': 'El farero sabe adónde van los barcos.', 'start': 'l1',
         'rewards': {'witch': 'axa', 'res': {'almitas': 7000, 'sal_cruda.luna': 5}},
         'scenes': [
             sc('l1', 'El farero no te abre. Te habla por la rendija: «La luz no es para los barcos. Es para ella. Si la apago, sube».', nxt='l2'),
             sc('l2', 'En una poza al pie del faro hay alguien sentada en el agua, con branquias rosadas y una sonrisa que no tiene nada que ver con lo que pasa.', nxt='l3'),
             sc('l3', '¡Hola! No te asustes, yo siempre estoy mojada. Te iba a avisar: las gaviotas de aquí ya no tienen nada adentro. Y vienen para acá.', speaker='Axa', portrait='axa',
                battle=bt(['gaviota_hueca', 'gaviota_hueca', 'medusa_salmuera'], 'l4')),
             sc('l4', 'Me ahogué una vez, ¿sabes? En esta misma poza. Volví. El agua todavía no me quiere. Si tu Tortuga tiene manantial, ¿me llevas? Prometo no ocupar mucho.', speaker='Axa', portrait='axa',
                choices=[ch('«Claro. Sube»', 'l5', marks=['axa_sube']),
                         ch('«¿Cómo que volviste?»', 'l6', stat='encanto', need=5, marks=['axa_conto'], paralaje=3)]),
             sc('l5', 'Axa se mete al manantial del caparazón con ropa y todo. El farero, por fin, abre la puerta.'),
             sc('l6', 'Abajo hay una cueva que brilla. Allá llegan todos los que se hunden. No se mueren, nada más se quedan muy quietos y muy bonitos. Yo me salí antes de quedarme bonita.', speaker='Axa', portrait='axa', nxt='l5'),
         ]},
        {'id': 'can_3', 'name': 'La cueva de nácar', 'description': 'Adonde van los que se hunden.', 'start': 'm1',
         'rewards': {'res': {'almitas': 8000, 'sales_de_bano': 80}},
         'scenes': [
             sc('m1', 'Con la marea más baja del año se abre la cueva. Las paredes son de nácar, y en el nácar hay figuras: marineros, salineras, un perro. Están dentro de la concha como en un espejo.',
                variants=[({'minCorruption': 2}, 'Con la marea más baja del año se abre la cueva. Las paredes son de nácar, y en el nácar hay figuras: marineros, salineras, un perro. Algunas tienen los ojos abiertos hacia arriba, hacia algo que el techo no deja ver.')],
                nxt='m2'),
             sc('m2', 'Almas en depósito: 3 112. Estado: perlificadas. Propietaria registrada: Casa del Cangrejo. Fecha de cobro: «cuando suba la marea grande».', speaker='Libro Mayor',
                effects={'paralaje': 6}, nxt='m3'),
             sc('m3', 'Los cangrejos salineros cargan costales de sal desde los barcos hundidos y los apilan junto a las figuras. Te ven, y sueltan la carga.',
                battle=bt(['cangrejo_salinero', 'cangrejo_salinero', 'medusa_salmuera'], 'm4')),
             sc('m4', 'Aquí estuve. ¿Ves esa figura sin cara? Era mi lugar. Alguien lo está guardando para alguien más.', speaker='Axa', portrait='axa',
                choices=[ch('Romper el nácar y liberar a las figuras', 'm5', marks=['rompio_nacar'], souls=15, paralaje=4),
                         ch('Calcular cuántas caben todavía', 'm5', stat='calculo', need=6, marks=['conto_nacar'], res={'sal_destilada.luna': 2})]),
             sc('m5', 'Al salir, la marea ya viene de regreso. A lo lejos, en el agua, algo enorme se mueve con casitas encima.'),
         ]},
        {'id': 'can_4', 'name': 'La Almiranta', 'description': 'La Casa del Cangrejo vive sobre su caparazón.', 'start': 'n1',
         'rewards': {'res': {'almitas': 10000, 'sales_de_bano': 80}},
         'scenes': [
             sc('n1', 'Una contadora. Bien. Nadie más en esta costa sabe sumar lo que se hunde. ¿Quieres saber por qué hundimos los barcos, {nombre}? Porque la sal que cargan es de gente. Y la gente no se vende.', speaker='Almiranta Corvina',
                variants=[({'marks': ['perdono_capitana']}, '«Me dijeron que perdonaste a Ferrán en los Montes», dice la Almiranta Corvina, sin saludar. «Aquí no hay a quién perdonar. Aquí hay cuentas. ¿Quieres saber por qué hundimos los barcos, {nombre}? Porque la sal que cargan es de gente».')],
                nxt='n2'),
             sc('n2', 'El pueblo del caparazón tiene escaleras, ropa tendida y una campana. Debajo, Madre Tenaza respira, y todo el pueblo sube y baja con ella.', nxt='n3'),
             sc('n3', 'Pero tampoco la dejamos en el mar. La guardamos. Hasta que suba la marea grande y alguien venga a cobrarla. No sé quién es. Sé que paga bien.', speaker='Almiranta Corvina',
                choices=[ch('«Eso no es guardar. Es vender más tarde»', 'n4', marks=['enfrento_corvina']),
                         ch('Pedirle el libro de cuentas de la Casa', 'n5', stat='encanto', need=6, marks=['libro_corvina'], paralaje=5)]),
             sc('n4', 'La Almiranta toca la campana. El caparazón entero se levanta del agua.', battle=bt(['madre_tenaza'], 'n6', boss=True)),
             sc('n5', 'Libro de la Casa del Cangrejo. Comprador: sin nombre. Firma: una constelación que no está en ningún mapa. Pago por adelantado: once mareas tranquilas.', speaker='Libro Mayor',
                effects={'paralaje': 6}, nxt='n4'),
             sc('n6', 'Basta. Basta. Si la matas, el pueblo se hunde con ella. ¿Eso también lo calculaste?', speaker='Almiranta Corvina'),
         ]},
        {'id': 'can_5', 'name': 'El pueblo sobre la tenaza', 'description': 'Decidir qué se hunde.', 'start': 'o1',
         'rewards': {'witch': 'charca', 'res': {'almitas': 12000, 'sales_de_bano': 160, 'quintaesencia.luna': 1}},
         'scenes': [
             sc('o1', 'Madre Tenaza está quieta. El pueblo de su lomo no hace ruido. En el agua, la figura sin cara de la cueva flota sola hacia la superficie.', nxt='o2'),
             sc('o2', 'Los últimos cangrejos salineros suben por el caparazón a defender a la abuela.', battle=bt(['cangrejo_salinero', 'gaviota_hueca', 'cangrejo_salinero'], 'o3')),
             sc('o3', '¡Cuac! Perdón. Soy de aquí, del pueblo. Bueno, de arriba del pueblo. Si van a decidir algo sobre la abuela Tenaza, quiero estar, porque me sacó del agua dos veces.', speaker='Charca', portrait='charca', nxt='o4'),
             sc('o4', 'Elige, contadora. Las once mareas tranquilas siguen si dejamos que el comprador cobre. Si no, el mar se pone bravo y la costa entera paga.', speaker='Almiranta Corvina',
                choices=[ch('Romper el trato: que el mar se ponga bravo', 'o5', marks=['rompio_trato_mar'], souls=20, paralaje=4),
                         ch('Renegociar: menos sal, menos mareas', 'o5', stat='calculo', need=7, marks=['trato_mar_justo']),
                         ch('Escuchar la campana', 'o6', when={'minParalaje': 20}, marks=['oyo_campana'], paralaje=12)]),
             sc('o5', 'Entonces me voy con ustedes. Alguien tiene que avisarle a la abuela cómo terminó la cuenta, y yo nado rapidísimo.', speaker='Charca', portrait='charca'),
             sc('o6', 'La campana suena cada vez que un alma se vuelve perla. Ha sonado 3 112 veces. Alguien lleva la cuenta en el cielo, y no eres tú.', speaker='Libro Mayor', nxt='o5'),
         ]},
    ],
}

LIBRA = {
    'id': 'libra', 'sign': 'libra', 'name': 'Ducado de las Balanzas', 'theme': 'Canales, bancos y almas en frascos',
    'echo': 'Los feacios',
    'missions': [
        {'id': 'lib_1', 'name': 'La aduana de almas', 'description': 'Para entrar al Ducado hay que pagar.', 'start': 'q1',
         'rewards': {'res': {'almitas': 6000, 'sales_de_bano': 80}},
         'scenes': [
             sc('q1', 'La ciudad flota sobre canales verdes. En la entrada hay una aduana con una balanza de cobre, y cada viajero deja algo en el platillo: una moneda, un diente, una almita que le sale del pecho como una luciérnaga.',
                variants=[({'marks': ['primera_cancer']}, 'Del mar a los canales. Aquí el agua está quieta y huele a cera. En la entrada hay una aduana con una balanza de cobre, y cada viajero deja una almita en el platillo, como una luciérnaga.'),
                          ({'minCorruption': 1}, 'La ciudad flota sobre canales verdes. En la entrada hay una aduana con una balanza de cobre que se inclina sola antes de que nadie ponga nada en el platillo.')],
                nxt='q2'),
             sc('q2', 'Nombre, signo y peso del alma. ¿{nombre}? No apareces. Pon el pecho en la balanza. Ya. No pesas nada. Eso, en el Ducado, es un delito o un milagro, y no sé cuál cobra más.', speaker='Aduanera', nxt='q3'),
             sc('q3', 'Las monedas del platillo empiezan a rodar solas hacia ti.', battle=bt(['moneda_andante', 'guardia_contrapeso'], 'q4')),
             sc('q4', 'Bueno. Pasa. Pero te van a querer conocer en el banco. A los que no pesan nada, el banco les presta sin límite.', speaker='Aduanera',
                choices=[ch('Entrar como si nada', 'q5', marks=['entro_libra']),
                         ch('Preguntar cuánto vale un alma aquí', 'q5', stat='calculo', need=5, marks=['precio_alma'], souls=5)]),
             sc('q5', 'En cada farol de la ciudad hay una almita encerrada. Alumbran poco, y parpadean como si quisieran decir algo.'),
         ]},
        {'id': 'lib_2', 'name': 'Cuentas pendientes', 'description': 'Una ratona le roba sal al banco.', 'start': 'r1',
         'rewards': {'witch': 'migaja', 'res': {'almitas': 7000, 'sal_cruda.venus': 5}},
         'scenes': [
             sc('r1', 'En el mercado alguien grita «¡ladrona!». Una ratona con los bolsillos llenos de granitos de sal corre entre tus piernas y se esconde detrás de tu capa.', nxt='r2'),
             sc('r2', '¡Shhh! No es robar si el banco se lo robó primero. Esa sal era de mi abuela. Bueno, era mi abuela. Ahí vienen.', speaker='Migaja', portrait='migaja',
                battle=bt(['guardia_contrapeso', 'guardia_contrapeso', 'tesorera_almas'], 'r3')),
             sc('r3', 'Gracias. ¿Tú eres la que no pesa? Todo el mercado habla de ti. La Duquesa también, seguro. Ella pesa todo.', speaker='Migaja', portrait='migaja',
                choices=[ch('Devolver la sal al banco', 'r4', marks=['devolvio_sal'], souls=-5),
                         ch('Ayudarla a esconderla', 'r4', stat='oficio', need=5, marks=['escondio_sal'], res={'sal_cruda.venus': 3})]),
             sc('r4', 'Me caes bien. Me voy en tu Tortuga, ¿sí? Aquí ya me conocen la cara. Además, sé dónde están las bóvedas.', speaker='Migaja', portrait='migaja'),
         ]},
        {'id': 'lib_3', 'name': 'Las bóvedas', 'description': 'Bajo el banco, los frascos con nombre.', 'start': 's1',
         'rewards': {'res': {'almitas': 8000, 'sales_de_bano': 80}},
         'scenes': [
             sc('s1', 'Bajo el banco hay pasillos de ladrillo que se repiten. Cada estante tiene frascos, y cada frasco una almita con su etiqueta: nombre, peso y fecha de depósito.',
                variants=[({'minCorruption': 2}, 'Bajo el banco hay pasillos de ladrillo que se repiten. Cada estante tiene frascos con almitas y etiquetas. Algunos están vacíos, con la etiqueta tachada en una letra que no es de nadie del Ducado.')],
                nxt='s2'),
             sc('s2', 'Coincidencia de registro: el 61 % de los frascos aparece en el Libro Mayor como «sin llegar al Fresno». El banco lleva una copia. La copia tiene más columnas que el original.', speaker='Libro Mayor',
                effects={'paralaje': 7}, nxt='s3'),
             sc('s3', 'Los pasillos cambian de orden cuando das la vuelta. Una tesorera camina hacia ti con el libro abierto.', battle=bt(['tesorera_almas', 'moneda_andante', 'moneda_andante'], 's4')),
             sc('s4', '¡Ahí! El frasco de mi abuela. Dice «Clara Granero, 41 gramos». Pesaba más, te lo juro. Le quitaron algo.', speaker='Migaja', portrait='migaja',
                choices=[ch('Abrir el frasco y dejarla ir', 's5', marks=['libero_frasco'], souls=10, paralaje=3),
                         ch('Leer el libro de la tesorera', 's6', stat='calculo', need=6, marks=['leyo_banco'], paralaje=6)]),
             sc('s5', 'La almita sale, da una vuelta alrededor de Migaja y vuela hacia arriba, hacia el Fresno. Las demás se pegan al vidrio.'),
             sc('s6', 'Columna de la derecha: «Comisión». Cada alma deja aquí un gramo para un cliente que firma con estrellas. El cliente nunca ha pedido un retiro. Está esperando a que crezca.', speaker='Libro Mayor', nxt='s5'),
         ]},
        {'id': 'lib_4', 'name': 'La Duquesa del Fiel', 'description': 'Ella pesa todo.', 'start': 't1',
         'rewards': {'res': {'almitas': 10000, 'sales_de_bano': 80}},
         'scenes': [
             sc('t1', 'Siéntate, {nombre}. Nadie se sienta aquí, pero tú no pesas, así que la silla no se va a quejar. Te hago una oferta: te llevo a casa. A tu casa de verdad, la de antes del río.', speaker='La Duquesa', nxt='t2'),
             sc('t2', 'En el Ducado siempre llevamos a los náufragos a su casa. Solo pido una cosa: tu alma, que no está escrita, en mi balanza. Para siempre. Ni siquiera la vas a extrañar.', speaker='La Duquesa',
                choices=[ch('«No»', 't3', marks=['rechazo_duquesa']),
                         ch('«¿Y quién es tu cliente?»', 't4', stat='encanto', need=6, marks=['cliente_duquesa'], paralaje=6)]),
             sc('t3', 'La Duquesa suspira. La balanza del salón se inclina hacia ti sin que nadie la toque.', battle=bt(['duquesa_fiel'], 't5', boss=True)),
             sc('t4', 'Mi cliente no tiene cara, querida. Tiene cuentas. Y paga en eclipses.', speaker='La Duquesa', effects={'paralaje': 4}, nxt='t3'),
             sc('t5', 'Está bien. Gana la casa, o sea, tú. Pero el platillo vacío ya no es mío. Si no lo llena nadie, lo llena él.', speaker='La Duquesa'),
         ]},
        {'id': 'lib_5', 'name': 'El platillo vacío', 'description': 'Lo que pesa la balanza grande.', 'start': 'u1',
         'rewards': {'witch': 'vellon', 'res': {'almitas': 12000, 'sales_de_bano': 160, 'quintaesencia.venus': 1}},
         'scenes': [
             sc('u1', 'En el centro del salón, la balanza colosal tiene en un platillo un monte de sal blanca, y en el otro, miles de almitas. Están en equilibrio. Si una se mueve, todo se cae.', nxt='u2'),
             sc('u2', 'Los guardias del salón no saben que la Duquesa perdió. Siguen cobrando.', battle=bt(['guardia_contrapeso', 'tesorera_almas', 'guardia_contrapeso'], 'u3')),
             sc('u3', 'Perdón, perdón. Soy la que limpia el salón. Nadie me ve porque casi no peso. Llevo años contando las almitas del platillo. Hoy hay una menos. ¿Fuiste tú?', speaker='Vellón', portrait='vellon', nxt='u4'),
             sc('u4', 'Si se cae la balanza, las almas se van al Fresno. Todas. Y la sal se vuelve gente otra vez, en la calle. Nadie sabe qué pasa después.', speaker='Vellón', portrait='vellon',
                choices=[ch('Volcar la balanza', 'u5', marks=['volco_balanza'], souls=25, paralaje=6),
                         ch('Equilibrarla con las cuentas del banco', 'u5', stat='calculo', need=7, marks=['balanza_justa']),
                         ch('Poner tu mano en el platillo vacío', 'u6', when={'minParalaje': 25}, marks=['mano_platillo'], paralaje=15)]),
             sc('u5', 'Bueno. Ya no hay nada que limpiar aquí. ¿Me llevas? Sé hacer cobijas, y en tu Tortuga hace frío de noche.', speaker='Vellón', portrait='vellon'),
             sc('u6', 'Tu mano no pesa. El platillo baja de todas formas. Algo del otro lado del cielo acaba de poner la suya encima de la tuya.', speaker='Libro Mayor', nxt='u5'),
         ]},
    ],
}
