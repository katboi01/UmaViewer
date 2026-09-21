#!/usr/bin/env python3
"""Turn a raw multi-period VMD capture into a seamless looping VMD.

Finds the true loop period (full-body resample + autocorrelation), extracts one dense
period with a short tail crossfade, and optionally injects a blink / strips mouth morphs.
Use it on a recording made with `--export --seconds N`.
"""
import argparse, struct, math, sys

BONE, MORPH = 111, 23
MOUTH = {'あ','い','う','え','お','あ2','い2','う2','え2','お2','▲','□'}
CORE = {'センター','グルーブ','下半身','上半身','右足','左足','右ひざ','左ひざ',
        '右足首','左足首','頭','右腕','左腕','右ひじ','左ひじ'}


def parse(path):
    d = open(path, 'rb').read()
    hdr, name, o = d[:30], d[30:50], 50
    (bc,) = struct.unpack_from('<I', d, o); o += 4
    bones = {}
    for _ in range(bc):
        raw = d[o:o+15]; k = raw.split(b'\x00')[0].decode('shift_jis', 'replace')
        fr, = struct.unpack_from('<I', d, o+15)
        pos = struct.unpack_from('<3f', d, o+19); q = struct.unpack_from('<4f', d, o+31)
        bones.setdefault(k, {'raw': raw, 'k': {}})['k'][fr] = (pos, q, d[o+47:o+111]); o += BONE
    (mc,) = struct.unpack_from('<I', d, o); o += 4
    morphs = {}
    for _ in range(mc):
        raw = d[o:o+15]; k = raw.split(b'\x00')[0].decode('shift_jis', 'replace')
        fr, = struct.unpack_from('<I', d, o+15); w, = struct.unpack_from('<f', d, o+19)
        morphs.setdefault(k, {'raw': raw, 'k': {}})['k'][fr] = w; o += MORPH
    return hdr, name, bones, morphs, d[o:]


def slerp(a, b, t):
    dot = sum(x*y for x, y in zip(a, b))
    if dot < 0: b = tuple(-x for x in b); dot = -dot
    if dot > 0.9995:
        r = [a[i] + t*(b[i]-a[i]) for i in range(4)]
    else:
        th = math.acos(max(-1, min(1, dot))); s = math.sin(th)
        r = [(math.sin((1-t)*th)*a[i] + math.sin(t*th)*b[i])/s for i in range(4)]
    n = math.sqrt(sum(x*x for x in r)) or 1.0
    return tuple(x/n for x in r)


def _bracket(ks, f):
    lo = ks[0]
    for kk in ks:
        if kk >= f: return lo, kk
        lo = kk
    return lo, ks[-1]


def sample_bone(bd, f):
    ks = bd['_s']
    if f <= ks[0]: return bd['k'][ks[0]]
    if f >= ks[-1]: return bd['k'][ks[-1]]
    lo, hi = _bracket(ks, f)
    if lo == hi: return bd['k'][lo]
    t = (f-lo)/(hi-lo); a, b = bd['k'][lo], bd['k'][hi]
    return (tuple(a[0][i] + t*(b[0][i]-a[0][i]) for i in range(3)), slerp(a[1], b[1], t), a[2])


def sample_morph(md, f):
    ks = md['_s']
    if f <= ks[0]: return md['k'][ks[0]]
    if f >= ks[-1]: return md['k'][ks[-1]]
    lo, hi = _bracket(ks, f)
    if lo == hi: return md['k'][lo]
    return md['k'][lo] + (f-lo)/(hi-lo)*(md['k'][hi]-md['k'][lo])


