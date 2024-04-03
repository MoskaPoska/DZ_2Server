namespace IndexLibrary
{
    public class Street
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public override string ToString()
        {
            return $"{ID}, {Name}, {Index}";
        }
    }
}