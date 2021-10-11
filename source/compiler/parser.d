module compiler.parser;

import compiler.config;
import compiler.scanner;
import compiler.token;
import compiler.ast;
import utilities.color;
import utilities.string;

import std.stdio;

enum
{
    symbol_flag_none,
}

struct Export
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

    private string export_name;

    Export[string] exports;
    Symbol[string] symbols;

    string main_file;

    Node[] ast;

    // TODO: temporary
    string   current_source;

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
    private NodePtr make_node(uint type)
    {
        Node node;
        node.type = type;

        ast ~= node;

        return NodePtr(cast(uint)(ast.length) - 1);
    }

    // Learn export
    private void learn_export()
    {
        export_name = "";

        for (;;)
        {
            consume(token_identifier, "Invalid export name.");

            for (uint i = 0; i < scanner.previous.length; ++i)
                export_name ~= scanner.previous.start[i];

            if (!match(token_dot))
                break;

            export_name ~= '.';
        }

        consume(token_semicolon, "Expected ';' after export name.");

        // Add export
        if (export_name != "")
        {
            Export ex;
            ex.source = current_source;

            exports[export_name] = ex;
        }
    }

    // Learn pass
    private void learn()
    {
        bool have_export = false;

        main_file = "";

        foreach (source; g_inputs)
        {
            current_source = source;

            // First pass - Find export and maybe entry point function
            scanner.set(source);
            scanner.next();

            while (!match(token_end))
            {
                scanner.next();

                switch (scanner.previous.type)
                {
                    case token_export:
                    {
                        learn_export();
                        have_export = true;
                        break;
                    }

                    default:
                        break;
                }
            }

            // File isn't exported?
            if (!have_export)
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

                        exports[export_name].symbols[name] = Symbol();
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

        export_name = "";
    }

    // Parse import
    private void parse_import()
    {
        string import_name = "";

        for (;;)
        {
            consume(token_identifier, "Invalid import name.");

            for (uint i = 0; i < scanner.previous.length; ++i)
                import_name ~= scanner.previous.start[i];

            if (!match(token_dot))
                break;

            import_name ~= '.';
        }

        consume(token_semicolon, "Expected ';' after import name.");
        
        if (!(import_name in exports))
        {
            scanner.previous.start  = import_name.ptr;
            scanner.previous.length = cast(uint)(import_name.length);

            error(scanner.previous, "Could not find import name.");
            return;
        }

        // Import
        if (import_name != "")
        {
            Export *ex = &exports[import_name];

            // Add file symbols to global symbol table
            foreach(key, value; ex.symbols)
                symbols[key] = value;

            // Include file code
            Scanner old_scanner = scanner;
            scanner.set(ex.source);
            scanner.next();

            while (!match(token_end))
                parse_declaration();

            scanner = old_scanner;
        }
    }

    // Parse type
    private NodePtr parse_type()
    {
        advance();

        if (scanner.previous.type >= token_char && scanner.previous.type <= token_string_type)
        {
            NodePtr    ptr  = make_node(node_type);
            NodeType  *node = &ptr.get(ast).as_type;
            node.name       = scanner.previous;

            return ptr;
        }
        
        error(scanner.previous, "Invalid type.");
        return NodePtr(-1);
    }

    // Parse expression
    private NodePtr parse_expression()
    {
        NodePtr ptr;

        if (match(token_number))
        {
            ptr = make_node(node_expression);

            ExpressionPrimary *expression = &ptr.get(ast).as_expression.as_primary;
            expression.type               = primary_number;
            expression.as_number          = 0; // TODO: parse number
        }
        else if (match(token_string))
        {
            ptr = make_node(node_expression);

            ExpressionPrimary *expression = &ptr.get(ast).as_expression.as_primary;
            expression.type               = primary_string;
            expression.as_string          = "string"; // TODO: parse string
        }

        return ptr;
    }

    // Parse return statement
    private void parse_return_statement()
    {
        NodeReturnStatement *node = &make_node(node_return_statement).get(ast).as_return_statement;
        node.start                = scanner.previous;

        if (match(token_semicolon))
            node.expression = NodePtr(-1);
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
    private NodePtr parse_block()
    {
        NodePtr    ptr  = make_node(node_block);
        NodeBlock *node = &ptr.get(ast).as_block;

        while (!match(token_right_brace) && !match(token_end))
            parse_statement();

        if (scanner.previous.type == token_end)
            error(scanner.previous, "Expected '}' after function statement(s).");
            
        node.end = cast(uint)(ast.length);
        return ptr;
    }

    // Parse function definition
    private void parse_function_definition()
    {
        NodeFunctionDeclaration *node = &make_node(node_function_declaration).get(ast).as_function_declaration; 

        consume(token_identifier, "Expected function name after 'define'.");
        node.name = scanner.previous;

        consume(token_left_parenthesis,  "Expected '(' after function name.");
        consume(token_right_parenthesis, "Expected ')' after function parameters.");

        if (match(token_colon))
            node.type = parse_type();
        else
            node.type = NodePtr(-1);

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

            case token_export:
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
        had_error = false;

        learn();
        
        if (!had_error)
            parse();

        return !had_error;
    }
}