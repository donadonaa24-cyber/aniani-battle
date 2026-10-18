const fs=require('fs'),crypto=require('crypto');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const adoption=read('art_source/paper_roster_action/single-key-adoption.json');
if(adoption.completedDedicatedCharacters.length!==9||adoption.remainingDedicatedCharacters.length)throw Error('Roster incomplete');
const rows=[];
for(const key of adoption.completedDedicatedCharacters){
 const m=read('art_source/paper_roster_action/'+key+'/current-adoption.json');
 const normals=m.entries.filter(e=>/^normal[13]_/.test(e.id));
 const unique=new Set(normals.map(e=>e.sha256)).size;
 if(!m.completeAttackArtwork||m.bodyFrames!==12||normals.length!==4||unique!==4)throw Error('Four-key normal scope failed: '+key);
 for(const action of ['air','down'])if(m.entries.filter(e=>e.id===action+'_single').length!==1)throw Error('Single-key scope failed: '+key);
 for(const e of m.entries)if(sha(e.runtime)!==e.sha256||sha(e.reviewFile)!==e.sha256)throw Error('Runtime/adoption differ: '+key+'/'+e.id);
 rows.push({key,bodyFrames:12,normalUniqueImages:4,air:1,down:1,sourceAndReviewMatch:true,verifiedTag:m.unityVerifiedTag});
}
const selected=read('art_source/paper_roster_action/king/effects-manifest.json');
const supports=['leila','shion','noa'].map(id=>{const j=selected.jobs.find(j=>j.id===id),e=selected.entries.find(e=>e.id===id&&e.version===j.version);if(!e.gameConnected||sha(e.source)!==e.sha256)throw Error('Support missing');return {id,name:e.name,source:e.source,version:e.version,sha256:e.sha256};});
const report={schemaVersion:1,tag:adoption.tag,completeLatestRequestedScope:true,roster:rows,existingKingSupports:supports,
 sourcePolicy:'通常1・2段共有、3段を含め4原画。下と空中N各1枚。透真・蓮の未生成姿勢の紙待機置換は今回の実Unityで既存検査を再実行。',
 additionalVariants:'2026-10-17最新指定に従いユーザー追加指示後。溜め／被弾を自動生成しない。',artApproved:false};
fs.writeFileSync('art_source/paper_roster_action/completion-audit.json',JSON.stringify(report,null,2)+'\n');
console.log(JSON.stringify({tag:adoption.tag,characters:rows.length,normalUnique:rows.reduce((n,e)=>n+e.normalUniqueImages,0),supports:supports.map(s=>s.name)}));
