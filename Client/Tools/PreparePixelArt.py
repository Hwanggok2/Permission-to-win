"""Copy approved project art without changing pixels; measure uniform sprite cells."""
from pathlib import Path
from PIL import Image
import hashlib, json, shutil

repo = Path(__file__).resolve().parents[2]
if (repo/'Docs/Design/assets/characters-pixel-20261015-v6/manifest.json').exists():
    from PrepareCharacterRefresh import prepare
    prepare()
    raise SystemExit(0)
dest = repo/'Client/Assets/StageGame/Art/PixelMilitary'
dest.mkdir(parents=True, exist_ok=True)
base = repo/'Docs/Design/assets/characters-20261015-v1'
caps = json.loads((repo/'Docs/Design/assets/characters-caps-20261015-v2/manifest.json').read_text(encoding='utf-8'))
cap_sources = {item['id']: repo/item['path'] for item in caps['images']}
items = []
for number in range(1,20):
    cid = f'C{number:02}'
    source = repo/'Overview/assets/military-pixel-20261015/recruit-buzz-cut-atlas-v12.png' if number == 1 else cap_sources[cid] if cid in cap_sources else next(base.glob(cid+'-*.png'))
    target = dest/(cid+('-cap-v2' if number == 6 else '')+'.png')
    if target.exists() and target.read_bytes() != source.read_bytes():
        raise RuntimeError(f'Refusing to overwrite changed asset: {target}')
    shutil.copyfile(source,target)
    with Image.open(source) as image:
        assert image.mode == 'RGBA' and image.size == (1536,1024)
        alpha = image.getchannel('A')
        bounds = [alpha.crop((i*512,0,(i+1)*512,1024)).point(lambda a:255 if a>=128 else 0).getbbox() for i in range(3)]
        left = max(0,min(b[0] for b in bounds)-2)
        top = max(0,min(b[1] for b in bounds)-2)
        right = min(512,max(b[2] for b in bounds)+2)
        bottom = min(1024,max(b[3] for b in bounds)+2)
    items.append({'characterId':cid,'file':target.name,'x':left,'y':1024-bottom,'width':right-left,'height':bottom-top,'source':source.relative_to(repo).as_posix(),'sha256':hashlib.sha256(target.read_bytes()).hexdigest()})
for source,name in [(repo/'Overview/assets/military-pixel-20261015/barracks-open-left-v20.png','Barracks.png'),(repo/'Docs/Design/assets/board-pixel-20261015-v1/WoodenBoard.png','WoodenBoard.png')]:
    target = dest/name
    if target.exists() and target.read_bytes() != source.read_bytes():
        raise RuntimeError(f'Refusing to overwrite changed asset: {target}')
    shutil.copyfile(source,target)
(dest/'portrait-import.json').write_text(json.dumps({'portraits':items},ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'copiedPortraits':len(items),'otherTextures':2,'pixelEdits':False,'cropBounds':{x['characterId']:[x['x'],x['y'],x['width'],x['height']] for x in items}},ensure_ascii=False))
