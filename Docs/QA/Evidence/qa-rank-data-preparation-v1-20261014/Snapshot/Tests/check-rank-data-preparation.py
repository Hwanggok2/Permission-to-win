"""Read-only frozen JSON comparison and reference fixture checks; no Unity product run."""
import hashlib
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BASE = "Docs/QA/Evidence/qa-graphics-core-v2-20261014/ClientData"
GENERATED = "GameData/Generated"
CLIENT = "Client/Assets/StageGame/Resources/GameData"
EXPECTED = {
    "stages": "6e66c8b0abe2dcc3b5297bf08a98953929b7e17b31e740569910255f7601fc26",
    "strings": "e191ce6773c2e608831f4f38bbb1b995bdd2caec3c156693cdbe71e50645828c",
    "ads": "d19f181faf52db28287bbed1b42291770dc3bf83d35e5cca8af4a6dcce9e194a",
}


def sha(data):
    return hashlib.sha256(data).hexdigest()


def windows(cells):
    found = set()
    for row in range(6):
        for col in range(7):
            for dx, dy in ((1, 0), (0, 1), (1, 1), (-1, 1)):
                coords = [(col + n * dx, row + n * dy) for n in range(4)]
                if not all(0 <= x < 7 and 0 <= y < 6 for x, y in coords):
                    continue
                line = tuple(sorted(y * 7 + x for x, y in coords))
                if cells[line[0]] and len({cells[cell] for cell in line}) == 1:
                    found.add(line)
    return found


