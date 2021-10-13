module compiler.compiler;

import compiler.typer;
import compiler.ir;
import compiler.ast;
import utilities.list;

import std.stdio;

struct Compiler
{
    Typer typer;
    bool  had_error;
    ulong node_id;

    List!(IRInstruction) instructions;
    List!(IRValue)       values;
    List!(BasicBlock)    blocks;

    // Make IR instruction
    private IRInstruction *make_instruction(uint type)
    {
        IRInstruction instruction;
        instruction.type = type;

        return &instructions[instructions.add(instruction)];
    }

    // Convert return statement
    private void convert_return_statement(NodeReturnStatement *node)
    {
        IRReturn *instruction = cast(IRReturn *)(make_instruction(ir_return));

        if (node.expression == -1)
        {
            instruction.type  = ir_type_void;
            instruction.value = -1;
        }
        else
            writeln("TODO: handle this at compiler.d:40");
    }

    // Convert nodes to IR
    private void convert()
    {
        Node *node = &typer.parser.ast[node_id++];

        switch (node.type)
        {
            case node_return_statement:
            {
                convert_return_statement(cast(NodeReturnStatement *)(node));
                break;
            }

            default:
                break;
        }
    }

    // Start the compiler
    bool start()
    {
        had_error = false;

        instructions.initialize();
        values      .initialize();
        blocks      .initialize();

        if (!typer.start())
            return false;

        for (node_id = 0; node_id < typer.parser.ast.size;)
        {
            Node *node = &typer.parser.ast[node_id++];

            switch (node.type)
            {
                case node_function_declaration:
                {
                    NodeFunctionDeclaration *function_node = cast(NodeFunctionDeclaration *)(node);
                    NodeBlock               *block_node    = cast(NodeBlock *)(typer.parser.ast[function_node.block]);

                    BasicBlock block;
                    block.start = cast(uint)(instructions.size);

                    node_id = (function_node.block + 1);

                    while (node_id < block_node.end)
                        convert();

                    block.end = cast(uint)(instructions.size);
                    blocks.add(block);
                    break;
                }

                default:
                    break;
            }
        }

        for (ulong i = 0; i < instructions.size; ++i)
        {
            IRInstruction *instruction = &instructions[i];

            final switch (instruction.type)
            {
                case ir_return:
                {
                    writeln("    ret void");
                    break;
                }
            }
        }

        return !had_error;
    }
}