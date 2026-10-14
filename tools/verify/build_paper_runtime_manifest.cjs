// Measure and select existing artwork. No resampling, repainting, or PNG writes.
const fs=require('node:fs'),sharp=require('sharp'),crypto=require('node:crypto');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const states={idle:[0,1],run:[2],jump:[3],fall:[4],crouch:[5,24,34],attack1:[6],attack2:[7],attack3:[8],air_neutral:[9,27],special_neutral:[10],special_side:[11],special_up:[12],special_down:[13],ultimate:[14],guard:[15],dodge:[16],counter:[17],counter_success:[18],hit_light:[19,20],ko:[21,23],win:[22],crouch_attack:[25],back_attack:[26],air_forward:[28],air_back:[29],air_up:[30],air_down:[31],air_forward_up:[32],air_forward_down:[33],hit_heavy:[]};
async function measure(source){
 const {data,info}=await sharp(source).ensureAlpha().raw().toBuffer({resolveWithObject:true});const w=info.width,h=info.height;
 const shoes=[];let headTop=h;
 for(let y=0;y<h;y++)for(let x=0;x<w;x++){
  const i=(y*w+x)*4,r=data[i],g=data[i+1],b=data[i+2];if(data[i+3]<128)continue;
  if(y>h*.55&&Math.min(r,g,b)>150&&Math.max(r,g,b)-Math.min(r,g,b)<35&&b>=r-8&&g>=r-8)shoes.push([x,y]);
  if(y<h*.55&&x>w*.24&&x<w*.78&&r>55&&r<205&&r>g*1.25&&g>b*1.05)headTop=Math.min(headTop,y);
 }
 if(shoes.length<1000||headTop===h)throw Error('Landmarks missing: '+source);
 // Exclude isolated light fringe: require a row with substantial sneaker paint.
 const rows=new Map();for(const[x,y]of shoes)rows.set(y,(rows.get(y)||0)+1);
 const solePaint=Math.max(...[...rows].filter(([,n])=>n>=16).map(([y])=>y));
 const bottom=shoes.filter(([,y])=>y>=solePaint-16);
 const left=Math.max(0,Math.min(...bottom.map(p=>p[0]))-18),right=Math.min(w,Math.max(...bottom.map(p=>p[0]))+19);
 let footY=solePaint+1;
 for(let y=solePaint;y<Math.min(h,solePaint+24);y++)for(let x=left;x<right;x++)if(data[(y*w+x)*4+3]>=32)footY=Math.max(footY,y+1);
 return {headTopY:Math.max(0,headTop-8),footY,footMinX:Math.max(0,Math.min(...bottom.map(p=>p[0]))-10),footMaxX:Math.min(w,Math.max(...bottom.map(p=>p[0]))+11),width:w,height:h};
}
async function main(){
 const catalog=read('art_source/paper_cutout/review-catalog.json');const poses=[],effects=[];
 for(const row of catalog.entries){if(row.key==='walk')continue;
  const sha256=crypto.createHash('sha256').update(fs.readFileSync(row.source)).digest('hex');
  if(row.key.startsWith('effect_')){const key=row.key.slice(7);effects.push({key,source:row.source,sha256,worldWidth:{bag_throw:.65,rice_ball:.42,mogu:.55,curry_wave:2.4}[key]});continue;}
  if(!states[row.key])throw Error('Unknown pose '+row.key);
  const markers=await measure(row.source);
  // Visual landmarks: raised brown bag / ladle edge must not be called the head.
  if(row.key==='special_up')markers.headTopY=169;
  if(row.key==='air_up')markers.headTopY=253;
  poses.push({key:row.key,source:row.source,sha256,states:states[row.key],airborne:/^(jump|fall|air_|special_up)/.test(row.key),...markers,artReview:row.artReview||(row.category==='repair'?'known_art_issue':row.key==='idle'?'previously_adopted':'provisional_user_requested_integration'),artNote:row.artNote||row.note});
 }
 const manifest={schemaVersion:1,canvasWidth:1304,canvasHeight:1450,note:'ユーザー指定で現参照を先行接続。絵の未修正点は残す。共通PPU、透明余白の梱包のみ、人物の拡縮なし。足元は靴の明色から計測し道具と区別。',poses,effects};
 fs.writeFileSync('art_source/paper_cutout/runtime-manifest.json',JSON.stringify(manifest,null,2)+'\n');
 process.stdout.write(JSON.stringify({poses:poses.length,effects:effects.length,states:new Set(poses.flatMap(p=>p.states)).size,landmarks:poses.map(({key,headTopY,footY,footMinX,footMaxX})=>({key,headTopY,footY,footMinX,footMaxX}))})+'\n');
}main().catch(e=>{process.stderr.write(e.stack+'\n');process.exitCode=1;});
