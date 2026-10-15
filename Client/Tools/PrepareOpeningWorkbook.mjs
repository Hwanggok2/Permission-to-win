import fs from 'node:fs/promises';
import {FileBlob, SpreadsheetFile} from '@oai/artifact-tool';
const root='C:/Github/StageGame';
const evidence=root+'/Client/Evidence/opening-ranks-20261015';
const workbook=await SpreadsheetFile.importXlsx(await FileBlob.load(evidence+'/GameData-before.xlsx'));
const sheet=workbook.worksheets.getItem('Stages');
const openings={7:'3,2',12:'2,4',17:'3,2,4,5',21:'2,4,3,1',25:'3,2,2,4',29:'2,4,4,2'};
const aiFirst=[5,8,11,14,18,22,26,30];
const editing=process.argv.includes('--apply');
if(editing){
  const strings=workbook.worksheets.getItem('Strings');
  strings.getRange('C291').values=[['분대장 남기석']];
  strings.getRange('E291').values=[['Squad Leader Nam Gi-seok']];
  strings.getRange('C292').values=[['병장 분대장 남기석과의 부대 대국. 느긋한 표정 뒤의 수를 읽는다.']];
  strings.getRange('E292').values=[['Play Sergeant Nam Gi-seok, the squad leader. Read the moves behind his relaxed smile.']];
  strings.getRange('C406').values=[['8강 · 분대장 남기석']];
  strings.getRange('E406').values=[['Quarterfinal · Squad Leader Nam Gi-seok']];
  for(let stage=1;stage<=30;stage++){
    const opening=openings[stage]??'', starter=aiFirst.includes(stage)?-1:1;
    const goal=opening?'초기 배치에서 연결 공간 넓히기':starter===-1?'AI 선공에 대응하는 전개':stage>=25?'대회 자유 대국':'빈 판에서 중앙과 연결 연습';
    sheet.getRange(`H${stage+4}:J${stage+4}`).values=[[starter,opening,goal]];
  }
  workbook.recalculate();
  console.log((await workbook.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!',options:{useRegex:true,maxResults:20},maxChars:1000})).ndjson);
  await (await SpreadsheetFile.exportXlsx(workbook)).save(evidence+'/GameData-candidate.xlsx');
}
console.log((await workbook.inspect({kind:'table',range:'Stages!E5:J34',include:'values,formulas',tableMaxRows:30,tableMaxCols:6,maxChars:4500})).ndjson);
const preview=await workbook.render({sheetName:'Stages',range:'E4:J18',scale:1,format:'png'});
await fs.writeFile(`${evidence}/workbook-${editing?'after':'before'}.png`,new Uint8Array(await preview.arrayBuffer()));
