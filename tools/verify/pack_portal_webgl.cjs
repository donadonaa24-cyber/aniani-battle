// Usage: node tools/verify/pack_portal_webgl.cjs <portal-checkout>
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),portal=path.resolve(process.argv[2]||'TestResults/portal_release');
if(!portal.startsWith(root+path.sep))throw Error('release checkout must be inside workspace');
const source=path.join(root,'Build/WebGL'),dest=path.join(portal,'games/anibato');
fs.mkdirSync(dest,{recursive:true});
// Remove only this script's previous generated release, preserving unrelated portal content.
for(const name of ['Build','payload-manifest.json','payload-parts']){const target=path.resolve(dest,name);if(!target.startsWith(dest+path.sep))throw Error('unsafe release target');if(fs.existsSync(target))fs.rmSync(target,{recursive:true,force:true});}
const manifest={version:'',files:{}};
const sha=bytes=>crypto.createHash('sha256').update(bytes).digest('hex');
for(const name of fs.readdirSync(source)){
 if(name==='Build')continue;
 const item=path.join(source,name);if(fs.statSync(item).isDirectory())fs.cpSync(item,path.join(dest,name),{recursive:true});else fs.copyFileSync(item,path.join(dest,name));
}
fs.mkdirSync(path.join(dest,'Build'),{recursive:true});fs.mkdirSync(path.join(dest,'payload-parts'),{recursive:true});
for(const name of fs.readdirSync(path.join(source,'Build'))){
 const bytes=fs.readFileSync(path.join(source,'Build',name));
 if(bytes.length<50*1024*1024){fs.writeFileSync(path.join(dest,'Build',name),bytes);continue;}
 const entry={length:bytes.length,sha256:sha(bytes),parts:[]};
 for(let offset=0,index=0;offset<bytes.length;offset+=8*1024*1024,index++){
  const chunk=bytes.subarray(offset,Math.min(bytes.length,offset+8*1024*1024)),hash=sha(chunk),file=`payload-parts/${hash}.bin`;
  fs.writeFileSync(path.join(dest,file),chunk);entry.parts.push({file,length:chunk.length,sha256:hash});
 }
 manifest.files['Build/'+name]=entry;
 const restored=Buffer.concat(entry.parts.map(p=>fs.readFileSync(path.join(dest,p.file))));if(!restored.equals(bytes))throw Error('payload changed');
}
manifest.version=sha(Buffer.from(JSON.stringify(manifest.files))).slice(0,16);
fs.writeFileSync(path.join(dest,'payload-manifest.json'),JSON.stringify(manifest));
const index=path.join(dest,'index.html');fs.writeFileSync(index,fs.readFileSync(index,'utf8').replace('<script id="anibato-payload-config" type="application/json">null</script>',`<script id="anibato-payload-config" type="application/json">${JSON.stringify({version:manifest.version})}</script>`));
fs.writeFileSync(path.join(portal,'.nojekyll'),'');
fs.writeFileSync(path.join(root,'TestResults/portal_payload_audit.json'),JSON.stringify(manifest,null,2));
console.log(JSON.stringify({destination:'games/anibato',version:manifest.version,files:Object.keys(manifest.files).length,parts:Object.values(manifest.files).reduce((n,x)=>n+x.parts.length,0),originalBytesPreserved:true}));