def main():
    run_id = sys.argv[1]
    if not re.fullmatch(r"[A-Za-z0-9_-]+", run_id):
        raise ValueError("Use a safe, new run ID")
    evidence = ROOT / "Docs/QA/Evidence" / run_id
    evidence.mkdir(exist_ok=False)
    paths = [f"{folder}/{name}.json" for folder in (BASE, GENERATED, CLIENT) for name in EXPECTED]
    paths += [
        "GameData/RANK-UI-CONTRACT.md", "GameData/Validation/rank-ui-changes.json",
        "GameData/Validation/rank-ui-results.json", "Docs/Management/CONTRACT.md",
        "Docs/QA/RANK-UI-RESULTS.md", "Tests/Fixtures/rank-ui-interaction-cases.json",
        "Tests/Fixtures/rank-ui-crossing-win-cases.json", "Tests/RankHarness/README.md",
        "Tests/RankHarness/WinningSegmentOracle.cs", "Tests/check-rank-data-preparation.py",
    ]
    copied = {}
    for path in paths:
        copied[path] = (ROOT / path).read_bytes()
        destination = evidence / "Snapshot" / path
        destination.parent.mkdir(parents=True, exist_ok=True)
        destination.write_bytes(copied[path])
    manifest = [{"source": path, "sha256": sha(data)} for path, data in copied.items()]
    (evidence / "source-manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")

    def read(path):
        return json.loads(copied[path].decode("utf-8-sig"))

    for name, expected in EXPECTED.items():
        assert sha(copied[f"{GENERATED}/{name}.json"]) == expected, name
        assert copied[f"{GENERATED}/{name}.json"] == copied[f"{CLIENT}/{name}.json"], name
    old = read(f"{BASE}/stages.json")["stages"]
    stages = read(f"{GENERATED}/stages.json")["stages"]
    assert [s["id"] for s in stages] == list(range(1, 31))
    assert len(old) == len(stages) == 30
    for before, after in zip(old, stages):
        assert all(before[key] == after[key] for key in before), after["id"]
    assert copied[f"{GENERATED}/ads.json"] == copied[f"{BASE}/ads.json"]

    old_strings = {s["key"]: s for s in read(f"{BASE}/strings.json")["strings"]}
    string_rows = read(f"{GENERATED}/strings.json")["strings"]
    strings = {s["key"]: s for s in string_rows}
    assert len(old_strings) == 137 and len(strings) == len(string_rows) == 228
    assert set(old_strings) <= set(strings)
    changed = {key for key in old_strings if old_strings[key] != strings[key]}
    changes = read("GameData/Validation/rank-ui-changes.json")["revisions"]
    assert len(changes) == len(changed) == 47 and changed == {s["key"] for s in changes}
    for revision in changes:
        for locale in ("ko", "en"):
            key = revision["key"]
            assert revision["before"][locale] == old_strings[key][locale], key
            assert revision["after"][locale] == strings[key][locale], key
    for row in string_rows:
        assert all(isinstance(row[loc], str) and row[loc].strip() for loc in ("ko", "en"))
        assert re.findall(r"\{\d+\}", row["ko"]) == re.findall(r"\{\d+\}", row["en"]), row["key"]
    for key, row in old_strings.items():
        for locale in ("ko", "en"):
            assert re.findall(r"\{\d+\}", row[locale]) == re.findall(r"\{\d+\}", strings[key][locale]), key
    ranks = ["훈련병", "이등병", "일병", "상병", "병장", "하사", "중사", "상사", "소위", "중위"]
    for stage in stages:
        stage_id = stage["id"]
        rank = 1 + (stage_id - 1) // 3
        assert stage["rankKey"] == f"rank_{rank:02}"
        assert strings[stage["rankKey"]]["ko"] == ranks[rank - 1]
        assert stage["descriptionKey"] == f"stage_desc_{stage_id:03}"
        assert stage["difficultyStars"] == 1 + (stage_id - 1) // 6
        for field, suffix in (("aiIntroKey", "intro"), ("aiWinKey", "win"), ("aiLoseKey", "lose")):
            assert stage[field] == f"ai_rank_{rank:02}_{suffix}"
        for field in ("rankKey", "descriptionKey", "aiIntroKey", "aiWinKey", "aiLoseKey"):
            assert stage[field] in strings
    assert strings["client_ui_start"]["ko"] == "시작하기" and strings["client_ui_start"]["en"] == "Start"
    assert strings["multiplayer_start"]["ko"] == "대국 시작"
    short_keys = [key for key in strings if key.startswith(("stage_desc_", "ai_rank_"))]
    assert len(short_keys) == 60
    lengths = {locale: max(len(strings[key][locale]) for key in short_keys) for locale in ("ko", "en")}
    assert lengths["ko"] <= 22 and lengths["en"] <= 41

    interaction = read("Tests/Fixtures/rank-ui-interaction-cases.json")
    groups = ("boardCases", "lifecycleCases", "modalCases", "additionalCases")
    interaction_ids = [case["id"] for group in groups for case in interaction[group]]
    assert len(interaction_ids) == len(set(interaction_ids)) == 23
    crossing = read("Tests/Fixtures/rank-ui-crossing-win-cases.json")["cases"]
    assert len(crossing) == len({case["id"] for case in crossing}) == 4
    for case in crossing:
        cells, heights = [0] * 42, [0] * 7
        for ply, column in enumerate(case["columns"]):
            assert not windows(cells) and 0 <= column < 7 and heights[column] < 6, case["id"]
            last = heights[column] * 7 + column
            cells[last] = case["starter"] * (-1 if ply % 2 else 1)
            heights[column] += 1
        assert last == case["lastCell"] and cells[last] == case["winner"]
        lines = windows(cells)
        assert lines == {tuple(sorted(line)) for line in case["winningLines"]} and len(lines) == 2
        assert all(last in line for line in lines)
        first, final = case["invalidUnionFarthestPair"]
        dx, dy = abs(first % 7 - final % 7), abs(first // 7 - final // 7)
        assert dx and dy and dx != dy
        union = set().union(*lines)
        distance = lambda a, b: (a % 7 - b % 7) ** 2 + (a // 7 - b // 7) ** 2
        assert distance(first, final) == max(distance(a, b) for a in union for b in union)
    changed_during_read = [path for path, data in copied.items() if (ROOT / path).read_bytes() != data]
    assert not changed_during_read, changed_during_read
    results = {
        "status": "FROZEN_JSON_STATIC_CHECKS_AND_REFERENCE_PREPARATION_PASSED",
        "frozenGeneratedClientSha256": EXPECTED, "legacyStageFieldsPreserved": True,
        "strings": {"total": 228, "preserved": 90, "approvedChanges": 47, "added": 91},
        "shortDescriptionDialogueCount": 60, "maxShortTextCharacters": lengths,
        "stageRankStarsAndDialogueReferences": 30, "interactionScenariosPrepared": 23,
        "crossingWinReferenceVectorsChecked": 4, "changedDuringRead": changed_during_read,
        "runtimeProductTestsExecuted": False, "winningSegmentCSharpHelperCompiled": False,
        "unityBrowserDeviceExecuted": False, "excelExporterTestsIndependentlyRerun": False,
        "limitations": "Static JSON/preservation checks and independent fixture coherence only; Client UI remains unfrozen. Data team's 42-test result is reported evidence, not an independent QA execution. No render, interaction, runtime validation, performance or release acceptance."
    }
    (evidence / "results.json").write_text(json.dumps(results, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(results, ensure_ascii=False))


if __name__ == "__main__":
    main()
