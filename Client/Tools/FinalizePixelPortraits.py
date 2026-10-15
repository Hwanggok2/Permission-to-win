"""Freeze image_gen style edits and manually correct small rank counts, as authorized.

The selected generation and older source atlases remain unchanged. Only explicitly
listed insignia pixels are changed; faces, poses and the full alpha channel remain
byte-identical to each selected generation.
"""
from pathlib import Path
import hashlib
import json
import math
import shutil

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parents[2]
FOLDER = ROOT / 'Docs/Design/assets/characters-pixel-20261015-v5'
EVIDENCE = ROOT / 'Client/Evidence/insignia-pixels-20261015'
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()

# Each pair of boxes encloses only the existing two symbols, not buttons/borders.
# Centers follow the shoulder slope, with three smaller symbols fitting the sleeve.
PATCHES = {
    'C13': [
        ([(98,473,122,498),(128,464,151,488)], [(104,487),(126,480),(148,473)], 'diamond'),
        ([(393,482,416,506),(422,494,445,518)], [(398,491),(420,500),(442,509)], 'diamond'),
        ([(609,494,633,518),(638,484,662,508)], [(614,508),(636,500),(658,492)], 'diamond'),
        ([(896,485,918,509),(924,498,946,521)], [(900,494),(922,503),(944,512)], 'diamond'),
    ],
    'C16': [
        ([(116,513,138,536),(143,502,165,524)], [(119,528),(140,520),(161,512)], 'flower'),
        ([(380,513,402,535),(407,523,430,546)], [(383,521),(404,529),(425,537)], 'flower'),
        ([(594,522,616,545),(620,512,642,534)], [(596,537),(617,529),(638,521)], 'flower'),
        ([(881,513,903,535),(907,523,930,545)], [(884,521),(905,529),(926,537)], 'flower'),
        ([(1150,520,1173,543),(1177,509,1199,532)], [(1153,535),(1174,527),(1195,519)], 'flower'),
        ([(1409,515,1431,536),(1435,526,1457,547)], [(1412,523),(1433,531),(1454,539)], 'flower'),
    ],
    'C08': [
        ([(610,515,627,538),(624,509,641,532)], [(607,534),(622,528),(637,522)], 'chevron'),
    ],
}


def glyph(kind, angle):
    # Small details use a two-pixel grid; no smoothing or antialiasing is introduced.
    canvas = Image.new('L', (17,17))
    draw = ImageDraw.Draw(canvas)
    if kind == 'diamond':
        draw.polygon([(3,8),(8,4),(13,8),(8,12)],fill=255)
    elif kind == 'flower':
        rows = ['...###...','...###...','##.###.##','#########','.#######.','..#####..','.###.###.','.###.###.','..#...#..']
        for y,row in enumerate(rows):
            for x,c in enumerate(row):
                if c=='#':draw.point((x+4,y+4),fill=255)
    else:
        draw.line([(10,4),(6,8),(10,12)],fill=255,width=2)
    if kind != 'flower':
        canvas = canvas.rotate(-angle, resample=Image.Resampling.NEAREST)
    return canvas.resize((34,34),Image.Resampling.NEAREST)


def correct(image, patches):
    before=np.array(image)
    rgb=image.convert('RGB')
    allowed=Image.new('L',image.size)
    record=[]
    for boxes,centers,kind in patches:
        # Pick the existing flat cloth color beside each insignia; erase its black
        # silhouette only, leaving the cloth pattern and strap outline untouched.
        for box in boxes:
            region=np.array(rgb.crop(box))
            dark=Image.fromarray(np.uint8(region.max(axis=2)<75)*255)
            dark=dark.filter(ImageFilter.MaxFilter(3))
            cloth=region[(region.min(axis=2)>95)&(region.max(axis=2)<220)]
            color=tuple(np.median(cloth,axis=0).astype(int)) if len(cloth) else (174,175,133)
            rgb.paste(color,box,dark)
            allowed.paste(255,box)
        angle=math.degrees(math.atan2(centers[-1][1]-centers[0][1],centers[-1][0]-centers[0][0]))
        for x,y in centers:
            mask=glyph(kind,angle)
            rgb.paste((16,18,13),(x-17,y-17),mask)
            allowed.paste(255,(x-17,y-17,x+17,y+17))
        record.append({'eraseBoxes':boxes,'centers':centers,'kind':kind,'count':len(centers),'angleDegrees':angle})
    result=rgb.convert('RGBA')
    result.putalpha(image.getchannel('A'))
    changed=np.any(np.array(result)!=before,axis=2)
    assert not np.any(changed & (np.array(allowed)==0))
    assert np.array_equal(np.array(result)[:,:,3],before[:,:,3])
    return result, {'regions':record,'changedPixels':int(changed.sum()),'alphaUnchanged':True,'outsideListedRegionsUnchanged':True}


def main():
    selected=json.loads((FOLDER/'selection.json').read_text(encoding='utf-8'))['selected']
    manifest=json.loads((ROOT/'Docs/Design/assets/characters-uniform-20261015-v4/manifest.json').read_text(encoding='utf-8'))
    corrections={}
    for e in selected:
        cid=e['id'];source=FOLDER/'generated'/f'{cid}-pixel-generated.png'
        target=FOLDER/f'{cid}-pixel-v5.png'
        image=Image.open(source)
        assert image.mode=='RGBA' and image.size==(1536,1024)
        if cid in PATCHES:
            image,proof=correct(image,PATCHES[cid]);image.save(target)
            corrections[cid]={'source':source.relative_to(ROOT).as_posix(),'beforeSha256':sha(source),'afterSha256':sha(target),**proof}
        else:shutil.copyfile(source,target)
        previous=next(p for p in manifest['images'] if p['id']==cid)
        previous.update({'path':target.relative_to(ROOT).as_posix(),'sha256':sha(target),
                         'previousVersion':f'Docs/Design/assets/characters-uniform-20261015-v4/{cid}-uniform-v4.png',
                         'source':e['generatedPath'],'rawGeneratedCopy':source.relative_to(ROOT).as_posix(),
                         'rawGeneratedSha256':sha(source),'key':f'pixel-v5-{cid}','manualRankCorrection':cid in PATCHES})
    manifest.update({'status':'READY_FOR_UNITY_VALIDATION','styleRevision':'Coarse stepped pixel contours matching the original recruit; 15 characters / 45 states redrawn.',
                     'runtimeIntegrated':False,'pendingArtDetails':[],
                     'verification':'Client/Evidence/insignia-pixels-20261015/verification.json',
                     'manualEditAuthorization':'2026-10-15 user: 픽셀 단위로 직접 수정해',
                     'manualEditTool':'Pillow; Client/Tools/FinalizePixelPortraits.py',
                     'manualEdits':'Docs/Design/assets/characters-pixel-20261015-v5/manual-corrections.json'})
    manifest['promptSets'].append('Docs/Design/assets/characters-pixel-20261015-v5/prompts.json')
    (FOLDER/'manual-corrections.json').write_text(json.dumps(corrections,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    (FOLDER/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'redrawnCharacters':len(selected),'manuallyCorrected':list(corrections),'states':60}))


if __name__=='__main__':main()
