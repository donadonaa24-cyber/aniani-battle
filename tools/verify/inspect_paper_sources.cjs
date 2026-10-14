// Record source facts without modifying generated artwork.
const fs = require('node:fs'), path = require('node:path'), crypto = require('node:crypto'), sharp = require('sharp');
async function main() {
  const folder = path.resolve('art_source/paper_cutout/generated'), files = fs.readdirSync(folder).filter(f => f.endsWith('.png')).sort();
  const images = [];
  for (const file of files) {
    const bytes = fs.readFileSync(path.join(folder, file));
    const { data, info } = await sharp(bytes).ensureAlpha().raw().toBuffer({ resolveWithObject:true });
    const { width, height } = info;
    let minX=width, minY=height, maxX=-1, maxY=-1, opaque=0, edge=0;
    for(let y=0;y<height;y++)for(let x=0;x<width;x++) if(data[(y*width+x)*4+3]>=32) {
      minX=Math.min(minX,x); minY=Math.min(minY,y); maxX=Math.max(maxX,x); maxY=Math.max(maxY,y); opaque++;
      if(x===0||y===0||x===width-1||y===height-1)edge++;
    }
    images.push({file:'art_source/paper_cutout/generated/'+file,width,height,sha256:crypto.createHash('sha256').update(bytes).digest('hex'),alpha32Bounds:[minX,minY,maxX+1,maxY+1],alpha32Pixels:opaque,edgePixels:edge,cornerAlpha:[data[3],data[(width-1)*4+3],data[((height-1)*width)*4+3],data[(width*height-1)*4+3]],promptSaved:fs.existsSync(path.join(folder,file.replace('.png','.prompt.json'))),approved:false});
  }
  fs.writeFileSync('art_source/paper_cutout/source-index.json', JSON.stringify({schemaVersion:1,note:'寸法/透明/原画ハッシュの記録。部位比較とUnity検証前の候補であり、承認済みと扱わない。',images},null,2)+'\n');
  process.stdout.write(JSON.stringify({原画件数:images.length,端の不透明画素がある原画:images.filter(x=>x.edgePixels>0).map(x=>x.file),全文プロンプトなし:images.filter(x=>!x.promptSaved).map(x=>x.file),共通寸法と異なる人物原画:images.filter(x=>!x.file.includes('_effect_')&&(x.width!==1246||x.height!==1263)).map(x=>({file:x.file,width:x.width,height:x.height}))})+'\n');
}
main().catch(error=>{process.stderr.write(error.stack+'\n');process.exitCode=1;});