def ang(a, b):
    dp = min(1.0, abs(sum(x*y for x, y in zip(a, b))))
    return math.degrees(2*math.acos(dp))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('input'); ap.add_argument('output')
    ap.add_argument('--blink', action='store_true', help='inject one periodic まばたき blink')
    ap.add_argument('--no-mouth', dest='no_mouth', action='store_true', help='strip mouth vowel morphs')
    ap.add_argument('--tiles', type=int, default=1, help='repeat the loop N times (e.g. slower blink cadence)')
    ap.add_argument('--pmin', type=int, default=8); ap.add_argument('--pmax', type=int, default=90)
    ap.add_argument('--crossfade', type=int, default=6, help='tail crossfade frames')
    ap.add_argument('--seam-cloth', action='store_true', help='include non-core (baked cloth) tracks when choosing the loop phase')
    ap.add_argument('--start', type=int, default=-1, help="fixed loop window start frame instead of searching for the best seam (0 = the capture start, which the exporter aligns to the clip's phase 0)")
    a = ap.parse_args()

    hdr, name, bones, morphs, tail = parse(a.input)
    for k in bones: bones[k]['_s'] = sorted(bones[k]['k'])
    for k in morphs: morphs[k]['_s'] = sorted(morphs[k]['k'])
    maxf = max(bones[k]['_s'][-1] for k in bones)
    core = [k for k in bones if k in CORE] or list(bones)

    def bodydiff(f1, f2):
        return sum(ang(sample_bone(bones[k], f1)[1], sample_bone(bones[k], f2)[1]) for k in core) / len(core)

    cand = []
    for P in range(a.pmin, min(a.pmax, maxf-6)):
        ds = [bodydiff(f, f+P) for f in range(20, maxf-P, 2)]
        if len(ds) >= 4: cand.append((sum(ds)/len(ds), P))
    if not cand: sys.exit("loopify: clip too short to find a period")
    # Multiples of the true period score nearly as well; pick the smallest period whose
    # score is within tolerance of the best, i.e. the fundamental, not 2x/3x of it.
    mind = min(d for d, _ in cand)
    tol = mind * 0.5 + 0.5
    P = min(P for d, P in cand if d <= mind + tol)

    # Phase choice. The period belongs to the body, but the seam does not: with baked
    # spring-bone tracks the cloth has to close too, and the phase that suits the skeleton
    # can leave a long chain like the tail mid-swing. Score both, weighting the body higher
    # so a cloth outlier cannot drag the skeleton off its own best phase.
    seam_bones = [k for k in bones if k not in CORE] if a.seam_cloth else []
    best = None
    for F0 in range(15, maxf-P-a.crossfade):
        m = max(ang(sample_bone(bones[k], F0+P)[1], sample_bone(bones[k], F0)[1]) for k in core)
        if seam_bones:
            c = max(ang(sample_bone(bones[k], F0+P)[1], sample_bone(bones[k], F0)[1]) for k in seam_bones)
            m = m + 0.5 * c
        if best is None or m < best[0]: best = (m, F0)
    _, F0 = best
    if a.start >= 0: F0 = a.start
    K = min(a.crossfade, P//2)

    def blendb(a1, a2, t):
        return (tuple(a1[0][i] + t*(a2[0][i]-a1[0][i]) for i in range(3)), slerp(a1[1], a2[1], t), a1[2])

    loopb = []
    for ph in range(P):
        frame = {}
        for k, bd in bones.items():
            p = sample_bone(bd, F0+ph)
            if ph >= P-K:
                m = ph-(P-K); alpha = (m+1)/(K+1)
                p = blendb(p, sample_bone(bd, F0-K+m), alpha)
            frame[k] = p
        loopb.append(frame)
    newmax = P*a.tiles - 1

    brecs = []
    for t in range(a.tiles):
        for ph in range(P):
            for k, bd in bones.items():
                p = loopb[ph][k]; r = bytearray(111); r[:15] = bd['raw']
                struct.pack_into('<I', r, 15, t*P+ph)
                struct.pack_into('<3f', r, 19, *p[0]); struct.pack_into('<4f', r, 31, *p[1])
                r[47:111] = p[2]; brecs.append(bytes(r))

    mrecs = []
    for k, md in morphs.items():
        if a.no_mouth and k in MOUTH: continue
        if a.blink and k == 'まばたき': continue
        if max(abs(w) for w in md['k'].values()) < 1e-4: continue
        for t in range(a.tiles):
            for ph in range(P):
                w = sample_morph(md, F0+ph); r = bytearray(23); r[:15] = md['raw']
                struct.pack_into('<I', r, 15, t*P+ph); struct.pack_into('<f', r, 19, w); mrecs.append(bytes(r))
    if a.blink:
        bn = 'まばたき'.encode('shift_jis'); c = newmax//2
        kf = [(0, 0.0), (c-2, 0.0), (c, 1.0), (c+3, 0.0), (newmax, 0.0)] if newmax >= 6 \
            else [(0, 0.0), (max(1, newmax//2), 1.0), (newmax, 0.0)]
        for f, w in kf:
            r = bytearray(23); r[:len(bn)] = bn
            struct.pack_into('<I', r, 15, f); struct.pack_into('<f', r, 19, w); mrecs.append(bytes(r))

    out = bytearray(); out += hdr + name
    out += struct.pack('<I', len(brecs)) + b''.join(brecs)
    out += struct.pack('<I', len(mrecs)) + b''.join(mrecs)
    out += tail
    open(a.output, 'wb').write(out)

    seam = max(ang(loopb[-1][k][1], loopb[0][k][1]) for k in core)
    print(f"loopify: period={P} phase={F0} tiles={a.tiles} -> 0..{newmax} ({(newmax+1)/30:.2f}s), core seam {seam:.2f} deg")
    print(f"loopify: wrote {a.output}")


if __name__ == '__main__':
    main()
