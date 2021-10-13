module compiler.parser;

import compiler.config;
import compiler.scanner;
import compiler.token;
import compiler.ast;
import utilities.color;
import utilities.string;
import utilities.list;

import std.stdio;
import std.conv;

enum
{
    symbol_flag_none,
}

struct Module
{
    Symbol[string] symbols;
    string         source;
}

struct Symbol 
{
    int flags;
}

struct Parser
{
    private Scanner scanner;
            bool    had_error;

    private string module_name;

    Module[string] modules;
    Symbol[string] symbols;

    string main_file;
    string current_source;

    List!(Node) ast;

    // Error 
    private void error(string message)
    {
        had_error = true;
        printf("%sParser Error:%s %s\n%s", c_red, c_white, message.ptr, c_reset);
    }

    // Error at token
    private void error(const(Token) token, string message)
    {
        had_error = true;
        printf("%s[%d:%d] Parser Error", c_red, token.line, token.column);

        if (token.type == token_error)
        {
            printf(":%s ", c_white);
            printf("%s\n", token.start);
        }
        else if (token.type == token_end)
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

    // Advance current token
    private void advance()
    {
        for (;;)
        {
            if (scanner.next() != token_error)
                break;

            error(scanner.current, "");
        }
    }

    // Match current type?
    private bool match(uint type)
    {
        if (scanner.current.type == type)
        {
            advance();
            return true;
        }

        return false;
    }
    
    // Consume token
    private void consume(uint type, string message)
    {
        if (match(type))
            return;

        error(scanner.current, message);
    }

    // Make AST node
    private Node *make_node(uint type, int *_id = null)
    {
        int  id;
        Node node;

        node.type = type;
        id        = cast(int)(ast.add(node));

        if (_id != null)
            *_id = id;

        return &ast[id];
    }

    // Make AST expression node
    private Node *make_expression_node(uint type, int *_id = null)
    {
        int  id;
        Node node;

        node.type               = node_expression;
        node.as_expression.type = type;
        id                      = cast(int)(ast.add(node));

        if (_id != null)
            *_id = id;

        return &ast[id];
    }

    // Learn module
    private void learn_module()
    {
        module_name = "";

        for (;;)
        {
            consume(token_identifier, "Invalid module name.");

            for (uint i = 0; i < scanner.previous.length; ++i)
                module_name ~= scanner.previous.start[i];

            if (!match(token_dot))
                break;

            module_name ~= '.';
        }

        consume(token_semicolon, "Expected ';' after module name.");

        // Add module
        if (module_name != "")
        {
            Module mod;
            mod.source = current_source;

            modules[module_name] = mod;
        }
    }

    // Learn pass
    private void learn()
    {
        bool have_module = false;

        main_file = "";

        foreach (source; g_inputs)
        {
            current_source = source;

            // First pass - Find module name and maybe entry point function
            scanner.set(source);
            scanner.next();

            while (!match(token_end))
            {
                scanner.next();

                switch (scanner.previous.type)
                {
                    case token_module:
                    {
                        learn_module();
                        have_module = true;
                        break;
                    }

                    default:
                        break;
                }
            }

            // No module?
            if (!have_module)
                break;

            // Second pass - Learn every global
            scanner.set(source);
            scanner.next();

            while (!match(token_end))
            {
                scanner.next();

                switch (scanner.previous.type)
                {
                    // just test
                    case token_define:
                    {
                        consume(token_identifier, "Expected function name after 'define'.");
                        
                        string name = "";

                        for (uint i = 0; i < scanner.previous.length; ++i)
                            name ~= scanner.previous.start[i];

                        if (name == "main")
                            main_file = source;

                        modules[module_name].symbols[name] = Symbol();
                        break;
                    }

                    default:
                        break;
                }
            }
        }

        // Check for entry point function
        if (main_file == "")
        {
            error("Could not find entry point function 'main'.");
            return;
        }

        module_name = "";
    }

    // Parse import
    private void parse_import()
    {
        string import_name = "";

        for (;;)
        {
            consume(token_identifier, "Invalid module name.");

            for (uint i = 0; i < scanner.previous.length; ++i)
                import_name ~= scanner.previous.start[i];

            if (!match(token_dot))
                break;

            import_name ~= '.';
        }

        consume(token_semicolon, "Expected ';' after module name.");
        
        if (!(import_name in modules))
        {
            scanner.previous.start  = import_name.ptr;
            scanner.previous.length = cast(uint)(import_name.length);

            error(scanner.previous, "Could not find module.");
            return;
        }

        // Import
        if (import_name != "")
        {
            Module *mod = &modules[import_name];

            // Add file symbols to global symbol table
            foreach(key, value; mod.symbols)
                symbols[key] = value;

            // Include file code
            Scanner old_scanner = scanner;
            scanner.set(mod.source);
            scanner.next();

            while (!match(token_end))
                parse_declaration();

            scanner = old_scanner;
        }
    }

    // Parse type
    private int parse_type()
    {
        int id = -1;
        advance();

        if (scanner.previous.type >= token_char && scanner.previous.type <= token_string_type)
        {
            NodeType  *node = cast(NodeType *)(make_node(node_type, &id));
            node.name       = scanner.previous;
        }
        else
            error(scanner.previous, "Expected type.");

        return id;
    }

    // Parse expression
    private int parse_expression()
    {
        int id;

        if (match(token_number))
        {
            string number;

            for (uint i = 0; i < scanner.previous.length; ++i)
                number ~= scanner.previous.start[i];

            ExpressionPrimary *expression = cast(ExpressionPrimary *)(make_expression_node(expression_primary, &id));
            expression.type               = primary_number;
            expression.as_number          = to!(double)(number);
        }
        else if (match(token_string))
        {
            string str;

            for (uint i = 0; i < scanner.previous.length; ++i)
                str ~= scanner.previous.start[i];

            ExpressionPrimary *expression = cast(ExpressionPrimary *)(make_expression_node(expression_primary, &id));
            expression.type               = primary_string;
            expression.as_string          = str;
        }

        return id;
    }

    // Parse return statement
    private void parse_return_statement()
    {
        NodeReturnStatement *node = cast(NodeReturnStatement *)(make_node(node_return_statement));
        node.start                = scanner.previous;

        if (match(token_semicolon))
            node.expression = -1;
        else
        {
            node.expression = parse_expression();
            consume(token_semicolon, "Expected ';' after return statement.");
        }
    }

    // Parse statement
    private void parse_statement()
    {
        scanner.next();

        switch (scanner.previous.type)
        {
            case token_return:
            {
                parse_return_statement();
                break;
            }

            default:
            {
                error(scanner.previous, "Expected statement.");
                break;
            }
        }
    }

    // Parse block of statements
    private int parse_block()
    {
        int        id;
        NodeBlock *node = cast(NodeBlock *)(make_node(node_block, &id));

        while (!match(token_right_brace) && !match(token_end))
            parse_statement();

        if (scanner.previous.type == token_end)
            error(scanner.previous, "Expected '}' after function statement(s).");
            
        node.end = cast(uint)(ast.size);
        return id;
    }

    // Parse function definition
    private void parse_function_definition()
    {
        NodeFunctionDeclaration *node = cast(NodeFunctionDeclaration *)(make_node(node_function_declaration)); 

        consume(token_identifier, "Expected function name after 'define'.");
        node.name = scanner.previous;

        consume(token_left_parenthesis,  "Expected '(' after function name.");
        consume(token_right_parenthesis, "Expected ')' after function parameters.");

        if (match(token_colon))
            node.type = parse_type();
        else
            node.type = -1;

        consume(token_left_brace, "Expected '{'.");
        node.block = parse_block();
    }

    // Parse declaration
    private void parse_declaration()
    {
        scanner.next();

        switch (scanner.previous.type)
        {
            case token_import:
            {
                parse_import();
                break;
            }

            case token_module:
            {
                // Skip everything
                for (;;)
                {
                    match(token_identifier);

                    if (!match(token_dot))
                        break;
                }

                match(token_semicolon);
                break;
            }

            case token_define:
            {
                parse_function_definition();
                break;
            }

            default:
            {
                error(scanner.previous, "Expected declaration.");
                break;
            }
        }
    }

    // Parse pass
    private void parse()
    {
        // Third pass - Parse everything into an AST
        scanner.set(main_file);
        scanner.next();

        while (!match(token_end))
            parse_declaration();
    }

    // Start the parser
    bool start()
    {
        ast.initialize();
        had_error = false;

        learn();
        
        if (!had_error)
            parse();

        return !had_error;
    }
}