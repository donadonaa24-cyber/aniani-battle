const fs=require('fs'),path=require('path'),crypto=require('crypto'),sharp=require('sharp');
const root=path.resolve(__dirname,'../..'),tag=process.argv[2]||'paper_ledge_A';
const read=p=>JSON.parse(fs.readFileSync(path.join(root,p),'utf8').replace(/^\uFEFF/,''));
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(path.join(root,p))).digest('hex');
const native={};
for(const platform of ['EditMode','PlayMode']){
 const file=`TestResults/${tag}_${platform}.xml`,xml=fs.readFileSync(path.join(root,file),'utf8');
 const header=xml.match(/<test-run\b[^>]+>/)[0],attributes=Object.fromEntries([...header.matchAll(/([\w-]+)="([^"]*)"/g)].map(m=>[m[1],m[2]]));
 if(attributes.result!=='Passed'||+attributes.failed!==0||+attributes.skipped!==0||+attributes.passed<({EditMode:797,PlayMode:101}[platform]))throw Error('Native tests not all passed: '+platform);
 native[platform]={total:+attributes.total,passed:+attributes.passed,failed:0,skipped:0,results:file,sha256:sha(file)};
}
for(const [stage,suffix] of [['BuildAll','buildall'],['Windows','build']]){
 const file=`TestResults/${tag}_${suffix}.log`,log=fs.readFileSync(path.join(root,file),'utf8');
 if(!log.includes('Application will exit with return code 0'))throw Error(stage+' exit not confirmed');
 if(stage==='Windows'&&!log.includes('Build Finished, Result: Success'))throw Error('Windows build not successful');
 native[stage]={exitCode:0,log:file,sha256:sha(file)};
}
(async()=>{
 const manifest=read('art_source/paper_ledge/adoption.json'),progress=read('art_source/paper_ledge/progress.json');
 if(manifest.schemaVersion!==3||manifest.frames.length!==9||manifest.handPattern!=='A'||manifest.thumbSide!=='image-left'||!manifest.userShapeConfirmed)throw Error('Only the final user-selected A pattern is accepted');
 const images=[];
 for(const entry of progress.characters){
  const frame=manifest.frames.find(f=>f.key===entry.key),game=`AniBato/Assets/_Project/Art/PaperAction/${entry.key}_ledge_hang.png`;
  if(frame.grippingHands!==1||frame.grippingHand!=='left'||frame.pose!=='one_hand_hang'||!frame.source.includes('hang_user_A_'))throw Error('Final user-selected A frame required');
  if(sha(frame.source)!==frame.sourceSha256||sha(frame.prepared)!==frame.sha256||sha(game)!==frame.sha256)throw Error('Artwork SHA differs');
  const source=await sharp(path.join(root,frame.source)).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  const packed=await sharp(path.join(root,game)).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  for(let y=0;y<source.info.height;y++)if(!source.data.subarray(y*source.info.width*4,(y+1)*source.info.width*4).equals(packed.data.subarray(((y+frame.offsetY)*1304+frame.offsetX)*4,((y+frame.offsetY)*1304+frame.offsetX+source.info.width)*4)))throw Error('Native source pixels changed');
  entry.status='game_connected_user_A_verified';entry.gameConnected=true;entry.artApproved=false;
  entry.prompt=entry.source.replace(/\.png$/,'_prompt.txt');entry.generator='built-in image_gen';
  if(!fs.existsSync(path.join(root,entry.prompt)))throw Error('Full generation prompt missing');
  entry.userFeedback='Final explicit selection A: thumb at image-left. The earlier B answer was withdrawn. Full artwork approval remains separate.';
  entry.agentVisualReview='Manually checked against comparison A: thumb at image-left and four curled finger pads visible. Native game ledge captures checked. This records the user-selected shape, not an automatic anatomy classification.';
  fs.copyFileSync(path.join(root,entry.source),path.join(root,entry.reviewFile));
  images.push({key:entry.key,source:entry.source,sourceSha256:frame.sourceSha256,game,gameSha256:frame.sha256,
   grippingHands:1,grippingHand:'left',handX:frame.handX,handY:frame.handY,nativePixelsPreserved:true,ppu:532.5893,manualPalmReview:true});
 }
 progress.status='user_A_native_verified_web_pending';progress.nativeTag=tag;
 progress.nativePixelsPolicy='Native RGBA copied verbatim into a transparent common canvas; measured single left-hand pivot and common PPU.';
 fs.writeFileSync(path.join(root,'art_source/paper_ledge/progress.json'),JSON.stringify(progress,null,2)+'\n');
 const report={schemaVersion:1,task:'PAPER_LEDGE_LEFT_HAND',nativeTag:tag,native,images,
  supersedes:['paper_ledge_final','paper_ledge_left','paper_ledge_correct'],userRequestedView:'Comparison A: thumb at native image-left, inside of curled fingers visible',handPattern:'A',thumbSide:'image-left',userShapeConfirmed:true,
  behavior:{maximumHoldFrames:300,staminaPerSecond:25,regrabUntilLanding:false,normalAttacksBlocked:true,downwardAttacksAllowed:true},
  web:{status:'pending'},balancePreservation:read('docs/codex_logs/ledge_balance_preservation.json')};
 fs.writeFileSync(path.join(root,'docs/codex_logs/paper_ledge_verification.json'),JSON.stringify(report,null,2)+'\n');
 console.log('Native verification and nine one-handed pixel contracts confirmed. Web remains pending.');
})().catch(e=>{console.error(e);process.exitCode=1;});
