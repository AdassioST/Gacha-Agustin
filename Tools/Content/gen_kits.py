# Genera Assets/SalYAlmas/Data/witches_ext.json: los kits de combate de las 18 brujas que no están en witches.json.
# Cada kit sale de una plantilla por papel más una tabla de nombres propia. Todos los números son // TUNING.
# Uso, desde la raíz del repositorio:  python Tools/Content/gen_kits.py
import json, copy, os

OUT = 'Assets/SalYAlmas/Data/witches_ext.json'

# Escala por rareza (traspaso de la Odisea): las 5★ como las de witches.json, ×0.9 para 4★ y ×0.8 para 3★.
RARITY_SCALE = {5: 1.0, 4: 0.9, 3: 0.8}
RARITY_SPD = {5: 0, 4: -2, 3: -4}

def eff(type_, **kw):
    d = {'type': type_}
    d.update(kw)
    return d

# Plantillas por papel. Cada movimiento: (multiplicador, Firmeza, golpes, efectos). Los golpes de Cadena terminan en
# 26 o 50 cuando conviene un Spark con un eslabón que empiece en 2 (GDD 9.6).
ROLES = {
    'Guardiana': {
        'stats': (11000, 1150, 100),
        'basic': (1.0, 10, [[6, 0.5], [18, 0.5]], []),
        'spell': dict(target='self', effects=[eff('taunt', target='self', turns=2, name='Provocación'),
                                              eff('shield', target='allAllies', value=0.12, turns=2, name='Escudo')]),
        'spell_desc': 'Atrae los golpes dos turnos y escuda al aquelarre con 12 % de su vida.',
        'opening': (0.9, 15, [[4, 0.5], [22, 0.5]], [eff('debuff', stat='atk', value=0.12, turns=2, at='start', name='Peso')]),
        'link': (1.0, 15, [[2, 0.4], [26, 0.6]], [eff('shield', target='allAllies', value=0.05, turns=2, name='Muro')]),
        'closing': (1.8, 30, [[10, 0.4], [34, 0.6]], [eff('delay', value=0.10, name='Retraso')]),
        'asc': [(2.4, 30, [[20, 0.5], [40, 0.5]], [eff('shield', target='allAllies', value=0.15, turns=2, name='Escudo')]),
                (3.4, 40, [[16, 0.3], [36, 0.3], [56, 0.4]], [eff('taunt', target='self', turns=2, name='Provocación'),
                                                              eff('buff', stat='res', value=0.20, turns=2, target='allAllies', name='Firmeza')]),
                (6.0, 60, [[30, 0.3], [60, 0.3], [90, 0.4]], [eff('shield', target='allAllies', value=0.30, turns=2, name='Gran Escudo')])],
        'self': 'agujaYSello',
    },
    'Sanadora': {
        'stats': (8800, 1250, 112),
        'basic': (1.0, 10, [[8, 0.5], [20, 0.5]], []),
        'spell': dict(target='ally', effects=[eff('heal', value=0.30, name='Cura'), eff('cleanse', count=1, name='Limpia')]),
        'spell_desc': 'Cura 30 % de la vida a una aliada y le quita un estado.',
        'opening': (0.8, 10, [[2, 0.5], [20, 0.5]], [eff('heal', target='allAllies', value=0.04, name='Rocío')]),
        'link': (1.0, 15, [[2, 0.3], [14, 0.3], [26, 0.4]], [eff('heal', target='allAllies', value=0.05, name='Rocío')]),
        'closing': (1.6, 25, [[8, 0.5], [30, 0.5]], [eff('heal', target='allAllies', value=0.10, name='Lluvia')]),
        'asc': [(2.0, 30, [[16, 0.5], [32, 0.5]], [eff('heal', target='allAllies', value=0.20, name='Lluvia'), eff('cleanse', target='allAllies', count=1, name='Limpia')]),
                (2.8, 40, [[12, 0.3], [30, 0.3], [48, 0.4]], [eff('heal', target='allAllies', value=0.30, name='Marea'),
                                                              eff('shield', target='allAllies', value=0.10, turns=2, name='Velo')]),
                (5.0, 60, [[20, 0.3], [50, 0.3], [80, 0.4]], [eff('heal', target='allAllies', value=0.45, name='Gran Marea')])],
        'self': 'hilos',
    },
    'Rompedora': {
        'stats': (9200, 1500, 106),
        'basic': (1.0, 15, [[6, 0.4], [16, 0.6]], []),
        'spell': dict(mult=2.0, tough=35, hits=[[10, 0.5], [26, 0.5]], effects=[eff('debuff', stat='defenseDown', value=0.15, turns=2, name='Grieta')]),
        'spell_desc': 'Quita mucha Firmeza y baja 15 % la defensa dos turnos.',
        'opening': (1.0, 20, [[4, 0.5], [26, 0.5]], [eff('nextToughness', target='otherAllies', value=0.15, at='start', name='Fisura')]),
        'link': (1.2, 25, [[2, 0.4], [26, 0.6]], []),
        'closing': (2.3, 45, [[8, 0.3], [28, 0.3], [50, 0.4]], []),
        'asc': [(3.0, 45, [[20, 0.5], [40, 0.5]], []),
                (4.8, 60, [[16, 0.3], [36, 0.3], [60, 0.4]], [eff('debuff', stat='defenseDown', value=0.20, turns=2, name='Quiebre')]),
                (8.0, 90, [[30, 0.3], [60, 0.3], [90, 0.4]], [])],
        'self': 'agujaYSello',
    },
    'Saboteadora': {
        'stats': (8200, 1450, 118),
        'basic': (1.0, 10, [8, 20], []),
        'spell': dict(mult=1.6, tough=20, hits=[[12, 0.5], [28, 0.5]], effects=[eff('dispel', count=1, name='Disipa'), eff('delay', value=0.15, name='Retraso')]),
        'spell_desc': 'Quita una mejora al objetivo y lo retrasa 15 %.',
        'opening': (0.8, 15, [[2, 0.5], [18, 0.5]], [eff('debuff', stat='speed', value=0.10, turns=2, at='start', name='Torpeza')]),
        'link': (1.1, 15, [[2, 0.5], [26, 0.5]], [eff('debuff', stat='damageTaken', value=0.10, turns=2, name='Mal de Ojo')]),
        'closing': (2.0, 25, [[8, 0.4], [34, 0.6]], [eff('delay', value=0.10, name='Retraso')]),
        'asc': [(2.6, 30, [[20, 0.5], [40, 0.5]], [eff('delay', value=0.20, name='Retraso')]),
                (4.2, 40, [[16, 0.3], [36, 0.3], [56, 0.4]], [eff('dispel', count=2, name='Disipa'), eff('debuff', stat='atk', value=0.15, turns=2, name='Desgano')]),
                (7.2, 60, [[30, 0.3], [60, 0.3], [90, 0.4]], [eff('delay', value=0.30, name='Gran Retraso')])],
        'self': 'hilos',
    },
    'Ofensiva': {
        'stats': (8400, 1750, 110),
        'basic': (1.0, 10, [[6, 0.4], [16, 0.6]], []),
        'spell': dict(mult=2.3, tough=20, hits=[[10, 0.3], [24, 0.7]], effects=[]),
        'spell_desc': '',
        'opening': (1.0, 15, [[4, 0.5], [26, 0.5]], [eff('debuff', stat='damageTaken', value=0.12, turns=2, at='start', name='Marca')]),
        'link': (1.4, 15, [[2, 0.3], [22, 0.3], [46, 0.4]], []),
        'closing': (2.5, 30, [[8, 0.2], [28, 0.3], [50, 0.5]], []),
        'asc': [(3.4, 30, [[20, 0.4], [40, 0.6]], []),
                (5.3, 40, [[16, 0.25], [36, 0.25], [60, 0.5]], [eff('buff', stat='atk', value=0.12, turns=2, target='allAllies', name='Arrojo')]),
                (8.8, 60, [[30, 0.2], [60, 0.3], [90, 0.5]], [])],
        'self': 'agujaYSello',
    },
    'Ofensiva con drenaje': {
        'stats': (9000, 1600, 110),
        'basic': (1.0, 10, [[6, 0.5], [18, 0.5]], [eff('heal', target='self', value=0.03, name='Sorbo')]),
        'spell': dict(mult=2.1, tough=20, hits=[[10, 0.5], [26, 0.5]], effects=[eff('heal', target='self', value=0.12, name='Sorbo')]),
        'spell_desc': 'Golpea y se cura 12 % de su vida.',
        'opening': (1.0, 15, [[2, 0.5], [26, 0.5]], [eff('heal', target='self', value=0.04, name='Sorbo')]),
        'link': (1.3, 15, [[2, 0.3], [22, 0.3], [46, 0.4]], [eff('heal', target='self', value=0.05, name='Sorbo')]),
        'closing': (2.4, 30, [[8, 0.3], [30, 0.7]], [eff('heal', target='self', value=0.08, name='Trago')]),
        'asc': [(3.2, 30, [[20, 0.5], [40, 0.5]], [eff('heal', target='self', value=0.15, name='Trago')]),
                (5.0, 40, [[16, 0.3], [36, 0.3], [60, 0.4]], [eff('heal', target='allAllies', value=0.10, name='Copa Compartida')]),
                (8.4, 60, [[30, 0.3], [60, 0.3], [90, 0.4]], [eff('heal', target='self', value=0.30, name='Banquete')])],
        'self': 'frenesiSalNegra',
    },
    'Tejedora': {
        'stats': (8000, 1350, 130),
        'basic': (1.0, 10, [4, 10, 16], []),
        'spell': dict(target='allAllies', effects=[eff('buff', stat='atk', value=0.12, turns=2, target='allAllies', name='Hilo de Fuerza'),
                                                   eff('advance', value=0.12, target='otherAllies', name='Adelanto')]),
        'spell_desc': '+12 % de ataque al aquelarre y adelanta 12 % a las demás.',
        'opening': (0.8, 10, [2, 12, 22], [eff('grains', value=1, at='start')]),
        'link': (1.0, 15, [2, 14, 26], [eff('buff', stat='damage', value=0.08, turns=2, target='allAllies', name='Trama')]),
        'closing': (1.8, 25, [6, 16, 26, 36, 50], []),
        'asc': [(2.4, 30, [[10, 0.5], [20, 0.5]], [eff('advance', value=0.20, target='otherAllies')]),
                (3.2, 40, [8, 18, 28, 38, 48], [eff('buff', stat='atk', value=0.18, turns=2, target='allAllies', name='Gran Trama'),
                                                eff('advance', value=0.20, target='otherAllies')]),
                (6.0, 60, [10, 20, 30, 40, 50, 60, 70], [])],
        'self': 'hilos',
    },
    'Ofensiva contra jefes': {
        'stats': (8000, 1650, 108),
        'basic': (1.0, 10, [[6, 0.5], [18, 0.5]], [eff('dot', dot='poison', value=0.15, turns=2, name='Intoxicación')]),
        'spell': dict(mult=0.6, tough=20, hits=[[14, 1.0]], effects=[eff('maxHpBurst', value=0.08, name='Explosión de Veneno')]),
        'spell_desc': 'Convierte su intoxicación en una explosión: 8 % de la vida máxima del objetivo (la mitad si no está intoxicada).',
        'opening': (1.0, 15, [[4, 0.5], [26, 0.5]], [eff('dot', dot='poison', value=0.25, turns=2, name='Veneno')]),
        'link': (1.3, 15, [[2, 0.5], [26, 0.5]], [eff('dot', dot='poison', value=0.25, turns=2, name='Veneno')]),
        'closing': (2.2, 30, [[8, 0.4], [34, 0.6]], [eff('dot', dot='poison', value=0.35, turns=2, name='Veneno')]),
        'asc': [(3.0, 30, [[20, 0.5], [40, 0.5]], [eff('maxHpBurst', value=0.05, name='Explosión')]),
                (4.6, 40, [[16, 0.3], [36, 0.3], [60, 0.4]], [eff('dot', dot='poison', value=0.5, turns=3, name='Veneno Viejo')]),
                (7.6, 60, [[30, 0.3], [60, 0.3], [90, 0.4]], [eff('maxHpBurst', value=0.10, name='Gran Explosión')])],
        'self': 'none',
    },
}

