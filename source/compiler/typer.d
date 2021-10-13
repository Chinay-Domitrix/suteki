module compiler.typer;

import compiler.parser;
import compiler.ast;
import compiler.token;
import utilities.color;

import std.stdio;

private enum
{
    type_void,
    type_string,
    type_bool,
    type_ubyte,
    type_ushort,
    type_uint,
    type_ulong,
    type_byte,
    type_short,
    type_int,
    type_long,
    type_single,
    type_double,

    type_integer,
    type_floating,
}

struct Typer
{
    Parser  parser;
    bool    had_error;

    int function_id;

    // Error 
    private void error(string message)
    {
        had_error = true;
        printf("%sTyper Error:%s %s\n%s", c_red, c_white, message.ptr, c_reset);
    }

    // Error at token
    private void error(const(Token) token, string message)
    {
        had_error = true;
        printf("%s[%d:%d] Typer Error", c_red, token.line, token.column);

        if (token.type == token_end)
        {
            printf(" at end:%s ", c_white);
            printf("%s\n", message.ptr);
        }
        else
        {
            printf(" at %s%.*s%s:%s ", c_white, token.length, token.start, c_red, c_white);
            printf("%s\n", message.ptr);
        }

        printf("%s", c_reset);
    }
    
    // Get type
    int get_node_type(NodeType *type)
    {
        switch (type.name.type)
        {
            case token_void:
                return type_void;

            case token_bool:
                return type_bool;
                
            case token_string_type:
                return type_string;
                
            case token_ubyte:
                return type_ubyte;
                
            case token_ushort:
                return type_ushort;
                
            case token_uint:
                return type_uint;
                
            case token_ulong:
                return type_ulong;
                
            case token_char:
            case token_byte:
                return type_byte;
                
            case token_short:
                return type_short;
                
            case token_int:
                return type_int;
                
            case token_long:
                return type_long;
                
            case token_single:
                return type_single;
                
            case token_double:
                return type_double;

            default:
                return -1;
        }
    }

    // Get type as number type
    int get_type_number(int type)
    {
        final switch (type)
        {
            case type_ubyte:
            case type_ushort:
            case type_uint:
            case type_ulong:
            case type_byte:
            case type_short:
            case type_int:
            case type_long:
                return type_integer;
                
            case type_single:
            case type_double:
                return type_floating;
        }
    }

    // Get number type
    int get_number_type(double number)
    {
        if ((cast(int)(number) - number) != 0)
        {
            if (number < float.sizeof)
                return type_single;
            else
                return type_double;
        }

        if (number < byte.sizeof)
            return type_byte;
        else if (number < short.sizeof)
            return type_short;
        else if (number < int.sizeof)
            return type_int;
        
        return type_long;
    }

    // Compare types
    bool compare(int a, int b)
    {
        // NOTE: 'a' is always going to be like function type
        // and 'b' the return type or something.

        if (a == b)
            return true;
        else if ((a >= type_bool && a <= type_long) && (b >= type_bool && b <= type_long))
            return true;
        else if ((a >= type_single && a <= type_double) && (b >= type_single && b <= type_double))
            return true;
        else if ((a >= type_single && a <= type_double) && (b >= type_bool && b <= type_long))
            return true;
        else if (a == type_floating && b == type_integer)
            return true;

        return false;
    }

    // Get primary type
    int get_primary_type(ExpressionPrimary *primary)
    {
        final switch(primary.type)
        {
            case primary_string:
                return type_string;

            case primary_number:
                return get_number_type(primary.as_number);
        }
    }
    
    // Get expression type
    int get_expression_type(NodeExpression *expression)
    {
        final switch (expression.type)
        {
            case expression_primary:
                return get_primary_type(&expression.as_primary);
        }
    }

    // Check return statement
    void check_return_statement(NodeReturnStatement *node)
    { 
        int function_type;
        int return_type;

        NodeFunctionDeclaration *node_function = cast(NodeFunctionDeclaration *)(parser.ast[function_id]);

        if (node_function.type != -1)
            function_type = get_node_type(cast(NodeType *)(parser.ast[node_function.type]));
        else
            function_type = type_void;

        if (node.expression != -1)
            return_type = get_expression_type(cast(NodeExpression *)(parser.ast[node.expression]));
        else
            return_type = type_void;
            
        if (!compare(function_type, return_type))
            error(node.start, "Return type does not match function type.");
    }

    // Start the typer
    bool start()
    {
        had_error = false;
        
        if (!parser.start())
            return false;

        for (ulong i = 0; i < parser.ast.size; ++i)
        {
            Node *node = &parser.ast[i];

            switch (node.type)
            {
                // Get function type
                case node_function_declaration:
                {
                    function_id = cast(int)(i);
                    break;
                }

                // Check return type
                case node_return_statement:
                {
                    check_return_statement(cast(NodeReturnStatement *)(node));
                    break;
                }

                default:
                    break;
            }
        }

        return !had_error;
    }
}