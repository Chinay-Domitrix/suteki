module compiler.typer;

import compiler.parser;
import compiler.ast;
import compiler.token;
import utilities.color;

import std.stdio;

struct Typer
{
    Parser parser;
    bool   had_error;

    // Error 
    private void error(string message)
    {
        had_error = true;
        printf("%sError:%s %s\n%s", c_red, c_white, message.ptr, c_reset);
    }

    // Error at token
    private void error(const(Token) token, string message)
    {
        had_error = true;
        printf("%s[%d:%d] Error", c_red, token.line, token.column);

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

    // Start the typer
    bool start()
    {
        had_error = false;
        parser.start();

        return !had_error;
    }
}