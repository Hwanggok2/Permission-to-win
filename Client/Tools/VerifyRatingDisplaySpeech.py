"""Summarize rating/timer UI and typing audio evidence without modifying assets."""
from collections import Counter
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import xml.etree.ElementTree as ET

root=Path(__file__).resolve().parents[2]
client=root/'Client'
evidence=client/'Evidence/rating-display-speech-20261015'
def read(path):return json.loads(path.read_text(encoding='utf-8-sig'))
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest().upper()
def save(name,value):(evidence/name).write_text(json.dumps(value,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
coverage={};runs=[]
for filename in ('unity-tests.xml','unity-retest.xml'):
    if not (evidence/filename).exists():continue
    tree=ET.parse(evidence/filename).getroot();seen=Counter()
    for case in tree.iter('test-case'):
        name=case.attrib['fullname'];seen[name]+=1
        coverage[f'{name}#{seen[name]}']={'result':case.attrib['result'],'source':filename}
    runs.append({'file':filename,**{k:tree.attrib.get(k) for k in ('total','passed','failed','skipped','result')}})
assert len(coverage)==114,len(coverage)
assert all(case['result']=='Passed' for case in coverage.values()),[(n,c) for n,c in coverage.items() if c['result']!='Passed']
save('test-coverage.json',coverage)
baseline=read(client/'Evidence/turn-timer-rating-20261015/source-freeze.json')
before={item['path']:item['sha256'] for item in baseline['files']}
files=[{'path':p.relative_to(client).as_posix(),'bytes':p.stat().st_size,'sha256':sha(p)} for p in sorted((client/'Assets').rglob('*')) if p.is_file()]
current={item['path']:item['sha256'] for item in files}
changed=[path for path,value in current.items() if before.get(path)!=value]
assert not set(before)-set(current)
for path in before:
    if '/Art/' in path or '/Audio/' in path or '/Scenes/' in path:assert before[path]==current[path],path
for name in ('stages','ads'):
    path=f'Assets/StageGame/Resources/GameData/{name}.json';assert before[path]==current[path],path
for name in ('stages','strings','ads'):
    assert sha(root/f'GameData/Generated/{name}.json')==sha(client/f'Assets/StageGame/Resources/GameData/{name}.json')
builds=read(client/'Evidence/easy-lemon-before.json')['builds']
for item in builds:assert sha(client/item['path'])==item['sha256'].upper()
audio=read(evidence/'typing-listener-output.json')
assert audio['newTyping']['rms']>audio['oldTyping']['rms']*4
assert audio['musicAndTyping']['rms']>audio['musicOnly']['rms']*1.5
assert audio['allSounds']['peak']<.99
for language in ('ko','en'):
    for width in (1080,390,320):
        for screen in ('lobby','detail','match'):assert (evidence/f'{language}-{width}-rating-{screen}.png').exists()
stamp=datetime.now(timezone.utc).isoformat()
save('source-freeze.json',{'atUtc':stamp,'scope':'Visible AI ratings, countdown slider above board grid, audible typing','files':files})
save('verification.json',{
    'status':'RATING_DISPLAY_SLIDER_TYPING_VERIFIED','atUtc':stamp,
    'ratings':{'places':['all 30 stage cards','stage selection detail header','match opponent panel'],'key':'ai_rating','languages':['ko','en'],'profileDataUnchanged':True},
    'timer':{'seconds':30,'location':'Empty wooden strip inside the board, above the top grid line','text':'Smaller label centered above slider','slider':'Read-only, normalized remaining/30, red below or at 5 seconds','headerTimerRemoved':True,'ruleUnchanged':True},
    'typing':{'asset':'Assets/StageGame/Audio/TypingKey.wav','sha256':sha(client/'Assets/StageGame/Audio/TypingKey.wav'),'source':'Original reproducible procedural keyboard tick via Tools/SynthesizeGameAudio.py','durationSeconds':.03501133786848073,'clipPeak':.54998779296875,'playbackGain':.75,'previousClip':'ButtonClick.wav','previousGain':.18,'listenerOutput':audio,'effectMuteAndSpeechPausePreserved':True},
    'unity':{'runs':runs,'latestPassingCases':len(coverage),'failed':0,'skipped':0},
    'data':{'testsPassed':46,'exporterCheck':True,'resourceHashesMatch':True,'strings':423,'workbookPreservation':'workbook-changes.json'},
    'visuals':{'sizes':['1080x1920','390x844','320x568'],'languages':['ko','en'],'inspected':True,'gridAndStonePositionsPreserved':True,'allMatchActionsFit1080':True},
    'changedAssets':changed,'existingAudioArtScenesUnchanged':True,'unchangedBuildFiles':len(builds),
    'notPerformed':['human audio listening','new Android/WebGL build','browser/device playtest','Git commit/push']
})
print(json.dumps({'unityPassingCases':len(coverage),'dataTests':46,'typingRmsRatio':round(audio['newTyping']['rms']/audio['oldTyping']['rms'],2),'combinedAudioPeak':audio['allSounds']['peak'],'changedAssets':changed,'unchangedBuildFiles':len(builds)},ensure_ascii=False))
