# Party ad break simulation (owner rule 2026-09-28): a break after every 2nd
# seven-out at the party, whoever threw it. The party's first 5 seven-outs always
# get their breaks; after that a break needs FLOOR seconds since the last one
# ended, but never more than CAP seven-outs go by without one.
import random, statistics as st
ROLL=5.0      # gesture + throw settle + result (fade window overlaps)
WIN=15        # 10s propose + 5s shooter lock: at the come-out and when the point is set
BETWEEN=5     # run-same/double-up/stake + shoot between shots
BREAK=20      # average ad break length (skippable at 15s); game paused
def dice(): return random.randint(1,6)+random.randint(1,6)
def turn():
    t=0
    while True:
        t+=WIN+ROLL; r=dice()
        if r in (7,11,2,3,12): t+=BETWEEN; continue          # shot over, shooter keeps dice
        p=r; t+=WIN
        while True:
            t+=ROLL; r=dice()
            if r==p: t+=BETWEEN; break                          # point hit, keeps dice
            if r==7: return t                                   # seven-out: dice pass
def run(floor, cap, players=5, parties=4000, minutes=60):
    breaks=0; time=0; first_ok=0; later_ok=0; later_n=0; turns=[]
    for _ in range(parties):
        clock=0; since=0; last_end=0; n7=0; rb=0
        while clock<minutes*60:
            d=turn(); turns.append(d); clock+=d; since+=1; n7+=1
            first_round = n7<=players
            if since>=2 and (first_round or clock-last_end>=floor or since>=cap):
                clock+=BREAK; last_end=clock; since=0; breaks+=1; rb+=1
            if n7%players==0:
                if n7==players: first_ok+= rb>=2
                else: later_ok+= rb>=2; later_n+=1
                rb=0
        time+=clock
    return breaks/(time/3600), first_ok/parties, later_ok/max(later_n,1), st.mean(turns)
random.seed(7)
for cap in (3,4):
    for floor in (90,150,180,210,240,270):
        b,f,l,t=run(floor,cap)
        print(f"cap {cap} wait {floor:3d}s | breaks/hr {b:5.1f} | first round 2 breaks {f*100:5.1f}% | later rounds 2 breaks {l*100:5.1f}% | turn {t:4.0f}s")
