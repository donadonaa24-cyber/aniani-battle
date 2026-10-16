// Keep rejected candidates and translate the selected generated key without resampling.
const fs=require('fs'),path=require('path'),crypto=require('crypto'),sharp=require('sharp');
const root='art_source/paper_roster_action/chizuru/normal3_hand_fix/',read=p=>JSON.parse(fs.readFileSync(p,'utf8'));
const write=(p,v)=>{fs.mkdirSync(path.dirname(p),{recursive:true});fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');},sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const assert=(b,n)=>{if(!b)throw Error(n);};
async function pixels(p){return sharp(p).ensureAlpha().raw().toBuffer({resolveWithObject:true});}
function bounds(data,info){let left=info.width,top=info.height,right=-1,bottom=-1;for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){left=Math.min(left,x);right=Math.max(right,x);top=Math.min(top,y);bottom=Math.max(bottom,y);}return {left,top,right,bottom};}
async function save(version,source){const j=read(root+`prompt_v${version}.json`),allowed=path.resolve(process.env.USERPROFILE,'.codex/generated_images')+path.sep;assert(path.resolve(source).startsWith(allowed),'Built-in output required');
 fs.mkdirSync(path.dirname(j.source),{recursive:true});if(fs.existsSync(j.source))assert(sha(j.source)===sha(source),'Do not overwrite raw');else fs.copyFileSync(source,j.source);
 const p=await pixels(j.source);write(root+`candidate_v${version}.json`,{...j,sha256:sha(j.source),native:{width:p.info.width,height:p.info.height,bounds:bounds(p.data,p.info)},artApproved:false,gameConnected:false});console.log(JSON.stringify({saved:j.source,native:p.info}));}
async function pack(version){const e=read(root+`candidate_v${version}.json`),a=await pixels(e.source),b=bounds(a.data,a.info),dx=57,dy=1328-b.bottom;
 assert(b.left>0&&b.right<a.info.width-1&&b.top>0&&b.bottom<a.info.height-1,'Visible edge cut');const packed=Buffer.alloc(1304*1450*4);let noise=0;
 for(let y=0;y<a.info.height;y++)for(let x=0;x<a.info.width;x++){const i=(y*a.info.width+x)*4,xx=x+dx,yy=y+dy;if(xx<0||xx>=1304||yy<0||yy>=1450){assert(a.data[i+3]<=1,'Visible source cropped');if(a.data[i+3]===1)noise++;continue;}a.data.copy(packed,(yy*1304+xx)*4,i,i+4);}
 const prepared=root+'prepared/horizontal_3_impact.png';fs.mkdirSync(path.dirname(prepared),{recursive:true});await sharp(packed,{raw:{width:1304,height:1450,channels:4}}).png().toFile(prepared);assert((await pixels(prepared)).data.equals(packed),'Packing changed pixels');
 let fl=1304,fr=-1;for(let y=1280;y<=1328;y++)for(let x=0;x<1304;x++)if(packed[(y*1304+x)*4+3]>=32){fl=Math.min(fl,x);fr=Math.max(fr,x);}
 const frame={key:'horizontal_3_impact',source:prepared,sha256:sha(prepared),headTopY:b.top+dy,footY:1329,footMinX:fl,footMaxX:fr+1,width:1304,height:1450};
 const file='art_source/paper_cutout/runtime-manifest.json',m=read(file),row=m.poses.find(p=>p.key==='attack3');row.frames[1]=frame;row.artNote='3段当てるの左手を1・2段採用の掌側の握りへ修正。大きな踏み込み、通常4枚と性能を維持。';write(file,m);
 write(root+'selected.json',{...e,prepared,preparedSha256:frame.sha256,offsetX:dx,offsetY:dy,rgbaWithinCanvasPreserved:true,croppedOutsideAlphaOnePixels:noise,resampledCharacterPixels:0,frame,gameConnected:'verification_pending'});console.log(JSON.stringify({selected:version,frame,noise}));}
const [command,version,source]=process.argv.slice(2);Promise.resolve().then(()=>({save,pack}[command]||(()=>{throw Error('save/pack required')}))(Number(version),source)).catch(e=>{console.error(e.stack);process.exitCode=1});
