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

struct NodePtr
{
    int index;

    // Get node
    Node *get(ref Node[] ast)
    {
        return &ast[index];
    }
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

struct NodeType
{
    Token name;
}

struct NodeFunctionDeclaration
{
    Token     name;
    NodePtr[] parameters;
    NodePtr   block;
    NodePtr   type;
}

struct NodeFunctionParameter
{
    Token   name;
    NodePtr type;
    NodePtr expression;
}

struct NodeBlock
{
    uint end;
}

struct NodeReturnStatement
{
    Token   start;
    NodePtr expression;
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
}