import fs from 'node:fs/promises';
import {fileURLToPath} from 'node:url';
import {FileBlob,SpreadsheetFile} from '@oai/artifact-tool';
const directory=fileURLToPath(new URL('.',import.meta.url));
const workbook=await SpreadsheetFile.importXlsx(await FileBlob.load(directory+'GameData-before.xlsx'));
const edit=process.argv.includes('--apply');
const sheet=workbook.worksheets.getItem('Strings');
if(sheet.getRange('B138').values[0][0]!=='help_drop_title')throw new Error('Unexpected row');
if(edit){
  sheet.getRange('C138').values=[['돌은 아래에서부터 쌓여요']];
  sheet.getRange('E138').values=[['Stones stack from the bottom']];
  workbook.recalculate();
  console.log((await workbook.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!',options:{useRegex:true,maxResults:10},maxChars:1000})).ndjson);
  await(await SpreadsheetFile.exportXlsx(workbook)).save(directory+'GameData-candidate.xlsx');
}
console.log((await workbook.inspect({kind:'table',range:'Strings!B138:E138',include:'values,formulas',tableMaxRows:1,tableMaxCols:4,maxChars:1000})).ndjson);
const preview=await workbook.render({sheetName:'Strings',range:'B137:E139',scale:1,format:'png'});
await fs.writeFile(directory+`workbook-${edit?'after':'before'}.png`,new Uint8Array(await preview.arrayBuffer()));
