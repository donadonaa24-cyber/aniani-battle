// Make named, byte-identical review copies. Never edit the source illustrations.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const root=process.cwd();
const read=p=>JSON.parse(fs.readFileSync(path.join(root,p),'utf8').replace(/^\uFEFF/,''));
const catalog=read('art_source/paper_cutout/review-catalog.json');
const facts=read('art_source/paper_cutout/source-index.json');
const base=path.resolve(root,catalog.outputFolder);
const folders={adopted:'01_ゲームで採用済み',candidate:'02_確認用候補_未採用',repair:'03_修正が必要_未採用',history:'04_修正前と旧版'};
const keepHistory=catalog.keepPreviousImages!==false;
if(!keepHistory)Object.defineProperty(folders,'history',{enumerable:false});
const labels={adopted:'ゲームで採用済み',candidate:'確認用候補・未採用',repair:'修正が必要・未採用',history:'修正前・旧版'};
function inside(parent,target){
 const resolved=path.resolve(target),rel=path.relative(parent,resolved);
 if(rel.startsWith('..')||path.isAbsolute(rel)||!rel)throw Error('Path outside intended folder');
 return resolved;
}
inside(root,base);
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const factMap=new Map(facts.images.map(f=>[f.file,f]));
const integration=catalog.runtimeIntegration;
const manifest=integration?read('art_source/paper_cutout/runtime-manifest.json'):null;
const integratedSources=new Map(manifest?[...manifest.poses,...manifest.effects].map(p=>[p.source,p.sha256.toLowerCase()]):[]);
const selected=new Set(),rows=[];
const adoptedHash=hash(path.join(root,'AniBato/Assets/_Project/Art/Paper/idle_paper_chizuru.png'));
for(const entry of catalog.entries){
 const fact=factMap.get(entry.source);
 if(!fact)throw Error('Missing source facts: '+entry.source);
 if(entry.category==='adopted'){
  const verified=integration&&integration.verified&&integratedSources.get(entry.source)===fact.sha256.toLowerCase();
  const original=['idle','walk'].includes(entry.key)&&fact.sha256.toLowerCase()===adoptedHash;
  if(!verified&&!original)throw Error('Unverified adoption');
 }
 const version=Number(entry.source.match(/_v(\d+)\.png$/)[1]);
 const name=entry.title+(entry.category==='adopted'?'_採用済み':'_候補'+String(version).padStart(2,'0'))+'.png';
 rows.push({...entry,version,file:folders[entry.category]+'/'+name,sha256:fact.sha256,width:fact.width,height:fact.height});
 selected.add(entry.source);
}
for(const fact of keepHistory?facts.images:[]){
 if(selected.has(fact.file))continue;
 const match=path.basename(fact.file).match(/^chizuru_paper_(.+)_v(\d+)\.png$/);
 if(!match||!catalog.titles[match[1]])throw Error('Unnamed source: '+fact.file);
 const key=match[1],version=Number(match[2]),title=catalog.titles[key];
 let note='以前の生成版です。現在の確認対象は候補フォルダの同じ動作です。';
 if(key==='back_attack'&&version<=3)note='背中側の構図でカバンが左腕にある旧版。ユーザー指摘により不採用です。';
 if(key==='back_attack'&&version===6)note='自由な左手の親指が下側になっている旧版。ユーザー指摘により不採用です。';
 if(key==='back_attack'&&version===7)note='自由な左手の白い紙縁が欠けた旧版。候補08で縁を戻しています。';
 if(key==='air_forward'&&version<=3)note='右手カバンとして使う前に、肩と腕の左右を修正する必要がある旧版です。';
 if(key==='air_forward'&&version===4)note='画面左の自由な拳も右手に見えるとユーザーが指摘した旧版。左手の内側が見える形へ修正中の版です。';
 if(key==='air_forward'&&version===5)note='左拳の親指が欠けているとユーザーが指摘した旧版。候補06で下側へ親指を描き足しています。';
 if(key==='air_forward_down'&&version===1)note='胸元と両手の左右が一致していないとユーザーが指摘した旧版。背中を見せる構図へ修正しています。';
 if(key==='air_forward_down'&&version===2)note='背中構図ですが、左拳の指の腹が見える誤りを指摘された旧版。候補03で甲が見える左拳へ修正しています。';
 if(key==='attack3')note='左右の手または肩と腕の所属に問題があった旧版です。採用していません。';
 rows.push({key,title,source:fact.file,category:'history',version,note,file:folders.history+'/'+title+'_旧版'+String(version).padStart(2,'0')+'.png',sha256:fact.sha256,width:fact.width,height:fact.height});
}
const planned=new Set(rows.map(r=>r.file));
fs.mkdirSync(base,{recursive:true});
for(const folder of Object.values(folders))fs.mkdirSync(inside(base,path.join(base,folder)),{recursive:true});
const priorPath=inside(base,path.join(base,'.managed.json'));
const prior=fs.existsSync(priorPath)?JSON.parse(fs.readFileSync(priorPath,'utf8')):[];
const historyBySource=new Map(rows.filter(r=>r.category==='history').map(r=>[r.source,r]));
function preserve(from,name){
 if(!keepHistory)throw Error('Review copy changed or cleanup required; not overwriting: '+path.basename(from));
 const checkedFrom=inside(base,from);
 let n=0,to;
 do{to=inside(base,path.join(base,folders.history,name+(n?'_'+n:'')+'.png'));n++;}while(fs.existsSync(to));
 fs.renameSync(checkedFrom,to);
}
for(const old of prior){
 if(!planned.has(old.file)){
  const oldPath=inside(base,path.join(base,old.file));
  if(fs.existsSync(oldPath)){
   const archived=historyBySource.get(old.source);
   const archivePath=archived?inside(base,path.join(base,archived.file)):null;
   if(archivePath&&!fs.existsSync(archivePath)&&hash(oldPath)===archived.sha256.toLowerCase()){
    // Both resolved paths are inside this review folder; move only the managed copy.
    fs.renameSync(oldPath,archivePath);
   }else preserve(oldPath,'整理前_'+path.basename(old.file,'.png'));
  }
 }
}
for(const row of rows){
 const src=inside(root,path.join(root,row.source));
 if(hash(src)!==row.sha256.toLowerCase())throw Error('Source changed: '+row.source);
 const dest=inside(base,path.join(base,row.file));
 if(fs.existsSync(dest)&&hash(dest)!==row.sha256.toLowerCase())preserve(dest,'保存_'+path.basename(row.file,'.png'));
 if(!fs.existsSync(dest))fs.copyFileSync(src,dest);
 if(hash(dest)!==hash(src))throw Error('Copy mismatch: '+row.file);
}
const esc=s=>String(s).replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const url=p=>p.split('/').map(encodeURIComponent).join('/');
const counts=Object.fromEntries(Object.keys(folders).map(c=>[c,rows.filter(r=>r.category===c).length]));
function card(r){
 return '<article data-category="'+r.category+'" data-search="'+esc(r.title+' '+r.note)+'"><a href="'+url(r.file)+'" target="_blank"><img loading="lazy" src="'+url(r.file)+'" alt="'+esc(r.title)+'"></a><div class="body"><span class="badge '+r.category+'">'+labels[r.category]+'</span><h3>'+esc(r.title)+'</h3><p class="filename">'+esc(path.basename(r.file))+'</p><p>'+esc(r.note)+'</p><a href="'+url(r.file)+'" target="_blank">原寸で開く</a> · <a href="'+url('../../'+r.source.replace('.png','.prompt.json'))+'" target="_blank">生成条件</a></div></article>';
}
function section(category){
 return '<section id="'+category+'"><h2>'+labels[category]+' <small>'+counts[category]+'枚</small></h2><div class="grid">'+rows.filter(r=>r.category===category).map(card).join('\n')+'</div></section>';
}
const html='<!doctype html><html lang="ja"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>千鶴のイラスト確認</title><style>'+
'body{margin:0;background:#f4f1e8;color:#242424;font:16px/1.65 system-ui,sans-serif}header{background:#282d3b;color:#fff;padding:28px max(24px,calc((100vw - 1300px)/2))}h1{margin:0;font-size:30px}header p{margin:10px 0 0}.wrap{max-width:1300px;margin:auto;padding:24px}nav{display:flex;gap:12px;flex-wrap:wrap}nav a{background:white;color:#282d3b;padding:8px 12px;border-radius:8px}.notice{background:white;padding:16px;border-left:5px solid #b57221;border-radius:8px}.tools{display:flex;gap:12px;flex-wrap:wrap;margin:20px 0}input,select{font:inherit;padding:10px 12px;border:1px solid #aaa;border-radius:8px}input{flex:1;min-width:220px}.grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(265px,1fr));gap:18px}article{background:white;border:1px solid #d2d0c8;border-radius:12px;overflow:hidden}article>a{display:block;background:repeating-conic-gradient(#333844 0 25%,#424956 0 50%) 0 0/28px 28px}img{display:block;width:100%;height:290px;object-fit:contain}.body{padding:15px}.badge{padding:4px 9px;border-radius:20px;font-size:13px;font-weight:700}.adopted{background:#daefdf;color:#175a2c}.candidate{background:#e3eaff;color:#21478e}.repair{background:#ffe4d8;color:#8e3018}.history{background:#e8e8e8;color:#555}h3{margin:10px 0 6px;font-size:20px}.filename{font-size:13px;overflow-wrap:anywhere;background:#f3f3f3;padding:7px;border-radius:5px;user-select:all}p{margin:8px 0}h2{margin-top:32px}small{font-weight:400}details{margin-top:28px}summary{cursor:pointer;font-size:22px;font-weight:700}.body a{color:#285998}footer{padding:24px 0;color:#666;font-size:14px}[hidden]{display:none!important}</style>'+
'<header><h1>千鶴のイラスト確認</h1><p>画像名を指定して、気になる部分を教えてください。例：「空中前方攻撃_候補04のカバンの手」</p></header><main class="wrap"><nav>'+
Object.keys(folders).map(c=>'<a href="#'+c+'">'+labels[c]+' '+counts[c]+'枚</a>').join('')+'</nav><p class="notice"><strong>採用済みは待機と以前の歩きだけです。</strong> ほかの紙イラストは、生成済みでもゲームには未採用です。歩きは待機と同じ一枚を使います。新しい固定15度歩き・45度走りの動きは未検証です。</p><div class="tools"><input id="search" placeholder="画像名や修正点で探す（ジャンプ、手、カバンなど）" aria-label="画像名や修正点で探す"><select id="category" aria-label="採用状態"><option value="all">すべての現在画像</option><option value="adopted">ゲームで採用済み</option><option value="candidate">確認用候補・未採用</option><option value="repair">修正が必要・未採用</option><option value="history">修正前・旧版</option></select></div>'+
section('adopted')+section('candidate')+section('repair')+'<details id="history"><summary>修正前・旧版 '+counts.history+'枚（現在の採用候補ではありません）</summary>'+section('history').replace('id="history"','id="history-list"')+'</details><footer>2026-10-14整理。画像は原画と同じバイトの確認用コピーです。今回Unity追加実行0回。保存済み待機・歩行のUnity結果はEditMode441/441・PlayMode36/36で、ほかの候補の検証結果ではありません。</footer></main><script>'+
'const search=document.getElementById("search"),category=document.getElementById("category"),history=document.getElementById("history");function filter(){const q=search.value.trim().toLowerCase(),c=category.value;history.open=c==="history"||location.hash==="#history";document.querySelectorAll("article").forEach(a=>{a.hidden=(c!=="all"&&a.dataset.category!==c)||(q&&!a.dataset.search.toLowerCase().includes(q))});document.querySelectorAll("section").forEach(s=>{s.hidden=!Array.from(s.querySelectorAll("article")).some(a=>!a.hidden)});}search.addEventListener("input",filter);category.addEventListener("change",filter);window.addEventListener("hashchange",filter);filter();</script></html>';
let deliveryHtml=keepHistory?html:html.replace(/<option value="history">.*?<\/option>/,'').replace(/<details id="history">[\s\S]*?<\/details>/,'').replace('history.open=','if(history)history.open=');
if(integration&&integration.verified)deliveryHtml=deliveryHtml.replace(/<p class="notice">[\s\S]*?<\/p>/,'<p class="notice"><strong>現在の35画像をゲームへ接続済みです。</strong> 手や部位の絵としての修正点は各画像の説明に残しています。歩き15度・走り45度は固定傾き、待機だけ揺れます。</p>').replace(/<footer>[\s\S]*?<\/footer>/,'<footer>実Unity検証 '+esc(integration.tag)+'：BuildAll成功／EditMode '+integration.editPassed+'件／PlayMode '+integration.playPassed+'件／Windowsビルド成功。画像は原画と同じバイトの確認用コピーです。</footer>');
fs.writeFileSync(path.join(base,'画像一覧.html'),deliveryHtml,'utf8');
const current=rows.filter(r=>r.category!=='history');
const md='# 千鶴のイラスト確認\n\n採用済みは待機と同じ画像を使う以前の歩きだけです。ほかは未採用。画像一覧.htmlで画像と名前を並べて確認できます。気になる画像の日本語名と部分を伝えてください。\n\n| 画像名 | 状態 | 現在の確認点 |\n|---|---|---|\n'+current.map(r=>'| ['+path.basename(r.file)+']('+url(r.file)+') | '+labels[r.category]+' | '+r.note+' |').join('\n')+'\n\n04_修正前と旧版には以前の版を保管しています。原画は移動・加工していません。今回Unity追加実行0回、新しい実機スクショ0件。\n';
let deliveryMd=keepHistory?md:md.replace('04_修正前と旧版には以前の版を保管しています。原画は移動・加工していません。','修正前の旧画像はユーザー指定で削除しました。現在の画像だけを掲載しています。');
if(integration&&integration.verified)deliveryMd=deliveryMd.replace('採用済みは待機と同じ画像を使う以前の歩きだけです。ほかは未採用。','現在の35画像をゲームへ接続済みです。手や部位の修正点は個別の説明に残します。').replace('今回Unity追加実行0回、新しい実機スクショ0件。','実Unity検証：BuildAll成功／EditMode '+integration.editPassed+'件／PlayMode '+integration.playPassed+'件／Windowsビルド成功。Unity画像はdocs/screenshots/paper_chizuru_all_unity/。');
fs.writeFileSync(path.join(base,'確認のしかた.md'),deliveryMd,'utf8');
const csvCell=s=>'"'+String(s).replace(/"/g,'""')+'"';
fs.writeFileSync(path.join(base,'画像一覧.csv'),'\uFEFF'+['画像名,採用状態,確認点,原画,幅,高さ',...rows.map(r=>[path.basename(r.file),labels[r.category],r.note,r.source,r.width,r.height].map(csvCell).join(','))].join('\r\n')+'\r\n','utf8');
for(const [category,folder] of Object.entries(folders)){
 fs.writeFileSync(path.join(base,folder,'このフォルダについて.txt'),labels[category]+'\n'+(category==='adopted'?'現在ゲームで使う紙イラストです。絵の修正残は一覧の各画像の説明を参照してください。':category==='history'?'以前の生成版です。現在の候補は02または03にあります。':'まだゲームには採用していません。画像名を指定して修正点を伝えられます。')+'\n','utf8');
}
fs.writeFileSync(priorPath,JSON.stringify(rows.map(({file,source,sha256})=>({file,source,sha256})),null,2)+'\n');
process.stdout.write(JSON.stringify({folder:catalog.outputFolder,counts,byteIdenticalCopies:rows.length,sources:factMap.size})+'\n');
