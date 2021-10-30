using System;
using System.Collections.Generic;

namespace Suteki
{
    class Compiler
    {
        public Dictionary<string, Module>     Modules;
        public Dictionary<string, Symbol>     Symbols;
        public int                            MainFile;
        public Scanner                        CurrentScanner;
        public bool                           HadError;
        public Dictionary<string, UserType>   UserTypes;
        public FileInput                      CurrentInput;

        // Initialize the Compiler
        public Compiler()
        {
            Modules   = new Dictionary<string, Module>();
            Symbols   = new Dictionary<string, Symbol>();
            UserTypes = new Dictionary<string, UserType>();
            MainFile  = -1;
            HadError  = false;
        }

        // Get previous Token
        public Token PreviousToken
        {
            get 
            { 
                return CurrentScanner.PreviousToken; 
            }
        }

        // Get current Token
        public Token CurrentToken
        {
            get 
            { 
                return CurrentScanner.CurrentToken; 
            }
        }

        // Show error
        public void Error(string message)
        {
            HadError = true;

            Utilities.WriteColor(ConsoleColor.Red,   "Error: ");
            Utilities.WriteColor(ConsoleColor.White, message);
            Console.WriteLine();
        }

        // Show error at Token
        public void Error(Token token, string message)
        {
            HadError = true;
            Utilities.WriteColor(ConsoleColor.Red, $"[{CurrentInput.Path}:{token.Line}:{token.Column}] Error");

            if (token.Type == TokenType.End)
                Utilities.WriteColor(ConsoleColor.Red, " at end: ");
            else if (token.Type == TokenType.Error)
                Utilities.WriteColor(ConsoleColor.Red, ": ");
            else
                Utilities.WriteColor(ConsoleColor.Red, " at ", ConsoleColor.White, token.Data, ConsoleColor.Red, ": ");

            Utilities.WriteColor(ConsoleColor.White, message);
            Console.WriteLine();
        }

        // Advance the Token
        public void Advance()
        {
            for (;;)
            {
                CurrentScanner.Scan();

                if (CurrentToken.Type != TokenType.Error)
                    break;

                Error(CurrentToken, (string)CurrentToken.Data);
            }
        }

        // Match Token?
        public bool Match(TokenType type)
        {
            if (CurrentToken.Type == type)
            {
                Advance();
                return true;
            }

            return false;
        }

        // Consume Token
        public bool Consume(TokenType type, string message)
        {
            if (Match(type))
                return true;

            Error(CurrentToken, message);
            return false;
        }

        // Is token a type?
        public bool IsType(Token token)
        {
            if (token.Type == TokenType.Int)
                return true;
            else
                return false;
        }

