"""Reuse unchanged QA evidence and record current imported data/build provenance, without Unity."""
import hashlib
import json
from pathlib import Path
import shutil
import sys
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parent.parent
name = sys.argv[1]
if not name.replace('-', '').replace('_', '').isalnum():
    raise ValueError('safe run ID required')
output = root/'Docs'/'QA'/'Evidence'/name
output.mkdir(exist_ok=False)
previous = root/'Docs'/'QA'/'Evidence'/'qa-core-v4-20261014'
ui_evidence = root/'Docs'/'QA'/'Evidence'/'qa-ui-v1-20261014'
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
sources = []
for item in json.loads((previous/'source-manifest.json').read_text(encoding='utf-8-sig')):
    if not item['source'].endswith('.cs'):
        continue
    path = root/item['source']
    ui_copy = ui_evidence/'Sources'/path.name
    sources.append({'path': str(path.relative_to(root)), 'sha256': sha(path), 'matches_v4': sha(path).lower() == item['sha256'].lower(),
                    'matches_focused_ui_run': ui_copy.exists() and sha(path) == sha(ui_copy)})
data = []
for file in ('stages.json', 'ads.json', 'strings.json'):
    generated = root/'GameData'/'Generated'/file
    imported = root/'Client'/'Assets'/'StageGame'/'Resources'/'GameData'/file
    data.append({'file': file, 'generatedSha256': sha(generated), 'importedSha256': sha(imported),
                 'same_bytes': generated.read_bytes() == imported.read_bytes(),
                 'same_semantics': json.loads(generated.read_text(encoding='utf-8-sig')) == json.loads(imported.read_text(encoding='utf-8-sig')),
                 'generated_matches_v4_paths': sha(generated) == sha(previous/'GeneratedData'/file)})
logs = []
for file in ('editmode-results.xml', 'build-Android.txt', 'build-WebGL.txt'):
    path = root/'Client'/'Logs'/file
    if not path.exists():
        logs.append({'file': file, 'status': 'MISSING'})
        continue
    shutil.copyfile(path, output/file)
    record = {'file': file, 'sha256': sha(path), 'source': 'Client owner generated; QA read only'}
    if path.suffix == '.xml':
        tree = ET.parse(path).getroot()
        record['result'] = {key: tree.attrib.get(key) for key in ('result', 'total', 'passed', 'failed', 'start-time', 'end-time')}
    else:
        record['summary'] = path.read_text(encoding='utf-8-sig')
    logs.append(record)
artifacts = []
for pattern in ('Android/*.apk', 'WebGL/Build/*.wasm', 'WebGL/Build/*.data', 'WebGL/Build/*.js', 'WebGL/index.html'):
    for path in (root/'Client'/'Builds').glob(pattern):
        artifacts.append({'path': str(path.relative_to(root)), 'bytes': path.stat().st_size, 'sha256': sha(path)})
graphics = []
for relative in ('Client/ProjectSettings/GraphicsSettings.asset', 'Client/Assets/StageGame/Editor/ProjectBuilder.cs',
                 'Client/Assets/StageGame/Prefabs/AppRoot.prefab', 'Client/Assets/StageGame/Prefabs/UI/Screens/NoticesModal.prefab',
                 'Client/Assets/StageGame/Scenes/Main.unity'):
    path = root/relative
    if path.exists():
        graphics.append({'path': relative, 'sha256': sha(path)})
data_validation_sources = []
data_evidence = root/'Docs'/'QA'/'Evidence'/'qa-data-v2-20261014'
for item in json.loads((data_evidence/'source-manifest.json').read_text(encoding='utf-8-sig')):
    path = root/item['source']
    data_validation_sources.append({'path': item['source'], 'sha256': sha(path), 'matches_data_v2': sha(path).lower() == item['sha256'].lower()})
report = {'reused_core_evidence': 'qa-core-v4-20261014 (21/21; 1000 rule sequences; 1000 AI positions)',
          'focused_changed_ui_evidence': 'qa-ui-v1-20261014 (7 app flow groups; Unity APIs replaced by doubles)' if ui_evidence.exists() else None,
          'reused_stage_paths': 'qa-core-v4-20261014/stage-paths.json (30/30 examples)',
          'sources': sources, 'data': data, 'client_logs': logs, 'build_artifacts': artifacts, 'graphics_ui_assets': graphics, 'data_validation_sources': data_validation_sources,
          'limitations': 'Hashes tie current files to QA source/data evidence. Build logs/artifacts were produced by Client; no device, embedded-build data extraction, real ads, store approval, performance or human fun verification. 650ms GameApp cutoff may change AI paths.'}
(output/'results.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps({'core_sources_unchanged': all(s['matches_v4'] for s in sources if Path(s['path']).name != 'GameApp.cs'),
                  'all_sources_covered_by_existing_qa': all(s['matches_v4'] or (Path(s['path']).name == 'GameApp.cs' and s['matches_focused_ui_run']) for s in sources), 'client_generated_match': all(d['same_semantics'] for d in data),
                  'data_validation_sources_unchanged': all(s['matches_data_v2'] for s in data_validation_sources),
                  'verified_stage_data_unchanged': all(d['generated_matches_v4_paths'] for d in data), 'build_artifacts': len(artifacts), 'evidence': str(output)}, indent=2))
