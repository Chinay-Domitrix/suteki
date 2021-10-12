module compiler.token;

enum
{
    token_end,
    token_error,

    token_number,
    token_string,
    token_identifier,

    token_left_parenthesis,
    token_right_parenthesis,
    token_left_brace,
    token_right_brace,
    token_comma,
    token_semicolon,
    token_dot,
    token_colon,

    token_char,
    token_byte,
    token_short,
    token_int,
    token_long,

    token_ubyte,
    token_ushort,
    token_uint,
    token_ulong,

    token_single,
    token_double,

    token_bool,
    token_void,
    token_string_type,

    token_true,
    token_false,
    token_null,

    token_declare,
    token_define,
    token_return,
    token_module,
    token_import,
}

struct Token 
{
    uint         type;
    const(char) *start;
    uint         length;
    uint         line;
    uint         column;
}