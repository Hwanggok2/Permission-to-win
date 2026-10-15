"""Copy versioned final portrait files without pixel edits and measure shared sprite crops."""
from pathlib import Path
from PIL import Image
import hashlib
import json
import shutil

ROOT = Path(__file__).resolve().parents[2]
MANIFEST = ROOT / 'Docs/Design/assets/characters-pixel-20261015-v6/manifest.json'
ART = ROOT / 'Client/Assets/StageGame/Art/PixelMilitary'

def prepare():
    manifest = json.loads(MANIFEST.read_text(encoding='utf-8'))
    portraits, final = [], None
    for entry in manifest['images']:
        source = ROOT / entry['path']
        digest = hashlib.sha256(source.read_bytes()).hexdigest()
        assert digest == entry['sha256']
        target = ART / source.name
        if target.exists() and target.read_bytes() != source.read_bytes():
            raise RuntimeError(f'Refusing to overwrite existing artwork: {target}')
        shutil.copyfile(source, target)
        with Image.open(source) as im:
            assert im.mode == 'RGBA' and im.size == (1536, 1024)
            alpha = im.getchannel('A')
            lo,hi=alpha.getextrema()
            assert lo==0 and hi>=250, f'No real alpha: {source}'
            bounds = [alpha.crop((i*512,0,(i+1)*512,1024)).point(lambda a:255 if a>=128 else 0).getbbox() for i in range(3)]
            assert all(bounds)
            left=max(0,min(b[0] for b in bounds)-2)
            top=max(0,min(b[1] for b in bounds)-2)
            right=min(512,max(b[2] for b in bounds)+2)
            bottom=min(1024,max(b[3] for b in bounds)+2)
        item={'characterId':'C01' if entry['id']=='C01Final' else entry['id'], 'file':target.name,
              'x':left,'y':1024-bottom,'width':right-left,'height':bottom-top,
              'source':entry['path'],'sha256':digest}
        if entry['id']=='C01Final': final=item
        else: portraits.append(item)
    assert len(portraits)==19 and final
    (ART/'portrait-import.json').write_text(json.dumps({'portraits':portraits,'finalRecruit':final},ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'portraits':len(portraits),'uniformVariants':1,'states':60,'manifest':MANIFEST.relative_to(ROOT).as_posix(),'realAlpha':True}))

if __name__=='__main__': prepare()
