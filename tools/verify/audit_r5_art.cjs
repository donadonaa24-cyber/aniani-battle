const fs=require('fs'),path=require('path'),crypto=require('crypto');
const sharp=require('sharp');
const root=path.resolve(__dirname,'../..');
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(path.join(root,p))).digest('hex');
(async()=>{
 const entries=[];
 for(const key of ['sf','shogi','cook','ensemble']){
  const source=`art_source/title_paper/${key}_v1.png`,prompt=`art_source/title_paper/${key}_v1.prompt.json`;
  const m=await sharp(path.join(root,source)).metadata();
  const data=JSON.parse(fs.readFileSync(path.join(root,prompt),'utf8'));
  entries.push({key,source,promptFile:prompt,sha256:sha(source),native:{width:m.width,height:m.height},references:data.references.map(p=>({path:p,sha256:sha(p)})),selected:key==='ensemble'});
 }
 fs.writeFileSync(path.join(root,'art_source/title_paper/manifest.json'),JSON.stringify({schemaVersion:1,tool:'builtin image_gen',entries,notes:'9人の待機→3作品別集合→9人集合。人物の正史の追加なし。背景のAI窓と小物は本作独自表現。原画は無加工で保持。'},null,2)+'\n');
 const stage=JSON.parse(fs.readFileSync(path.join(root,'art_source/paper_stages/runtime-manifest.json')));
 for(const e of stage.entries)if(sha(e.source)!==e.sha256)throw Error('SHA mismatch '+e.id);
 console.log(`stage layers ${stage.entries.length}/21, ensemble source chain ${entries.length}/4: OK`);
})().catch(e=>{console.error(e.message);process.exitCode=1});
