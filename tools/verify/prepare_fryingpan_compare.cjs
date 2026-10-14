// Technical transparent padding and contact sheet only; no creative pixel edits or body rescaling.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),sharp=require('sharp');
const root=process.cwd(),base='art_source/paper_cutout/variants/fryingpan',out='イラスト確認/千鶴/比較_フライパン版_未採用';
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
const width=1304,height=1450,footY=1328;
const titles=['1段目_1予備','1段目_2当てる','2段目_1予備','2段目_2当てる','3段目_1最大の溜め','3段目_2最大の一撃'];
async function main(){
 fs.mkdirSync(base+'/prepared',{recursive:true});fs.mkdirSync(out,{recursive:true});
 const entries=[],thumbs=[],cellW=330,cellH=410;
 for(const [row,style] of ['horizontal','vertical'].entries())for(let index=0;index<6;index++){
  const key=style+'_'+(Math.floor(index/2)+1)+'_'+(index%2?'impact':'anticipation');
  const version=style==='horizontal'&&[0,3,5].includes(index)?2:1;
  const source=base+'/generated/chizuru_pan_'+key+'_v'+version+'.png';
  const original=fs.readFileSync(source),{data,info}=await sharp(original).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  let minX=info.width,minY=info.height,maxX=-1,maxY=-1;
  for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){minX=Math.min(minX,x);minY=Math.min(minY,y);maxX=Math.max(maxX,x);maxY=Math.max(maxY,y);}
  const dx=Math.floor((width-info.width)/2),dy=footY-maxY;
  if(dx<0||dy<0||dy+info.height>height)throw Error(key+' cannot be padded without cropping; retain source and review');
  const padded=Buffer.alloc(width*height*4);
  for(let y=0;y<info.height;y++)data.copy(padded,((y+dy)*width+dx)*4,y*info.width*4,(y+1)*info.width*4);
  const prepared=base+'/prepared/'+key+'.png';await sharp(padded,{raw:{width,height,channels:4}}).png().toFile(prepared);
  const title=(style==='horizontal'?'横振り_':'上下振り_')+titles[index];fs.copyFileSync(prepared,out+'/'+title+'.png');
  const decoded=await sharp(prepared).ensureAlpha().raw().toBuffer();
  for(let y=0;y<info.height;y++)if(!decoded.subarray(((y+dy)*width+dx)*4,((y+dy)*width+dx+info.width)*4).equals(data.subarray(y*info.width*4,(y+1)*info.width*4)))throw Error('pixel copy mismatch');
  entries.push({key,title,source,sourceSha256:hash(original),prompt:source.replace('.png','.prompt.json'),nativeWidth:info.width,nativeHeight:info.height,prepared,width,height,footY,offsetX:dx,offsetY:dy,sourcePixelsPreserved:true,preparedSha256:hash(fs.readFileSync(prepared)),gameConnected:false,approved:false,artReview:'頭・靴・腕と握りの部位審査待ち。キャンバス一致だけで絵の完成とは扱わない。'});
  thumbs.push({input:await sharp(prepared).resize(320,356).png().toBuffer(),left:index*cellW+5,top:row*cellH+48});
 }
 const text=entries.map((e,i)=>`<text x="${i%6*cellW+cellW/2}" y="${Math.floor(i/6)*cellH+28}" text-anchor="middle">${e.title}</text>`).join('');
 const labels=Buffer.from(`<svg width="1980" height="820"><style>text{font-family:'Yu Gothic',sans-serif;font-size:18px;fill:#f4efdf}</style>${text}</svg>`);
 await sharp({create:{width:1980,height:820,channels:4,background:'#292e3b'}}).composite([...thumbs,{input:labels,left:0,top:0}]).png().toFile(out+'/00_横振りと上下振り_比較一覧.png');
 fs.writeFileSync(base+'/comparison-manifest.json',JSON.stringify({schemaVersion:1,status:'comparison_only_not_adopted',canvas:{width,height,footY,pixelsPerUnit:532.5893},noResampling:true,entries},null,2)+'\n');
 fs.writeFileSync(out+'/最初に読んでください.txt','全12枚はフライパン攻撃の比較用で、ゲーム未採用です。横振り／上下振りは各3段、各段1予備→2当てる。全画像1304×1450、接地靴底1328、原画の画素を拡縮せず透明余白だけ追加しています。頭や靴・手の絵そのものの寸法と左右は最終審査待ちです。カバン版は削除指示まで予備に保持。現在ゲームで使う絵は01_ゲームで採用済み/で確認できます。\n');
 const bag=read('art_source/paper_cutout/variants/bag-backup.json');
 for(const e of bag.entries)if(hash(fs.readFileSync(e.source))!==e.sha256||hash(fs.readFileSync(e.copy))!==e.sha256)throw Error('protected bag changed');
 process.stdout.write(JSON.stringify({comparisonImages:entries.length,canvas:[width,height],footY,pixelsPreserved:true,protectedBag:bag.entries.length})+'\n');
}main().catch(e=>{process.stderr.write(e.stack+'\n');process.exitCode=1});
