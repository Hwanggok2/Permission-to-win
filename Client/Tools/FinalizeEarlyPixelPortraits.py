"""Select the three early-stage pixel revisions without replacing older sources."""
from pathlib import Path
import hashlib
import json
import shutil
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
FOLDER=ROOT/'Docs/Design/assets/characters-pixel-20261015-v6'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
write=lambda p,v:p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

manifest=json.loads((ROOT/'Docs/Design/assets/characters-pixel-20261015-v5/manifest.json').read_text(encoding='utf-8'))
selected=json.loads((FOLDER/'selection.json').read_text(encoding='utf-8'))['selection']
corrections=[]
for e in selected:
    cid=e['id'];source=FOLDER/'generated'/f'{cid}-pixel-generated.png';target=FOLDER/f'{cid}-pixel-v6.png'
    if cid=='C02':
        im=Image.open(source).convert('RGBA');rgb=im.convert('RGB')
        box=(1326,679,1372,695)
        for y in range(box[1],box[3]):
            left=rgb.getpixel((1325,y));right=rgb.getpixel((1373,y))
            for x in range(box[0],box[2]):
                t=(x-1325)/48
                rgb.putpixel((x,y),tuple(round(a+(b-a)*t) for a,b in zip(left,right)))
        corrected=rgb.convert('RGBA');corrected.putalpha(im.getchannel('A'));corrected.save(target)
        corrections.append({'id':cid,'reason':'Remove second stripe from private defeat pose; retain the single stripe on the pocket patch.',
                            'source':source.relative_to(ROOT).as_posix(),'target':target.relative_to(ROOT).as_posix(),
                            'beforeSha256':sha(source),'afterSha256':sha(target),'allowedBox':box,'expectedStripes':1,
                            'authorization':'User authorized direct pixel corrections on 2026-10-15.'})
    else:shutil.copyfile(source,target)
    entry=next(p for p in manifest['images'] if p['id']==cid)
    entry.update({'previousVersion':entry['path'],'path':target.relative_to(ROOT).as_posix(),'sha256':sha(target),
                  'source':e['source'],'rawGeneratedCopy':source.relative_to(ROOT).as_posix(),'rawGeneratedSha256':sha(source),
                  'key':f'pixel-v6-{cid}','manualRankCorrection':cid=='C02'})
manifest.update({'status':'READY_FOR_UNITY_VALIDATION','runtimeIntegrated':False,
                 'styleRevision':'C02-C04 refreshed after early-stage omissions were identified; C01 trainee/final variant retained as style anchor; C05-C19 retain v5.',
                 'verification':'Client/Evidence/pixel-complete-20261015/verification.json',
                 'manualEditTool':'Pillow; Client/Tools/FinalizePixelPortraits.py and Client/Tools/FinalizeEarlyPixelPortraits.py',
                 'earlyManualEdits':'Docs/Design/assets/characters-pixel-20261015-v6/manual-corrections.json'})
manifest['promptSets'].append('Docs/Design/assets/characters-pixel-20261015-v6/prompts.json')
write(FOLDER/'manual-corrections.json',corrections);write(FOLDER/'manifest.json',manifest)
print('Selected C02/C03/C04 v6; kept all other portraits and older source assets.')
