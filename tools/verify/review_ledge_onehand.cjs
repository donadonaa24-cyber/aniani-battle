// Inspection-only contact sheet. Native source, prepared and game textures are never resized here.
const fs=require('fs'),path=require('path'),sharp=require('sharp');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'イラスト確認/ゲーム採用/崖つかまり_採用A');
const data=JSON.parse(fs.readFileSync(path.join(root,'art_source/paper_ledge/progress.json'),'utf8').replace(/^\uFEFF/,''));
const anchors=JSON.parse(fs.readFileSync(path.join(root,'art_source/paper_ledge/adoption.json'),'utf8'));
const names=['透真','蓮','千鶴','湊','シロ','結衣','黒獅子王','暁','龍太'];
(async()=>{
 fs.mkdirSync(out,{recursive:true});const images=[],labels=[],hands=[],handLabels=[];
 for(let i=0;i<9;i++){
  const entry=data.characters[i],x=(i%3)*380,y=Math.floor(i/3)*470;
  const preview=await sharp(path.join(root,entry.source)).resize(350,410,{fit:'contain',background:'#00000000'}).png().toBuffer();
  images.push({input:preview,left:x+15,top:y+45});
  labels.push(`<text x="${x+18}" y="${y+31}" fill="#fff" font-size="23">${names[i]}・指定A</text>`);
  const anchor=anchors.frames.find(f=>f.key===entry.key),meta=await sharp(path.join(root,entry.source)).metadata();
  const left=Math.max(0,Math.min(meta.width-230,anchor.handX-anchor.offsetX-170));
  const top=Math.max(0,Math.min(meta.height-230,anchor.handY-anchor.offsetY-110));
  const hand=await sharp(path.join(root,entry.source)).extract({left,top,width:230,height:230}).resize(460,460,{kernel:'nearest'}).png().toBuffer();
  hands.push({input:hand,left:(i%3)*480+10,top:Math.floor(i/3)*510+45});
  handLabels.push(`<text x="${(i%3)*480+15}" y="${Math.floor(i/3)*510+32}" fill="#fff" font-size="25">${names[i]}・指定A（親指左）</text>`);
 }
 const svg=`<svg xmlns="http://www.w3.org/2000/svg" width="1140" height="1410"><rect width="1140" height="1410" fill="#263849"/><g font-family="Meiryo,sans-serif">${labels.join('')}</g></svg>`;
 await sharp(Buffer.from(svg)).composite(images).png().toFile(path.join(out,'採用A_9人一覧.png'));
 const handSvg=`<svg xmlns="http://www.w3.org/2000/svg" width="1440" height="1530"><rect width="1440" height="1530" fill="#263849"/><g font-family="Meiryo,sans-serif">${handLabels.join('')}</g></svg>`;
 await sharp(Buffer.from(handSvg)).composite(hands).png().toFile(path.join(out,'採用A_手元拡大一覧.png'));
 console.log('Inspection-only sheet saved; all game images retain native RGBA pixels.');
})().catch(e=>{console.error(e);process.exitCode=1;});
