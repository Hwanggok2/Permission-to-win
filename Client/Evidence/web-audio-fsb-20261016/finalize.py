import datetime, hashlib, json, xml.etree.ElementTree as ET
from pathlib import Path

folder=Path(__file__).resolve().parent
client=folder.parents[1]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest().upper()
def save(name,value):(folder/name).write_text(json.dumps(value,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
before=json.loads((folder/'before.json').read_text())
changes=[]
for entry in before['assets']:
    path=client/entry['path']
    if not path.exists() or sha(path)!=entry['sha256']:changes.append(entry['path'])
assert not any(path.startswith(('Assets/StageGame/Art/','Assets/StageGame/Audio/','Assets/StageGame/Resources/','Assets/StageGame/Scenes/','Assets/StageGame/Scripts/Core/')) for path in changes),changes
prefabs=[path for path in changes if '/Prefabs/' in path]
assert prefabs==['Assets/StageGame/Prefabs/RankUI/RankAppRoot.prefab'],prefabs
for entry in before['builds']:assert sha(client/entry['path'])==entry['sha256'],entry['path']
latest={}
runs=[]
for filename in ('unity-tests.xml','unity-audio-tests.xml'):
    root=ET.parse(folder/filename).getroot()
    runs.append(dict(path=filename,total=int(root.get('total')),passed=int(root.get('passed')),failed=int(root.get('failed'))))
    for test in root.iter('test-case'):latest[test.get('fullname')]=dict(result=test.get('result'),run=filename)
assert all(test['result']=='Passed' for test in latest.values()),latest
preserved=[entry['path'] for entry in before['assets'] if entry['path'] not in changes]
new=[path.relative_to(client).as_posix() for path in (client/'Assets').rglob('*') if path.is_file() and path.relative_to(client).as_posix() not in {e['path'] for e in before['assets']}]
save('verification.json',dict(status='CLIENT_SOURCE_READY_FOR_WEBGL_REBUILD_AND_DEVICE_RETEST',timestamp=datetime.datetime.now(datetime.timezone.utc).isoformat(),unity=dict(uniqueTests=len(latest),passed=len(latest),runs=runs,latestPerTest=latest,initialFailures='Two exact imported-PCM equality assertions encountered resampler/codec ringing. Source WAV bytes now compared exactly, imported peak checked separately; both corrected tests passed.'),adapterChecks=10,renderedScreens=len(list((folder/'screens').glob('*.png'))),audioReport='codec-report.json',actualUnityImport='unity-import/import-settings.json',viewportMeasurements='screens/viewport-metrics.json',changedExistingAssets=changes,newAssets=new,preserved=dict(originalAudio=True,art=True,gameData=True,core=True,scenes=True,otherPrefabs=True,assets=len(preserved),buildFiles=len(before['builds'])),limits=['No new WebGL or Android player build/deployment in Client session.','Unity RenderTexture previews use Expand-equivalent CanvasScaler math, not real iPhone execution.','Real iPhone Safari/Kakao decoding and visualViewport behavior require retest after coordinator rebuild.','Guarded effects add about 46 ms of onset latency.'],localBrowserProbe='file:// page open was rejected by browser-tool URL policy; no workaround was used. Coordinator independently supplied desktop Chromium codec evidence.'))
files=[]
for directory in ('Assets','Packages','ProjectSettings'):
    for path in sorted((client/directory).rglob('*')):
        if path.is_file():files.append(dict(path=path.relative_to(client).as_posix(),bytes=path.stat().st_size,sha256=sha(path)))
save('source-freeze.json',dict(timestamp=datetime.datetime.now(datetime.timezone.utc).isoformat(),purpose='WebGL audio and mobile viewport handoff; no further Client source edits expected',files=files))
print(json.dumps(dict(uniqueUnityTests=len(latest),changedAssets=changes,newAssets=new,buildsPreserved=len(before['builds']),frozenFiles=len(files)),ensure_ascii=False,indent=2))
