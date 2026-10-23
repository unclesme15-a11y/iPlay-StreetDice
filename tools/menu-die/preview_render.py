# Software render of the menu die with the same geometry, camera, lights and label rule as
# StreetDiceMenuDie.cs, so the owner can see the look before Unity compiles it.
import cv2, numpy as np, sys
facedir, shot, static, out, yaw, pitch = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4], float(sys.argv[5]), float(sys.argv[6])
H_, R_ = 0.5, 0.13
def unity_cross(a, b): return np.cross(a, b)          # same formula Unity uses
def norm(v): return v/np.linalg.norm(v, axis=-1, keepdims=True)
cam_pos = np.array([0, 1.81, -2.58]); target = np.array([0, -0.04, 0])
f = norm(target - cam_pos); r = norm(unity_cross([0,1,0], f)); u = unity_cross(f, r)
# die rotation: AngleAxis(pitch, camRight) * Euler(0, yaw, 0)
def rot_y(a):
    c, s = np.cos(np.radians(a)), np.sin(np.radians(a))
    return np.array([[c,0,s],[0,1,0],[-s,0,c]])
def rot_axis(axis, a):
    axis = axis/np.linalg.norm(axis); x,y,z = axis; c,s = np.cos(np.radians(a)), np.sin(np.radians(a)); C=1-c
    # left-handed (Unity) rotation = right-handed formula with negated angle
    pass
    return np.array([[c+x*x*C, x*y*C-z*s, x*z*C+y*s],[y*x*C+z*s, c+y*y*C, y*z*C-x*s],[z*x*C-y*s, z*y*C+x*s, c+z*z*C]])
Rm = rot_axis(r, pitch) @ rot_y(yaw)
Ri = Rm.T
faces = {  # normal, up, texture
 'profile': ([0,1,0],[0,0,1]), 'logo': ([0,-1,0],[0,0,1]), 'craps': ([0,0,-1],[0,1,0]),
 'ceelo': ([1,0,0],[0,1,0]), 'tutorial-off': ([0,0,1],[0,1,0]), 'settings': ([-1,0,0],[0,1,0])}
tex = {k: cv2.cvtColor(cv2.imread(f"{facedir}/menu-die-{k}.jpg"), cv2.COLOR_BGR2RGB).astype(np.float32)/255 for k in faces}
N = 860
ys, xs = np.mgrid[0:N, 0:N]
tanh = np.tan(np.radians(15))
px = (xs+0.5)/N*2-1; py = 1-(ys+0.5)/N*2
dirs = norm(f[None,None,:] + px[...,None]*tanh*r + py[...,None]*tanh*u)
o = (Ri @ cam_pos)                      # ray origin in die space
d = dirs @ Ri.T                          # ray dirs in die space
def sdf(p):
    q = np.abs(p) - (H_-R_)
    return np.linalg.norm(np.maximum(q,0), axis=-1) + np.minimum(np.max(q, axis=-1), 0) - R_
t = np.full((N,N), 1.5); hit = np.zeros((N,N), bool)
for _ in range(90):
    p = o + d*t[...,None]
    dist = sdf(p)
    hit |= dist < 1e-4
    t = np.where(hit, t, t + dist)
p = o + d*t[...,None]
e = 1e-4
nrm = norm(np.stack([sdf(p+[e,0,0])-sdf(p-[e,0,0]), sdf(p+[0,e,0])-sdf(p-[0,e,0]), sdf(p+[0,0,e])-sdf(p-[0,0,e])], -1))
img = np.zeros((N,N,3), np.float32); alpha = hit.astype(np.float32)
cam_up_w = u; cam_f_w = f
for name,(n,up) in faces.items():
    n = np.array(n,float); up = np.array(up,float); right = unity_cross(n, up)
    sel = hit & (np.argmax(np.abs(p)*np.abs(np.sign(p)==np.sign(n)) * (np.abs(n)>0), axis=-1) == np.argmax(np.abs(n))) & (np.sign(p[..., np.argmax(np.abs(n))]) == n[np.argmax(np.abs(n))])
    sel &= np.max(np.abs(p), axis=-1) - np.abs(p[..., np.argmax(np.abs(n))]) <= 1e-9
    uu = (p@right/H_+1)/2; vv = (p@up/H_+1)/2
    if name in ('profile','logo'):
        nw, rw, uw = Rm@n, Rm@right, Rm@up
        want = cam_up_w - nw*np.dot(cam_up_w, nw)
        if np.linalg.norm(want) < 0.1: want = cam_f_w - nw*np.dot(cam_f_w, nw)
        want /= np.linalg.norm(want)
        ang = np.arctan2(np.dot(want, rw), np.dot(want, uw))
        c, s = np.cos(ang), np.sin(ang)
        x0, y0 = uu-0.5, vv-0.5
        uu, vv = x0*c - y0*s + 0.5, x0*s + y0*c + 0.5
        uu = 1-np.abs(np.mod(uu,2)-1); vv = 1-np.abs(np.mod(vv,2)-1)   # mirror wrap
    T = tex[name]; S = T.shape[0]
    cols = np.clip((uu*(S-1)).astype(int),0,S-1); rows = np.clip(((1-vv)*(S-1)).astype(int),0,S-1)
    img[sel] = T[rows[sel], cols[sel]]
# lighting: same three directional lights as the Unity code, plus a little ambient
nw = nrm @ Rm.T
lights = [(np.array([-1.6,3.2,-2.2]),0.85,np.array([1,0.97,0.92])),(np.array([2.4,0.8,-1.6]),0.3,np.array([0.85,0.9,1])),(np.array([0.5,1.5,3]),0.3,np.ones(3))]
shade = np.full((N,N,3), 0.3, np.float32); spec = np.zeros((N,N), np.float32)
view = -dirs
for L,I,C in lights:
    l = L/np.linalg.norm(L)
    shade += np.clip(nw@l,0,1)[...,None]*I*C
    h = norm(view + l)
    spec += np.clip((nw*h).sum(-1),0,1)**70 * I * 0.35
col = np.nan_to_num(np.clip(img*shade*0.92 + spec[...,None], 0, 1))
rgba = np.nan_to_num(np.dstack([col, alpha]))
# composite on the owner's screenshot: erase the old die, PROFILE/SETTINGS plates and tutorial toggle
bg = cv2.cvtColor(cv2.imread(shot), cv2.COLOR_BGR2RGB)
st = cv2.imread(static, cv2.IMREAD_UNCHANGED)
size, rx, ry = 855, 372, 48
m = np.zeros(bg.shape[:2], np.uint8)
sa = cv2.resize(st[:,:,3], (size,size)); m[ry:ry+size, rx:rx+size][sa>20] = 255
m[355:545, 1250:1490] = 255; m[630:700, 0:370] = 255
m = cv2.dilate(m, np.ones((15,15),np.uint8))
bg = cv2.inpaint(bg, m, 12, cv2.INPAINT_TELEA)
gx, gy = 800 - N//2, 462 + 11 - N//2
region = bg[gy:gy+N, gx:gx+N].astype(np.float32)/255
a = rgba[...,3:4]
a = cv2.GaussianBlur(a, (3,3), 0)[...,None] if a.ndim==3 else a
region = region*(1-a) + rgba[...,:3]*a
bg[gy:gy+N, gx:gx+N] = (region*255).astype(np.uint8)
cv2.imwrite(out, cv2.cvtColor(bg, cv2.COLOR_RGB2BGR))
