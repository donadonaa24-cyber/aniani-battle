// Technical packing only: copy each native RGBA pixel into a transparent fixed canvas.
const fs=require('fs'),path=require('path'),crypto=require('crypto'),sharp=require('sharp');
const root=path.resolve(__dirname,'../..');
const read=p=>JSON.parse(fs.readFileSync(path.join(root,p),'utf8').replace(/^\uFEFF/,''));
const jobs=read('art_source/paper_ledge/progress.json').characters;
// Pixel coordinates on the native one-handed PNG, from top left; the curled fingertips touch the edge.
const contacts={toma:[918,95],ren:[1007,174],chizuru:[891,101],minato:[1023,96],shiro:[1149,141],yui:[1090,185],king:[1050,130],akatsuki:[974,79],ryuta:[1038,84]};
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
(async()=>{
 const rows=[];
 for(let index=0;index<jobs.length;index++){
  const j=jobs[index];if(j.pose!=='one_hand_hang'||j.grippingHand!=='left'||j.handPattern!=='A'||j.thumbSide!=='image-left'||!j.userShapeConfirmed||!j.source.endsWith('/hang_user_A_v1.png'))throw Error('Only the final user-selected A artwork may be adopted');
  const input=path.join(root,j.source),p=await sharp(input).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  if(p.info.width>1304||p.info.height>1450)throw Error('Native artwork exceeds fixed canvas');
  const dx=Math.floor((1304-p.info.width)/2),dy=Math.floor((1450-p.info.height)/2),output=Buffer.alloc(1304*1450*4);
  for(let y=0;y<p.info.height;y++)p.data.copy(output,((y+dy)*1304+dx)*4,y*p.info.width*4,(y+1)*p.info.width*4);
  const prepared=`art_source/paper_ledge/${j.key}/prepared_left.png`;
  await sharp(output,{raw:{width:1304,height:1450,channels:4}}).png().toFile(path.join(root,prepared));
  const decoded=await sharp(path.join(root,prepared)).raw().toBuffer();
  for(let y=0;y<p.info.height;y++)if(!p.data.subarray(y*p.info.width*4,(y+1)*p.info.width*4).equals(decoded.subarray(((y+dy)*1304+dx)*4,((y+dy)*1304+dx+p.info.width)*4)))throw Error('Native pixels changed');
  rows.push({key:j.key,index,pose:j.pose,grippingHands:1,grippingHand:'left',source:j.source,sourceSha256:sha(fs.readFileSync(input)),prepared,sha256:sha(fs.readFileSync(path.join(root,prepared))),
   handX:contacts[j.key][0]+dx,handY:contacts[j.key][1]+dy,offsetX:dx,offsetY:dy,nativePixelsPreserved:true,artApproved:false});
 }
 fs.writeFileSync(path.join(root,'art_source/paper_ledge/adoption.json'),JSON.stringify({schemaVersion:3,complete:true,pose:'one_hand_hang',grippingHand:'left',handPattern:'A',thumbSide:'image-left',userShapeConfirmed:true,frames:rows},null,2)+'\n');
 console.log('9 native RGBA images packed without resampling; hand anchors recorded.');
})().catch(e=>{console.error(e);process.exitCode=1;});