# Tabla propia de cada bruja: nombres de sus movimientos y lo que la distingue. Los nombres son propuestas // TUNING.
W = [
    dict(id='cervanda', name='Cervanda', species='Cierva con cornamenta', rarity=5, role='Guardiana', p='saturno', s='jupiter',
         sign='Capricornio', asc='La Cabra de Plomo Real', ritual='Sirve sal de un salero de plata con modales de corte', mix='Plomo Real',
         selfname='Sello de la Corte',
         moves=['Reverencia de Asta', 'Audiencia de Plomo', 'Edicto', 'Muralla de Corte', 'Sentencia Grave', 'Cornamenta de Plomo', 'Corte Cerrada', 'La Corte Inmóvil'],
         regencia=dict(name='Corte de Plomo', description='+15 % de vida a todo el aquelarre.', teamHp=0.15),
         tweak=lambda k: k['moves']['spell']['effects'].append(eff('delay', target='allEnemies', value=0.05, name='Gravedad'))),
    dict(id='cupria', name='Cupria', species='Serpiente', rarity=5, role='Sanadora', p='venus', s='luna',
         sign='Virgo', asc='La Doncella de Cobre', ritual='Su Ascendente le pone la sal en la lengua', mix='Leche de Cobre',
         selfname='Sello de Cobre',
         moves=['Lengua Bífida', 'Remedio Ambiguo', 'Muda de Piel', 'Savia de Cobre', 'Veneno que Cura', 'Huerto Secreto', 'Herbolaria Mayor', 'El Jardín que Perdona'],
         regencia=dict(name='Lengua de Cobre', description='Empieza el combate con un Grano.', startGrains=1),
         tweak=lambda k: k['moves']['closing']['effects'].append(eff('dot', dot='poison', value=0.2, turns=2, name='Veneno Moral')),
         selfform='agujaYSello'),
    dict(id='brea', name='Brea', species='Gata negra (kemonomimi)', rarity=4, role='Rompedora', p='saturno', s='luna',
         sign='Libra', asc='La Balanza de Brea', ritual='Lame sal de un plato de alquitrán frío', mix='Humo Óseo', selfname='Contrapeso',
         moves=['Zarpazo de Brea', 'Platillo Caído', 'Fiel Torcido', 'Contrapeso', 'Balanza Rota', 'Peso Muerto', 'Juicio de Alquitrán', 'La Balanza Negra']),
    dict(id='grajilla', name='Grajilla', species='Cuerva', rarity=4, role='Saboteadora', p='mercurio', s='saturno',
         sign='Acuario', asc='El Aguador de Hollín', ritual='Roba sal brillante de los saleros ajenos', mix='Azogue de Plomo', selfname='Hilos de Hollín',
         moves=['Picotazo', 'Rapiña', 'Graznido Torcido', 'Pluma Falsa', 'Nido de Mentiras', 'Bandada', 'Ladrona de Brillos', 'El Cielo de Hollín']),
    dict(id='madrona', name='Madroña', species='Osa', rarity=4, role='Guardiana', p='venus', s='marte',
         sign='Tauro', asc='El Toro de Madroño', ritual='Unta sal en miel de madroño', mix='Herrumbre Dulce', selfname='Sello de Miel',
         moves=['Zarpa Pesada', 'Abrazo de Osa', 'Gruñido', 'Panal', 'Golpe de Hibernación', 'Cueva Tibia', 'Madre del Monte', 'El Bosque Cerrado'],
         tweak=lambda k: k['stats'].update(hp=int(k['stats']['hp'] * 1.1))),
    dict(id='risca', name='Risca', species='Hiena', rarity=4, role='Ofensiva', p='marte', s='mercurio',
         sign='Escorpio', asc='El Escorpión que Ríe', ritual='Mastica sal gruesa con risa nerviosa', mix='Chispa Herrumbrosa', selfname='Sello de Risa',
         moves=['Dentellada', 'Risa de Sierra', 'Herida Abierta', 'Carroña', 'Jauría', 'Cosquilla de Hierro', 'La Que Ríe Última', 'El Escorpión Desatado'],
         tweak=lambda k: [k['moves'][m]['effects'].append(eff('dot', dot='bleed', value=0.25, turns=2, name='Sangrado')) for m in ('spell', 'link')]),
    dict(id='nacarina', name='Nacarina', species='Nutria (kemonomimi)', rarity=4, role='Sanadora', p='luna', s='jupiter',
         sign='Cáncer', asc='El Cangrejo de Nácar', ritual='Abre ostras con sal y las sorbe', mix='Marea Tonante', selfname='Hilos de Nácar',
         moves=['Coletazo', 'Perla Tibia', 'Corriente', 'Oleaje', 'Concha Cerrada', 'Marea Alta', 'Arrecife', 'La Perla del Fondo'],
         tweak=lambda k: k['moves']['spell']['effects'].append(eff('advance', value=0.15, name='Corriente'))),
    dict(id='centella', name='Centella', species='Lince', rarity=4, role='Rompedora', p='jupiter', s='marte',
         sign='Sagitario', asc='El Arquero de Rayo', ritual='Frota sal entre las patas hasta que chispea', mix='Rayo Férreo', selfname='Sello de Rayo',
         moves=['Zarpazo Eléctrico', 'Descarga', 'Mirada de Lince', 'Rayo Partido', 'Trueno Seco', 'Flecha de Rayo', 'Tormenta Férrea', 'El Cielo que Cae'],
         tweak=lambda k: k['moves']['spell']['effects'].append(eff('dot', dot='shock', value=0.25, turns=2, name='Electrocución'))),
    dict(id='vespera', name='Véspera', species='Murciélaga', rarity=4, role='Ofensiva con drenaje', p='saturno', s='venus',
         sign='Escorpio', asc='El Escorpión del Ocaso', ritual='Bebe agua salada a la hora del ocaso', mix='Cobre Viejo', selfname='Frenesí del Ocaso',
         moves=['Mordisco', 'Sed de Ocaso', 'Ala Negra', 'Sorbo Largo', 'Noche Cerrada', 'Vuelo Bajo', 'Copa Roja', 'El Ocaso Sin Fin']),
    dict(id='cempa', name='Cempa', species='Coyota', rarity=4, role='Saboteadora', p='mercurio', s='sol',
         sign='Sagitario', asc='El Arquero Embaucador', ritual='Espolvorea sal con chile sobre fruta robada', mix='Fango Sulfúrico', selfname='Hilos de Trato',
         moves=['Mordida Ladina', 'Trato Hecho', 'Cobro por Adelantado', 'Letra Chica', 'Truco Viejo', 'La Coyota Cobra', 'Aquí No Pasó Nada', 'El Trato Final'],
         tweak=lambda k: k['moves']['spell']['effects'].append(eff('resonance', target='self', value=10, name='Ganancia'))),
    dict(id='ixbalam', name='Ixbalam', species='Jaguar', rarity=4, role='Ofensiva', p='sol', s='saturno',
         sign='Leo', asc='El León de Obsidiana', ritual='Muerde sal negra de las Salinas de Obsidiana', mix='Oro Negro', selfname='Frenesí del Jaguar',
         moves=['Garra Solar', 'Sol de Obsidiana', 'Acecho', 'Zarpa Ardiente', 'Mediodía Negro', 'Salto del Jaguar', 'Sol Devorado', 'El Jaguar del Eclipse'],
         selfform='frenesiSalNegra',
         tweak=lambda k: [k['moves'][m]['effects'].append(eff('dot', dot='burn', value=0.3, turns=2, name='Quemadura')) for m in ('spell', 'closing')]),
    dict(id='estrige', name='Estrige', species='Lechuza (kemonomimi)', rarity=4, role='Tejedora', p='luna', s='mercurio',
         sign='Piscis', asc='Los Peces de Medianoche', ritual='Deja que la sal se le derrita en la lengua a medianoche', mix='Plata Viva', selfname='Hilos de Medianoche',
         moves=['Pico de Plata', 'Vigilia', 'Ojo Abierto', 'Pluma Silenciosa', 'Búho Real', 'Noche Larga', 'Medianoche Compartida', 'El Ojo que No Duerme']),
    dict(id='salitre', name='Salitre', species='Cabra', rarity=3, role='Ofensiva contra jefes', p='venus', s='saturno',
         sign='Capricornio', asc='La Cabra Intoxicada', ritual='Lame un bloque de sal hasta enfermarse', mix='Cobre Viejo',
         moves=['Topetazo', 'Explosión de Veneno', 'Lamida', 'Cuerno Verde', 'Vómito de Sal', 'Cabra Brava', 'Intoxicación Total', 'La Cabra que No Muere'],
         passive=dict(name='Primera Pizca', description='Empieza intoxicada. Su Hechizo convierte la intoxicación en una explosión que escala con la vida máxima del enemigo.', startPoison=0.3)),
    dict(id='axa', name='Axa', species='Ajolote', rarity=3, role='Sanadora', p='luna', s='venus',
         sign='Cáncer', asc='El Cangrejo de Lodo', ritual='Se sumerge en agua salada y sale sonriendo', mix='Leche de Cobre',
         moves=['Coletazo Blando', 'Agua Tibia', 'Branquias', 'Charco', 'Regeneración', 'Lodo Sanador', 'Volver del Agua', 'El Ajolote Eterno'],
         passive=dict(name='Volver del Agua', description='Al caer, revive una vez con 30 % de vida y la Salinidad en cero.', reviveOnce=0.30)),
    dict(id='migaja', name='Migaja', species='Ratona', rarity=3, role='Tejedora', p='mercurio', s='luna',
         sign='Géminis', asc='Las Gemelas Ratonas', ritual='Guarda granitos de sal en los bolsillos', mix='Plata Viva',
         moves=['Mordisquito', 'Recado Corto', 'Corre que Corre', 'Migaja a Migaja', 'Despensa', 'Atajo', 'Ratonera', 'Las Mil Migajas'],
         tweak=lambda k: k['moves']['spell']['effects'][1].update(value=0.20)),
    dict(id='vellon', name='Vellón', species='Oveja (kemonomimi)', rarity=3, role='Sanadora', p='venus', s='luna',
         sign='Tauro', asc='El Toro de Lana', ritual='Lame sal de la palma, como el ganado', mix='Leche de Cobre',
         moves=['Topecito', 'Lana Tibia', 'Balido', 'Esquila', 'Manta', 'Rebaño', 'Abrigo de Lana', 'La Nube de Lana'],
         tweak=lambda k: k['moves']['spell']['effects'].append(eff('shield', value=0.10, turns=2, name='Lana'))),
    dict(id='pua', name='Púa', species='Eriza', rarity=3, role='Rompedora', p='marte', s='sol',
         sign='Aries', asc='El Carnero de Púas', ritual='Se revuelca en sal hasta que se le pega a las púas', mix='Brasa Roja',
         moves=['Pinchazo', 'Bola de Púas', 'Erizarse', 'Púa Suelta', 'Cerca de Espinas', 'Rodar', 'Espinas al Rojo', 'La Eriza de Mil Púas'],
         tweak=lambda k: k['moves']['closing'].update(toughness=50)),
    dict(id='charca', name='Charca', species='Pata', rarity=3, role='Guardiana', p='luna', s='jupiter',
         sign='Cáncer', asc='El Cangrejo de Charco', ritual='Chapotea en salmuera antes de pelear', mix='Marea Tonante',
         moves=['Picotazo', 'Ala Abierta', 'Cuac', 'Charco Hondo', 'Chapuzón', 'Plumas Mojadas', 'La Charca Entera', 'El Estanque Sin Fondo']),
]

