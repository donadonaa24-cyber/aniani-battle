-- Independent addition; never re-run the portal's 001_common.sql.
begin;
create schema if not exists anibato_private;
revoke all on schema anibato_private from public, anon, authenticated;
create table if not exists anibato_private.room_budget (
  day date primary key, created integer not null check (created between 0 and 80)
);
create table if not exists anibato_private.rooms (
  id uuid primary key default gen_random_uuid(),
  host_hash bytea not null, join_hash bytea not null,
  offer jsonb not null, answer jsonb,
  reads integer not null default 0 check (reads between 0 and 48),
  expires_at timestamptz not null default (now() + interval '120 seconds')
);
alter table anibato_private.room_budget enable row level security;
alter table anibato_private.rooms enable row level security;
revoke all on all tables in schema anibato_private from public, anon, authenticated;

create or replace function public.anibato_room_create(p_host text, p_join text, p_offer jsonb)
returns jsonb language plpgsql security definer set search_path = pg_catalog, anibato_private as $$
declare room anibato_private.rooms; total integer;
begin
  if p_host is null or p_join is null or p_host !~ '^[0-9a-f]{32}$' or p_join !~ '^[0-9a-f]{32}$' or p_host = p_join
    or jsonb_typeof(p_offer) is distinct from 'object' or octet_length(p_offer::text) > 65536
    or p_offer->>'type' is distinct from 'offer' then raise exception 'INVALID_ROOM'; end if;
  -- One global lock bounds storage and the shared project's daily request budget.
  perform pg_advisory_xact_lock(1684108385);
  delete from anibato_private.rooms where expires_at < now();
  delete from anibato_private.room_budget where day < current_date - 2;
  insert into anibato_private.room_budget values (current_date, 0) on conflict do nothing;
  select created into total from anibato_private.room_budget where day = current_date for update;
  if total >= 80 or (select count(*) from anibato_private.rooms) >= 4 then raise exception 'FREE_ROOM_LIMIT'; end if;
  update anibato_private.room_budget set created = created + 1 where day = current_date;
  insert into anibato_private.rooms(host_hash, join_hash, offer)
    values(sha256(convert_to(p_host,'UTF8')),sha256(convert_to(p_join,'UTF8')),p_offer) returning * into room;
  return jsonb_build_object('id',room.id,'expires_at',room.expires_at);
end $$;

create or replace function public.anibato_room_exchange(p_room uuid, p_key text, p_answer jsonb default null)
returns jsonb language plpgsql security definer set search_path = pg_catalog, anibato_private as $$
declare room anibato_private.rooms; key_hash bytea;
begin
  if p_key is null or p_key !~ '^[0-9a-f]{32}$' then raise exception 'INVALID_ROOM'; end if;
  key_hash := sha256(convert_to(p_key,'UTF8'));
  select * into room from anibato_private.rooms where id=p_room and expires_at>now() for update;
  if not found or key_hash not in (room.host_hash,room.join_hash) or room.reads >=48 then raise exception 'ROOM_EXPIRED'; end if;
  if p_answer is not null then
    if key_hash <> room.join_hash or room.answer is not null
      or jsonb_typeof(p_answer) is distinct from 'object' or octet_length(p_answer::text)>65536
      or p_answer->>'type' is distinct from 'answer' then raise exception 'INVALID_ANSWER'; end if;
    update anibato_private.rooms set answer=p_answer,reads=reads+1 where id=p_room;
    return jsonb_build_object('ok',true);
  end if;
  update anibato_private.rooms set reads=reads+1 where id=p_room;
  if key_hash=room.host_hash then return jsonb_build_object('answer',room.answer); end if;
  if room.answer is not null then raise exception 'ROOM_FULL'; end if;
  return jsonb_build_object('offer',room.offer);
end $$;

create or replace function public.anibato_room_close(p_room uuid,p_key text)
returns boolean language plpgsql security definer set search_path = pg_catalog, anibato_private as $$
begin
  delete from anibato_private.rooms where id=p_room and host_hash=sha256(convert_to(p_key,'UTF8'));
  return found;
end $$;
revoke all on function public.anibato_room_create(text,text,jsonb) from public;
revoke all on function public.anibato_room_exchange(uuid,text,jsonb) from public;
revoke all on function public.anibato_room_close(uuid,text) from public;
revoke all on function public.anibato_room_create(text,text,jsonb),public.anibato_room_exchange(uuid,text,jsonb),public.anibato_room_close(uuid,text) from anon,authenticated;
grant execute on function public.anibato_room_create(text,text,jsonb),public.anibato_room_exchange(uuid,text,jsonb),public.anibato_room_close(uuid,text) to service_role;
commit;
