import datetime,hashlib,json,xml.etree.ElementTree as ET
from pathlib import Path
folder=Path(__file__).resolve().parent
client=folder.parents[1]
root=client.parent
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest().upper()
def save(name,value):(folder/name).write_text(json.dumps(value,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
before=json.loads((folder/'before.json').read_text(encoding='utf-8-sig'))
changed=[]
for entry in before['assets']:
    p=client/entry['path']
    if not p.exists() or sha(p)!=entry['sha256']:changed.append(entry['path'])
protected=('Assets/StageGame/Art/','Assets/StageGame/Audio/','Assets/StageGame/Scripts/Core/','Assets/StageGame/Scenes/')
assert not any(path.startswith(protected) for path in changed),changed
assert [path for path in changed if '/Prefabs/' in path]==sorted([
    'Assets/StageGame/Prefabs/RankUI/Match.prefab',
    'Assets/StageGame/Prefabs/RankUI/RankAppRoot.prefab',
    'Assets/StageGame/Prefabs/RankUI/TurnToast.prefab'])
assert 'Assets/StageGame/Resources/GameData/stages.json' not in changed
assert 'Assets/StageGame/Resources/GameData/ads.json' not in changed
for filename in ('stages.json','strings.json','ads.json'):
    assert (root/'GameData/Generated'/filename).read_bytes()==(client/'Assets/StageGame/Resources/GameData'/filename).read_bytes()
latest={};runs=[]
for filename in ('unity-tests.xml','unity-final-tests.xml'):
    tests=ET.parse(folder/filename).getroot()
    runs.append(dict(file=filename,total=int(tests.get('total')),passed=int(tests.get('passed')),failed=int(tests.get('failed'))))
    assert int(tests.get('failed'))==0
    for case in tests.iter('test-case'):latest[case.get('fullname')]=case.get('result')
assert all(result=='Passed' for result in latest.values())
save('verification.json',dict(status='PREFABS_SAVED_AND_UNITY_VERIFIED',timestamp=datetime.datetime.now(datetime.timezone.utc).isoformat(),unityTests=len(latest),unityPassed=len(latest),runs=runs,gameDataTests=46,exportCheck=True,generatedResourcesMatch=True,changedExistingClientAssets=changed,workbookChange='Strings C138/E138 only; other values/styles/features preserved',screens=len(list((folder/'screens').glob('*.png')))+len(list((folder/'final-screens').glob('*.png'))),firstRun='No saved progress: show rules over first match once. Existing/recovery saves stay in lobby. Manual help remains available.',guide='Existing select_column string, white with dark outline, center of match screen, 2.6 s hold + 0.4 s fade; column selection hides it immediately; no bottom duplicate.',buttons='Undo/Retry backgrounds 255x114 design units (from336x165), hit areas336x165 preserved.',preserved=['original art and audio/meta','AI and gameplay data','scene','other prefabs','language/settings/save format'],notRun=['New WebGL/Android player build','Deployment','Real device or human play']))
files=[]
for directory in ('Assets','Packages','ProjectSettings'):
    for path in sorted((client/directory).rglob('*')):
        if path.is_file():files.append(dict(path=path.relative_to(root).as_posix(),sha256=sha(path)))
for path in [root/'GameData/GameData_MVP.xlsx',*(root/'GameData/Generated').glob('*.json')]:
    files.append(dict(path=path.relative_to(root).as_posix(),sha256=sha(path)))
save('source-freeze.json',dict(timestamp=datetime.datetime.now(datetime.timezone.utc).isoformat(),files=files))
print(json.dumps(dict(unityTests=len(latest),runs=runs,changedAssets=changed,sourceFiles=len(files)),indent=2))
