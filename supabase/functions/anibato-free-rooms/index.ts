// Deploy as a public capability-protected endpoint; never expose a service key to the browser.
const origins = new Set(['https://donadonaa24-cyber.github.io']);
const actions: Record<string, string[]> = { create: ['p_host','p_join','p_offer'], exchange: ['p_room','p_key','p_answer'], close: ['p_room','p_key'] };
Deno.serve(async (request: Request) => {
  const origin = request.headers.get('origin') || '';
  if (!origins.has(origin) && !/^http:\/\/127\.0\.0\.1:\d+$/.test(origin)) return new Response('Origin not allowed',{status:403});
  const headers = {'Access-Control-Allow-Origin':origin,'Vary':'Origin','Access-Control-Allow-Methods':'POST, OPTIONS','Access-Control-Allow-Headers':'content-type','Content-Type':'application/json','Cache-Control':'no-store'};
  if (request.method==='OPTIONS') return new Response(null,{status:204,headers});
  if (request.method!=='POST') return new Response('{"error":"METHOD"}',{status:405,headers});
  try {
    const reader=request.body?.getReader(); if(!reader)throw Error('EMPTY');
    const chunks:Uint8Array[]=[];let size=0;
    while(true){const item=await reader.read();if(item.done)break;size+=item.value.length;if(size>73728){await reader.cancel();throw Error('SIZE');}chunks.push(item.value);}
    const bytes=new Uint8Array(size);let offset=0;for(const c of chunks){bytes.set(c,offset);offset+=c.length;}
    const body=JSON.parse(new TextDecoder().decode(bytes));
    if(!Object.prototype.hasOwnProperty.call(actions,body.action)||!body.args||typeof body.args!=='object'||Array.isArray(body.args)||Object.keys(body.args).some(k=>!actions[body.action].includes(k)))throw Error('ACTION');
    const secret=Deno.env.get('SUPABASE_SERVICE_ROLE_KEY');const url=Deno.env.get('SUPABASE_URL');
    if(!secret||!url)throw Error('CONFIG');
    const response=await fetch(url+'/rest/v1/rpc/anibato_room_'+body.action,{method:'POST',headers:{apikey:secret,Authorization:'Bearer '+secret,'Content-Type':'application/json'},body:JSON.stringify(body.args),signal:AbortSignal.timeout(8000)});
    const value=await response.json();
    if(!response.ok)return new Response(JSON.stringify({error:String(value.message).includes('FREE_ROOM_LIMIT')?'FREE_ROOM_LIMIT':'ROOM_UNAVAILABLE'}),{status:400,headers});
    return new Response(JSON.stringify(value),{headers});
  }catch{return new Response('{"error":"INVALID_REQUEST"}',{status:400,headers});}
});
