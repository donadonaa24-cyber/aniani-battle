// Native image records and integer-only transparent packing. No image generation API here.
const fs=require('fs'),path=require('path'),crypto=require('crypto'),sharp=require('sharp');
const root='art_source/paper_roster_motion',order=['toma','ren','minato','shiro','yui','king','akatsuki','ryuta'];
const action=require('./paper_action_jobs.cjs');
const labels={run:'走り',jump:'ジャンプ上昇',fall:'落下',dodge:'回避',counter_success:'カウンター成功',hit_light:'通常被弾',hit_heavy:'強被弾'};
const poses={
 run:'走る瞬間の強い決め姿。右へ進む両腕の振り、前脚を大きく出し後ろ脚の膝を曲げる。胸と腰の軸はほぼ直立〜前傾10度まで。ゲーム内で絵全体を45度傾けるので、絵に45度の前傾を重ねない。',
 jump:'ジャンプの上昇中。両膝を曲げて両足を浮かせ、両肘も曲げて身体の横へ。上を見た集中した顔。空中攻撃ではなく上昇の明快な全身シルエット。',
 fall:'ジャンプから落下中。足を下へ伸ばし膝を柔らかく曲げ、両腕を横へ少し開き着地に備える。両手は待機と同じ軽い握り拳。目線は斜め下前方、ジャンプ上昇と異なる姿。両足は浮いている。',
 dodge:'攻撃をくぐる回避。膝と腰を深く落とし、上体を右前へ少し倒す低い姿勢。両腕は身体の横へ引き、両手は待機と同じ軽い握り拳。顔は前を見て警戒。立ち絵や攻撃と区別できる低いシルエット。',
 counter_success:'防御から反撃が成功した短い左の正拳突き。本人左肩から上腕・肘・前腕・左拳まで自然なほぼ一直線で右前方へ伸ばす。本人右拳は胸前に構える。両腕はそれぞれの肩から追えるようにつなぎ、逆に折れた肘や顔の前を不自然に横切る腕を描かない。両足で踏み込む鋭い決め。',
 hit_light:'通常攻撃を受けた瞬間。右前方から打撃を受けて胸と顔が少し左後ろへ傾き、片足を後ろへ踏み出す。両膝は曲がり、拳を胸前へ引き、歯を食いしばって何とか踏ん張る。まだ立って耐えている。',
 hit_heavy:'ハートが減る強い被弾。右前方の衝撃に胸と頭が大きく左後ろへのけぞる。両膝を曲げ一方の足は後ずさり、両手を胸の前に広げる。目をぎゅっとつむり口を大きく開け、目尻に涙粒。いかにも痛そうなコミカルな顔。血・傷・破れなし。'
};
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
function write(p,j){fs.mkdirSync(path.dirname(p),{recursive:true});fs.writeFileSync(p,JSON.stringify(j,null,2)+'\n');}
const check=(v,m)=>{if(!v)throw Error(m);};
async function pixels(p){return sharp(p).ensureAlpha().raw().toBuffer({resolveWithObject:true});}
function bounds(data,info){let left=info.width,top=info.height,right=-1,bottom=-1;for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){left=Math.min(left,x);top=Math.min(top,y);right=Math.max(right,x);bottom=Math.max(bottom,y);}check(right>=left,'Empty image');return{left,top,right,bottom};}
function feet(data,info,b){let left=info.width,right=-1;for(let y=Math.max(0,b.bottom-Math.max(32,Math.floor((b.bottom-b.top+1)*.1)));y<=b.bottom;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){left=Math.min(left,x);right=Math.max(right,x);}return{left,right,center:(left+right)/2};}
async function init(key){
 check(order.includes(key),'Unknown character');const folder=`${root}/${key}`,file=`${folder}/manifest.json`;if(fs.existsSync(file)){console.log(file);return;}
 const c=key==='toma'?{name:'透真',index:0,reference:'art_source/paper_roster_idle/generated/toma_idle_v2.png',identity:'黒い大きな跳ね髪、青い瞳、黒いスーツ、白シャツ、緩めた黒ネクタイ、黒革靴。生身の両腕、装備や義手なし。'}:action[key];
 const meta=await sharp(c.reference).metadata();const jobs=[];
 for(const [id,label]of Object.entries(labels)){
  let pose=poses[id];
  if(key==='shiro')pose={run:'右へ駆ける四足走行。前脚と後脚の一組を前へ、もう一組を後ろへ、全4脚を別々に明快に描く。胴体は水平で顔は右へ。',jump:'四足で右斜め上へ跳ぶ。前脚2本と後脚2本の膝を曲げて浮かせる。顔は右上を見る。',fall:'四足で着地へ備える落下中。前脚2本を下へ伸ばし、後脚2本は軽く曲げる。顔は右下、胴は水平。',dodge:'攻撃をくぐるよう四足を曲げ、胸を低く伏せ、頭と耳を低く保ち右を見る。尾は1本だけ。',counter_success:'受けた直後に右前へ鋭く噛み返す全身の決め。前脚2本で支え、後脚2本を踏ん張り、口を開く。牙は自然、顔を巨大化しない。',hit_light:'右前方の打撃に頭と胸を少し後ろへ引き、四脚で耐える。耳を少し伏せ、目を細めて痛みをこらえる。4本の足先が全部見える。',hit_heavy:'右前方の強打に頭と胸が大きく後ろへのけぞり、前脚2本が浮き、後脚2本を曲げて後ずさる。耳を伏せ目を閉じ、口を大きく開け目尻に涙。血や傷なし、4脚と尾1本を守る。'}[id];
  if(id==='counter_success'&&key==='minato')pose='反撃成功の決め。本人左の機械拳を右前へ伸ばして短い反撃パンチ、本人右の生身拳は胸前へ。両足で踏み込み冷静な表情。';
  if(id==='counter_success'&&key==='ren')pose='反撃成功の決め。片足で踏み込み、反対の膝を上げ、両方の白い肉球ミトンを前へ強く出す可愛い体当たり。顔と犬頭巾が明快、頭とミトンの大きさを変えない。';
  if(id==='counter_success'&&key==='yui')pose='反撃成功の決め。本人左の人間の手で同じステッキを前へ突き出し、本人右の肉球ミトンは胸へ引く。両足で踏み込み可愛くきりっとした顔。魔法や効果は別素材なので描かない。';
  if(id==='counter_success'&&key==='king')pose='反撃成功の決め。両足を広げて踏み込み、片方の爪の手袋を右前へ引っかくように振り出し、反対の爪は身体の横へ引く。獅子の頭巾の中の人間の顔を明快に、獰猛な笑み。';
  if(id==='counter_success'&&key==='akatsuki')pose='反撃成功の決め。本人左手に持つカードを右前へ振り出し、本人右手は胸へ引く。左手の親指と人差し指でカードの下角を挟み残る3指は自然に曲げる。両足で踏み込み冷静な顔。カード背面は既存と同じ紺と金の料理モチーフ。';
  if(key==='yui')pose+=' 本人左の人間の手は採用待機と同じステッキを握って離さない。本人右手は白い肉球ミトンで、人間の指にしない。左手を開く一般指定よりステッキの自然な左手握りを優先する。';
  if(key==='akatsuki')pose+=' 本人左手には待機と同じカードを保持。左の親指と人差し指でカード下角を挟み、他の3指は自然に曲げる。手を開く一般指定よりカードの左手つまみを優先。本人右手は空手。';
  if(key==='ren')pose+=' 両手は採用待機と同じ白い肉球ミトン。人間の裸の指や手を描かない。';
  const prompt=`Use case: identity-preserve. ゲーム用全身原画1枚だけ。画像1は本人の採用待機、編集対象と容姿・頭身・部位の実寸基準。${c.identity}\n新しい動作「${label}」：${pose}\n本人の髪・顔・衣装・左右の装備・靴や足先を厳密に維持。同じ普通のアニメの線とセル塗り、薄い硬いクリーム白の切り抜き縁を全外周へ。紙の繊維なし。右向きの斜め横で顔と両腕両脚を見せる。画面の左右と本人の左右を混同せず、肩→肘→手首→手を自然につなぐ。人間は各手5指、左右1本ずつ。ミトンを人の指へ変えない。\n参照と同じ透明キャンバス${meta.width}×${meta.height}。人物を枠へ合わせて拡縮しない。頭髪・顔・手・靴の実寸は採用待機を維持し、低姿勢は上に大きな透明余白を残す。頭・手先・靴・装備を切らず余白付きの全身。顔や手だけを巨大化しない。道具を動作と無関係に増やさず、指定がない動作では待機と同じ持ち物。縁の外は本物の完全透明。背景、床、影、効果線、衝撃、文字、名前、相手、複数ポーズ、連続コマ、グロー、3D、ピクセルアートは禁止。`;
  const job={id,label,version:1,source:`${folder}/generated/${id}_v1.png`,prompt,references:[c.reference],referenceRoles:['採用待機：編集対象、本人の容姿・部位実寸・倍率の基準'],tool:'built-in image_gen'};
  const promptFile=`${folder}/prompts/${id}_v1.json`;write(promptFile,job);jobs.push({id,version:1,promptFile});
 }
 write(file,{schemaVersion:1,key,name:c.name,characterIndex:c.index,baseIdle:c.reference,canvasWidth:1304,canvasHeight:1450,pixelsPerUnit:532.5893,sole:1328,jobs,entries:[],integrated:false,verifiedTag:null});console.log(JSON.stringify({key,jobs:jobs.length}));
}
async function save(key,id,source){const file=`${root}/${key}/manifest.json`,m=read(file),j=read(m.jobs.find(j=>j.id===id).promptFile);check(path.resolve(source).toLowerCase().startsWith((path.resolve(process.env.USERPROFILE,'.codex/generated_images')+path.sep).toLowerCase()),'Use built-in output');fs.mkdirSync(path.dirname(j.source),{recursive:true});if(fs.existsSync(j.source))check(sha(j.source)===sha(source),'Do not overwrite original');else fs.copyFileSync(source,j.source);const {data,info}=await pixels(j.source);if(!m.entries.some(e=>e.id===id&&e.version===j.version))m.entries.push({...j,sha256:sha(j.source),native:{width:info.width,height:info.height,bounds:bounds(data,info)},status:'candidate',artApproved:false});write(file,m);console.log(j.source);}
async function revise(key,id,note){const file=`${root}/${key}/manifest.json`,m=read(file),j=m.jobs.find(j=>j.id===id),old=read(j.promptFile);check(fs.existsSync(old.source)&&note,'Save original and specify correction');const version=j.version+1,next={...old,version,source:old.source.replace(/_v\d+\.png$/,`_v${version}.png`),prompt:old.prompt+'\n修正条件：'+note};m.entries.find(e=>e.id===id&&e.version===j.version).status='revision_required';j.version=version;j.promptFile=j.promptFile.replace(/_v\d+\.json$/,`_v${version}.json`);write(j.promptFile,next);write(file,m);}
async function pack(key){
 const file=`${root}/${key}/manifest.json`,m=read(file),chosen=m.jobs.map(j=>m.entries.find(e=>e.id===j.id&&e.version===j.version));check(chosen.every(Boolean),'Complete all seven before packing');
 const idle=await pixels(`art_source/paper_roster_idle/prepared/${key}_idle.png`),ib=bounds(idle.data,idle.info),anchor=feet(idle.data,idle.info,ib).center,frames=[];
 for(const e of chosen){const{data,info}=await pixels(e.source),b=bounds(data,info),f=feet(data,info,b),desired=Math.round(anchor-f.center),dx=info.width<=1304?Math.max(0,Math.min(1304-info.width,desired)):Math.max(8-b.left,Math.min(1295-b.right,desired)),dy=1328-b.bottom,out=Buffer.alloc(1304*1450*4);check(b.left>0&&b.top>0&&b.right<info.width-1&&b.bottom<info.height-1,e.id+': source clipped');check(b.left+dx>=0&&b.right+dx<1304,e.id+': native visible width exceeds canvas');let noise=0;
  for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++){const i=(y*info.width+x)*4,xx=x+dx,yy=y+dy;if(xx<0||xx>=1304||yy<0||yy>=1450){check(data[i+3]<=1,e.id+': visible pixel outside canvas');if(data[i+3])noise++;continue;}data.copy(out,(yy*1304+xx)*4,i,i+4);}
  const prepared=`${root}/${key}/prepared/${e.id}.png`;fs.mkdirSync(path.dirname(prepared),{recursive:true});await sharp(out,{raw:{width:1304,height:1450,channels:4}}).png().toFile(prepared);check((await pixels(prepared)).data.equals(out),'RGBA mismatch');Object.assign(e,{prepared,preparedSha256:sha(prepared),offsetX:dx,offsetY:dy,rgbaWithinCanvasPreserved:true,croppedOutsideAlphaOnePixels:noise,status:'packed_for_review'});
  frames.push({id:e.id,prepared,sha256:e.preparedSha256,headTopY:b.top+dy,footMinX:f.left+dx,footMaxX:f.right+dx,originOffsetY:['jump','fall'].includes(e.id)?(b.top+dy-ib.top)/m.pixelsPerUnit:0});
 }
 write(file,m);write(`${root}/${key}/packed-manifest.json`,{schemaVersion:1,key,characterIndex:m.characterIndex,name:m.name,complete:true,frames});console.log(JSON.stringify({key,frames:frames.length,rgbaVerified:true}));
}
async function review(key){
 const folder=`${root}/${key}`,m=read(`${folder}/manifest.json`),p=read(`${folder}/packed-manifest.json`),entries=m.jobs.map(j=>m.entries.find(e=>e.id===j.id&&e.version===j.version));
 const target=`イラスト確認/${m.name}/追加7動作_確認中`;fs.mkdirSync(target,{recursive:true});
 fs.copyFileSync(`art_source/paper_roster_idle/prepared/${key}_idle.png`,`${target}/00_待機基準.png`);
 const files=[`art_source/paper_roster_idle/prepared/${key}_idle.png`];
 for(const f of p.frames){fs.copyFileSync(f.prepared,`${target}/${labels[f.id]}.png`);files.push(f.prepared);}
 const tiles=[];for(let i=0;i<files.length;i++)tiles.push({input:await sharp(files[i]).resize(326,363,{fit:'contain',background:'#28313d'}).flatten({background:'#28313d'}).png().toBuffer(),left:(i%4)*326,top:Math.floor(i/4)*363});
 await sharp({create:{width:1304,height:726,channels:3,background:'#28313d'}}).composite(tiles).png().toFile(`${folder}/overview.png`);
 write(`${folder}/part-review.json`,{schemaVersion:1,key,artApproved:false,selected:entries.map(e=>({id:e.id,version:e.version,source:e.source,sha256:e.sha256,native:e.native})),canvasPolicy:'原画RGBAの整数移動と透明余白のみ。個別拡縮0。頭髪と靴の投影差は原画で目視し、美術の最終承認と実装検証を区別。',anatomyReview:'肩・肘・手首のつながり、左右の手の親指と折った指、全身の外周白縁を確認。低姿勢を埋める拡大なし。',rgbaAudit:p.frames.map(f=>({id:f.id,preserved:entries.find(e=>e.id===f.id).rgbaWithinCanvasPreserved,sha256:f.sha256})),knownDifferences:[]});
 fs.writeFileSync(`${target}/必ず読んでください.txt`,'新しい7動作の確認中候補です。ゲーム採用とUnity合格はまだ別です。現在採用は「現在ゲームで採用中」の人物フォルダで確認できます。\n');
 console.log(JSON.stringify({key,review:target}));
}
async function adopt(key,tag){
 const folder=`${root}/${key}`,m=read(`${folder}/manifest.json`),p=read(`${folder}/packed-manifest.json`),target=`イラスト確認/現在ゲームで採用中/${m.name}`;
 check(tag,'Unity検証名が必要');for(const plat of ['EditMode','PlayMode']){const xml=fs.readFileSync(`TestResults/${tag}_${plat}.xml`,'utf8'),r=xml.match(/<test-run\b[^>]*>/)[0];check(/result="Passed"/.test(r)&&/failed="0"/.test(r),plat+' not passed');}
 fs.mkdirSync(target,{recursive:true});for(const f of p.frames){const runtime=`AniBato/Assets/_Project/Art/PaperAction/${key}_motion_${f.id}.png`;check(sha(runtime)===f.sha256,'Game image differs');fs.copyFileSync(f.prepared,`${target}/${labels[f.id]}.png`);}
 write(`${target}/追加7動作_採用一覧.json`,{key,verifiedTag:tag,frames:p.frames});
 for(const job of m.jobs){const e=m.entries.find(e=>e.id===job.id&&e.version===job.version);e.status='adopted_for_runtime';e.gameConnected=true;}
 m.integrated=true;m.verifiedTag=tag;write(`${folder}/manifest.json`,m);
 write(`${folder}/current-adoption.json`,{key,verifiedTag:tag,artApproved:false,frames:p.frames});
 const proof=`docs/screenshots/${tag}/${'paper_motion_'+key}`,captured=`TestResults/${tag}_screenshots/${'paper_motion_'+key}`;
 if(fs.existsSync(captured)){fs.mkdirSync(proof,{recursive:true});for(const name of fs.readdirSync(captured))fs.copyFileSync(`${captured}/${name}`,`${proof}/${name}`);}
 const progress=read(`${root}/progress.json`),row=progress.characters.find(c=>c.key===key);
 Object.assign(row,{generated:7,integrated:true,verification:tag});progress.currentCharacter=progress.characters.find(c=>!c.integrated)?.key??null;
 progress.status=progress.currentCharacter?'in_progress':'native_complete_web_pending';write(`${root}/progress.json`,progress);
 fs.writeFileSync(`イラスト確認/${m.name}/追加7動作_確認中/必ず読んでください.txt`,'この7動作はUnity検証後にゲームへ接続済みです。現在の採用画像は「現在ゲームで採用中/'+m.name+'」にも同じSHAで保存しています。原画の最終美術承認とは別です。\n');
 console.log(JSON.stringify({key,adopted:p.frames.length,verifiedTag:tag}));
}
const[command,key,id,arg]=process.argv.slice(2);({init,save,revise,pack,review,adopt}[command]||(()=>{throw Error('init/save/revise/pack/review/adopt')}))(key,id,arg).catch(e=>{console.error(e.stack);process.exitCode=1;});
