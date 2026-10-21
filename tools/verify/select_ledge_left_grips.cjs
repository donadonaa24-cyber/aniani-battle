// Select the user's final A pattern; this is not an automatic anatomy classifier.
const fs=require('fs'),path=require('path'),crypto=require('crypto'),sharp=require('sharp');
const root=path.resolve(__dirname,'../..'),file=path.join(root,'art_source/paper_ledge/progress.json');
(async()=>{
 const progress=JSON.parse(fs.readFileSync(file,'utf8').replace(/^\uFEFF/,''));
 for(const entry of progress.characters){
  entry.source=`art_source/paper_ledge/${entry.key}/hang_user_A_v1.png`;
  const source=path.join(root,entry.source),meta=await sharp(source).metadata();
  entry.sha256=crypto.createHash('sha256').update(fs.readFileSync(source)).digest('hex');
  entry.width=meta.width;entry.height=meta.height;entry.grippingHand='left';
  entry.status='user_pattern_A_verification_pending';entry.gameConnected=false;entry.artApproved=false;
  entry.handPattern='A';entry.thumbSide='image-left';entry.userShapeConfirmed=true;
  entry.prompt=entry.source.replace(/\.png$/,'_prompt.txt');entry.generator='built-in image_gen';
  entry.agentVisualReview='Matches the user-selected A palm grip: thumb at native image-left; four curled fingers visible.';
  entry.userFeedback='Final explicit answer A (image-left thumb) supersedes the earlier B answer. Full artwork approval remains separate.';
 }
 progress.status='user_pattern_A_verification_pending';progress.handPattern='A';progress.thumbSide='image-left';progress.userShapeConfirmed=true;delete progress.nativeTag;delete progress.webTag;
 progress.latestUserFeedback='Final answer after the repeated question: A, thumb at image-left. No further reinterpretation.';
 fs.writeFileSync(file,JSON.stringify(progress,null,2)+'\n');
 console.log('Nine user-selected A candidates fixed (image-left thumb); native and Web verification pending.');
})().catch(e=>{console.error(e);process.exitCode=1;});
