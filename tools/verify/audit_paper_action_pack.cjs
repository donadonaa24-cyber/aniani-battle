const fs=require('fs'),crypto=require('crypto'),sharp=require('sharp');
const key=process.argv[2];if(!/^[a-z]+$/.test(key||''))throw Error('Character key required');
const root='art_source/paper_roster_action/'+key+'/',read=p=>JSON.parse(fs.readFileSync(p)),sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const assert=(b,n)=>{if(!b)throw Error(n);};
(async()=>{
 const m=read(root+'manifest.json'),p=read(root+'packed-manifest.json'),rows=[];
 for(const f of p.frames){
  const job=m.jobs.find(j=>j.id===f.id),e=m.entries.find(e=>e.id===f.id&&e.version===job.version);assert(e,'Selected source missing');
  assert(sha(e.source)===e.sha256&&sha(f.prepared)===f.sha256,'SHA mismatch');
  const a=await sharp(e.source).ensureAlpha().raw().toBuffer({resolveWithObject:true}),b=await sharp(f.prepared).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  assert(b.info.width===1304&&b.info.height===1450,'Native frame differs');let noise=0,lowest=-1;
  for(let y=0;y<a.info.height;y++)for(let x=0;x<a.info.width;x++){
   const at=(y*a.info.width+x)*4,xx=x+e.offsetX,yy=y+e.offsetY;
   if(xx<0||xx>=1304||yy<0||yy>=1450){assert(a.data[at+3]<=1,'Visible source cropped');if(a.data[at+3]===1)noise++;continue;}
   const bt=(yy*1304+xx)*4;assert(a.data.subarray(at,at+4).equals(b.data.subarray(bt,bt+4)),'Source RGBA altered');
  }
  for(let y=0;y<1450;y++)for(let x=0;x<1304;x++){
   const alpha=b.data[(y*1304+x)*4+3];if(alpha>=32)lowest=y;
   if(x-e.offsetX<0||x-e.offsetX>=a.info.width||y-e.offsetY<0||y-e.offsetY>=a.info.height)assert(alpha===0,'Nontransparent padding');
  }
  assert(lowest===1328&&noise===e.croppedOutsideAlphaOnePixels,'Support anchor or noise accounting differs');
  rows.push({id:f.id,rgbaWithinCanvasPreserved:true,croppedOutsideAlphaOnePixels:noise,visibleSupportY:lowest,sha256:f.sha256});
 }
 assert(!p.complete||p.frames.length===m.jobs.length,'Incomplete batch marked complete');
 fs.writeFileSync(root+'pixel-audit.json',JSON.stringify({key,bodyFrames:rows,resampledCharacterPixels:0,complete:p.complete},null,2)+'\n');
 console.log(key+': '+rows.length+' packed poses audited, source pixels preserved; complete='+p.complete);
})().catch(e=>{console.error(e.message);process.exitCode=1;});
