# Genera Assets/SalYAlmas/Data/odyssey.json (reglas, enemigos y zonas de la Odisea) y aplica a story.json las regiones
# de Cáncer y Libra, el mapeo de signos por elemento y los enemigos regionales de Aries y Capricornio.
# Diseño en docs/propuestas/ODISEA.md. Todo número es // TUNING.
# Uso, desde la raíz del repositorio:  python Tools/Content/gen_odyssey.py
import json, os, re, sys
sys.path.insert(0, os.path.dirname(__file__))
from odyssey_story import CANCER, LIBRA

DATA = 'Assets/SalYAlmas/Data/'

RULES = {
    'levelBase': 5, 'levelStep': 6, 'zoneAdd': [0, 2, 4, 8], 'bossAdd': 2, 'maxTide': 5, 'tideHp': 0.06,
    'waveStrength': 0.9, 'sectionAlmitas': 100, 'sectionExponent': 1.5,
    'materialChance': 0.6, 'materialMin': 1, 'materialMax': 3, 'saltChance': 0.35, 'treasureChance': 0.2,
    'zoneSales': 100, 'expeditionSections': 3, 'dailyFull': 20, 'afterCap': 0.25, 'fallenHp': 0.10,
    'grietaInvaders': ['cristalizado'], 'abismoInvaders': ['sin_carta', 'huesped_menor'],
    # La caravana comercia: los tesoros de Superficie dan materiales de cualquier región, así todos se consiguen.
    'tradeMaterials': ['hierro_rojo', 'cobre_llano', 'vidrio_espejo', 'nacar', 'arena_aurea', 'hilo_hilandera',
                       'pesa_ducal', 'azufre', 'crin_centauro', 'obsidiana', 'hielo_eterno', 'escama_ninfa'],
}

# Estadísticas a nivel 50 por arquetipo. El juego las escala al nivel de la zona con la curva de progresión.
ARCH = {
    'comun':   dict(hp=14000, atk=1050, spd=104, toughness=45),
    'rapido':  dict(hp=11000, atk=1150, spd=124, toughness=40),
    'tanque':  dict(hp=21000, atk=900,  spd=90,  toughness=75),
    'nativo':  dict(hp=17000, atk=1100, spd=106, toughness=55),
    'fuerte':  dict(hp=30000, atk=1250, spd=102, toughness=110),
    'invasor': dict(hp=18000, atk=1150, spd=100, toughness=70),
    'jefe_ex': dict(hp=80000, atk=1500, spd=110, toughness=200, boss=True, actionsPerTurn=2),
    'jefe':    dict(hp=95000, atk=1550, spd=108, toughness=230, boss=True, actionsPerTurn=2),
}

def sk(id_, name, target, mult=0, hits=None, effects=None, **kw):
    d = {'id': id_, 'name': name, 'target': target}
    if mult: d['multiplier'] = mult; d['hits'] = hits or [12]
    if effects: d['effects'] = effects
    d.update(kw)
    return d

def dot(kind, v, t, n): return {'type': 'dot', 'dot': kind, 'value': v, 'turns': t, 'name': n}
def debuff(stat, v, t, n): return {'type': 'debuff', 'stat': stat, 'value': v, 'turns': t, 'name': n}
def buff(stat, v, t, n): return {'type': 'buff', 'stat': stat, 'value': v, 'turns': t, 'target': 'self', 'name': n}

def enemy(id_, name, arch, planet, weak, skills, pattern, desc='', article='la', traits=None, phase2=None, short=None, **over):
    a = dict(ARCH[arch]); a.update(over)
    e = {'id': id_, 'name': name, 'shortName': short or name, 'article': article, 'description': desc, 'level': 50,
         'hp': a['hp'], 'atk': a['atk'], 'spd': a['spd'], 'toughness': a['toughness'], 'attackPlanet': planet,
         'weaknesses': weak, 'res': {planet: 0.2}, 'skills': skills, 'pattern': pattern}
    if a.get('boss'): e['boss'] = True
    if a.get('actionsPerTurn'): e['actionsPerTurn'] = a['actionsPerTurn']
    if traits: e['traits'] = traits
    if phase2: e['phase2'] = phase2
    return e

