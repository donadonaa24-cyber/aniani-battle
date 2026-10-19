using System.Collections.Generic;
using System.Linq;
using AniBato.Battle;

namespace AniBato.Game
{
    // Presentation identity survives removal of another member of the same formation.
    public sealed class ProjectileArtworkSlots
    {
        private readonly Dictionary<Projectile,int> slots=new Dictionary<Projectile,int>();
        private readonly Dictionary<AttackInstance,int> next=new Dictionary<AttackInstance,int>();
        public int Index(Projectile projectile)=>slots[projectile];
        public void Refresh(IReadOnlyList<Projectile> projectiles)
        {
            var live=new HashSet<Projectile>(projectiles);
            foreach(var old in slots.Keys.Where(p=>!live.Contains(p)).ToArray())slots.Remove(old);
            var attacks=new HashSet<AttackInstance>(projectiles.Select(p=>p.Group));
            foreach(var old in next.Keys.Where(a=>!attacks.Contains(a)).ToArray())next.Remove(old);
            foreach(var projectile in projectiles)
            {
                if(slots.ContainsKey(projectile))continue;
                next.TryGetValue(projectile.Group,out int index);slots.Add(projectile,index);next[projectile.Group]=index+1;
            }
        }
    }
}
