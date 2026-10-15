"""Read-only check: append one localized rating key without changing existing cells/features."""
import json
from pathlib import Path
import openpyxl
root=Path(__file__).resolve().parents[2]
folder=root/'Client/Evidence/rating-display-speech-20261015'
old=openpyxl.load_workbook(folder/'GameData-before.xlsx')
new=openpyxl.load_workbook(folder/'GameData-candidate.xlsx')
assert old.sheetnames==new.sheetnames
for a,b in zip(old,new):
    assert str(a.merged_cells)==str(b.merged_cells)
    assert str(a.data_validations)==str(b.data_validations)
    assert a.freeze_panes==b.freeze_panes
    assert a.max_column==b.max_column
    assert b.max_row==a.max_row+(1 if a.title=='Strings' else 0)
    for key,dim in a.row_dimensions.items():assert dim.height==b.row_dimensions[key].height
    for key,dim in a.column_dimensions.items():assert dim.width==b.column_dimensions[key].width
    for row in a:
        for cell in row:
            assert cell.value==b[cell.coordinate].value,(a.title,cell.coordinate)
            assert cell._style==b[cell.coordinate]._style,(a.title,cell.coordinate,'style')
assert new['Strings']['B427'].value=='ai_rating'
assert new['Strings']['C427'].value=='AI 레이팅 {0}'
assert new['Strings']['E427'].value=='AI Rating {0}'
(folder/'workbook-changes.json').write_text(json.dumps({'addedRow':427,'addedKey':'ai_rating','existingCellValuesAndStylesPreserved':True,'sheetFeaturesPreserved':True},indent=2)+'\n',encoding='utf-8')
print('Workbook preservation passed; only Strings row 427 added.')