# Regencia (habilidad de líder, GDD 9.1) por papel, al valor de una 5★. Las 4★ dan 70 % y las 3★ 50 %. // TUNING
REGENCIA = {
    'Guardiana': ('teamHp', 0.15, '+{v:.0%} de vida máxima a todo el aquelarre.'),
    'Sanadora': ('startResonance', 30, 'El aquelarre empieza con {v:.0f} de Resonancia.'),
    'Rompedora': ('teamAtk', 0.10, '+{v:.0%} de ataque a todo el aquelarre.'),
    'Saboteadora': ('startEnemyDelay', 0.15, 'Los enemigos empiezan el combate retrasados {v:.0%}.'),
    'Ofensiva': ('teamAtk', 0.12, '+{v:.0%} de ataque a todo el aquelarre.'),
    'Ofensiva con drenaje': ('teamHp', 0.10, '+{v:.0%} de vida máxima a todo el aquelarre.'),
    'Tejedora': ('teamSpdFlat', 8, '+{v:.0f} de Velocidad a todo el aquelarre.'),
    'Ofensiva contra jefes': ('startResonance', 20, 'El aquelarre empieza con {v:.0f} de Resonancia.'),
}
REGENCIA_NAMES = {
    'brea': 'Fiel de la Balanza', 'grajilla': 'Nido Ladrón', 'madrona': 'Cueva de Invierno', 'risca': 'Risa de Jauría',
    'nacarina': 'Corriente Tibia', 'centella': 'Cielo Cargado', 'vespera': 'Hora del Ocaso', 'cempa': 'Trato de Coyota',
    'ixbalam': 'Sol de Obsidiana', 'estrige': 'Vigilia Larga', 'salitre': 'Pizca Compartida', 'axa': 'Agua que Vuelve',
    'migaja': 'Despensa Común', 'vellon': 'Rebaño Tibio', 'pua': 'Cerca de Púas', 'charca': 'Charco Hondo',
}
REGENCIA_RARITY = {5: 1.0, 4: 0.7, 3: 0.5}

