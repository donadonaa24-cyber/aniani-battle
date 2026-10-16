// Review copies of raw candidates only; never import, resize or adopt an unreviewed pose.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const key=process.argv[2];if(!/^[a-z]+$/.test(key||''))throw Error('Character key required');
const root='art_source/paper_roster_action/'+key+'/',m=JSON.parse(fs.readFileSync(root+'manifest.json'));
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const folder='イラスト確認/'+m.name+'/攻撃原画_確認中/未梱包_残り候補';fs.mkdirSync(folder,{recursive:true});
const records=[];for(const j of m.jobs){const e=m.entries.find(e=>e.id===j.id&&e.version===j.version);if(!e||e.gameConnected||e.status.startsWith('retained'))continue;
 if(sha(e.source)!==e.sha256)throw Error('Source SHA mismatch: '+e.source);
 const destination=folder+'/'+e.label+'_'+(e.phase==='prepare'?'予備':'当てる')+'.png';
 if(!fs.existsSync(destination)||sha(destination)!==e.sha256){const pending=destination+'.pending';fs.copyFileSync(e.source,pending);fs.renameSync(pending,destination);}
 records.push({id:e.id,title:path.basename(destination),source:e.source,sha256:e.sha256,native:e.native,gameConnected:false,artApproved:false});
}
fs.copyFileSync(m.baseIdle,folder+'/00_待機原画_部位実寸の基準.png');
fs.writeFileSync(folder+'/必ず読んでください.txt','このフォルダはゲームへ未接続の原画候補です。透明余白の梱包もまだで、画面サイズは元の生成出力のままです。待機と頭・髪・靴の実寸、手足、装備、端切れを確認してください。最終採用ではありません。ゲームで使う通常4枚はイラスト確認/全9人_通常4原画/の本人フォルダです。\n');
fs.writeFileSync(root+'pending-review-index.json',JSON.stringify({key,character:m.name,folder,gameConnected:false,artApproved:false,entries:records},null,2)+'\n');
console.log(JSON.stringify({key,pending:records.length,folder}));
