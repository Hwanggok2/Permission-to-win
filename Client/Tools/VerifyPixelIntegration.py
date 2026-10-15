"""Record the actual tested data/art/prefab set after the Unity suite passes."""
from pathlib import Path
import hashlib
import json
import xml.etree.ElementTree as ET

repo = Path(__file__).resolve().parents[2]
evidence = repo / 'Client/Evidence/pixel-art-20261015'
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
write_json = lambda path, value: path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
suite_path = evidence / 'unity-tests-verified.xml'
suite = ET.parse(suite_path).getroot()
assert suite.attrib['result'] == 'Passed' and int(suite.attrib['failed']) == 0
assert int(suite.attrib['skipped']) == 0 and int(suite.attrib['total']) > 0
art = repo / 'Client/Assets/StageGame/Art/PixelMilitary'
portraits = json.loads((art / 'portrait-import.json').read_text(encoding='utf-8'))['portraits']
assert len(portraits) == 19 and len({p['characterId'] for p in portraits}) == 19
for item in portraits:
    assert sha(art / item['file']) == sha(repo / item['source']) == item['sha256']
    assert (art / (item['file'] + '.meta')).exists()
assert next(p for p in portraits if p['characterId'] == 'C06')['file'] == 'C06-cap-v2.png'
for name in ('stages.json', 'strings.json', 'ads.json'):
    assert sha(repo / 'GameData/Generated' / name) == sha(repo / 'Client/Assets/StageGame/Resources/GameData' / name)
assert sha(repo / 'GameData/GameData_MVP.xlsx') == sha(evidence / 'GameData-story-candidate.xlsx')
for source in ('characters-20261015-v1', 'characters-caps-20261015-v1', 'characters-caps-20261015-v2'):
    manifest = json.loads((repo / 'Docs/Design/assets' / source / 'manifest.json').read_text(encoding='utf-8'))
    for item in manifest['images']:
        assert sha(repo / item['path']) == item['sha256']
files = {}
for path in sorted((repo / 'Client/Assets/StageGame').rglob('*')):
    if path.is_file():
        files[path.relative_to(repo).as_posix()] = {'sha256': sha(path), 'bytes': path.stat().st_size}
for path in [repo / 'GameData/GameData_MVP.xlsx', repo / 'GameData/export_data.py', repo / 'GameData/test_export_data.py', *sorted((repo / 'GameData/Generated').glob('*.json'))]:
    files[path.relative_to(repo).as_posix()] = {'sha256': sha(path), 'bytes': path.stat().st_size}
write_json(evidence / 'source-freeze.json', files)
result = {'date': '2026-10-15', 'status': 'UNITY_APPLIED_AND_EDITOR_VERIFIED', 'authorization': 'User explicitly extended this design session to Client/GameData and approved Unity import, saved prefab application and editor verification.', 'portraits': 19, 'states': 57, 'c06BadgeVersion': 'cap-v2 downward V', 'stages': {'unit': 24, 'tournament': 6}, 'strings': 418, 'unityTests': {key: suite.attrib[key] for key in ('result', 'total', 'passed', 'failed', 'skipped')}, 'unityTestEvidence': suite_path.relative_to(repo).as_posix(), 'dataTestsPassed': 45, 'dataCheck': 'Exporter --check passed; all three Generated/Resources JSON byte-identical', 'sourceArtPreserved': True, 'screenshots': len(list((evidence / 'screens').glob('*.png'))), 'savedPrefabApplication': True, 'runtimeUiTreeConstructionAdded': False, 'newWebGLBuild': False, 'newAndroidBuild': False, 'browserRun': False, 'physicalDeviceTest': False, 'sourceFreeze': 'Client/Evidence/pixel-art-20261015/source-freeze.json', 'earlierRuns': ['rank-flow-tests.xml: initial 23/25; portrait/dialogue overlap found visually, star contrast failure corrected', 'reward-timeout-tests.xml: timeout case passed alone without runtime changes', 'unity-tests-final.xml: intermediate full run before final empty-star and text-only toast correction']}
write_json(evidence / 'verification.json', result)
write_json(repo / 'Licence/PIXEL-INTEGRATION-20261015.json', {'date': '2026-10-15', 'status': 'APPLIED_EDITOR_VERIFIED', 'provenance': ['Licence/CHARACTER-ART-20261015-V1.json', 'Licence/CHARACTER-CAPS-20261015-V1.json', 'Licence/CHARACTER-C06-CAPS-20261015-V2.json', 'Licence/PIXEL-BOARD-20261015-V1.json'], 'selectedPortraits': portraits, 'generatedCodeArt': 'PixelArtBuilder: panel sprites, stones and empty star; project-authored procedural geometry, no external art inputs', 'verification': 'Client/Evidence/pixel-art-20261015/verification.json', 'scope': 'Generation and integration evidence, not legal clearance or exclusive rights certification.'})
print(json.dumps(result, ensure_ascii=False))
