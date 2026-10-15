"""Freeze selected ImageGen output as versioned source assets; never alters pixels."""
from pathlib import Path
from PIL import Image
import hashlib, json, shutil

root=Path(__file__).resolve().parents[2]
folder=root/'Docs/Design/assets/characters-uniform-20261015-v4'
selection=json.loads((folder/'selection.json').read_text(encoding='utf-8'))
images=[]
for entry in selection['images']:
    source=Path(entry['source'])
    target=folder/(entry['id']+'-uniform-v4.png')
    if target.exists() and target.read_bytes()!=source.read_bytes():
        raise RuntimeError('Refusing to overwrite '+str(target))
    shutil.copyfile(source,target)
    with Image.open(target) as im:
        assert im.size==(1536,1024) and im.mode=='RGBA'
        lo,hi=im.getchannel('A').getextrema()
        assert lo==0 and hi>=250, f'No alpha range: {source}'
    images.append({**entry,'path':target.relative_to(root).as_posix(),
                   'sha256':hashlib.sha256(target.read_bytes()).hexdigest()})
record={'date':'2026-10-15','tool':'built-in image_gen.imagegen','status':'GENERATED_ASSETS_SELECTED',
        'states':60,'originalAssetsPreserved':True,'images':images,
        'promptSets':['Docs/Design/assets/characters-personality-20261015-v2/prompts.json',
                      'Docs/Design/assets/characters-uniform-20261015-v3/prompts-part1.json',
                      'Docs/Design/assets/characters-uniform-20261015-v3/prompts-part2.json',
                      'Docs/Design/assets/characters-uniform-20261015-v3/shoulder-pilot-prompt.json']+
                     [p.relative_to(root).as_posix() for p in sorted(folder.glob('*.json')) if p.name not in ('manifest.json','selection.json')],
        'research':[
          'https://www.law.go.kr/LSW/flDownload.do?bylClsCd=110204&flSeq=162126721&gubun=',
          'https://www.korea.kr/news/interviewView.do?newsId=148797200',
          'https://www.newsis.com/view/NISX20240415_0002699494'],
        'referenceUse':'User-provided clothing photographs guide placement only; no photographed people or background pixels used as runtime assets. Green command bands follow the user reference as a stylized game costume, not a claim of current standard uniform.',
        'scope':'Generation and provenance record, not exclusive-rights certification.'}
(folder/'manifest.json').write_text(json.dumps(record,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
(root/'Licence/CHARACTER-UNIFORMS-20261015-V4.json').write_text(json.dumps(record,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'images':len(images),'states':60,'realAlpha':True,'pixelEdits':False}))
