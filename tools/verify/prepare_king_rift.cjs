const fs=require('fs'),crypto=require('crypto'),sharp=require('sharp');
(async()=>{
 const source='art_source/king_march/dimensional_rift_v1.png',bytes=fs.readFileSync(source);
 const metadata=await sharp(bytes).metadata(),stats=await sharp(bytes).stats();
 if(!metadata.hasAlpha||stats.isOpaque)throw Error('Rift must have transparent alpha.');
 const adoption={source,prompt:'art_source/king_march/dimensional_rift_v1_prompt.txt',generator:'built-in image_gen',
  sha256:crypto.createHash('sha256').update(bytes).digest('hex'),width:metadata.width,height:metadata.height,
  alphaMin:stats.channels[3].min,alphaMax:stats.channels[3].max,sourcePixelsUnmodified:true,
  runtime:'AniBato/Assets/_Project/Art/PaperAction/king_dimensional_rift.png'};
 fs.writeFileSync('art_source/king_march/adoption.json',JSON.stringify(adoption,null,2)+'\n');console.log(adoption);
})();
