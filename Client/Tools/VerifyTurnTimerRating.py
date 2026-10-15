"""Record current source/build hashes and latest Unity coverage after timer/rating tests."""
from collections import Counter
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import xml.etree.ElementTree as ET

root=Path(__file__).resolve().parents[2]
client=root/'Client'
evidence=client/'Evidence/turn-timer-rating-20261015'
def read(path): return json.loads(path.read_text(encoding='utf-8-sig'))
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest().upper()
def save(name,value): (evidence/name).write_text(json.dumps(value,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

coverage={}
runs=[]
for filename in ('unity-tests.xml','unity-retest.xml'):
    tree=ET.parse(evidence/filename).getroot()
    occurrences=Counter()
    for case in tree.iter('test-case'):
        name=case.attrib['fullname'];occurrences[name]+=1
        coverage[f'{name}#{occurrences[name]}']={'result':case.attrib['result'],'source':filename}
    runs.append({'file':filename,**{key:tree.attrib.get(key) for key in ('total','passed','failed','skipped','result')}})
assert len(coverage)==110
assert all(case['result']=='Passed' for case in coverage.values()),[(name,case) for name,case in coverage.items() if case['result']!='Passed']
save('test-coverage.json',coverage)

baseline=read(client/'Evidence/heavy-stone-speech-source-freeze.json')
previous={entry['path']:entry['sha256'] for entry in baseline['files']}
files=[]
for path in sorted((client/'Assets').rglob('*')):
    if path.is_file():files.append({'path':path.relative_to(client).as_posix(),'bytes':path.stat().st_size,'sha256':sha(path)})
current={entry['path']:entry['sha256'] for entry in files}
changed=[path for path,value in current.items() if previous.get(path)!=value]
deleted=sorted(set(previous)-set(current))
assert not deleted,deleted
for path in changed:
    assert not any(part in path for part in ('/Art/','/Audio/','/Scenes/')),path
builds=read(client/'Evidence/easy-lemon-before.json')['builds']
for entry in builds:assert sha(client/entry['path'])==entry['sha256'].upper(),entry['path']
for name in ('stages','strings','ads'):
    assert sha(root/f'GameData/Generated/{name}.json')==sha(client/f'Assets/StageGame/Resources/GameData/{name}.json')
comparison=read(evidence/'bot-comparison.json')
assert len(comparison['stages'])==30 and comparison['games']==15360
assert all(row['after']['playerWins']>row['before']['playerWins'] for row in comparison['stages'])
for name in ('ko-1080-timer','en-1080-timer','ko-390-timer','en-390-timer','ko-320-timer','en-320-timer','ko-1080-timer-warning','en-1080-timer-warning','ko-390-timer-help','en-390-timer-help'):
    assert (evidence/(name+'.png')).exists(),name
stamp=datetime.now(timezone.utc).isoformat()
save('source-freeze.json',{'atUtc':stamp,'scope':'30-second auto placement and gentler rated AI profiles','files':files})
save('verification.json',{
    'status':'TURN_TIMER_AND_RATED_AI_VERIFIED','atUtc':stamp,
    'rules':{'playerTurnSeconds':30,'timeout':'Selected legal column, otherwise center-out legal fallback; same placement/result/sound flow','suspendedDuring':['AI turn','hint search/reward ad','pause/confirm/settings/help/notices','focus loss/application pause'],'resetOn':['new player turn','undo','retry/new stage'],'preserveOn':['selection','language refresh','modal cancel','hint']},
    'rating':{'type':'Internal design tiers, not measured human Elo','minimum':200,'maximum':1600,'profiles':15,'stagesPerProfile':2,'noOpeningStrengthOverride':True,'newSeedPerMatch':True,'calibration':comparison},
    'unity':{'runs':runs,'latestCaseCoverage':len(coverage),'passed':len(coverage),'failed':0,'skipped':0,'note':'One full run (108/110) plus focused rerun; old win/identical-retry fixtures updated for beginner oversights and per-match seeds.'},
    'data':{'testsPassed':46,'exporterCheck':True,'resourceHashesMatch':True,'strings':422,'workbookPreservation':'workbook-changes.json'},
    'visuals':{'inspectedSizes':['1080x1920','390x844','320x568'],'languages':['ko','en'],'topGapPreserved':True,'actionsFit1080WithoutScroll':True},
    'changedAssets':changed,'deletedAssets':deleted,'unchangedBuildFiles':len(builds),
    'notPerformed':['Human difficulty playtest','new Android/WebGL build','browser/device playtest','Git commit/push'],
    'handoff':'Management/Overview owners should add aiRating/aiOversightPermille and the 30-second turn rule to shared contracts.'
})
print(json.dumps({'latestPassingTests':len(coverage),'dataTests':46,'automaticGames':15360,'changedAssets':changed,'unchangedBuildFiles':len(builds)},ensure_ascii=False))
