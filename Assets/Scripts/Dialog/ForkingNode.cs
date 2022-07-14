/// <summary>
/// A complex type of <see cref="DialogNode"/>, with multiple possible next paths depending on the option selected and the <see cref="Item"/>s in the inventory
/// </summary>
public class ForkingNode : DialogNode
{
    [System.Serializable]
    public class ForkNodeOption
    {
        public DialogNode nextNode;
        public Item requiredItem;
        public bool spendItemOnUse = false;
        public string displayText;
    }

    public ForkNodeOption[] nextNodes;

    private int currentSelectedOption = 0;

    public override DialogNode GetNextNode()
    {
        if (nextNodes[currentSelectedOption].requiredItem && nextNodes[currentSelectedOption].spendItemOnUse) Inventory.LoseItem(nextNodes[currentSelectedOption].requiredItem);

        return nextNodes[currentSelectedOption].nextNode;
    }

    public void SelectOption(int optionIndex)
    {
        currentSelectedOption = optionIndex;
    }
}
