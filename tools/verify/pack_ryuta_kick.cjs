// Native RGBA packing by integer translation. Never resize or redraw the generated artwork.
const fs=require('fs'),path=require('path'),crypto=require('crypto'),sharp=require('sharp');
const base='art_source/paper_roster_action/ryuta/',read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex'),write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
(async()=>{
 const manifest=read(base+'manifest.json'),packed=read(base+'packed-manifest.json'),adopted=[];
 for(const phase of ['prepare','impact']){
  const id='normal3_'+phase,source='art_source/ryuta_kick/'+phase+'_v1.png',promptFile='art_source/ryuta_kick/'+phase+'_prompt.txt';
  const job=manifest.jobs.find(j=>j.id===id),old=manifest.entries.find(e=>e.id===id&&e.version===job.version);
  if(old.source===source){adopted.push(old);continue;}
  const {data,info}=await sharp(source).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  let left=info.width,top=info.height,right=-1,bottom=-1;
  for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){left=Math.min(left,x);top=Math.min(top,y);right=Math.max(right,x);bottom=Math.max(bottom,y);}
  if(left<=0||top<=0||right>=info.width-1||bottom>=info.height-1)throw Error('Kick illustration touches edge');
  const dx=Math.floor((1304-info.width)/2),dy=1328-bottom;
  if(dx<0||dy<0||dx+info.width>1304||dy+info.height>1450)throw Error('Native canvas needs adjustment; resampling forbidden');
  const bytes=Buffer.alloc(1304*1450*4);
  for(let y=0;y<info.height;y++)data.copy(bytes,((y+dy)*1304+dx)*4,y*info.width*4,(y+1)*info.width*4);
  const prepared='art_source/ryuta_kick/'+phase+'_packed.png';await sharp(bytes,{raw:{width:1304,height:1450,channels:4}}).png().toFile(prepared);
  const out=await sharp(prepared).ensureAlpha().raw().toBuffer();
  for(let y=0;y<info.height;y++)if(!data.subarray(y*info.width*4,(y+1)*info.width*4).equals(out.subarray(((y+dy)*1304+dx)*4,((y+dy)*1304+dx+info.width)*4)))throw Error('Source RGBA changed');
  let footMinX=info.width,footMaxX=0;
  for(let y=bottom-Math.floor((bottom-top)*.15);y<=bottom;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){footMinX=Math.min(footMinX,x);footMaxX=Math.max(footMaxX,x);}
  const entry={...old,id,phase,version:Math.max(...manifest.entries.filter(e=>e.id===id).map(e=>e.version))+1,source,sha256:sha(source),promptFile,
   prompt:fs.readFileSync(promptFile,'utf8'),tool:'built-in image_gen',prepared,preparedSha256:sha(prepared),offsetX:dx,offsetY:dy,
   native:{width:info.width,height:info.height,bounds:{left,top,right,bottom}},croppedOutsideAlphaOnePixels:0,
   sourceRgbaPreserved:true,noResampling:true,gameConnected:false,artApproved:false,status:'kick_waiting_for_unity_verification'};
  old.status='retained_before_kick_replacement';old.gameConnected=false;manifest.entries.push(entry);job.version=entry.version;job.promptFile=promptFile;
  Object.assign(packed.frames.find(f=>f.id===id),{prepared,sha256:sha(prepared),headTopY:top+dy,footMinX:footMinX+dx,footMaxX:footMaxX+dx,originOffsetY:0});adopted.push(entry);
 }
 write(base+'manifest.json',manifest);write(base+'packed-manifest.json',packed);
 write('art_source/ryuta_kick/adoption.json',{entries:adopted,gameConnected:false,artApproved:false,noResampling:true,notes:'Third normal attack becomes a right front kick, with a chambered preparation. Stages 1/2 unchanged.'});
 console.log(adopted.map(e=>({id:e.id,version:e.version,native:e.native,offsetX:e.offsetX,offsetY:e.offsetY})));
})().catch(e=>{console.error(e);process.exitCode=1;});
