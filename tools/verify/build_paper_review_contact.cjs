// Inspection sheet only. The named game/source PNG copies remain unchanged.
const fs=require('node:fs'),path=require('node:path'),sharp=require('sharp');
const catalog=JSON.parse(fs.readFileSync('art_source/paper_cutout/review-catalog.json','utf8').replace(/^\uFEFF/,''));
const folder=catalog.outputFolder;
const summary=catalog.runtimeIntegration&&catalog.runtimeIntegration.verified?'全'+catalog.entries.length+'画像をゲームへ接続済み。手や部位の絵の修正残は画像一覧の説明を参照。':'採用と未検証を各画像の表示で確認してください。';
const categories={adopted:'採用済み',candidate:'確認用候補・未採用',repair:'修正が必要・未採用'};
const colors={adopted:'#24653a',candidate:'#294e94',repair:'#983f25'};
const order={adopted:0,candidate:1,repair:2};
const rows=catalog.entries.slice().sort((a,b)=>order[a.category]-order[b.category]);
const esc=s=>String(s).replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
(async()=>{
 const columns=5,w=290,h=340,top=100,width=columns*w,height=top+Math.ceil(rows.length/columns)*h;
 const overlays=[],text=[];
 for(let i=0;i<rows.length;i++){
  const r=rows[i],x=i%columns*w,y=top+Math.floor(i/columns)*h,version=(r.source.match(/_v(\d+)\.png$/)||[])[1]||r.version||1;
  const png=await sharp(r.source).resize(258,265,{fit:'contain',background:{r:0,g:0,b:0,alpha:0}}).png().toBuffer();
  overlays.push({input:png,left:x+16,top:y+56});
  text.push('<rect x="'+(x+5)+'" y="'+(y+5)+'" width="280" height="329" rx="8" fill="#ffffff"/><rect x="'+(x+5)+'" y="'+(y+52)+'" width="280" height="277" fill="#353b48"/><text x="'+(x+14)+'" y="'+(y+27)+'" font-size="16" font-weight="700">'+esc(r.title)+'</text><text x="'+(x+14)+'" y="'+(y+46)+'" font-size="13" fill="'+colors[r.category]+'">'+categories[r.category]+(r.category==='adopted'?'':'　候補'+String(version).padStart(2,'0'))+'</text>');
 }
 const svg='<svg xmlns="http://www.w3.org/2000/svg" width="'+width+'" height="'+height+'"><rect width="100%" height="100%" fill="#efede4"/><g font-family="Yu Gothic UI,Meiryo,sans-serif" fill="#242424"><text x="22" y="37" font-size="26" font-weight="700">千鶴の現在の確認画像</text><text x="22" y="68" font-size="17">'+summary+'</text>'+text.join('')+'</g></svg>';
 await sharp(Buffer.from(svg)).composite(overlays).png().toFile(path.join(folder,'現在の画像一覧.png'));
 process.stdout.write(JSON.stringify({sheet:path.join(folder,'現在の画像一覧.png'),currentImages:rows.length})+'\n');
})().catch(e=>{process.stderr.write(e.message+'\n');process.exitCode=1;});