def ult(id_, name, mult, text, effects=None):
    return sk(id_, name, 'all', mult, [[10, 0.4], [30, 0.6]], effects, ultimate=True, charge=True, chargeText=text)

E = [
    # --- Prólogo
    enemy('figura_sal', 'Figura de Sal', 'comun', 'saturno', ['marte', 'sol'],
          [sk('mano', 'Mano Estirada', 'random', 1.2, [8, 18]), sk('susurro', 'Susurro', 'all', 0.5, [14])], ['mano', 'mano', 'susurro'],
          'Alguien a medio volverse sal. Todavía susurra.', traits={'shape': 'cristal', 'hint': 'Sin trucos: aprende la Cadena.'}),
    # --- Aries: Montes de Hierro Rojo
    enemy('escoria_viva', 'Escoria Viva', 'comun', 'marte', ['saturno', 'luna'],
          [sk('salpicar', 'Salpicar', 'random', 1.2, [10]), sk('fundir', 'Fundirse', 'lowestHp', 1.0, [8, 16], [dot('burn', 0.2, 2, 'Escoria')])],
          ['salpicar', 'fundir'], 'Masa de hierro fundido que se arrastra.',
          traits={'shape': 'masa', 'hint': 'Al caer salpica escoria ardiente a todas.', 'onDeath': [dot('burn', 0.15, 2, 'Salpicadura')]}),
    enemy('perro_fragua', 'Perro de Fragua', 'rapido', 'marte', ['saturno', 'venus'],
          [sk('mordida', 'Mordida', 'lowestHp', 1.3, [6, 14]), sk('aullido', 'Aullido de Fuelle', 'self', 0, None, [buff('speed', 0.15, 2, 'Fuelle')])],
          ['mordida', 'mordida', 'aullido'], 'Perro de hierro remachado con brasas en el pecho.', article='el',
          traits={'shape': 'bestia', 'hint': 'Rápido: va por la bruja más herida.'}),
    enemy('soldada_crisol', 'Soldada de Crisol', 'nativo', 'marte', ['saturno', 'luna'],
          [sk('tajo', 'Tajo', 'random', 1.3, [10, 20]), sk('sorbo', 'Sorbo de Crisol', 'self', 0, None, [buff('res', 0.15, 2, 'Sal Fundida')])],
          ['tajo', 'tajo', 'sorbo'], 'Bebe sal sin refinar. Crece y olvida su nombre.',
          traits={'shape': 'torre', 'rampAtk': 0.10, 'hint': 'Gana 10 % de ataque cada turno que sigue en pie: mátala pronto.'}),
    enemy('ave_a_medias', 'El Ave a Medias', 'fuerte', 'saturno', ['marte', 'sol'],
          [sk('picotazo', 'Picotazo de Cristal', 'random', 1.4, [6, 12, 18]), sk('aletazo', 'Alas de Más', 'all', 0.6, [10, 22], [debuff('speed', 0.1, 2, 'Vértigo')])],
          ['picotazo', 'aletazo'], 'Algo de sal que tiene forma de ave y no la ha entendido del todo.', article='el',
          traits={'shape': 'ave', 'hint': 'Baja la Velocidad de todas: abre la Cadena antes que ella.'}),
    enemy('ferran', 'Capitana Ferrán', 'jefe', 'marte', ['saturno', 'luna', 'venus'],
          [sk('tajo', 'Tajo de Capitana', 'random', 1.5, [8, 20], [dot('bleed', 0.3, 2, 'Herida')]),
           sk('carga', 'Carga de Hierro', 'all', 0.75, [12, 24]),
           sk('grito', 'Grito del Carnero', 'self', 0, None, [buff('atk', 0.25, 2, 'Grito del Carnero')]),
           ult('juramento', 'Juramento Roto', 2.0, 'Ferrán clava la espada en el suelo. El Carnero se levanta detrás de ella.')],
          ['tajo', 'carga', 'grito', 'tajo', 'juramento'], 'Manda la Casa de Aries en estos montes.',
          traits={'shape': 'torre', 'hint': 'Cuando grita, sube su ataque: rómpela antes de que cargue el Juramento.'},
          phase2={'hpBelow': 0.5, 'spdBonus': 15, 'text': 'Ferrán suelta la espada. El Carnero mira por sus ojos.', 'pattern': ['tajo', 'carga', 'juramento']}),
    enemy('carnero_hierro', 'Carnero de Hierro', 'jefe_ex', 'marte', ['saturno', 'luna'],
          [sk('embestida', 'Embestida', 'random', 1.6, [10, 24]), sk('cuernos', 'Cuernos en Espiral', 'all', 0.7, [8, 20, 32]),
           ult('estrella', 'La Estrella del Pecho', 2.2, 'La grieta del pecho se abre. Hay una estrella adentro que parpadea.')],
          ['embestida', 'cuernos', 'embestida', 'estrella'], 'El espíritu de Ferrán, suelto sobre las islas de mena.', article='el',
          traits={'shape': 'bestia', 'rampAtk': 0.06, 'hint': 'Crece cada turno, como las soldadas que lo alimentan.'},
          phase2={'hpBelow': 0.5, 'spdBonus': 20, 'text': 'La estrella del pecho deja de parpadear y te mira.', 'pattern': ['embestida', 'estrella', 'cuernos']}),
    # --- Cáncer: Costa de las Conchas
    enemy('medusa_salmuera', 'Medusa de Salmuera', 'comun', 'luna', ['marte', 'sol'],
          [sk('roce', 'Roce Turbio', 'random', 1.1, [8, 16], [dot('poison', 0.2, 2, 'Salmuera')]), sk('nube', 'Nube de Sal', 'all', 0.45, [14])],
          ['roce', 'nube'], 'Medusa de salmuera con cristales flotando dentro.', traits={'shape': 'flor', 'hint': 'Envenena: ten a mano una cura.'}),
    enemy('gaviota_hueca', 'Gaviota Hueca', 'rapido', 'luna', ['jupiter', 'marte'],
          [sk('picada', 'Picada', 'lowestHp', 1.35, [6, 12])], ['picada'], 'Una gaviota con cuerpo de concha vacía.',
          traits={'shape': 'ave', 'hint': 'Siempre ataca a la más herida.'}),
    enemy('cangrejo_salinero', 'Cangrejo Salinero', 'nativo', 'luna', ['marte', 'jupiter'],
          [sk('pinza', 'Pinza', 'random', 1.2, [10, 20]), sk('escudo', 'Concha Alzada', 'self', 0, None, [buff('res', 0.2, 2, 'Concha Alzada')])],
          ['pinza', 'escudo'], 'Alza la pinza como escudo frente a los suyos.', article='el', hp=19000, toughness=70,
          traits={'shape': 'cangrejo', 'protectAllies': 0.4, 'hint': 'Mientras viva, los demás reciben 40 % menos de daño: rómpelo primero.'}),
    enemy('madre_tenaza', 'Madre Tenaza', 'jefe', 'luna', ['marte', 'jupiter', 'sol'],
          [sk('tenaza', 'Tenaza', 'lowestHp', 1.6, [10, 24]),
           sk('marea', 'Marea que Hunde', 'all', 0.7, [14, 28], [{'type': 'delay', 'value': 0.10, 'name': 'Marea'}]),
           sk('caparazon', 'Caparazón de Pueblo', 'self', 0, None, [buff('res', 0.3, 2, 'Caparazón')]),
           ult('hundimiento', 'Hundimiento', 2.1, 'Madre Tenaza se sumerge. La campana del pueblo de su lomo empieza a sonar.')],
          ['tenaza', 'marea', 'caparazon', 'tenaza', 'hundimiento'], 'Un cangrejo colosal con un pueblo entero sobre el caparazón.',
          traits={'shape': 'cangrejo', 'hint': 'Su caparazón resiste: quiébrala con sus debilidades antes del Hundimiento.'},
          phase2={'hpBelow': 0.5, 'spdBonus': 10, 'text': 'Las casitas del lomo encienden sus ventanas. Alguien sigue viviendo ahí.', 'pattern': ['tenaza', 'hundimiento', 'marea']}),
    enemy('medusa_cielo', 'Medusa del Cielo', 'jefe_ex', 'luna', ['marte', 'sol'],
          [sk('tentaculo', 'Tentáculo que Sube', 'random', 1.4, [8, 16, 24], [dot('poison', 0.25, 2, 'Agua de Arriba')]),
           sk('lluvia', 'Lluvia al Revés', 'all', 0.6, [12, 24], [dot('poison', 0.15, 2, 'Agua de Arriba')]),
           ult('marea_alta', 'Marea del Cielo', 2.0, 'El mar entero sube hacia la medusa.')],
          ['tentaculo', 'lluvia', 'marea_alta'], 'Flota entre las nubes; sus tentáculos caen hacia arriba.',
          traits={'shape': 'flor', 'healAllies': 0.04, 'hint': 'Se cura sola cada turno: el veneno se acumula, la paciencia no.'}),
    # --- Libra: Ducado de las Balanzas
    enemy('moneda_andante', 'Moneda Andante', 'tanque', 'venus', ['marte', 'saturno'],
          [sk('rodar', 'Rodar de Canto', 'random', 1.1, [10, 18])], ['rodar'], 'Una moneda de bronce con una cara que parpadea.',
          traits={'shape': 'ojo', 'hint': 'Mucha Firmeza: rómpela con sus debilidades.'}),
    enemy('guardia_contrapeso', 'Guardia de Contrapeso', 'comun', 'venus', ['marte', 'mercurio'],
          [sk('pesa', 'Golpe de Pesa', 'random', 1.25, [12]), sk('peso', 'Peso Muerto', 'random', 0.6, [10], [debuff('atk', 0.15, 2, 'Peso Muerto')])],
          ['pesa', 'peso'], 'Guardia del banco con una pesa colgada del brazo.', article='el',
          traits={'shape': 'torre', 'hint': 'Baja el ataque de una bruja: límpiala o cámbiala de turno.'}),
    enemy('tesorera_almas', 'Tesorera de Almas', 'nativo', 'venus', ['marte', 'saturno'],
          [sk('cobro', 'Cobro', 'random', 1.1, [8, 16], [{'type': 'stealGrains', 'value': 1, 'name': 'Cobro'}]),
           sk('interes', 'Interés', 'all', 0.5, [14])],
          ['cobro', 'interes'], 'Lleva el libro de cuentas encadenado a la muñeca.',
          traits={'shape': 'torre', 'healAllies': 0.06, 'hint': 'Cobra un Grano en cada golpe y paga a sus guardias con vida: es la prioridad.'}),
    enemy('duquesa_fiel', 'La Duquesa del Fiel', 'jefe', 'venus', ['marte', 'saturno', 'mercurio'],
          [sk('fiel', 'Aguja del Fiel', 'highestSalinity', 1.6, [10, 22]),
           sk('balanza', 'Balanza Inclinada', 'all', 0.7, [12, 26], [debuff('damage', 0.1, 2, 'Deuda')]),
           sk('pago', 'Pago Pendiente', 'self', 0, None, [buff('atk', 0.2, 2, 'Intereses')]),
           ult('juicio', 'Juicio de la Balanza', 2.0, 'La Duquesa pone tu nombre en un platillo. El otro está vacío.')],
          ['fiel', 'balanza', 'pago', 'fiel', 'juicio'], 'Pesa almas. Nunca se equivoca de gramo.',
          traits={'shape': 'espejo', 'reflect': 0.10, 'hint': 'Te devuelve 10 % del daño: lo que pagas, te lo cobra.'},
          phase2={'hpBelow': 0.5, 'spdBonus': 15, 'text': 'El platillo vacío baja solo. Algo invisible acaba de sentarse en él.', 'pattern': ['fiel', 'juicio', 'balanza']}),
    enemy('contador_sin_rostro', 'Contador sin Rostro', 'jefe_ex', 'venus', ['marte', 'saturno'],
          [sk('borron', 'Borrón', 'all', 0.6, [10, 20], [{'type': 'dispel', 'count': 1, 'name': 'Borrón'}]),
           sk('pluma', 'Pluma en el Aire', 'random', 1.5, [6, 14, 22]),
           ult('saldo', 'Saldo en Cero', 2.1, 'Las páginas del cielo se ordenan en columnas. Tu nombre está en la de restar.')],
          ['pluma', 'borron', 'pluma', 'saldo'], 'Una toga de páginas sin cara, con manos que escriben.', article='el',
          traits={'shape': 'sombra', 'hint': 'Borra tus mejoras: lánzalas justo antes de la Cadena.'}),
    # --- Capricornio: Salinas de Obsidiana
    enemy('gota_obsidiana', 'Gota de Obsidiana', 'comun', 'saturno', ['sol', 'luna'],
          [sk('rebote', 'Rebote', 'random', 1.15, [8, 18])], ['rebote'], 'Una gota de vidrio negro que rebota.',
          traits={'shape': 'masa', 'hint': 'Al romperse suelta sal: +10 de Salinidad a todas.', 'onDeath': [{'type': 'salinity', 'value': 10, 'name': 'Esquirlas'}]}),
    enemy('garza_obsidiana', 'Garza de Obsidiana', 'rapido', 'saturno', ['jupiter', 'sol'],
          [sk('navaja', 'Pico de Navaja', 'random', 1.3, [6, 14], [dot('bleed', 0.25, 2, 'Corte de Vidrio')])], ['navaja'],
          'Plumas de vidrio negro y patas larguísimas.', traits={'shape': 'ave', 'hint': 'Sus cortes sangran: termina rápido o cura.'}),
    enemy('sorbedor', 'Sorbedor de Poza', 'nativo', 'saturno', ['sol', 'jupiter'],
          [sk('sorbo', 'Sorbo', 'random', 1.15, [10]), sk('remolino', 'Remolino', 'all', 0.4, [14], [{'type': 'delay', 'value': 0.10, 'name': 'Remolino'}])],
          ['sorbo', 'remolino'], 'Un remolino pequeño con boca.', article='el',
          traits={'shape': 'ojo', 'hint': 'Cada dos turnos retrasa a todo el equipo: quiébralo antes del Remolino.'}),
    enemy('boca_vieja', 'La Boca Vieja', 'jefe', 'saturno', ['sol', 'luna', 'jupiter'],
          [sk('trago', 'Trago', 'random', 1.6, [10, 24]),
           sk('giro', 'Giro', 'all', 0.6, [12, 26], [{'type': 'delay', 'value': 0.15, 'name': 'Giro'}]),
           ult('traga_barcas', 'Traga Barcas', 2.0, 'La Boca Vieja se abre. Ya no se ve el fondo.')],
          ['trago', 'giro', 'trago', 'traga_barcas'], 'El remolino que se traga las barcas.', hp=55000, actionsPerTurn=1, short='Boca Vieja',
          traits={'shape': 'ojo', 'hint': 'Retrasa a todas cada dos turnos. Su gemela la cura: decide a cuál romper primero.'}),
    enemy('boca_nueva', 'La Boca Nueva', 'jefe', 'saturno', ['sol', 'marte', 'jupiter'],
          [sk('sorbo', 'Sorbo Amable', 'lowestHp', 1.4, [8, 18]), sk('canto', 'Canto de Espuma', 'all', 0.5, [14], [debuff('speed', 0.1, 2, 'Espuma')])],
          ['sorbo', 'canto'], 'La gemela limpia. Se traga a la gente y deja las barcas.', hp=45000, actionsPerTurn=1, short='Boca Nueva',
          traits={'shape': 'ojo', 'healAllies': 0.05, 'hint': 'Cura a su gemela cada turno.'}),
    enemy('serpiente_nube', 'Serpiente de Nube', 'jefe_ex', 'saturno', ['sol', 'jupiter'],
          [sk('escamas', 'Lluvia de Escamas', 'all', 0.55, [6, 14, 22, 30], [dot('shock', 0.15, 2, 'Rayo de Sal')]),
           sk('mordida', 'Mordida de Nube', 'random', 1.5, [10, 24]),
           ult('tormenta', 'Tormenta de Cacao', 2.1, 'La serpiente se enrosca en las dos columnas. Huele a cacao quemado.')],
          ['mordida', 'escamas', 'mordida', 'tormenta'], 'Larguísima, hecha de nubes de sal y escamas de obsidiana.',
          traits={'shape': 'ave', 'hint': 'Golpes en área con electrocución: reparte la vida.'}),
    # --- Invasores de la Marea (GDD 6.5)
    enemy('sin_carta', 'Sin Carta', 'invasor', 'saturno', ['sol', 'marte'],
          [sk('vacio', 'Vacío', 'random', 1.3, [12]), sk('borrar', 'Borrar el Cielo', 'all', 0.5, [16])], ['vacio', 'borrar'],
          'No tiene carta natal. Los Aspectos no tienen dónde agarrarse.', article='el',
          traits={'shape': 'sombra', 'aspectImmune': True, 'hint': 'Inmune a los Aspectos: gana con Spark, Firmeza y Ascensiones.'}),
    enemy('huesped_menor', 'Huésped Menor', 'invasor', 'jupiter', ['saturno', 'luna'],
          [sk('plegar', 'Plegarse', 'random', 1.4, [8, 16, 24]), sk('mirar', 'Mirar con Todos', 'all', 0.6, [12], [debuff('res', 0.1, 2, 'Paralaje')])],
          ['plegar', 'mirar'], 'Metal líquido que se pliega en ángulos que no deberían existir.', article='el', hp=24000,
          traits={'shape': 'ojo', 'reflect': 0.08, 'hint': 'Devuelve parte del daño y baja tus resistencias.'},
          phase2={'hpBelow': 0.5, 'spdBonus': 25, 'text': 'El Huésped se despliega. Tiene más ojos de los que cabían.', 'pattern': ['plegar', 'plegar', 'mirar']}),
]

