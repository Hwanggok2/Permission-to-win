"""Arrange captured game portraits for visual inspection without smoothing."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parents[2]
EVIDENCE=ROOT/'Client/Evidence/pixel-complete-20261015'
font=ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',18)
intro=sorted((EVIDENCE/'screens').glob('*-intro-390.png'))
assert len(intro)==21
for page in range(3):
    canvas=Image.new('RGB',(900,1750),'#272824');draw=ImageDraw.Draw(canvas)
    for row,path in enumerate(intro[page*7:page*7+7]):
        for col,state in enumerate(('intro','thinking','defeat')):
            p=path.with_name(path.name.replace('-intro-','-'+state+'-'))
            im=Image.open(p).crop((16,190,142,384))
            canvas.paste(im,(col*300+87,row*250+40))
            draw.text((col*300+12,row*250+6),p.stem.replace('-390',''),font=font,fill='white')
    canvas.save(EVIDENCE/f'game-portraits-{page+1}.png')

# Compare all six full-resolution Oh Junho captures at their native portrait crop.
canvas=Image.new('RGB',(900,840),'#272824');draw=ImageDraw.Draw(canvas)
for row,stage in enumerate((3,26)):
    for col,state in enumerate(('intro','thinking','defeat')):
        p=EVIDENCE/'screens'/f'stage-{stage:02d}-C03-{state}-1080.png'
        im=Image.open(p).crop((125,430,405,790))
        canvas.paste(im,(col*300+10,row*420+42))
        draw.text((col*300+12,row*420+8),f'Stage {stage} - {state}',font=font,fill='white')
canvas.save(EVIDENCE/'oh-junho-in-game.png')
print('Saved 63 small game portrait crops and six native-resolution Oh Junho crops.')
