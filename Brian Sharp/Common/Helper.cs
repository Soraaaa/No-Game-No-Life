using System.Collections.Generic;
using System.Linq;

using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Enumerations;

using SharpDX;
    
namespace BrianSharp.Common
{
    internal class Helper : Program
    {
        public enum SmiteType
        {
            Grey,

            Purple,

            Red,

            Blue,

            None
        }
        public static SmiteType CurrentSmiteType
        {
            get
            {
                if (Player.GetSpellSlot("s5_summonersmitequick").IsReady())
                {
                    return SmiteType.Grey;
                }
                if (Player.GetSpellSlot("itemsmiteaoe").IsReady())
                {
                    return SmiteType.Purple;
                }
                if (Player.GetSpellSlot("s5_summonersmiteduel").IsReady())
                {
                    return SmiteType.Red;
                }
                return Player.GetSpellSlot("s5_summonersmiteplayerganker").IsReady() ? SmiteType.Blue : SmiteType.None;
            }
        }

        public static float GetWardRange
        {
            get
            {
                return 600;
            }
        }

        public static InventorySlot GetWardSlot
        {
            get
            {
                var ward = Items.GetWardSlot();
                if (GetValue<bool>("Flee", "PinkWard") && ward == null)
                {
                    var wardPink = new[] { 3362, 2043 };
                    foreach (var item in
                        wardPink.Where(Items.CanUseItem)
                            .Select(i => Player.InventoryItems.FirstOrDefault(a => a.Id == (ItemId)i))
                            .Where(i => i != null))
                    {
                        ward = item;
                    }
                }
                return ward;
            }
        }

        public static bool PacketCast
        {
            get
            {
                return GetValue<bool>("Misc", "UsePacket");
            }
        }

        public static bool CanKill(Obj_AI_Base target, double subDmg)
        {
            return target.Health < subDmg;
        }

        public static bool CastFlash(Vector3 pos)
        {
            return Flash.IsReady() && pos.IsValid() && Player.Spellbook.CastSpell(Flash, pos);
        }

        public static bool CastSmite(Obj_AI_Base target, bool killable = true)
        {
            return Smite.IsReady() && target.IsValidTarget(760)
                   && (!killable || target.Health < Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Smite))
                   && Player.Spellbook.CastSpell(Smite, target);
        }

        public static void CustomOrbwalk(Obj_AI_Base target)
        {
            Orbwalker.Orbwalk(Orbwalker.InAutoAttackRange(target) ? target : null);
        }

        private static bool CanSmiteMob(string name)
        {
            if (GetValue<bool>("SmiteMob", "Baron") && name.StartsWith("SRU_Baron"))
            {
                return true;
            }
            if (GetValue<bool>("SmiteMob", "Dragon") && name.StartsWith("SRU_Dragon"))
            {
                return true;
            }
            if (name.Contains("Mini"))
            {
                return false;
            }
            if (GetValue<bool>("SmiteMob", "Red") && name.StartsWith("SRU_Red"))
            {
                return true;
            }
            if (GetValue<bool>("SmiteMob", "Blue") && name.StartsWith("SRU_Blue"))
            {
                return true;
            }
            if (GetValue<bool>("SmiteMob", "Krug") && name.StartsWith("SRU_Krug"))
            {
                return true;
            }
            if (GetValue<bool>("SmiteMob", "Gromp") && name.StartsWith("SRU_Gromp"))
            {
                return true;
            }
            if (GetValue<bool>("SmiteMob", "Raptor") && name.StartsWith("SRU_Razorbeak"))
            {
                return true;
            }
            return GetValue<bool>("SmiteMob", "Wolf") && name.StartsWith("SRU_Murkwolf");
        }
    }
}
