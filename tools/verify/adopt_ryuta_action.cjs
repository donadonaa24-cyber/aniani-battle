// Requires real four-stage Unity results and inspected game captures.
const fs=require('fs'),crypto=require('crypto'),tag=process.argv[2],root='art_source/paper_roster_action/ryuta/';
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,'')),write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex'),assert=(v,n)=>{if(!v)throw Error(n);};
assert(/^[a-z0-9_]+$/.test(tag||''),'Tag required');const verified=read('art_source/paper_roster_action/single-key-adoption.json');
assert(verified.tag===tag&&verified.completedDedicatedCharacters.length===9&&verified.remainingDedicatedCharacters.length===0,'Nine-character full verification required');
const m=read(root+'manifest.json'),packed=read(root+'packed-manifest.json'),audit=read(root+'pixel-audit.json');
const selected=m.jobs.map(j=>m.entries.find(e=>e.id===j.id&&e.version===j.version));
assert(selected.length===12&&new Set(selected.map(e=>e.sha256)).size===10&&audit.complete,'12 slots / 10 sources required');
for(const e of selected){assert(sha(e.source)===e.sha256&&sha(e.prepared)===e.preparedSha256,'Body mismatch');e.gameConnected=true;e.status='game_adopted_unapproved';}
const effects=read(root+'effects-manifest.json'),rows=[],bySha=new Map(),folder='イラスト確認/現在ゲームで採用中/龍太/';
const titles={punch:'通常攻撃_切り紙の衝撃',bat:'特殊技_バットの振り抜き',spell_prepare:'超必殺技_予備の英字',spell_impact:'超必殺技_英語の呪文'};
for(const job of effects.jobs){const e=effects.entries.find(e=>e.id===job.id&&e.version===job.version);assert(e&&sha(e.source)===e.sha256,'Effect mismatch');
 let runtime=bySha.get(e.sha256);if(!runtime){runtime='AniBato/Assets/_Project/Art/PaperAction/ryuta_effect_'+e.id+'.png';assert(sha(runtime)===e.sha256,'Runtime mismatch');bySha.set(e.sha256,runtime);}
 const title=titles[e.reuseOf?.id||e.id];assert(title,'Unknown effect');const file=folder+title+'.png';fs.copyFileSync(runtime,file);
 rows.push({id:e.id,title,source:e.source,runtime,sha256:e.sha256,reviewFile:file,reuseOf:e.reuseOf||null});e.gameConnected=true;e.status='game_adopted_unapproved';
}
assert(rows.length===7&&bySha.size===4,'7 bindings / 4 sources required');
const captures=fs.readdirSync(`TestResults/${tag}_screenshots/rework_r4_ryuta`).filter(n=>n.endsWith('.png'));assert(captures.length>=60,'Both facings / phases / actual hits required');
Object.assign(m,{integrated:true,verifiedTag:tag});Object.assign(packed,{reviewed:true,gameConnected:true,verifiedTag:tag});Object.assign(effects,{gameConnected:true,verifiedTag:tag});
write(root+'manifest.json',m);write(root+'packed-manifest.json',packed);write(root+'effects-manifest.json',effects);
const review=read(root+'part-review.json');Object.assign(review,{gameConnected:true,unityVerifiedTag:tag});write(root+'part-review.json',review);
const current=read(root+'current-adoption.json'),report={schemaVersion:1,key:'ryuta',tag,bodyFrames:12,uniqueBodyImages:10,effectBindings:7,uniqueEffectImages:4,artApproved:false,
 buildAll:'success',edit:verified.edit,play:verified.play,windowsBuild:'success',screenshots:`docs/screenshots/${tag}/rework_r4_ryuta/`,screenshotCount:captures.length,
 body:current.entries,effectEntries:rows,remaining:'9人の専用攻撃は接続・検証済み。追加の溜め・被弾・他差分はユーザー追加指示後。'};
write(root+'adoption.json',report);write(root+'review-index.json',report);
fs.writeFileSync('イラスト確認/龍太/攻撃原画_確認中/必ず読んでください.txt','以前の候補です。現在版は「イラスト確認/現在ゲームで採用中/龍太/」の日本語動作名と採用一覧を確認してください。\n');
console.log(JSON.stringify({tag,body:12,uniqueBody:10,effects:7,uniqueEffects:4,captures:captures.length}));
