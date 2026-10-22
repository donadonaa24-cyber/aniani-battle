/* Preserve the exact Unity payload while keeping every static file below GitHub's file limit. */
'use strict';
self.addEventListener('install',event=>event.waitUntil(self.skipWaiting()));
self.addEventListener('activate',event=>event.waitUntil(self.clients.claim()));
let manifestPromise;
function manifest(){return manifestPromise??=fetch(new URL('payload-manifest.json',self.registration.scope),{cache:'no-store'}).then(r=>{if(!r.ok)throw Error('payload manifest unavailable');return r.json();});}
self.addEventListener('fetch',event=>{
 const url=new URL(event.request.url),base=new URL(self.registration.scope);
 if(event.request.method!=='GET'||url.origin!==base.origin||!url.pathname.startsWith(base.pathname+'Build/')||!url.pathname.endsWith('.data.unityweb'))return;
 event.respondWith((async()=>{
  const data=await manifest(),relative=url.pathname.slice(base.pathname.length),entry=data.files[relative];
  if(!entry)return fetch(event.request);
  let index=0,total=0;
  const stream=new ReadableStream({async pull(controller){
   try{
    if(index>=entry.parts.length){if(total!==entry.length)throw Error('payload size mismatch');controller.close();return;}
    const part=entry.parts[index++],response=await fetch(new URL(part.file,base));if(!response.ok)throw Error('payload part missing');
    const bytes=await response.arrayBuffer();if(bytes.byteLength!==part.length)throw Error('payload part size mismatch');
    const hash=Array.from(new Uint8Array(await crypto.subtle.digest('SHA-256',bytes)),b=>b.toString(16).padStart(2,'0')).join('');
    if(hash!==part.sha256)throw Error('payload integrity mismatch');total+=bytes.byteLength;controller.enqueue(new Uint8Array(bytes));
   }catch(error){controller.error(error);}
  }});
  return new Response(stream,{headers:{'Content-Type':'application/octet-stream','Content-Length':String(entry.length),'Cache-Control':'no-cache'}});
 })());
});