MOVE_KEYS = ['basic', 'spell', 'opening', 'link', 'closing', 'ascension1', 'ascension2', 'ascension3']

def slug(s):
    t = s.lower()
    for a, b in (('á', 'a'), ('é', 'e'), ('í', 'i'), ('ó', 'o'), ('ú', 'u'), ('ñ', 'n'), ('ü', 'u')):
        t = t.replace(a, b)
    return '_'.join(''.join(c if c.isalnum() else ' ' for c in t).split())

def move(name, wid, mult, tough, hits, effects, **extra):
    m = {'id': wid + '_' + slug(name), 'name': name}
    if mult: m['multiplier'] = mult
    if tough: m['toughness'] = tough
    if hits: m['hits'] = copy.deepcopy(hits)
    m['effects'] = copy.deepcopy(effects or [])
    m.update(extra)
    return m

def build(w):
    role = ROLES[w['role']]
    k = w['rarity']
    sc = RARITY_SCALE[k]
    hp, atk, spd = role['stats']
    kit = {
        'id': w['id'], 'name': w['name'], 'shortName': w['name'], 'species': w['species'], 'rarity': k, 'role': w['role'],
        'ascendant': {'sign': w['sign'], 'name': w['asc']}, 'ritual': w['ritual'],
        'mix': {'primary': w['p'], 'secondary': w['s'], 'name': w['mix']}, 'via': 'mayor', 'grade': 'B',
        'stats': {'level': 50, 'hp': int(round(hp * sc / 50.0) * 50), 'atk': int(round(atk * sc / 10.0) * 10),
                  'spd': spd + RARITY_SPD[k], 'critRate': 0.05, 'critDmg': 0.5},
    }
    if 'passive' in w: kit['passive'] = w['passive']
    form = w.get('selfform', role['self']) if k >= 4 else 'none'
    if k >= 4 and form == 'none': form = 'hilos'
    kit['selfCast'] = {'form': form} if form == 'none' else {'form': form, 'name': w['selfname']}
    n = w['moves']
    moves = {}
    b = role['basic']
    moves['basic'] = move(n[0], w['id'], b[0], b[1], b[2], b[3], byAscendant=False)
    sp = role['spell']
    moves['spell'] = move(n[1], w['id'], sp.get('mult', 0), sp.get('tough', 0), sp.get('hits'), sp['effects'])
    if 'target' in sp: moves['spell']['target'] = sp['target']
    if role['spell_desc']: moves['spell']['description'] = role['spell_desc']
    for i, key in enumerate(['opening', 'link', 'closing']):
        m = role[key]
        moves[key] = move(n[2 + i], w['id'], m[0], m[1], m[2], m[3])
    for i in range(3):
        a = role['asc'][i]
        moves['ascension%d' % (i + 1)] = move(n[5 + i], w['id'], a[0], a[1], a[2], a[3])
    if w['role'] in ('Tejedora',):
        moves['ascension2']['target'] = 'allEnemies'
        moves['ascension3']['target'] = 'allEnemies'
    kit['moves'] = moves
    if 'regencia' in w: kit['regencia'] = w['regencia']
    else:
        key, val, desc = REGENCIA[w['role']]
        v = val * REGENCIA_RARITY[k]
        v = int(round(v)) if key in ('startResonance', 'teamSpdFlat') else round(v, 3)
        kit['regencia'] = {'name': REGENCIA_NAMES[w['id']], 'description': desc.format(v=v).replace('%', ' %'), key: v}
    if 'tweak' in w: w['tweak'](kit)
    for m in kit['moves'].values():
        if not m['effects']: del m['effects']
    return kit

def main():
    kits = [build(w) for w in W]
    # Validaciones de las reglas del cargador, para fallar aquí y no en Unity.
    for kt in kits:
        for key in ('opening', 'link', 'closing'):
            last = max(h[0] if isinstance(h, list) else h for h in kt['moves'][key]['hits'])
            assert last <= 108, (kt['id'], key, last)
        assert len(set(m['id'] for m in kt['moves'].values())) == 8, kt['id']
    header = ('// GENERADO por Tools/Content/gen_kits.py: no lo edites a mano, cambia el generador.\n'
              '// Kits de las 18 brujas que faltaban (GDD 17 y 18). Se suman a witches.json en MetaLoader.\n'
              '// Plantilla por papel; estadísticas ×0.9 para 4★ y ×0.8 para 3★. Todo número es // TUNING.\n')
    body = json.dumps({'witches': kits}, ensure_ascii=False, indent=1)
    with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
        f.write(header + body + '\n')
    print('kits:', len(kits), '->', OUT)

if __name__ == '__main__':
    main()
