module compiler.scanner;

import compiler.token;
import utilities.string;

private enum uint[string] keywords =
[
    "char":  token_char,
    "byte":  token_byte,
    "short": token_short,
    "int":   token_int,
    "long":  token_long,

    "ubyte":  token_ubyte,
    "ushort": token_ushort,
    "uint":   token_uint,
    "ulong":  token_ulong,

    "single": token_single,
    "double": token_double,

    "bool":   token_bool,
    "void":   token_void,
    "string": token_string_type,

    "true":  token_true,
    "false": token_false,
    "null":  token_null,

    "declare": token_declare,
    "define":  token_define,
    "return":  token_return,
    "export":  token_export,
    "import":  token_import,
];

struct Scanner
{
    Token previous;
    Token current;

    private string source;

    private const(char) *start;
    private const(char) *end;

    private uint line;
    private uint column;

    // Set the scanner source
    void set(string content)
    {
        source = content;
        start  = source.ptr;
        end    = source.ptr;
        line   = 1;
        column = 1;
    }

    // Advance current character
    private char advance()
    {
        ++end;
        ++column;
        
        return end[-1];
    }

    // Is character a number?
    private bool is_number(char character)
    {
        return (character >= '0' && character <= '9');
    }

    // Is character an identifier?
    private bool is_identifier(char character)
    {
        return (character >= 'A' && character <= 'Z') ||
               (character >= 'a' && character <= 'z') ||
               (character == '_');
    }

    // Skip whitespace
    private void skip_whitespace()
    {
        for (;;)
        {
            switch (*end)
            {
                // Line
                case '\n':
                {
                    ++line;
                    column = 0;
                }

                // Spacing
                case ' ':
                case '\r':
                case '\t':
                {
                    advance();
                    break;
                }

                // Not a whitespace
                default:
                    return;
            }
        }
    }

    // Make token
    private uint make_token(uint type)
    {
        current.type   = type;
        current.start  = start;
        current.length = cast(uint)(end - start);
        current.line   = line;
        current.column = column;

        return type;
    }

    // Make error token
    private uint make_token(string message)
    {
        current.type   = token_error;
        current.start  = message.ptr;
        current.length = cast(uint)(message.length);
        current.line   = line;
        current.column = column;

        return token_error;
    }

    // Make number token
    private uint make_number_token()
    {
        while (is_number(*end))
            advance();

        if (*end == '.')
        {
            advance();

            while (is_number(*end))
                advance();
        }

        return make_token(token_number);
    }

    // Make identifier token
    private uint make_identifier_token()
    {
        while (is_identifier(*end) || is_number(*end))
            advance();

        foreach (key, value; keywords)
        {
            if (string_equals(key.ptr, start, cast(uint)(end - start)))
                return make_token(value);
        }

        return make_token(token_identifier);
    }

    // Make string token
    private uint make_string_token()
    {
        while (*end != '"' && *end != '\0')
            advance();

        if (*end == '\0')
            return make_token("Unterminated string.");

        advance();
        return make_token(token_string);
    }

    // Get next token
    uint next()
    {
        skip_whitespace();

        start    = end;
        previous = current;

        if (*end == '\0')
            return make_token(token_end);

        char character = advance();

        if (is_number(character))
            return make_number_token();
        
        if (is_identifier(character))
            return make_identifier_token();

        switch (character)
        {
            case '(':
                return make_token(token_left_parenthesis);

            case ')':
                return make_token(token_right_parenthesis);
                
            case '{':
                return make_token(token_left_brace);
                
            case '}':
                return make_token(token_right_brace);
                
            case ',':
                return make_token(token_comma);
                
            case ';':
                return make_token(token_semicolon);
                
            case '.':
                return make_token(token_dot);
                
            case ':':
                return make_token(token_colon);

            case '"':
                return make_string_token();

            default:
                return make_token("Unexpected character.");
        }
    }
}