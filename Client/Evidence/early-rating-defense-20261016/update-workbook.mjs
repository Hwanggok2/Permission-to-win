import fs from 'node:fs/promises';
import {FileBlob, SpreadsheetFile} from '@oai/artifact-tool';
import {fileURLToPath} from 'node:url';
const folder=fileURLToPath(new URL('.',import.meta.url));
const workbook=await SpreadsheetFile.importXlsx(await FileBlob.load(folder+'GameData-before.xlsx'));
const apply=process.argv.includes('--apply');
if(apply){
  const sheet=workbook.worksheets.getItem('Stages');
  sheet.getRange('E9:E12').values=[[2],[2],[2],[2]];
  sheet.getRange('T9:T14').values=[[0],[0],[0],[0],[0],[0]];
  workbook.recalculate();
  console.log((await workbook.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!',options:{useRegex:true,maxResults:20},maxChars:1000})).ndjson);
}
for(const [name,range] of [['search','E4:G16'],['oversight','S4:T16']]){
  console.log((await workbook.inspect({kind:'table',range:'Stages!'+range,include:'values,formulas',tableMaxRows:13,tableMaxCols:3,maxChars:1500})).ndjson);
  const preview=await workbook.render({sheetName:'Stages',range,scale:1.5,format:'png'});
  await fs.writeFile(folder+`${name}-${apply?'after':'before'}.png`,new Uint8Array(await preview.arrayBuffer()));
}
if(apply)await(await SpreadsheetFile.exportXlsx(workbook)).save('C:/Github/StageGame/GameData/GameData_MVP.xlsx');
