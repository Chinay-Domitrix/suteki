module compiler.ast;

import compiler.token;

enum
{
    node_none,

    node_type,
    node_function_declaration,
    node_function_parameter,
    node_block,
    node_return_statement,

    node_expression,
}

enum
{
    expression_binary,
    expression_primary,
    expression_unary,
}

enum
{
    primary_number,
    primary_string,
}

struct ExpressionBinary
{
    int  left;
    int  right;
    uint operator;
}

struct ExpressionPrimary
{
    union
    {
        double as_number;
        string as_string;
    }

    uint type;
}

struct ExpressionUnary 
{
    int  operand;
    uint operator;
}

struct NodeExpression
{
    union
    {
        ExpressionBinary  as_binary;
        ExpressionPrimary as_primary;
        ExpressionUnary   as_unary;
    }

    uint type;

    ExpressionBinary *opCast(T : ExpressionBinary *)()
    {
        return &as_binary;
    }

    ExpressionPrimary *opCast(T : ExpressionPrimary *)()
    {
        return &as_primary;
    }

    ExpressionUnary *opCast(T : ExpressionUnary *)()
    {
        return &as_unary;
    }
}

struct NodeType
{
    Token name;
}

struct NodeFunctionDeclaration
{
    Token   name;
    int  [] parameters;
    int     block;
    int     type;
}

struct NodeFunctionParameter
{
    Token name;
    int   type;
    int   expression;
}

struct NodeBlock
{
    uint end;
}

struct NodeReturnStatement
{
    Token start;
    int   expression;
}

struct Node
{
    union
    {
        NodeType                as_type;
        NodeFunctionDeclaration as_function_declaration;
        NodeFunctionParameter   as_function_parameter;
        NodeBlock               as_block;
        NodeReturnStatement     as_return_statement;

        NodeExpression as_expression;
    }

    uint type;

    NodeType *opCast(T : NodeType *)()
    {
        return &as_type;
    }

    NodeFunctionDeclaration *opCast(T : NodeFunctionDeclaration *)()
    {
        return &as_function_declaration;
    }

    NodeFunctionParameter *opCast(T : NodeFunctionParameter *)()
    {
        return &as_function_parameter;
    }

    NodeBlock *opCast(T : NodeBlock *)()
    {
        return &as_block;
    }

    NodeReturnStatement *opCast(T : NodeReturnStatement *)()
    {
        return &as_return_statement;
    }

    NodeExpression *opCast(T : NodeExpression *)()
    {
        return &as_expression;
    }
}