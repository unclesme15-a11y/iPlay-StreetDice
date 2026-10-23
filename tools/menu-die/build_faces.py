import cv2, numpy as np, sys
from PIL import Image, ImageDraw, ImageFont
rawdir, fontpath, outdir = sys.argv[1], sys.argv[2], sys.argv[3]
N = 1024
BASE = np.array([224, 216, 199], np.float32)   # RGB off-white, sampled from the die photo

def fbm(rng, size, octaves=5, start=4):
    out = np.zeros((size,size), np.float32); amp = 1; tot = 0
    for o in range(octaves):
        res = start * 2**o
        n = rng.random((res,res)).astype(np.float32)
        out += cv2.resize(n, (size,size), interpolation=cv2.INTER_CUBIC) * amp
        tot += amp; amp *= 0.5
    return out / tot

def worn_surface(seed):
    rng = np.random.default_rng(seed)
    img = np.tile(BASE, (N,N,1))
    mott = fbm(rng, N) - 0.5
    img += mott[...,None] * np.array([16,16,18], np.float32)
    # faint hairline scratches
    scratches = np.zeros((N,N), np.float32)
    for _ in range(70):
        x0, y0 = rng.integers(0,N,2); ang = rng.uniform(0, np.pi); ln = rng.integers(30, 220)
        x1, y1 = int(x0 + np.cos(ang)*ln), int(y0 + np.sin(ang)*ln)
        cv2.line(scratches, (int(x0),int(y0)), (x1,y1), float(rng.uniform(0.15,0.45)), 1, cv2.LINE_AA)
    img -= scratches[...,None] * np.array([40,42,46], np.float32)
    # brown grime specks, thicker toward the edges like the real die
    yy, xx = np.mgrid[0:N,0:N].astype(np.float32) / N
    edge = np.maximum(np.abs(xx-0.5), np.abs(yy-0.5)) * 2
    grime = np.zeros((N,N), np.float32)
    for _ in range(620):
        x, y = rng.integers(0,N,2)
        if rng.random() > 0.25 + 0.75*edge[y,x]**3: continue
        r = rng.uniform(1.2, 4.5)
        cv2.circle(grime, (int(x),int(y)), int(r), float(rng.uniform(0.3,0.9)), -1, cv2.LINE_AA)
    grime = cv2.GaussianBlur(grime, (0,0), 1.4) * (0.6 + 0.8*fbm(rng, N))
    grime_col = np.array([128, 92, 58], np.float32)
    g = np.clip(grime, 0, 1)[...,None] * 0.75
    img = img*(1-g) + grime_col*g
    return np.clip(img, 0, 255)

def ink_from_photo(path):
    bgr = cv2.imread(path); rgb = cv2.cvtColor(bgr, cv2.COLOR_BGR2RGB).astype(np.float32)
    lum = rgb.mean(axis=2)
    a = np.clip((205 - lum) / (205 - 40), 0, 1)
    a[a < 0.12] = 0
    b = 22   # drop any bleed from the neighbouring face at the very edge
    a[:b,:] = 0; a[-b:,:] = 0; a[:,:b] = 0; a[:,-b:] = 0
    # keep the lettering on the flat part of the face: the rounded edge is the outer ~13%
    small = cv2.resize(a, None, fx=0.8, fy=0.8, interpolation=cv2.INTER_AREA)
    out = np.zeros_like(a); o = (N - small.shape[0]) // 2
    out[o:o+small.shape[0], o:o+small.shape[1]] = small
    return out

