// Copy original pixels into transparent common canvases; thumbnails are review-only.
const fs=require('node:fs'),crypto=require('node:crypto'),sharp=require('sharp');
const base='art_source/paper_roster_idle',folder='イラスト確認/全9人_待機',W=1304,H=1450;
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
async function main(){
 const plan=read(base+'/generation-plan.json'),byKey=new Map(plan.characters.map(r=>[r.key,r]));
 const integrated=fs.existsSync(base+'/integration.json')&&read(base+'/integration.json').verified;
 const keys=['toma','minato','shiro','ren','yui','king','chizuru','akatsuki','ryuta'];
 fs.mkdirSync(base+'/prepared',{recursive:true});fs.mkdirSync(folder,{recursive:true});
 const entries=[],pictures=[],captions=[];
 for(const [i,key]of keys.entries()){
  const existing=key==='chizuru',row=existing?{name:'千鶴',work:'Battle à la carte'}:byKey.get(key);
  const version=row.selectedVersion||1;
  const source=existing?'AniBato/Assets/_Project/Art/Paper/pose_idle_paper_chizuru.png':base+'/generated/'+key+'_idle_v'+version+'.png';
  const {data,info}=await sharp(source).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  let minX=info.width,minY=info.height,maxX=-1,maxY=-1;
  for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){minX=Math.min(minX,x);minY=Math.min(minY,y);maxX=Math.max(maxX,x);maxY=Math.max(maxY,y);}
  const dx=Math.floor((W-info.width)/2),dy=existing?0:1328-maxY;
  if(dx<0||dy<0||dy+info.height>H)throw Error(key+' needs canvas review; do not rescale the original');
  const padded=Buffer.alloc(W*H*4);
  for(let y=0;y<info.height;y++)data.copy(padded,((y+dy)*W+dx)*4,y*info.width*4,(y+1)*info.width*4);
  const prepared=base+'/prepared/'+key+'_idle.png';await sharp(padded,{raw:{width:W,height:H,channels:4}}).png().toFile(prepared);
  const decoded=await sharp(prepared).ensureAlpha().raw().toBuffer();
  for(let y=0;y<info.height;y++)if(!decoded.subarray(((y+dy)*W+dx)*4,((y+dy)*W+dx+info.width)*4).equals(data.subarray(y*info.width*4,(y+1)*info.width*4)))throw Error('pixel mismatch '+key);
  const file=folder+'/'+String(i+1).padStart(2,'0')+'_'+row.name+'_待機_'+(existing?'ゲーム使用中':'確認用')+'.png';fs.copyFileSync(prepared,file);
  entries.push({key,name:row.name,work:row.work,version,source,sourceSha256:sha(source),prepared,preparedSha256:sha(prepared),nativeWidth:info.width,nativeHeight:info.height,width:W,height:H,offsetX:dx,offsetY:dy,opaqueBounds:{left:minX+dx,top:minY+dy,right:maxX+dx,bottom:maxY+dy},sourcePixelsPreserved:true,gameConnected:existing||integrated,selectionConnected:!!integrated,battleWholeSprite:existing||!!integrated&&!['toma','ren'].includes(key),artApproved:existing,reviewNote:existing?'現在ゲームで使う基準。':(integrated?'タイトル／選択で使用。追加6人は共通技・1枚絵の試作。透真／蓮の戦闘素材は旧版。':'待機原画の確認用。ゲーム未接続。')+'手／服／身長と頭・靴の最終審査待ち。'+(key==='toma'&&version>1?'湊に合わせて頭身から描き直した現行版。':'')+(key==='minato'?'下半身は資料にないため本作用の仮補完。':'')});
  const x=i%3*450,y=Math.floor(i/3)*530;
  pictures.push({input:await sharp(prepared).resize(430,478).png().toBuffer(),left:x+10,top:y+46});
  captions.push(`<text x="${x+225}" y="${y+29}" text-anchor="middle">${row.name}｜${existing?'ゲーム使用中':integrated?['toma','ren'].includes(key)?'選択で使用':'試作で使用':'確認用・未接続'}</text>`);
 }
 await sharp({create:{width:1350,height:1590,channels:4,background:'#292e3b'}}).composite([...pictures,{input:Buffer.from(`<svg width="1350" height="1590"><style>text{font-family:'Yu Gothic',sans-serif;font-size:22px;fill:#f4efdf}</style>${captions.join('')}</svg>`),left:0,top:0}]).png().toFile(folder+'/00_全9人_待機一覧.png');
 fs.writeFileSync(base+'/manifest.json',JSON.stringify({schemaVersion:1,stage:'全員の待機原画が揃った確認段階',newIdleImages:8,existingIdleImages:1,canvas:{width:W,height:H},noSourceResampling:true,entries},null,2)+'\n');
 fs.writeFileSync(folder+'/最初に読んでください.txt','全9人の待機絵です。千鶴だけ現在ゲーム使用中、残り8人は新規の確認用で未接続です。全画像1304×1450、原画画素を拡縮せず透明余白で揃えています。実ゲーム用の身長差・頭／靴の部位寸法と左右の手は最終審査待ちです。湊の資料にない下半身は本作用の仮補完。ガード／しゃがみは全員待機を揃える区切りの後に進めます。\n');
 if(integrated)fs.writeFileSync(folder+'/最初に読んでください.txt','9人ともタイトル／選択で使用中。追加6人は各作品の共通技と待機1枚絵を使う試作で対戦できます。透真／蓮の戦闘動作素材は旧版、千鶴は紙の差分です。1304×1450、原画画素を保持。絵の最終審査と固有技／ポーズは後続。湊の下半身は仮補完。\n');
 process.stdout.write(JSON.stringify({newImages:8,totalIdleImages:entries.length,canvas:[W,H],allSourcePixelsPreserved:true,folder})+'\n');
}main().catch(e=>{process.stderr.write(e.stack+'\n');process.exitCode=1});
