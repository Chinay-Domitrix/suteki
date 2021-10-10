module assembler.x64;

struct X64Register
{
    uint index;
    uint size;
}

const X64Register x64_rax = { 0, 8 };

struct X64Assembler
{
    ubyte[] code;

    // Write bytes to code
    void write_32(int value)
    {
        ubyte *bytes = cast(ubyte *)(&value);

        for (uint i = 0; i < 4; ++i)
            code ~= bytes[i];
    }

    // MOV
    void mov(const ref X64Register register, int value)
    {
        code ~= cast(ubyte)(0xB8 + register.index);
        write_32(value);
    }

    // SYSCALL
    void syscall()
    {
        code ~= cast(ubyte)(0x050F);
    }

    // LEAVE
    void leave()
    {
        code ~= 0xC9;
    }

    // RET
    void ret()
    {
        code ~= 0xC3;
    }
}