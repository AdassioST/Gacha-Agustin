"""Pinta por código el kit visual de la interfaz de Sal y Almas (Libro Mayor + Termas).

Uso:  python scripts/ui_kit.py [grupo ...]     grupos: bg chrome icons fx logo cards witches all
Salida: Assets/SalYAlmas/Art/Ui/Resources/Ui/<grupo>/*.png
Todo es reproducible: cada imagen usa una semilla fija. No hay imágenes de terceros.
Paleta: docs/REFERENCIAS.md. Antirreferencias: nada de bisel dorado genérico, la sal es cúbica y no nieve.
"""
import math
import os
import shutil
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont
from scipy import ndimage as ndi

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "Assets", "SalYAlmas", "Art", "Ui", "Resources", "Ui")
FONTS = os.path.join(ROOT, "Assets", "SalYAlmas", "Art", "Ui", "Resources", "Ui", "Fonts")
RECORTES = os.environ.get("RECORTES", os.path.join(ROOT, "art", "recortes"))


def hx(s):
    s = s.lstrip("#")
    return np.array([int(s[i:i + 2], 16) / 255.0 for i in (0, 2, 4)])


PARCH, PARCH_S, INK = hx("EDE3CC"), hx("CFBE9A"), hx("2A2018")
ABYSS, PANEL = hx("0B0B14"), hx("1A1A28")
GOLD, GOLD_B, CRIM = hx("B8923A"), hx("E6C76E"), hx("8E2430")
SALT, SPARK, FIVE = hx("F6F4EE"), hx("FFF1B8"), hx("FFD84D")
VIOLET = hx("A25CF5")


def save(img, group, name):
    d = os.path.join(OUT, group)
    os.makedirs(d, exist_ok=True)
    if isinstance(img, np.ndarray):
        img = to_img(img)
    img.save(os.path.join(d, name + ".png"), optimize=True)
    print(f"  {group}/{name} {img.size}")


def to_img(a):
    a = np.clip(a, 0, 1)
    return Image.fromarray((a * 255 + 0.5).astype(np.uint8), "RGBA" if a.shape[2] == 4 else "RGB")


def noise(h, w, scale, seed, octaves=5, persistence=0.5):
    """fBm suave normalizado a 0..1."""
    rng = np.random.default_rng(seed)
    out = np.zeros((h, w))
    amp, total = 1.0, 0.0
    s = float(scale)
    for _ in range(octaves):
        gh, gw = max(2, int(h / s) + 2), max(2, int(w / s) + 2)
        g = rng.random((gh, gw))
        z = ndi.zoom(g, (h / (gh - 1) + 0.01, w / (gw - 1) + 0.01), order=3)[:h, :w]
        out += z * amp
        total += amp
        amp *= persistence
        s = max(2.0, s / 2)
    out /= total
    lo, hi = out.min(), out.max()
    return (out - lo) / (hi - lo + 1e-9)


def fit(a, h, w):
    """Reescala un mapa 2D a exactamente h x w."""
    return np.asarray(Image.fromarray(a.astype(np.float32), "F").resize((w, h), Image.BICUBIC)).astype(float)


def smooth(a, b, x):
    t = np.clip((x - a) / (b - a + 1e-9), 0, 1)
    return t * t * (3 - 2 * t)


def lerp(a, b, t):
    t = np.asarray(t)
    if t.ndim == 2:
        t = t[..., None]
    return a * (1 - t) + b * t


def grad(h, w, stops):
    """Degradado vertical. stops = [(0..1, color), ...]"""
    ys = np.linspace(0, 1, h)
    xs = [s[0] for s in stops]
    col = np.stack([np.interp(ys, xs, [s[1][i] for s in stops]) for i in range(3)], -1)
    return np.repeat(col[:, None, :], w, 1)


def font(name, size):
    return ImageFont.truetype(os.path.join(FONTS, name), size)


class SS:
    """Lienzo con superesmuestreo: se dibuja a 4x y se reduce."""

    def __init__(self, w, h, k=4):
        self.w, self.h, self.k = w, h, k
        self.im = Image.new("RGBA", (w * k, h * k), (0, 0, 0, 0))
        self.d = ImageDraw.Draw(self.im, "RGBA")

    def p(self, pts):
        return [(x * self.k, y * self.k) for x, y in pts]

    def line(self, pts, c, w=1.0):
        self.d.line(self.p(pts), fill=c, width=max(1, int(w * self.k)), joint="curve")

    def poly(self, pts, fill=None, outline=None, w=1.0):
        pp = self.p(pts)
        if fill is not None:
            self.d.polygon(pp, fill=fill)
        if outline is not None:
            self.d.line(pp + [pp[0]], fill=outline, width=max(1, int(w * self.k)), joint="curve")

    def circle(self, x, y, r, fill=None, outline=None, w=1.0):
        k = self.k
        self.d.ellipse([(x - r) * k, (y - r) * k, (x + r) * k, (y + r) * k], fill=fill, outline=outline, width=max(1, int(w * k)))

    def arc(self, x, y, r, a0, a1, c, w=1.0):
        k = self.k
        self.d.arc([(x - r) * k, (y - r) * k, (x + r) * k, (y + r) * k], a0, a1, fill=c, width=max(1, int(w * k)))

    def text(self, x, y, s, f, c, anchor="mm"):
        fk = ImageFont.truetype(f.path, f.size * self.k)
        self.d.text((x * self.k, y * self.k), s, font=fk, fill=c, anchor=anchor)

    def done(self):
        return self.im.resize((self.w, self.h), Image.LANCZOS)


def rgba(c, a=1.0):
    return tuple(int(v * 255) for v in c) + (int(a * 255),)


# ---------------------------------------------------------------------------------------------- Fondos


