import fs from 'node:fs/promises';
import {fileURLToPath} from 'node:url';
import {FileBlob,SpreadsheetFile} from '@oai/artifact-tool';

const folder=fileURLToPath(new URL('.',import.meta.url));
const workbook=await SpreadsheetFile.importXlsx(await FileBlob.load(folder+'GameData-before.xlsx'));
const apply=process.argv.includes('--apply');
if(apply){
  const rates=[400,300,200,120,80,50,40,30,20,15,10,5,0,0,0];
  workbook.worksheets.getItem('Stages').getRange('T5:T34').values=Array.from({length:30},(_,i)=>[rates[Math.floor(i/2)]]);
  workbook.recalculate();
  console.log((await workbook.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!',options:{useRegex:true,maxResults:20},maxChars:1000})).ndjson);
  await(await SpreadsheetFile.exportXlsx(workbook)).save(folder+'GameData-candidate.xlsx');
}
console.log((await workbook.inspect({kind:'table',range:'Stages!S5:T34',include:'values,formulas',tableMaxRows:30,tableMaxCols:2,maxChars:2300})).ndjson);
const preview=await workbook.render({sheetName:'Stages',range:'S4:T18',scale:1.5,format:'png'});
await fs.writeFile(folder+`workbook-${apply?'after':'before'}.png`,new Uint8Array(await preview.arrayBuffer()));
