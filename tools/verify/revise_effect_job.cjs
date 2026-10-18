const fs=require('fs');const [key,id,reason,prompt]=process.argv.slice(2);
if(!/^[a-z]+$/.test(key||'')||!id||!reason||!prompt)throw Error('key, id, reason and full prompt required');
const root='art_source/paper_roster_action/'+key+'/',read=p=>JSON.parse(fs.readFileSync(p,'utf8')),write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const m=read(root+'effects-manifest.json'),job=m.jobs.find(j=>j.id===id),old=job&&m.entries.find(e=>e.id===id&&e.version===job.version);
if(!old)throw Error('Save current candidate first');old.status='rejected';old.rejectionReason=reason;
const j=read(job.promptFile);j.version++;j.source=root+'generated/effect_'+id+'_v'+j.version+'.png';j.revisionReason=reason;j.prompt=prompt;
j.references=[old.source,...j.references];j.referenceRoles=['編集対象の候補。指定箇所だけを変更',...j.referenceRoles];
job.version=j.version;job.promptFile=root+'prompts/effect_'+id+'_v'+j.version+'.json';write(job.promptFile,j);write(root+'effects-manifest.json',m);console.log(job.promptFile);
