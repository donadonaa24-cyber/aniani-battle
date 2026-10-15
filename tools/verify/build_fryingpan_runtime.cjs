// Select existing two-key-pose originals, preserve the protected bag sources, and measure layout.
const fs=require('node:fs'),crypto=require('node:crypto'),sharp=require('sharp');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const combinations={attack1:['horizontal_1'],attack2:['horizontal_2'],attack3:['horizontal_3'],crouch_attack:['horizontal_2','horizontal_3'],back_attack:['horizontal_2'],air_neutral:['horizontal_1'],air_forward:['horizontal_1'],air_back:['horizontal_2'],air_up:['horizontal_2'],air_down:['horizontal_3'],air_forward_up:['horizontal_2'],air_forward_down:['horizontal_3']};
async function markers(source){
 const {data,info}=await sharp(source).ensureAlpha().raw().toBuffer({resolveWithObject:true}),w=info.width,h=info.height,shoes=[];let head=h;
 for(let y=0;y<h;y++)for(let x=0;x<w;x++){
  const i=(y*w+x)*4,r=data[i],g=data[i+1],b=data[i+2];if(data[i+3]<128)continue;
  if(y>h*.55&&Math.min(r,g,b)>150&&Math.max(r,g,b)-Math.min(r,g,b)<35&&b>=r-8&&g>=r-8)shoes.push([x,y]);
  if(y<h*.55&&x>w*.24&&x<w*.78&&r>55&&r<205&&r>g*1.25&&g>b*1.05)head=Math.min(head,y);
 }
 if(shoes.length<1000||head===h)throw Error('Missing head or shoes: '+source);
 const counts=new Map();for(const[,y]of shoes)counts.set(y,(counts.get(y)||0)+1);
 const sole=Math.max(...[...counts].filter(([,n])=>n>=16).map(([y])=>y)),bottom=shoes.filter(([,y])=>y>=sole-16);
 const left=Math.max(0,Math.min(...bottom.map(p=>p[0]))-18),right=Math.min(w,Math.max(...bottom.map(p=>p[0]))+19);let footY=sole+1;
 for(let y=sole;y<Math.min(h,sole+24);y++)for(let x=left;x<right;x++)if(data[(y*w+x)*4+3]>=32)footY=Math.max(footY,y+1);
 return {headTopY:Math.max(0,head-8),footY,footMinX:Math.max(0,Math.min(...bottom.map(p=>p[0]))-10),footMaxX:Math.min(w,Math.max(...bottom.map(p=>p[0]))+11),width:w,height:h};
}
async function main(){
 const path='art_source/paper_cutout/runtime-manifest.json',manifest=read(path),comparison=read('art_source/paper_cutout/variants/fryingpan/comparison-manifest.json');
 const selected=new Map(comparison.entries.map(e=>[e.key,e])),cache=new Map();
 for(const pose of manifest.poses){
  const pair=combinations[pose.key];if(!pair)continue;
  pose.frames=[];
  for(const [i,phase]of ['anticipation','impact'].entries()){
   const key=(pair[i]||pair[0])+'_'+phase,entry=selected.get(key);if(!entry)throw Error('Missing '+key);
   if(!cache.has(key))cache.set(key,{key,source:entry.prepared,sha256:hash(entry.prepared),...await markers(entry.prepared)});
   pose.frames.push({...cache.get(key)});
  }
  Object.assign(pose,pose.frames[0]);pose.key=Object.keys(combinations).find(k=>combinations[k]===pair);
  pose.reverseFacing=pose.key==='back_attack'||pose.key==='air_back';
  pose.tiltDegrees=({air_up:60,air_down:-60,air_forward_up:30,air_forward_down:-30})[pose.key]||0;
  pose.artReview='integrated_art_review_pending';pose.artNote='左手フライパンの2原画。頭・靴・手の絵の最終審査は残る。';
 }
 manifest.schemaVersion=2;manifest.normalWeapon='frying_pan';manifest.note='ユーザー指定で全通常攻撃を左手フライパンへ。2原画を発生前／有効以後へ固定。カバン原画と旧manifestはvariants/で保護。';
 fs.writeFileSync(path,JSON.stringify(manifest,null,2)+'\n');
 const catPath='art_source/paper_cutout/review-catalog.json',catalog=read(catPath),rows=[];
 for(const row of catalog.entries){
  if(row.key==='effect_bag_throw'||row.phase==='impact')continue;
  const pose=manifest.poses.find(p=>p.key===row.key);
  if(!pose||!pose.frames){rows.push(row);continue;}
  const title=row.title.replace('・カバン投げ','').replace(/_フライパン_.*$/,'');
  pose.frames.forEach((frame,i)=>rows.push({...row,key:row.key+(i?'_impact':''),source:frame.source,title:title+'_フライパン_'+(i?'当てる':'予備'),phase:i?'impact':'anticipation',version:1,category:'candidate',gameConnected:false,artReview:pose.artReview,artNote:pose.artNote,note:'フライパンへ切替中。実Unity検証前。'}));
 }
 catalog.entries=rows;catalog.runtimeIntegration={verified:false,previousTag:'paper_theatre_2'};catalog.updated='2026-10-15';
 catalog.titles.attack3='通常攻撃3段目・フライパン大振り';fs.writeFileSync(catPath,JSON.stringify(catalog,null,2)+'\n');
 const factsPath='art_source/paper_cutout/source-index.json',facts=read(factsPath);
 for(const frame of cache.values())if(!facts.images.some(f=>f.file===frame.source))facts.images.push({file:frame.source,sha256:frame.sha256,width:frame.width,height:frame.height,alpha:true,kind:'framed_fryingpan',sourcePixelsPreserved:true});
 fs.writeFileSync(factsPath,JSON.stringify(facts,null,2)+'\n');
 const bag=read('art_source/paper_cutout/variants/bag-backup.json');for(const e of bag.entries)if(hash(e.source)!==e.sha256||hash(e.copy)!==e.sha256)throw Error('Protected bag changed');
 process.stdout.write(JSON.stringify({twoFrameActions:manifest.poses.filter(p=>p.frames).length,selectedOriginals:cache.size,catalogImages:rows.length,protectedBag:bag.entries.length})+'\n');
}
module.exports={markers};
if(require.main===module)main().catch(e=>{process.stderr.write(e.stack+'\n');process.exitCode=1});
