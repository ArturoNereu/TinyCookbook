namespace Unity.Editor
{
    internal struct ComponentFilter
    {
        public NameFilter Name { get; set; }
        public bool Universal => string.IsNullOrEmpty(Name.Name);
        public bool Inverted => Name.Inverted;
        public bool Keep(string str) => Name.Keep(str);
    }
}
