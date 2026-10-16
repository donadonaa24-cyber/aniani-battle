const fs=require('fs'),sharp=require('sharp'),crypto=require('crypto');
const read=p=>JSON.parse(fs.readFileSync(p)),write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n'),sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const root='art_source/paper_roster_action/toma/';
async function main(){const m=read(root+'manifest.json'),p=read(root+'packed-manifest.json'),selected=[];
for(const phase of ['prepare','impact']){
 const id='normal3_'+phase,j=read(root+'dual_gun/prompts/'+id+(phase==='impact'?'_v3':'_v1')+'.json');
 const job=m.jobs.find(x=>x.id===id),old=m.entries.find(e=>e.id===id&&e.version===job.version);
 if(old.source===j.source)continue;
 const {data,info}=await sharp(j.source).ensureAlpha().raw().toBuffer({resolveWithObject:true});let left=info.width,top=info.height,right=-1,bottom=-1;
 for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){left=Math.min(left,x);top=Math.min(top,y);right=Math.max(right,x);bottom=Math.max(bottom,y);}
 if(left<=0||top<=0||right>=info.width-1||bottom>=info.height-1)throw Error('Visible source edge clipped');
 const dx=old.offsetX,dy=1328-bottom,bytes=Buffer.alloc(1304*1450*4);let noise=0;
 for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++){
 const i=(y*info.width+x)*4,xx=x+dx,yy=y+dy;
 if(xx<0||xx>=1304||yy<0||yy>=1450){if(data[i+3]>1)throw Error('Visible source crop');if(data[i+3]===1)noise++;continue;}data.copy(bytes,(yy*1304+xx)*4,i,i+4);
 }
 let fmin=info.width,fmax=-1;for(let y=bottom-Math.floor((bottom-top+1)*.1);y<=bottom;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){fmin=Math.min(fmin,x);fmax=Math.max(fmax,x);}
 const prepared=root+'dual_gun/prepared/'+id+'.png';fs.mkdirSync(root+'dual_gun/prepared',{recursive:true});await sharp(bytes,{raw:{width:1304,height:1450,channels:4}}).png().toFile(prepared);
 old.gameConnected=false;old.status=old.source.includes('dual_gun')?'retained_previous_dual_left_hand_revision':'retained_previous_single_gun_normal3';
 const entry={...j,id,phase,pose:'normal3',label:'通常3段二丁拳銃',key:'toma',version:job.version+1,sha256:sha(j.source),native:{width:info.width,height:info.height,bounds:{left,top,right,bottom}},prepared,preparedSha256:sha(prepared),offsetX:dx,offsetY:dy,sourceRgbaPreserved:noise===0,rgbaWithinCanvasPreserved:true,croppedOutsideAlphaOnePixels:noise,status:'packed_for_review',artApproved:false,gameConnected:false};
 m.entries.push(entry);job.version=entry.version;job.promptFile=root+'dual_gun/prompts/'+id+(phase==='impact'?'_v3':'_v1')+'.json';
 Object.assign(p.frames.find(f=>f.id===id),{prepared,sha256:sha(prepared),headTopY:top+dy,footMinX:fmin+dx,footMaxX:fmax+dx});selected.push(entry);
}
write(root+'manifest.json',m);write(root+'packed-manifest.json',p);write(root+'dual_gun/manifest.json',{schemaVersion:1,key:'toma',bodyFrames:2,entries:['normal3_prepare','normal3_impact'].map(id=>{const j=m.jobs.find(j=>j.id===id);return m.entries.find(e=>e.id===id&&e.version===j.version);}),gameConnected:false,artApproved:false,verification:null,notes:'左右1丁ずつの二丁拳銃。通常1・2段は従来のピストルを共用。通常3段の2枚だけ差し替え、元単銃原画と梱包は保持。二重ダメージを与えない。上側左手は当てるv3で掌と握った指の内側、下側右手は従来の甲側。v2の上手の甲はユーザーが明示却下。最終美術承認は別。'});
console.log(selected.map(e=>({id:e.id,width:e.native.width,height:e.native.height,bounds:e.native.bounds,noise:e.croppedOutsideAlphaOnePixels})));
}main().catch(e=>{console.error(e);process.exitCode=1;});
