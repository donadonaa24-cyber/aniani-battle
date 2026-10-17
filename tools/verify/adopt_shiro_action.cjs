// Finalize selected Shiro drawings only after the actual four Unity stages.
const fs=require('fs'),crypto=require('crypto');const tag=process.argv[2];
const read=p=>JSON.parse(fs.readFileSync(p,'utf8')),write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex'),assert=(b,n)=>{if(!b)throw Error(n);};
assert(/^[a-z0-9_]+$/.test(tag||''),'Verification tag required');
const verified=read('art_source/paper_roster_action/single-key-adoption.json');
assert(verified.tag===tag&&verified.completedDedicatedCharacters.includes('shiro'),'Run adopt_single_attack_keys after four actual stages first');
const root='art_source/paper_roster_action/shiro/',m=read(root+'manifest.json'),packed=read(root+'packed-manifest.json'),audit=read(root+'pixel-audit.json');
const selected=m.jobs.map(j=>m.entries.find(e=>e.id===j.id&&e.version===j.version));
assert(selected.length===12&&new Set(selected.map(e=>e.sha256)).size===10&&audit.complete&&audit.bodyFrames.length===12,'Twelve audited slots / ten originals required');
for(const e of selected){assert(sha(e.source)===e.sha256&&sha(e.prepared)===e.preparedSha256,'Body source mismatch');e.gameConnected=true;e.status='game_adopted_unapproved';}
const current=read(root+'current-adoption.json'),effects=read(root+'effects-manifest.json'),rows=[];
const titles={emp:'特殊技_EMPの輪',alma_prepare:'超必殺_二進数空間の枠',alma_impact:'超必殺_ALMA攻撃効果'};
for(const job of effects.jobs){
 const e=effects.entries.find(e=>e.id===job.id&&e.version===job.version),runtime='AniBato/Assets/_Project/Art/PaperAction/shiro_effect_'+job.id+'.png';
 assert(e&&sha(e.source)===e.sha256&&sha(runtime)===e.sha256,'Selected effect/runtime mismatch');
 const file='イラスト確認/現在ゲームで採用中/シロ/'+titles[e.id]+'.png';fs.copyFileSync(runtime,file);
 rows.push({id:e.id,version:e.version,title:titles[e.id],source:e.source,runtime,sha256:e.sha256,reviewFile:file});e.gameConnected=true;e.status='game_adopted_unapproved';
}
assert(rows.length===3,'Three generated effects required');
const captures=fs.readdirSync(`TestResults/${tag}_screenshots/rework_r4_shiro`).filter(n=>n.endsWith('.png'));
assert(captures.length>=54,'Both facings, all phases and close-up effects required');
Object.assign(m,{integrated:true,verifiedTag:tag});Object.assign(packed,{reviewed:true,gameConnected:true,verifiedTag:tag});
Object.assign(effects,{gameConnected:true,verifiedTag:tag});write(root+'manifest.json',m);write(root+'packed-manifest.json',packed);write(root+'effects-manifest.json',effects);
const review=read(root+'part-review.json');Object.assign(review,{gameConnected:true,unityVerifiedTag:tag});write(root+'part-review.json',review);
const adoption={schemaVersion:1,key:'shiro',tag,bodyFrames:12,uniqueBodyImages:10,effects:rows.length,artApproved:false,
 buildAll:'success',edit:verified.edit,play:verified.play,windowsBuild:'success',screenshots:`docs/screenshots/${tag}/rework_r4_shiro/`,screenshotCount:captures.length,
 body:current.entries,effectEntries:rows,remaining:'追加差分は追加指示後。残4人の専用攻撃は次工程。'};
write(root+'adoption.json',adoption);write(root+'review-index.json',adoption);
fs.writeFileSync('イラスト確認/シロ/攻撃原画_確認中/必ず読んでください.txt','このフォルダは旧候補の確認用です。現在の採用12枠と効果3種は「イラスト確認/現在ゲームで採用中/シロ/」で動作名から確認してください。修正前の候補と区別します。\n');
console.log(JSON.stringify({key:'shiro',tag,bodyFrames:12,uniqueBodyImages:10,effects:rows.length,captures:captures.length}));
