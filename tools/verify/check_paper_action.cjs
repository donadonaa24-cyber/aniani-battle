// Independent source/padding audit; the contact sheet is a review thumbnail, never a runtime asset.
const fs = require('fs'), crypto = require('crypto'), sharp = require('sharp');
const root = 'art_source/paper_roster_action/toma/';
const read = p => JSON.parse(fs.readFileSync(p, 'utf8'));
const sha = p => crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const assert = (ok, label) => { if (!ok) throw Error(label); };
const pixels = p => sharp(p).ensureAlpha().raw().toBuffer({resolveWithObject:true});
(async()=>{
 const m=read(root+'manifest.json'),packed=read(root+'packed-manifest.json'),effects=read(root+'effects-manifest.json');
 assert(packed.complete&&packed.frames.length===16,'16 frames required');
 const report=[];
 for(const job of m.jobs){
  const entry=m.entries.find(e=>e.id===job.id&&e.version===job.version),f=packed.frames.find(f=>f.id===job.id);
  assert(entry&&f,job.id); assert(sha(entry.source)===entry.sha256,job.id+' source hash');
  assert(sha(f.prepared)===f.sha256,job.id+' packed hash');
  const s=await pixels(entry.source),p=await pixels(f.prepared);assert(p.info.width===1304&&p.info.height===1450,job.id+' canvas');
  let outsideOne=0,lowest=-1;
  for(let y=0;y<s.info.height;y++)for(let x=0;x<s.info.width;x++){
   const i=(y*s.info.width+x)*4,xx=x+entry.offsetX,yy=y+entry.offsetY;
   if(xx<0||xx>=1304||yy<0||yy>=1450){assert(s.data[i+3]<=1,job.id+' visible crop');if(s.data[i+3]===1)outsideOne++;continue;}
   const pi=(yy*1304+xx)*4;assert(s.data.subarray(i,i+4).equals(p.data.subarray(pi,pi+4)),job.id+' changed RGBA');
  }
  for(let y=0;y<1450;y++)for(let x=0;x<1304;x++){
   const a=p.data[(y*1304+x)*4+3];if(a>=32)lowest=y;
   if(x-entry.offsetX<0||x-entry.offsetX>=s.info.width||y-entry.offsetY<0||y-entry.offsetY>=s.info.height)assert(a===0,job.id+' padding');
  }
  assert(lowest===1328,job.id+' sole');assert(outsideOne===entry.croppedOutsideAlphaOnePixels,job.id+' noise count');
  report.push({id:job.id,rgbaInsideCanvas:'unchanged',visibleSole:lowest,croppedOutsideAlphaOnePixels:outsideOne,sourceSha256:entry.sha256,packedSha256:f.sha256});
 }
 assert(effects.entries.length>=3,'3 effect sprites');
 for(const e of effects.entries)assert(sha(e.source)===e.sha256,e.id+' effect hash');
 const folder=m.integrated?'イラスト確認/透真/攻撃原画_ゲーム採用済み':'イラスト確認/透真/攻撃原画_確認中';fs.mkdirSync(folder,{recursive:true});
 const images=[{id:'idle',prepared:'art_source/paper_roster_idle/prepared/toma_idle.png'},...packed.frames];
 const composite=[];
 for(let i=0;i<images.length;i++){
  const x=i%4*326,y=Math.floor(i/4)*385;
  composite.push({input:await sharp(images[i].prepared).resize(326,363).toBuffer(),left:x,top:y});
  composite.push({input:Buffer.from(`<svg width="326" height="22"><text x="8" y="17" fill="white" font-size="15">${images[i].id}</text></svg>`),left:x,top:y+363});
 }
 await sharp({create:{width:1304,height:Math.ceil(images.length/4)*385,channels:4,background:'#354153'}}).composite(composite).png().toFile(folder+'/一覧_比較用.png');
 fs.writeFileSync(root+'pixel-audit.json',JSON.stringify({schemaVersion:1,bodyFrames:report,effects:effects.entries.map(e=>({id:e.id,sha256:e.sha256})),resampledCharacterPixels:0},null,2)+'\n');
 console.log(JSON.stringify({bodyFrames:report.length,effects:effects.entries.length,croppedOutsideAlphaOnePixels:report.reduce((n,r)=>n+r.croppedOutsideAlphaOnePixels,0),audit:root+'pixel-audit.json'}));
})().catch(e=>{console.error(e.stack);process.exitCode=1});
