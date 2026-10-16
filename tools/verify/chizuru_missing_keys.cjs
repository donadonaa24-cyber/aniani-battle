// Supplement Chizuru's existing paper profile; keep its adopted normal/rice poses intact.
const fs=require('fs'),path=require('path'),crypto=require('crypto'),sharp=require('sharp');
const root='art_source/paper_roster_action/chizuru/',read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const write=(p,v)=>{fs.mkdirSync(path.dirname(p),{recursive:true});fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');};
const assert=(b,n)=>{if(!b)throw Error(n);},sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const idle='art_source/paper_cutout/generated/chizuru_paper_idle_v1.png',pan='art_source/paper_cutout/variants/fryingpan/prepared/horizontal_1_impact.png';
const actionRows={
 down_prepare:['地上下攻撃','down','prepare','両足を地面につけ、膝と腰を低く落とす。本人の左肘を曲げ、小型のフライパンを腰より低い身体の横へ引く。これから相手の足元の低さへ振り出す予備。右手は空の軽い握りで胸元、指を大きく開かない。'],
 down_impact:['地上下攻撃','down','impact','両足を地面につけた低い構えのまま、本人左手の小型フライパンを画面右前の膝の高さへ横に打ち出す決め姿勢。上半身を少し前へ入れ、腰と肩も入れて打つ。空いた右手は胸元の軽い握り。道具を投げない。'],
 air_prepare:['空中ニュートラル','air','prepare','ジャンプ中で両脚とも地面から離れ、両膝を曲げて身体へ引き寄せる。本人左手の小型フライパンを肩の横へ引き、肘を曲げて空中の一撃を溜める。右手は空の軽い握りでバランス。両靴が独立して見え、立ち姿勢にならない。'],
 air_impact:['空中ニュートラル','air','impact','ジャンプ中で両脚とも地面から離れ、後ろ脚を膝で曲げ、前の膝も軽く曲げる。本人左手の小型フライパンを画面右前へ振り抜く空中打撃。胸元側から両腕と両靴のつながりを見せる。右手は空の軽い握りで後方へ引く。立った姿勢や地面の足跡にしない。'],
 super_impact:['超必殺カレーぶちまけ','super','impact','画像2のカレー皿を持つ絵の続き。左手の同じ白いカレー皿を画面右前へ大きく突き出し、皿を前に傾けて中のカレーをぶちまけた瞬間の全身の決めポーズ。皿はほぼ空で、カレーの茶色の小さい残りだけ。飛ぶカレーの波は別素材なので描かない。右手は同じ銀スプーンを握って後ろへ引く。両膝を曲げて前足へ体重を乗せ、顔は元気でコミカル。満杯のカレーを手元に残さない。']};
function init(){
 const file=root+'manifest.json';if(fs.existsSync(file)){console.log(file);return;}
 const m={schemaVersion:1,key:'chizuru',name:'千鶴',characterIndex:2,canvas:{width:1304,height:1450},sole:1328,pixelsPerUnit:532.5893,baseIdle:idle,jobs:[],entries:[],integrated:false,scope:'five_missing_keys_only'};
 for(const [id,[label,pose,phase,action]] of Object.entries(actionRows)){
  const references=[idle,pose==='super'?'art_source/paper_cutout/generated/chizuru_paper_ultimate_v1.png':pan];
  const prompt=`Use case: identity-preserve. 全身のゲーム原画を1枚。画像1は千鶴の採用待機、容姿・アニメ塗り・頭身・部位の実寸の基準。画像2は${pose==='super'?'同じ人物のカレー皿とスプーン、連続する構え':'同じ人物の左手のフライパンと握り'}の参考。茶色ボブ髪、茶色の瞳、オレンジ長袖セーター、紺プリーツスカート、白スニーカーを維持。\n動作：${label}の${phase==='prepare'?'予備、まだ当てない':'当てる決め姿'}。${action}\n胸元側が見える斜め横、画面右を向く。腕は左右1本ずつ、脚2本、手は各5指。フライパンは本人左手（胸側を見たこの構図では画面右側につながる肩）、空手は本人右手。肩→肘→手首→掌と親指を自然につなぐ。両手を右手にしない。武器は同じ丸い灰色の小型パンと短い黒い柄、2本に増やさない。\n頭髪・靴・手の大きさを画像1から変えない。髪の横幅約460px、靴は前後の投影で約170〜215pxが基準。低い姿勢をキャンバスいっぱいへ拡大しない。出力キャンバスは画像1と同じ1246×1263、余白を残して全身・手・道具・髪・靴まで切らずに収める。普通のアニメの線とセル塗り、薄い硬い白い切り抜き縁を外周全てに。縁の外は完全透明。背景・床・影・相手・文字・光彩・速度線・衝撃・紙の繊維・関節分割・3D・ピクセルなし。1人1姿勢だけ。`;
  const j={id,key:'chizuru',character:'千鶴',pose,phase,label,version:1,source:root+'generated/'+id+'_v1.png',references,referenceRoles:['採用待機：人物と部位実寸','道具と握り／連続する構え'],prompt,tool:'built-in image_gen',status:'planned'},promptFile=root+'prompts/'+id+'_v1.json';write(promptFile,j);m.jobs.push({id,version:1,promptFile});
 }write(file,m);console.log(JSON.stringify({planned:5,existingNormalAndRice:'preserved'}));
}
async function apply(){
 const file=root+'packed-manifest.json',missing=root+'missing-packed-manifest.json';
 if(fs.existsSync(file)){assert(!fs.existsSync(missing),'Do not overwrite previous missing pack');fs.renameSync(file,missing);}
 const m=read(root+'manifest.json'),p=read(missing);assert(p.complete&&p.frames.length===5,'Five missing keys must be packed');
 const runtime='art_source/paper_cutout/runtime-manifest.json',r=read(runtime),before=JSON.stringify(r.poses.filter(p=>['attack1','attack2','attack3','special_neutral'].includes(p.key)));
 async function frame(source,id){const {data,info}=await sharp(source).ensureAlpha().raw().toBuffer({resolveWithObject:true});let left=info.width,right=-1,top=info.height,bottom=-1;
  for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){left=Math.min(left,x);right=Math.max(right,x);top=Math.min(top,y);bottom=Math.max(bottom,y);}
  let fl=info.width,fr=-1;for(let y=bottom-48;y<=bottom;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){fl=Math.min(fl,x);fr=Math.max(fr,x);}
  return {key:id,source,sha256:sha(source),headTopY:top,footY:bottom+1,footMinX:fl,footMaxX:fr+1,width:info.width,height:info.height};}
 const selected=id=>p.frames.find(f=>f.id===id).prepared;
 const up=[await frame('art_source/paper_cutout/variants/fryingpan/prepared/vertical_2_anticipation.png','up_prepare'),await frame('art_source/paper_cutout/variants/fryingpan/prepared/vertical_2_impact.png','up_impact')];
 const sets={air_up:up,crouch_attack:[await frame(selected('down_prepare'),'down_prepare'),await frame(selected('down_impact'),'down_impact')],air_neutral:[await frame(selected('air_prepare'),'air_prepare'),await frame(selected('air_impact'),'air_impact')]};
 const ultimate=r.poses.find(p=>p.key==='ultimate');sets.ultimate=[{key:'super_prepare',source:ultimate.source,sha256:ultimate.sha256,headTopY:ultimate.headTopY,footY:ultimate.footY,footMinX:ultimate.footMinX,footMaxX:ultimate.footMaxX},await frame(selected('super_impact'),'super_impact')];
 for(const [key,frames]of Object.entries(sets)){const pose=r.poses.find(p=>p.key===key);Object.assign(pose,{source:frames[0].source,sha256:frames[0].sha256,frames,airborne:key==='air_neutral',tiltDegrees:0,headTopY:frames[0].headTopY,footY:frames[0].footY,footMinX:frames[0].footMinX,footMaxX:frames[0].footMaxX,artReview:'integration_verification_pending',artNote:'不足する2コマを補完。性能不変、美術最終承認は別。'});}
 assert(before===JSON.stringify(r.poses.filter(p=>['attack1','attack2','attack3','special_neutral'].includes(p.key))),'Normal/rice data must stay unchanged');
 write(runtime,r);write(root+'runtime-supplement.json',{keys:Object.keys(sets),frameCount:8,newGeneratedFrames:5,reusedFrames:3,normalAndRiceUnchanged:true,gameConnected:'verification_pending'});console.log(JSON.stringify({modifiedKeys:Object.keys(sets),normalAndRiceUnchanged:true}));
}
Promise.resolve().then(()=>({init,apply}[process.argv[2]]||(()=>{throw Error('init/apply required')}))()).catch(e=>{console.error(e.stack);process.exitCode=1});
