namespace Unity.Tiny.UIControls
{
    public enum PointerType
    {
        Mouse = 0,
        Touch = 1
    };

    public struct PointerID
    {
        public PointerType Type
        {
            get;
            private set;
        }

        public int ID
        {
            get;
            private set;
        }

        public PointerID(PointerType type, int id)
        {
            Type = type;
            ID = id;
        }

        public override bool Equals(object other)
        {
            if (!(other is PointerID))
                return false;

            return
                ((PointerID)other).Type == Type &&
                ((PointerID)other).ID == ID;
        }

        public override int GetHashCode()
        {
            return ID | ((int)Type << 16);
        }
    }
}
