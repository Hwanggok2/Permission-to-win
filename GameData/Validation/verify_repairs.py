"""Reproduce old loss / new bounded-win findings without changing Excel or runtime JSON."""
from pathlib import Path
import hashlib
import json
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT))
from audit_openings import position, solve
from export_data import read_source, validate, SOURCE

OLD_DEPTHS = {10: 8, 12: 6, 20: 6, 22: 6, 24: 18, 26: 22}
tables = read_source(SOURCE)
validate(tables)
stages = {row["Id"]: row for row in tables["Stages"]}
repairs = json.loads((ROOT / "Validation/opening-repairs.json").read_text(encoding="utf-8"))
findings = []
for repair in repairs:
    stage = stages[repair["id"]]
    assert stage["OpeningMoves"] == repair["new"], "current source differs from repair record"
    old = dict(stage, OpeningMoves=repair["old"])
    old_score = solve(*position(old), OLD_DEPTHS[stage["Id"]])
    solve.cache_clear()
    new_score = solve(*position(stage), 8)
    solve.cache_clear()
    assert old_score == -1 and new_score == 1
    findings.append({"id": stage["Id"], "oldDepth": OLD_DEPTHS[stage["Id"]],
                     "oldBoundedOutcome": old_score, "newDepth": 8, "newBoundedOutcome": new_score})
    print(json.dumps(findings[-1]), flush=True)

result = {
    "method": "Independent seven-bit-column bitboard minimax. No heuristic values. Horizon=0 means unknown/draw. Immediate wins and compulsory blocks prune only proven tactical branches.",
    "limits": "Bounded model proof, not actual AI timing behavior, device performance, human playability, complete 30-stage certification, or finished balance.",
    "sourceSha256": hashlib.sha256(SOURCE.read_bytes()).hexdigest(),
    "stagesSha256": hashlib.sha256((ROOT / "Generated/stages.json").read_bytes()).hexdigest(),
    "findings": findings,
}
(ROOT / "Validation/bounded-outcomes.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
