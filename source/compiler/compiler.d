module compiler.compiler;

import compiler.typer;
import compiler.ir;
import compiler.ast;
import compiler.x64_backend;
import compiler.config;
import utilities.list;

import std.stdio;

struct Compiler
{
    Typer typer;
    ulong node_id;

    List!(IRInstruction) instructions;
    List!(IRValue)       values;
    List!(BasicBlock)    blocks;

    X64Backend x64_backend;

    // Make IR instruction
    private IRInstruction *make_instruction(uint type)
    {
        IRInstruction instruction;
        instruction.type = type;

        return &instructions[instructions.add(instruction)];
    }

    // Make IR value
    private IRValue *make_value(uint type, int *_id = null)
    {
        IRValue value;
        value.type = type;

        int id = cast(int)(values.add(value));

        if (_id != null)
            *_id = id;

        return &values[id];
    }

    // Convert primary to value
    private int convert_primary(ExpressionPrimary *primary)
    {
        int id;

        final switch (primary.type)
        {
            case primary_number:
            {
                if ((cast(int)(primary.as_number) - primary.as_number) != 0)
                {
                    if (primary.as_number < float.max)
                    {
                        IRValue *value = make_value(ir_type_f32, &id);
                        value.as_f32   = cast(float)(primary.as_number);
                    }
                    else
                    {
                        IRValue *value = make_value(ir_type_f64, &id);
                        value.as_f64   = primary.as_number;
                    }
                }
                else
                {
                    if (primary.as_number < byte.max)
                    {
                        IRValue *value = make_value(ir_type_i8, &id);
                        value.as_i8    = cast(byte)(primary.as_number);
                    }
                    else if (primary.as_number < short.max)
                    {
                        IRValue *value = make_value(ir_type_i16, &id);
                        value.as_i16   = cast(short)(primary.as_number);
                    }
                    else if (primary.as_number < int.max)
                    {
                        IRValue *value = make_value(ir_type_i32, &id);
                        value.as_i32   = cast(int)(primary.as_number);
                    }
                    else
                    {
                        IRValue *value = make_value(ir_type_i64, &id);
                        value.as_i64   = cast(long)(primary.as_number);
                    }
                }
                break;
            }
        }

        return id;
    }

    // Convert expression to value
    private int convert_expression(NodeExpression *expression)
    {
        int id;

        final switch (expression.type)
        {
            case expression_primary:
                return convert_primary(&expression.as_primary);
        }

        return id;
    }

    // Convert return statement
    private void convert_return_statement(NodeReturnStatement *node)
    {
        IRReturn *instruction = cast(IRReturn *)(make_instruction(ir_return));

        if (node.expression == -1)
            make_value(ir_type_void, &instruction.value);
        else
            instruction.value = convert_expression(cast(NodeExpression *)(typer.parser.ast[node.expression]));
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

        final switch (g_backend)
        {
            case backend_x64:
            {
                x64_backend.convert(&this);
                break;
            }
        }

        return true;
    }
}