def brush_ink(lines, seed, cap_frac, max_w=0.72, shear=-0.12, stretch=1.3, max_h=0.62):
    rng = np.random.default_rng(seed)
    big = 2048
    ink = Image.new('L', (big,big), 0); d = ImageDraw.Draw(ink)
    size = int(big*cap_frac/0.72)
    font = ImageFont.truetype(fontpath, size)
    boxes = [d.textbbox((0,0), t, font=font, stroke_width=size//22) for t in lines]
    widest = max(b[2]-b[0] for b in boxes)
    if widest > big*max_w:
        size = int(size * big*max_w / widest); font = ImageFont.truetype(fontpath, size)
        boxes = [d.textbbox((0,0), t, font=font, stroke_width=size//22) for t in lines]
    gap = size*0.06
    total = sum(b[3]-b[1] for b in boxes) + gap*(len(lines)-1)
    y = (big-total)/2
    for t,b in zip(lines, boxes):
        d.text(((big-(b[2]-b[0]))/2 - b[0], y - b[1]), t, font=font, fill=255,
               stroke_width=size//22, stroke_fill=255)
        y += (b[3]-b[1]) + gap
    a = np.asarray(ink).astype(np.float32)/255
    # taller, more condensed letters like the brushed CRAPS: stretch vertically about the centre
    S = np.float32([[1,0,0],[0,stretch,-(stretch-1)*big/2]])
    a = cv2.warpAffine(a, S, (big,big), flags=cv2.INTER_LINEAR)
    # lean the letters like the hand-brushed CRAPS
    M = np.float32([[1, shear, -shear*big/2],[0,1,0]])
    a = cv2.warpAffine(a, M, (big,big), flags=cv2.INTER_LINEAR)
    # ragged brush edges: push the outline around with noise
    dx = (fbm(rng, big, 4, 16)-0.5) * 40; dy = (fbm(rng, big, 4, 16)-0.5) * 40
    gx, gy = np.meshgrid(np.arange(big, dtype=np.float32), np.arange(big, dtype=np.float32))
    a = cv2.remap(a, gx+dx, gy+dy, cv2.INTER_LINEAR)
    # dry-brush: streaks running along the stroke direction (mostly vertical strokes)
    streak = rng.random((big, big//10)).astype(np.float32)
    streak = cv2.resize(streak, (big,big), interpolation=cv2.INTER_LINEAR)
    streak = cv2.GaussianBlur(streak, (0,0), sigmaX=1.2, sigmaY=14)
    soft = cv2.GaussianBlur(a, (0,0), 3)
    edge_zone = np.clip((1 - soft) * 2.2, 0, 1)            # near the outline
    a = a * np.clip(1 - edge_zone * np.clip((0.62 - streak)*5, 0, 1), 0, 1)
    # fine grain/white specks inside the ink, like the photo
    grain = rng.random((big,big)).astype(np.float32)
    a = a * (1 - (grain > 0.985) * 0.9)
    a = cv2.resize(a, (N,N), interpolation=cv2.INTER_AREA)
    # fit the whole word inside the allowed box (flat part of the face)
    ys, xs = np.where(a > 0.05)
    if len(xs):
        w = (xs.max()-xs.min())/N; h = (ys.max()-ys.min())/N
        k = min(1.0, max_w/w, max_h/h)
        cx, cy = (xs.max()+xs.min())/2, (ys.max()+ys.min())/2
        M = np.float32([[k,0,N/2-k*cx],[0,k,N/2-k*cy]])
        a = cv2.warpAffine(a, M, (N,N), flags=cv2.INTER_AREA)
    return a

def compose(surface, alpha, ink=(24, 22, 21)):
    a = np.clip(alpha, 0, 1)[...,None]
    out = surface*(1-a) + np.array(ink, np.float32)*a
    return cv2.cvtColor(np.clip(out,0,255).astype(np.uint8), cv2.COLOR_RGB2BGR)

surfaces = {name: worn_surface(seed) for seed, name in enumerate(
    ['craps','ceelo','profile','settings','tutorial','logo'])}
cv2.imwrite(f"{outdir}/face-craps.png", compose(surfaces['craps'], ink_from_photo(f"{rawdir}/face-craps.png")))
cv2.imwrite(f"{outdir}/face-ceelo.png", compose(surfaces['ceelo'], ink_from_photo(f"{rawdir}/face-ceelo.png")))
cv2.imwrite(f"{outdir}/face-profile.png", compose(surfaces['profile'], brush_ink(["PROFILE"], 11, 0.44, max_w=0.58, max_h=0.32)))
cv2.imwrite(f"{outdir}/face-settings.png", compose(surfaces['settings'], brush_ink(["SETTINGS"], 12, 0.44)))
cv2.imwrite(f"{outdir}/face-tutorial-off.png", compose(surfaces['tutorial'], brush_ink(["TUTORIAL", "OFF"], 13, 0.3, stretch=1.15)))
cv2.imwrite(f"{outdir}/face-tutorial-on.png", compose(surfaces['tutorial'], brush_ink(["TUTORIAL", "ON"], 13, 0.3, stretch=1.15)))

# logo: cut the blue i-play mark (and its inset rim) out of the upright diamond view
dia = cv2.imread(f"{rawdir}/../raw-logo-diamond.png")
hsv = cv2.cvtColor(dia, cv2.COLOR_BGR2HSV)
m = cv2.inRange(hsv, (78, 110, 90), (108, 255, 255))
n, lab, stats, _ = cv2.connectedComponentsWithStats(m)
keep = [i for i in range(1,n) if stats[i, cv2.CC_STAT_AREA] > 4000]
m = np.isin(lab, keep).astype(np.uint8)*255
m = cv2.morphologyEx(m, cv2.MORPH_CLOSE, np.ones((25,25),np.uint8))
m = cv2.dilate(m, np.ones((13,13),np.uint8))
x, y, w, h = cv2.boundingRect(m)
print("logo box", x, y, w, h)
pad = 10
crop = dia[y-pad:y+h+pad, x-pad:x+w+pad]; cm = m[y-pad:y+h+pad, x-pad:x+w+pad]
scale = (N*0.5) / max(crop.shape[:2])
crop = cv2.resize(crop, None, fx=scale, fy=scale, interpolation=cv2.INTER_AREA)
cm = cv2.GaussianBlur(cv2.resize(cm, (crop.shape[1], crop.shape[0])), (0,0), 2).astype(np.float32)/255
logo_face = cv2.cvtColor(surfaces['logo'].astype(np.uint8), cv2.COLOR_RGB2BGR).astype(np.float32)
oy, ox = (N-crop.shape[0])//2, (N-crop.shape[1])//2
reg = logo_face[oy:oy+crop.shape[0], ox:ox+crop.shape[1]]
logo_face[oy:oy+crop.shape[0], ox:ox+crop.shape[1]] = reg*(1-cm[...,None]) + crop.astype(np.float32)*cm[...,None]
cv2.imwrite(f"{outdir}/face-logo.png", logo_face.astype(np.uint8))
