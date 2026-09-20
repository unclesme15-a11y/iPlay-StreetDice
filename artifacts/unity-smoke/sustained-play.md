# Sustained play verification

Unity Editor, normal time scale, real animations and opponent loop. Not physical-phone performance evidence.

Craps roll/fade cycle 1: shooter p1, phase ShooterDecision, point -

Craps roll/fade cycle 2: shooter p3, phase Point, point 6

Craps roll/fade cycle 3: shooter p3, phase Point, point 6

Craps roll/fade cycle 4: shooter p3, phase Point, point 6

Craps roll/fade cycle 5: shooter p1, phase ComeOut, point -

Craps roll/fade cycle 6: shooter p4, phase Point, point 5

Craps: 6 completed cycles, 3 shooters, 128.4s elapsed, total money 5000. Balances: [p1, 1040], [p3, 944], [p4, 1031], [p2, 983], [bot-5, 1002].

CeeLo roll/fade cycle 1: shooter p1, phase CeeLo, point -

CeeLo roll/fade cycle 2: shooter p3, phase CeeLo, point -

CeeLo roll/fade cycle 3: shooter p4, phase CeeLo, point -

CeeLo roll/fade cycle 4: shooter p2, phase CeeLo, point -

CeeLo roll/fade cycle 5: shooter bot-5, phase CeeLo, point -

CeeLo roll/fade cycle 6: shooter p1, phase CeeLo, point -

CeeLo: 6 completed cycles, 5 shooters, 36.4s elapsed, total money 5000. Balances: [p1, 1000], [p3, 980], [p4, 1000], [p2, 1000], [bot-5, 1020].

## Editor timing sample

9832 rendered-frame intervals after a five-second warmup. Mean 16.67ms; p50 16.66ms; p95 19.53ms; p99 22.49ms; max 36.29ms. Frames over 33.34ms: 1; over 50ms: 0.

Unity 6000.4.11f1; 1600x924; graphics NVIDIA GeForce RTX 5050 Laptop GPU; target 60fps. Includes Editor overhead, betting, rolls and payouts. No screenshots are captured during sampling. This is not GPU timing, a standalone-player benchmark or mobile performance certification.

Result: PASS
