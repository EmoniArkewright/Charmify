using Modding;
using SereCore;
using SFCore;

namespace Charmify
{

    public class CharmifyMod : Mod
    {
        private const int LANTERN = 0;
        private const int ISMAS = 1;
        private const int QUILL = 2;
        private const int NAIL_UPGRADES = 3;
        private CharmHelper charmHelper { get; set; }

        private bool LanternEquipped;
        private bool IsmasEquipped;
        private bool NailRefined;
        private bool QuillEquipped;

        public override string GetVersion() => "0.1";

        #region Charm Names and Descriptions
        private string[] charmNames = { "Lumafly Lantern", "Ismas Tear", "Quill", "Nail Upgrades" };
        private string[] charmDescriptions = { "A bright light", "A useless charm", "Makes benching slower", "A sharper nail"};
        #endregion

        public override void Initialize()
        {
            Log("Initialising Charmify");
            var sprites = ResourceHelper.GetSprites("Charmify.Resources.");
            
            charmHelper = new CharmHelper();
            charmHelper.customCharms = 4;
            charmHelper.customSprites = new[] {sprites["Lantern"], sprites["Isma"], sprites["Quill"], sprites["PaleOre"]};

            InitCallbacks();
        }

        private void InitCallbacks()
        {
            ModHooks.Instance.GetPlayerBoolHook += OnGetPlayerBoolHook;
            ModHooks.Instance.SetPlayerBoolHook += OnSetPlayerBoolHook;
            ModHooks.Instance.LanguageGetHook += OnLanguageGetHook;
            ModHooks.Instance.GetPlayerIntHook += OnGetPlayerIntHook;
            ModHooks.Instance.SetPlayerIntHook += OnSetPlayerIntHook;
            ModHooks.Instance.AfterSavegameLoadHook += EquipCharmsOnLoadHook;
        }

        private void EquipCharmsOnLoadHook(SaveGameData data)
        {
            LanternEquipped = data.playerData.hasLantern;
            IsmasEquipped = data.playerData.hasAcidArmour;
            NailRefined = data.playerData.nailSmithUpgrades > 0;
            QuillEquipped = data.playerData.hasMap;
        }

        private string OnLanguageGetHook(string key, string sheet)
        {
            if (key.StartsWith("CHARM_NAME_"))
            {
                int charmNum = int.Parse(key.Split('_')[2]);
                if (charmHelper.charmIDs.Contains(charmNum))
                {
                    return charmNames[charmHelper.charmIDs.IndexOf(charmNum)];
                }
            }
            if (key.StartsWith("CHARM_DESC_"))
            {
                int charmNum = int.Parse(key.Split('_')[2]);
                if (charmHelper.charmIDs.Contains(charmNum))
                {
                    return charmDescriptions[charmHelper.charmIDs.IndexOf(charmNum)];
                }
            }
            return Language.Language.GetInternal(key, sheet);
        }


        public bool OnGetPlayerBoolHook(string target)
        {
            if (target.StartsWith("gotCharm_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (charmHelper.charmIDs.Contains(charmNum))
                {
                    return HasCharm(charmHelper.charmIDs.IndexOf(charmNum));
                }
            }
            if (target.StartsWith("newCharm_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (charmHelper.charmIDs.Contains(charmNum))
                {
                    return false;
                }
            }
            if (target.StartsWith("equippedCharm_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (charmHelper.charmIDs.Contains(charmNum))
                {
                    return IsEquipped(charmHelper.charmIDs.IndexOf(charmNum));
                }
            }
            
            bool @internal = PlayerData.instance.GetBoolInternal(target);

            switch (target)
            {
                case "hasAcidArmour":
                    return IsmasEquipped;
                case "hasLantern":
                    return LanternEquipped;
                case "hasQuill":
                    return QuillEquipped;
                default:
                    return @internal;
            }
            
        }

        private void OnSetPlayerBoolHook(string target, bool val)
        {
            if (target.StartsWith("equippedCharm_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (charmHelper.charmIDs.Contains(charmNum))
                {
                    Equip(charmHelper.charmIDs.IndexOf(charmNum), val);
                    return;
                }
            }

            switch (target)
            {
                case "hasLantern" :
                    LanternEquipped = val;
                    break;

                case "hasAcidArmour":
                    IsmasEquipped = val;
                    break;
                
                case "hasQuill":
                    QuillEquipped = val;
                    break;
            }
            
            PlayerData.instance.SetBoolInternal(target, val);
        }

        private int OnGetPlayerIntHook(string target)
        {
            if (target.StartsWith("charmCost_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (charmHelper.charmIDs.Contains(charmNum))
                {
                    return 0;
                }
            }

            if (target.Equals("nailDamage") && !NailRefined)
            {
                return 5;
            }
            return PlayerData.instance.GetIntInternal(target);
        }

        private void OnSetPlayerIntHook(string target, int val)
        {

            if ("nailSmithUpgrades".Equals(target))
            {
                NailRefined = val > 0;
            }
            
            PlayerData.instance.SetIntInternal(target, val);
        }

        private bool IsEquipped(int index)
        {
            switch (index)
            {
                case LANTERN:
                    return LanternEquipped;
                case ISMAS:
                    return IsmasEquipped;
                case QUILL:
                    return QuillEquipped;
                case NAIL_UPGRADES:
                    return NailRefined;
                default:
                    return false;
            }
        }

        private void Equip(int index, bool val)
        {
            switch (index)
            {
                case LANTERN:
                    LanternEquipped = val;
                    return;
                case ISMAS:
                    IsmasEquipped = val;
                    return;
                case QUILL:
                    QuillEquipped = val;
                    return;
                case NAIL_UPGRADES:
                    NailRefined = val;
                    return;
            }
        }

        private bool HasCharm(int index)
        {
            switch (index)
            {
                case LANTERN:
                    return PlayerData.instance.GetBoolInternal("hasLantern");
                case ISMAS:
                    return PlayerData.instance.GetBoolInternal("hasAcidArmour");
                case QUILL:
                    return PlayerData.instance.GetBoolInternal("hasQuill");
                case NAIL_UPGRADES:
                    return PlayerData.instance.GetIntInternal("nailSmithUpgrades") > 0;
                default:
                    return false;
            }
        }
    }
    
    
}