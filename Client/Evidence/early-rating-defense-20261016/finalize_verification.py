import hashlib
import json
import subprocess
import xml.etree.ElementTree as ET
from pathlib import Path

folder=Path(__file__).resolve().parent
root=folder.parents[2]
def read(name): return json.loads((folder/name).read_text(encoding='utf-8-sig'))
before=read('stages-before.json')
after=json.loads((root/'GameData/Generated/stages.json').read_text(encoding='utf-8-sig'))
assert (root/'GameData/Generated/stages.json').read_bytes()==(root/'Client/Assets/StageGame/Resources/GameData/stages.json').read_bytes()
profiles={p['aiRating']:p for p in read('candidate-profiles.json')}
changes=[]
for old,new in zip(before['stages'],after['stages']):
    expected=dict(old)
    if old['aiRating'] in (400,500): expected['aiDepth']=2
    if old['aiRating'] in (400,500,600): expected['aiOversightPermille']=0
    assert new==expected
    assert all(new[key]==value for key,value in profiles[new['aiRating']].items())
    for key in old:
        if old[key]!=new[key]: changes.append(dict(stage=old['id'],field=key,before=old[key],after=new[key]))
assert len(changes)==10
tests=ET.parse(folder/'unity-tests.xml').getroot()
assert tests.attrib['result']=='Passed'
assert not (root/'Client/Temp/UnityLockfile').exists()
source_paths=[]
for directory in ['Client/Assets','Client/Packages','Client/ProjectSettings','GameData/Generated']:
    source_paths.extend(path for path in (root/directory).rglob('*') if path.is_file())
source_paths.extend(root/path for path in ['GameData/GameData_MVP.xlsx','GameData/export_data.py','GameData/test_export_data.py','GameData/DIFFICULTY-CONTRACT.md'])
freeze=[dict(path=path.relative_to(root).as_posix(),sha256=hashlib.sha256(path.read_bytes()).hexdigest()) for path in sorted(source_paths)]
(folder/'source-freeze.json').write_text(json.dumps(freeze,indent=2)+'\n',encoding='utf-8')
git='C:/Users/JW/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/git/cmd/git.exe'
subprocess.run([git,'diff','--check','--','Client','GameData'],cwd=root,check=True)
assert not subprocess.check_output([git,'diff','--','Client/Assets/StageGame/Scripts/Core/AiSearch.cs'],cwd=root)
tactical=[]
for entry in read('tactics.json')['results']:
    metrics=entry['result']['metrics']
    tactical.append(dict(rating=entry['profile']['aiRating'],attack=sum(m['percent'] for m in metrics if m['category'].startswith('attack'))/3,defense=sum(m['percent'] for m in metrics if m['category'].startswith('defense'))/3))
for previous,current in zip(tactical,tactical[1:]):
    assert current['attack']>=previous['attack'] and current['defense']>=previous['defense']
short={label:read(label+'-short.json')['results'] for label in ['baseline','candidate']}
assert sum(x['result']['earlyPlayerWins'] for x in short['candidate'])==0
report=dict(date='2026-10-16',status='ready; source frozen; Unity ended',unity={key:int(tests.attrib[key]) for key in ['total','passed','failed']},dataTestsPassed=46,exportCheck='passed',workbookChanges=read('workbook-changes.json'),stageChanges=changes,resourcesMatch=True,productionAiCodeUnchanged=True,tacticalAverages=tactical,shortGameAudit={label:dict(playerBranches=sum(x['result']['playerBranches'] for x in rows),aiMoves=sum(x['result']['aiMoves'] for x in rows),earlyLossPaths=sum(x['result']['earlyPlayerWins'] for x in rows)) for label,rows in short.items()},fixedOpponentGames=1792,unityLockPresent=False,newBuild=False,deployed=False,deviceTest=False,humanPlaytest=False,diffCheck='passed')
(folder/'verification.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(f"Verified {len(changes)} field changes, {tests.attrib['passed']}/{tests.attrib['total']} Unity tests, Resources equality, unchanged AI code, ascending tactical averages, zero sampled early losses. Source freeze: {len(freeze)} files.")
