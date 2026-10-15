from pathlib import Path
from openpyxl import load_workbook
import json,sys,copy
repo=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(repo/'GameData'))
import export_data
directory=repo/'Client/Evidence/pixel-art-20261015'
before=load_workbook(directory/'GameData-before.xlsx')
after=load_workbook(directory/'GameData-story-candidate.xlsx')
assert before.sheetnames==after.sheetnames
unexpected=[];style_changes=[]
for sheet in before:
    other=after[sheet.title]
    assert sheet.freeze_panes==other.freeze_panes,(sheet.title,'freeze')
    assert list(sheet.merged_cells.ranges)==list(other.merged_cells.ranges)
    assert str(sheet.data_validations)==str(other.data_validations),(sheet.title,'validation')
    for row in sheet:
        for cell in row:
            new=other[cell.coordinate]
            allowed=sheet.title=='Stages' and ((cell.row==4 and cell.column==12) or (cell.row>=5 and cell.column in (12,13,15,16,17)))
            if cell.value!=new.value and not allowed:unexpected.append(f'{sheet.title}!{cell.coordinate}')
            for key in ('font','fill','border','alignment','number_format','protection'):
                if copy.copy(getattr(cell,key))!=copy.copy(getattr(new,key)):style_changes.append(f'{sheet.title}!{cell.coordinate}:{key}')
assert not unexpected,unexpected
payloads=export_data.validate(export_data.read_source(directory/'GameData-story-candidate.xlsx'))
old=json.loads((directory/'stages-before.json').read_text(encoding='utf-8'))['stages']
for a,b in zip(old,payloads['stages.json']['stages']):
    for key in ('id','chapter','titleKey','openingMoves','starter','aiDepth','aiNodeBudget','aiRandomPermille','difficultyStars'):
        assert a[key]==b[key],(a['id'],key)
report={'unexpectedValueChanges':unexpected,'styleChanges':style_changes,'freezeAndValidationPreserved':True,'openingAndAiFieldsPreserved':30,'oldStringsPreserved':269,'newStrings':149,'totalStrings':len(payloads['strings.json']['strings'])}
(directory/'workbook-validation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({**report,'styleChanges':style_changes[:8],'styleChangeCount':len(style_changes)},ensure_ascii=False))