        // Learn everything
        public bool Learn()
        {
            Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();
            Dictionary<string, Symbol> symbolTable;

            // Loop thorugh every file and learn every global 
            // symbol from it
            for (int index = 0; index < Program.Inputs.Count; ++index)
            {
                symbols.Clear();

                CurrentInput   = Program.Inputs[index];
                CurrentScanner = new Scanner(CurrentInput.Source);

                // Start learning everything possible
                Advance();

                while (!Match(TokenType.End))
                {
                    Advance();

                    switch (PreviousToken.Type)
                    {
                        // Export module?
                        case TokenType.Export:
                        {
                            // Parse the module name
                            Token  startName  = CurrentToken;
                            string moduleName = "";

                            for (;;)
                            {
                                if (Match(TokenType.Identifier))
                                    moduleName += (string)PreviousToken.Data;
                                else
                                {
                                    startName.Data = moduleName;
                                    Error(startName, "Invalid module name.");
                                }

                                if (!Match(TokenType.Dot))
                                    break;

                                moduleName += '.';
                            }

                            Consume(TokenType.Semicolon, "Expected ';' after module name.");

                            // Make the module
                            CurrentInput.ModuleName = moduleName;

                            if (!Modules.ContainsKey(moduleName))
                                Modules[moduleName] = new Module();
                            else
                            {
                                startName.Data = moduleName;
                                Error(startName, "This module was already declared.");
                            }

                            break;
                        }

                        // Declaration?
                        default:
                        {
                            if (!IsType(PreviousToken))
                                break;

                            Token  nameToken;
                            string nameString;

                            // Parse declaration name
                            Consume(TokenType.Identifier, "Expected identifier after type.");

                            nameToken  = PreviousToken;
                            nameString = (string)PreviousToken.Data;

                            // Make symbol
                            Symbol symbol = new Symbol();
                            symbol.Name   = nameString;

                            if (Match(TokenType.LeftParenthesis))
                                symbol.Type = SymbolType.Function;
                            else
                                symbol.Type = SymbolType.Variable;

                            // Make sure that symbol was not declared before
                            if (CurrentInput.ModuleName != "")
                                symbolTable = Modules[CurrentInput.ModuleName].Symbols;
                            else
                                symbolTable = Symbols;

                            if (symbolTable.ContainsKey(nameString))
                                Error(nameToken, "This symbol was already declared.");
                            
                            // Is entry point function declaration?
                            if (nameString == "main" && symbol.Type == SymbolType.Function)
                            {
                                if (MainFile != -1)
                                    Error(nameToken, "The main function was already declared.");
        
                                MainFile = index;
                            }

                            // Add symbol
                            symbols[nameString] = symbol;

                            // Skip other tokens that are not going to be used
                            if (symbol.Type == SymbolType.Variable)
                            {
                                if (!Match(TokenType.Semicolon))
                                {
                                    while (!Match(TokenType.Semicolon) && !Match(TokenType.End))
                                        Advance();

                                    if (PreviousToken.Type == TokenType.End)
                                        Error(PreviousToken, "Expected ';' after variable declaration.");
                                }
                            }
                            else
                            {
                                if (!Match(TokenType.RightParenthesis))
                                {
                                    while (!Match(TokenType.RightParenthesis) && !Match(TokenType.End))
                                        Advance();
                                    
                                    if (PreviousToken.Type == TokenType.End)
                                        Error(PreviousToken, "Expected ')' after function parameters.");
                                }

                                Consume(TokenType.LeftBrace, "Expected '{' after ')'.");

                                if (!Match(TokenType.RightBrace))
                                {
                                    while (!Match(TokenType.RightBrace) && !Match(TokenType.End))
                                        Advance();

                                    if (PreviousToken.Type == TokenType.End)
                                        Error(PreviousToken, "Expected '}' after function statement(s).");
                                }
                            }
                            break;
                        }
                    }
                }

                // Add all symbols to module symbols or global symbols
                if (CurrentInput.ModuleName != "")
                    symbolTable = Modules[CurrentInput.ModuleName].Symbols;
                else
                    symbolTable = Symbols;

                foreach (KeyValuePair<string, Symbol> entry in symbols)
                    symbolTable[entry.Key] = entry.Value;
            }

            // Make sure we found entry point file
            if (MainFile == -1)
            {
                Error("Could not find entry point file.");
                return false;
            }

            return !HadError;
        }

        // Generate function declaration
        public void GenerateFunctionDeclaration(Token nameToken)
        {

        }

        // Generate variable declaration
        public void GenerateVariableDeclaration(Token nameToken, bool isGlobal)
        {

        }

        // Start compiling 
        public bool Compile()
        {
            if (!Learn())
                return false;

            // Loop thorugh every file and generate C files
            for (int index = 0; index < Program.Inputs.Count; ++index)
            {
                CurrentInput   = Program.Inputs[index];
                CurrentScanner = new Scanner(CurrentInput.Source);

                // Start learning everything possible
                Advance();

                while (!Match(TokenType.End))
                {
                    Advance();

                    switch (PreviousToken.Type)
                    {
                        // Export module?
                        case TokenType.Export:
                        {
                            // Skip module name
                            for (;;)
                            {
                                Match(TokenType.Identifier);

                                if (!Match(TokenType.Dot))
                                    break;
                            }

                            Advance();
                            break;
                        }

                        // Declaration?
                        default:
                        {
                            if (!IsType(PreviousToken))
                            {
                                Error(PreviousToken, "Unexpected token.");
                                break;
                            }

                            Token nameToken;

                            // Parse declaration name
                            Consume(TokenType.Identifier, "Expected identifier after type.");
                            nameToken  = PreviousToken;

                            // Generate declaration
                            if (Match(TokenType.LeftParenthesis))
                                GenerateFunctionDeclaration(nameToken);
                            else
                                GenerateVariableDeclaration(nameToken, true);
                            break;
                        }
                    }
                }
            }

            return true;
        }
    }
}