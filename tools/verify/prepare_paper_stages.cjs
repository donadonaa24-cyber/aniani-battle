// Select source rectangles only: no generated PNG pixels are changed or resampled.
const fs=require('fs'),sharp=require('sharp'),crypto=require('crypto');
const root='art_source/paper_stages',m=JSON.parse(fs.readFileSync(root+'/manifest.json','utf8').replace(/^\uFEFF/,''));
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
(async()=>{
 const entries=[];
 for(const j of m.jobs){const job=JSON.parse(fs.readFileSync(j.promptFile,'utf8').replace(/^\uFEFF/,'')),e=m.entries.find(e=>e.id===j.id&&e.version===job.version);
  if(!e||hash(e.source)!==e.sha256)throw Error(j.id+' source missing/hash');
  const b=e.visible;let rect={x:b.left,y:e.native.height-b.bottom-1,width:b.right-b.left+1,height:b.bottom-b.top+1};
  if(e.layer==='far'||e.layer==='mid'||e.layer==='frame')rect={x:0,y:0,width:e.native.width,height:e.native.height};
  if(e.layer==='main'||e.layer==='upper'){
   const {data,info}=await sharp(e.source).ensureAlpha().raw().toBuffer({resolveWithObject:true});
   const starts={future_main:420,board_main:398,festival_main:357,future_upper:535,board_upper:450,festival_upper:460};
   let found=null;
   for(let y=starts[e.id];y<b.bottom-2&&!found;y++){
    let start=0,bestStart=0,best=0;
    for(let x=0;x<=info.width;x++)if(x===info.width||data[(y*info.width+x)*4+3]<230){if(x-start>best){best=x-start;bestStart=start;}start=x+1;}
    if(best>=(b.right-b.left+1)*.94)found={x:bestStart,y:e.native.height-b.bottom-1,width:best,height:b.bottom-y+1};
   }
   if(!found)throw Error(e.id+' horizontal deck not found');rect=found;
  }
  entries.push({id:e.id,key:e.key,layer:e.layer,label:e.label,source:e.source,sha256:e.sha256,native:e.native,rect,version:e.version});
 }
 const output={schemaVersion:1,entries,boardPanel:{x:818,y:237,width:310,height:264},notes:'原画はそのまま保持。足場は不透明な水平天面から下のSprite領域を採用し、上面の不要な背景は描画しない。盤面は生成された無地の木目へ正確な6×6の線を重ねる。'};
 fs.writeFileSync(root+'/runtime-manifest.json',JSON.stringify(output,null,2)+'\n');console.log(JSON.stringify({selected:entries.length,surfaces:entries.filter(e=>['main','upper'].includes(e.layer)).map(e=>({id:e.id,rect:e.rect}))}));
})().catch(e=>{console.error(e.stack);process.exitCode=1});
