import cv2, numpy as np, sys
src, outdir = sys.argv[1], sys.argv[2]
im = cv2.imread(src, cv2.IMREAD_UNCHANGED)
k = 1254/700
P = {n:(x*k, y*k) for n,(x,y) in dict(T=(353,30), L=(40,183), R=(668,205), F=(345,345),
     BL=(68,522), B=(335,690), BR=(628,540)).items()}
N = 1024
def warp(quad, name, size=N):
    srcq = np.float32([P[q] for q in quad])
    dst = np.float32([[0,0],[size,0],[size,size],[0,size]])
    H = cv2.getPerspectiveTransform(srcq, dst)
    out = cv2.warpPerspective(im, H, (size,size), flags=cv2.INTER_CUBIC)
    cv2.imwrite(f"{outdir}/{name}.png", out)
warp(['L','F','B','BL'], 'raw-craps')
warp(['F','R','BR','B'], 'raw-ceelo')
warp(['T','R','F','L'], 'raw-top')   # back corner at top-left
# diamond mapping keeps the logo upright
srcq = np.float32([P['L'],P['T'],P['R'],P['F']])
dst = np.float32([[0,N/2],[N/2,0],[N,N/2],[N/2,N]])
H = cv2.getPerspectiveTransform(srcq, dst)
cv2.imwrite(f"{outdir}/raw-logo-diamond.png", cv2.warpPerspective(im, H, (N,N), flags=cv2.INTER_CUBIC))
