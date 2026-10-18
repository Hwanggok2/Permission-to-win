"""Check the exact Pages commit, completed workflow and published build bytes."""
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
from urllib.parse import quote
from urllib.request import Request, urlopen

ROOT = Path(__file__).resolve().parent
REPO = "Hwanggok2/Permission-to-win"
SITE = "https://hwanggok2.github.io/Permission-to-win/"
COMMIT = (ROOT / "deployment-commit.txt").read_text().strip()


def fetch(url):
    request = Request(url, headers={"User-Agent": "Permission-to-win-deployment-check", "Cache-Control": "no-cache"})
    with urlopen(request, timeout=60) as response:
        return response.status, response.read()


def api(path):
    return json.loads(fetch("https://api.github.com/repos/" + REPO + path)[1])


branch = api("/branches/" + quote("codex/github-pages", safe=""))
if branch["commit"]["sha"] != COMMIT:
    print(json.dumps({"status": "WAITING_FOR_USER_PUSH", "expected": COMMIT, "remote": branch["commit"]["sha"]}))
    raise SystemExit(3)

runs = api("/actions/runs?branch=codex%2Fgithub-pages&per_page=10")["workflow_runs"]
run = next((item for item in runs if item["head_sha"] == COMMIT), None)
if not run or run["status"] != "completed":
    print(json.dumps({"status": "WAITING_FOR_PAGES", "run": None if run is None else {key: run[key] for key in ("id", "status", "html_url")}}))
    raise SystemExit(3)
if run["conclusion"] != "success":
    raise RuntimeError("Pages workflow did not succeed: " + run["html_url"])


def check(item):
    status, content = fetch(SITE + quote(item["path"], safe="/") + "?v=" + COMMIT[:7])
    digest = hashlib.sha256(content).hexdigest()
    return {"path": item["path"], "status": status, "bytes": len(content), "sha256": digest,
            "matches": len(content) == item["bytes"] and digest == item["sha256"]}


manifest = json.loads((ROOT / "published-files.json").read_text())
with ThreadPoolExecutor(max_workers=4) as pool:
    files = list(pool.map(check, manifest))
report = {"checkedAt": datetime.now(timezone.utc).isoformat(), "url": SITE, "commit": COMMIT,
          "pushedBy": "user", "action": {key: run[key] for key in ("id", "html_url", "head_sha", "status", "conclusion", "updated_at")},
          "files": files, "allMatch": all(item["matches"] for item in files)}
(ROOT / "public-deployment-verification.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
print(json.dumps({"url": SITE, "commit": COMMIT, "action": report["action"], "matchingFiles": sum(item["matches"] for item in files), "totalFiles": len(files)}))
if not report["allMatch"]:
    raise SystemExit(1)

