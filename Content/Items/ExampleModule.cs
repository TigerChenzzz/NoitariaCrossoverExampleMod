using Terraria.GameContent.Creative;

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
}
