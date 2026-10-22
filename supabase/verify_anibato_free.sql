-- Run after 002, immediately before opening rooms to users. This entire test rolls back.
begin;
do $$
declare id uuid; data jsonb; blocked boolean; room_count integer;
begin
  if has_function_privilege('anon','public.anibato_room_create(text,text,jsonb)','EXECUTE') then raise exception 'anon must not call privileged RPC'; end if;
  if has_table_privilege('anon','anibato_private.rooms','SELECT') then raise exception 'rooms must not be public'; end if;
  if has_table_privilege('authenticated','anibato_private.rooms','SELECT') then raise exception 'rooms must not be readable by other accounts'; end if;
  data:=public.anibato_room_create(repeat('a',32),repeat('b',32),'{"type":"offer","sdp":"test-only"}');
  id:=(data->>'id')::uuid;
  blocked:=false;begin perform public.anibato_room_exchange(id,null);exception when others then blocked:=true;end;
  if not blocked then raise exception 'null key was accepted';end if;
  blocked:=false;begin perform public.anibato_room_exchange(id,repeat('c',32));exception when others then blocked:=true;end;
  if not blocked then raise exception 'wrong key was accepted';end if;
  data:=public.anibato_room_exchange(id,repeat('b',32));
  if data->'offer'->>'sdp'<>'test-only' then raise exception 'offer missing';end if;
  perform public.anibato_room_exchange(id,repeat('b',32),'{"type":"answer","sdp":"reply"}');
  data:=public.anibato_room_exchange(id,repeat('a',32));
  if data->'answer'->>'sdp'<>'reply' then raise exception 'answer missing';end if;
  blocked:=false;begin perform public.anibato_room_exchange(id,repeat('b',32),'{"type":"answer","sdp":"duplicate"}');exception when others then blocked:=true;end;
  if not blocked then raise exception 'duplicate answer was accepted';end if;
  if public.anibato_room_close(id,repeat('c',32)) then raise exception 'wrong owner closed a room';end if;
  if not public.anibato_room_close(id,repeat('a',32)) then raise exception 'owner cannot close room';end if;
  select count(*) into room_count from anibato_private.rooms;
  for i in room_count+1..4 loop perform public.anibato_room_create(repeat('a',32),repeat('b',32),'{"type":"offer","sdp":"test-only"}');end loop;
  blocked:=false;begin perform public.anibato_room_create(repeat('a',32),repeat('b',32),'{"type":"offer","sdp":"test-only"}');exception when others then if sqlerrm like '%FREE_ROOM_LIMIT%' then blocked:=true;else raise;end if;end;
  if not blocked then raise exception 'active room limit not enforced';end if;
end $$;
rollback;
select 'PASS: isolated rooms, capability checks, one answer, owner close, free limits; test changes rolled back' as result;
