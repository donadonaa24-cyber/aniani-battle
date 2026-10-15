const fs=require('node:fs'),crypto=require('node:crypto'),sharp=require('sharp');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
(async()=>{
 const m=read('art_source/paper_cutout/runtime-manifest.json'),cat=read('art_source/paper_cutout/review-catalog.json');
 for(const row of [...m.poses.flatMap(p=>p.frames||[p]),...m.effects])if(sha(row.source)!==row.sha256.toLowerCase())throw Error('Source differs from verified manifest: '+row.source);
 for(const row of cat.entries)if(!fs.existsSync(row.prompt))throw Error('Missing full prompt: '+row.prompt);
 for(const row of read('art_source/paper_cutout/variants/bag-backup.json').entries)if(sha(row.source)!==row.sha256||sha(row.copy)!==row.sha256)throw Error('Protected bag differs');
 const source='AniBato/Assets/_Project/Art/Paper/effect_rice_ball_chizuru.png';
 const {data,info}=await sharp(source).ensureAlpha().raw().toBuffer({resolveWithObject:true});let min=info.width,max=-1;
 for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){min=Math.min(min,x);max=Math.max(max,x);}
 const meta=fs.readFileSync(source+'.meta','utf8'),ppu=Number(meta.match(/spritePixelsToUnits:\s*([\d.]+)/)[1]);
 const size=read('art_source/paper_cutout/rice-size.json'),actual=(max-min+1)/ppu*size.projectileScale;
 if(Math.abs(actual-size.heldWorldWidth)>.00001)throw Error('Rice world sizes differ');
 process.stdout.write(JSON.stringify({verifiedManifestSources:true,fullPromptLinks:cat.entries.length,protectedBag:13,riceVisibleWorldWidth:actual,heldWorldWidth:size.heldWorldWidth})+'\n');
})().catch(e=>{process.stderr.write(e.stack+'\n');process.exitCode=1});
