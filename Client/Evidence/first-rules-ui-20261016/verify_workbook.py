"""Read-only preservation check of the artifact-tool edit."""
from pathlib import Path
import json, openpyxl
folder=Path(__file__).resolve().parent
before=openpyxl.load_workbook(folder/'GameData-before.xlsx')
after=openpyxl.load_workbook(folder/'GameData-candidate.xlsx')
assert before.sheetnames==after.sheetnames
changes=[]
for a,b in zip(before,after):
    assert (a.max_row,a.max_column)==(b.max_row,b.max_column)
    assert str(a.merged_cells)==str(b.merged_cells)
    assert str(a.data_validations)==str(b.data_validations)
    assert a.freeze_panes==b.freeze_panes
    assert a.auto_filter==b.auto_filter
    for key,dim in a.row_dimensions.items():assert dim.height==b.row_dimensions[key].height
    for key,dim in a.column_dimensions.items():assert dim.width==b.column_dimensions[key].width
    for row in a:
        for old in row:
            new=b[old.coordinate]
            assert old._style==new._style,(a.title,old.coordinate,'style')
            assert old.comment==new.comment
            if old.value!=new.value:
                assert a.title=='Strings' and old.coordinate in ('C138','E138')
                changes.append(dict(sheet=a.title,cell=old.coordinate,before=old.value,after=new.value))
assert len(changes)==2
(folder/'workbook-changes.json').write_text(json.dumps(dict(changes=changes,otherValuesAndStylesPreserved=True,sheetFeaturesPreserved=True),ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Only Strings C138/E138 changed; other values, styles and sheet features preserved.')
