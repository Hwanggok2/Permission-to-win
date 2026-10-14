"""Write a durable report for publication/rollback fault-injection checks."""
from pathlib import Path
import hashlib
import io
import json
import sys
import unittest

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT))
from test_export_data import ExportTests

names = ["test_replace_failure_rolls_back_complete_bundle",
         "test_failed_first_publication_restores_file_absence",
         "test_rollback_failure_retains_marker_backups_and_blocks_check"]
files = [ROOT / "GameData_MVP.xlsx"] + [ROOT / "Generated" / name for name in ("stages.json", "ads.json", "strings.json")]
before = {str(path.relative_to(ROOT)): hashlib.sha256(path.read_bytes()).hexdigest() for path in files}
log = io.StringIO()
result = unittest.TextTestRunner(stream=log, verbosity=2).run(unittest.TestSuite(ExportTests(name) for name in names))
after = {str(path.relative_to(ROOT)): hashlib.sha256(path.read_bytes()).hexdigest() for path in files}
assert before == after, "fault checks changed source or runtime data"
(ROOT / "Validation/export-failure-tests.log").write_text(log.getvalue(), encoding="utf-8")
report = {"testsRun": result.testsRun, "failures": len(result.failures), "errors": len(result.errors),
          "scenarios": ["publication replacements 1/2/3 fail: all old files restored",
                        "first publication fails: original absence restored",
                        "rollback fails: marker/backups retained, --check and retry blocked"],
          "dataUnchanged": before == after, "dataHashes": after,
          "exporterSha256": hashlib.sha256((ROOT / "export_data.py").read_bytes()).hexdigest(),
          "limits": "Handled exceptions tested; no guarantee of bundle atomicity for process kill, power loss, or concurrent readers."}
(ROOT / "Validation/export-failure-results.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
print(json.dumps(report, indent=2))
raise SystemExit(0 if result.wasSuccessful() else 1)
