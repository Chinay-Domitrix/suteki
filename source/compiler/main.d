module compiler.main;

import std.stdio;
import compiler.scanner;

void main(string[] args)
{
    Scanner scanner;
    scanner.set("deefine main() {}");
    scanner.next();

    printf("%.*s %d\n", scanner.current.length, scanner.current.start, scanner.current.type);
}