"""Check selected artwork, local edits, saved imports and the focused Unity run."""
from pathlib import Path
import hashlib
import json
import re
import xml.etree.ElementTree as ET

import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
FOLDER=ROOT/'Docs/Design/assets/characters-pixel-20261015-v6'
ART=ROOT/'Client/Assets/StageGame/Art/PixelMilitary'
EVIDENCE=ROOT/'Client/Evidence/pixel-complete-20261015'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
write=lambda p,v:p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

manifest=read(FOLDER/'manifest.json')
index=read(ART/'portrait-import.json')
assert len(index['portraits'])==19
all_portraits=index['portraits']+[index['finalRecruit']]
assert len(manifest['images'])==len(all_portraits)==20
selected={e['id']:e for e in manifest['images']}
theme=(ROOT/'Client/Assets/StageGame/Art/RankUI/RankTheme.asset').read_text(encoding='utf-8')
regular_theme,final_theme=theme.split('  finalRecruit:')
references=[]
for i,p in enumerate(all_portraits):
    cid=p['characterId'] if i<19 else 'C01Final'
    e=selected[cid]
    expected=(cid+'-uniform-v4.png' if cid in ('C01','C01Final') else
              cid+'-pixel-v6.png' if cid in ('C02','C03','C04') else cid+'-pixel-v5.png')
    assert p['file']==expected,(cid,p['file'])
    assert e['sha256']==p['sha256']==sha(ROOT/e['path'])==sha(ART/p['file'])
    im=Image.open(ART/p['file']);assert im.mode=='RGBA' and im.size==(1536,1024)
    alpha=np.asarray(im)[:,:,3];assert alpha.min()==0 and alpha.max()>=250
    meta=(ART/(p['file']+'.meta')).read_text(encoding='utf-8')
    assert re.search(r'filterMode: 0\b',meta) and re.search(r'enableMipMap: 0\b',meta)
    platforms=meta.split('  platformSettings:')[1].split('  spriteSheet:')[0]
    default=platforms.split('buildTarget: DefaultTexturePlatform')[1].split('- serializedVersion:')[0]
    assert 'textureCompression: 0' in default
    assert 'overridden: 1' not in platforms
    assert len(re.findall(r'      name: C\d\d_(?:intro|thinking|defeat)',meta))==3
    guid=re.search(r'^guid: (\w+)',meta,re.M)[1]
    block=(regular_theme.split('  - characterId: '+cid+'\n')[1].split('  - characterId:')[0]
           if i<19 else final_theme)
    states={}
    for state in ('intro','thinking','defeat'):
        ref=re.search(state+r': \{fileID: (-?\d+), guid: (\w+), type: 3\}',block)
        assert ref and ref[2]==guid and int(ref[1])!=0,(cid,state)
        states[state]=int(ref[1])
    assert len(set(states.values()))==3
    references.append({'id':cid,'runtimeFile':p['file'],'source':e['path'],'sha256':p['sha256'],'guid':guid,'states':states})
write(EVIDENCE/'runtime-reference-audit.json',references)

for e in read(ROOT/'Docs/Design/assets/characters-uniform-20261015-v4/manifest.json')['images']:
    assert sha(ROOT/e['path'])==e['sha256']

corrections=read(ROOT/manifest['manualEdits'])
for cid,e in corrections.items():
    source=ROOT/e['source'];target=ROOT/selected[cid]['path']
    assert sha(source)==e['beforeSha256'] and sha(target)==e['afterSha256']
    before=np.array(Image.open(source));after=np.array(Image.open(target))
    assert np.array_equal(before[:,:,3],after[:,:,3])
    allowed=np.zeros(before.shape[:2],dtype=bool)
    for region in e['regions']:
        assert region['count']==3
        for x0,y0,x1,y1 in region['eraseBoxes']:allowed[y0:y1,x0:x1]=True
        for x,y in region['centers']:allowed[y-17:y+17,x-17:x+17]=True
    changed=np.any(before!=after,axis=2)
    assert not np.any(changed & ~allowed)
    assert int(changed.sum())==e['changedPixels']

