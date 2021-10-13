module compiler.x64_backend;

import assembler.x64;
import compiler.compiler;
import compiler.ir;

import std.stdio;

struct X64Backend
{
    private X64Assembler  assembler;
    private Compiler     *compiler;

    IRInstruction *instruction;

    // Convert return
    void convert_return()
    {
        IRReturn *return_instruction = cast(IRReturn *)(instruction);

        if (return_instruction.value == -1)
        {
            assembler.xor(g_eax, g_eax);
            assembler.ret();
        }
        else
            writeln("TODO: handle this at x64_backend.d:27");
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

        writeln(assembler.code);
    }
}