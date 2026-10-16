import fs from 'node:fs/promises';
import {FileBlob, SpreadsheetFile} from '@oai/artifact-tool';
const folder='C:/Github/StageGame/Client/Evidence/title-20261016';
const wb=await SpreadsheetFile.importXlsx(await FileBlob.load(folder+'/GameData-before.xlsx'));
const sheet=wb.worksheets.getItem('Strings');
const titles={game_title:['이겨도 됩니까?','Permission to Win'],client_game_title:['이겨도 됩니까?','Permission to Win'],game_brand:['이겨도 됩니까?','Permission to Win'],web_canvas_label:['이겨도 됩니까? 사목 게임','Permission to Win — four-in-a-row game']};
const changes=[];
for(const [i,row] of sheet.getRange('A1:E427').values.entries()){
  const key=row[1];if(!titles[key])continue;
  changes.push({key,row:i+1,before:[row[2],row[4]],after:titles[key]});
}
if(changes.length!==4)throw new Error('Expected four title keys.');
const apply=process.argv.includes('--apply');
if(apply){
  for(const c of changes){sheet.getRange(`C${c.row}`).values=[[c.after[0]]];sheet.getRange(`E${c.row}`).values=[[c.after[1]]];}
  sheet.getRange('D35').values=[['2026-10-16 사용자 확정 제목']];
  wb.recalculate();
  console.log((await wb.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!',options:{useRegex:true,maxResults:10},maxChars:500})).ndjson);
  await (await SpreadsheetFile.exportXlsx(wb)).save(folder+'/GameData-candidate.xlsx');
  await fs.writeFile(folder+'/string-changes.json',JSON.stringify(changes,null,2));
}
for(const c of changes.filter(c=>c.key==='game_title'||c.key==='web_canvas_label')){
  const range=`B${c.row}:E${c.row}`;
  console.log((await wb.inspect({kind:'table',range:'Strings!'+range,include:'values,formulas',tableMaxRows:1,tableMaxCols:4,maxChars:600})).ndjson);
  const preview=await wb.render({sheetName:'Strings',range,scale:1,format:'png'});
  await fs.writeFile(`${folder}/${c.key}-${apply?'after':'before'}.png`,new Uint8Array(await preview.arrayBuffer()));
}