early_corrections=read(ROOT/manifest['earlyManualEdits'])
for e in early_corrections:
    source=ROOT/e['source'];target=ROOT/e['target']
    assert sha(source)==e['beforeSha256'] and sha(target)==e['afterSha256']
    before=np.array(Image.open(source));after=np.array(Image.open(target))
    assert np.array_equal(before[:,:,3],after[:,:,3])
    allowed=np.zeros(before.shape[:2],dtype=bool)
    x0,y0,x1,y1=e['allowedBox'];allowed[y0:y1,x0:x1]=True
    changed=np.any(before!=after,axis=2)
    assert changed.any() and not np.any(changed & ~allowed)
    assert e['expectedStripes']==1

suite=ET.parse(EVIDENCE/'unity-tests.xml').getroot()
assert suite.attrib['result']=='Passed' and int(suite.attrib['total'])==3
assert int(suite.attrib['failed'])==int(suite.attrib['skipped'])==0
stages=read(ROOT/'GameData/Generated/stages.json')['stages']
first_stages={}
for stage in stages:first_stages.setdefault(stage['characterId'],stage['id'])
cases=[(stage,cid) for cid,stage in first_stages.items()]+[(26,'C03'),(30,'C01')]
expected_screens={f'stage-{stage:02d}-{cid}-{state}-{width}.png'
                  for stage,cid in cases for state in ('intro','thinking','defeat')
                  for width in ([390,1080] if cid=='C03' else [390])}
screens=list((EVIDENCE/'screens').glob('*.png'))
assert {p.name for p in screens}==expected_screens and len(screens)==69
for p in screens:
    assert Image.open(p).size==((390,844) if '-390' in p.name else (1080,1920))

freeze={}
for p in all_portraits:
    for path in [ART/p['file'],ART/(p['file']+'.meta'),ROOT/p['source']]:
        freeze[path.relative_to(ROOT).as_posix()]=sha(path)
for path in [ART/'portrait-import.json',ROOT/'Client/Assets/StageGame/Art/RankUI/RankTheme.asset',
             ROOT/'Client/Assets/StageGame/Prefabs/RankUI/Match.prefab',ROOT/'Client/Assets/StageGame/Prefabs/RankUI/RankAppRoot.prefab',
             ROOT/'Client/Assets/StageGame/Editor/PixelArtBuilder.cs',ROOT/'Client/Assets/StageGame/Tests/EditMode/PixelArtTests.cs']:
    freeze[path.relative_to(ROOT).as_posix()]=sha(path)
write(EVIDENCE/'source-freeze.json',freeze)
visual=read(EVIDENCE/'visual-review.json')
assert visual['status']=='REVIEWED' and visual['reviewedGameCaptures']==69
report={'date':'2026-10-15','status':'UNITY_APPLIED_EDITOR_VERIFIED','redrawnCharacters':18,'redrawnStates':54,
        'thisRevisionCharacters':['C02','C03','C04'],'thisRevisionStates':9,
        'selectedAtlases':20,'selectedStates':60,'manualCorrections':list(corrections)+[e['id'] for e in early_corrections],'pendingArtDetails':[],
        'manualRankReview':'C02 defeat corrected to one stripe; prior C08 three chevrons, C13 three diamonds and C16 three flowers retained. Raised hands naturally obscure portions of other insignia/flags.',
        'styleReview':visual,
        'unityTests':{k:suite.attrib[k] for k in ['result','total','passed','failed','skipped']},
        'unityTestScope':'Portrait selection through 30 stages and match states, explicit selected revision for every character; point/no-mipmap/uncompressed imports; saved prefab references; all 19 characters plus final variant and C03 tournament appearance in three states. C03 stages 3 and 26 also captured at 1080 width.',
        'runtimeReferenceAudit':'Client/Evidence/pixel-complete-20261015/runtime-reference-audit.json',
        'captures':69,'captureWidths':[390,1080],'previousV4AtlasesPreserved':20,'manualAlphaAndOutsideRegionsPreserved':True,
        'priorFullSuite':'Client/Evidence/opening-ranks-20261015/unity-tests.xml (87 passed before this art revision; not rerun)',
        'newAndroidBuild':False,'newWebGLBuild':False,'physicalDeviceTest':False,
        'sourceFreeze':'Client/Evidence/pixel-complete-20261015/source-freeze.json'}
write(EVIDENCE/'verification.json',report)
manifest.update({'status':report['status'],'runtimeIntegrated':True})
write(FOLDER/'manifest.json',manifest)
print(json.dumps(report,ensure_ascii=False))