# Zonas: las tres primeras bajan y la EX sube. "sections" = combates previos en cada combate de historia;
# en la EX, secciones antes del jefe.
REGIONS = [
    dict(id='aries', native='soldada_crisol', material='hierro_rojo', planet='marte', station='fragua_hierro_rojo', zones=[
        dict(id='aries_orilla', kind='orilla', name='Valdehierro', description='El pueblo minero arde bajo las torres de hierro.',
             missions=['aries_1'], pool=['escoria_viva', 'perro_fragua', 'soldada_crisol'], sections=1),
        dict(id='aries_subsuelo', kind='subsuelo', name='El tiro de los Diecinueve', description='Túneles de sal que respiran.',
             missions=['aries_2'], pool=['escoria_viva', 'figura_sal', 'perro_fragua'], sections=2),
        dict(id='aries_corazon', kind='corazon', name='Rojaforja', description='La fortaleza de los crisoles y la catedral de sal.',
             missions=['aries_3', 'aries_4', 'aries_5'], pool=['soldada_crisol', 'perro_fragua', 'escoria_viva'], boss='ferran', sections=2),
        dict(id='aries_cielo', kind='cielo', name='Islas de Mena', description='Islas de hierro flotando sobre los montes. Hay una estrella de más.',
             pool=['soldada_crisol', 'perro_fragua', 'ave_a_medias'], boss='carnero_hierro', sections=4),
    ]),
    dict(id='cancer', native='cangrejo_salinero', material='nacar', planet='luna', station='', zones=[
        dict(id='cancer_orilla', kind='orilla', name='Kerbruma', description='Marea baja entre barcas salineras varadas.',
             missions=['can_1', 'can_2'], pool=['medusa_salmuera', 'gaviota_hueca', 'cangrejo_salinero'], sections=1),
        dict(id='cancer_subsuelo', kind='subsuelo', name='La Cueva de Nácar', description='Solo se abre cuando el mar se retira.',
             missions=['can_3'], pool=['medusa_salmuera', 'cangrejo_salinero', 'figura_sal'], sections=2),
        dict(id='cancer_corazon', kind='corazon', name='El Pueblo del Caparazón', description='La Casa vive sobre un cangrejo colosal.',
             missions=['can_4', 'can_5'], pool=['cangrejo_salinero', 'gaviota_hueca', 'medusa_salmuera'], boss='madre_tenaza', sections=2),
        dict(id='cancer_cielo', kind='cielo', name='La Marea del Cielo', description='El agua sube en columnas y no vuelve.',
             pool=['medusa_salmuera', 'gaviota_hueca', 'cangrejo_salinero'], boss='medusa_cielo', sections=4),
    ]),
    dict(id='libra', native='tesorera_almas', material='pesa_ducal', planet='venus', station='', zones=[
        dict(id='libra_orilla', kind='orilla', name='La Aduana de Almas', description='Canales, logias y faroles con almitas presas.',
             missions=['lib_1', 'lib_2'], pool=['moneda_andante', 'guardia_contrapeso', 'tesorera_almas'], sections=1),
        dict(id='libra_subsuelo', kind='subsuelo', name='Las Bóvedas', description='Pasillos que se repiten bajo el banco.',
             missions=['lib_3'], pool=['moneda_andante', 'tesorera_almas', 'figura_sal'], sections=2),
        dict(id='libra_corazon', kind='corazon', name='El Salón del Fiel', description='Una balanza colosal entre sal y almas.',
             missions=['lib_4', 'lib_5'], pool=['guardia_contrapeso', 'tesorera_almas', 'moneda_andante'], boss='duquesa_fiel', sections=2),
        dict(id='libra_cielo', kind='cielo', name='El Libro del Cielo', description='Páginas flotantes y balanzas colgadas de las estrellas.',
             pool=['moneda_andante', 'guardia_contrapeso', 'tesorera_almas'], boss='contador_sin_rostro', sections=4),
    ]),
    dict(id='capricornio', native='sorbedor', material='obsidiana', planet='saturno', station='horno_obsidiana', zones=[
        dict(id='capricornio_orilla', kind='orilla', name='Las Pozas de Zapotitlán', description='Salinas como espejos negros.',
             missions=['cap_1', 'cap_2'], pool=['gota_obsidiana', 'garza_obsidiana', 'sorbedor'], sections=1),
        dict(id='capricornio_subsuelo', kind='subsuelo', name='El Mercado de Abajo', description='Cavernas de obsidiana y venas de magma.',
             missions=['cap_3'], pool=['gota_obsidiana', 'sorbedor', 'figura_sal'], sections=2),
        dict(id='capricornio_corazon', kind='corazon', name='Las Dos Bocas', description='Dos columnas de sal que giran sin moverse.',
             missions=['cap_4', 'cap_5'], pool=['sorbedor', 'garza_obsidiana', 'gota_obsidiana'], boss='boca_vieja', sections=2),
        dict(id='capricornio_cielo', kind='cielo', name='El Ojo de Nube', description='Las columnas terminan en una nube que mira.',
             pool=['garza_obsidiana', 'gota_obsidiana', 'sorbedor'], boss='serpiente_nube', sections=4),
    ]),
]

