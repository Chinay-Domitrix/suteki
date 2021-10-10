module compiler.ast;

import compiler.token;

enum
{
    node_function_declaration,
    node_function_parameter,
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

struct NodePtr
{
    uint index;
}

struct ExpressionBinary
{
    NodePtr left;
    NodePtr right;
    uint    operator;
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
    NodePtr operand;
    uint    operator;
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
}

struct NodeFunctionDeclaration
{
    Token     name;
    NodePtr[] parameters;
    NodePtr   block;
}

struct NodeFunctionParameter
{
    Token   name;
    NodePtr expression;
}

struct NodeReturnStatement
{
    NodePtr expression;
}

struct Node
{
    union
    {
        NodeFunctionDeclaration as_function_declaration;
        NodeFunctionParameter   as_function_parameter;
        NodeReturnStatement     as_return_statement;

        NodeExpression as_expression;
    }

    uint type;
}