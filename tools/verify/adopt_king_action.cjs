// Adopt only after the four real Unity stages, pixel audit and reviewed captures.
const fs=require('fs'),crypto=require('crypto'),tag=process.argv[2],root='art_source/paper_roster_action/king/';
const read=p=>JSON.parse(fs.readFileSync(p,'utf8')),write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex'),assert=(ok,n)=>{if(!ok)throw Error(n);};
assert(/^[a-z0-9_]+$/.test(tag||''),'Tag required');const verified=read('art_source/paper_roster_action/single-key-adoption.json');
assert(verified.tag===tag&&verified.completedDedicatedCharacters.includes('king'),'Actual full verification and adopt_single_attack_keys required');
const m=read(root+'manifest.json'),packed=read(root+'packed-manifest.json'),audit=read(root+'pixel-audit.json');
const selected=m.jobs.map(j=>m.entries.find(e=>e.id===j.id&&e.version===j.version));
assert(selected.length===12&&new Set(selected.map(e=>e.sha256)).size===9&&audit.complete,'Twelve audited slots / nine body sources required');
for(const e of selected){assert(sha(e.source)===e.sha256&&sha(e.prepared)===e.preparedSha256,'Body source mismatch');e.gameConnected=true;e.status='game_adopted_unapproved';}
const effects=read(root+'effects-manifest.json'),rows=[],runtimeBySha=new Map(),folder='イラスト確認/現在ゲームで採用中/黒獅子王/';
const titles={claw:'通常1・2段_邪悪な爪の効果',roar:'通常3段_咆哮効果',leila:'召喚部下1_豹妬レイラ',shion:'召喚部下2_銀狼シオン',noa:'召喚部下3_鹿角ノア'};
for(const job of effects.jobs){const e=effects.entries.find(e=>e.id===job.id&&e.version===job.version);assert(e&&sha(e.source)===e.sha256,'Effect source mismatch');
 let runtime=runtimeBySha.get(e.sha256);if(!runtime){runtime='AniBato/Assets/_Project/Art/PaperAction/king_effect_'+e.id+'.png';assert(sha(runtime)===e.sha256,'Native effect runtime mismatch');runtimeBySha.set(e.sha256,runtime);}
 const title=titles[e.reuseOf?.id||e.id];assert(title,'Unknown effect');const file=folder+title+'.png';fs.copyFileSync(runtime,file);
 rows.push({id:e.id,name:e.name,version:e.version,title,source:e.source,runtime,sha256:e.sha256,reviewFile:file,reuseOf:e.reuseOf||null,variantIndex:e.kind==='projectile'?e.variantIndex:null});e.gameConnected=true;e.status='game_adopted_unapproved';
}
assert(rows.length===7&&runtimeBySha.size===5,'Seven bindings / five generated sources required');
const captures=fs.readdirSync(`TestResults/${tag}_screenshots/rework_r4_king`).filter(n=>n.endsWith('.png'));assert(captures.length>=62,'Actual actions, both facings, three supports and closeups required');
Object.assign(m,{integrated:true,verifiedTag:tag});Object.assign(packed,{reviewed:true,gameConnected:true,verifiedTag:tag});Object.assign(effects,{gameConnected:true,verifiedTag:tag});
write(root+'manifest.json',m);write(root+'packed-manifest.json',packed);write(root+'effects-manifest.json',effects);const review=read(root+'part-review.json');Object.assign(review,{gameConnected:true,unityVerifiedTag:tag});write(root+'part-review.json',review);
const current=read(root+'current-adoption.json'),adoption={schemaVersion:1,key:'king',tag,bodyFrames:12,uniqueBodyImages:9,effectBindings:7,uniqueEffectImages:5,artApproved:false,
 buildAll:'success',edit:verified.edit,play:verified.play,windowsBuild:'success',screenshots:`docs/screenshots/${tag}/rework_r4_king/`,screenshotCount:captures.length,body:current.entries,effectEntries:rows,
 supportSource:'docs/art_reference/king_supports/sources.json',supportOrder:['豹妬レイラ','銀狼シオン','鹿角ノア'],remaining:'追加差分は追加指示後。残2人は暁→龍太。'};
write(root+'adoption.json',adoption);write(root+'review-index.json',adoption);
fs.writeFileSync('イラスト確認/黒獅子王/攻撃原画_確認中/必ず読んでください.txt','ここは以前の候補です。現在の12枠と爪・咆哮・レイラ・シオン・ノアは「イラスト確認/現在ゲームで採用中/黒獅子王/」で動作名から確認してください。\n');
console.log(JSON.stringify({key:'king',tag,bodyFrames:12,uniqueBodyImages:9,effectBindings:7,uniqueEffectImages:5,captures:captures.length}));
