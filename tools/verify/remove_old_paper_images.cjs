// Explicit user policy: remove superseded project paper PNGs, keep current references.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const root=process.cwd(),read=p=>JSON.parse(fs.readFileSync(path.join(root,p),'utf8').replace(/^\uFEFF/,''));
const catalog=read('art_source/paper_cutout/review-catalog.json');
if(catalog.keepPreviousImages!==false)throw Error('Old-image removal is not enabled');
const facts=read('art_source/paper_cutout/source-index.json');
function inside(base,target){const full=path.resolve(target),rel=path.relative(base,full);if(!rel||rel.startsWith('..')||path.isAbsolute(rel))throw Error('Out of scope');return full;}
const sourceBase=inside(root,path.join(root,'art_source/paper_cutout/generated'));
const reviewBase=inside(root,path.join(root,catalog.outputFolder));
const selected=new Set(catalog.entries.map(r=>r.source));
const bagBackupPath=path.join(root,'art_source/paper_cutout/variants/bag-backup.json');
if(fs.existsSync(bagBackupPath)){
 const backup=read('art_source/paper_cutout/variants/bag-backup.json');
 if(backup.protected)for(const row of backup.entries)selected.add(row.source);
}
const folders={adopted:'01_ゲームで採用済み',candidate:'02_確認用候補_未採用',repair:'03_修正が必要_未採用'};
const plannedReview=new Set(catalog.entries.map(r=>{
 if(!folders[r.category])throw Error('Unknown current category');
 const match=r.source.match(/_v(\d+)\.png$/);if(!match)throw Error('Unexpected current source');
 return folders[r.category]+'/'+r.title+(r.category==='adopted'?'_採用済み':'_候補'+String(Number(match[1])).padStart(2,'0'))+'.png';
}));
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
for(const file of selected){if(!fs.existsSync(inside(sourceBase,path.join(root,file))))throw Error('Missing current source: '+file);}
const auditPath=inside(root,path.join(root,'art_source/paper_cutout/deleted-old-images.json'));
const audit=fs.existsSync(auditPath)?read('art_source/paper_cutout/deleted-old-images.json'):{reason:'user_requested_old_image_removal',entries:[]};
const before=audit.entries.length,deleted={source:0,review:0},skipped=[];
function removeChecked(file,expected,scope){
 if(!fs.existsSync(file))return;
 if(hash(file)!==expected.toLowerCase()){skipped.push(path.relative(root,file).replace(/\\/g,'/'));return;}
 fs.unlinkSync(file);
 audit.entries.push({file:path.relative(root,file).replace(/\\/g,'/'),sha256:expected.toLowerCase(),scope});
 deleted[scope]++;
 fs.writeFileSync(auditPath,JSON.stringify(audit,null,2)+'\n');
}
for(const fact of facts.images){
 if(selected.has(fact.file))continue;
 const file=inside(sourceBase,path.join(root,fact.file));
 if(!/^chizuru_paper_.+_v\d+\.png$/.test(path.basename(file)))throw Error('Unexpected source name');
 removeChecked(file,fact.sha256,'source');
}
const managed=path.join(reviewBase,'.managed.json');
for(const old of fs.existsSync(managed)?JSON.parse(fs.readFileSync(managed,'utf8')):[]){
 if(plannedReview.has(old.file))continue;
 removeChecked(inside(reviewBase,path.join(reviewBase,old.file)),old.sha256,'review');
}
const legacy=inside(reviewBase,path.join(reviewBase,'04_修正前と旧版'));
if(fs.existsSync(legacy)){
 const note=inside(legacy,path.join(legacy,'このフォルダについて.txt'));
 if(fs.existsSync(note)&&fs.readFileSync(note,'utf8').replace(/\r/g,'')==='修正前・旧版\n以前の生成版です。現在の候補は02または03にあります。\n')fs.unlinkSync(note);
 if(fs.readdirSync(legacy).length===0)fs.rmdirSync(legacy);
}
process.stdout.write(JSON.stringify({deleted,skipped,currentUniqueSources:selected.size,newAuditEntries:audit.entries.length-before})+'\n');
