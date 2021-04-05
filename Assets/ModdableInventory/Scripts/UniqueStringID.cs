namespace ModdableInventory
{
    public class UniqueStringID
    {
        public string StringID { get; private set; }

        public UniqueStringID(string stringIDName)
        {
            StringID = stringIDName;
        }
    }
}