import fs from 'node:fs/promises';
import {FileBlob,SpreadsheetFile} from '@oai/artifact-tool';
const directory=new URL('.',import.meta.url).pathname.replace(/^\/(\w:)/,'$1');
const workbook=await SpreadsheetFile.importXlsx(await FileBlob.load(directory+'GameData-before.xlsx'));
const stages=workbook.worksheets.getItem('Stages'),strings=workbook.worksheets.getItem('Strings');
const editing=process.argv.includes('--apply');
if(editing){
  // Each row defines a rating profile: depth, nodes, safe randomness, tactical oversight (per mille).
  const profiles=[
    [1,500,900,650],[1,600,850,550],[1,700,800,450],[1,800,750,350],[1,1000,700,300],
    [1,1200,650,250],[2,1800,600,200],[2,2200,550,160],[2,2600,500,120],[2,3000,400,100],
    [3,3500,350,80],[3,4000,300,60],[3,5000,250,40],[4,6000,200,20],[4,7500,150,0]
  ];
  stages.getRange('S1:T34').copyFrom(stages.getRange('F1:G34'),'all');
  stages.getRange('S1:T4').values=[['AiRating','AiOversightPermille'],['int','int'],['All','All'],['내부 실력 등급 200~1600, 100 단위 (실측 Elo 아님)','전술을 살피지 않고 합법 수를 고를 확률, 0~1000']];
  stages.getRange('S:T').format.columnWidth=30;
  for(let id=1;id<=30;id++){
    const tier=Math.floor((id-1)/2),profile=profiles[tier];
    stages.getRange(`E${id+4}:G${id+4}`).values=[profile.slice(0,3)];
    stages.getRange(`S${id+4}:T${id+4}`).values=[[200+tier*100,profile[3]]];
  }
  strings.getRange('A425:E426').copyFrom(strings.getRange('A423:E424'),'all');
  strings.getRange('A425:E426').values=[
    ['USE','turn_timer_seconds','{0}초','플레이어 턴 남은 초','{0}s'],
    ['USE','turn_timeout_placed','시간이 끝나 {0}열에 자동으로 놓았어요.','제한 시간 초과 자동 착수','Time is up. Placed in column {0}.']
  ];
  strings.getRange('C118').values=[["1. 열을 골라 놓일 자리를 확인해요.\n\n2. '놓기'로 돌을 놓아요. 턴마다 30초, 시간이 끝나면 자동으로 놓여요.\n\n3. 가로·세로·대각선으로 네 돌을 먼저 이으면 승리해요.\n\n나는 흑돌, AI는 백돌이에요."]];
  strings.getRange('E118').values=[['1. Choose a column to preview your stone.\n\n2. Press Place. You have 30 seconds per turn; then a stone is placed automatically.\n\n3. Connect four across, vertically or diagonally first to win.\n\nYou play black. The AI plays white.']];
  const rows=strings.getRange('B5:B424').values;
  const captionRow=rows.findIndex(row=>row[0]==='help_drop_caption')+5;
  if(captionRow<5)throw new Error('Missing visible help caption');
  strings.getRange(`C${captionRow}`).values=[["열을 고르고 '놓기'를 눌러요.\n턴마다 30초, 시간이 끝나면 자동으로 놓여요."]];
  strings.getRange(`E${captionRow}`).values=[['Choose a column, then press Place.\n30 seconds per turn; after that, a stone is placed automatically.']];
  workbook.recalculate();
  console.log((await workbook.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!',options:{useRegex:true,maxResults:20},maxChars:1000})).ndjson);
  await(await SpreadsheetFile.exportXlsx(workbook)).save(directory+'GameData-candidate.xlsx');
}
const range=editing?'S1:T14':'E1:G14';
console.log((await workbook.inspect({kind:'table',range:`Stages!${range}`,include:'values,formulas',tableMaxRows:14,tableMaxCols:3,maxChars:2500})).ndjson);
const preview=await workbook.render({sheetName:'Stages',range,scale:1,format:'png'});
await fs.writeFile(directory+`workbook-${editing?'after':'before'}.png`,new Uint8Array(await preview.arrayBuffer()));
