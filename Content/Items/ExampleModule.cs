using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ModLoader.IO;
using static NoitariaCrossoverExampleMod.Common.Systems.NoitariaCrossoverSystem;

namespace NoitariaCrossoverExampleMod.Content.Items;
internal class ExampleModule : ModItem {
    public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("Noitaria");
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }
    public override void SetDefaults() {
        Item.width = 32;
        Item.height = 32;
        Item.value = Item.sellPrice(0, 0, 10, 0);
        Item.rare = ItemRarityID.Blue;
    }
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.Find<ModItem>("Noitaria", "空模块").Type)
            .AddIngredient(ModContent.Find<ModItem>("Noitaria", "铭刻金粉").Type)
            .AddIngredient(ItemID.DirtBlock, 10)
            .AddTile(ModContent.Find<ModTile>("Noitaria", "铭刻台").Type)
            .Register();
    }

    public static int ExtraBehavior(Item self, Item wand, object state, Entity entity, object pars) {
        if (Main.netMode != NetmodeID.Server) {
            CombatText.NewText(entity.Hitbox, Color.Yellow, "WOW");
        }
        return 0;
    }

    private int number = 100;
    private int GetNumber(int num) {
        number = Math.Max(number - num, 0);
        if (number > 0) {
            number -= 1;
        }
        return number;
    }
    public static void ShootModify(Item self, Item wand, object state, Entity entity, IEntitySource source, ref Vector2 position, ref float rotation, int num) {
        ExampleModule module = self.ModItem as ExampleModule;
        CombatText.NewText(new Rectangle((int)position.X - 10, (int)position.Y - 10, 20, 20), Color.White, module.GetNumber(num));
    }
    public override void SaveData(TagCompound tag) {
        tag["number"] = number;
    }
    public override void LoadData(TagCompound tag) {
        if (!tag.TryGet("number", out number)) {
            number = 100;
        }
    }

    public static Dictionary<string, object> Modifiers => new() {
        { "type", CardType.Projectile },
        { "manaCost", 20 },

        { "useTimeAdd", 20 },
        { "shoot", (int)ProjectileID.WoodenArrowFriendly },
        { "damage", 10 },
        { "shootSpeed", 6 },
        { "critAdd", 4 },

        { "extraBehavior", new ExtraBehaviorDelegate(ExtraBehavior) },
        { "shootModify", new ShootModifyDelegate(ShootModify) },
    };
}
