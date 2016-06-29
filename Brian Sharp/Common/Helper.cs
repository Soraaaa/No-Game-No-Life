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
        #region Enums

        public enum SmiteType
        {
            Grey,

            Purple,

            Red,

            Blue,

            None
        }

        #endregion

        #region Public Properties

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

        #endregion

        #region Public Methods and Operators

        public static bool CanKill(Obj_AI_Base target, double subDmg)
        {
            return target.Health < subDmg;
        }

        public static bool CastFlash(Vector3 pos)
        {
            return Flash.IsReady() && pos.IsValid() && Player.Spellbook.CastSpell(Flash, pos);
        }

        public static bool CastIgnite(Obj_AI_Hero target)
        {
            return Ignite.IsReady() && target.IsValidTarget(600)
                   && target.Health + 5 < Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite)
                   && Player.Spellbook.CastSpell(Ignite, target);
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

        public static MenuItem GetItem(string subMenu, string item)
        {
            return MainMenu.Item("_" + subMenu + "_" + item, true);
        }

        public static List<Obj_AI_Minion> GetMinions(
            Vector3 from,
            float range,
            MinionTypes type = MinionTypes.All,
            MinionTeam team = MinionTeam.Enemy,
            MinionOrderTypes order = MinionOrderTypes.Health)
        {
            var result = from minion in ObjectManager.Get<Obj_AI_Minion>()
                         where minion.IsValidTarget(range, false, @from)
                         let minionTeam = minion.Team
                         where
                             (team == MinionTeam.Neutral && minionTeam == GameObjectTeam.Neutral)
                             || (team == MinionTeam.Ally
                                 && minionTeam
                                 == (Player.Team == GameObjectTeam.Chaos ? GameObjectTeam.Chaos : GameObjectTeam.Order))
                             || (team == MinionTeam.Enemy
                                 && minionTeam
                                 == (Player.Team == GameObjectTeam.Chaos ? GameObjectTeam.Order : GameObjectTeam.Chaos))
                             || (team == MinionTeam.NotAlly && minionTeam != Player.Team)
                             || (team == MinionTeam.NotAllyForEnemy
                                 && (minionTeam == Player.Team || minionTeam == GameObjectTeam.Neutral))
                             || team == MinionTeam.All
                         where
                             (minion.IsMelee() && type == MinionTypes.Melee)
                             || (!minion.IsMelee() && type == MinionTypes.Ranged) || type == MinionTypes.All
                         where MinionManager.IsMinion(minion) || minionTeam == GameObjectTeam.Neutral || IsPet(minion)
                         select minion;
            switch (order)
            {
                case MinionOrderTypes.Health:
                    result = result.OrderBy(i => i.Health);
                    break;
                case MinionOrderTypes.MaxHealth:
                    result = result.OrderBy(i => i.MaxHealth).Reverse();
                    break;
            }
            return result.ToList();
        }

        public static List<Obj_AI_Minion> GetMinions(
            float range,
            MinionTypes type = MinionTypes.All,
            MinionTeam team = MinionTeam.Enemy,
            MinionOrderTypes order = MinionOrderTypes.Health)
        {
            return GetMinions(Player.ServerPosition, range, type, team, order);
        }

        public static T GetValue<T>(string subMenu, string item)
        {
            return MainMenu.Item("_" + subMenu + "_" + item, true).GetValue<T>();
        }

        public static bool IsPet(Obj_AI_Minion obj)
        {
            var pets = new[]
                           {
                               "annietibbers", "elisespiderling", "heimertyellow", "heimertblue", "leblanc",
                               "malzaharvoidling", "shacobox", "shaco", "yorickspectralghoul", "yorickdecayedghoul",
                               "yorickravenousghoul", "zyrathornplant", "zyragraspingplant"
                           };
            return pets.Contains(obj.CharData.BaseSkinName.ToLower());
        }

        public static bool IsSmiteable(Obj_AI_Minion obj)
        {
            return MinionManager.IsMinion(obj) || obj.Team == GameObjectTeam.Neutral || IsPet(obj);
        }

        public static bool IsWard(Obj_AI_Minion obj)
        {
            return obj.Team != GameObjectTeam.Neutral && !MinionManager.IsMinion(obj) && !IsPet(obj)
                   && MinionManager.IsMinion(obj, true);
        }

        public static void SmiteMob()
        {
            if (!GetValue<bool>("SmiteMob", "Smite") || !Smite.IsReady())
            {
                return;
            }
            var obj =
                MinionManager.GetMinions(760, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(i => CanSmiteMob(i.Name));
            if (obj == null)
            {
                return;
            }
            CastSmite(obj);
        }

        #endregion

        #region Methods

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

        #endregion
    }
}
