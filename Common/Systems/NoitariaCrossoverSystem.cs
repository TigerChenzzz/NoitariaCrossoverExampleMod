using NoitariaCrossoverExampleMod.Content.Items;
using Terraria.DataStructures;

namespace NoitariaCrossoverExampleMod.Common.Systems;
internal class NoitariaCrossoverSystem : ModSystem {
    private static bool getNoitaria;
    private static Mod noitaria;
    public static Mod Noitaria {
        get {
            if(!getNoitaria) {
                getNoitaria = true;
                if(!ModLoader.TryGetMod("Noitaria", out noitaria)) {
                    noitaria = null;
                }
            }
            return noitaria;
        }
    }
    public override void PostSetupContent() {
        if(Noitaria != null) {
            AddExtraModule(ModContent.ItemType<ExampleModule>(), ExampleModule.Modifiers);
        }
    }
    
    /// <summary>
    /// 添加联动模块
    /// 最好在PostSetupContent中调用
    /// </summary>
    /// <param name="itemID">此模块的ID</param>
    /// <param name="modifiers">
    /// 可用的键值对如下:(最好按顺序摆放)
    /// <para/>"type"           , int  值, 代表模块类型(实际上是int转CardType):
    /// <para/>"manaCost"       , int  值, 代表消耗魔量, 设置为负值可以回复魔量
    /// <para/>"extraDraw"      , int  值, 代表可以额外再抽取多少模块, -1代表无限
    /// <para/>
    /// <para/>"useTimeAdd"     , int  值, 代表施放延迟, 单位为帧
    /// <para/>"rechargeTimeAdd", int  值, 代表充能延迟, 单位为帧
    /// <para/>"shoot"          , int  值, 代表射出的projectileID
    /// <para/>"shootNum"       , int  值, 代表射出的弹幕数量
    /// <para/>"damage"         , int  值, 代表弹幕的伤害, 如果设置了shoot则一定要设置此值(除非你不想要弹幕有伤害)
    /// <para/>"knockBack"      , float值, 代表弹幕的击退
    /// <para/>"shootSpeed"     , float值, 代表弹幕的射速, 如果设置了shoot则一定要设置此值(除非你不想要弹幕有初始速度)
    /// <para/>
    /// <para/>"damageAdd"      , int  值, 代表直接增加的伤害, 当然可以设置为负数, 代表减益, 下同
    /// <para/>"damagePer"      , float值, 代表增加伤害的百分比, 如0.2f代表增加20%的伤害(加算)
    /// <para/>"damageMul"      , float值, 代表伤害的倍率, 如1.6f代表变为1.6倍(乘算), 默认值为1(这个就别设置成负数了)
    /// <para/>"critAdd"        , int  值, 代表直接增加的暴击率, 如3代表增加3%的暴击率
    /// <para/>"knockBackAdd"   , float值, 代表增加击退
    /// <para/>"scatterAdd"     , float值, 代表增加散射, 弧度值
    /// <para/>"penetrateAdd"   , int  值, 代表增加的穿透, -1代表无限穿透, -2代表不穿透
    /// <para/>"timeAdd"        , int  值, 代表修正弹幕的持续时间, 单位为帧
    /// <para/>"speedMul"       , float值, 代表弹幕速度乘数, 默认1
    /// <para/>"timeMul"        , float值, 代表持续时间乘数, 默认1
    /// <para/>
    /// <para/>"ignoreTile"     , bool 值, 代表是否穿墙
    /// <para/>"recoilAdd"      , float值, 代表后坐力
    /// <para/>"castDistanceAdd", float值, 代表远程施法的距离
    /// <para/>"formRadian"     , float值, 弧度制, 如果想要一圈发射(就像召唤黄蜂群那样)就把它设置为2PI, 详细解释参见 https://noita.wiki.gg/zh/wiki/%E9%98%B5%E5%9E%8B
    /// <para/>
    /// <para/>"triggerType"    , int  值, 代表触发类型, 一般不填, 如果填1代表消失触发, 2代表受击触发,3代表1+2
    /// <para/>"triggerMaxDraw" , int  值, 代表触发中的最大抽数, -1(默认值)代表无限, 只有triggerType非0时才有效
    /// <para/>
    /// <para/>"extraBehavior"  , <see cref="ExtraBehaviorDelegate"/> 值, 直接是对应的委托也可以
    /// <para/>"shootModify"    , <see cref="ShootModifyDelegate"/> 值, 直接是对应的委托也可以
    /// </param>
    /// <returns>ModuleID, 若添加失败, 则是-1</returns>
    public static int AddExtraModule(int itemID, Dictionary<string, object> modifiers = null) {
        //其实Call的第一个参数直接用字符串"AddExtraModule"或者数字1也可以
        return (int)Noitaria.Call(ModCallID.AddExtraModule, itemID, modifiers);
    }
    public static int ModuleIDToItemID(int moduleID) {
        return (int)Noitaria.Call(ModCallID.ModuleIDToItemID, moduleID);
    }
    public enum ModCallID
    {
        None                  = 0,
        AddExtraModule        = 1,
        ModuleIDToItemID      = 2,
    }
    public enum CardType
    {
        Empty            =  0,   //空
        Projectile       =  1,   //弹幕
        Modifier         =  2,   //修正
        StaticProjectile =  3,   //静态弹幕
        MultiDraw        =  4,   //多重施法
        Special          =  5,   //特殊, 如触发, 一分为多等
        Passive          =  6,   //被动, 只要拿在手上就会生效
        Condition        =  7,   //条件
        Other            = -1,   //其他
    }
    [Flags]
    public enum TriggerType
    {
        None = 0,           //不触发
        Death = 1,          //死亡触发
        Collide = 2,        //接触触发
        Time = 4,           //定时触发
        Other = 0x40000000  //其他
    }
    /// <summary>
    /// 在成功地抽出此模块时执行
    /// </summary>
    /// <param name="self">自己</param>
    /// <param name="wand">发射所使用的法杖(物品)</param>
    /// <param name="state">施法状态, 不知道可以不管</param>
    /// <param name="entity">发射的实体, 有可能为<see cref="Player"/>(玩家), <see cref="Projectile"/>(灵化魔杖), 或者<see cref="NPC"/>(暂时还只能作弊调出来的野生杖灵)</param>
    /// <param name="pars">不知道就别管</param>
    /// <returns>
    /// <para/>实际返回值为<see cref="DrawReturn"/>类型， 一般返回0就行了
    /// <para/>返回1(<see cref="DrawReturn.CardFail"/>)可以让此模块不起作用, 比如一些限定条件不满足等
    /// <para/>但这样仍然会占用施法次数并消耗魔法
    /// <para/>返回8(<see cref="DrawReturn.DrawOutCount"/>)会强制结束这次抽取
    /// </returns>
    public delegate int ExtraBehaviorDelegate(Item self, Item wand, object state, Entity entity, object pars);
    /// <summary>
    /// 在发射时执行, 包括触发时
    /// </summary>
    /// <param name="self">自己</param>
    /// <param name="state">施法状态</param>
    /// <param name="entity">发射的实体</param>
    /// <param name="source">源</param>
    /// <param name="position">发射的位置</param>
    /// <param name="rotation">发射的旋转角</param>
    /// <param name="num">有多少个此模块起作用, 如果不清楚这个是什么, 可以把整个函数用 for(int i = 0; num > i; ++i) 包起来</param>//小于号要用&lt;转义, 所以就用大于号了
    public delegate void ShootModifyDelegate(Item self, Item wand, object state, Entity entity, IEntitySource source, ref Vector2 position, ref float rotation, int num);
    [Flags]
    public enum DrawReturn
    {
        Default = 0,
        /// <summary>
        /// 此卡执行失败
        /// </summary>
        CardFail = 1,
        /// <summary>
        /// 代表抽完卡组
        /// 一般为有考虑在回绕中的情况
        /// </summary>
        DrawOutCard = 2,
        /// <summary>
        /// 代表法杖是否为空
        /// 设置时会同时设置抽完卡组
        /// </summary>
        EmptyWand = 4,
        /// <summary>
        /// 表示抽取数用完
        /// </summary>
        DrawOutCount = 8,
        /// <summary>
        /// 表示是否强制重新充能
        /// 一般会直接结束抽取
        /// 仅由<see cref="Card.OnDraw"/>返回
        /// </summary>
        ForceRecharge = 16,
        /// <summary>
        /// 表示是否应该结束施法
        /// 在抽完牌或抽数, 法杖为空或强制充能时返回真
        /// </summary>
        EndDraw = DrawOutCard | EmptyWand | DrawOutCount | ForceRecharge,
        /// <summary>
        /// 表示是否需要让法杖充能
        /// 在抽完牌或法杖为空或强制充能时返回真
        /// </summary>
        NeedRecharge = DrawOutCard | EmptyWand | ForceRecharge,
    }
}
