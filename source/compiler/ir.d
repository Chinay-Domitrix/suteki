module compiler.ir;

enum
{
    ir_return,
}

enum 
{
    type_i8,
    type_i16,
    type_i32,
    type_i64,
    type_f32,
    type_f64,
    type_ptr,
    type_addr,
}

struct IRValuePtr
{
    uint index;
}

struct IRPtr
{
    uint index;
}

struct IRValue
{
    union
    {
        byte   as_i8;
        short  as_i16;
        int    as_i32;
        long   as_i64;
        float  as_f32;
        double as_f64;
        long   as_ptr;
        long   as_addr;
    }

    uint type;
}

struct IRReturn
{
    IRValuePtr value;
    uint       type;
}

struct IRInstruction
{
    union
    {
        IRReturn as_return;
    }

    uint type;
}