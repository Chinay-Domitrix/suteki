module compiler.main;

import std.stdio;
import std.file;

import compiler.typer;
import compiler.config;

void main(string[] args)
{
    // Read all input files
    for (uint i = 1; i < args.length; ++i)
        g_inputs ~= readText(args[i]);

    Typer typer;
    typer.start();
}