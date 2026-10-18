// Our first two Akatsuki runs omitted -CaptureRoot. Preserve their generated
// outputs before putting the pre-run historical captures back. Not a source reset.
const fs=require('fs'),path=require('path'),cp=require('child_process'),crypto=require('crypto');
const root=process.cwd(),prefix=root+path.sep,destination='TestResults/rework_r4_akatsuki_2_misrouted_captures/';
if(fs.existsSync(destination+'preservation.json'))throw Error('This one-time preservation has already completed.');
if(cp.spawnSync('git',['rev-parse','--short','HEAD'],{encoding:'utf8'}).stdout.trim()!=='371ba4c')throw Error('This correction only applies to the known pre-Akatsuki checkout.');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
const result=cp.spawnSync('git',['diff','--name-only','-z','--','docs/screenshots','docs/HITBOX_REVIEW_CHIZURU.md'],{encoding:'utf8'});
if(result.status!==0)throw Error(result.stderr);
const rows=[];
for(const file of result.stdout.split('\0').filter(Boolean)){
 if(!(file.startsWith('docs/screenshots/')&&file.endsWith('.png'))&&file!=='docs/HITBOX_REVIEW_CHIZURU.md')throw Error('Unexpected path');
 const current=path.resolve(file),backup=path.resolve(destination+file);
 if(!current.startsWith(prefix)||!backup.startsWith(path.resolve(destination)+path.sep))throw Error('Unsafe target');
 const previous=cp.spawnSync('git',['show','HEAD:'+file],{maxBuffer:32*1024*1024});if(previous.status!==0)throw Error('Missing tracked history '+file);
 const generated=fs.readFileSync(current);if(generated.equals(previous.stdout))continue;
 if(fs.existsSync(backup))throw Error('Preserved capture already exists');
 fs.mkdirSync(path.dirname(backup),{recursive:true});fs.writeFileSync(backup,generated);fs.writeFileSync(current,previous.stdout);
 rows.push({file,generatedSha256:sha(generated),historicalSha256:sha(previous.stdout),backup:destination+file});
}
fs.mkdirSync(destination,{recursive:true});fs.writeFileSync(destination+'preservation.json',JSON.stringify({reason:'Only our test-generated changes; these paths were unmodified at turn start. Full source changes remain untouched.',rows},null,2)+'\n');
console.log(JSON.stringify({preservedGeneratedOutputs:rows.length,sourceFilesReset:0}));
