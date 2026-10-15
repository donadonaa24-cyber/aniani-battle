const fs=require('node:fs'),crypto=require('node:crypto');
const {markers}=require('./build_fryingpan_runtime.cjs');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const save=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
(async()=>{
 const source='art_source/paper_cutout/generated/chizuru_paper_special_neutral_v3.png';
 const sha256=crypto.createHash('sha256').update(fs.readFileSync(source)).digest('hex'),layout=await markers(source);
 const p='art_source/paper_cutout/runtime-manifest.json',m=read(p),row=m.poses.find(p=>p.key==='special_neutral');
 Object.assign(row,{source,sha256,...layout,artReview:'integrated_art_review_pending',artNote:'手持ちおにぎりを拡大し、飛翔の紙版と同じ表示幅へ。人物画素は拡縮せず共通枠へ梱包。部位最終審査は残る。'});
 // Manually inspected bounding rectangle of the held rice ball, including its paper edge.
 const held={left:177,top:196,width:224,height:202};
 const world=held.width/532.5893;m.effects.find(p=>p.key==='rice_ball').worldWidth=world/.5;save(p,m);
 const cp='art_source/paper_cutout/review-catalog.json',cat=read(cp),entry=cat.entries.find(e=>e.key==='special_neutral');
 Object.assign(entry,{source,version:3,category:'candidate',gameConnected:false,artNote:row.artNote});save(cp,cat);
 const fp='art_source/paper_cutout/source-index.json',facts=read(fp);
 if(!facts.images.some(f=>f.file===source))facts.images.push({file:source,sha256,width:layout.width,height:layout.height,alpha:true});save(fp,facts);
 save('art_source/paper_cutout/rice-size.json',{heldSource:source,heldPixels:held,bodyPixelsPerUnit:532.5893,heldWorldWidth:world,projectileScale:.5,projectileWorldWidth:world,previousProjectileWorldWidth:.45,previousHeldApproximateWidth:.2,sourceBodyNotResampled:true,rejectedVersion:2,reason:'v2は1129×1393で体格変化。v3は1254×1254、元1246×1263に近い倍率と頭／靴を目視して共通枠へ透明梱包。'});
 process.stdout.write(JSON.stringify({source,layout,heldWorldWidth:world,projectileWorldWidth:world})+'\n');
})().catch(e=>{process.stderr.write(e.stack+'\n');process.exitCode=1});
