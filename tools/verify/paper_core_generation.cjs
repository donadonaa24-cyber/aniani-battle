// Persist reproducible per-character jobs and copy built-in outputs, never redraw or resize artwork.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),sharp=require('sharp');
const ROOT='art_source/paper_roster_core',GEN=process.env.PAPER_CORE_IMAGE_DIR||path.join(process.env.USERPROFILE,'.codex/generated_images/01a13044-db10-7cf1-a55e-fb93e3cf7e4d');
function fileRetry(action){for(let i=0;;i++){try{return action()}catch(e){if(i===8||!['UNKNOWN','EBUSY','EPERM','EACCES'].includes(e.code))throw e;Atomics.wait(new Int32Array(new SharedArrayBuffer(4)),0,0,100)}}}
const read=p=>JSON.parse(fileRetry(()=>fs.readFileSync(p,'utf8')).replace(/^\uFEFF/,''));
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const write=(p,v)=>{fs.mkdirSync(path.dirname(p),{recursive:true});fileRetry(()=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n'))};
const labels={guard:'ガード',crouch:'しゃがみ',counter:'カウンター',win:'勝利',lose:'敗北'};
const poseText={guard:'両肘を曲げ、体と顔の前に左右の拳またはミトンを別々に構え、膝を少し曲げて守る。顔と両目を隠さない。閉じた防御のシルエット。盾・バリアを追加しない。',crouch:'両膝と股関節を深く曲げて腰を低くする。両足は地面に着け、頭・髪・靴・手は待機と同じ画素寸法のまま。全高は待機の約60〜65%。余った上半分は透明な空間として残し、しゃがんだ人物を画面いっぱいに拡大しない。両手は膝と胸の間に自然に構える。',counter:'ガードとは違う受け流しの構え。片方の手を開いて相手方向へ出し、もう片方は胸元へ引く。攻撃を払う直前の鋭い静かな構え。開いた手は親指1本と4本の指、肩・肘・掌のつながりが自然。',win:'待機から大きく姿勢を変えない控えめな勝利の立ち姿。両足を接地したまま、ささやかな笑顔と胸元の小さなガッツポーズ。飛び跳ねない、座らない、大きく腕を頭上に上げない。本人らしい自信を表情で示す。',lose:'立ったままの控えめな敗北の姿。両足を接地したまま肩を落とし、少しうつむき、腕を自然に下げる。悔しそう／無念そうな表情。頭を少し下げるが頭や靴は同じ大きさ。座り込まない、膝を地面につけない、倒れない。'};
async function bounds(p){const {data,info}=await sharp(p).ensureAlpha().raw().toBuffer({resolveWithObject:true});let l=info.width,t=info.height,r=-1,b=-1;for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){l=Math.min(l,x);t=Math.min(t,y);r=Math.max(r,x);b=Math.max(b,y)}return{width:info.width,height:info.height,left:l,top:t,right:r,bottom:b,opaqueHeight:b-t+1};}
async function init(){
 const plan=read('art_source/paper_roster_idle/generation-plan.json'),idle=read('art_source/paper_roster_idle/manifest.json');
 const styles=read('art_source/paper_cutout/runtime-manifest.json').poses;
 const order=['toma','ren','minato','shiro','yui','king','akatsuki','ryuta'];
 for(const key of order){const c=plan.characters.find(x=>x.key===key),base=idle.entries.find(x=>x.key===key),size=await bounds(base.prepared);
  const jobs=[];for(const pose of Object.keys(labels)){const version=key==='toma'&&['guard','crouch'].includes(pose)?2:1;
   const style=styles.find(p=>p.key===(pose==='lose'?'ko':pose)).source;
   const animal=key==='shiro'?'白い犬型AIは必ず四足の犬の体のまま。人間の手・拳や二足立ちにしない。ガードは四足を広げ頭を低めに守る、しゃがみは四肢を曲げ腹と肩を低くする、カウンターは片前足を少し上げ払う準備で他の三足を接地、勝利は四足で立ち耳を起こして小さく笑う、敗北は四足で立ったまま耳と尾と頭を下げる。':'';
   const held=key==='yui'?'杖は常に本人の左手。本人の右手は肉球ミトン。杖を持つ手を交換しない。カウンターでは杖を少し斜め前に構え、空いた右ミトンを開いて払う。':'';
   const prompt=`用途：オリジナルゲーム「あにあにバトル！！」の全身1枚絵スプライト。入力1は${c.name}の採用済み待機で、人物・服装・頭身・頭と靴の画素寸法の厳密な基準兼編集対象。入力2は千鶴の同動作の画風参考だけ（千鶴の顔や衣装・体型に変えない、勝敗のポーズは下記の控えめな立ち姿を優先）。入力3は元作品の容姿資料だけで、リアルな頭身や背景を写さない。入力1と同じ普通のアニメ塗り、くっきりした濃い輪郭、色面で分けた影。1枚の全身イラスト、関節分割なし。人物設定：${c.detail} ただし姿勢は待機でなく下記に変える。${animal}${held} 今回の動作：${labels[pose]}。${poseText[pose]} 出力は入力1と同じ1304×1450px。頭・髪・手・靴の大きさは入力1から変えない。${pose==='crouch'?'頭の上を大きく空けること。全高目安'+Math.round(size.opaqueHeight*.63)+'pxで人物を小さく描き直すのでなく膝を折る。':'背丈と体格を入力1と保つ。'}接地はy=1328px。人物の上下左右に余白を残し、靴・髪・持ち物を切らない。正面でも真横でもない右向き3/4、両肩・両手・両足が読める。胸側か背中側かを一貫させ、右手を2つ作らない。人間の手は親指1本と4指（ミトン衣装はミトン）。全身外周に入力1と同じ細いクリーム白の切り抜き縁を3〜6px程度でくっきり一周。白い縁の外は完全透明。グロー、ぼかし、にじみ、半透明の光彩を禁止、外周アンチエイリアスは最小1px。背景・床・影・文字・名前・効果線・武器追加・小物追加なし。紙の繊維や折れ目の質感、3D、ピクセルアート、複数ポーズのシートは禁止。`;
   const job={key,character:c.name,pose,label:labels[pose],version,source:`${ROOT}/${key}/generated/${key}_${pose}_v${version}.png`,references:[base.prepared,style,c.reference],referenceRoles:['採用待機：容姿・衣装・部位実寸の基準兼編集対象','千鶴：画風と動作の意味のみ','元作品：容姿のみ。頭身は入力1'],prompt,tool:'built-in image_gen',status:'planned'};
   const file=`${ROOT}/${key}/prompts/${pose}_v${version}.json`;if(!fs.existsSync(file))write(file,job);jobs.push({pose,version,file});
  }const file=`${ROOT}/${key}/manifest.json`;if(!fs.existsSync(file))write(file,{schemaVersion:1,key,name:c.name,baseIdle:base.prepared,baseline:1328,canvas:{width:1304,height:1450},pixelsPerUnit:532.5893,referenceBounds:size,integrated:false,verifiedTag:null,jobs,entries:[]});
 }console.log(JSON.stringify({characters:8,newPoses:40,reuseChizuru:5}));
}
async function begin(key,pose){const m=read(`${ROOT}/${key}/manifest.json`),j=read(m.jobs.find(x=>x.pose===pose).file);if(fs.existsSync(j.source))throw Error('原画は既に保存済み。再生成しない: '+j.source);write(`${ROOT}/pending-job.json`,{key,pose,version:j.version,before:fs.readdirSync(GEN).filter(n=>n.endsWith('.png'))});console.log(JSON.stringify(j));}
async function saveRaw(key,pose,source){const file=`${ROOT}/${key}/manifest.json`,m=read(file),job=read(m.jobs.find(j=>j.pose===pose).file);
 const allowed=path.resolve(process.env.USERPROFILE,'.codex/generated_images')+path.sep;
 if(!path.resolve(source).toLowerCase().startsWith(allowed.toLowerCase())||path.extname(source)!=='.png')throw Error('内蔵ツール出力以外の取り込みを拒否');
 fs.mkdirSync(path.dirname(job.source),{recursive:true});
 if(fs.existsSync(job.source)){if(sha(source)!==sha(job.source))throw Error('異なる原画の上書き禁止')}else fileRetry(()=>fs.copyFileSync(source,job.source));
 const digest=sha(job.source),size=await bounds(job.source),existing=m.entries.find(e=>e.pose===pose&&e.version===job.version);
 if(existing){if(existing.sha256!==digest)throw Error('保存済み記録と原画SHAが不一致')}else m.entries.push({...job,sha256:digest,native:size,status:'candidate',artApproved:false,gameConnected:false});
 write(file,m);console.log(JSON.stringify({saved:job.source,native:size,count:m.entries.length}));
}
async function capture(){const p=read(`${ROOT}/pending-job.json`),files=fs.readdirSync(GEN).filter(n=>n.endsWith('.png')&&!p.before.includes(n));if(files.length!==1)throw Error('生成元を一意に特定できない: '+files.length);return saveRaw(p.key,p.pose,path.join(GEN,files[0]));}
async function job(key,pose){const m=read(`${ROOT}/${key}/manifest.json`),j=read(m.jobs.find(x=>x.pose===pose).file);if(fs.existsSync(j.source))throw Error('原画は保存済み: '+j.source);console.log(JSON.stringify(j));}
async function prepare(key,tag){
 const file=`${ROOT}/${key}/manifest.json`,m=read(file),idle=read('art_source/paper_roster_idle/manifest.json').entries.find(x=>x.key===key);
 const entries=m.jobs.map(j=>m.entries.find(e=>e.pose===j.pose&&e.version===j.version));if(entries.some(e=>!e))throw Error('5ポーズを揃えてから梱包する');
 const folder=`イラスト確認/${m.name}/基本5ポーズ_${tag?'ゲーム採用済み':'確認中'}`;
 const pendingFolder=`イラスト確認/${m.name}/基本5ポーズ_確認中`;
 if(tag&&fs.existsSync(pendingFolder)&&!fs.existsSync(folder)){
  const workspace=path.resolve('.')+path.sep,from=path.resolve(pendingFolder),to=path.resolve(folder);
  if(!from.startsWith(workspace)||!to.startsWith(workspace))throw Error('確認フォルダの移動先が作業領域外');
  fs.renameSync(from,to);
 }
 fs.mkdirSync(folder,{recursive:true});
 const packed=[],cards=[{label:'待機（基準）',prepared:idle.prepared}];
 for(const e of entries){const {data,info}=await sharp(e.source).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  // The task explicitly requests binary alpha at the cutout contour. Keep all RGB and every opaque interior pixel unchanged.
  let alphaChanged=0;for(let i=3;i<data.length;i+=4){const a=data[i],next=a>=128?255:0;if(a!==next){data[i]=next;alphaChanged++;}}
  const clean=`${ROOT}/${key}/edge_clean/${key}_${e.pose}.png`;fs.mkdirSync(path.dirname(clean),{recursive:true});await sharp(data,{raw:{width:info.width,height:info.height,channels:4}}).png().toFile(clean);
  const b=await bounds(clean);
  if(b.left===0||b.top===0||b.right===info.width-1||b.bottom===info.height-1)throw Error('原画端に人物が接しているため切断を要審査: '+e.source);
  const dx=Math.floor((1304-info.width)/2),dy=1328-b.bottom,out=Buffer.alloc(1304*1450*4);let copied=0;
  for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++){const i=(y*info.width+x)*4,xx=x+dx,yy=y+dy;if(xx<0||xx>=1304||yy<0||yy>=1450){if(data[i+3]!==0)throw Error('人物を切るため梱包停止: '+e.source);continue;}data.copy(out,(yy*1304+xx)*4,i,i+4);copied++;}
  const p=`${ROOT}/${key}/prepared/${key}_${e.pose}.png`;fs.mkdirSync(path.dirname(p),{recursive:true});await sharp(out,{raw:{width:1304,height:1450,channels:4}}).png().toFile(p);
  const decoded=await sharp(p).ensureAlpha().raw().toBuffer();if(!decoded.equals(out))throw Error('梱包画素不一致');
  let footMin=1304,footMax=-1;for(let y=Math.max(0,b.bottom-Math.max(32,Math.floor(b.opaqueHeight*.1)));y<=b.bottom;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){footMin=Math.min(footMin,x+dx);footMax=Math.max(footMax,x+dx);}
  Object.assign(e,{prepared:p,preparedSha256:sha(p),edgeCleaned:clean,edgeCleanedSha256:sha(clean),edgeCleanup:{method:'alpha threshold only; RGB and opaque interior unchanged',threshold:128,changedAlphaPixels:alphaChanged,referenceInstruction:'docs/codex_tasks/PAPER_ROSTER_CORE_POSES.md#白い切り抜き縁の品質'},offsetX:dx,offsetY:dy,sourcePixelsPreserved:alphaChanged===0,edgeCleanedPixelsPreserved:true,opaqueInteriorPixelsPreserved:true,noSourceResampling:true,status:tag?'unity_verified':'prepared_for_unity',gameConnected:!!tag,headTopY:b.top+dy,footMinX:footMin,footMaxX:footMax,opaqueHeightRatio:b.opaqueHeight/m.referenceBounds.opaqueHeight});
  packed.push({key:e.pose,prepared:p,sha256:e.preparedSha256,source:e.source,sourceSha256:e.sha256,edgeCleaned:clean,headTopY:e.headTopY,footMinX:footMin,footMaxX:footMax});cards.push({label:e.label,prepared:p});
 }
 m.integrated=!!tag;m.verifiedTag=tag||null;write(file,m);
 const runtimeFile=ROOT+'/runtime-manifest.json',runtime=fs.existsSync(runtimeFile)?read(runtimeFile):{schemaVersion:1,canvasWidth:1304,canvasHeight:1450,pixelsPerUnit:532.5893,characters:[]};
 const index={toma:0,ren:1,minato:3,shiro:4,yui:5,king:6,akatsuki:7,ryuta:8}[key];
 const row={key,characterIndex:index,name:m.name,idle:{prepared:idle.prepared,sha256:sha(idle.prepared),headTopY:m.referenceBounds.top,footMinX:m.referenceBounds.left,footMaxX:m.referenceBounds.right},poses:packed};
 runtime.characters=runtime.characters.filter(c=>c.key!==key).concat(row).sort((a,b)=>a.characterIndex-b.characterIndex);write(runtimeFile,runtime);
 const pictures=[];let titles='';for(let i=0;i<cards.length;i++){const x=i%3*440,y=Math.floor(i/3)*520;const c=cards[i];fs.copyFileSync(c.prepared,folder+'/'+String(i).padStart(2,'0')+'_'+c.label+'.png');pictures.push({input:await sharp(c.prepared).resize(430,478).png().toBuffer(),left:x+5,top:y+41});titles+=`<text x="${x+220}" y="${y+30}" text-anchor="middle">${m.name} ${c.label}</text>`;}
 await sharp({create:{width:1320,height:1040,channels:4,background:'#303748'}}).composite([...pictures,{input:Buffer.from(`<svg width="1320" height="1040"><style>text{font-family:'Yu Gothic',sans-serif;fill:#fff4d9;font-size:22px}</style>${titles}</svg>`),left:0,top:0}]).png().toFile(folder+'/00_ポーズ一覧.png');
 fs.writeFileSync(folder+'/最初に読んでください.txt',`${m.name}：待機＋ガード・しゃがみ・カウンター・勝利・敗北。${tag?'ゲーム接続・実Unity全4工程を検証済み：'+tag:'ゲーム接続の準備中。Unity未検証。'}\n全画像1304×1450、PPU532.5893、足元1328。人物は拡縮せず、外縁アルファの2値化と透明余白だけを調整。原画と処理後を別保存。採用と絵の最終承認は別です。原画・全文条件・審査はart_source/paper_roster_core/${key}/manifest.json。\n`);
 console.log(JSON.stringify({key,poses:5,packedWithoutResampling:true,folder,tag:tag||null}));
}
async function main(){const [mode,key,pose,image]=process.argv.slice(2);if(mode==='init')return init();if(mode==='job')return job(key,pose);if(mode==='begin')return begin(key,pose);if(mode==='save')return saveRaw(key,pose,image);if(mode==='capture')return capture();if(mode==='prepare')return prepare(key,pose);throw Error('init | job/begin <key> <pose> | save <key> <pose> <toolOutputPng> | capture | prepare <key> [verifiedTag]');}main().catch(e=>{console.error(e.message);process.exitCode=1});
