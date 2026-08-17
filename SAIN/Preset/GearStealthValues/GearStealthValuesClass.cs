using System.Collections.Generic;
using SAIN.Preset.Shared.GearStealthValues;

namespace SAIN.Preset.GearStealthValues;

public class GearStealthValuesClass
{
    public Dictionary<EEquipmentType, List<ItemStealthValue>> ItemStealthValues = [];
    public readonly List<ItemStealthValue> Defaults = [];

    public GearStealthValuesClass(Dictionary<EEquipmentType, List<ItemStealthValue>> serverValues)
    {
        if (serverValues == null || serverValues.Count == 0)
        {
            Logger.LogError("[SAIN] Server preset contained no gear stealth values.");
            return;
        }

        foreach (var kv in serverValues)
        {
            var list = getList(kv.Key);
            foreach (var item in kv.Value)
            {
                addItem(item.Name, item.EquipmentType, item.ItemID, item.StealthValue, list, true);
            }
        }
    }

    private List<ItemStealthValue> getList(EEquipmentType type)
    {
        if (!ItemStealthValues.TryGetValue(type, out var list))
        {
            list = [];
            ItemStealthValues.Add(type, list);
        }

        return list;
    }

    private void addItem(
        string name,
        EEquipmentType type,
        string id,
        float stealthValue,
        List<ItemStealthValue> list,
        bool addAsDefault = false
    )
    {
        if (!doesItemExist(name, list))
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
        if (addAsDefault)
        {
            addItem(name, type, id, stealthValue, Defaults, false);
        }
    }

    private bool doesItemExist(string name, List<ItemStealthValue> list)
    {
        foreach (var item in list)
        {
            if (item.Name == name)
            {
                return true;
            }
        }
        return false;
    }
}
