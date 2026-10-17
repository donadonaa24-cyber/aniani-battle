// Reuse an appropriate completed native key without claiming another image generation.
const fs=require('fs');const [key,targetId,fromId,note]=process.argv.slice(2);
if(!/^[a-z]+$/.test(key)||!note)throw Error('Character, target, source and reason required');
const root='art_source/paper_roster_action/'+key+'/',file=root+'manifest.json',m=JSON.parse(fs.readFileSync(file,'utf8'));
const job=m.jobs.find(j=>j.id===targetId),fromJob=m.jobs.find(j=>j.id===fromId),from=m.entries.find(e=>e.id===fromId&&e.version===fromJob.version);
if(!from||!fs.existsSync(from.source)||m.entries.some(e=>e.id===targetId&&e.version===job.version))throw Error('Missing source or target already saved');
const planned=JSON.parse(fs.readFileSync(job.promptFile,'utf8'));
const reuseOf={id:from.id,version:from.version,source:from.source,sha256:from.sha256,promptFile:fromJob.promptFile};
m.entries.push({...from,id:targetId,pose:planned.pose,phase:planned.phase,label:planned.label,version:job.version,
 reuseOf,newlyGenerated:false,status:'candidate',gameConnected:false,artNote:note});
Object.assign(planned,{reuseOf,status:'reused_existing_native_art',source:from.source,reuseReason:note});
fs.writeFileSync(job.promptFile,JSON.stringify(planned,null,2)+'\n');fs.writeFileSync(file,JSON.stringify(m,null,2)+'\n');
console.log(JSON.stringify({key,targetId,reuseOf,reason:note}));
