module utilities.memory;

import core.stdc.stdlib : exit;
import std.algorithm;

version (linux)
{
    import core.sys.linux.sys.mman;

    T *memory_reserve(T)(ulong size)
    {
        T *memory = cast(T *)(mmap(null, size * T.sizeof, 0, MAP_ANONYMOUS | MAP_PRIVATE, -1, 0));

        if (memory == null)
            exit(1);

        return memory;
    }

    void memory_commit(T)(void *memory, ulong size)
    {
        if (mprotect(memory, size * T.sizeof, PROT_READ | PROT_WRITE) == -1)
            exit(1);   
    }

    void memory_release(T)(void *memory, ulong size)
    {
        if (munmap(memory, size * T.sizeof) == -1)
            exit(1);
    }
}

version (Windows)
{
    import core.sys.windows.windows;

    T *memory_reserve(T)(ulong size)
    {
        T *memory = cast(T *)(VirtualAlloc(null, size * T.sizeof, MEM_RESERVE, PAGE_READWRITE));

        if (memory == null)
            exit(1);

        return memory;
    }

    void memory_commit(T)(void *memory, ulong size)
    {
        if (VirtualAlloc(memory, size * T.sizeof, MEM_COMMIT, PAGE_READWRITE) == null)
            exit(1);   
    }

    void memory_release(T)(void *memory, ulong size)
    {
        if (!VirtualFree(memory, size * T.sizeof))
            exit(1);
    }
}

struct BumpAllocator
{
    ulong  size;
    ulong  room;
    ubyte *data;

    // Initialize the allocator
    void initialize()
    {
        data = memory_reserve!(ubyte)((1024 * 1024 * 1024) * 1);
        size = 0;
        room = 1024;

        memory_commit!(ubyte)(data, room);
    }

    // Grow the allocator
    T *grow(T)(ulong _size)
    {
        T *memory  = cast(T *)(data + size);
        size      += _size;

        if (size >= room)
        {
            room = max(size * 2, size + _size);
            memory_commit!(ubyte)(data, room);
        }
        
        return memory; 
    }
}