module compiler.ir;

enum
{
    ir_return,
}

enum 
{
    ir_type_void,
    ir_type_i8,
    ir_type_i16,
    ir_type_i32,
    ir_type_i64,
    ir_type_f32,
    ir_type_f64,
    ir_type_ptr,
    ir_type_addr,
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
    int value;
}

struct IRInstruction
{
    union
    {
        IRReturn as_return;
    }

    uint type;

    IRReturn *opCast(T : IRReturn *)()
    {
        return &as_return;
    }
}

struct BasicBlock
{
    int start;
    int end;
}