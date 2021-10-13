module utilities.list;

import utilities.memory;

struct List(T)
{
    private BumpAllocator memory;

    public  ulong  size;
    private ulong  room;
    private T     *data;

    // Initialize the list
    void initialize()
    {
        memory.initialize();

        size = 0;
        room = 64;
        data = memory.grow!(T)(room);
    }

    // Add value to list
    ulong add(T value)
    {
        if (size >= room)
        {
            room *= 2;
            memory.grow!(T)(room);
        }

        data[size++] = value;
        return (size - 1);
    }

    // Get value by index
    ref T get(ulong index)
    {
        return data[index];
    }

    ref T opIndex(ulong index)
    {
        return data[index];
    }

    // Find value in list
    bool find(T value, ref T ret)
    {
        for (ulong i = 0; i < size; ++i)
        {
            if (data[i] == value)
            {
                ret = data[i];
                return true;
            }
        }

        return false;
    }
}