// Review-only layouts: game artwork is never resized in the runtime pipeline.
const fs=require('node:fs'),sharp=require('sharp');
const root='art_source/paper_roster_core',out='イラスト確認/全9人_基本5ポーズ';
const labels={guard:'ガード',crouch:'しゃがみ',counter:'カウンター',win:'勝利',lose:'敗北'};
const names={toma:'透真',ren:'蓮',chizuru:'千鶴',minato:'湊',yui:'結衣',akatsuki:'暁',shiro:'シロ',king:'黒獅子王',ryuta:'龍太'};
const order=['toma','ren','chizuru','minato','yui','akatsuki','shiro','king','ryuta'];
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
async function main(){fs.mkdirSync(out,{recursive:true});const entries=[];
 for(const [pose,label]of Object.entries(labels)){const pictures=[],texts=[];
  for(let i=0;i<order.length;i++){const key=order[i],x=i%3*440,y=Math.floor(i/3)*520+60;
   let image=null,status='未生成',tag=null;
   if(key==='chizuru'){image=`AniBato/Assets/_Project/Art/Paper/pose_${pose==='lose'?'ko':pose}_paper_chizuru.png`;status='既存採用';tag='paper_sideswing_3';}
   else{const p=`${root}/${key}/manifest.json`;if(fs.existsSync(p)){const m=read(p),j=m.jobs.find(j=>j.pose===pose),e=m.entries.find(e=>e.pose===pose&&e.version===j.version);if(e&&e.prepared&&fs.existsSync(e.prepared)){image=e.prepared;status=m.integrated?'ゲーム採用':'確認中';tag=m.verifiedTag;}}}
   entries.push({key,name:names[key],pose,label,image,status,verifiedTag:tag});
   if(image)pictures.push({input:await sharp(image).resize(430,478).png().toBuffer(),left:x+5,top:y+40});
   texts.push(`<text x="${x+220}" y="${y+28}" text-anchor="middle">${names[key]}｜${status}</text>`);
   if(!image)texts.push(`<text x="${x+220}" y="${y+280}" text-anchor="middle" class="pending">未生成</text>`);
  }
  const header=['星の終わり','推し駒Battle','Battle à la carte'].map((s,i)=>`<text x="${i*440+220}" y="40" text-anchor="middle">${s}</text>`).join('');
  await sharp({create:{width:1320,height:1620,channels:4,background:'#303748'}}).composite([...pictures,{input:Buffer.from(`<svg width="1320" height="1620"><style>text{font-family:'Yu Gothic',sans-serif;font-size:22px;fill:#fff4d9}.pending{fill:#9298a7;font-size:32px}</style>${header+texts.join('')}</svg>`),left:0,top:0}]).png().toFile(`${out}/${String(Object.keys(labels).indexOf(pose)+1).padStart(2,'0')}_${label}_全9人.png`);
 }
 fs.writeFileSync(root+'/review-index.json',JSON.stringify({canvas:{width:1304,height:1450},entries},null,2)+'\n');
 fs.writeFileSync(out+'/最初に読んでください.txt','5種類それぞれを全9人で並べています。各画像の「未生成」「確認中」「ゲーム採用」「既存採用」を見てください。千鶴は既存5原画を再利用し、敗北は従来の座り込みです。新8人は控えめな立ち姿の勝敗です。個別の等倍画像はイラスト確認/<名前>/基本5ポーズ_ゲーム採用済み/、検証前は基本5ポーズ_確認中/。この一覧は縮小表示の確認用で、ゲーム用原画は縮小していません。手・服・部位の最終承認は別です。\n');
 const generated=entries.filter(e=>e.image).length,verified=entries.filter(e=>e.verifiedTag).length;console.log(JSON.stringify({total:45,preparedOrExisting:generated,unityVerifiedOrExisting:verified,folder:out}));
}main().catch(e=>{console.error(e.stack);process.exitCode=1});
