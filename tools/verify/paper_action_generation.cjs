// Persist one character's action jobs; preserve generated RGBA and magnification.
const fs = require('node:fs'), path = require('node:path'), crypto = require('node:crypto'), sharp = require('sharp');
const root = 'art_source/paper_roster_action';
const read = p => JSON.parse(fs.readFileSync(p, 'utf8').replace(/^\uFEFF/, ''));
const write = (p, value) => { fs.mkdirSync(path.dirname(p), { recursive: true }); fs.writeFileSync(p, JSON.stringify(value, null, 2) + '\n'); };
const sha = p => crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const requireThat = (ok, message) => { if (!ok) throw Error(message); };
const poses = {
 normal1: ['通常1段', '胸元へ両手で小型拳銃を引き、肘を曲げて構える。腰と両膝を少し曲げ、ほんの少し後ろへ体重を置く。まだ発砲しない。', '両手で同じ拳銃を前方の画面右へ伸ばし、軽く一歩踏み込んで射撃する瞬間。両腕の肩・肘・手首がつながり、右手が銃を握り左手が右手を下から支える。顔を銃で隠さない。'],
 normal2: ['通常2段', '同じ拳銃を両手で胸の前へ引き戻す。腰と胸を1段目より少しひねり、左脚へ体重を移して次弾を構える。まだ発砲しない。', '腰と肩を切り返し、同じ拳銃を両手で前方の画面右へ突き出して射撃。1段目とは違う足の開きと体のひねり。背中ではなく胸元を斜めから見せ、左右の腕が自然につながる。'],
 normal3: ['通常3段', '三段の中で最も大きく溜める。両膝を深く曲げて腰を後ろへ引き、両手の同じ小型拳銃を胸元に引き付ける。鋭い決意の顔。頭と靴は拡大しない。', '三段の中で最も大きく踏み込んだ射撃の決めポーズ。前脚へ体重を乗せ、腰・胸・両腕を一斉に前へ伸ばす。同じ小型拳銃を両手で握る。銃や頭を大型化しない。'],
 up: ['地上上攻撃', '両脚を接地し、同じ小型拳銃を両手で胸元へ引き付けて上方を狙う準備。顔は画面右上を見て、肘を曲げる。', '両脚を接地したまま両腕と同じ拳銃を頭の右上へ向け、ほぼ真上へ射撃する決めポーズ。髪を腕で隠さない。銃口・両手を切らない。'],
 down: ['地上下攻撃', '両脚を接地し腰を低くした構え。両手で同じ小型拳銃を胸元へ引き付ける。膝と股関節を曲げるが頭・髪・靴は待機と同じ実寸。', '腰を低くしたまま両手で同じ小型拳銃を前方やや下、画面右下へ伸ばして射撃する。しゃがみの余白を埋めるため人物を拡大しない。両足は接地。'],
 air: ['空中ニュートラル', '空中で両膝を軽く曲げて両脚を浮かせ、両手の同じ小型拳銃を胸元に引き付ける。空中射撃の準備。地面や影なし。', '両脚を空中で曲げたまま、両手の同じ小型拳銃を画面右へ伸ばして射撃する。跳躍中とわかる全身シルエット。頭や靴は同じ実寸。'],
 special: ['特殊ドローン指令', '銃を持たず、左手に小さな暗色の通信端末を持ち、右手を胸元に引いて指示を出す準備。両足は接地、前方の画面右を見る。ドローン自体は描かない。', '左手の同じ通信端末を胸元に持ち、右腕を画面右へ伸ばしてドローンの突入を指示する決めポーズ。人差し指で指し示し、右手の甲と親指のつながりを正しく描く。ドローン自体は別素材なので描かない。'],
 super: ['超必殺ロケット指令', '銃を持たず、左手の小さな暗色通信端末を胸元で構え、右腕を大きく後ろへ引く。両膝を曲げて最大の指令の予備動作、冷静で鋭い表情。', '左手の通信端末を胸元に持ち、右腕を斜め下前方へ強く振り下ろし、画面右の標的へ落下攻撃を命令する決めポーズ。両足は大きく開いて接地。ロケットや背景は別素材なので描かない。']
};
async function pixels(p) { return sharp(p).ensureAlpha().raw().toBuffer({ resolveWithObject: true }); }
function bounds(data, info) {
 let left = info.width, top = info.height, right = -1, bottom = -1;
 for (let y=0;y<info.height;y++) for(let x=0;x<info.width;x++) if(data[(y*info.width+x)*4+3]>=32) {
  left=Math.min(left,x);top=Math.min(top,y);right=Math.max(right,x);bottom=Math.max(bottom,y);
 }
 requireThat(right>=left,'No visible character'); return {left,top,right,bottom};
}
function feet(data,info,b) {
 let left=info.width,right=-1;
 for(let y=Math.max(0,b.bottom-Math.max(32,Math.floor((b.bottom-b.top+1)*.1)));y<=b.bottom;y++)
  for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){left=Math.min(left,x);right=Math.max(right,x);}
 return {left,right,center:(left+right)/2};
}
async function init(key) {
 requireThat(key==='toma','本人の指示を追加してから次の人物を初期化する');
 const folder=`${root}/${key}`, file=`${folder}/manifest.json`; if(fs.existsSync(file)){console.log(file);return;}
 const reference='art_source/paper_roster_idle/generated/toma_idle_v2.png'; const jobs=[];
 for(const [pose,[label,prepare,impact]] of Object.entries(poses)) for(const [phase,text] of [['prepare',prepare],['impact',impact]]) {
  const id=pose+'_'+phase, prompt=`参照画像は本作の透真の採用待機であり、同じ人物・同じ絵柄・同じ人物倍率で新しい1姿勢だけ描いてください。通常のアニメ塗りの全身ゲームスプライト。黒い大きな髪と青い瞳、黒スーツ、白シャツ、黒ネクタイ、黒革靴を厳密に維持。頭身・頭や髪の大きさ・手と靴の大きさは参照と同じ。\n動作：${label}の${phase==='prepare'?'予備動作（まだ当たらない）':'攻撃／指令の決めポーズ'}。${text}\n銃を持つ場合は右手で握る小型の黒い拳銃1丁、左手が右手を支える両手構え。拳銃は角形の黒いスライドと短い銃身、細いシアンのアクセント1本。同じ銃を全姿勢で使い、銃口炎・弾丸・流血・相手・効果線は描かない。手は左右1本ずつ、各5指。胸側を斜めから見た肩→肘→手首→手のつながりを正しく描く。\n参照の原画と同じ透明キャンバス1189×1323を厳守。共通ゲーム枠への余白追加は後で行うため、出力画像の縦横へ人物を合わせて縮小しない。人物の再拡縮禁止、立ち基準の髪の横幅約544px、奥の靴約218px、手前の靴約306pxを維持。接地姿勢は靴底y1291。低い姿勢では上の透明余白を残す。画面右向きの斜め横、両腕両脚が見える。髪、靴、手、銃口まで全身を余白付きで収め、切らない。薄い硬いクリーム白の切り抜き縁3〜6pxを手と銃を含む外周一周へ。縁の外は完全透明、グロー・ぼかし・半透明の光彩なし。背景、床、影、文字、名前、枠、複数ポーズ、複数人、紙の繊維、関節分割、3D、ピクセルアートは禁止。描画品質と顔は参照を維持。`;
  const job={id,key,character:'透真',pose,phase,label,version:1,source:`${folder}/generated/${id}_v1.png`,references:[reference],referenceRoles:['採用待機：直接の編集対象、容姿・部位実寸・倍率の基準'],prompt,tool:'built-in image_gen',status:'planned'};
  const promptFile=`${folder}/prompts/${id}_v1.json`;write(promptFile,job);jobs.push({id,version:1,promptFile});
 }
 write(file,{schemaVersion:1,key,name:'透真',characterIndex:0,canvas:{width:1304,height:1450},sole:1328,pixelsPerUnit:532.5893,baseIdle:reference,jobs,entries:[],integrated:false,verifiedTag:null});
 console.log(JSON.stringify({file,jobs:jobs.length}));
}
async function job(key,id) {const m=read(`${root}/${key}/manifest.json`), j=read(m.jobs.find(j=>j.id===id).promptFile);requireThat(!fs.existsSync(j.source),'原画保存済み。重複生成しない');console.log(JSON.stringify(j));}
async function save(key,id,source) {
 const file=`${root}/${key}/manifest.json`,m=read(file),j=read(m.jobs.find(j=>j.id===id).promptFile);
 const allowed=path.resolve(process.env.USERPROFILE,'.codex/generated_images')+path.sep;
 requireThat(path.resolve(source).toLowerCase().startsWith(allowed.toLowerCase()),'内蔵ツールの出力を指定');
 fs.mkdirSync(path.dirname(j.source),{recursive:true});
 if(fs.existsSync(j.source))requireThat(sha(j.source)===sha(source),'異なる原画を上書きしない');else fs.copyFileSync(source,j.source);
 const {data,info}=await pixels(j.source),nativeBounds=bounds(data,info),digest=sha(j.source);
 const old=m.entries.find(e=>e.id===id&&e.version===j.version);
 if(old)requireThat(old.sha256===digest,'記録済み原画と相違');else m.entries.push({...j,sha256:digest,native:{width:info.width,height:info.height,bounds:nativeBounds},status:'candidate',artApproved:false,gameConnected:false});
 write(file,m);console.log(JSON.stringify({saved:j.source,sha256:digest,native:{width:info.width,height:info.height,bounds:nativeBounds}}));
}
async function effectSave(key,id,source) {
 const file=`${root}/${key}/effects-manifest.json`,m=read(file),job=m.jobs.find(j=>j.id===id);requireThat(job,'Unknown effect');
 const j=read(job.promptFile),allowed=path.resolve(process.env.USERPROFILE,'.codex/generated_images')+path.sep;
 requireThat(path.resolve(source).toLowerCase().startsWith(allowed.toLowerCase()),'内蔵ツールの出力を指定');
 fs.mkdirSync(path.dirname(j.source),{recursive:true});
 if(fs.existsSync(j.source))requireThat(sha(j.source)===sha(source),'異なる原画を上書きしない');else fs.copyFileSync(source,j.source);
 const {data,info}=await pixels(j.source),nativeBounds=bounds(data,info),digest=sha(j.source),old=m.entries.find(e=>e.id===id&&e.version===j.version);
 if(old)requireThat(old.sha256===digest,'記録済み原画と相違');else m.entries.push({...j,sha256:digest,native:{width:info.width,height:info.height,bounds:nativeBounds},status:'candidate',artApproved:false,gameConnected:false});
 write(file,m);console.log(JSON.stringify({saved:j.source,sha256:digest,native:{width:info.width,height:info.height,bounds:nativeBounds}}));
}
async function pack(key, partial=false) {
 const file=`${root}/${key}/manifest.json`,m=read(file),entries=m.jobs.map(j=>m.entries.find(e=>e.id===j.id&&e.version===j.version));
 if(!partial)requireThat(entries.every(Boolean),'本人の16原画を揃えてから梱包する');
 const output=[];
 const idle=await pixels(`art_source/paper_roster_idle/prepared/${key}_idle.png`);
 const idleBounds=bounds(idle.data,idle.info),anchor=feet(idle.data,idle.info,idleBounds).center;
 let airAnchor=null;
 const firstAir=entries.find(e=>e&&e.id==='air_prepare');
 if(firstAir){const a=await pixels(firstAir.source),b=bounds(a.data,a.info);airAnchor=feet(a.data,a.info,b).center+Math.floor((1304-a.info.width)/2);}
 const reviewFolder=`イラスト確認/${m.name}/攻撃原画_確認中`;fs.mkdirSync(reviewFolder,{recursive:true});
 fs.copyFileSync(`art_source/paper_roster_idle/prepared/${key}_idle.png`,`${reviewFolder}/00_待機基準.png`);
 for(const e of entries.filter(Boolean)) {
  const {data,info}=await pixels(e.source),b=bounds(data,info),span=feet(data,info,b);
  const desired=Math.round((e.pose==='air'?airAnchor:anchor)-span.center);
  const dx=info.width<=1304?Math.max(0,Math.min(1304-info.width,desired)):desired,dy=1328-b.bottom;
  requireThat(b.left>0&&b.top>0&&b.right<info.width-1&&b.bottom<info.height-1,e.id+':原画端の切断を要確認');
  const packed=Buffer.alloc(1304*1450*4);let croppedOutsideAlphaOnePixels=0;
  for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++) {
   const i=(y*info.width+x)*4,xx=x+dx,yy=y+dy;
   if(xx<0||xx>=1304||yy<0||yy>=1450){
    // The raw original stays intact. Report isolated 1/255 canvas noise; never crop a visible figure pixel.
    requireThat(data[i+3]<=1,e.id+':透明以外の画素が枠外へ出るため停止');
    if(data[i+3]===1)croppedOutsideAlphaOnePixels++;continue;
   }
   data.copy(packed,(yy*1304+xx)*4,i,i+4);
  }
  const prepared=`${root}/${key}/prepared/${e.id}.png`;fs.mkdirSync(path.dirname(prepared),{recursive:true});await sharp(packed,{raw:{width:1304,height:1450,channels:4}}).png().toFile(prepared);
  requireThat((await pixels(prepared)).data.equals(packed),'梱包画素が一致しない');
  Object.assign(e,{prepared,preparedSha256:sha(prepared),offsetX:dx,offsetY:dy,sourceRgbaPreserved:croppedOutsideAlphaOnePixels===0,
   rgbaWithinCanvasPreserved:true,croppedOutsideAlphaOnePixels,status:'packed_for_review'});
  output.push({id:e.id,prepared,sha256:e.preparedSha256,headTopY:b.top+dy,footMinX:span.left+dx,footMaxX:span.right+dx,groundFootAnchor:anchor,
   originOffsetY:e.pose==='air'?(b.top+dy-idleBounds.top)/m.pixelsPerUnit:0});
  fs.copyFileSync(prepared,`${reviewFolder}/${e.label}_${e.phase==='prepare'?'予備':'当てる'}.png`);
 }
 fs.writeFileSync(`${reviewFolder}/必ず読んでください.txt`,'このフォルダは新しい攻撃原画の確認中候補です。ゲームにはまだ差し替えていません。現在のゲームはR3の技と旧・仮の表示です。待機基準と頭・靴・手を比べてください。原画の画素と倍率を保った透明梱包で、絵の最終承認とUnity合格は別です。\n');
 write(file,m);write(`${root}/${key}/packed-manifest.json`,{schemaVersion:1,key,frames:output,complete:entries.every(Boolean),reviewed:false,gameConnected:false});console.log(JSON.stringify({key,packed:output.length,complete:entries.every(Boolean)}));
}
const [command,key,id,source]=process.argv.slice(2);
({init,job,save,'save-effect':effectSave,pack:key=>pack(key),review:key=>pack(key,true)}[command]||(()=>{throw Error('init/job/save/save-effect/pack/reviewを指定')}))(key,id,source).catch(e=>{console.error(e.stack);process.exitCode=1});
