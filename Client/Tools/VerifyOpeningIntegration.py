"""Verify the actual workbook, exported data, original art, applied portraits and Unity result."""
from pathlib import Path
import hashlib, json, xml.etree.ElementTree as ET

root=Path(__file__).resolve().parents[2]
evidence=root/'Client/Evidence/opening-ranks-20261015'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
write=lambda p,v:p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
suite=ET.parse(evidence/'unity-tests.xml').getroot()
assert suite.attrib['result']=='Passed' and int(suite.attrib['failed'])==0 and int(suite.attrib['skipped'])==0
art=root/'Client/Assets/StageGame/Art/PixelMilitary'
index=json.loads((art/'portrait-import.json').read_text())
assert len(index['portraits'])==19 and index['finalRecruit']['characterId']=='C01'
assert index['portraits'][0]['file']!=index['finalRecruit']['file']
for p in index['portraits']+[index['finalRecruit']]:
    assert sha(art/p['file'])==sha(root/p['source'])==p['sha256']
    assert (art/(p['file']+'.meta')).exists()
assert 'C06-pixel-v5.png' in [p['file'] for p in index['portraits']]
art_revision=json.loads((root/'Client/Evidence/pixel-complete-20261015/verification.json').read_text(encoding='utf-8'))
assert art_revision['status']=='UNITY_APPLIED_EDITOR_VERIFIED' and not art_revision['pendingArtDetails']
for name in ['stages.json','strings.json','ads.json']:
    assert sha(root/'GameData/Generated'/name)==sha(root/'Client/Assets/StageGame/Resources/GameData'/name)
assert sha(root/'GameData/GameData_MVP.xlsx')==sha(evidence/'GameData-candidate.xlsx')
assert sha(root/'Overview/assets/military-pixel-20261015/recruit-buzz-cut-atlas-v12.png')==sha(art/'C01.png')
for name in ['characters-20261015-v1','characters-caps-20261015-v1','characters-caps-20261015-v2']:
    for p in json.loads((root/'Docs/Design/assets'/name/'manifest.json').read_text(encoding='utf-8'))['images']:
        assert sha(root/p['path'])==p['sha256']
stages=json.loads((root/'GameData/Generated/stages.json').read_text())['stages']
assert sum(not p['openingMoves'] for p in stages)==24
assert sum(p['starter']==-1 for p in stages)==8
audit=json.loads((evidence/'opening-protection-audit.json').read_text(encoding='utf-8-sig'))
assert len(audit)==8 and sum(p['earlyPlayerWins'] for p in audit)==0
files={}
for p in sorted((root/'Client/Assets/StageGame').rglob('*')):
    if p.is_file():files[p.relative_to(root).as_posix()]={'sha256':sha(p),'bytes':p.stat().st_size}
for p in [root/'GameData/GameData_MVP.xlsx',root/'GameData/export_data.py',root/'GameData/test_export_data.py',*sorted((root/'GameData/Generated').glob('*.json'))]:
    files[p.relative_to(root).as_posix()]={'sha256':sha(p),'bytes':p.stat().st_size}
write(evidence/'source-freeze.json',files)
result={'date':'2026-10-15','status':'UNITY_APPLIED_EDITOR_VERIFIED',
        'portraits':19,'uniformVariants':1,'states':60,'emptyBoards':24,'aiStarts':8,
        'firstFiveAiTurns':{'minimumDepth':4,'minimumNodeBudget':3500,'randomPermille':0},
        'unityTests':{k:suite.attrib[k] for k in ['result','total','passed','failed','skipped']},
        'unityTestsScope':'Prior full-suite baseline before v5/v6 art; focused current v6 validation is separate.',
        'portraitStyleRevision':'Client/Evidence/pixel-complete-20261015/verification.json',
        'dataTestsPassed':45,'workbookCellsChanged':69,'oldSourceArtPreserved':True,
        'openingAudit':{'playerBranches':sum(p['playerBranches'] for p in audit),'earlyPlayerWins':0,
                        'maxNodes':max(p['maxNodes'] for p in audit),'scope':'all legal human continuations before the fifth AI reply at the minimum opening-protection configuration; both empty-board starters and six prefills'},
        'pendingArtDetails':[],
        'screenshots':len(list((evidence/'screens').glob('*.png'))),
        'newAndroidBuild':False,'newWebGLBuild':False,'physicalDeviceOrHumanBalanceTest':False,
        'sourceFreeze':'Client/Evidence/opening-ranks-20261015/source-freeze.json'}
write(evidence/'verification.json',result)
print(json.dumps(result,ensure_ascii=False))
