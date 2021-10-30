namespace Suteki
{
    enum UserTypeKind
    {
        Primitive,
    }

    class UserType
    {
        UserTypeKind Kind;
        string       Name;
        uint         Size;

        // Initialize the Type
        public UserType(UserTypeKind kind, string name, uint size)
        {
            Kind = kind;
            Name = name;
            Size = size;
        }
    }
}