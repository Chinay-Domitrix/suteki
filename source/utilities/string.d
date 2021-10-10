module utilities.string;

// Get c-string length
uint string_length(const(char) *str)
{
    const(char) *base = str;

    while (*str++)
    {

    }

    return cast(uint)(str - base);
}

// Are both c-strings equal?
bool string_equals(const(char) *a, const(char) *b, uint length = 0)
{
    uint a_length = string_length(a);
    uint b_length = string_length(b);

    if ((a_length != b_length) && length == 0)
        return false;

    if (length != 0)
        a_length = length;

    for (uint i = 0; i < a_length; ++i)
    {
        if (a[i] != b[i])
            return false;
    }

    return true;
}