using SAIN.Preset.Shared.GearStealthValues;

namespace SAINServerMod.Generators;

public static class GearStealthGenerator
{
    private const string Backpack_pilgrim = "59e763f286f7742ee57895da";
    private const string Backpack_raid = "5df8a4d786f77412672a1e3b";
    private const string Boonie_MILTEC = "5b4327aa5acfc400175496e0";
    private const string Boonie_CHIMERA = "60b52e5bc7d8103275739d67";
    private const string Boonie_DOORKICKER = "5d96141523f0ea1b7f2aacab";
    private const string Boonie_JACK_PYKE = "618aef6d0a5a59657e5f55ee";
    private const string Helmet_TAN_ULACH = "5b40e2bc5acfc40016388216";
    private const string Helmet_UNTAR_BLUE = "5aa7d03ae5b5b00016327db5";

    public static Dictionary<EEquipmentType, List<ItemStealthValue>> BuildDefaults()
    {
        var result = new Dictionary<EEquipmentType, List<ItemStealthValue>>();

        var headWears = GetList(result, EEquipmentType.Headwear);
        Add(headWears, "MILTEC", EEquipmentType.Headwear, Boonie_MILTEC, 1.2f);
        Add(headWears, "CHIMERA", EEquipmentType.Headwear, Boonie_CHIMERA, 1.2f);
        Add(headWears, "DOORKICKER", EEquipmentType.Headwear, Boonie_DOORKICKER, 1.2f);
        Add(headWears, "JACK_PYKE", EEquipmentType.Headwear, Boonie_JACK_PYKE, 1.2f);
        Add(headWears, "TAN_ULACH", EEquipmentType.Headwear, Helmet_TAN_ULACH, 0.9f);
        Add(headWears, "UNTAR_BLUE", EEquipmentType.Headwear, Helmet_UNTAR_BLUE, 0.85f);

        var backPacks = GetList(result, EEquipmentType.BackPack);
        Add(backPacks, "Pilgrim", EEquipmentType.BackPack, Backpack_pilgrim, 0.85f);
        Add(backPacks, "Raid", EEquipmentType.BackPack, Backpack_raid, 0.875f);

        return result;
    }

    private static List<ItemStealthValue> GetList(Dictionary<EEquipmentType, List<ItemStealthValue>> dict, EEquipmentType type)
    {
        if (!dict.TryGetValue(type, out var list))
        {
            list = new List<ItemStealthValue>();
            dict.Add(type, list);
        }
        return list;
    }

    public static void Add(List<ItemStealthValue> list, string name, EEquipmentType type, string id, float stealthValue)
    {
        list.Add(
            new ItemStealthValue
            {
                Name = name,
                EquipmentType = type,
                ItemID = id,
                StealthValue = stealthValue,
            }
        );
    }
}
