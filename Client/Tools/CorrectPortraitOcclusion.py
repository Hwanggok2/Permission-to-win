"""Reproduce the user-authorized C02 forearm correction; preserve previous PNGs."""
import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "Docs/Design/assets/characters-pixel-20261015-v6/C02-pixel-v6.png"
REVISION = ROOT / "Docs/Design/assets/characters-occlusion-20261018-v7"
ART = ROOT / "Client/Assets/StageGame/Art/PixelMilitary"
EVIDENCE = ROOT / "Client/Evidence/portrait-occlusion-20261018"
EXPECTED_SOURCE = "0f6c78e8340bf0195479127e44474eda2f95c48e7c3e3c06d94b4a86aeed05a2"


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write_json(path, value):
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def main():
    assert digest(SOURCE) == EXPECTED_SOURCE, "Unexpected input revision"
    REVISION.mkdir(parents=True, exist_ok=True)
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    before = Image.open(SOURCE).convert("RGBA")
    after = before.copy()
    draw = ImageDraw.Draw(after)
    # Chest pocket is behind this forearm. Restore sleeve, not a displaced badge.
    # Preserve the cuff and the stepped black silhouette on the left.
    draw.polygon([(813, 687), (879, 687), (879, 737), (831, 737),
                  (831, 734), (826, 734), (826, 724), (813, 724)], fill=(91, 107, 84, 255))
    draw.polygon([(879, 688), (879, 719), (868, 719), (868, 729),
                  (854, 729), (854, 718), (842, 718), (842, 707),
                  (853, 707), (853, 698), (866, 698), (866, 688)], fill=(185, 184, 135, 255))
    draw.polygon([(814, 688), (831, 688), (831, 696), (840, 696),
                  (840, 707), (829, 707), (829, 715), (814, 715)], fill=(76, 93, 72, 255))
    after.putalpha(before.getchannel("A"))
    a, b = np.array(before), np.array(after)
    mask = np.any(a[:, :, :3] != b[:, :, :3], axis=2)
    # Reuse the adjacent sleeve's subtle color texture within coarse pixel shapes.
    # Sub-pixel edge shading matches the source; no image scaling or smoothing outside the patch.
    yy, xx = np.indices(mask.shape)
    noise = a[700 + yy % 10, 882 + xx % 8, :3].astype(float)
    noise -= noise.mean(axis=(0, 1))
    b[:, :, :3] = np.where(mask[:, :, None],
                          np.clip(b[:, :, :3].astype(float) + noise * .4, 0, 255),
                          b[:, :, :3]).astype("uint8")
    shaded = np.array(Image.fromarray(b).filter(ImageFilter.GaussianBlur(.65)))
    b[mask, :3] = shaded[mask, :3]
    after = Image.fromarray(b)
    assert after.size == before.size == (1536, 1024)
    assert np.array_equal(a[:, :, 3], b[:, :, 3]), "Alpha changed"
    assert np.array_equal(a[:, :512], b[:, :512]), "Intro changed"
    assert np.array_equal(a[:, 1024:], b[:, 1024:]), "Defeat changed"
    changed = np.any(a != b, axis=2)
    ys, xs = np.where(changed)
    bounds = [int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1]
    assert bounds == [813, 687, 880, 738]
    target = REVISION / "C02-occlusion-v7.png"
    after.save(target)
    (ART / target.name).write_bytes(target.read_bytes())
    relative = target.relative_to(ROOT).as_posix()
    record = {
        "date": "2026-10-18", "characterId": "C02", "state": "thinking",
        "reason": "Hide the left chest rank behind the raised left forearm and restore sleeve camouflage.",
        "source": SOURCE.relative_to(ROOT).as_posix(), "sourceSha256": EXPECTED_SOURCE,
        "target": relative, "targetSha256": digest(target), "changedBoundsExclusive": bounds,
        "changedPixels": int(changed.sum()), "alphaUnchanged": True,
        "introUnchanged": True, "defeatUnchanged": True, "originalAssetsPreserved": True,
        "authorization": "User explicitly authorized direct pixel corrections; requested this occlusion repair on 2026-10-18."
    }
    write_json(REVISION / "manual-corrections.json", record)
    write_json(EVIDENCE / "pixel-diff.json", record)
    descriptor = json.loads((ART / "portrait-import.json").read_text(encoding="utf-8-sig"))
    entry = next(p for p in descriptor["portraits"] if p["characterId"] == "C02")
    entry.update(file=target.name, source=relative, sha256=digest(target))
    write_json(ART / "portrait-import.json", descriptor)
    manifest = json.loads((ROOT / "Docs/Design/assets/characters-pixel-20261015-v6/manifest.json").read_text(encoding="utf-8-sig"))
    manifest.update(date="2026-10-18", tool="Pillow/NumPy: authorized local pixel correction",
                    status="PREPARED_PENDING_UNITY_VERIFICATION", changedStates=1,
                    previousManifest="Docs/Design/assets/characters-pixel-20261015-v6/manifest.json")
    selection = next(p for p in manifest["images"] if p["id"] == "C02")
    selection.update(key="occlusion-v7-C02", path=relative, sha256=digest(target),
                     previousVersion=record["source"], manualOcclusionCorrection=True)
    write_json(REVISION / "manifest.json", manifest)
    # Evidence uses nearest-neighbour enlargement; never resize production images.
    sheet = Image.new("RGB", (648, 446), "#dedbd1")
    d = ImageDraw.Draw(sheet)
    for index, (label, picture) in enumerate((("BEFORE", before), ("AFTER", after))):
        pose = picture.crop((512, 210, 1024, 861)).resize((320, 407), Image.Resampling.NEAREST)
        sheet.paste(pose, (index * 328, 30), pose)
        d.text((index * 328 + 8, 9), label, fill="#20251b")
    sheet.save(EVIDENCE / "before-after.png")
    print(json.dumps(record, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
