// Run only after real Unity four-stage verification and inspection of its captures.
const fs=require('fs'),crypto=require('crypto'),tag=process.argv[2],root='art_source/paper_roster_action/akatsuki/';
const read=p=>JSON.parse(fs.readFileSync(p,'utf8')),write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex'),assert=(v,n)=>{if(!v)throw Error(n);};
assert(/^[a-z0-9_]+$/.test(tag||''),'Tag required');const verified=read('art_source/paper_roster_action/single-key-adoption.json');
assert(verified.tag===tag&&verified.completedDedicatedCharacters.includes('akatsuki'),'Full verification and single-key adoption required');
const m=read(root+'manifest.json'),packed=read(root+'packed-manifest.json'),audit=read(root+'pixel-audit.json');
const selected=m.jobs.map(j=>m.entries.find(e=>e.id===j.id&&e.version===j.version));
assert(selected.length===12&&new Set(selected.map(e=>e.sha256)).size===9&&audit.complete,'12 slots / 9 sources required');
for(const e of selected){assert(sha(e.source)===e.sha256&&sha(e.prepared)===e.preparedSha256,'Body source mismatch');e.gameConnected=true;e.status='game_adopted_unapproved';}
const effects=read(root+'effects-manifest.json'),rows=[],bySha=new Map(),folder='イラスト確認/現在ゲームで採用中/暁/';
const titles={card:'特殊・通常3段_飛翔カード_正式背面',pyramid:'超必殺技_落下ピラミッド',slash:'通常1・2段_カードの斬撃',sand:'超必殺技_砂粒'};
for(const job of effects.jobs){const e=effects.entries.find(e=>e.id===job.id&&e.version===job.version);assert(e&&sha(e.source)===e.sha256,'Effect mismatch');
 let runtime=bySha.get(e.sha256);if(!runtime){runtime='AniBato/Assets/_Project/Art/PaperAction/akatsuki_effect_'+e.id+'.png';assert(sha(runtime)===e.sha256,'Runtime source mismatch');bySha.set(e.sha256,runtime);}
 const title=titles[e.reuseOf?.id||e.id];assert(title,'Unknown effect');const file=folder+title+'.png';fs.copyFileSync(runtime,file);
 rows.push({id:e.id,version:e.version,title,source:e.source,runtime,sha256:e.sha256,reviewFile:file,reuseOf:e.reuseOf||null});e.gameConnected=true;e.status='game_adopted_unapproved';
}
assert(rows.length===6&&bySha.size===4,'6 bindings / 4 effects required');
const captures=fs.readdirSync(`TestResults/${tag}_screenshots/rework_r4_akatsuki`).filter(n=>n.endsWith('.png'));assert(captures.length>=60,'Both facings, all phases, real projectiles and hits required');
Object.assign(m,{integrated:true,verifiedTag:tag});Object.assign(packed,{reviewed:true,gameConnected:true,verifiedTag:tag});Object.assign(effects,{gameConnected:true,verifiedTag:tag});
write(root+'manifest.json',m);write(root+'packed-manifest.json',packed);write(root+'effects-manifest.json',effects);
const review=read(root+'part-review.json');Object.assign(review,{gameConnected:true,unityVerifiedTag:tag});write(root+'part-review.json',review);
const current=read(root+'current-adoption.json'),report={schemaVersion:1,key:'akatsuki',tag,bodyFrames:12,uniqueBodyImages:9,effectBindings:6,uniqueEffectImages:4,artApproved:false,
 buildAll:'success',edit:verified.edit,play:verified.play,windowsBuild:'success',screenshots:`docs/screenshots/${tag}/rework_r4_akatsuki/`,screenshotCount:captures.length,
 body:current.entries,effectEntries:rows,normal3:'実投射・元の自分側端と最遠端、発生7／有効3／硬直12、前進.18、通常被弾1回を維持',remaining:'専用攻撃は龍太が残り。追加差分は追加指示後。'};
write(root+'adoption.json',report);write(root+'review-index.json',report);
fs.writeFileSync('イラスト確認/暁/攻撃原画_確認中/必ず読んでください.txt','ここは以前の候補です。現在は「イラスト確認/現在ゲームで採用中/暁/」の日本語動作名と採用一覧を確認してください。\n');
console.log(JSON.stringify({tag,body:12,uniqueBody:9,effects:6,uniqueEffects:4,captures:captures.length}));
