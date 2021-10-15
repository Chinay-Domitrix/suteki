module compiler.x64_backend;

import assembler.x64;
import compiler.compiler;
import compiler.ir;

import std.stdio;

struct X64Backend
{
    public  X64Assembler  assembler;
    private Compiler     *compiler;

    IRInstruction *instruction;

    // Convert return
    void convert_return()
    {
        IRReturn *return_instruction = cast(IRReturn *)(instruction);
        IRValue  *return_value       = &compiler.values[return_instruction.value];
    
        final switch (return_value.type)
        {
            case ir_type_void:
            {
                assembler.mov(g_eax, 60);
                assembler.xor(g_edi, g_edi);
                assembler.syscall();
                break;
            }

            case ir_type_i8:
            {
                assembler.mov(g_eax, 60);
                assembler.mov(g_edi, return_value.as_i8);
                assembler.syscall();
                break;
            }

            case ir_type_i32:
            {
                assembler.mov(g_eax, 60);
                assembler.mov(g_edi, return_value.as_i32);
                assembler.syscall();
                break;
            }
        }
    }

    // Convert IR into x64
    void convert(Compiler *_compiler)
    {
        compiler = _compiler;

        for (ulong i = 0; i < compiler.instructions.size;)
        {
            instruction = &compiler.instructions[i++];

            final switch (instruction.type)
            {
                case ir_return:
                {
                    convert_return();
                    break;
                }
            }
        }
    }
}