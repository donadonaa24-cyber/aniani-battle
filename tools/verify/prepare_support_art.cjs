// Read-only image measurements. Unity copies the original PNG bytes without resizing.
const fs=require('fs'),path=require('path'),crypto=require('crypto'),sharp=require('sharp');
const root=path.resolve(__dirname,'../..');
const read=p=>JSON.parse(fs.readFileSync(path.join(root,p),'utf8').replace(/^\uFEFF/,''));
const heights={alma:3.2,shion_car:2.2,saki_hana:2.55,sara:2.6,mai:2.4,tsuyoshi:2.6,mio:2.45,hota:2.35};
(async()=>{
 const entries=[];
 for(const row of read('art_source/paper_support_items/progress.json').entries){
  const bytes=fs.readFileSync(path.join(root,row.source)),{data,info}=await sharp(bytes).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  let left=info.width,top=info.height,right=-1,bottom=-1;
  for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){left=Math.min(left,x);right=Math.max(right,x);top=Math.min(top,y);bottom=Math.max(bottom,y);}
  if(right<left)throw Error('No visible artwork: '+row.key);
  entries.push({key:row.key,source:row.source,sha256:crypto.createHash('sha256').update(bytes).digest('hex'),
   width:info.width,height:info.height,left,top,right,bottom,worldHeight:heights[row.key]||.7,nativePixelsPreserved:true});
 }
 fs.writeFileSync(path.join(root,'art_source/paper_support_items/adoption.json'),JSON.stringify({schemaVersion:1,complete:true,entries},null,2)+'\n');
 console.log('14 support originals measured; no image pixels changed.');
})().catch(e=>{console.error(e);process.exitCode=1;});