# Combates de historia de Aries y Capricornio: de enemigos del Fresno a enemigos de su región.
BATTLES = {
    'p4': (['figura_sal'], None),
    'a2': (['escoria_viva', 'perro_fragua'], 1.0), 'b2': (['figura_sal', 'escoria_viva', 'figura_sal'], 1.0),
    'c4': (['soldada_crisol', 'soldada_crisol'], 1.0), 'd1': (['ferran'], 1.0), 'e2': (['ave_a_medias'], 1.0),
    'f2': (['gota_obsidiana', 'garza_obsidiana'], 1.0), 'g3': (['sorbedor', 'figura_sal'], 1.0),
    'h3': (['garza_obsidiana', 'gota_obsidiana', 'sorbedor'], 1.0), 'i2': (['sorbedor', 'sorbedor', 'garza_obsidiana'], 1.0),
    'j2': (['boca_vieja', 'boca_nueva'], 1.0),
}

SIGNS = {'Fuego': 'aries', 'Tierra': 'capricornio', 'Aire': 'libra', 'Agua': 'cancer'}

def write_odyssey():
    out = {'rules': RULES, 'enemies': E, 'regions': REGIONS}
    header = ('// GENERADO por Tools/Content/gen_odyssey.py: cambia el generador, no este archivo.\n'
              '// La Odisea (docs/propuestas/ODISEA.md): reglas, enemigos regionales con sus rasgos y zonas de cada región.\n'
              '// Estadísticas de enemigos a nivel 50; el juego las escala al nivel de la zona. Todo número es // TUNING.\n')
    with open(DATA + 'odyssey.json', 'w', encoding='utf-8', newline='\n') as f:
        f.write(header + compact(json.dumps(out, ensure_ascii=False, indent=1)) + '\n')

