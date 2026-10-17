const fs=require('fs'),path=require('path'),crypto=require('crypto'),sharp=require('sharp');
const root=path.resolve(__dirname,'../..'),folder=path.join(root,'AniBato/Assets/_Project/Art');
function files(dir){return fs.readdirSync(dir,{withFileTypes:true}).flatMap(e=>e.isDirectory()?files(path.join(dir,e.name)):[path.join(dir,e.name)]);}
(async()=>{
 const entries=[];
 for(const file of files(folder).filter(p=>p.endsWith('.png'))){
  const meta=fs.readFileSync(file+'.meta','utf8'),guid=meta.match(/^guid: (\w+)/m)[1],bytes=fs.readFileSync(file);
  const mipmaps=['Paper','PaperAction','PaperCore','PaperRoster'].includes(path.relative(folder,file).split(path.sep)[0]);
  const sha=crypto.createHash('sha256').update(bytes).digest('hex'),packed=path.join(root,'AniBato/Assets/_WebGLPacked',guid+'_'+sha.slice(0,12).toUpperCase()+(mipmaps?'_mip1':'')+'.png');
  if(!fs.existsSync(packed))throw Error('Packed image missing: '+path.relative(root,file));
  const a=await sharp(bytes).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  const b=await sharp(packed).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  const padded=n=>mipmaps?2**Math.ceil(Math.log2(n)):Math.ceil(n/4)*4;
  if(b.info.width!==padded(a.info.width)||b.info.height!==padded(a.info.height))throw Error('Padding size differs');
  const dy=b.info.height-a.info.height;
  for(let y=0;y<a.info.height;y++)if(!a.data.subarray(y*a.info.width*4,(y+1)*a.info.width*4).equals(b.data.subarray(((y+dy)*b.info.width)*4,((y+dy)*b.info.width+a.info.width)*4)))throw Error('Source pixels changed: '+path.relative(root,file));
  for(let y=0;y<b.info.height;y++)for(let x=0;x<b.info.width;x++)if((y<dy||x>=a.info.width)&&b.data[(y*b.info.width+x)*4+3]!==0)throw Error('Nontransparent padding: '+path.relative(root,file));
  entries.push({source:path.relative(root,file).replaceAll('\\','/'),sha256:sha,width:a.info.width,height:a.info.height,packedWidth:b.info.width,packedHeight:b.info.height,mipmaps,rgbaPreserved:true});
 }
 const result={schemaVersion:1,entries,checked:entries.length,failed:0};
 fs.writeFileSync(path.join(root,'TestResults/webgl-pixel-audit.json'),JSON.stringify(result,null,2));
 console.log('RGBA pixel preservation: '+entries.length+'/'+entries.length+'; no resampling');
})().catch(e=>{console.error(e.message);process.exitCode=1;});
