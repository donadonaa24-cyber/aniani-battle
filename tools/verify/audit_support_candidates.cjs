// Candidate bookkeeping only. Never connects unreviewed artwork to the game.
const fs=require('fs'),path=require('path'),crypto=require('crypto'),sharp=require('sharp');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const names={toma:'透真',ren:'蓮',chizuru:'千鶴',minato:'湊',shiro:'シロ',yui:'結衣',king:'黒獅子王',akatsuki:'暁',ryuta:'龍太',alma:'ALMA',shion_car:'志遠と赤い車',saki_hana:'咲と花_もやしカレー',sara:'紗良_救急箱',mai:'舞依_弓道着',tsuyoshi:'剛_水着と財布',mio:'美桜_バリア',hota:'歩太_変身前',pickup_book:'拾う本',pickup_silver:'拾う銀',pickup_pawn:'拾う歩',wallet:'回復財布'};
(async()=>{
 const report={status:'artwork_connection_status_per_entry',generator:'built-in image_gen',groups:[]};
 for(const group of ['paper_ledge','paper_support_items']){
  const file='art_source/'+group+'/progress.json',v=read(file),entries=v.characters||v.entries;
  const rows=[];
  for(const entry of entries){
   const m=await sharp(entry.source).metadata(),s=await sharp(entry.source).stats();
   if(!m.hasAlpha||s.channels[3].min!==0)throw Error('True transparent PNG required: '+entry.source);
   const row={key:entry.key,source:entry.source,sha256:sha(entry.source),width:m.width,height:m.height,alphaMin:s.channels[3].min,alphaMax:s.channels[3].max,gameConnected:entry.gameConnected===true,artApproved:entry.artApproved===true};
   Object.assign(entry,row);
   const review=entry.reviewFile||'イラスト確認/追加予定_未実装/'+(group==='paper_ledge'?'崖つかまり':'アイテムサポート')+'/'+(names[entry.key]||({mai_arrow:'舞依の矢',mio_barrier:'美桜の紙羽根バリア'}[entry.key]))+'.png';
   fs.mkdirSync(path.dirname(review),{recursive:true});fs.copyFileSync(entry.source,review);entry.reviewFile=review;rows.push(row);
  }
  fs.writeFileSync(file,JSON.stringify(v,null,2)+'\n');report.groups.push({group,count:rows.length,entries:rows});
 }
 fs.writeFileSync('art_source/support_and_ledge_candidate_audit.json',JSON.stringify(report,null,2)+'\n');
 console.log(JSON.stringify(report.groups.map(g=>({group:g.group,count:g.count,trueAlpha:true,connected:g.entries.filter(e=>e.gameConnected).length}))));
})();
