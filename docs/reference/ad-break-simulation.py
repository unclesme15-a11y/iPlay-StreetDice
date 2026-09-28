import random, statistics as st
random.seed(7)
ROLL=5.0      # gesture + throw settle + result (fade window overlaps)
POINT_WIN=15  # 10s propose + 5s shooter lock, once when the point is set
BETWEEN=5     # run-same/double-up/stake + shoot between shots
BREAK=20      # average ad break length (skippable at 15s); game paused
def dice(): return random.randint(1,6)+random.randint(1,6)
def turn(comeout_window):
    t=0; shots=0
    while True:
        shots+=1; t+=comeout_window+ROLL
        r=dice()
        if r in (7,11,2,3,12): t+=BETWEEN; continue          # shot over, shooter keeps dice
        p=r; t+=POINT_WIN
        while True:
            t+=ROLL; r=dice()
            if r==p: t+=BETWEEN; break                          # point hit, keeps dice
            if r==7: return t, shots                            # seven-out: dice pass
def run(comeout_window, floor, players=5, rounds=20000):
    clock=0; since=0; last_end=0; breaks=[]; per_round=[]; shot_times=[]; turns=[]
    rb=0; n7=0
    for r in range(rounds*players):
        d,s=turn(comeout_window); turns.append(d); shot_times.append(d/s)
        clock+=d; n7+=1; since+=1
        if since>=2 and clock-last_end>=floor:
            breaks.append(clock); clock+=BREAK; last_end=clock; since=0; rb+=1
        if (r+1)%players==0: per_round.append(rb); rb=0
    hours=clock/3600
    return dict(shot=st.mean(shot_times), turn=st.mean(turns), round_min=st.mean(turns)*players/60,
                breaks_hr=len(breaks)/hours, min_between=None,
                pct_round_ge2=sum(1 for x in per_round if x>=2)/len(per_round))
for cw,label in ((15,"come-out window 15s"),(0,"no come-out window")):
    for floor in (0,90,120,150,180):
        r=run(cw,floor)
        print(f"{label:22s} floor {floor:3d}s | shot {r['shot']:4.0f}s  turn {r['turn']:4.0f}s  5-player round {r['round_min']:4.1f}min | breaks/hr {r['breaks_hr']:4.1f} | rounds w/ >=2 breaks {r['pct_round_ge2']*100:5.1f}%")
