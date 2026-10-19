// Integer translation and transparent packing only; never resample or redraw source artwork.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),sharp=require('sharp');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
async function main(){
 const root='art_source/paper_roster_action/akatsuki/',m=read(root+'manifest.json'),packed=read(root+'packed-manifest.json');
 const adopted=[];
 for(const phase of ['prepare','impact']){
  const id='normal1_'+phase,version=phase==='prepare'?2:1;
  const source=`art_source/directional_specials/akatsuki_slash_${phase}_v${version}.png`;
  const prompt=`art_source/directional_specials/prompts/akatsuki_slash_${phase}_v${version}.txt`;
  const job=m.jobs.find(j=>j.id===id),old=m.entries.find(e=>e.id===id&&e.version===job.version);
  if(old.source===source&&old.sha256===sha(source)){adopted.push(old);continue;}
  const {data,info}=await sharp(source).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  let left=info.width,top=info.height,right=-1,bottom=-1;
  for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){left=Math.min(left,x);right=Math.max(right,x);top=Math.min(top,y);bottom=Math.max(bottom,y);}
  if(left<=0||top<=0||right>=info.width-1||bottom>=info.height-1)throw Error('Visible source touches an edge: '+source);
  const dx=Math.floor((1304-info.width)/2),dy=1328-bottom;
  if(dx<0||dy<0||info.width+dx>1304||info.height+dy>1450)throw Error('Source needs a larger native canvas; resizing is forbidden');
  const bytes=Buffer.alloc(1304*1450*4);
  for(let y=0;y<info.height;y++)data.copy(bytes,((y+dy)*1304+dx)*4,y*info.width*4,(y+1)*info.width*4);
  const prepared=`art_source/directional_specials/prepared/akatsuki_slash_${phase}.png`;
  fs.mkdirSync(path.dirname(prepared),{recursive:true});await sharp(bytes,{raw:{width:1304,height:1450,channels:4}}).png().toFile(prepared);
  const check=await sharp(prepared).ensureAlpha().raw().toBuffer();
  for(let y=0;y<info.height;y++)if(!data.subarray(y*info.width*4,(y+1)*info.width*4).equals(check.subarray(((y+dy)*1304+dx)*4,((y+dy)*1304+dx+info.width)*4)))throw Error('RGBA preservation failed');
  let footMinX=info.width,footMaxX=-1;
  for(let y=bottom-Math.floor((bottom-top)*.15);y<=bottom;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){footMinX=Math.min(footMinX,x);footMaxX=Math.max(footMaxX,x);}
  const entry={...old,id,phase,version:Math.max(...m.entries.filter(e=>e.id===id).map(e=>e.version))+1,source,sha256:sha(source),
    promptFile:prompt,prompt:fs.readFileSync(prompt,'utf8'),tool:'built-in imagegen',
    prepared,preparedSha256:sha(prepared),native:{width:info.width,height:info.height,bounds:{left,top,right,bottom}},
    offsetX:dx,offsetY:dy,sourceRgbaPreserved:true,noResampling:true,status:'packed_for_refinement_verification',gameConnected:false,artApproved:false};
  old.status='retained_before_card_slash_refinement';old.gameConnected=false;m.entries.push(entry);job.version=entry.version;job.promptFile=prompt;
  Object.assign(packed.frames.find(f=>f.id===id),{prepared,sha256:sha(prepared),headTopY:(phase==='prepare'?65:54)+dy,
    footMinX:footMinX+dx,footMaxX:footMaxX+dx,originOffsetY:0});adopted.push(entry);
 }
 write(root+'manifest.json',m);write(root+'packed-manifest.json',packed);
 write('art_source/directional_specials/akatsuki-slash-adoption.json',{schemaVersion:1,gameConnected:false,artApproved:false,
   notes:'Left-hand overhead preparation into downward-forward slash. Same two keys shared by normal stages 1/2; stage 3 unchanged. Rejected first candidate used the wrong arm.',entries:adopted});
 process.stdout.write(JSON.stringify(adopted.map(e=>({id:e.id,version:e.version,source:e.source,sha256:e.preparedSha256})))+'\n');
}
main().catch(error=>{process.stderr.write(error.stack+'\n');process.exitCode=1;});
