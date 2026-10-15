"""Read-only preservation check before installing the artifact-tool workbook."""
from pathlib import Path
import json
import openpyxl

root=Path(__file__).resolve().parents[2]
evidence=root/'Client/Evidence/turn-timer-rating-20261015'
before=openpyxl.load_workbook(evidence/'GameData-before.xlsx')
after=openpyxl.load_workbook(evidence/'GameData-candidate.xlsx')
assert before.sheetnames==after.sheetnames
changes=[]
for old,new in zip(before,after):
    assert str(old.merged_cells)==str(new.merged_cells)
    assert str(old.data_validations)==str(new.data_validations)
    assert old.freeze_panes==new.freeze_panes
    for key,value in old.row_dimensions.items():
        assert value.height==new.row_dimensions[key].height
    for key,value in old.column_dimensions.items():
        assert value.width==new.column_dimensions[key].width
    for row in old:
        for cell in row:
            other=new[cell.coordinate]
            assert cell._style==other._style,(old.title,cell.coordinate,'style')
            if cell.value!=other.value:
                allowed=(old.title=='Stages' and cell.column in (5,6,7) and cell.row>=5) or (old.title=='Strings' and cell.column in (3,5) and old.cell(cell.row,2).value in ('client_help_rules','help_drop_caption'))
                assert allowed,(old.title,cell.coordinate)
                changes.append({'sheet':old.title,'cell':cell.coordinate,'before':cell.value,'after':other.value})
    assert all(c.data_type not in ('f','e') for row in new for c in row)
assert (after['Stages'].max_row,after['Stages'].max_column)==(34,20)
assert (after['Strings'].max_row,after['Strings'].max_column)==(426,5)
assert after['AdConfig'].max_row==before['AdConfig'].max_row
assert [after['Stages'].cell(r,19).value for r in range(5,35)]==[200+100*(n//2) for n in range(30)]
(evidence/'workbook-changes.json').write_text(json.dumps({'changes':changes,'addedStageColumns':['AiRating','AiOversightPermille'],'addedStrings':2,'existingStylesPreserved':True},ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(f'Preserved original workbook features; {len(changes)} existing values changed, 2 rating columns and 2 timer strings added.')