def sky(W, H):
    g = grad(H, W, [(0, hx("1d1830")), (0.26, hx("3b2748")), (0.48, hx("7e4f66")), (0.62, hx("cc8579")), (0.70, hx("f6bd94")), (1, hx("f6bd94"))])
    ys = np.linspace(0, 1, H)
    # nubes estiradas, suaves, con vientre oscuro y filo encendido por el sol bajo
    n = fit(noise(H // 2, W // 8, 70, 11, 6), H, W)
    n2 = fit(noise(H // 2, W // 3, 60, 12, 5), H, W)
    band = (smooth(0.16, 0.36, ys) * (1 - smooth(0.56, 0.70, ys)))[:, None]
    cloud = ndi.gaussian_filter(smooth(0.50, 0.74, n * 0.7 + n2 * 0.3), 3) * band
    soft = ndi.gaussian_filter(cloud, 3)
    lit = np.clip(soft - np.roll(soft, -12, axis=0), 0, 1) * 5
    g = lerp(g, hx("4a2c48"), cloud * 0.62)
    g = lerp(g, hx("f7ae8e"), np.clip(lit, 0, 1) * 0.55)
    rng = np.random.default_rng(13)
    stars = np.zeros((H, W))
    ys_ = (rng.random(1500) ** 2.2 * H * 0.42).astype(int)
    xs = rng.integers(0, W, 1500)
    stars[ys_, xs] = rng.random(1500) ** 4 * 2.0
    stars = ndi.gaussian_filter(stars, 0.8) * 3 + ndi.gaussian_filter(stars, 2.4) * 2.2
    g = g + stars[..., None] * hx("ffe9c4") * (1 - cloud[..., None])
    yy, xx = np.mgrid[0:H, 0:W]
    glow = np.exp(-(((xx - W * 0.66) / (W * 0.30)) ** 2 + ((yy - H * 0.71) / (H * 0.13)) ** 2))
    g = g + glow[..., None] * hx("ffb078") * 0.38
    return np.clip(g, 0, 1)


def constellations(W, H):
    c = SS(W, H, 2)
    rng = np.random.default_rng(21)
    for _ in range(9):
        x, y = rng.random() * W, rng.random() * H * 0.34
        pts = [(x, y)]
        for _ in range(rng.integers(4, 8)):
            a = rng.random() * math.tau
            r = 40 + rng.random() * 90
            pts.append((pts[-1][0] + math.cos(a) * r, pts[-1][1] + math.sin(a) * r * 0.7))
        c.line(pts, rgba(GOLD_B, 0.34), 1.2)
        for p in pts:
            c.circle(p[0], p[1], 2 + rng.random() * 2.4, fill=rgba(SPARK, 0.95))
    return c.done()


HOR = 0.71


def cellular(u, v, seed):
    """Voronoi en coordenadas de mundo (aristas rectas: costra de sal cúbica). Devuelve (F2-F1, id de celda)."""
    iu, iv = np.floor(u).astype(np.int64), np.floor(v).astype(np.int64)
    f1 = np.full(u.shape, 9.0)
    f2 = np.full(u.shape, 9.0)
    cid = np.zeros(u.shape)

    def h(a, b, k):
        x = (a * 374761393 + b * 668265263 + k * 1274126177 + seed * 15485863) & 0xFFFFFFFF
        x = ((x ^ (x >> 13)) * 1274126177) & 0xFFFFFFFF
        return ((x ^ (x >> 16)) & 0xFFFF) / 65535.0

    for dv in (-1, 0, 1):
        for du in (-1, 0, 1):
            cu, cv = iu + du, iv + dv
            px, py = cu + h(cu, cv, 1), cv + h(cu, cv, 2)
            d = np.hypot(px - u, py - v)
            better = d < f1
            f2 = np.where(better, f1, np.minimum(f2, d))
            cid = np.where(better, h(cu, cv, 3), cid)
            f1 = np.where(better, d, f1)
    return f2 - f1, cid


def persp(W, H, hor_y, c=26.0):
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    dy = np.maximum(yy - hor_y, 0.0)
    z = 1000.0 / (dy + c)
    u = (xx - W / 2) * z / 1000.0
    return u, z, yy, xx


def far_ruins(W, H):
    """Sierras en bruma y el viaducto en ruinas de los retratos, con arcos de verdad."""
    hor = int(H * HOR)
    prof = noise(1, W, 340, 31, 5)[0]
    top = hor - 26 - prof * 120
    yy = np.arange(H)[:, None]
    a = np.zeros((H, W, 4))
    a[..., :3] = lerp(hx("7c5266"), hx("d58f80"), smooth(hor - 150, hor, np.repeat(yy, W, 1).astype(float)))
    a[..., 3] = smooth(-1.5, 1.5, yy - top[None, :]) * (yy[:, 0] < hor + 4)[:, None]
    im = to_img(a)
    k = 2
    wall = Image.new("L", (W * k, H * k), 0)
    d = ImageDraw.Draw(wall)
    rng = np.random.default_rng(32)
    for x0, x1, topy, ah, aw in [(0.02, 0.27, hor - 104, 74, 62), (0.72, 1.0, hor - 124, 90, 74)]:
        x = W * x0
        while x < W * x1:
            brk = rng.random()
            h_ = topy + (40 + rng.random() * 50 if brk < 0.22 else 0)
            d.rectangle([x * k, h_ * k, (x + 20) * k, (hor + 4) * k], fill=255)
            if brk >= 0.22:
                span = aw
                d.rectangle([x * k, topy * k, (x + 20 + span) * k, (topy + ah * 0.55) * k], fill=255)
                d.ellipse([(x + 20) * k, (topy + ah * 0.34) * k, (x + 20 + span) * k, (topy + ah * 0.34 + span) * k], fill=0)
            x += 20 + aw
    wall = wall.resize((W, H), Image.LANCZOS).filter(ImageFilter.GaussianBlur(0.8))
    v = np.zeros((H, W, 4))
    v[..., :3] = hx("5b3f56")
    v[..., 3] = np.asarray(wall) / 255.0 * 0.92
    im.alpha_composite(to_img(v))
    return im


def salt_water(W, H):
    """Salina al atardecer en perspectiva: costra blanca-rosa y agua carmesí que refleja el cielo."""
    hor = H * HOR
    u, z, yy, xx = persp(W, H, hor)
    t = np.clip((yy - hor) / (H - hor), 0, 1)
    sk = grad(int(hor) + 1, 1, [(0, hx("1d1830")), (0.26, hx("3b2748")), (0.48, hx("7e4f66")), (0.62, hx("cc8579")), (0.70, hx("f6bd94")), (1, hx("f6bd94"))])[:, 0]
    # el reflejo toma la fila del cielo a la misma distancia del horizonte
    refl = sk[np.clip((hor - (yy - hor) * 0.9).astype(int), 0, int(hor))]
    water = lerp(refl, hx("6e1a26"), 0.34 + 0.34 * t)
    ripple = noise(H, W // 2, 8, 44, 2)
    water = water + (fit(ripple, H, W) - 0.5)[..., None] * 0.07 * (0.3 + t[..., None])
    gap, cid = cellular(u * 2.6, z * 0.62, 41)
    crack = smooth(0.0, 0.11, gap)
    big = fit(noise(H // 2, W // 2, 110, 42, 5), H, W)
    world = noise(H // 2, W // 2, 60, 45, 5)
    pools = smooth(0.46, 0.54, big * 0.55 + fit(world, H, W) * 0.45)
    crust = lerp(hx("e9cfca"), hx("fbf2ea"), cid * 0.7 + 0.15)
    crust = crust * (0.62 + 0.38 * crack[..., None])
    crust = lerp(crust, hx("f5b89a"), (1 - smooth(0, 0.3, t)) * 0.62)
    rgb = lerp(crust, water, pools)
    rgb = lerp(rgb, hx("f6bd94"), (1 - smooth(0, 0.14, t)) * 0.7)
    a = smooth(hor - 2, hor + 2, yy)
    return np.dstack([np.clip(rgb, 0, 1), a])


def shell_terrace(W, H):
    """Terraza del caparazón de la Tortuga en perspectiva, con escudos hexagonales, y la poza de agua termal."""
    hor = H * HOR
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    edge = H * 0.815 + 34 * np.sin(xx / W * math.pi * 1.1 + 0.3) + (noise(1, W, 70, 51, 4)[0] - 0.5)[None, :] * 14
    z = 1000.0 / (yy - hor + 60)
    u = (xx - W * 0.42) * z / 1000.0
    v = z * 1.5
    s = 0.42
    q = (u * 0.57735 - v * 0.33333) / s
    r_ = (v * 0.66667) / s
    y_ = -q - r_
    rq, rr, ry = np.round(q), np.round(r_), np.round(y_)
    dq, dr, dy = np.abs(rq - q), np.abs(rr - r_), np.abs(ry - y_)
    fix_q = (dq > dr) & (dq > dy)
    fix_r = (~fix_q) & (dr > dy)
    rq = np.where(fix_q, -rr - ry, rq)
    rr = np.where(fix_r, -rq - ry, rr)
    fq, fr = q - rq, r_ - rr
    hexd = np.maximum.reduce([np.abs(fq), np.abs(fr), np.abs(fq + fr)])
    seam = smooth(0.40, 0.49, hexd)
    idv = (np.sin(rq * 12.9 + rr * 78.2) * 43758.5) % 1.0
    # roca real (Rock030, CC0) proyectada en perspectiva para que no se estire
    rock = cc0("Rock030", "Color", 1024).mean(axis=2)
    rock_n = cc0("Rock030", "NormalGL", 1024)
    uu = (u * 900.0).astype(np.int64) % 1024
    vv = (v * 340.0).astype(np.int64) % 1024
    grain = np.clip((rock[vv, uu] - rock.mean()) * 2.2 + 0.55, 0, 1)[..., None]
    stone = lerp(hx("4a3a42"), hx("86685a"), idv * 0.5 + 0.25)
    stone = stone * (0.52 + 0.85 * grain)
    nmap = rock_n[vv, uu]
    stone = stone * (0.82 + 0.5 * np.clip((nmap[..., 1] - nmap[..., 0]) * 1.3 + 0.2, 0, 1))[..., None]
    dome = np.clip(1 - hexd * 2, 0, 1) ** 0.8
    stone = stone * (0.66 + 0.52 * dome[..., None])
    stone = stone + (np.clip(fq * 0.8 - fr, -1, 1) * 0.5 + 0.5)[..., None] * hx("ffb48a") * 0.09
    stone = stone * (1 - 0.68 * seam[..., None])
    crust = smooth(0.54, 0.70, noise(H, W, 34, 53, 5)) * seam
    gap, cid = cellular(xx / 11.0, yy / 9.0, 55)
    cubes = lerp(hx("c8b1ac"), hx("fff6ee"), cid)
    stone = lerp(stone, cubes, crust * smooth(0.0, 0.1, gap))
    px, py, prx, pry = W * 0.50, H * 1.02, W * 0.42, H * 0.135
    pd = np.sqrt(((xx - px) / prx) ** 2 + ((yy - py) / pry) ** 2)
    pool = 1 - smooth(0.975, 0.99, pd)
    rim = (1 - smooth(0.99, 1.075, pd)) * smooth(0.97, 0.99, pd)
    rimlit = np.clip((py - yy) / (pry * 1.1), 0, 1)
    wt = np.clip(pd, 0, 1)
    water = lerp(hx("ffcdb0"), hx("b45a5e"), smooth(0.2, 0.98, wt))
    ond = np.sin((pd * 54) + (noise(H, W, 70, 56, 3) * 8))
    water = water + (ond * 0.5 + 0.5)[..., None] * hx("fff0dc") * 0.035
    rimcol = lerp(hx("a37e6c"), hx("f1dcc6"), rimlit)
    rgb = lerp(stone, rimcol, rim)
    rgb = lerp(rgb, water, pool)
    rgb = rgb * (0.7 + 0.3 * smooth(0, 46, yy - edge))[..., None]
    # calor de la poza sobre la roca
    warm = np.exp(-((pd - 1.0) / 0.55) ** 2) * (1 - pool)
    rgb = rgb + warm[..., None] * hx("ff9a70") * 0.11
    return np.dstack([np.clip(rgb, 0, 1), smooth(-1.6, 1.6, yy - edge)])


def make_bg():
    W, H = 2560, 1440
    s = sky(W, H)
    save(s, "bg", "home_sky")
    save(constellations(W, H), "bg", "home_stars")
    save(far_ruins(W, H), "bg", "home_far")
    mid = salt_water(W, H)
    save(mid, "bg", "home_mid")
    save(shell_terrace(W, H), "bg", "home_near")
    # Fondo oscuro de pantallas de contenido: abisal con pergamino apenas sugerido y carta natal al fondo.
    g = grad(H // 2, W // 2, [(0, hx("15111f")), (0.6, hx("1a1422")), (1, hx("0b0b14"))])
    n = noise(H // 2, W // 2, 300, 61, 6)
    g = lerp(g, hx("3a2036"), smooth(0.5, 0.85, n) * 0.5)
    yy, xx = np.mgrid[0:H // 2, 0:W // 2]
    v = ((xx / (W / 2) - 0.5) ** 2 + (yy / (H / 2) - 0.5) ** 2)
    g = g * (1 - smooth(0.1, 0.55, v)[..., None] * 0.55)
    save(np.clip(g, 0, 1), "bg", "abyss")
    # Agua del Manantial, vista desde arriba: rosa profunda con ondas.
    W2, H2 = 1600, 900
    yy, xx = np.mgrid[0:H2, 0:W2].astype(float)
    r = np.hypot((xx - W2 / 2) / W2 * 1.6, (yy - H2 * 0.55) / H2)
    base = lerp(hx("c7705f"), hx("1a0c1c"), smooth(0.05, 0.75, r))
    n = noise(H2, W2, 90, 71, 5)
    caust = np.abs(n - 0.5) * 2
    caust = (1 - smooth(0.0, 0.1, caust)) * (1 - smooth(0.2, 0.9, r))
    base = base + caust[..., None] * hx("ffd9b8") * 0.28
    save(np.clip(base, 0, 1), "bg", "spring")


# ---------------------------------------------------------------------------------------------- Astrolabio


def astrolabe(size, rings=3, glyphs=True, seed=3):
    c = SS(size, size, 2)
    cx = cy = size / 2
    R = size / 2 - 6
    gold = rgba(GOLD_B, 0.95)
    thin = rgba(GOLD_B, 0.55)
    c.circle(cx, cy, R, outline=gold, w=3)
    c.circle(cx, cy, R - 10, outline=thin, w=1.4)
    for i in range(360):
        a = math.radians(i)
        l = 16 if i % 30 == 0 else 8 if i % 5 == 0 else 4
        r0, r1 = R - 10, R - 10 - l
        c.line([(cx + math.cos(a) * r0, cy + math.sin(a) * r0), (cx + math.cos(a) * r1, cy + math.sin(a) * r1)], gold if i % 30 == 0 else thin, 1.4 if i % 5 == 0 else 0.9)
    r2 = R * 0.80
    c.circle(cx, cy, r2, outline=gold, w=2)
    if glyphs:
        f = font("NotoSansSymbols-Regular.ttf", int(size * 0.052))
        for i, g in enumerate("♈♉♊♋♌♍♎♏♐♑♒♓"):
            a = math.radians(i * 30 + 15)
            rr = (R - 26 + r2) / 2
            c.text(cx + math.cos(a) * rr, cy + math.sin(a) * rr, g, f, gold)
        for i in range(12):
            a = math.radians(i * 30)
            c.line([(cx + math.cos(a) * r2, cy + math.sin(a) * r2), (cx + math.cos(a) * (R - 26), cy + math.sin(a) * (R - 26))], thin, 1.1)
    r3 = R * 0.56
    c.circle(cx, cy, r3, outline=thin, w=1.6)
    if rings >= 3:
        for i in range(12):
            a = math.radians(i * 30)
            c.line([(cx, cy), (cx + math.cos(a) * r2, cy + math.sin(a) * r2)], rgba(GOLD_B, 0.25), 1)
        rng = np.random.default_rng(seed)
        pts = [(cx + math.cos(a) * r3 * 0.92, cy + math.sin(a) * r3 * 0.92) for a in rng.random(7) * math.tau]
        for i in range(7):
            for j in range(i + 1, 7):
                if rng.random() < 0.3:
                    c.line([pts[i], pts[j]], rgba(GOLD_B, 0.35), 1.1)
    c.circle(cx, cy, 8, fill=gold)
    return c.done()


def ring_only(size, dots=72):
    c = SS(size, size, 2)
    cx = cy = size / 2
    R = size / 2 - 8
    c.circle(cx, cy, R, outline=rgba(GOLD_B, 0.9), w=2)
    for i in range(dots):
        a = i / dots * math.tau
        c.circle(cx + math.cos(a) * (R - 12), cy + math.sin(a) * (R - 12), 2.4 if i % 6 == 0 else 1.2, fill=rgba(GOLD_B, 0.9))
    return c.done()


# ---------------------------------------------------------------------------------------------- Marcos


def corner_flourish(c, x, y, sx, sy, s, col, col2):
    """Esquina del Libro Mayor: cuarto de astrolabio con rombo; nada de bisel."""
    c.arc(x, y, s, 0 if sx > 0 and sy > 0 else 90 if sx < 0 and sy > 0 else 180 if sx < 0 else 270, (0 if sx > 0 and sy > 0 else 90 if sx < 0 and sy > 0 else 180 if sx < 0 else 270) + 90, col, 1.6)
    c.arc(x, y, s * 0.68, 0 if sx > 0 and sy > 0 else 90 if sx < 0 and sy > 0 else 180 if sx < 0 else 270, (0 if sx > 0 and sy > 0 else 90 if sx < 0 and sy > 0 else 180 if sx < 0 else 270) + 90, col2, 1.1)
    d = s * 0.24
    cx, cy = x + sx * s * 0.34, y + sy * s * 0.34
    c.poly([(cx - d, cy), (cx, cy - d), (cx + d, cy), (cx, cy + d)], fill=col)
    c.circle(x + sx * s * 0.86, y, 2.2, fill=col)
    c.circle(x, y + sy * s * 0.86, 2.2, fill=col)


# ---------------------------------------------------------------------------------------------- Materiales CC0 y relieve

CC0 = os.path.join(ROOT, "art", "ui", "cc0")


def cc0(asset, kind, size):
    """Lee un mapa de ambientCG (CC0) como matriz 0..1. kind: Color, NormalGL, Roughness, Displacement."""
    path = os.path.join(CC0, asset, f"{asset}_1K-JPG_{kind}.jpg")
    im = Image.open(path).convert("RGB" if kind in ("Color", "NormalGL") else "L")
    if im.size != (size, size):
        im = im.resize((size, size), Image.LANCZOS)
    return np.asarray(im) / 255.0


def tile_to(a, h, w):
    """Repite un mapa cuadrado hasta llenar h x w, sin estirarlo."""
    ry, rx = h // a.shape[0] + 1, w // a.shape[1] + 1
    return np.tile(a, (ry, rx) + ((1,) if a.ndim == 3 else ()))[:h, :w]


def shade(height, albedo, tex_normal=None, tex_strength=0.6, relief=1.0, spec=0.35, gloss=28.0, ambient=0.52, key=0.78):
    """Ilumina una altura: luz cálida arriba a la izquierda y contraluz rosa abajo a la derecha. Aquí nace todo el relieve."""
    gy, gx = np.gradient(height)
    nx, ny = -gx * relief, -gy * relief
    if tex_normal is not None:
        nx = nx + (tex_normal[..., 0] - 0.5) * 2 * tex_strength
        ny = ny - (tex_normal[..., 1] - 0.5) * 2 * tex_strength
    nz = np.ones_like(nx)
    ln = np.sqrt(nx * nx + ny * ny + nz * nz)
    nx, ny, nz = nx / ln, ny / ln, nz / ln
    L = np.array([-0.46, -0.62, 0.64])
    L2 = np.array([0.6, 0.55, 0.45])
    H = (L + np.array([0, 0, 1.0]))
    H = H / np.linalg.norm(H)
    d1 = np.clip(nx * L[0] + ny * L[1] + nz * L[2], 0, 1)
    d2 = np.clip(nx * L2[0] + ny * L2[1] + nz * L2[2], 0, 1)
    s = np.clip(nx * H[0] + ny * H[1] + nz * H[2], 0, 1) ** gloss
    lit = ambient + key * d1
    out = albedo * lit[..., None] + albedo * d2[..., None] * hx("ff9d80") * 0.14
    return out + s[..., None] * spec


def mask_of(draw, w, h, k=3):
    """Máscara 0..1 a partir de un dibujo vectorial, con superesmuestreo."""
    c = SS(w, h, k)
    draw(c)
    return np.asarray(c.done())[..., 3] / 255.0


def bevel(mask, r=2.2):
    """Altura redondeada a partir de una máscara: la línea se vuelve metal repujado."""
    a = ndi.gaussian_filter(mask, r)
    b = ndi.gaussian_filter(mask, r * 0.45)
    return a * 0.65 + b * 0.35


def metal(h, w, tint=(1.0, 0.82, 0.48), dark=0.5, seed=901):
    """Hoja de metal con vetas (Metal042A) y degradado vertical, teñible."""
    col = tile_to(cc0("Metal042A", "Color", 512), h, w)
    grey = col.mean(axis=2, keepdims=True)
    grey = (grey - grey.min()) / (grey.max() - grey.min() + 1e-9)
    ys = np.linspace(0, 1, h)[:, None, None]
    band = 0.78 + 0.32 * np.cos(ys * 6.2 + 0.4)
    base = np.array(tint)[None, None, :] * (dark + (1 - dark) * grey) * band
    return np.clip(base, 0, 1.4)


def compose_frame(w, h, draw, tint=(1.0, 0.82, 0.48), r=2.2, shadow=0.75, relief=26.0):
    """Marco de metal repujado con sombra propia. Interior transparente para el 9-slice."""
    m = mask_of(draw, w, h)
    he = bevel(m, r)
    nrm = tile_to(cc0("Metal042A", "NormalGL", 512), h, w)
    rgb = shade(he, metal(h, w, tint), nrm, 0.35, relief, 0.65, 22, 0.42, 0.92)
    sh = ndi.gaussian_filter(np.roll(np.roll(m, 3, 0), 2, 1), 3.5) * shadow
    a = np.clip(m + sh * (1 - m), 0, 1)
    rgb = lerp(np.zeros_like(rgb), rgb, m)
    premult_fix = np.where(a[..., None] > 0, rgb * m[..., None] / np.maximum(a[..., None], 1e-4), rgb)
    return np.dstack([np.clip(premult_fix, 0, 1), a])


def double_frame(size, inset=6, inner=18, line=4.0, thin=1.6, corner=True, fl=70):
    def draw(c):
        c.poly([(inset, inset), (size - inset, inset), (size - inset, size - inset), (inset, size - inset)], outline=(255, 255, 255, 255), w=line)
        c.poly([(inner, inner), (size - inner, inner), (size - inner, size - inner), (inner, size - inner)], outline=(255, 255, 255, 255), w=thin)
        if corner:
            W = (255, 255, 255, 255)
            for sx, sy, x, y in [(1, 1, inner, inner), (-1, 1, size - inner, inner), (1, -1, inner, size - inner), (-1, -1, size - inner, size - inner)]:
                corner_flourish2(c, x, y, sx, sy, fl, W)

    return draw


def corner_flourish2(c, x, y, sx, sy, s, W):
    """Esquina de astrolabio más rica: dos arcos, rombo con gema y dos remates."""
    a0 = 0 if sx > 0 and sy > 0 else 90 if sx < 0 and sy > 0 else 180 if sx < 0 else 270
    c.arc(x, y, s, a0, a0 + 90, W, 3.2)
    c.arc(x, y, s * 0.72, a0, a0 + 90, W, 1.8)
    d = s * 0.2
    cx, cy = x + sx * s * 0.36, y + sy * s * 0.36
    c.poly([(cx - d, cy), (cx, cy - d), (cx + d, cy), (cx, cy + d)], fill=W)
    for t in (1.0, 1.26):
        c.circle(x + sx * s * t, y, 3.2 - (t - 1) * 3, fill=W)
        c.circle(x, y + sy * s * t, 3.2 - (t - 1) * 3, fill=W)


def make_tiles():
    """Superficies que se repiten sin estirarse. El 9-slice de oro va encima."""
    S = 512
    # Cuero oscuro con tinte ciruela: el panel de todo el juego
    col = cc0("Leather030", "Color", S)
    nrm = cc0("Leather030", "NormalGL", S)
    grey = col.mean(axis=2, keepdims=True)
    grey = np.clip((grey - grey.mean()) * 2.6 + 0.5, 0, 1)
    alb = lerp(hx("120d18"), hx("3a2436"), grey)
    save(to_img(np.dstack([np.clip(shade(np.zeros((S, S)), alb, nrm, 0.75, 0, 0.09, 18, 0.62, 0.62), 0, 1), np.ones((S, S))])), "chrome", "tile_leather")
    # Cuero carmesí, botón principal
    col = cc0("Leather037", "Color", S)
    nrm = cc0("Leather037", "NormalGL", S)
    grey = col.mean(axis=2, keepdims=True)
    grey = np.clip((grey - grey.mean()) * 3 + 0.5, 0, 1)
    alb = lerp(hx("5e101d"), hx("b2303f"), grey)
    save(to_img(np.dstack([np.clip(shade(np.zeros((S, S)), alb, nrm, 0.9, 0, 0.14, 16, 0.62, 0.66), 0, 1), np.ones((S, S))])), "chrome", "tile_crimson")
    # Pergamino: color de Paper005, grano de Paper001
    col = cc0("Paper005", "Color", S)
    nrm = cc0("Paper001", "NormalGL", S)
    grey = col.mean(axis=2, keepdims=True)
    grey = np.clip((grey - grey.mean()) * 7 + 0.5, 0, 1)
    n = noise(S, S, 170, 95, 5)
    # el ruido debe tener bordes iguales para teselar: se mezcla con su versión desplazada
    ramp = np.linspace(0, 1, S)
    n = n * (1 - smooth(0.8, 1, ramp))[:, None] + np.roll(n, S // 2, 0) * smooth(0.8, 1, ramp)[:, None]
    n = n * (1 - smooth(0.8, 1, ramp))[None, :] + np.roll(n, S // 2, 1) * smooth(0.8, 1, ramp)[None, :]
    alb = lerp(hx("d9c6a0"), hx("f2e9d5"), grey * 0.5 + n[..., None] * 0.5)
    save(to_img(np.dstack([np.clip(shade(np.zeros((S, S)), alb, nrm, 1.1, 0, 0.03, 12, 0.72, 0.42), 0, 1), np.ones((S, S))])), "chrome", "tile_parchment")
    # Oro batido, para barras y sellos grandes
    save(to_img(np.dstack([np.clip(shade(np.zeros((S, S)), metal(S, S), cc0("Metal042A", "NormalGL", S), 0.8, 0, 0.5, 20), 0, 1), np.ones((S, S))])), "chrome", "tile_gold")


def make_chrome():
    make_tiles()
    # Todos estos sprites son @2x: el lienzo los pinta a la mitad, así se ven nítidos también en pantallas densas.
    gold = (1.0, 0.82, 0.48)
    silver = (0.86, 0.85, 0.82)
    amethyst = (0.78, 0.55, 1.0)
    save(to_img(compose_frame(420, 420, double_frame(420), gold)), "chrome", "frame_panel")
    save(to_img(compose_frame(320, 320, double_frame(320, 5, 15, 5.0, 2.0, True, 58), gold, 2.2)), "chrome", "frame_r5")
    save(to_img(compose_frame(320, 320, double_frame(320, 5, 15, 4.4, 1.8, True, 46), amethyst, 2.0)), "chrome", "frame_r4")
    save(to_img(compose_frame(320, 320, double_frame(320, 5, 14, 3.6, 1.4, False), silver, 1.8)), "chrome", "frame_r3")
    # Marco de botón: chaflán y filete interno
    def btn_draw(c, w=320, h=128, cut=20):
        W = (255, 255, 255, 255)
        pts = [(cut, 3), (w - cut, 3), (w - 3, cut), (w - 3, h - cut), (w - cut, h - 3), (cut, h - 3), (3, h - cut), (3, cut)]
        c.poly(pts, outline=W, w=4.5)
        i = 10
        p2 = [(cut + i * .6, 3 + i), (w - cut - i * .6, 3 + i), (w - 3 - i, cut + i * .6), (w - 3 - i, h - cut - i * .6), (w - cut - i * .6, h - 3 - i), (cut + i * .6, h - 3 - i), (3 + i, h - cut - i * .6), (3 + i, cut + i * .6)]
        c.poly(p2, outline=W, w=1.4)

    save(to_img(compose_frame(320, 128, btn_draw, gold, 1.8, 0.55)), "chrome", "frame_btn")
    save(to_img(compose_frame(320, 128, btn_draw, silver, 1.8, 0.4)), "chrome", "frame_btn_dim")
    # Forma de botón para recortar la superficie
    def btn_shape(c, w=320, h=128, cut=20):
        c.poly([(cut, 3), (w - cut, 3), (w - 3, cut), (w - 3, h - cut), (w - cut, h - 3), (cut, h - 3), (3, h - cut), (3, cut)], fill=(255, 255, 255, 255))

    m = mask_of(btn_shape, 320, 128)
    save(to_img(np.dstack([np.ones((128, 320, 3)), m])), "chrome", "shape_btn")
    # Brillo de vidrio del botón: arriba claro, abajo oscuro; da volumen sin bisel genérico
    ys = np.linspace(0, 1, 128)[:, None].repeat(320, 1)
    gl = np.dstack([np.ones((128, 320, 3)) * (ys < 0.5)[..., None], np.maximum((0.5 - ys) * 0.42, 0) + np.maximum((ys - 0.6) * 0.62, 0)])
    gl[..., 3] *= m
    save(to_img(gl), "chrome", "gloss_btn")
    # Botones horneados a 640x128: superficie con grano, volumen de luz y marco. Estirar el centro no deforma el grano visible.
    def bake_btn(name, tile, frame_tint, lift=1.0, rim=True):
        w, h = 640, 128
        cut = 20
        def sh(c):
            c.poly([(cut, 3), (w - cut, 3), (w - 3, cut), (w - 3, h - cut), (w - cut, h - 3), (cut, h - 3), (3, h - cut), (3, cut)], fill=(255, 255, 255, 255))
        mk = mask_of(sh, w, h)
        surf = tile_to(np.asarray(Image.open(os.path.join(OUT, "chrome", f"tile_{tile}.png")).convert("RGB")) / 255.0, h, w)
        ys = np.linspace(0, 1, h)[:, None].repeat(w, 1)
        vol = 1.28 - 0.62 * smooth(0.0, 1.0, ys) + 0.34 * np.exp(-((ys - 0.13) / 0.10) ** 2)
        rgb = np.clip(surf * vol[..., None] * lift, 0, 1)
        # sombra interior, hunde la superficie bajo el marco
        inner = 1 - ndi.gaussian_filter(mk, 6)
        rgb = rgb * (1 - np.clip(inner * 1.2, 0, 0.75))[..., None]
        im = to_img(np.dstack([rgb, mk]))
        def fr(c):
            W = (255, 255, 255, 255)
            pts = [(cut, 3), (w - cut, 3), (w - 3, cut), (w - 3, h - cut), (w - cut, h - 3), (cut, h - 3), (3, h - cut), (3, cut)]
            c.poly(pts, outline=W, w=4.6)
            i = 10
            p2 = [(cut + i * .6, 3 + i), (w - cut - i * .6, 3 + i), (w - 3 - i, cut + i * .6), (w - 3 - i, h - cut - i * .6), (w - cut - i * .6, h - 3 - i), (cut + i * .6, h - 3 - i), (3 + i, h - cut - i * .6), (3 + i, cut + i * .6)]
            c.poly(p2, outline=W, w=1.3)
        im.alpha_composite(to_img(compose_frame(w, h, fr, frame_tint, 1.7, 0.5)))
        save(im, "chrome", name)

    bake_btn("btn_primary", "crimson", gold, 1.0)
    bake_btn("btn_secondary", "leather", gold, 1.25)
    bake_btn("btn_disabled", "leather", (0.55, 0.52, 0.5), 0.8)
    bake_btn("btn_parchment", "parchment", (0.72, 0.5, 0.26), 1.0)
    for old in ("frame_btn", "frame_btn_dim", "shape_btn", "gloss_btn", "panel_abyss", "panel_parchment", "parchment_tile"):
        q = os.path.join(OUT, "chrome", old + ".png")
        if os.path.exists(q):
            os.remove(q)
            if os.path.exists(q + ".meta"):
                os.remove(q + ".meta")
    # Versiones @1x para el hub IMGUI, cuyos 9-slice se dibujan a un píxel por unidad.
    for nm, (ww, hh) in {"frame_panel": (210, 210), "btn_primary": (320, 64), "btn_secondary": (320, 64), "btn_disabled": (320, 64), "btn_parchment": (320, 64)}.items():
        src = Image.open(os.path.join(OUT, "chrome", nm + ".png")).convert("RGBA")
        save(src.resize((ww, hh), Image.LANCZOS), "chrome", nm + "_1x")
    # Borde de pergamino: quemado, rasgado y con sombra de hoja. Sobre la superficie tileada.
    S = 480
    yy, xx = np.mgrid[0:S, 0:S]
    d = np.minimum.reduce([xx, yy, S - 1 - xx, S - 1 - yy]).astype(float)
    n = noise(S, S, 40, 94, 5)
    deck = smooth(0, 1.6, d - 2 - n * 16)
    burn = 1 - smooth(10, 74, d + (n - 0.5) * 42)
    edge = np.dstack([lerp(hx("c9ab7b"), hx("5d3a1c"), burn ** 1.5), np.maximum(burn * 0.86, 0) * deck])
    # afuera de la hoja, transparente: se pinta negro con alfa de sombra
    out = (1 - deck)
    shadow = ndi.gaussian_filter(deck, 5) * out * 0.55
    rgb = lerp(edge[..., :3], np.zeros(3), out)
    alpha = np.maximum(edge[..., 3], shadow)
    save(to_img(np.dstack([rgb, alpha])), "chrome", "edge_parchment")
    # Barras superior e inferior: cuero con filo de oro y caída a transparente
    w, h = 1024, 200
    col = tile_to(cc0("Leather030", "Color", 512), h, w).mean(axis=2, keepdims=True)
    nrm = tile_to(cc0("Leather030", "NormalGL", 512), h, w)
    col = np.clip((col - col.mean()) * 2.4 + 0.5, 0, 1)
    alb = lerp(hx("0d0a12"), hx("2a1a2c"), col)
    base = np.clip(shade(np.zeros((h, w)), alb, nrm, 0.7, 0, 0.07, 18, 0.6, 0.6), 0, 1)
    ys = np.linspace(0, 1, h)
    # barra superior: opaca arriba, filo a 2/3, se desvanece
    def rail(c, y):
        c.line([(0, y), (w, y)], (255, 255, 255, 255), 3)
        c.line([(0, y + 8), (w, y + 8)], (255, 255, 255, 255), 1.1)

    m_top = mask_of(lambda c: rail(c, 152), w, h, 2)
    he = bevel(m_top, 1.4)
    goldrgb = shade(he, metal(h, w), None, 0, 22, 0.6, 18, 0.42, 0.92)
    rgb_top = lerp(base, goldrgb, m_top)
    a_top = np.clip(1 - smooth(0.80, 0.98, ys), 0, 1)[:, None].repeat(w, 1) * 0.97
    a_top = np.maximum(a_top, m_top)
    # sombra de la barra sobre la escena, que sigue al corte de la luz
    save(to_img(np.dstack([rgb_top, a_top])), "chrome", "bar_top")
    m_bot = mask_of(lambda c: rail(c, 62), w, h, 2)
    he = bevel(m_bot, 1.4)
    goldrgb = shade(he, metal(h, w), None, 0, 22, 0.6, 18, 0.42, 0.92)
    rgb_bot = lerp(base, goldrgb, m_bot)
    a_bot = smooth(0.10, 0.30, ys)[:, None].repeat(w, 1) * 0.98
    a_bot = np.maximum(a_bot, m_bot)
    save(to_img(np.dstack([rgb_bot, a_bot])), "chrome", "bar_bottom")
    # Filete ornamental en metal
    def div(c):
        W = (255, 255, 255, 255)
        c.poly([(0, 28), (600, 25), (640, 28), (600, 31)], fill=W)
        c.poly([(1280, 28), (680, 25), (640, 28), (680, 31)], fill=W)
        c.poly([(640, 10), (658, 28), (640, 46), (622, 28)], fill=W)
        c.poly([(596, 20), (604, 28), (596, 36), (588, 28)], fill=W)
        c.poly([(684, 20), (692, 28), (684, 36), (676, 28)], fill=W)

    save(to_img(compose_frame(1280, 56, div, gold, 1.2, 0.55, 18)), "chrome", "divider")
    # Sello de lacre: relieve real, borde irregular y anillo estampado
    S = 192
    rng = np.random.default_rng(5)
    pts = []
    for i in range(70):
        a_ = i / 70 * math.tau
        pts.append((96 + math.cos(a_) * (82 + rng.random() * 8), 96 + math.sin(a_) * (82 + rng.random() * 8)))
    m = mask_of(lambda c: c.poly(pts, fill=(255, 255, 255, 255)), S, S)
    ring = mask_of(lambda c: c.circle(96, 96, 62, outline=(255, 255, 255, 255), w=4), S, S)
    he = ndi.gaussian_filter(m, 7) - ndi.gaussian_filter(ring, 1.6) * 0.35 + noise(S, S, 40, 7, 3) * 0.05
    wax = lerp(hx("7c1824"), hx("b42e3f"), noise(S, S, 60, 8, 3))
    rgb = shade(he, wax, None, 0, 34, 0.5, 30, 0.5, 0.86)
    sh = ndi.gaussian_filter(np.roll(m, 4, 0), 5) * 0.6
    save(to_img(np.dstack([lerp(np.zeros_like(rgb), np.clip(rgb, 0, 1), m), np.maximum(m, sh)])), "chrome", "seal_wax")
    # Sello oscuro con aro de oro: soporte de bustos y planetas
    def seal(c):
        c.circle(96, 96, 88, outline=(255, 255, 255, 255), w=5.5)
        c.circle(96, 96, 76, outline=(255, 255, 255, 255), w=1.6)

    frame = compose_frame(S, S, seal, gold, 1.8, 0.7)
    inner = mask_of(lambda c: c.circle(96, 96, 86, fill=(255, 255, 255, 255)), S, S)
    leather = tile_to(cc0("Leather030", "Color", 512), S, S).mean(axis=2, keepdims=True)
    leather = lerp(hx("0e0a14"), hx("2a1a2c"), np.clip((leather - leather.mean()) * 2.4 + 0.5, 0, 1))
    yy2, xx2 = np.mgrid[0:S, 0:S]
    v = np.hypot(xx2 - 80, yy2 - 74) / 110
    leather = leather * (1.25 - 0.7 * np.clip(v, 0, 1))[..., None]
    a = inner.copy()
    base = np.dstack([np.clip(leather, 0, 1), a])
    im = to_img(base)
    im.alpha_composite(to_img(frame))
    save(im, "chrome", "seal_dark")
    # Riel y relleno de barras: hendido, con luz interna en el relleno
    def rail_shape(c, w=128, h=40):
        c.poly([(10, 2), (w - 10, 2), (w - 2, h / 2), (w - 10, h - 2), (10, h - 2), (2, h / 2)], fill=(255, 255, 255, 255))

    m = mask_of(rail_shape, 128, 40)
    inner_sh = 1 - ndi.gaussian_filter(m, 3)
    rgb = np.ones((40, 128, 3)) * hx("0b0810") + inner_sh[..., None] * -0.03
    frame_r = compose_frame(128, 40, lambda c: c.poly([(10, 2), (118, 2), (126, 20), (118, 38), (10, 38), (2, 20)], outline=(255, 255, 255, 255), w=2.6), gold, 1.1, 0.5)
    im = to_img(np.dstack([np.clip(rgb, 0, 1), m]))
    im.alpha_composite(to_img(frame_r))
    save(im, "chrome", "bar_rail")
    ys = np.linspace(0, 1, 40)[:, None].repeat(128, 1)
    fill = 0.72 + 0.5 * np.exp(-((ys - 0.3) / 0.22) ** 2) - 0.25 * smooth(0.6, 1, ys)
    save(to_img(np.dstack([np.clip(fill[..., None].repeat(3, 2), 0, 1), m])), "chrome", "bar_fill")
    # Cola de globo de diálogo, del color del pergamino
    c = SS(44, 30, 4)
    c.poly([(0, 0), (44, 0), (8, 30)], fill=rgba(hx("e5d8bb"), 1))
    save(c.done(), "chrome", "bubble_tail")
    # Volúmenes para tarjetas: degradado negro-transparente y brillo de cristal
    ys = np.linspace(0, 1, 256)[:, None].repeat(4, 1)
    save(to_img(np.dstack([np.zeros((256, 4, 3)), smooth(0.25, 1, ys) * 0.92])), "chrome", "shade_down")
    save(to_img(np.dstack([np.zeros((256, 4, 3)), (1 - smooth(0, 0.5, ys)) * 0.7])), "chrome", "shade_up")


def icon_gold(src_img, tint=(1.0, 0.84, 0.52)):
    """Icono de metal repujado con contorno de tinta, a partir de su silueta. Se lee sobre cualquier fondo."""
    a = np.asarray(src_img)[..., 3] / 255.0
    h, w = a.shape
    he = bevel(a, 2.0)
    rgb = shade(he, metal(h, w, tint, 0.62), None, 0, 20, 0.55, 18, 0.46, 0.9)
    out = ndi.gaussian_filter(a, 3.0)
    out = smooth(0.02, 0.28, out)
    sh = ndi.gaussian_filter(np.roll(a, 3, 0), 3.4) * 0.5
    base_a = np.clip(np.maximum(out * 0.92, sh), 0, 1)
    final_rgb = lerp(hx("120a10"), np.clip(rgb, 0, 1), a)
    return np.dstack([final_rgb, np.maximum(a, base_a)])


def planet_seal(glyph_img, color):
    """Medallón del planeta: fondo de cuero, metal del planeta en relieve (color y glifo: nunca va solo)."""
    S = 192
    m = np.asarray(glyph_img.resize((S, S), Image.LANCZOS))[..., 3] / 255.0
    pad = int(S * 0.2)
    small = np.asarray(Image.fromarray((m * 255).astype(np.uint8)).resize((S - pad * 2, S - pad * 2), Image.LANCZOS)) / 255.0
    g = np.zeros((S, S))
    g[pad:S - pad, pad:S - pad] = small
    frame = compose_frame(S, S, lambda c: (c.circle(96, 96, 88, outline=(255, 255, 255, 255), w=6), c.circle(96, 96, 76, outline=(255, 255, 255, 255), w=1.8)), tuple(np.clip(color * 1.15 + 0.1, 0, 1)), 1.8, 0.7)
    disc = mask_of(lambda c: c.circle(96, 96, 87, fill=(255, 255, 255, 255)), S, S)
    yy, xx = np.mgrid[0:S, 0:S]
    v = np.hypot(xx - 76, yy - 66) / 130
    bg = lerp(color * 0.5 + hx("1a1220") * 0.5, hx("0d0a12"), np.clip(v, 0, 1))
    he = bevel(g, 2.2)
    rgb = shade(he, np.clip(color * 0.65 + np.array([0.35, 0.35, 0.35]), 0, 1) * np.ones((S, S, 1)), None, 0, 26, 0.7, 22, 0.5, 0.9)
    rgb = lerp(bg, np.clip(rgb, 0, 1), g)
    im = to_img(np.dstack([np.clip(rgb, 0, 1), disc]))
    im.alpha_composite(to_img(frame))
    return im


def make_gold_icons():
    d = os.path.join(OUT, "icons")
    for f in sorted(os.listdir(d)):
        if not f.endswith(".png"):
            continue
        save(to_img(icon_gold(Image.open(os.path.join(d, f)).convert("RGBA"))), "iconsg", f[:-4])
    colors = {"sol": "F5B02E", "luna": "E2F0FA", "marte": "C7301A", "mercurio": "1CC8EE", "venus": "86A775", "jupiter": "6262EA", "saturno": "85608A"}
    pd = os.path.join(OUT, "planets")
    for pid, col in colors.items():
        save(planet_seal(Image.open(os.path.join(pd, pid + ".png")).convert("RGBA"), hx(col)), "seals", pid)



# ---------------------------------------------------------------------------------------------- Iconos


def icon_canvas(s=128):
    return SS(s, s, 4)


def icons():
    W = (255, 255, 255, 255)
    res = {}

    def stroke(c, pts, w=5):
        c.line(pts, W, w)

    # Inicio: caparazón de la Tortuga
    c = icon_canvas()
    pts = [(14, 88), (30, 44), (64, 26), (98, 44), (114, 88)]
    c.line(pts, W, 6)
    c.line([(14, 88), (114, 88)], W, 6)
    c.poly([(64, 42), (82, 54), (82, 74), (64, 86), (46, 74), (46, 54)], outline=W, w=4.5)
    c.line([(64, 42), (64, 28)], W, 3.5)
    c.line([(82, 54), (98, 45)], W, 3.5)
    c.line([(46, 54), (30, 45)], W, 3.5)
    c.line([(114, 88), (122, 84), (124, 76)], W, 6)
    c.line([(32, 88), (28, 104)], W, 7)
    c.line([(96, 88), (100, 104)], W, 7)
    res["home"] = c
    # Aquelarre: sombrero de bruja
    c = icon_canvas()
    c.poly([(58, 12), (84, 40), (94, 84), (34, 84), (46, 40)], outline=W, w=5.5)
    c.line([(10, 94), (34, 84), (94, 84), (118, 94), (64, 106), (10, 94)], W, 5.5)
    c.line([(38, 72), (90, 72)], W, 6)
    c.poly([(64, 62), (70, 72), (64, 82), (58, 72)], fill=W)
    res["coven"] = c
    # Yunque
    c = icon_canvas()
    c.poly([(12, 40), (98, 40), (118, 48), (96, 62), (84, 62), (80, 80), (92, 92), (92, 104), (36, 104), (36, 92), (48, 80), (44, 62), (28, 62), (12, 52)], outline=W, w=5.5)
    c.circle(112, 22, 3.5, fill=W)
    c.circle(98, 16, 2.5, fill=W)
    c.circle(120, 34, 2.5, fill=W)
    res["forge"] = c
    # Caldero
    c = icon_canvas()
    c.line([(18, 54), (110, 54)], W, 7)
    c.d.arc([24 * 4, 34 * 4, 104 * 4, 110 * 4], 0, 180, fill=W, width=24)
    c.line([(24, 54), (24, 72)], W, 6)
    c.line([(104, 54), (104, 72)], W, 6)
    c.line([(42, 106), (34, 118)], W, 6)
    c.line([(86, 106), (94, 118)], W, 6)
    for x, y in [(50, 34), (64, 24), (80, 36)]:
        c.circle(x, y, 5, outline=W, w=3)
    res["cauldron"] = c
    # Manantial: gota y ondas con vapor
    c = icon_canvas()
    c.d.arc([14 * 4, 78 * 4, 114 * 4, 118 * 4], 0, 360, fill=W, width=20)
    c.d.arc([38 * 4, 88 * 4, 90 * 4, 108 * 4], 0, 360, fill=W, width=14)
    for x in (42, 64, 86):
        pts = [(x + math.sin(i * 0.7) * 6, 72 - i * 7) for i in range(9)]
        c.line(pts, W, 4.5)
    res["spring"] = c
    # Tienda: balanza contable
    c = icon_canvas()
    c.line([(64, 14), (64, 110)], W, 6)
    c.line([(40, 112), (88, 112)], W, 6)
    c.line([(18, 32), (110, 32)], W, 5)
    for x in (24, 104):
        c.line([(x, 32), (x - 14, 72)], W, 3)
        c.line([(x, 32), (x + 14, 72)], W, 3)
        c.d.arc([(x - 18) * 4, 54 * 4, (x + 18) * 4, 90 * 4], 0, 180, fill=W, width=18)
    c.circle(64, 14, 5, fill=W)
    res["shop"] = c
    # Libro abierto
    c = icon_canvas()
    c.poly([(64, 36), (40, 24), (12, 28), (12, 96), (40, 92), (64, 104), (88, 92), (116, 96), (116, 28), (88, 24)], outline=W, w=5)
    c.line([(64, 36), (64, 104)], W, 4.5)
    for y in (46, 60, 74):
        c.line([(26, y), (50, y + 3)], W, 3)
        c.line([(78, y + 3), (102, y)], W, 3)
    res["book"] = c
    # Fresno
    c = icon_canvas()
    c.line([(64, 118), (64, 66)], W, 8)

    def br(x, y, a, l, d):
        x2, y2 = x + math.cos(a) * l, y + math.sin(a) * l
        c.line([(x, y), (x2, y2)], W, max(2, d * 1.4))
        if d > 0:
            br(x2, y2, a - 0.48, l * 0.74, d - 1)
            br(x2, y2, a + 0.48, l * 0.74, d - 1)
        else:
            c.circle(x2, y2, 3.2, fill=W)

    br(64, 70, -math.pi / 2, 26, 3)
    res["tree"] = c
    # Sobre
    c = icon_canvas()
    c.poly([(14, 34), (114, 34), (114, 98), (14, 98)], outline=W, w=5.5)
    c.line([(14, 34), (64, 72), (114, 34)], W, 5)
    c.circle(64, 72, 9, fill=W)
    res["mail"] = c
    # Encargos: rollo con marcas
    c = icon_canvas()
    c.poly([(28, 20), (100, 20), (100, 108), (28, 108)], outline=W, w=5.5)
    for y in (42, 64, 86):
        c.line([(40, y), (48, y + 7), (58, y - 7)], W, 4)
        c.line([(66, y), (90, y)], W, 4)
    res["tasks"] = c
    # Ajustes: rueda de astrolabio
    c = icon_canvas()
    c.circle(64, 64, 26, outline=W, w=6)
    c.circle(64, 64, 8, fill=W)
    for i in range(8):
        a = i / 8 * math.tau
        c.line([(64 + math.cos(a) * 28, 64 + math.sin(a) * 28), (64 + math.cos(a) * 46, 64 + math.sin(a) * 46)], W, 9)
    res["settings"] = c
    # Atrás
    c = icon_canvas()
    c.line([(78, 22), (36, 64), (78, 106)], W, 10)
    res["back"] = c
    # Juicios: balanza inclinada / espada corta
    c = icon_canvas()
    c.poly([(64, 12), (76, 34), (70, 88), (58, 88), (52, 34)], outline=W, w=5)
    c.line([(36, 90), (92, 90)], W, 7)
    c.line([(64, 92), (64, 116)], W, 7)
    res["trial"] = c
    # Lupa/probabilidades
    c = icon_canvas()
    c.circle(54, 54, 32, outline=W, w=6.5)
    c.line([(78, 78), (112, 112)], W, 10)
    c.text(54, 54, "%", font("Alegreya-Bold.ttf", 38), W)
    res["odds"] = c
    # Plus
    c = icon_canvas()
    c.line([(64, 26), (64, 102)], W, 10)
    c.line([(26, 64), (102, 64)], W, 10)
    res["plus"] = c
    # Candado
    c = icon_canvas()
    c.poly([(30, 58), (98, 58), (98, 112), (30, 112)], fill=W)
    c.d.arc([40 * 4, 18 * 4, 88 * 4, 78 * 4], 180, 360, fill=W, width=28)
    res["lock"] = c
    # Altavoz
    c = icon_canvas()
    c.poly([(18, 50), (42, 50), (68, 26), (68, 102), (42, 78), (18, 78)], fill=W)
    c.d.arc([58 * 4, 38 * 4, 96 * 4, 90 * 4], 300, 60, fill=W, width=20)
    c.d.arc([58 * 4, 22 * 4, 114 * 4, 106 * 4], 310, 50, fill=W, width=20)
    res["sound"] = c
    # Espejo
    c = icon_canvas()
    c.d.ellipse([34 * 4, 12 * 4, 94 * 4, 88 * 4], outline=W, width=24)
    c.line([(64, 88), (64, 118)], W, 8)
    c.line([(48, 118), (80, 118)], W, 6)
    c.line([(52, 36), (66, 22)], W, 4)
    res["mirror"] = c
    for k, v in res.items():
        save(v.done(), "icons", k)

    # Planetas: glifos blancos para teñir
    f = font("NotoSansSymbols-Regular.ttf", 92)
    c = icon_canvas()
    c.circle(64, 64, 40, outline=W, w=8)
    c.circle(64, 64, 9, fill=W)
    save(c.done(), "planets", "sol")
    for pid, g in [("luna", "☽"), ("marte", "♂"), ("mercurio", "☿"), ("venus", "♀"), ("jupiter", "♃"), ("saturno", "♄")]:
        c = icon_canvas()
        c.text(64, 62, g, f, W)
        save(c.done(), "planets", pid)

    # Monedas, con color propio: la identidad de cada recurso se lee sin texto
    def crystal(c, cx, cy, s, col, seed=0):
        """Cubo de sal en perspectiva: tres caras; aristas rectas."""
        t = lerp(col, np.array([1, 1, 1]), 0.45)
        l = col * 0.82
        r = col * 0.55
        top = [(cx, cy - s), (cx + s * .9, cy - s * .5), (cx, cy), (cx - s * .9, cy - s * .5)]
        left = [(cx - s * .9, cy - s * .5), (cx, cy), (cx, cy + s), (cx - s * .9, cy + s * .5)]
        right = [(cx + s * .9, cy - s * .5), (cx, cy), (cx, cy + s), (cx + s * .9, cy + s * .5)]
        c.poly(top, fill=rgba(t))
        c.poly(left, fill=rgba(l))
        c.poly(right, fill=rgba(r))
        for p in (top, left, right):
            c.poly(p, outline=rgba(np.array([1, 1, 1]), 0.5), w=1.4)

    c = icon_canvas()
    crystal(c, 66, 44, 26, hx("f0a9a0"))
    crystal(c, 40, 78, 22, hx("f7c8bb"))
    crystal(c, 88, 86, 18, hx("d97f7c"))
    save(c.done(), "currency", "sales_de_bano")
    # Almita: llama-alma, núcleo blanco, halo dorado
    s = 128
    yy, xx = np.mgrid[0:s, 0:s].astype(float)
    dy = (yy - 74) / 40
    dx = (xx - 64) / (30 * np.clip(1.15 - (np.clip(-dy, -1, 1) + 1) / 2, 0.12, 1))
    d = np.hypot(dx, dy)
    core = 1 - smooth(0.25, 1.0, d)
    halo = np.exp(-(((xx - 64) ** 2 + (yy - 68) ** 2) / 1500))
    rgb = lerp(hx("ffd98a"), hx("fffbe8"), core[..., None] ** 0.8)
    alpha = np.clip(core * 1.6 + halo * 0.42, 0, 1)
    save(to_img(np.dstack([rgb, alpha])), "currency", "almitas")
    # Savia: gota verde dorada
    c = icon_canvas()
    pts = [(64 + (34 * math.sin(a)) * (1 - 0.72 * (1 + math.cos(a)) / 2 * 0.85), 80 - 52 * math.cos(a) * (0.9 if math.cos(a) > 0 else 0.62)) for a in np.linspace(0, math.tau, 60)]
    c.poly(pts, fill=rgba(hx("9fb35a")), outline=rgba(hx("e6e39a")), w=3)
    c.circle(54, 78, 7, fill=(255, 255, 255, 160))
    save(c.done(), "currency", "savia")
    # Polvo astral: estrellas violáceas
    c = icon_canvas()
    for x, y, r in [(62, 56, 34), (98, 90, 16), (32, 96, 14), (102, 30, 10)]:
        pts = []
        for i in range(8):
            a = i / 8 * math.tau - math.pi / 2
            rr = r if i % 2 == 0 else r * 0.26
            pts.append((x + math.cos(a) * rr, y + math.sin(a) * rr))
        c.poly(pts, fill=rgba(hx("cdb4ff")))
    save(c.done(), "currency", "polvo_astral")
    # Perla: orbe nacarado
    yy, xx = np.mgrid[0:s, 0:s].astype(float)
    d = np.hypot(xx - 64, yy - 64) / 46
    n_ = np.clip(1 - d ** 2, 0, 1) ** 0.5
    lx = np.clip(((xx - 64) * -0.45 + (yy - 64) * -0.6) / 46 + n_ * 0.66, 0, 1)
    rgb = lerp(hx("c9b6c4"), hx("fff8f2"), lx ** 1.4) + (np.sin(xx * 0.09 + yy * 0.06) * 0.03)[..., None]
    alpha = 1 - smooth(0.97, 1.02, d)
    save(to_img(np.dstack([np.clip(rgb, 0, 1), alpha])), "currency", "perlas")


# ---------------------------------------------------------------------------------------------- Efectos


def make_fx():
    # Punto suave (partículas, almitas, brillos)
    s = 128
    yy, xx = np.mgrid[0:s, 0:s].astype(float) - 63.5
    r = np.hypot(xx, yy) / 64
    save(to_img(np.dstack([np.ones((s, s, 3)), np.clip(1 - r, 0, 1) ** 2.4])), "fx", "soft_dot")
    # Destello de cuatro puntas
    a = np.exp(-np.abs(xx) / 3.4) * np.exp(-np.abs(yy) / 34) + np.exp(-np.abs(yy) / 3.4) * np.exp(-np.abs(xx) / 34) + np.exp(-(r * 5) ** 2) * 0.6
    save(to_img(np.dstack([np.ones((s, s, 3)), np.clip(a * (1 - smooth(0.8, 1, r)), 0, 1)])), "fx", "sparkle")
    # Vapor: cuatro bocanadas de ruido
    for i in range(4):
        s2 = 512
        yy2, xx2 = np.mgrid[0:s2, 0:s2].astype(float) - 255.5
        rr = np.hypot(xx2, yy2) / 256
        n = noise(s2, s2, 120, 200 + i, 6)
        puff = np.clip(1 - rr * (1.1 - n * 0.5), 0, 1)
        puff = smooth(0.0, 0.85, puff) * smooth(0.25, 0.8, n + 0.25) * (1 - smooth(0.82, 1.0, rr))
        save(to_img(np.dstack([np.ones((s2, s2, 3)), puff])), "fx", f"steam_{i}")
    # Rayos de luz radiales
    s3 = 1024
    yy3, xx3 = np.mgrid[0:s3, 0:s3].astype(float) - 511.5
    ang = np.arctan2(yy3, xx3)
    rr = np.hypot(xx3, yy3) / 512
    rng = np.random.default_rng(301)
    rays = np.zeros_like(ang)
    for f, w_ in zip(rng.integers(9, 90, 26), rng.random(26)):
        rays += w_ * np.cos(ang * f + rng.random() * 6.28)
    rays = (rays - rays.min()) / (rays.max() - rays.min())
    a = smooth(0.42, 0.92, rays) ** 1.3 * (1 - smooth(0.02, 1.0, rr)) ** 1.8
    save(to_img(np.dstack([np.ones((s3, s3, 3)), a])), "fx", "rays")
    # Onda en el agua
    s4 = 512
    yy4, xx4 = np.mgrid[0:s4, 0:s4].astype(float) - 255.5
    rr = np.hypot(xx4, yy4) / 256
    a = np.exp(-((rr - 0.9) / 0.03) ** 2) + 0.4 * np.exp(-((rr - 0.78) / 0.02) ** 2)
    save(to_img(np.dstack([np.ones((s4, s4, 3)), np.clip(a, 0, 1) * (1 - smooth(0.97, 1, rr))])), "fx", "ripple")
    # Cristales de sal (cúbicos: la arista recta distingue la sal de la nieve)
    for i, (col, sz) in enumerate([(hx("ffffff"), 40), (hx("ffe5dc"), 34), (hx("f7c8bb"), 28)]):
        c = SS(128, 128, 4)
        cx, cy, s_ = 64, 64, sz
        t, l, r_ = lerp(col, np.ones(3), 0.4), col * 0.85, col * 0.58
        top = [(cx, cy - s_), (cx + s_ * .9, cy - s_ * .5), (cx, cy), (cx - s_ * .9, cy - s_ * .5)]
        left = [(cx - s_ * .9, cy - s_ * .5), (cx, cy), (cx, cy + s_), (cx - s_ * .9, cy + s_ * .5)]
        right = [(cx + s_ * .9, cy - s_ * .5), (cx, cy), (cx, cy + s_), (cx + s_ * .9, cy + s_ * .5)]
        for p_, q_ in ((top, t), (left, l), (right, r_)):
            c.poly(p_, fill=rgba(q_), outline=rgba(np.ones(3), 0.55), w=1.2)
        save(c.done(), "fx", f"crystal_{i}")
    # Viñeta y degradado
    s5 = 512
    yy5, xx5 = np.mgrid[0:s5, 0:s5].astype(float) / (s5 - 1) - 0.5
    v = smooth(0.35, 0.78, np.hypot(xx5, yy5))
    save(to_img(np.dstack([np.zeros((s5, s5, 3)), v])), "fx", "vignette")
    w = np.linspace(0, 1, 256)[:, None].repeat(4, 1)
    save(to_img(np.dstack([np.ones((256, 4, 3)), w])), "fx", "grad_v")
    # Ruido teselable para shaders (R: fBm grande, G: fBm chico, B: voronoi, A: 1)
    s6 = 512

    def tile_noise(scale, seed, oct_):
        rng = np.random.default_rng(seed)
        out = np.zeros((s6, s6))
        amp = 1.0
        for o in range(oct_):
            n_ = 2 ** (o + scale)
            g = rng.random((n_, n_))
            big = ndi.zoom(np.pad(g, 3, mode="wrap"), s6 / n_, order=3)
            off = int(3 * s6 / n_)
            out += big[off:off + s6, off:off + s6] * amp
            amp *= 0.55
        return (out - out.min()) / (out.max() - out.min())

    r_ = tile_noise(3, 401, 5)
    g_ = tile_noise(5, 402, 4)
    b_ = tile_noise(2, 403, 6)
    arr = (np.dstack([r_, g_, b_]) * 255).astype(np.uint8)
    d = os.path.join(OUT, "fx")
    Image.fromarray(arr, "RGB").save(os.path.join(d, "noise.png"))
    print("  fx/noise")
    # Grietas doradas (kintsugi), para Paralaje alto y para el quiebre
    W_, H_ = 1600, 900
    c = SS(W_, H_, 1)
    rng = np.random.default_rng(405)

    def crack(x, y, a, l, w_):
        n = int(l / 26)
        pts = [(x, y)]
        for i in range(n):
            a += (rng.random() - 0.5) * 0.9
            x += math.cos(a) * 26
            y += math.sin(a) * 26
            pts.append((x, y))
            if rng.random() < 0.14 and w_ > 1.4:
                crack(x, y, a + (rng.random() - 0.5) * 2.4, l * 0.42, w_ * 0.6)
        c.line(pts, rgba(GOLD_B, 1), w_)

    for sx, sy, a in [(0, 120, 0.35), (W_, 90, 2.7), (0, 760, -0.4), (W_, 820, 3.5), (720, 0, 1.4), (1020, H_, -1.7)]:
        crack(sx, sy, a, 900, 3.6)
    save(c.done().filter(ImageFilter.GaussianBlur(0.6)), "fx", "cracks")
    # Astrolabios
    save(astrolabe(1024), "fx", "astrolabe")
    save(ring_only(1024), "fx", "ring")
    # Silueta genérica de bruja para quien aún no tiene retrato: sombrero y hombros en blanco para teñir
    c = SS(512, 768, 2)
    c.poly([(246, 40), (312, 130), (340, 270), (172, 270), (206, 130)], fill=(255, 255, 255, 255))
    c.d.ellipse([60 * 2, 250 * 2, 452 * 2, 310 * 2], fill=(255, 255, 255, 255))
    c.d.ellipse([190 * 2, 270 * 2, 322 * 2, 420 * 2], fill=(255, 255, 255, 255))
    c.poly([(198, 390), (314, 390), (420, 760), (92, 760)], fill=(255, 255, 255, 255))
    save(c.done().filter(ImageFilter.GaussianBlur(2)), "fx", "silhouette")


# ---------------------------------------------------------------------------------------------- Logo


def make_logo():
    W, H = 1400, 520
    k = 2
    f = ImageFont.truetype(os.path.join(FONTS, "PirataOne-Regular.ttf"), 250 * k)
    fs = ImageFont.truetype(os.path.join(FONTS, "Texturina-SemiBold.ttf"), 46 * k)
    m = Image.new("L", (W * k, H * k), 0)
    d = ImageDraw.Draw(m)
    d.text((W * k // 2, 215 * k), "Sal y Almas", font=f, fill=255, anchor="mm")
    mask = np.asarray(m) / 255.0
    # relieve: pan de oro con vetas y luz arriba
    blur = ndi.gaussian_filter(mask, 5 * k * 0.6)
    gy, gx = np.gradient(blur)
    light = np.clip((-gy * 0.9 - gx * 0.4) * 22, -1, 1)
    h_, w_ = mask.shape
    veta = noise(h_, w_, 120, 501, 5)
    vy = np.linspace(0, 1, h_)[:, None].repeat(w_, 1)
    col = lerp(hx("fff0b8"), hx("b8802a"), smooth(0.28, 0.62, vy + (veta - 0.5) * 0.25))
    col = col + light[..., None] * 0.4
    # costra de sal sobre la parte alta de las letras: cubitos blancos
    crust = smooth(0.62, 0.78, noise(h_, w_, 30, 502, 3)) * (1 - smooth(0.24, 0.42, vy))
    col = lerp(col, hx("fbf6ee"), crust * 0.8)
    alpha = mask
    # contorno de tinta y sombra
    out = ndi.gaussian_filter(mask, 7.0 * k * 0.5)
    out = smooth(0.05, 0.25, out)
    shadow = np.roll(ndi.gaussian_filter(mask, 12 * k), 10 * k, axis=0) * 0.75
    base_a = np.clip(out + shadow, 0, 1)
    rgb = lerp(hx("170d14"), col, alpha)
    a = np.clip(alpha + base_a * (1 - alpha), 0, 1)
    im = to_img(np.dstack([rgb, a]))
    # subtítulo con filete
    c = Image.new("RGBA", (W * k, H * k), (0, 0, 0, 0))
    dc = ImageDraw.Draw(c, "RGBA")
    dc.text((W * k // 2, 402 * k), "La Odisea del Fresno", font=fs, fill=rgba(hx("f2e6cd"), 1), anchor="mm", stroke_width=3, stroke_fill=(20, 12, 18, 230))
    for sx in (-1, 1):
        x0 = W * k // 2 + sx * 300 * k
        dc.line([(x0, 402 * k), (x0 + sx * 200 * k, 402 * k)], fill=rgba(GOLD_B, 0.9), width=2 * k)
        dc.polygon([(x0, 394 * k), (x0 + 8 * k, 402 * k), (x0, 410 * k), (x0 - 8 * k, 402 * k)], fill=rgba(GOLD_B, 1))
    im.alpha_composite(c)
    save(im.resize((W, H), Image.LANCZOS), "logo", "logo")


# ---------------------------------------------------------------------------------------------- Tarjetas de destino


def engraved_card(W, H, seed, tint_top, tint_bot, draw):
    g = grad(H, W, [(0, tint_top), (1, tint_bot)])
    n = noise(H, W, 120, seed, 5)[..., None]
    g = g * (0.82 + 0.34 * n)
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    v = np.hypot((xx - W / 2) / W, (yy - H / 2) / H)
    g = g * (1 - smooth(0.32, 0.78, v)[..., None] * 0.7)
    lc = tile_to(cc0("Leather030", "NormalGL", 512), H, W)
    g = np.clip(shade(np.zeros((H, W)), g, lc, 0.55, 0, 0.05, 16, 0.72, 0.5), 0, 1)
    im = to_img(np.dstack([g, np.ones((H, W))]))
    c = SS(W, H, 2)
    draw(c, W, H)
    layer = c.done()
    glow = layer.filter(ImageFilter.GaussianBlur(7))
    im.alpha_composite(glow)
    im.alpha_composite(layer)
    return im


def draw_tree(c, W, H):
    rng = np.random.default_rng(601)
    gold = rgba(GOLD_B, 0.92)
    # anillos de zona
    for i, r in enumerate([0.26, 0.42, 0.58, 0.74]):
        c.arc(W * 0.5, H * 1.02, H * r * 1.25, 180, 360, rgba(GOLD_B, 0.25), 1.2)

    def br(x, y, a, l, d):
        x2 = x + math.cos(a) * l
        y2 = y + math.sin(a) * l
        c.line([(x, y), (x2, y2)], gold, max(1.0, d * 1.05))
        if d > 0:
            n = 2 if rng.random() < 0.8 else 3
            for i in range(n):
                br(x2, y2, a + (i - (n - 1) / 2) * 0.62 + (rng.random() - 0.5) * 0.3, l * (0.68 + rng.random() * 0.14), d - 1)
        elif rng.random() < 0.55:
            c.circle(x2, y2, 1.8 + rng.random() * 1.6, fill=rgba(SPARK, 0.95))

    br(W * 0.5, H * 0.96, -math.pi / 2, H * 0.22, 8)
    # raíces
    for a in (0.3, 0.9, 2.2, 2.85):
        br(W * 0.5, H * 0.96, a, H * 0.07, 2)


def draw_book(c, W, H):
    gold = rgba(GOLD_B, 0.92)
    cx, cy = W * 0.5, H * 0.62
    s = H * 0.3
    c.poly([(cx, cy - s * .3), (cx - s * 1.4, cy - s * .75), (cx - s * 1.5, cy + s * .55), (cx, cy + s), (cx + s * 1.5, cy + s * .55), (cx + s * 1.4, cy - s * .75)], fill=rgba(hx("efe3c8"), 0.11), outline=gold, w=2.2)
    c.line([(cx, cy - s * .3), (cx, cy + s)], gold, 2)
    for i in range(5):
        t = i / 5
        for sg in (-1, 1):
            c.line([(cx + sg * s * .14, cy - s * .18 + t * s * .9), (cx + sg * s * 1.24, cy - s * .55 + t * s * .9)], rgba(GOLD_B, 0.5), 1.1)
    rng = np.random.default_rng(611)
    pts = [(cx + (rng.random() - .5) * W * .8, H * .07 + rng.random() * H * .3) for _ in range(9)]
    pts.sort()
    c.line(pts, rgba(GOLD_B, 0.5), 1.1)
    for p in pts:
        c.circle(p[0], p[1], 2.8, fill=rgba(SPARK, 1))
    # hilo de almas que sube de la página
    for i in range(10):
        x = cx + (rng.random() - .5) * s * 1.6
        y = cy - s * .35 - rng.random() * s * 0.9
        c.circle(x, y, 1.4 + rng.random() * 2, fill=rgba(SPARK, .85))


def draw_spring(c, W, H):
    gold = rgba(GOLD_B, 0.92)
    cx, cy = W * 0.5, H * 0.8
    for i, r in enumerate([0.17, 0.3, 0.45, 0.62, 0.8]):
        c.d.arc([(cx - W * r) * 2, (cy - H * r * .26) * 2, (cx + W * r) * 2, (cy + H * r * .26) * 2], 0, 360, fill=rgba(GOLD_B, .75 - i * .12), width=3)
    for i, x in enumerate((-.22, -.07, .08, .24)):
        pts = [(cx + W * x + math.sin(t * .9 + i) * 11, cy - H * .1 - t * H * .055) for t in range(10)]
        c.line(pts, rgba(np.array([1, .93, .85]), .55), 2.4)
    # cubo de sal que cae
    s = H * .085
    x0, y0 = cx, H * .24
    top = [(x0, y0 - s), (x0 + s * .9, y0 - s * .5), (x0, y0), (x0 - s * .9, y0 - s * .5)]
    left = [(x0 - s * .9, y0 - s * .5), (x0, y0), (x0, y0 + s), (x0 - s * .9, y0 + s * .5)]
    right = [(x0 + s * .9, y0 - s * .5), (x0, y0), (x0, y0 + s), (x0 + s * .9, y0 + s * .5)]
    c.poly(top, fill=rgba(SALT), outline=gold, w=1.4)
    c.poly(left, fill=rgba(hx("f0d3c8")), outline=gold, w=1.4)
    c.poly(right, fill=rgba(hx("c99a92")), outline=gold, w=1.4)


def draw_scale(c, W, H):
    gold = rgba(GOLD_B, 0.92)
    cx = W * .5
    c.line([(cx, H * .14), (cx, H * .86)], gold, 3)
    c.line([(cx - W * .12, H * .86), (cx + W * .12, H * .86)], gold, 3)
    c.line([(cx - W * .3, H * .32), (cx + W * .3, H * .22)], gold, 2.6)
    for sx, y in ((-.3, .32), (.3, .22)):
        x = cx + W * sx
        c.line([(x, H * y), (x - W * .07, H * (y + .25))], rgba(GOLD_B, .7), 1.4)
        c.line([(x, H * y), (x + W * .07, H * (y + .25))], rgba(GOLD_B, .7), 1.4)
        c.d.arc([(x - W * .08) * 2, (H * (y + .15)) * 2, (x + W * .08) * 2, (H * (y + .36)) * 2], 0, 180, fill=gold, width=4)


def make_cards():
    save(engraved_card(900, 520, 701, hx("3a2236"), hx("170f1d"), draw_tree), "cards", "fresno")
    save(engraved_card(900, 520, 702, hx("4a2a30"), hx("1c1016"), draw_book), "cards", "story")
    save(engraved_card(900, 520, 703, hx("7d3d44"), hx("220f18"), draw_spring), "cards", "spring")
    save(engraved_card(900, 520, 704, hx("2c2638"), hx("120e18"), draw_scale), "cards", "trial")


# ---------------------------------------------------------------------------------------------- Brujas


# ---------------------------------------------------------------------------------------------- Escenarios de combate


BW, BH = 2560, 1280
GROUND = 0.60  # línea de horizonte del suelo, desde arriba


def rot_tile(a):
    return np.rot90(a, 1)


def persp_map(tex, W, H, hor, scale_u=1.0, scale_v=1.0, cx=0.5):
    """Proyecta una textura en el suelo con perspectiva real: no se estira, se aleja."""
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    dy = np.maximum(yy - hor, 1.0)
    z = 900.0 / (dy + 40)
    u = ((xx - W * cx) * z * 0.7 * scale_u).astype(np.int64) % tex.shape[1]
    v = ((z * 620 * scale_v).astype(np.int64)) % tex.shape[0]
    return tex[v, u], np.clip((yy - hor) / (H - hor), 0, 1)


def light_cone(W, H, x, top_w, bottom_w, y0=0, y1=None):
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    y1 = H if y1 is None else y1
    t = np.clip((yy - y0) / (y1 - y0), 0, 1)
    half = top_w + (bottom_w - top_w) * t
    return np.exp(-((xx - x - t * 80) / np.maximum(half, 1)) ** 2) * (1 - t) ** 1.4


def spores(rgb, count, seed, color, ymax=0.8, size=2.0, strength=1.0):
    H, W, _ = rgb.shape
    rng = np.random.default_rng(seed)
    g = np.zeros((H, W))
    ys = (rng.random(count) * H * ymax).astype(int)
    xs = rng.integers(0, W, count)
    g[ys, xs] = rng.random(count) ** 2 * 6
    g = ndi.gaussian_filter(g, size) * 40 * size + ndi.gaussian_filter(g, size * 4) * 24 * size
    return np.clip(rgb + np.clip(g, 0, 1.4)[..., None] * color * strength, 0, 1)


def stroke_mask(W, H, seed, n, thick0, draw_fn):
    c = SS(W, H, 1)
    draw_fn(c, np.random.default_rng(seed))
    return np.asarray(c.done())[..., 3] / 255.0


def grow_root(c, rng, x, y, a, l, w, curl=0.35, depth=0):
    """Raíz o rama: se va curvando y bifurcando. Da siluetas orgánicas, no líneas."""
    steps = int(l / 26)
    for i in range(steps):
        a += (rng.random() - 0.5) * curl
        nx, ny = x + math.cos(a) * 26, y + math.sin(a) * 26
        ww = max(2.0, w * (1 - i / max(steps, 1) * 0.72))
        c.line([(x, y), (nx, ny)], (255, 255, 255, 255), ww)
        c.circle(nx, ny, ww * 0.5, fill=(255, 255, 255, 255))
        x, y = nx, ny
        if depth < 3 and rng.random() < 0.13:
            grow_root(c, rng, x, y, a + (rng.random() - 0.5) * 2.0, l * 0.55, ww * 0.62, curl, depth + 1)


def bark_shade(mask, seed, tint_a, tint_b, relief=30.0, spec=0.14):
    """Convierte una silueta en madera en relieve con textura real (Rock022 rotada)."""
    H, W = mask.shape
    tex = rot_tile(cc0("Rock022", "Color", 1024)).mean(axis=2)
    nrm = rot_tile(cc0("Rock022", "NormalGL", 1024))
    t = tile_to(tex, H, W)
    n = tile_to(nrm, H, W)
    t = np.clip((t - t.mean()) * 2.4 + 0.5, 0, 1)
    height = ndi.gaussian_filter(mask, 14) * 1.0 + ndi.gaussian_filter(mask, 5) * 0.4
    alb = lerp(tint_a, tint_b, t)
    return np.clip(shade(height, alb, n, 0.65, relief, spec, 18, 0.5, 0.82), 0, 1)


def over(rgb, top, a):
    return lerp(rgb, top, np.clip(a, 0, 1))


def ground_soil(W, H, hor, tint_a, tint_b, tex_id="Rock030", su=1.0, sv=1.0, fog=None):
    tex = cc0(tex_id, "Color", 1024).mean(axis=2)
    nrm = cc0(tex_id, "NormalGL", 1024)
    t, depth = persp_map(tex, W, H, hor, su, sv)
    nz, _ = persp_map(nrm, W, H, hor, su, sv)
    t = np.clip((t - tex.mean()) * 2.2 + 0.5, 0, 1)
    rgb = lerp(tint_a, tint_b, t)
    rgb = rgb * (0.7 + 0.5 * np.clip((nz[..., 1] - nz[..., 0]) * 1.2 + 0.4, 0, 1))[..., None]
    rgb = rgb * (0.55 + 0.65 * smooth(0, 0.7, depth))[..., None]
    if fog is not None:
        rgb = lerp(rgb, fog, (1 - smooth(0, 0.28, depth)) * 0.85)
    return rgb, depth


def vignette(rgb, amt=0.6):
    H, W, _ = rgb.shape
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    v = np.hypot((xx - W / 2) / W, (yy - H * 0.55) / H)
    return rgb * (1 - smooth(0.34, 0.8, v)[..., None] * amt)


def stage_salinas():
    """Salinas al anochecer: la tierra de las brujas."""
    W, H = BW, BH
    sky_ = np.asarray(Image.open(os.path.join(OUT, "bg", "home_sky.png")).convert("RGB").resize((W, int(W * 1440 / 2560)), Image.LANCZOS)) / 255.0
    im = Image.fromarray((sky_ * 255).astype(np.uint8)).convert("RGBA")
    for n in ("home_far", "home_mid"):
        im.alpha_composite(Image.open(os.path.join(OUT, "bg", n + ".png")).convert("RGBA").resize((W, int(W * 1440 / 2560)), Image.LANCZOS))
    a = np.asarray(im.convert("RGB")) / 255.0
    # encuadre: el horizonte del arte está a 0.71; aquí debe caer en GROUND
    top = int(a.shape[0] * 0.71 - H * GROUND)
    a = a[max(0, top):max(0, top) + H]
    if a.shape[0] < H:
        a = np.vstack([a, np.repeat(a[-1:], H - a.shape[0], 0)])
    a = a * np.array([0.72, 0.68, 0.78])
    # terreno de combate: costra en primer plano, cuarteada
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    hor = H * GROUND
    u, z, _, _ = persp(W, H, hor, 44.0)
    gap, cid = cellular(u * 1.9, z * 0.72, 77)
    crust = lerp(hx("c4a8a8"), hx("efe2da"), cid * 0.7 + 0.15) * (0.55 + 0.45 * smooth(0, 0.08, gap))[..., None]
    t = np.clip((yy - hor) / (H - hor), 0, 1)
    pool = smooth(0.48, 0.56, fit(noise(H // 2, W // 2, 120, 78, 5), H, W))
    # el centro queda seco para leer a las figuras
    center = np.exp(-(((xx - W * 0.5) / (W * 0.42)) ** 2)) * smooth(0.25, 0.55, t)
    pool = pool * (1 - center)
    keep = lerp(crust, a, pool)
    a = lerp(a, keep, smooth(hor + 2, hor + 60, yy))
    return np.clip(vignette(a, 0.62), 0, 1)


def stage_raices():
    """Raíces del Fresno: gruta bajo la tierra, raíces vivas y esporas. Pavor sin gore."""
    W, H = BW, BH
    hor = H * GROUND
    g = grad(H, W, [(0, hx("0b0614")), (0.4, hx("1d0f2c")), (0.6, hx("3a1d3e")), (1, hx("0d0810"))])
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    # boca de la gruta: luz violeta y ámbar al fondo
    glow = np.exp(-(((xx - W * 0.52) / (W * 0.30)) ** 2 + ((yy - hor + 40) / (H * 0.22)) ** 2))
    g = g + glow[..., None] * hx("b0569e") * 0.5
    glow2 = np.exp(-(((xx - W * 0.66) / (W * 0.10)) ** 2 + ((yy - hor + 20) / (H * 0.10)) ** 2))
    g = g + glow2[..., None] * hx("ffb066") * 0.55
    # roca del fondo
    rock = tile_to(cc0("Rock030", "Color", 1024), H, W).mean(axis=2, keepdims=True)
    g = g * (0.55 + 1.0 * np.clip((rock - rock.mean()) * 2.2 + 0.5, 0, 1))
    # tres planos de raíces, de lejos a cerca: cada uno más oscuro y más grande
    for layer, (seed, n, wd, ta, tb, haze) in enumerate([
        (801, 15, 34, "2b1834", "4d2a44", 0.5),
        (802, 12, 66, "1a0f1e", "3c2228", 0.2),
        (803, 8, 118, "120a12", "2e1a1c", 0.0),
    ]):
        def draw(c, rng):
            for i in range(n):
                edge = rng.random()
                x0 = W * (i + rng.random()) / n
                if edge < 0.55:
                    grow_root(c, rng, x0, -30, math.pi / 2 + (rng.random() - 0.5) * 0.9, H * (0.5 + rng.random() * 0.4), wd)
                else:
                    side = rng.random() < 0.5
                    grow_root(c, rng, -30 if side else W + 30, H * rng.random() * 0.65, (0 if side else math.pi) + (rng.random() - 0.5), W * 0.32, wd)

        m = stroke_mask(W, H, seed, n, wd, draw)
        wood = bark_shade(m, seed, hx(ta), hx(tb))
        wood = lerp(wood, hx("7a3d7a"), haze * 0.6)
        g = over(g, wood, m)
    # venas de savia luminosa en lo cercano
    n1 = noise(H, W, 260, 804, 5)
    vein = (1 - smooth(0.0, 0.012, np.abs(n1 - 0.5))) * smooth(0.4, 0.2, yy / H + 0.0)
    g = g + vein[..., None] * hx("ffb066") * 0.5
    soil, depth = ground_soil(W, H, hor, hx("120a10"), hx("4a2e2c"), "Rock030", 1.2, 1.1, hx("3a1d3e"))
    g = over(g, soil, smooth(hor - 2, hor + 26, yy + (noise(1, W, 140, 805, 4)[0][None, :] - 0.5) * 30))
    # charcos de sal turbia que brillan en el suelo
    pool = smooth(0.62, 0.68, fit(noise(H // 2, W // 2, 90, 806, 4), H, W)) * smooth(hor + 20, hor + 120, yy) * (1 - np.exp(-(((xx - W * 0.5) / (W * 0.4)) ** 2)) * smooth(0.2, 0.6, depth))
    g = over(g, hx("7c2448") + (noise(H, W, 18, 807, 2)[..., None] - 0.5) * 0.12, pool * 0.75)
    g = spores(g, 520, 808, hx("ffc48a"), 0.9, 2.4, 0.9)
    g = spores(g, 130, 809, hx("c88cff"), 0.8, 2.2, 0.45)
    return np.clip(vignette(g, 0.72), 0, 1)


def stage_tronco():
    """Tronco: dentro del Fresno. Corteza en todas partes, luz que entra por las grietas."""
    W, H = BW, BH
    hor = H * GROUND
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    tex = rot_tile(cc0("Rock022", "Color", 1024)).mean(axis=2)
    nrm = rot_tile(cc0("Rock022", "NormalGL", 1024))
    # pared curva: la textura se comprime hacia los lados como un cilindro interior
    th = (xx / W - 0.5) * 2
    uu = (np.arcsin(np.clip(th, -1, 1)) * 800).astype(np.int64) % 1024
    vv = (yy * 0.9).astype(np.int64) % 1024
    t = np.clip((tex[vv, uu] - tex.mean()) * 2.6 + 0.5, 0, 1)
    n = nrm[vv, uu]
    wall = lerp(hx("1c1210"), hx("7a5236"), t)
    wall = np.clip(shade(np.zeros((H, W)), wall, n, 0.9, 0, 0.12, 14, 0.52, 0.75), 0, 1)
    wall = wall * (0.35 + 0.85 * (1 - np.abs(th) ** 1.6))[..., None]
    # grieta de luz al fondo y conos que caen
    crack = np.exp(-((xx - W * 0.58 - np.sin(yy / 90) * 16 - (noise(H, 1, 90, 811, 4)[:, 0][:, None] - 0.5) * 120) / 22) ** 2) * smooth(hor, hor * 0.2, yy)
    wall = wall + crack[..., None] * hx("ffe2a8") * 1.1
    for x, tw, bw in [(0.58, 24, 300), (0.30, 16, 220), (0.8, 14, 180)]:
        wall = wall + light_cone(W, H, W * x, tw, bw, 0, H * 0.95)[..., None] * hx("ffcf8a") * 0.32
    # anillos del árbol en el suelo: pisas el corte de una rama
    cx, cy = W * 0.5, H * 1.25
    d = np.hypot((xx - cx) / 2.6, (yy - cy) * 1.0)
    ringt = (np.sin(d * 0.16 + noise(H, W, 170, 812, 3) * 6) * 0.5 + 0.5)
    tex2, depth = persp_map(tex, W, H, hor, 0.8, 0.8)
    wood = lerp(hx("3a2418"), hx("a0764e"), ringt * 0.55 + np.clip((tex2 - tex.mean()) * 1.4 + 0.25, 0, 1) * 0.45)
    wood = wood * (0.55 + 0.65 * smooth(0, 0.7, depth))[..., None]
    wood = lerp(wood, hx("6e4a2e"), (1 - smooth(0, 0.22, depth)) * 0.7)
    edge = hor + (noise(1, W, 200, 813, 4)[0][None, :] - 0.5) * 24
    g = over(wall, wood, smooth(-2, 2, yy - edge))
    # sal cristalizada creciendo en la base de la pared
    crust = smooth(0.56, 0.68, noise(H, W, 40, 814, 4)) * np.exp(-((yy - edge) / 46) ** 2)
    gap, cid = cellular(xx / 15.0, yy / 12.0, 815)
    g = over(g, lerp(hx("c9b4ab"), hx("fff6ee"), cid) * (0.6 + 0.4 * smooth(0, 0.1, gap))[..., None], crust)
    g = spores(g, 420, 816, hx("ffe2a8"), 0.85, 2.0, 0.9)
    return np.clip(vignette(g, 0.62), 0, 1)


def stage_ramas():
    """Ramas: por encima de las nubes, sobre una rama ancha como un camino."""
    W, H = BW, BH
    hor = H * 0.55
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    g = grad(H, W, [(0, hx("20305e")), (0.3, hx("4a4f88")), (0.52, hx("e6a487")), (0.62, hx("f7d7b4")), (1, hx("f7d7b4"))])
    n = fit(noise(H // 2, W // 8, 70, 821, 6), H, W)
    cl = ndi.gaussian_filter(smooth(0.48, 0.72, n), 3) * smooth(0.12, 0.4, yy / H)
    soft = ndi.gaussian_filter(cl, 4)
    lit = np.clip(soft - np.roll(soft, -14, axis=0), 0, 1) * 4
    g = lerp(g, hx("f6ece6"), cl * 0.85)
    g = lerp(g, hx("8a6e96"), np.clip(np.roll(soft, 18, 0) - soft, 0, 1) * 1.6 * 0.5)
    g = lerp(g, hx("fff0dc"), np.clip(lit, 0, 1) * 0.6)
    # mar de nubes abajo
    sea = smooth(0.46, 0.62, fit(noise(H // 2, W // 4, 60, 822, 6), H, W) + (yy / H - 0.5) * 0.6)
    g = lerp(g, hx("f3dfd2"), sea * smooth(hor - 30, hor + 40, yy))
    # ramas lejanas y cercanas
    for seed, n_, wd, ta, tb, haze in [(823, 7, 26, "6b5272", "8a6a7e", 0.5), (824, 5, 60, "2b1a20", "5c3c30", 0.05)]:
        def draw(c, rng):
            for i in range(n_):
                side = rng.random() < 0.5
                grow_root(c, rng, -40 if side else W + 40, H * rng.random() * 0.5, (rng.random() * 0.6 - 0.5) if side else (math.pi + rng.random() * 0.6 - 0.1), W * 0.5, wd, 0.25)

        m = stroke_mask(W, H, seed, n_, wd, draw)
        g = over(g, lerp(bark_shade(m, seed, hx(ta), hx(tb)), hx("c8a0a8"), haze), m)
    # la rama donde se pelea: ancha, con corteza y curva hacia los lados
    top = H * 0.60 + ((xx / W - 0.5) ** 2) * H * 0.24
    m = smooth(-2, 2, yy - top)
    tex = rot_tile(cc0("Rock022", "Color", 1024)).mean(axis=2)
    nrm = rot_tile(cc0("Rock022", "NormalGL", 1024))
    z = 900.0 / (np.maximum(yy - top, 1) + 60)
    uu = ((xx - W / 2) * z * 0.5).astype(np.int64) % 1024
    vv = (z * 460).astype(np.int64) % 1024
    t = np.clip((tex[vv, uu] - tex.mean()) * 2.6 + 0.5, 0, 1)
    br = lerp(hx("2c1a14"), hx("9c7050"), t)
    br = np.clip(shade(np.zeros((H, W)), br, nrm[vv, uu], 1.0, 0, 0.1, 14, 0.56, 0.78), 0, 1)
    br = br * (0.72 + 0.4 * smooth(0, 200, yy - top))[..., None]
    br = lerp(br, hx("7e6a4a"), np.exp(-((yy - top) / 18) ** 2) * 0.5)
    g = over(g, br, m)
    # hojas de sal en el borde
    g = spores(g, 180, 825, hx("fff0d8"), 0.8, 2.6, 0.6)
    return np.clip(vignette(g, 0.45), 0, 1)


def stage_copa():
    """Copa: el cielo está demasiado cerca. Hojas de oro, constelaciones enormes y brasas que suben."""
    W, H = BW, BH
    hor = H * GROUND
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    g = grad(H, W, [(0, hx("090818")), (0.35, hx("1a1642")), (0.58, hx("5a2c5a")), (0.66, hx("e0955e")), (1, hx("e0955e"))])
    rng = np.random.default_rng(831)
    st = np.zeros((H, W))
    ys = (rng.random(2600) ** 1.4 * H * 0.55).astype(int)
    xs = rng.integers(0, W, 2600)
    st[ys, xs] = rng.random(2600) ** 3 * 3
    st = ndi.gaussian_filter(st, 0.9) * 3 + ndi.gaussian_filter(st, 3) * 3
    g = g + st[..., None] * hx("ffefd0")
    # rueda natal enorme en el cielo
    wheel = astrolabe(1500, 3, True, 8)
    wa = np.zeros((H, W))
    arr = np.asarray(wheel)[..., 3] / 255.0
    x0, y0 = int(W * 0.5 - 750), int(H * 0.28 - 750)
    y1 = max(0, y0)
    wa[y1:y0 + 1500, x0:x0 + 1500] = arr[y1 - y0:, :]
    g = g + wa[..., None] * hx("ffd98a") * 0.32
    # dosel de hojas de oro, en bokeh, a los lados
    for seed, cnt, sz, a_ in [(832, 170, 18, 0.5), (833, 90, 34, 0.8)]:
        r2 = np.random.default_rng(seed)
        layer = np.zeros((H, W))
        for _ in range(cnt):
            side = r2.random() < 0.5
            x = (r2.random() * 0.26 if side else 1 - r2.random() * 0.26) * W
            y = r2.random() ** 1.5 * H * 0.75
            layer[int(min(y, H - 1)), int(min(x, W - 1))] = r2.random() + 0.4
        s = ndi.gaussian_filter(layer, sz)
        s = np.clip(s * sz * sz * 6.28, 0, 1)
        g = lerp(g, hx("e9b65a") * (0.72 + 0.4 * noise(H, W, 90, seed + 9, 3)[..., None]), s * a_)
    # ramas que sostienen la copa
    def draw(c, rg):
        for i in range(9):
            side = i % 2 == 0
            grow_root(c, rg, -30 if side else W + 30, H * rg.random() * 0.62, (rg.random() * 0.5 - 0.1) if side else (math.pi - rg.random() * 0.5 + 0.1), W * 0.34, 60, 0.3)

    m = stroke_mask(W, H, 834, 9, 60, draw)
    g = over(g, bark_shade(m, 834, hx("1f120c"), hx("6e4a2a")), m)
    # suelo: ramas tejidas con flores de sal
    soil, depth = ground_soil(W, H, hor, hx("2a1810"), hx("a07844"), "Rock022", 0.9, 0.9, hx("d0895a"))
    edge = hor + (noise(1, W, 140, 835, 4)[0][None, :] - 0.5) * 28
    g = over(g, soil, smooth(-2, 2, yy - edge))
    bloom = smooth(0.66, 0.72, fit(noise(H // 2, W // 2, 34, 836, 3), H, W)) * smooth(hor + 10, hor + 90, yy) * (1 - np.exp(-(((xx - W * 0.5) / (W * 0.34)) ** 2)) * 0.7)
    gap, cid = cellular(xx / 10.0, yy / 8.0, 837)
    g = over(g, lerp(hx("e6cfc0"), hx("fff6ec"), cid), bloom)
    g = spores(g, 520, 838, hx("ffcf7a"), 1.0, 2.6, 1.0)
    return np.clip(vignette(g, 0.55), 0, 1)


def make_battle():
    for name, fn in [("salinas", stage_salinas), ("raices", stage_raices), ("tronco", stage_tronco), ("ramas", stage_ramas), ("copa", stage_copa)]:
        rgb = fn()
        save(np.clip(rgb, 0, 1), "battle", name)


# ---------------------------------------------------------------------------------------------- Enemigos


def eyes_constellation(c, pts, r=7):
    for x, y in pts:
        c.circle(x, y, r * 2.6, fill=(255, 190, 110, 50))
        c.circle(x, y, r * 1.5, fill=(255, 170, 90, 150))
        c.circle(x, y, r * 0.7, fill=(255, 245, 210, 255))


def finish_enemy(mask, albedo_a, albedo_b, tex="Rock030", relief=46, spec=0.3, gloss=20, seed=0):
    H, W = mask.shape
    t = tile_to(cc0(tex, "Color", 1024).mean(axis=2), H, W)
    n = tile_to(cc0(tex, "NormalGL", 1024), H, W)
    t = np.clip((t - t.mean()) * 2.4 + 0.5, 0, 1)
    he = ndi.gaussian_filter(mask, 18) * 1.0 + ndi.gaussian_filter(mask, 6) * 0.5
    rgb = shade(he, lerp(albedo_a, albedo_b, t), n, 0.8, relief, spec, gloss, 0.5, 0.95)
    # luz de contorno, la misma de las brujas
    edge = np.clip(np.roll(mask, 6, 1) - mask, 0, 1)
    edge2 = np.clip(mask - np.roll(ndi.gaussian_filter(mask, 2), -5, 1), 0, 1)
    rgb = rgb + edge2[..., None] * hx("ff9d80") * 0.5
    return np.clip(rgb, 0, 1)


def finalize(rgb, mask, eyes=None):
    im = to_img(np.dstack([rgb * mask[..., None], mask]))
    a = np.asarray(im).copy()
    # sin halo negro: deja color bajo el alfa
    a[..., :3] = (np.clip(rgb, 0, 1) * 255).astype(np.uint8)
    im = Image.fromarray(a, "RGBA")
    if eyes is not None:
        im.alpha_composite(eyes)
    return im


def cube_cluster(c, cubes, W_=(255, 255, 255, 255)):
    for x, y, s in cubes:
        c.poly([(x, y - s), (x + s * .9, y - s * .5), (x + s * .9, y + s * .5), (x, y + s), (x - s * .9, y + s * .5), (x - s * .9, y - s * .5)], fill=W_)


def leaf(c, x, y, ang, ln, wd, fill):
    """Pluma u hoja: un huso afilado desde (x, y) en la dirección ang."""
    ca, sa = math.cos(ang), math.sin(ang)
    px, py = -sa, ca
    pts = []
    for i in range(9):
        t = i / 8
        w = wd * math.sin(min(1, t * 1.15) * math.pi) * (1 - t * 0.35)
        pts.append((x + ca * ln * t + px * w, y + sa * ln * t + py * w))
    for i in range(8, -1, -1):
        t = i / 8
        w = wd * math.sin(min(1, t * 1.15) * math.pi) * (1 - t * 0.35) * 0.55
        pts.append((x + ca * ln * t - px * w, y + sa * ln * t - py * w))
    c.poly(pts, fill=fill)


def enemy_golem():
    """Cristalizado: lo que queda cuando la sal termina de comerse a alguien. Una figura encorvada hecha de cubos,
    nunca agujas, con el puño a ras del suelo y una brasa rosa entre las juntas."""
    S = 1100
    rng = np.random.default_rng(901)
    # esqueleto: segmentos (x0, y0, x1, y1, tamaño de cubo, densidad)
    bones = [
        (450, 1040, 470, 760, 54, 9), (640, 1040, 615, 760, 54, 9),        # piernas
        (470, 760, 640, 760, 70, 6),                                       # cadera
        (520, 740, 470, 470, 92, 14), (600, 740, 590, 470, 92, 14),         # torso ancho
        (330, 470, 720, 450, 80, 12),                                      # hombros
        (420, 430, 390, 330, 60, 5),                                       # cuello, adelantado
        (330, 470, 250, 720, 58, 8), (250, 720, 230, 930, 52, 6),           # brazo delantero, hacia las brujas
        (720, 450, 790, 680, 56, 8), (790, 680, 770, 860, 50, 6),           # brazo trasero
    ]
    cubes = []
    for x0, y0, x1, y1, s, n in bones:
        for i in range(n):
            t = (i + rng.random() * 0.6) / n
            cubes.append((x0 + (x1 - x0) * t + (rng.random() - 0.5) * s * 0.9, y0 + (y1 - y0) * t + (rng.random() - 0.5) * s * 0.6, s * (0.55 + rng.random() * 0.6)))
    for (cx, cy, s) in [(370, 300, 76), (330, 290, 52), (400, 255, 48), (230, 960, 88), (770, 890, 74)]:  # cabeza y puños
        cubes.append((cx, cy, s))
    for _ in range(26):  # esquirlas sueltas al pie
        cubes.append((300 + rng.random() * 560, 1000 + rng.random() * 60, 10 + rng.random() * 22))
    cubes.sort(key=lambda q: q[1] + q[2] * 0.2)
    m = mask_of(lambda c: cube_cluster(c, cubes), S, S, 2)
    # brasa interior: se ve por las juntas, antes de pintar los cubos
    glow = ndi.gaussian_filter(m, 26)[..., None] * hx("ff8a8a") * 1.2
    c = SS(S, S, 2)
    for x, y, s in cubes:
        v = 0.78 + rng.random() * 0.22
        rim = 0.9 + 0.1 * rng.random()
        t_ = rgba(np.clip(hx("fff6f0") * v, 0, 1))
        l_ = rgba(np.clip(hx("d4aeb0") * v, 0, 1))
        r_ = rgba(np.clip(hx("6e4a62") * v, 0, 1))
        k = 0.86  # junta: el cubo pintado es un poco más chico que su silueta
        c.poly([(x, y - s * k), (x + s * .9 * k, y - s * .5 * k), (x, y), (x - s * .9 * k, y - s * .5 * k)], fill=t_)
        c.poly([(x - s * .9 * k, y - s * .5 * k), (x, y), (x, y + s * k), (x - s * .9 * k, y + s * .5 * k)], fill=l_)
        c.poly([(x + s * .9 * k, y - s * .5 * k), (x, y), (x, y + s * k), (x + s * .9 * k, y + s * .5 * k)], fill=r_)
        c.line([(x - s * .9 * k, y - s * .5 * k), (x, y - s * k), (x + s * .9 * k, y - s * .5 * k)], (255, 255, 255, int(150 * rim)), 1.4)
    faces = np.asarray(c.done()).astype(float) / 255.0
    fa = faces[..., 3]
    rgb = lerp(np.clip(glow, 0, 1) * 0.9 + hx("3a2030") * 0.5, faces[..., :3], fa)
    # textura de sal sobre las caras y oclusión hacia abajo
    t = tile_to(cc0("Rock030", "Color", 1024).mean(axis=2), S, S)
    rgb = rgb * (0.86 + 0.28 * (t - t.mean()))[..., None]
    yy = np.mgrid[0:S, 0:S][0].astype(float)
    rgb = rgb * (1 - 0.22 * smooth(500, 1080, yy))[..., None]
    e = SS(S, S, 2)
    eyes_constellation(e, [(340, 290), (385, 305), (362, 335)], 9)
    eyes_constellation(e, [(540, 560), (600, 600), (520, 640)], 7)
    return finalize(np.clip(rgb, 0, 1), m, e.done())


def enemy_root():
    """Raíz viva, sin rostro: la Madre Raíz y el Guardián. Nunca se ve entera; solo lo que sale del suelo."""
    S = 1200
    def draw(c):
        rng = np.random.default_rng(911)
        for i in range(11):
            x = 600 + (i - 5) * 66 + (rng.random() - 0.5) * 30
            grow_root(c, rng, x, S + 20, -math.pi / 2 + (i - 5) * 0.09 + (rng.random() - 0.5) * 0.2, S * (0.82 - abs(i - 5) * 0.06), 86 - abs(i - 5) * 8, 0.42)

    m = mask_of(draw, S, S, 1)
    rgb = bark_shade(m, 911, hx("1a0f18"), hx("6c4252"), 30, 0.2)
    n = fit(noise(S // 2, S // 2, 90, 912, 4), S, S)
    vein = (1 - smooth(0.0, 0.016, np.abs(n - 0.5))) * m
    rgb = rgb + vein[..., None] * hx("ff9d55") * 0.75
    e = SS(S, S, 1)
    eyes_constellation(e, [(560, 300), (660, 260), (610, 400), (500, 420), (720, 400), (610, 520), (480, 560), (740, 570)], 12)
    return finalize(rgb, m, e.done())


def feather(c, x, y, ang, ln, wd, fill):
    """Pluma de ala: hoja ancha de punta roma, no aguja."""
    ca, sa = math.cos(ang), math.sin(ang)
    px, py = -sa, ca
    side_a, side_b = [], []
    for i in range(13):
        t = i / 12
        w = wd * min(1.0, t * 3.5 + 0.25) * (1 - t ** 5 * 0.85)
        side_a.append((x + ca * ln * t + px * w * 0.5, y + sa * ln * t + py * w * 0.5))
        side_b.append((x + ca * ln * t - px * w * 0.5, y + sa * ln * t - py * w * 0.5))
    c.poly(side_a + side_b[::-1], fill=fill)


def crow_feathers():
    """Plumas de la corneja: (x, y, ángulo, largo, ancho, capa). Capa 0 detrás del cuerpo, 1 delante."""
    rng = np.random.default_rng(921)
    f = []
    # ala lejana, asomando detrás y arriba
    for i in range(8):
        t = i / 7
        f.append((560 + t * 90, 460 - t * 10, math.radians(-100 + t * 50), 200 + 120 * t, 60, 0))
    # cola, abajo y atrás (derecha), plegada
    for i in range(7):
        t = i / 6
        f.append((690, 630 + t * 18, math.radians(14 + t * 22), 240 + rng.random() * 20, 60, 0))
    # ala cercana, alzada y barrida hacia atrás: las primarias, largas, apuntan arriba a la derecha
    for i in range(14):
        t = i / 13
        bx, by = 470 + t * 210, 470 + t * 20
        a = math.radians(-84 + t * 66)
        ln = 190 + 210 * t ** 0.8
        f.append((bx, by, a, ln + rng.random() * 20, 74 - t * 12, 1))
    return f


def enemy_crow():
    """Corneja del eclipse: cuervo grande de perfil hacia las brujas (izquierda), con el ala alzada en abanico como
    amenaza. Plumas largas con borde iridiscente, pico de hueso viejo y un solo ojo de eclipse."""
    S = 1024
    W_ = (255, 255, 255, 255)
    F = crow_feathers()

    def body_shape(c):
        # cuerpo inclinado: la cabeza arriba a la izquierda, la cola abajo a la derecha
        cx, cy, rx, ry, rot = 545, 575, 235, 140, math.radians(-24)
        pts = []
        for i in range(48):
            a = i / 48 * math.tau
            x, y = math.cos(a) * rx, math.sin(a) * ry * (1.12 if math.sin(a) > 0 else 1)
            pts.append((cx + x * math.cos(rot) - y * math.sin(rot), cy + x * math.sin(rot) + y * math.cos(rot)))
        c.poly(pts, fill=W_)

    def head(c):
        c.poly([(320, 380), (420, 330), (480, 430), (430, 530), (330, 480)], fill=W_)  # cuello
        c.circle(360, 362, 108, fill=W_)

    def beak(c):
        c.poly([(290, 322), (200, 340), (128, 392), (176, 394), (226, 388), (292, 406)], fill=W_)

    def legs(c):
        for x0, x1 in ((520, 505), (590, 588)):
            c.line([(x0, 690), (x1, 830)], W_, 17)
            for k in (-1, 0, 1):
                c.line([(x1, 830), (x1 - 40 + k * 32, 862)], W_, 10)

    def draw(layer):
        def d(c):
            if layer in (0, None):
                for x, y, a, ln, wd, lay in F:
                    if lay == 0:
                        feather(c, x, y, a, ln, wd, W_)
                legs(c)
            if layer in (1, None):
                body_shape(c)
                beak(c)
            if layer in (2, None):
                for x, y, a, ln, wd, lay in reversed(F):
                    if lay == 1:
                        feather(c, x, y, a, ln, wd, W_)
            if layer in (3, None):
                head(c)
                beak(c)
        return d

    m = mask_of(draw(None), S, S, 2)
    back = mask_of(draw(0), S, S, 2)
    wing = mask_of(draw(2), S, S, 2)
    hd = mask_of(draw(3), S, S, 2)
    wing = wing * (1 - hd)

    def lines(c):
        for x, y, a, ln, wd, lay in F:
            c.line([(x, y), (x + math.cos(a) * ln * 0.96, y + math.sin(a) * ln * 0.96)], W_, 4)
    ln_m = ndi.gaussian_filter(mask_of(lines, S, S, 2), 1.4)

    rgb = finish_enemy(m, hx("07050b"), hx("2c2342"), "Leather030", 46, 0.55, 30)
    # el ala cercana se ilumina como una capa propia: borde de luz arriba y sombra sobre el cuerpo
    wing_h = ndi.gaussian_filter(wing, 10)
    wing_rgb = shade(wing_h * 60 + ln_m * 6, np.full((S, S, 3), hx("17121f")), None, 0, 1.0, 0.6, 26, 0.55, 0.95)
    shadow = np.clip(ndi.gaussian_filter(wing, 16) - wing, 0, 1)
    rgb = rgb * (1 - 0.45 * shadow[..., None])
    rgb = lerp(rgb, wing_rgb, wing)
    rgb = lerp(rgb, rgb * 0.62, back * (1 - wing))  # lo de atrás, más oscuro
    yy, xx = np.mgrid[0:S, 0:S].astype(float)
    ir = (np.sin(xx * 0.02 - yy * 0.016) * 0.5 + 0.5)[..., None]
    sheen = (ir * hx("7a62de") + (1 - ir) * hx("2f9aa8")) * m[..., None]
    rgb = rgb + sheen * (0.08 + 0.3 * ln_m[..., None])
    rgb = rgb * (1 - 0.3 * ln_m[..., None])
    bk = mask_of(beak, S, S, 2)
    beak_rgb = shade(ndi.gaussian_filter(bk, 7) * 50, lerp(np.full((S, S, 3), hx("2a2224")), np.full((S, S, 3), hx("8a7a70")), smooth(400, 300, yy)[..., None] * 0.6), None, 0, 1.0, 0.7, 34, 0.5, 0.9)
    rgb = lerp(rgb, beak_rgb, bk)
    e = SS(S, S, 2)
    eyes_constellation(e, [(330, 340)], 15)
    return finalize(np.clip(rgb, 0, 1), m, e.done())


def enemy_husk():
    """Lo hueco del Fresno: la Corteza Hueca y el Leñador. Una figura sin interior, con un vacío por pecho."""
    S = 1100
    def draw(c):
        W_ = (255, 255, 255, 255)
        rng = np.random.default_rng(931)
        pts = []
        for i in range(28):
            t = i / 27
            pts.append((390 - 90 * math.sin(t * 2.6) + rng.random() * 24, 120 + t * 960))
        for i in range(28):
            t = 1 - i / 27
            pts.append((710 + 110 * math.sin(t * 2.6) + rng.random() * 24, 120 + t * 960))
        c.poly(pts, fill=W_)
        c.poly([(330, 260), (120, 560), (170, 610), (340, 420)], fill=W_)
        c.poly([(770, 260), (990, 520), (940, 590), (760, 420)], fill=W_)
        c.circle(550, 150, 130, fill=W_)
        # astas secas
        for sg, x in ((-1, 470), (1, 630)):
            grow_root(c, rng, x, 80, -math.pi / 2 + sg * 0.5, 240, 22, 0.45)

    m = mask_of(draw, S, S, 1)
    # el vacío del pecho
    hole = mask_of(lambda c: c.poly([(550, 330), (660, 480), (620, 680), (540, 760), (470, 660), (450, 470)], fill=(255, 255, 255, 255)), S, S, 1)
    hole = ndi.gaussian_filter(hole, 5)
    rgb = bark_shade(m, 931, hx("20140f"), hx("86603f"), 34, 0.14)
    rgb = lerp(rgb, hx("070408"), hole)
    rgb = rgb + (ndi.gaussian_filter(hole, 30) * (1 - hole))[..., None] * hx("ff9d55") * 0.7
    e = SS(S, S, 1)
    eyes_constellation(e, [(510, 150), (600, 165), (548, 510), (500, 600), (590, 610)], 10)
    return finalize(np.clip(rgb, 0, 1), m, e.done())


def enemy_wisp():
    """Lo que flota: savia, viento, recuerdos. Una voluta con ojos."""
    S = 1024
    yy, xx = np.mgrid[0:S, 0:S].astype(float)
    n = noise(S, S, 200, 941, 6)
    n2 = noise(S, S, 60, 942, 4)
    r = np.hypot((xx - 512) / 330, (yy - 480) / 430)
    body = smooth(1.05, 0.4, r + (n - 0.5) * 0.9)
    tail = smooth(0.0, 0.4, (yy - 500) / 500) * smooth(0.5, 0.2, np.abs(xx - 512 - np.sin(yy / 70) * 60) / 400 + (n2 - 0.5) * 0.5)
    m = np.clip(np.maximum(body, tail), 0, 1)
    he = ndi.gaussian_filter(m, 22)
    rgb = shade(he + n * 0.2, lerp(hx("2c1a46"), hx("c49ae0"), smooth(0.25, 0.9, n2 * 0.6 + (1 - r) * 0.5)), None, 0, 80, 0.5, 18, 0.6, 0.9)
    core = np.exp(-(r / 0.42) ** 2)
    rgb = rgb + core[..., None] * hx("ffb0d8") * 0.35
    e = SS(S, S, 2)
    eyes_constellation(e, [(440, 420), (580, 400), (512, 520)], 11)
    return finalize(np.clip(rgb, 0, 1), m, e.done())


def enemy_larva():
    """Larva de la raíz: segmentos húmedos y abultados, la cabeza hacia las brujas (izquierda), con cilios."""
    S = 1024
    segs = []
    for i in range(11):
        t = i / 10
        x = 190 + t * 650
        y = 760 - math.sin(t * math.pi * 0.95) * 230 + t * 40
        r = 118 + 34 * math.sin(min(1, t * 1.6) * math.pi * 0.85) - t * 70
        segs.append((x, y, max(r, 46)))
    yy, xx = np.mgrid[0:S, 0:S].astype(float)
    hgt = np.zeros((S, S))
    sid = np.full((S, S), -1)
    for i, (x, y, r) in enumerate(reversed(segs)):  # la cabeza queda encima
        d = np.clip(1 - ((xx - x) ** 2 + (yy - y) ** 2) / (r * r), 0, 1)
        dome = np.sqrt(d) * r * 0.55
        on = d > 0
        hgt = np.where(on, dome, hgt)
        sid = np.where(on, len(segs) - 1 - i, sid)
    m = ndi.gaussian_filter((sid >= 0).astype(float), 1.0)
    t = tile_to(cc0("Leather037", "Color", 1024).mean(axis=2), S, S)
    nrm = tile_to(cc0("Leather037", "NormalGL", 1024), S, S)
    t = np.clip((t - t.mean()) * 2.2 + 0.5, 0, 1)
    base = lerp(hx("5a3044"), hx("d6aca6"), t * 0.5 + 0.3)
    base = lerp(base, hx("3a1e2c"), (sid % 2 == 1)[..., None] * 0.22)
    rgb = shade(ndi.gaussian_filter(hgt, 2), base, nrm, 0.4, 0.9, 0.85, 50, 0.4, 0.95)
    # pliegues oscuros entre segmentos y vientre en sombra
    crease = np.clip(1 - hgt / 14.0, 0, 1) * m
    rgb = rgb * (1 - 0.55 * crease[..., None])
    rgb = rgb + np.clip(ndi.gaussian_filter(hgt, 18) / 60.0, 0, 1)[..., None] * hx("ff9db0") * 0.08
    c = SS(S, S, 2)
    for x, y, r in segs[1:]:
        for k in (-0.3, 0.3):
            c.line([(x + k * r, y + r * 0.8), (x + k * r - 14, y + r * 0.8 + 46)], (40, 22, 30, 255), 6)
    legs = c.done()
    img = finalize(np.clip(rgb, 0, 1), m)
    out = Image.new("RGBA", img.size)
    out.alpha_composite(legs)
    out.alpha_composite(img)
    e = SS(S, S, 2)
    hx0, hy0, hr = segs[0]
    eyes_constellation(e, [(hx0 - hr * 0.55, hy0 - hr * 0.25), (hx0 - hr * 0.2, hy0 - hr * 0.5), (hx0 - hr * 0.35, hy0 + hr * 0.05)], 8)
    out.alpha_composite(e.done())
    return out


def make_enemies():
    for name, fn in [("golem", enemy_golem), ("raiz", enemy_root), ("cuervo", enemy_crow), ("hueco", enemy_husk), ("voluta", enemy_wisp), ("larva", enemy_larva)]:
        img = fn()
        # recorta para que el pie del cuadro sea el suelo
        bbox = img.getbbox()
        if bbox:
            img = img.crop(bbox)
        save(img, "enemigos", name)
    # Sombra de contacto y aro de turno, para el combate
    s = 256
    yy, xx = np.mgrid[0:s, 0:s].astype(float) - 127.5
    r = np.hypot(xx, yy) / 128
    save(to_img(np.dstack([np.zeros((s, s, 3)), np.clip(1 - r, 0, 1) ** 1.4 * 0.85])), "enemigos", "sombra")



# id de datos -> carpeta de arte. Los nombres de arte son los de docs/Retratos para Codex.md.
ART_IDS = {"zarigueya": "muertita", "topo": "topacia"}


def make_witches():
    """Copia los recortes al juego. Si Codex los regenera, basta volver a correr este grupo."""
    d = os.path.join(OUT, "brujas")
    os.makedirs(d, exist_ok=True)
    n = 0
    for art in sorted(os.listdir(RECORTES)):
        folder = os.path.join(RECORTES, art)
        if not os.path.isdir(folder):
            continue
        rid = {v: k for k, v in ART_IDS.items()}.get(art, art)
        for ph in ("f1", "f3"):
            for kind, src in (("cuerpo", f"{art}_{ph}_cuerpo.png"), ("busto", f"{art}_{ph}_busto_320.png")):
                p = os.path.join(folder, src)
                if os.path.exists(p):
                    shutil.copyfile(p, os.path.join(d, f"{rid}_{ph}_{kind}.png"))
                    n += 1
    print(f"  brujas: {n} archivos")


GROUPS = {"bg": make_bg, "chrome": make_chrome, "icons": icons, "gold": make_gold_icons, "fx": make_fx, "logo": make_logo, "cards": make_cards, "battle": make_battle, "enemies": make_enemies, "witches": make_witches}

if __name__ == "__main__":
    want = sys.argv[1:] or ["all"]
    for g, fn in GROUPS.items():
        if "all" in want or g in want:
            print(g)
            fn()