def compact(text):
    # Junta en una línea las listas cortas de números o textos, para que el archivo se pueda leer.
    return re.sub(r'\[\s*((?:-?[\d.]+|"[^"\n]*"|\[\s*-?[\d.]+,\s*-?[\d.]+\s*\])(?:,\s*(?:-?[\d.]+|"[^"\n]*"|\[\s*-?[\d.]+,\s*-?[\d.]+\s*\]))*)\s*\]',
                  lambda m: '[' + re.sub(r'\s+', ' ', m.group(1)).replace('[ ', '[').replace(' ]', ']') + ']', text)

def scene_battle_line(sid, enemies, strength, boss_hint):
    return None

def patch_story():
    p = DATA + 'story.json'
    s = open(p, encoding='utf-8', newline='').read()
    crlf = '\r\n' in s
    s = s.replace('\r\n', '\n')
    # 1. Signos por elemento a la región cardinal (D-13): Fuego Aries, Tierra Capricornio, Aire Libra, Agua Cáncer.
    def sign_rep(m):
        line = m.group(0)
        el = re.search(r'"element": "(\w+)"', line).group(1)
        return re.sub(r'"region": "\w+"', '"region": "%s"' % SIGNS[el], line)
    s = re.sub(r'\{ "id": "\w+", "name": "[^"]+", "element": "\w+".*\n', sign_rep, s)
    s = s.replace('// Solo dos regiones en esta versión: cada signo lleva a la más cercana por elemento (HUECOS C-36, D-13).',
                  '// Cuatro regiones cardinales, una por elemento: cada signo lleva a la de su elemento (D-13, ODISEA 9).')
    # 2. La corrupción es la Marea de Sal: llega a 5 (ODISEA 3).
    s = s.replace('"maxCorruption": 2, // HUECOS H-16 (tres estados)', '"maxCorruption": 5, // ODISEA 3: la corrupción es la Marea de Sal (0 a 5)')
    # 3. Combates de historia con enemigos regionales.
    for sid, (enemies, strength) in BATTLES.items():
        m = re.search(r'\{ "id": "%s",.*?"battle": \{([^}]*)\}' % sid, s, re.S)
        if not m: print('NO BATTLE', sid); continue
        body = m.group(1)
        nb = re.sub(r'"enemies": \[[^\]]*\]', '"enemies": ' + json.dumps(enemies, ensure_ascii=False), body)
        if strength is not None: nb = re.sub(r'"strength": [\d.]+', '"strength": %s' % strength, nb)
        s = s[:m.start(1)] + nb + s[m.end(1):]
    # 4. Cáncer y Libra, antes del cierre de "regions".
    if '"id": "can_1"' not in s:
        tail = '\n    }\n  ]\n}'
        i = s.rstrip().rfind(tail.strip()[:1])
        idx = s.rstrip().rfind('    }\n  ]\n}')
        assert idx > 0, 'no encontré el cierre de regions'
        insert = ''.join(',\n' + indent(json.dumps(r, ensure_ascii=False, indent=2), 4) for r in (CANCER, LIBRA))
        s = s[:idx + len('    }')] + insert + s[idx + len('    }'):]
    if crlf: s = s.replace('\n', '\r\n')
    open(p, 'w', encoding='utf-8', newline='').write(s)

def indent(text, n):
    return '\n'.join(' ' * n + line for line in compact(text).split('\n'))

if __name__ == '__main__':
    write_odyssey()
    patch_story()
    print('odyssey.json:', len(E), 'enemigos,', len(REGIONS), 'regiones; story.json actualizado')
