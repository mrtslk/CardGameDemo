namespace RotaryHeart.Lib.SerializableDictionary
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class IDAttribute : System.Attribute
    {
        private string _id;

        public string Id
        {
            get { return _id; }
        }

        public IDAttribute(string id)
        {
            _id = id;
        }
    }
}