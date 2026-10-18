// Compact, public-safe evidence retained when old TestResults captures are pruned.
const fs=require('fs'),crypto=require('crypto');
const base='art_source/paper_roster_motion';
const progress=JSON.parse(fs.readFileSync(base+'/progress.json','utf8'));
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const check=(value,message)=>{if(!value)throw Error(message);};
const reportFile='docs/codex_logs/motion_roster_native_verification.json';
const retained=fs.existsSync(reportFile)?JSON.parse(fs.readFileSync(reportFile,'utf8')):null;
const rows=[];
for(const character of progress.characters){
 if(!character.integrated)continue;
 const manifest=JSON.parse(fs.readFileSync(`${base}/${character.key}/manifest.json`,'utf8'));
 const tag=manifest.verifiedTag;
 const selected=manifest.jobs.map(job=>manifest.entries.find(e=>e.id===job.id&&e.version===job.version));
 check(selected.length===7&&selected.every(Boolean),character.key+': seven selected images required');
 for(const entry of selected){
  check(entry.gameConnected&&entry.rgbaWithinCanvasPreserved,character.key+': image not adopted');
  check(sha(`AniBato/Assets/_Project/Art/PaperAction/${character.key}_motion_${entry.id}.png`)===entry.preparedSha256,character.key+': runtime image differs');
 }
 const evidenceFiles=[`TestResults/${tag}_EditMode.xml`,`TestResults/${tag}_PlayMode.xml`,`TestResults/${tag}_buildall.log`,`TestResults/${tag}_build.log`];
 if(evidenceFiles.some(file=>!fs.existsSync(file))){
  const previous=retained?.characters.find(row=>row.key===character.key&&row.tag===tag);
  check(previous&&selected.every(entry=>previous.selected.some(old=>old.id===entry.id&&old.version===entry.version&&old.sha256===entry.preparedSha256)),tag+': retained evidence does not match current selection');
  rows.push({...previous,evidenceSource:'retained_summary'});
  continue;
 }
 const testResults={};
 for(const platform of ['EditMode','PlayMode']){
  const source=`TestResults/${tag}_${platform}.xml`;
  const header=fs.readFileSync(source,'utf8').match(/<test-run\b[^>]*>/)?.[0];
  check(header,`${tag}: missing ${platform} result`);
  const attribute=name=>header.match(new RegExp(name+'="([^"]+)"'))?.[1];
  check(attribute('result')==='Passed'&&attribute('failed')==='0'&&attribute('skipped')==='0',`${tag}: failed or skipped tests`);
  testResults[platform]={total:Number(attribute('total')),passed:Number(attribute('passed')),failed:0,skipped:0,source,sha256:sha(source)};
 }
 const buildAllLog=`TestResults/${tag}_buildall.log`,windowsLog=`TestResults/${tag}_build.log`;
 check(fs.readFileSync(buildAllLog,'utf8').includes('Exiting batchmode successfully now!'),tag+': BuildAll did not exit successfully');
 check(/Build completed with a result of 'Succeeded'|Build Finished, Result: Success/.test(fs.readFileSync(windowsLog,'utf8').replace(/\0/g,'')),tag+': Windows build not successful');
 rows.push({key:character.key,name:character.name,tag,evidenceSource:'result_files',BuildAll:{success:true,log:buildAllLog,sha256:sha(buildAllLog)},...testResults,Windows:{success:true,log:windowsLog,sha256:sha(windowsLog)},images:7,selected:selected.map(e=>({id:e.id,version:e.version,source:e.source,prepared:e.prepared,sha256:e.preparedSha256}))});
}
const report={schemaVersion:1,task:progress.task,expectedCharacters:8,expectedImages:56,verifiedCharacters:rows.length,verifiedImages:rows.length*7,complete:rows.length===8,retentionNote:'元XML・ログの実件数とSHAを保存。容量管理で古いTestResultsを削除した後はこの集計と人物別18PNGを参照。',characters:rows};
fs.writeFileSync(reportFile,JSON.stringify(report,null,2)+'\n');
console.log(JSON.stringify({verifiedCharacters:rows.length,verifiedImages:rows.length*7,complete:report.complete}));
